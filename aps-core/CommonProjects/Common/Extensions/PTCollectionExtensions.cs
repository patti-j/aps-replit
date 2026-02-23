using System.Collections;

namespace PT.Common.Extensions;

public static class PTCollectionExtensions
{
    /// <summary>
    /// Returns whether any values in the provided list exist in this list.
    /// </summary>
    public static bool ContainsAny<T>(this IEnumerable<T> a_collection, IEnumerable<T> a_values)
    {
        foreach (T value in a_values)
        {
            if (a_collection.Contains(value))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns whether all values in the provided list exist in this list.
    /// </summary>
    public static bool ContainsAll<T>(this IEnumerable<T> a_collection, IEnumerable<T> a_values)
    {
        foreach (T value in a_values)
        {
            if (!a_collection.Contains(value))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Merges a second collection into the first without duplicates
    /// </summary>
    public static void MergeWith<T>(this ISet<T> a_collection, ISet<T> a_values)
    {
        foreach (T value in a_values)
        {
            if (!a_collection.Contains(value))
            {
                a_collection.Add(value);
            }
        }
    }

    /// <summary>
    /// Returns a new list with the same object references as the first.
    /// </summary>
    public static List<T> ShallowCopy<T>(this List<T> a_collection)
    {
        if (a_collection == null)
        {
            return null;
        }

        List<T> clone = new (a_collection.Count);
        foreach (T value in a_collection)
        {
            clone.Add(value);
        }

        return clone;
    }

    /// <summary>
    /// Returns a new HashSet with the same object references as the first.
    /// </summary>
    public static void ShallowCopyTo<T>(this HashSet<T> a_collection, HashSet<T> a_targetCollection)
    {
        a_targetCollection.Clear();
        if (a_collection == null || a_collection.Count == 0)
        {
            return;
        }

        foreach (T value in a_collection)
        {
            a_targetCollection.Add(value);
        }
    }

    /// <summary>
    /// Adds an object to the collection if it does not already exist
    /// Calls Contains on the collection
    /// </summary>
    /// <returns>Whether the object was added</returns>
    public static bool AddIfNew<TKey, TValue>(this IDictionary<TKey, TValue> a_collection, KeyValuePair<TKey, TValue> a_object)
    {
        if (!a_collection.ContainsKey(a_object.Key))
        {
            a_collection.Add(a_object);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds an object to the collection if it does not already exist
    /// Calls Contains on the collection
    /// </summary>
    /// <returns>Whether the object was added</returns>
    public static bool AddIfNew<TKey, TValue>(this IDictionary<TKey, TValue> a_collection, TKey a_key, TValue a_value)
    {
        if (!a_collection.ContainsKey(a_key))
        {
            a_collection.Add(a_key, a_value);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds an object to the collection if it does not already exist
    /// Calls Contains on the collection
    /// </summary>
    /// <returns>Whether the object was added</returns>
    public static bool AddIfNew<T>(this ICollection<T> a_collection, T a_object)
    {
        if (!a_collection.Contains(a_object))
        {
            a_collection.Add(a_object);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds an object to the collection if it does not already exist
    /// Calls Contains on the collection
    /// </summary>
    /// <returns>Whether the object was added</returns>
    public static int AddRangeIfNew<T>(this ICollection<T> a_collection, IEnumerable<T> a_objects)
    {
        int count = 0;
        foreach (T o in a_objects)
        {
            a_collection.AddIfNew(o);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Adds all of the elements of the provided dictionary into the destination dictionary.
    /// Any existing keys will be overridden with the source dictionary's value.
    /// </summary>
    /// <param name="a_destinationDictionary">The dictionary to add values to</param>
    /// <param name="a_sourceDictionary">The dictionary to take values from</param>
    /// <returns>Returns the destination dictionary</returns>
    public static Dictionary<T, T2> MergeWithOverride<T, T2>(this Dictionary<T, T2> a_destinationDictionary, Dictionary<T, T2> a_sourceDictionary)
    {
        a_sourceDictionary.ToList().ForEach(x => a_destinationDictionary[x.Key] = x.Value);
        return a_destinationDictionary;
    }

    /// <summary>
    /// Shallow copy. Adds all elements from a dictionary into a new dictionary
    /// </summary>
    /// <param name="a_sourceDictionary">The dictionary to clone</param>
    public static Dictionary<T, T2> Clone<T, T2>(this Dictionary<T, T2> a_sourceDictionary)
    {
        Dictionary<T, T2> clone = new ();
        clone.MergeWithOverride(a_sourceDictionary);
        return clone;
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T> a_enumerable)
    {
        if (a_enumerable is string stringCheck)
        {
            return string.IsNullOrEmpty(stringCheck);
        }
        
        return a_enumerable == null || !a_enumerable.Any();
    }

    public static object[] GetArray(this SortedList l)
    {
        object[] objs = new object[l.Count];
        for (int i = 0; i < l.Count; ++i)
        {
            objs[i] = l.GetByIndex(i);
        }

        return objs;
    }

    /// <summary>
    /// Finds the entry with the greatest key strictly less than the target key <paramref name="a_targetKey"/>
    /// or less than or equal to if <paramref name="a_targetKeyIncluded"/> is true .
    /// </summary>
    public static bool TryGetLowerEntry<TKey, TValue>(this SortedList<TKey, TValue> a_list, TKey a_targetKey, bool a_targetKeyIncluded, out KeyValuePair<TKey, TValue> a_entry) where TKey : notnull
    {
        IList<TKey> keys = a_list.Keys;
        IComparer<TKey> cmp = a_list.Comparer;
        int lo = 0;
        int hi = a_list.Count - 1;
        int best = -1;

        while (lo <= hi)
        {
            int mid = lo + ((hi - lo) >> 1);
            int c = cmp.Compare(keys[mid], a_targetKey);
            if (c < 0 || (a_targetKeyIncluded && c == 0))
            {
                //keys[mid] < key → candidate
                best = mid;
                lo = mid + 1;
            }
            else
            {
                // keys[mid] >= key → go left
                hi = mid - 1;
            }
        }

        if (best >= 0)
        {
            a_entry = new KeyValuePair<TKey, TValue>(keys[best], a_list.Values[best]);
            return true;
        }

        a_entry = default;
        return false;
    }

    public static int FirstIndexOfStartWith(this List<string> a_list, string a_value, StringComparison a_comparisonType)
    {
        for (int i = 0; i < a_list.Count; ++i)
        {
            string s = a_list[i];

            if (s.StartsWith(a_value, a_comparisonType))
            {
                return i;
            }
        }

        return -1;
    }
}
