namespace PT.Common;

/// <summary>
/// An interface that allows an object to be stored in a CustomSortedDictionary or any other collection that requires a unique key from an object
/// </summary>
/// <typeparam name="KeyT"></typeparam>
public interface IKey<KeyT> : IEquatable<KeyT>
{
    KeyT GetKey();
}