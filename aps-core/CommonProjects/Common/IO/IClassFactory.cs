namespace PT.Common;

public delegate object ObjectCreatorDelegate(IReader a_reader);

public interface IClassFactory
{
    /// <summary>
    /// Create an object that was serialized.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    object Deserialize(IReader a_reader);
}