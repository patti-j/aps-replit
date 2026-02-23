namespace PT.Scheduler;

/// <summary>
/// Manages a list of Change Orders for a Job.
/// </summary>
public class ChangeOrderList
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 473;

    public ChangeOrderList(IReader a_reader, Job a_job)
    {
        if (a_reader.VersionNumber >= 41)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                new ChangeOrder(a_reader, a_job);
            }
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion
}