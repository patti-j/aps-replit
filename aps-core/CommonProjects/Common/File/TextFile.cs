using System.Text;

namespace PT.Common.File;

public class TextFile : IPTSerializable
{
    public TextFile() { }

    /// <summary>
    /// Reads the contents of the path into the file automatically.
    /// </summary>
    /// <param name="a_fullPath"></param>
    public TextFile(string a_fullPath)
    {
        ReadFile(a_fullPath);
    }

    /// <summary>
    /// Reads the contents of the path into the file automatically.
    /// </summary>
    public TextFile(string a_fullPath, FileShare a_fileShare)
    {
        m_fileLines.Clear();

        using (StreamReader sr = new (new FileStream(a_fullPath, FileMode.Open, FileAccess.Read, a_fileShare)))
        {
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                m_fileLines.Add(line);
            }
        }

        m_pathReadFrom = a_fullPath; //store for later reference
    }

    private readonly StringArrayList m_fileLines = new ();

    #region IPTSerializable Members
    public TextFile(IReader reader)
    {
        if (reader.VersionNumber >= 55)
        {
            reader.Read(out m_pathReadFrom);
            m_fileLines = new StringArrayList(reader);
        }

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            m_fileLines = new StringArrayList(reader);
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_pathReadFrom);
        m_fileLines.Serialize(writer);
    }

    public int UniqueId =>
        // TODO:  Add TextFile.UniqueId getter implementation
        0;
    #endregion

    private string m_pathReadFrom;

    /// <summary>
    /// If the Text file read in from a path then this is stored here.
    /// </summary>
    public string PathReadFrom => m_pathReadFrom;

    /// <summary>
    /// Read the file and stored its data in this class.
    /// </summary>
    /// <param name="a_fullPath">The path and file name of the file.</param>
    public void ReadFile(string a_fullPath)
    {
        m_fileLines.Clear();

        using (StreamReader sr = new (a_fullPath))
        {
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                m_fileLines.Add(line);
            }
        }

        m_pathReadFrom = a_fullPath; //store for later reference
    }

    public void WriteFile(string a_fullPath)
    {
        WriteFile(a_fullPath, false);
    }

    public void WriteFile(string a_fullPath, bool a_append)
    {
        using (StreamWriter sw = new (a_fullPath, a_append))
        {
            for (int i = 0; i < m_fileLines.Count; ++i)
            {
                sw.WriteLine(m_fileLines[i]);
            }
        }
    }

    /// <summary>
    /// Append some text to this text file.
    /// </summary>
    /// <param name="a_text"></param>
    public void AppendText(string a_text)
    {
        m_fileLines.Add(a_text);
    }

    /// <summary>
    /// Append text from the specified Text File to this Text File.
    /// </summary>
    /// <param name="a_appendFrom"></param>
    public void Append(TextFile a_appendFrom)
    {
        for (int i = 0; i < a_appendFrom.Count; i++)
        {
            m_fileLines.Add(a_appendFrom[i]);
        }
    }

    public int Count => m_fileLines.Count;

    public string this[int a_line]
    {
        get => m_fileLines[a_line];

        set => m_fileLines[a_line] = value;
    }

    public string Text
    {
        get
        {
            StringBuilder builder = new ();
            for (int i = 0; i < m_fileLines.Count; i++)
            {
                builder.AppendLine(m_fileLines[i]);
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Determine whether two successor MOs are Equal.
    /// </summary>
    /// <param name="a_obj"></param>
    /// <returns></returns>
    public override bool Equals(object a_obj)
    {
        if (a_obj == null || GetType() != a_obj.GetType())
        {
            return false;
        }

        TextFile tmpFile = (TextFile)a_obj;

        if (Count != tmpFile.Count)
        {
            return false;
        }

        for (int lineI = 0; lineI < Count; ++lineI)
        {
            if (this[lineI] != tmpFile[lineI])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determine whether two successor MOs are equal.
    /// </summary>
    /// <param name="a_obj1"></param>
    /// <param name="a_obj2"></param>
    /// <returns></returns>
    public static bool operator ==(TextFile a_obj1, TextFile a_obj2)
    {
        // This also handles the case where 
        if (ReferenceEquals(a_obj1, a_obj2))
        {
            return true;
        }

        return a_obj1.Equals(a_obj2);
    }

    /// <summary>
    /// Determine whether two successor MOs are not equal.
    /// </summary>
    /// <param name="a_obj1"></param>
    /// <param name="a_obj2"></param>
    /// <returns></returns>
    public static bool operator !=(TextFile a_obj1, TextFile a_obj2)
    {
        return !(a_obj1 == a_obj2);
    }

    //TODO: Can this function be removed?
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Find the first line number that contains the text. -1 is returned if no line is found.
    /// O(n).
    /// </summary>
    public int FindFirstOf(string a_findText)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (this[i].Contains(a_findText))
            {
                return i;
            }
        }

        return -1;
    }

    public void ClearText()
    {
        m_fileLines.Clear();
    }
}