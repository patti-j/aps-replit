using System.ComponentModel;

namespace PT.SystemServicesReportLibrary;

public class Helper
{
    private static string GetUrl(string a_url)
    {
        UriBuilder uriBuilder = new (a_url) { Scheme = "https" };
        return uriBuilder.Uri.AbsoluteUri;
    }

    public static string GetSystemMessage(int a_errorCode)
    {
        Win32Exception exception = new (a_errorCode);
        return exception.Message;
    }

    public static string GetFailureMessage(EventViewerLog a_failureLog)
    {
        string errorMsg = string.Empty;

        if (a_failureLog.SystemLog)
        {
            string errorIdStr = a_failureLog.Entry.Message.Substring(a_failureLog.Entry.Message.IndexOf("%%"), a_failureLog.Entry.Message.Length - a_failureLog.Entry.Message.IndexOf("%%"));
            int errorId = Convert.ToInt32(errorIdStr.Replace("%%", ""));

            try
            {
                errorMsg = GetSystemMessage(errorId);
            }
            catch (Exception ee)
            {
                throw ee;
            }
        }


        return errorMsg;
    }

    public static string GetInstanceNameFromServiceName(string a_serviceName)
    {
        string[] splitServiceName = a_serviceName.Split(' ');

        //APS {INSTANCE} {VERSION} {SERVICE NAME}
        return splitServiceName.Length < 4 ? string.Empty : splitServiceName[1];
    }

    public static string GetVersionNumberFromServiceName(string a_serviceName)
    {
        string[] splitServiceName = a_serviceName.Split(' ');

        //APS {INSTANCE} {VERSION} {SERVICE NAME}
        return splitServiceName.Length < 4 ? string.Empty : splitServiceName[2];
    }

    public static string GetServiceNameFromServiceName(string a_serviceName)
    {
        List<string> splitServiceName = a_serviceName.Split(' ').ToList();

        if (splitServiceName.Count < 4)
        {
            return string.Empty;
        }

        splitServiceName.RemoveRange(0, 3);

        return string.Join(" ", splitServiceName);
    }

    public static bool Between(DateTime a_date, DateTime a_startDate, DateTime a_endDate)
    {
        //If we are running the report where the report enddate is selected as the current date then it will ignore any events after 12:00am of the current/selected date, so this considers that.
        if (a_endDate.ToShortDateString() == PTDateTime.UserDateTimeNow.ToShortDateString())
        {
            return a_date >= a_startDate && a_date < a_endDate.AddDays(1);
        }

        return a_date >= a_startDate && a_date <= a_endDate;
    }
}