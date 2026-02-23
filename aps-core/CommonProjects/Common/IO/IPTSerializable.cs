namespace PT.Common;

public interface IPTSerializable
{
    void Serialize(IWriter a_writer);

    void Serialize(IWriter a_writer, params string[] a_params)
    {
        Serialize(a_writer);
    }

    int UniqueId { get; }
}

/// <summary>
/// This is a clarifying interface that indicates that this type can be deserialized with an IReader constructor
/// </summary>
public interface IPTDeserializable : IPTSerializable
{
}