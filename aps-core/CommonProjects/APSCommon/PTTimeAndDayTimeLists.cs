namespace PT.APSCommon;

public class PTTimeAndDayTimeLists : TimeAndDayTimeLists
{
    protected override void ThrowException(string a_nextEntry)
    {
        throw new PTValidationException("2042", new object[] { a_nextEntry });
    }
}