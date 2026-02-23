namespace PT.Scheduler;

public class DateIntKey : IComparable<DateIntKey>, IPTSerializable
{
    public const int UNIQUE_ID = 397;

    #region IPTSerializable Members
    public DateIntKey(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out count);

            reader.Read(out date);
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif

        writer.Write(count);

        writer.Write(date);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public DateIntKey(DateTime date, int count)
    {
        this.date = date;
        this.count = count;
    }

    private readonly DateTime date;
    private readonly int count; //To make key unique.

    #region IComparable Members
    public int CompareTo(DateIntKey a_c)
    {
        if (date < a_c.date)
        {
            return -1;
        }

        if (date > a_c.date)
        {
            return 1;
        }

        return count.CompareTo(a_c.count);
    }
    #endregion
}