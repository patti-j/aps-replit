namespace PT.Common.Collections;

/// <summary>
/// The purpose of this class is to simplify the usage of a dictionary of collections.
/// It is common to have a lookup dictionary for key, List
/// <T>
/// .
/// This dictionary handles the verbosity caused by adding and looking up collections from a dictionary.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TCollectionType"></typeparam>
public class DictionaryCollection<TKey, TCollectionType> : Dictionary<TKey, List<TCollectionType>>
{
    public void Add(TKey a_key, TCollectionType a_object)
    {
        if (TryGetValue(a_key, out List<TCollectionType> collection))
        {
            collection.Add(a_object);
        }
        else
        {
            Add(a_key, new List<TCollectionType> { a_object });
        }
    }

    public bool Remove(TKey a_key, TCollectionType a_object)
    {
        if (TryGetValue(a_key, out List<TCollectionType> collection))
        {
            return collection.Remove(a_object);
        }

        return false;
    }

    public List<TCollectionType> GetCollection(TKey a_key)
    {
        if (TryGetValue(a_key, out List<TCollectionType> collection))
        {
            return collection;
        }

        return new List<TCollectionType>();
    }

    public List<TCollectionType> GetCollectionCopy(TKey a_key)
    {
        if (TryGetValue(a_key, out List<TCollectionType> collection))
        {
            return collection.ToList();
        }

        return new List<TCollectionType>();
    }
}