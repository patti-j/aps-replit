using System.Drawing;
using System.Resources;
using System.Text;

using DevExpress.Utils;
using DevExpress.Utils.Svg;

using PT.Common.Debugging;

namespace PT.APSCommon.Windows;

public static class PtImageCache
{
    private static readonly ResourceManager s_rm;

    static PtImageCache()
    {
        s_images = new SvgImageCollection();
        s_staticImages = new Dictionary<string, Image>();
        s_exportableKeys = new HashSet<string>();
        s_lock = new object();
        s_rm = new ResourceManager(typeof(Properties.Resources));
        object o = s_rm.GetObject(c_missingResource);
        s_images.Add(c_missingResource, (byte[])o);
        s_staticImages.Add(c_missingResource, new SvgBitmap(GetImage(c_missingResource)).Render(null, 2.0));
    }

    private static readonly SvgImageCollection s_images;
    private static readonly Dictionary<string, Image> s_staticImages;
    private static readonly HashSet<string> s_exportableKeys;
    private const string c_missingResource = "warningFence";
    private static readonly object s_lock;

    private static readonly System.Collections.Concurrent.ConcurrentBag<string> s_missingList = new ();
    #if DEBUG
    //Cache missing icons to help easily add them

    public static bool ReportMissingIcons()
    {
        if (!s_missingList.IsEmpty)
        {
            StringBuilder sb = new ();
            foreach (string s in s_missingList)
            {
                sb.AppendLine(s);
            }

            string logDirectory = @"C:\Temp\";
            string logFileName = "MissingImages.txt";


            if (!System.IO.Directory.Exists(logDirectory))
            {
                System.IO.Directory.CreateDirectory(logDirectory);
            }

            System.IO.File.WriteAllText(logDirectory + logFileName, sb.ToString());
            return true;
        }

        return false;
    }
#endif

    public static void RegisterImage(string a_imageKey, SvgImage a_image)
    {
        RegisterImage(a_imageKey, a_image, false, false);
    }
    
    public static void RegisterImage(string a_imageKey, SvgImage a_image, bool a_exportable)
    {
        RegisterImage(a_imageKey, a_image, a_exportable, false);
    }

    public static void RegisterImageOverride(string a_imageKey, SvgImage a_image)
    {
        RegisterImage(a_imageKey, a_image, false, true);
    }

    public static void RegisterImage(string a_imageKey, SvgImage a_image, bool a_exportable, bool a_override)
    {
        lock (s_lock)
        {
            if (s_images.ContainsKey(a_imageKey))
            {
                if (a_override) //Allow overriding an existing image.
                {
                    s_exportableKeys.Remove(a_imageKey);
                    s_images.RemoveAt(a_imageKey);
                }
                else
                {
                    DebugException.ThrowInTest($"Image already registered: {a_imageKey}");
                    return;
                }
            }

            s_images.Add(a_imageKey, a_image);

            if (a_exportable)
            {
                s_exportableKeys.Add(a_imageKey);
            }
        }
    }

    /// <summary>
    /// Return a registered image registered with the provided key
    /// </summary>
    /// <param name="a_imageKey">Image key</param>
    /// <param name="a_found">Whether an image for that key was found</param>
    /// <returns></returns>
    public static SvgImage GetImage(string a_imageKey, out bool a_found)
    {
        SvgImage svgImage = GetImage(a_imageKey);
        a_found = !s_missingList.Contains(a_imageKey);
        return svgImage;
    }

    public static SvgImage GetImage(string a_imageKey)
    {
        //a_imageKey = a_imageKey.ToLower();
        SvgImage image = s_images[a_imageKey];
        if (image != null)
        {
            return image;
        }

        object o = s_rm.GetObject(a_imageKey);

        #if TEST
            if (image == null)
            {
                throw new Exception($"Missing image resource {a_imageKey}")
            }
        #else
        if (o == null)
        {
            if (!s_missingList.Contains(a_imageKey))
            {
                s_missingList.Add(a_imageKey);
            }

            return s_images[c_missingResource];
        }
        #endif
        s_images.Add(a_imageKey, (byte[])o);
        image = s_images[a_imageKey];

        return image;
    }

    #region Static Images
    public static void RegisterImage(string a_imageKey, Image a_image)
    {
        if (s_staticImages.ContainsKey(a_imageKey))
        {
            s_staticImages.Remove(a_imageKey);
            DebugException.ThrowInTest($"Image already registered: {a_imageKey}");
        }

        s_staticImages.Add(a_imageKey, a_image);
    }

    public static Image GetStaticImage(string a_imageKey)
    {
        if (s_staticImages.TryGetValue(a_imageKey, out Image image))
        {
            return image;
        }

        return s_staticImages[c_missingResource];
    }

    public static IEnumerable<string> GetCachedImageKeys()
    {
        lock (s_lock)
        {
            return s_exportableKeys.ToArray();
        }
    }
    #endregion
}