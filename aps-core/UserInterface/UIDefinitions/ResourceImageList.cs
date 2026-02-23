using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using PT.APSCommon;
using PT.APSCommon.Windows;
using PT.Common.Localization;
using PT.UIDefinitions.Interfaces;

namespace PT.UIDefinitions;

/// <summary>
/// Stores a list of Images used by the UI and Gantt.
/// </summary>
public class ImageLists
{
    public static ImageList ResourceImageList = new ();
    private static IMessageProvider m_messageProvider;

    /// <summary>
    /// Loads all images into all Image Lists.
    /// </summary>
    public static void LoadImages(string a_workingDirectory, IMessageProvider a_messageProvider)
    {
        m_messageProvider = a_messageProvider;
        if (!string.IsNullOrWhiteSpace(a_workingDirectory))
        {
            m_customImagePath = a_workingDirectory;
        }

        //Clear in case reloading
        ResourceImageList.Images.Clear();

        ResourceImageList.ImageSize = new Size(24, 24);
        List<string> imageList = new ();
        if (Directory.Exists(Imagespath()))
        {
            imageList.AddRange(Directory.GetFiles(Imagespath(), "*.*")); //gets files with paths
        }

        if (!string.IsNullOrWhiteSpace(m_customImagePath) && Directory.Exists(ImagePathCustom()))
        {
            imageList.AddRange(Directory.GetFiles(ImagePathCustom(), "*.*")); //gets files with paths
        }

        //Create a sorted list of file names, sorted by name so they can be grouped.
        SortedList sortedFiles = new ();
        for (int i = 0; i < imageList.Count; i++)
        {
            string fileWithPath = imageList[i];
            string fileName = Path.GetFileName(fileWithPath);

            //Ignore hidden files.  my thumbs.db which was hidden was causing an out of memory error
            FileAttributes fa = File.GetAttributes(fileWithPath);
            int hidden = (int)FileAttributes.Hidden & (int)fa;
            if (hidden != (int)FileAttributes.Hidden && fileName != "vssver.scc" && fileName != "MSSCCPRJ.SCC" && !sortedFiles.ContainsKey(fileName)) //Ignore source safe and source anywhere files
            {
                sortedFiles.Add(fileName, fileWithPath);
            }
        }

        //Load the images
        for (int i = 0; i < sortedFiles.Count; i++)
        {
            string fileWithPath = sortedFiles.GetByIndex(i).ToString();
            string fileName = sortedFiles.GetKey(i).ToString();
            try
            {
                Image nextImage = Image.FromFile(fileWithPath);
                ResourceImageList.Images.Add(fileName.ToLower(), nextImage); //store lower so case insensitive
            }
            catch (Exception err)
            {
                //Warn so it can be fixed and keep loading.
                m_messageProvider.ShowMessageBox(new PTMessage(Localizer.GetErrorString("2698", new object[] { fileWithPath, err.Message }), "Failed to load image.") { Classification = PTMessage.EMessageClassification.Information }, true);
            }
        }
    }

    public static string Imagespath()
    {
        return Path.Combine(Application.StartupPath, "ResourceImages");
    }

    private static string m_customImagePath = "";

    private static string ImagePathCustom()
    {
        return Path.Combine(m_customImagePath, "ResourceImages"); //This must match the folder created during installation on the server and brought to the client with the Client Updater.
    }
}