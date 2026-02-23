namespace PT.Scheduler;

internal class InternalActivityLinkedList : Templates.LinkedLists.SerializableLinkedList<InternalActivity>
{
    #region Serialization
    public InternalActivityLinkedList(IReader reader) :
        base(reader) { }

    public override InternalActivity Read(IReader reader)
    {
        return new InternalActivity(reader);
    }

    public const int UNIQUE_ID = 674;

    public override int UniqueId => UNIQUE_ID;
    #endregion
}