namespace PT.Common.Localization;

/// <summary>
/// This class is used to enable users to override the display text in any or all languages.
/// </summary>
internal class UserTextOverrider
{
    internal UserTextOverrider(string a_currentLanguage)
    {
        CurrentLanguage = a_currentLanguage;
    }

    private string m_currentLanguage;

    /// <summary>
    /// Set the language whose override file should be used.
    /// </summary>
    private string CurrentLanguage
    {
        set
        {
            m_currentLanguage = value;
            LoadStringForLanguage(m_currentLanguage);
        }
    }

    /// <summary>
    /// See if the overrides contains the specified string.
    /// Case sensitive.
    /// </summary>
    internal bool ContainsString(string a_findString)
    {
        return m_stringOverridesDictionary.ContainsKey(a_findString);
    }

    /// <summary>
    /// Returns the override string for the specified string.
    /// Case sensitive.
    /// Use ContainsString() first to ensure it exists or an error will occur.
    /// </summary>
    internal string GetOverrideString(string a_findString)
    {
        return m_stringOverridesDictionary[a_findString];
    }

    private readonly Dictionary<string, string> m_stringOverridesDictionary = new ();

    private void LoadStringForLanguage(string a_language)
    {
        string folder = Path.Combine(Localizer.LocalizationPath, "TextOverrides");

        string path = Path.Combine(folder, a_language + ".txt");
        if (System.IO.File.Exists(path))
        {
            Common.File.TextFile txtFile = new (path);
            char[] tabSepChar = "\t".ToCharArray(); //tab
            const string commentStartLine = "***";

            for (int i = 0; i < txtFile.Count; i++)
            {
                string nextLine = txtFile[i];
                if (!nextLine.StartsWith(commentStartLine)) //allow comments in the file
                {
                    string[] lineSegments = nextLine.Split(tabSepChar);
                    if (lineSegments.Length > 1)
                    {
                        string originalText = lineSegments.GetValue(0).ToString();
                        string newText = lineSegments.GetValue(1).ToString();
                        if (m_stringOverridesDictionary.ContainsKey(originalText))
                        {
                            throw new LocalizationException(Localizer.GetErrorString("2729", new object[] { path, originalText, i }));
                        }

                        m_stringOverridesDictionary.Add(originalText, newText);
                    }
                    else
                    {
                        throw new LocalizationException(Localizer.GetErrorString("2730", new object[] { i, path }));
                    }
                }
            }
        }
    }
}