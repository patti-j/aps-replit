using System.IO;
using System.IO.Compression;

namespace PT.ServerManagerSharedLib.Utils
{
    public class ZipUtils
    {
        public static void Extract(byte[] a_archiveBytes, string a_destDir)
        {
            if (!System.IO.Directory.Exists(a_destDir))
            {
                System.IO.Directory.CreateDirectory(a_destDir);
            }

            using (MemoryStream stream = new MemoryStream(a_archiveBytes))
            {
                using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    zip.ExtractToDirectory(a_destDir);
                }
            }
        }

        /// <summary>
        /// Extract a zip file to provided destination directory 
        /// </summary>
        /// <param name="a_sourcePath"></param>
        /// <param name="a_destinationDir"></param>
        public static void Extract(string a_sourcePath, string a_destinationDir)
        {
            ZipFile.ExtractToDirectory(a_sourcePath, a_destinationDir);
        }
    }
}
