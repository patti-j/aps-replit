using System.Diagnostics;
using System.Xml.XPath;

namespace PipelinesPublisher;

internal static class FileUtil
{
    public static List<string> GetFilesPack(string a_xmlFilePath, bool a_isPackage = false)
    {
        if (!File.Exists(a_xmlFilePath))
        {
            throw new Exception("BuildConfig file not found: " + a_xmlFilePath);
        }

        List<string> assembliesToPack = new ();

        XPathDocument xd = new (a_xmlFilePath);
        XPathNavigator nav = xd.CreateNavigator();
        XPathNodeIterator ni;

        ni = nav.SelectDescendants(a_isPackage ? "IncludeAssemblies" : "PackAssemblies", "", false);
        if (ni.MoveNext())
        {
            string assemblyList = ni.Current.Value;
            string[] assemblies = assemblyList.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string assembly in assemblies)
            {
                string trimmedName = assembly.Trim();
                if (!string.IsNullOrEmpty(trimmedName))
                {
                    assembliesToPack.Add(assembly.Trim());
                }
            }
        }

        return assembliesToPack;
    }

    // <summary>
    /// Copies files from the source folder to the destination folder.  If folders don't exist they're created.
    /// </summary>
    /// <param name="a_sourceFolder"></param>
    /// <param name="a_destinationFolder"></param>
    public static void CopyAllFiles(string a_sourceFolder, string a_destinationFolder)
    {
        RoboCopyResult result = RoboCopy($"\"{a_sourceFolder}\"", $"\"{a_destinationFolder}\"");
        Console.WriteLine(result.Log);
        if (result.ExitCode >= 8)
        {
            //File copy error
            Console.WriteLine("Error on file copy from {0}  to  {1}", a_sourceFolder, a_destinationFolder);
        }
    }

    private static RoboCopyResult RoboCopy(string a_sourceFolder, string a_destinationFolder)
    {
        //Use Robocopy
        RoboCopyResult result = new ();
        string procArgs = a_sourceFolder + " " + a_destinationFolder;
        procArgs += " /mir /copy:DT /R:5 /W:2 /np /ns /njh /njs";
        Process robocopy = new ();
        robocopy.StartInfo.Arguments = procArgs;
        robocopy.StartInfo.FileName = "robocopy";
        robocopy.StartInfo.UseShellExecute = false;
        robocopy.StartInfo.RedirectStandardOutput = true;
        robocopy.StartInfo.CreateNoWindow = true;
        robocopy.Start();

        result.Log = robocopy.StandardOutput.ReadToEnd();
        robocopy.WaitForExit();
        result.ExitCode = robocopy.ExitCode;
        return result;
    }

    public static void DeleteFiles(string[] a_filePaths)
    {
        foreach (string file in a_filePaths)
        {
            Console.WriteLine("Cleaned: " + file);
            File.Delete(file);
        }
    }

    private class RoboCopyResult
    {
        public int ExitCode;
        public string Log;
    }
}