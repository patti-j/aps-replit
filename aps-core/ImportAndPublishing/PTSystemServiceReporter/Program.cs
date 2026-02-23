using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.Common.File;
using PTEventIds = PT.Common.File.SimpleExceptionLogger.PTEventId;
using APSInstance = ServerManagerProxyLib.ServerManagerManagement.APSInstance;

namespace PT.SystemServiceReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            ReportManager reportMgr = new ReportManager();

            //Generate the report data to get our list of downtimes that require user input
            reportMgr.GenerateReportsData();

            foreach(InstanceReport report in reportMgr.Reports)
            {
                var unionedDowntimes = report.DowntimeRangesMaintenance.Union(report.DowntimeRangesOffline);

                var missingStartTimes = unionedDowntimes.Where(dt => dt.Downtime.RequiresManualInput).Select(dt => dt.Downtime).OrderBy( dt => dt.StartTime).ToList();

                if(missingStartTimes.Count > 0)
                {
                    HandleUserInputOfStartTimes(missingStartTimes);
                    Console.WriteLine("\nStart times have been added to those records. Please restart the application to run the report.\nPress [ENTER] to exit...");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }

            DisplayReports(reportMgr.Reports);

            Console.ReadLine();
        }

        private static void HandleUserInputOfStartTimes(List<Downtime> a_listOfDowntimesMissingStarts)
        {
            Console.WriteLine($"{a_listOfDowntimesMissingStarts.Count} downtime instances caused by unexpected system shutdowns were found in the 'System' event log without a valid start datetime.\nYou must manually enter the approximate start datetime for each instance in yyyy/MM/dd HH:mm:ss format\n");
            for (int i = 0; i < a_listOfDowntimesMissingStarts.Count; i++)
            {
                var downtime = a_listOfDowntimesMissingStarts[i];
                Console.WriteLine($"Unexpected Shutdown #{i} - Windows EventID: {downtime.StoppedLog.Entry.EventID} , Reported At: {downtime.StoppedLog.Entry.TimeWritten}\nEnter approximate start datetimes in yyyy/MM/dd HH:mm:ss format:\n");

                DateTime startInput = DateTime.Now;
                while (!DateTime.TryParse(Console.ReadLine(), out startInput))
                {
                    Console.WriteLine($"\nERROR: Invalid format of start datetime. Please enter in yyyy/MM/dd HH:mm:ss format:\n");
                }

                bool userConfirmation = false;

                while(!userConfirmation)
                {
                    Console.WriteLine($"Are you sure that you want to save {startInput} as the start datetime?\ny/n: ");
                    userConfirmation = Console.ReadLine().StartsWith("y", StringComparison.CurrentCultureIgnoreCase);

                    if (userConfirmation) break;

                    Console.WriteLine("Enter approximate start datetimes in yyyy/MM/dd HH:mm:ss format:");
                    while (!DateTime.TryParse(Console.ReadLine(), out startInput))
                    {
                        Console.WriteLine($"\nERROR: Invalid format of start datetime. Please enter in yyyy/MM/dd HH:mm:ss format:\n");
                    }
                }

                downtime.StartTime = startInput;

                //Write our custom log entry so that when the report is ran again we can find the updated data
                string msg = $"The unexpected shutdown previously logged with eventId: {downtime.StoppedLog.Entry.EventID} @ {downtime.StoppedLog.Entry.TimeWritten} has been updated.\nApprox. StartTime: {startInput}";
                SimpleExceptionLogger.LogMessage("APS Server Manager", msg, PTEventIds.MANUAL_INPUT);
            }
        }
        private static void DisplayReports(List<InstanceReport> a_reports)
        {
            Console.WriteLine("APS INSTANCE UPTIME REPORT - LAST 30 DAYS\n");

            Console.WriteLine("UPTIME STATS:");
            foreach (InstanceReport report in a_reports)
            {
                double totalTime = report.TotalUptime.TotalSeconds + report.TotalDowntimeMaintenance.TotalSeconds + report.TotalDowntimeOffline.TotalSeconds;
                double percentUptime = (report.TotalUptime.TotalSeconds / (totalTime > 0 ? totalTime : 1)) * 100;
                double percentOffline = (report.TotalDowntimeOffline.TotalSeconds / (totalTime > 0 ? totalTime : 1)) * 100;
                double percentMaintenance = (report.TotalDowntimeMaintenance.TotalSeconds / (totalTime > 0 ? totalTime : 1)) * 100;
                Console.WriteLine($"\t{report.Instance.PublicInfo.InstanceName} ({report.Instance.PublicInfo.SoftwareVersion}) - UPTIME: {report.TotalUptime} , % ONLINE: {percentUptime.ToString("F4")}% , % OFFLINE: {percentOffline.ToString("F4")}% , % MAINTENANCE: {percentMaintenance.ToString("F4")}%");
            }


            Console.WriteLine("\nDOWNTIME STATS:");
            foreach (InstanceReport report in a_reports)
            {
                var unionedDowntimes = report.DowntimeRangesMaintenance.Union(report.DowntimeRangesOffline).OrderBy( tr => tr.Start );
                foreach (TimeRange dt in unionedDowntimes)
                {
                    Console.WriteLine($"\t{report.Instance.PublicInfo.InstanceName} ({report.Instance.PublicInfo.SoftwareVersion}) [{dt.Start}] - DURATION: {dt.Duration} , END: {dt.Start + dt.Duration} , REASON: {(dt.Downtime == null ? "N/A" : dt.Downtime.Reason)}");
                }
            }
        }
    }
}
