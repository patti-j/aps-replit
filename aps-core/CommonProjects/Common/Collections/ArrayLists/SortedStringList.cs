using System.Collections;

namespace PT.Common;

/// <summary>
/// A sorted list of strings.
/// </summary>
public class SortedStringList : IEnumerable<string>
{
    private readonly SortedList<string, string> m_sl = new ();

    /// <summary>
    /// Add an array of strings to the sorted list that's created.
    /// </summary>
    /// <param name="a_strings"></param>
    public SortedStringList(string[] a_strings)
    {
        Add(a_strings);
    }

    public SortedStringList() { }

    /// <summary>
    /// Add a string to the list.
    /// </summary>
    /// <param name="s"></param>
    public void Add(string s)
    {
        m_sl.Add(s, s);
    }

    /// <summary>
    /// Add an array of strings to the sorted list.
    /// </summary>
    /// <param name="a_strings"></param>
    public void Add(string[] a_strings)
    {
        for (int i = 0; i < a_strings.Length; ++i)
        {
            string s = a_strings[i];
            m_sl.Add(s, s);
        }
    }

    /// <summary>
    /// The number of strings in the sorted list.
    /// </summary>
    public int Count => m_sl.Count;

    /// <summary>
    /// Return the string at a specified index within the sorted list.
    /// </summary>
    public string this[int a_index] => m_sl.Values[a_index];

    /// <summary>
    /// Determines whether the list contains the specified string.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public bool Contains(string s)
    {
        return m_sl.ContainsKey(s);
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach (string slValue in m_sl.Values)
        {
            yield return slValue;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}