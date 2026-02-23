using PT.Common.File;

namespace PT.Common.Extensions;

//extends linq functionality to build exception message
public static class ExceptionExtensions
{
    public static IEnumerable<Exception> InnerExceptions(this Exception a_exception)
    {
        if (a_exception is AggregateException aggregate)
        {
            foreach (Exception innerException in aggregate.InnerExceptions)
            {
                Exception ex = innerException;
                while (ex != null)
                {
                    yield return ex;
                    ex = ex.InnerException;
                }
            }
        }
        else
        {
            Exception ex = a_exception;
            while (ex != null)
            {
                yield return ex;
                ex = ex.InnerException;
            }
        }
    }

    /// <summary>
    /// Displays exception messages for all nested exceptions.
    /// </summary>
    /// <param name="a_e"></param>
    /// <returns></returns>
    public static string GetExceptionFullMessage(this Exception a_e)
    {
        if (a_e == null)
        {
            return "";
        }

        return string.Concat(a_e.InnerExceptions().Select(e => e.Message + Environment.NewLine + Environment.NewLine));
    }

    /// <summary>
    /// Displays stack traces for all nested exceptions.
    /// </summary>
    /// <param name="a_e"></param>
    /// <returns></returns>
    public static string GetExceptionFullStackTrace(this Exception a_e)
    {
        return string.Concat(a_e.InnerExceptions()
                                .Select(e =>
                                    e.GetType() + Environment.NewLine + e.StackTrace + Environment.NewLine)
                                .Reverse());
    }

    /// <summary>
    /// Displays both message and stack trace for all nested exceptions.
    /// </summary>
    /// <param name="a_e"></param>
    /// <returns></returns>
    public static string GetExceptionFull(this Exception a_e)
    {
        return string.Concat(a_e.InnerExceptions()
                                .Select(e =>
                                    e.GetType() + ": " + e.Message + Environment.NewLine + e.StackTrace + Environment.NewLine + Environment.NewLine)
                                .Reverse());
    }

    public static ExceptionDescriptionInfo GenerateDescriptionInfo(this Exception a_e)
    {
        return new ExceptionDescriptionInfo(a_e);
    }
}