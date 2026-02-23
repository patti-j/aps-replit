namespace PT.SchedulerDefinitions.Templates.Lists;

public class ExternalIdObject : IPTSerializable
{
    public const int UNIQUE_ID = 835;

    public string JobExternalId;
    public string MoExternalId;
    public string OperationExternalId;
    public string ActivityExternalId;

    #region IPTSerializable Members
    public ExternalIdObject(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out JobExternalId);
            a_reader.Read(out MoExternalId);
            a_reader.Read(out OperationExternalId);
            a_reader.Read(out ActivityExternalId);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(JobExternalId);
        a_writer.Write(MoExternalId);
        a_writer.Write(OperationExternalId);
        a_writer.Write(ActivityExternalId);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public ExternalIdObject(string a_jobExternalId, string a_moExternalId, string a_operationExternalId, string a_activityExternalId)
    {
        JobExternalId = a_jobExternalId;
        MoExternalId = a_moExternalId;
        OperationExternalId = a_operationExternalId;
        ActivityExternalId = a_activityExternalId;
    }
}