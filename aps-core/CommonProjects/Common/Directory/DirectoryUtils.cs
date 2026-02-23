namespace PT.Common.Directory;

/// <summary>
/// Summary description for DirectoryUtils.
/// </summary>
public class DirectoryUtils
{
    public static string GetPathFromFullPath(string a_fullPath)
    {
        string fileName = Path.GetFileName(a_fullPath);
        int fileNameIdx = a_fullPath.IndexOf(fileName);
        string directory = a_fullPath.Substring(0, fileNameIdx);
        return directory;
    }

    public static void CopyDirectory(string a_sourcePath, string a_destinationPath, bool a_recurse)
    {
        string[] files;

        if (a_destinationPath[a_destinationPath.Length - 1] != Path.DirectorySeparatorChar)
        {
            a_destinationPath += Path.DirectorySeparatorChar;
        }

        if (!System.IO.Directory.Exists(a_destinationPath))
        {
            System.IO.Directory.CreateDirectory(a_destinationPath);
        }

        files = System.IO.Directory.GetFileSystemEntries(a_sourcePath);
        foreach (string element in files)
        {
            if (a_recurse)
            {
                if (System.IO.Directory.Exists(element))
                {
                    CopyDirectory(element, a_destinationPath + Path.GetFileName(element), true);
                }
                else
                {
                    System.IO.File.Copy(element, a_destinationPath + Path.GetFileName(element), true);
                }
            }
            else
            {
                if (!System.IO.Directory.Exists(element))
                {
                    System.IO.File.Copy(element, a_destinationPath + Path.GetFileName(element), true);
                }
            }
        }
    }

    public static bool FileExistsInFolderOrSubfolder(string a_parentFolder, string a_searchPattern)
    {
        if (!System.IO.Directory.Exists(a_parentFolder))
        {
            return false;
        }

        if (System.IO.Directory.GetFiles(a_parentFolder, a_searchPattern).Length > 0)
        {
            return true;
        }

        //Search subfolders recursively
        string[] files = System.IO.Directory.GetFileSystemEntries(a_parentFolder);

        foreach (string element in files)
        {
            if (FileExistsInFolderOrSubfolder(element, a_searchPattern))
            {
                return true;
            }
        }

        return false;
    }

    public static string FindFileInFolderOrSubfolder(string a_parentFolder, string a_searchPattern)
    {
        if (!System.IO.Directory.Exists(a_parentFolder))
        {
            return "";
        }

        string[] foundFiles = System.IO.Directory.GetFiles(a_parentFolder, a_searchPattern, SearchOption.AllDirectories);
        if (foundFiles.Length > 0)
        {
            return foundFiles.GetValue(0).ToString();
        }

        return "";
    }

    // Different than the .NET Directory.Delete function in that it will delete read only files too.
    public static void Delete(string a_directory)
    {
        string[] files = System.IO.Directory.GetFileSystemEntries(a_directory);

        foreach (string element in files)
        {
            if (System.IO.Directory.Exists(element))
            {
                Delete(element);
            }
            else
            {
                System.IO.File.SetAttributes(element, FileAttributes.Normal);
                System.IO.File.Delete(element);
            }
        }

        File.FileUtils.DeleteDirectoryRecursivelyWithRetry(a_directory);
    }

    #region Invalid character replacement for File and Path names.
    //static void Main(string[] args)
    //{
    //    Console.WriteLine(TEST_STRING);

    //    string x = ReplaceInvalidFilePathChars(TEST_STRING, (int)'_');
    //    Console.WriteLine(x);

    //    string y = ReplaceInvalidFilePathChars(TEST_STRING, -1);
    //    Console.WriteLine(y);
    //}

    /// <summary>
    /// Assumes the path passed in doesn't contain any of the characters between 200 and 254. And that there are less 56 invalid characters.
    /// If you don't specify a replacement character invalid characters are mapped onto this range of characters.
    /// </summary>
    /// <param name="a_invalids">Array of invalid characters</param>
    /// <param name="a_name">Name to adjust</param>
    /// <param name="a_replacementChar">-1 to use defaults; otherwise invalid character are mapped to the range from 200-254.</param>
    /// <returns>A string with any replacements made</returns>
    public static string ReplaceInvalidFilePathChars(string a_filePath, int a_replacementChar)
    {
        int split = a_filePath.LastIndexOf('\\');
        int start = split + 1;
        string fileNameTemp = a_filePath.Substring(start, a_filePath.Length - start);
        string pathTemp = a_filePath.Substring(0, split);
        string fileName = ReplaceInvalidFileNameChars(fileNameTemp, a_replacementChar);
        string path = ReplaceInvalidPathChars(pathTemp, a_replacementChar);
        string newPath = Path.Combine(path, fileName);
        return newPath;
    }

    /// <summary>
    /// Assumes the path passed in doesn't contain any of the characters between 200 and 254
    /// if you don't specify a replacement character.
    /// </summary>
    /// <param name="a_invalids">Array of invalid characters</param>
    /// <param name="a_name">Name to adjust</param>
    /// <param name="a_replacementChar">-1 to use defaults; otherwise invalid character are mapped to the range from 200-254.</param>
    /// <returns>A string with any replacements made</returns>
    public static string ReplaceInvalidFileNameChars(string a_fileName, int a_replacementChar)
    {
        return ReplaceInvalidChars(Path.GetInvalidFileNameChars(), a_fileName, a_replacementChar);
    }

    /// <summary>
    /// Assumes the path passed in doesn't contain any of the characters between 200 and 254
    /// if you don't specify a replacement character.
    /// Sync this with Silverlight.Common.DirectoryUtilsGetInvalidDirectoryChars().
    /// </summary>
    /// <param name="a_invalids">Array of invalid characters</param>
    /// <param name="a_name">Name to adjust</param>
    /// <param name="a_replacementChar">-1 to use defaults; otherwise invalid character are mapped to the range from 200-254.</param>
    /// <returns>A string with any replacements made</returns>
    public static string ReplaceInvalidPathChars(string a_path, int a_replacementChar)
    {
        char[] temp = Path.GetInvalidPathChars();
        char[] invalids = new char[temp.Length + 9];
        for (int i = 0; i < temp.Length; ++i)
        {
            invalids[i] = temp[i];
        }

        invalids[temp.Length] = '\\';
        invalids[temp.Length + 1] = '/';
        invalids[temp.Length + 2] = ':';
        invalids[temp.Length + 3] = '*';
        invalids[temp.Length + 4] = '?';
        invalids[temp.Length + 5] = '"';
        invalids[temp.Length + 6] = '<';
        invalids[temp.Length + 7] = '>';
        invalids[temp.Length + 8] = '|';

        bool hasDriveLetter = a_path[1] == ':';
        char[] newPathCharArray = ReplaceInvalidChars(invalids, a_path, a_replacementChar).ToCharArray();
        if (hasDriveLetter)
        {
            newPathCharArray[1] = ':';
        }

        return new string(newPathCharArray);
    }

    /// <summary>
    /// Assumes the path passed in doesn't contain any of the characters between 200 and 254
    /// if you don't specify a replacement character.
    /// </summary>
    /// <param name="a_invalids">Array of invalid characters</param>
    /// <param name="a_name">Name to adjust</param>
    /// <param name="a_replacementChar">-1 to use defaults; otherwise invalid character are mapped to the range from 200-254.</param>
    /// <returns>A string with any replacements made</returns>
    private static string ReplaceInvalidChars(char[] a_invalids, string a_name, int a_replacementChar)
    {
        char[] array = new char[a_name.Length];

        for (int i = 0; i < a_name.Length; ++i)
        {
            char c = a_name[i];
            if (a_invalids.Contains(c))
            {
                if (a_replacementChar >= 0)
                {
                    c = (char)a_replacementChar;
                }
                else
                {
                    c = (char)(c + 200);
                }
            }

            array[i] = c;
        }

        return new string(array);
    }
    #endregion

    #region VisitAllSubDirectories
    // Created for VisitAllSubDirectories().
    public delegate void SubDirectoryVisitDelegate(string a_subDirectory);

    /// <summary>
    /// Call a delegate for every subdirectory, drilling into each subdirectory and so on. Also called on the initial subdirectory passed in.
    /// </summary>
    /// <param name="a_subDirectory">The starting subdirectory.</param>
    /// <param name="a_visitDelegate">This delegate is called for each subdirectory.</param>
    public static void VisitAllSubDirectories(string a_subDirectory, SubDirectoryVisitDelegate a_visitDelegate)
    {
        if (System.IO.Directory.Exists(a_subDirectory))
        {
            a_visitDelegate(a_subDirectory);

            string[] subdirectories = System.IO.Directory.GetDirectories(a_subDirectory);

            foreach (string folder in subdirectories)
            {
                VisitAllSubDirectories(folder, a_visitDelegate);
            }
        }
    }
    #endregion

    /// <summary>
    /// Validates if the directory exists and creates it if missing.
    /// </summary>
    /// <param name="a_directory"></param>
    public static void ValidateDirectory(string a_directory)
    {
        if (!System.IO.Directory.Exists(a_directory))
        {
            System.IO.Directory.CreateDirectory(a_directory);
        }
    }
}