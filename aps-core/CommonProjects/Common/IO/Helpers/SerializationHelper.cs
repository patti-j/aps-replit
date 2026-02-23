namespace PT.Common;

public class SerializationHelper
{
    public static long ReadTimeSpanAsLong(IReader a_reader)
    {
        TimeSpan ts;
        a_reader.Read(out ts);
        return ts.Ticks;
    }

    public static long ReadDateTimeAsLong(IReader a_reader)
    {
        DateTime dt;
        a_reader.Read(out dt);
        return dt.Ticks;
    }
}