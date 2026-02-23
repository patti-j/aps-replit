using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all UserFieldDefinition related transmissions.
/// </summary>
public abstract class UserFieldDefinitionIdBaseT : UserFieldDefinitionBaseT
{
    // Shouldn't need an UNIQUE_ID for an abstract class
    //public const int UNIQUE_ID = 1117;

    #region IPTSerializable Members
    public UserFieldDefinitionIdBaseT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12503)
        {
            a_reader.Read(out int udfDefinitionCount);
            for (int i = 0; i < udfDefinitionCount; i++)
            {
                UserFieldDefinitionIds.Add(i, new BaseId(a_reader));
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(UserFieldDefinitionIds.Count);
        for (int i = 0; i < UserFieldDefinitionIds.Count; i++)
        {
            UserFieldDefinitionIds[i].Serialize(a_writer);
        }
    }
    #endregion

    public SortedList<int, BaseId> UserFieldDefinitionIds = new ();

    protected UserFieldDefinitionIdBaseT() { }

    protected UserFieldDefinitionIdBaseT(IEnumerable<BaseId> a_udfDefinitionIds)
    {
        int count = 0;
        foreach (BaseId udfDefinitionId in a_udfDefinitionIds)
        {
            UserFieldDefinitionIds.Add(count, udfDefinitionId);
            count++;
        }
    }
}