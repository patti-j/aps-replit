using System.IO.Compression;

namespace PT.Common.Compression;

/// <summary>
/// Utilities for creating and reading Zip archives
/// </summary>
public class Zip
{
    /// <summary>
    /// Extracts byte[] of ZipArchive to a directory
    /// </summary>
    /// <param name="a_archiveBytes"></param>
    /// <param name="a_destDir"></param>
    public static void Extract(byte[] a_archiveBytes, string a_destDir)
    {
        if (!System.IO.Directory.Exists(a_destDir))
        {
            System.IO.Directory.CreateDirectory(a_destDir);
        }

        using (MemoryStream stream = new (a_archiveBytes))
        {
            using (ZipArchive zip = new (stream, ZipArchiveMode.Read))
            {
                zip.ExtractToDirectory(a_destDir);
            }
        }
    }

    /// <summary>
    /// Extracts byte[] of ZipArchive to a directory, one file at a time, overwriting files is specified
    /// </summary>
    /// <param name="a_archiveBytes"></param>
    /// <param name="a_destDir"></param>
    /// <param name="a_overwriteFiles"></param>
    /// <exception cref="InvalidDataException">
    /// The contents of the stream could not be interpreted as a zip archive.-or-<paramref name="mode" /> is
    /// <see cref="F:System.IO.Compression.ZipArchiveMode.Update" /> and an entry is missing from the archive or is corrupt and cannot be read.-or-<paramref name="mode" /> is
    /// <see cref="F:System.IO.Compression.ZipArchiveMode.Update" /> and an entry is too large to fit into memory.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
    /// <exception cref="IOException">
    /// <paramref name="destinationFileName" /> already exists and <paramref name="overwrite" /> is false.-or- An I/O error occurred.-or-The entry is currently open for
    /// writing.-or-The entry has been deleted from the archive.
    /// </exception>
    /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission to create the new file.</exception>
    public static void Extract(byte[] a_archiveBytes, string a_destDir, bool a_overwriteFiles)
    {
        if (!System.IO.Directory.Exists(a_destDir))
        {
            System.IO.Directory.CreateDirectory(a_destDir);
        }

        List<Exception> exceptions = new ();

        using (MemoryStream stream = new (a_archiveBytes))
        {
            using (ZipArchive zip = new (stream, ZipArchiveMode.Read))
            {
                IEnumerable<ZipArchiveEntry> sortedEntries = zip.Entries.OrderBy(entry => entry.FullName.Length);

                foreach (ZipArchiveEntry entry in sortedEntries)
                {
                    string fullPath = Path.Combine(a_destDir, entry.FullName);
                    try
                    {
                        if (fullPath.Replace("/", "\\").EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                        {
                            //This is a directory, create it if it doesn't exist.
                            string dir = Path.GetDirectoryName(fullPath);
                            if (!System.IO.Directory.Exists(dir))
                            {
                                System.IO.Directory.CreateDirectory(dir);
                            }
                        }
                        else
                        {
                            entry.ExtractToFile(fullPath, a_overwriteFiles);
                        }
                    }
                    catch (Exception e)
                    {
                        exceptions.Add(e);
                    }
                }
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }

    /// <summary>
    /// Extracts all files in all sub directories of the zip into the directory specified
    /// </summary>
    /// <param name="a_archiveBytes"></param>
    /// <param name="a_destDir"></param>
    public static void ExtractToRootDirectory(byte[] a_archiveBytes, string a_destDir)
    {
        if (!System.IO.Directory.Exists(a_destDir))
        {
            System.IO.Directory.CreateDirectory(a_destDir);
        }

        using (MemoryStream stream = new (a_archiveBytes))
        {
            using (ZipArchive zip = new (stream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.FullName.EndsWith("/"))
                    {
                        //This is a directory, skip.
                    }
                    else
                    {
                        string newPath = Path.Combine(a_destDir, entry.Name);
                        entry.ExtractToFile(newPath, true);
                    }
                }
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

    /// <summary>
    /// Given a path, creates a zip archive and returns the bytes for the newly created zip archive.
    /// the zip is created in the same directory as the path provided
    /// optionally it deletes the zip from directory.
    /// </summary>
    /// <param name="a_sourcePath"></param>
    /// <returns></returns>
    public static byte[] CompressToZipArchiveBytes(string a_sourcePath, bool a_deleteFromDisk = true)
    {
        string zipPath = Create(a_sourcePath);
        byte[] zipBytes = System.IO.File.ReadAllBytes(zipPath);
        if (a_deleteFromDisk)
        {
            System.IO.File.Delete(zipPath);
        }

        return zipBytes;
    }

    public static byte[] CompressToZipArchiveBytes(List<string> a_files)
    {
        string zipPath = Path.GetTempFileName() + ".zip";
        Create(new List<string>(a_files), zipPath);
        byte[] zipBytes = System.IO.File.ReadAllBytes(zipPath);
        System.IO.File.Delete(zipPath);

        return zipBytes;
    }

    /// <summary>
    /// Similar to right-clicking a file/directory and compressing. If file to be zipped is
    /// "myfile.ext" it will create a zip called "myfile.zip" in the same directory
    /// </summary>
    /// <param name="a_sourcePath"></param>
    /// <returns></returns>
    public static string Create(string a_sourcePath)
    {
        string zipDir = Path.GetDirectoryName(a_sourcePath);
        string zipName = GetZipArchiveEntryName(a_sourcePath) + ".zip";
        string zipPath = File.FileUtils.GetUniqueFileName(Path.Combine(zipDir, zipName));

        Create(a_sourcePath, zipPath);

        return zipPath;
    }

    /// <summary>
    /// zips file/Directory in a_sourcePath to a_zipPath
    /// </summary>
    /// <param name="a_sourcePath"></param>
    /// <param name="a_zipPath"></param>
    public static void Create(string a_sourcePath, string a_zipPath)
    {
        using (FileStream destStream = new (a_zipPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (ZipArchive zip = new (destStream, ZipArchiveMode.Update))
            {
                AddEntry(zip, a_sourcePath, GetZipArchiveEntryName(a_sourcePath));
            }
        }
    }

    /// <summary>
    /// Create and save a Zip File given a table of file paths and file name
    /// </summary>
    /// <param name="a_zipFilePath">Zip file to create</param>
    /// <param name="a_filesToCompress">FilePaths as Keys and FileNames as Values</param>
    public static void Create(Dictionary<string, string> a_filesToCompress, string a_zipPath)
    {
        using (FileStream destStream = new (a_zipPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (ZipArchive zip = new (destStream, ZipArchiveMode.Update))
            {
                foreach (KeyValuePair<string, string> namePath in a_filesToCompress)
                {
                    AddEntry(zip, namePath.Key, namePath.Value);
                }
            }
        }
    }

    /// <summary>
    /// Create and save a Zip File given a table of file paths and file name
    /// </summary>
    /// <param name="a_zipFilePath">Zip file to create</param>
    /// <param name="a_filesToCompress">FilePaths as Keys and FileNames as Values</param>
    public static void Create(List<string> a_filesToCompress, string a_zipPath)
    {
        using (FileStream destStream = new (a_zipPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            using (ZipArchive zip = new (destStream, ZipArchiveMode.Update))
            {
                foreach (string filePath in a_filesToCompress)
                {
                    AddEntry(zip, filePath, GetZipArchiveEntryName(filePath));
                }
            }
        }
    }

    /// <summary>
    /// Adds entries to ZipArchive recursively
    /// </summary>
    /// <param name="a_archive"></param>
    /// <param name="a_pathToAdd"></param>
    /// <param name="a_name"></param>
    private static void AddEntry(ZipArchive a_archive, string a_pathToAdd, string a_name)
    {
        if (string.IsNullOrEmpty(a_name))
        {
            a_name = GetZipArchiveEntryName(a_pathToAdd);
        }

        if (System.IO.File.Exists(a_pathToAdd))
        {
            a_archive.CreateEntryFromFile(a_pathToAdd, a_name, CompressionLevel.Optimal);
        }
        else // directory
        {
            ZipArchiveEntry entry = a_archive.CreateEntry(a_name + Path.DirectorySeparatorChar);
            DirectoryInfo dirInfo = new (a_pathToAdd);

            foreach (FileInfo fileInfo in dirInfo.GetFiles())
            {
                string name = Path.Combine(entry.FullName, fileInfo.Name);
                AddEntry(a_archive, fileInfo.FullName, name);
            }

            foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
            {
                string name = Path.Combine(entry.FullName, subDir.Name);
                AddEntry(a_archive, subDir.FullName, name);
            }
        }
    }

    /// <summary>
    /// given a path to either File or Directory returns the name to be used
    /// in a zip archive (get short name and removes extension)
    /// </summary>
    /// <param name="a_sourcePath"></param>
    /// <returns></returns>
    private static string GetZipArchiveEntryName(string a_sourcePath)
    {
        if (System.IO.File.Exists(a_sourcePath))
        {
            return Path.GetFileName(a_sourcePath);
        }

        if (System.IO.Directory.Exists(a_sourcePath))
        {
            DirectoryInfo dirInfo = new (a_sourcePath);
            return dirInfo.Name;
        }

        throw new FileNotFoundException(a_sourcePath);
    }
}