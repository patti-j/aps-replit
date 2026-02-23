namespace PT.Scheduler.Templates.LinkedLists;

/// <summary>
/// A System.Collections.Generic.LinkedList that serializes the collection.
/// You have to override the Read function for deserialization of a single object.
/// </summary>
/// <typeparam name="Ty"></typeparam>
public abstract class SerializableLinkedList<Ty> : LinkedList<Ty>, IPTSerializable where Ty : IPTSerializable
{
    #region IPTSerializable Members
    public void Serialize(IWriter writer)
    {
        writer.Write(Count);
        LinkedListNode<Ty> current = First;
        while (current != null)
        {
            IPTSerializable o = current.Value;
            o.Serialize(writer);
            current = current.Next;
        }
    }

    public SerializableLinkedList(IReader reader)
    {
        int count;
        reader.Read(out count);
        for (int i = 0; i < count; ++i)
        {
            Ty o = Read(reader);
            AddLast(o);
        }
    }

    public abstract Ty Read(IReader reader);

    //674
    public abstract int UniqueId { get; }
    #endregion
}