using PT.Common.File;

namespace PT.Common.Compression;

public class ZipInfo
{
    public ZipInfo(string aZipFilePath)
    {
        m_zipFilePath = aZipFilePath;
    }

    private readonly Dictionary<string, string> addedFileNames = new ();

    internal void AddFile(string aFilePath)
    {
        //Make sure the path was not added already.  Shouldn't happen.
        string fileName = Path.GetFileName(aFilePath);
        if (!addedFileNames.ContainsKey(fileName)) //can't zip two files with the same name in the same file/path.  Some projects may reference others so duplicates are possible. Just need one.
        {
            addedFileNames.Add(fileName, fileName);
            FilesPathsToInclude.Add(aFilePath);
        }
    }

    public void AddFiles(string[] aFilePaths)
    {
        for (int i = 0; i < aFilePaths.Length; i++)
        {
            AddFile(aFilePaths.GetValue(i).ToString());
        }
    }

    private readonly string m_zipFilePath;
    private readonly List<string> FilesPathsToInclude = new ();

    public void SaveToZipFile()
    {
        byte[] zipBytes = Zip.CompressToZipArchiveBytes(FilesPathsToInclude);
        FileUtils.SaveBinaryFile(m_zipFilePath, zipBytes);
    }
}