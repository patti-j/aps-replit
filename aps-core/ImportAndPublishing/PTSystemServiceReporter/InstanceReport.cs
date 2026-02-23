using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using PTEventIds = PT.Common.File.SimpleExceptionLogger.PTEventId;
using APSInstance = ServerManagerProxyLib.ServerManagerManagement.APSInstance;

namespace PT.SystemServiceReporter
{
    public class InstanceReport
    {
        public static string[] s_services = new string[]
        {
            "System",
            "Scheduling Agent",
            "Interface",
            "Extra Services",
            "ClientUpdater"
        };
        private Dictionary<int, string> m_windowsEventIds;
        private EventLog m_applicationEventLogs;
        private EventLog m_systemEventLogs;

        private List<EventViewerLog> m_stoppedLogs { get; set; }
        private List<EventViewerLog> m_startedLogs { get; set; }
        private List<EventViewerLog> m_appFailureLogs { get; set; }
        private List<EventViewerLog> m_updatedLogs { get; set; }

        public APSInstance Instance { get; set; }
        public List<string> FullServiceNames { get; set; }
        public List<EventViewerLog> SystemEventLogs { get; set; }
        public List<EventViewerLog> ApplicationEventLogs { get; set; }

        public List<Downtime> Downtimes { get; set; }
        public List<TimeRange> DowntimeRangesMaintenance { get; set; }
        public List<TimeRange> DowntimeRangesOffline { get; set; }
        public List<TimeRange> UptimeRanges { get; set; }

        #region STATS

        public TimeSpan TotalDowntimeMaintenance { get; set; }
        public TimeSpan TotalDowntimeOffline { get; set; }
        public TimeSpan TotalUptime { get; set; }

        #endregion

        public InstanceReport()
        {
            FullServiceNames = new List<string>();
            SystemEventLogs = new List<EventViewerLog>();
            ApplicationEventLogs = new List<EventViewerLog>();
            Downtimes = new List<Downtime>();
            DowntimeRangesMaintenance = new List<TimeRange>();
            DowntimeRangesOffline = new List<TimeRange>();
            UptimeRanges = new List<TimeRange>();

            m_startedLogs = new List<EventViewerLog>();
            m_stoppedLogs = new List<EventViewerLog>();
            m_appFailureLogs = new List<EventViewerLog>();
            m_updatedLogs = new List<EventViewerLog>();
            m_applicationEventLogs = new EventLog("Application");
            m_systemEventLogs = new EventLog("System");

            m_windowsEventIds = new Dictionary<int, string>()
            {
                { 41, "Unexpected Shutdown" },
                { 1074, "An App Or User Initiated Restart" }, //This includes restarts due to windows updates
                { 4609, "Windows Shutting Down" }
            };
        }

        /// <summary>
        /// Collects all necessary log files and calculates uptime/downtime statistics for the report.
        /// </summary>
        public void Generate()
        {
            //Get all of the logs for each of our "per-instance" services
            foreach(string serviceName in FullServiceNames)
            {
                GetEventViewerLogsForService(serviceName);
            }
            GetEventViewerLogsForWindows();

            //Sort the lists of logs for future use
            m_stoppedLogs.Sort((x, y) => x.Entry.TimeGenerated.CompareTo(y.Entry.TimeGenerated));
            m_startedLogs.Sort((x, y) => x.Entry.TimeGenerated.CompareTo(y.Entry.TimeGenerated));
            m_appFailureLogs.Sort((x, y) => x.Entry.TimeGenerated.CompareTo(y.Entry.TimeGenerated));
            m_updatedLogs.Sort((x, y) => x.Entry.TimeGenerated.CompareTo(y.Entry.TimeGenerated));

            //Calculate uptime/downtime from graceful stops, failures, windows events, etc.
            CalculateDowntimeFromGracefulStop();
            CalculateDowntimeFromFailures();
            CalculateDowntimeFromWindowsEvents();
            CalculateCurrentUptime();

            //Merge and remove any overlaping downtimes
            RemoveOverlappingTimeRanges(DowntimeRangesMaintenance);
            RemoveOverlappingTimeRanges(DowntimeRangesOffline);
            RemoveOverlappingTimeRanges(UptimeRanges);

            foreach (TimeRange tr in DowntimeRangesMaintenance)
            {
                TotalDowntimeMaintenance += tr.Duration;
            }

            foreach (TimeRange tr in DowntimeRangesOffline)
            {
                TotalDowntimeOffline += tr.Duration;
            }

            foreach (TimeRange tr in UptimeRanges)
            {
                TotalUptime += tr.Duration;
            }
        }

        /// <summary>
        /// Populates lists of System event logs from Windows events.
        /// </summary>
        private void GetEventViewerLogsForWindows()
        {
            foreach (EventLogEntry systemLog in m_systemEventLogs.Entries)
            {
                EventViewerLog eventViewLog = new EventViewerLog();

                //Get system logs for windows events: restarts, shutdowns, etc.
                if (m_windowsEventIds.Keys.Contains(systemLog.EventID) && systemLog.TimeGenerated >= DateTime.Now.AddDays(-30))
                {
                    eventViewLog.Entry = systemLog;
                    eventViewLog.SystemLog = true;

                    SystemEventLogs.Add(eventViewLog);
                }
            }

            foreach(EventLogEntry appLog in m_applicationEventLogs.Entries)
            {
                EventViewerLog eventViewLog = new EventViewerLog();

                //Find the updated records if any exist
                if(appLog.Source == "APS Server Manager" && appLog.EventID == (int)PTEventIds.MANUAL_INPUT && appLog.TimeGenerated >= DateTime.Now.AddDays(-30))
                {
                    eventViewLog.Entry = appLog;
                    m_updatedLogs.Add(eventViewLog);
                }
            }
        }

        /// <summary>
        /// Populates the lists of log files with event logs from an APS service
        /// </summary>
        /// <param name="a_serviceName">Full APS service name. Example: APS Debug 0.0 System</param>
        private void GetEventViewerLogsForService(string a_serviceName)
        {
            string instanceName = Helper.GetInstanceNameFromServiceName(a_serviceName);
            string serviceName = Helper.GetServiceNameFromServiceName(a_serviceName);
            string versionNumber = Helper.GetVersionNumberFromServiceName(a_serviceName);

            //Get Application logs
            foreach (EventLogEntry appLog in m_applicationEventLogs.Entries)
            {
                EventViewerLog eventViewLog = new EventViewerLog();

                if(ServiceNameMatches(appLog.Source, a_serviceName) && appLog.TimeGenerated >= DateTime.Now.AddDays(-30))
                {
                    eventViewLog.Entry = appLog;
                    eventViewLog.FullSourceName = appLog.Source;

                    ApplicationEventLogs.Add(eventViewLog);
                }
            }

            if(ApplicationEventLogs.Count == 0)
            {
                throw new Exception("No APS service logs found in event viewer."); //If we have no logs from APS then we can't do any calc. Maybe we could check for SQL logs at this point.
            }


            m_appFailureLogs.AddRange(ApplicationEventLogs.Where(x => ServiceNameMatches(a_serviceName, x.Entry.Source) && (x.Entry.Message.Contains("service terminated unexpectedly") || x.Entry.EventID == (int)PTEventIds.UNHANDLED_EXCEPTION))
                                                  .OrderBy(x => x.Entry.TimeGenerated)
                                                  .ToList());

            m_stoppedLogs.AddRange(ApplicationEventLogs.Where(x => ServiceNameMatches(a_serviceName, x.Entry.Source) && (x.Entry.EventID == (int)PTEventIds.STOPPED_SUCCESSFULLY || x.Entry.EventID == (int)PTEventIds.SYSTEM_SHUTDOWN))
                                                  .OrderBy(x => x.Entry.TimeGenerated)
                                                  .ToList());

            m_startedLogs.AddRange(ApplicationEventLogs.Where(x => ServiceNameMatches(a_serviceName, x.Entry.Source) && x.Entry.EventID == (int)PTEventIds.STARTED_SUCCESSFULLY)
                                                  .OrderBy(x => x.Entry.TimeGenerated)
                                                  .ToList());

            
        }

        /// <summary>
        /// Calculates the current uptime
        /// </summary>
        private void CalculateCurrentUptime()
        {
            string systemServiceId = string.Format(ReportManager.s_eventServiceNameFormat, Instance.PublicInfo.InstanceName, Instance.PublicInfo.SoftwareVersion, "System");

            var startLog = m_startedLogs.Where(x => ServiceNameMatches(x.FullSourceName, systemServiceId)).LastOrDefault();

            if(startLog != null)
            {
                if(FindNextFailureLog(startLog) == null && FindNextStopLog(startLog) == null)
                {
                    UptimeRanges.Add(new TimeRange(startLog.Entry.TimeGenerated, DateTime.Now));
                }
            }
        }

        /// <summary>
        /// Calculates the downtime and uptime stats from successful stop/start logs
        /// </summary>
        private void CalculateDowntimeFromGracefulStop()
        {
            TimeSpan duration = TimeSpan.Zero;

            foreach (EventViewerLog stopLog in m_stoppedLogs)
            {
                var nextStart = FindNextStartLog(stopLog);
                var previousStart = FindPreviousStartLog(stopLog);

                Downtime dt = new Downtime()
                {
                    Reason = WasVersionUpgraded(stopLog.Entry.Source, nextStart.Entry.Source) ? "Service Was Updated to New Version" : "Service Restarted or Stopped.",
                    StartTime = stopLog.Entry.TimeGenerated,
                    StoppedLog = stopLog
                };

                //If the next start log was not found we can assume we are still in a downtime period so calculate the duration from DateTime.Now
                dt.Duration = nextStart == null ? DateTime.Now - dt.StartTime
                                                : nextStart.Entry.TimeGenerated - dt.StartTime;

                //Parse the uptime duration that is included in the stop log
                string uptimeDurationStr = stopLog.Entry.Message.Split(' ').Last();
                TimeSpan uptimeDuration = TimeSpan.Zero;
                if (TimeSpan.TryParse(uptimeDurationStr, out uptimeDuration))
                {
                    dt.ServiceStartTime = dt.StartTime - uptimeDuration;
                    dt.DurationSinceServiceStart = uptimeDuration;
                }
                //If we cannot parse an uptime string then we will attempt to calculate it from the previousStart log
                else
                {
                    if (previousStart != null)
                    {
                        dt.ServiceStartTime = previousStart.Entry.TimeGenerated;
                        dt.DurationSinceServiceStart = dt.StartTime - previousStart.Entry.TimeGenerated;
                    }
                    //In theory the only way we wouldn't have a previous successful start log would be if the service failed to start originally so we should check for any errors at this point
                    else
                    {
                        dt.ServiceStartTime = DateTime.MinValue;
                        dt.DurationSinceServiceStart = TimeSpan.Zero;
                    }
                }

                DowntimeRangesOffline.Add(new TimeRange(dt.StartTime, dt.StartTime + dt.Duration, dt));

                // I added a TimeRange.Overlap check here to see if this potential uptime range overlaps any downtime in order to prevent any instances of single running services being counted as uptime
                TimeRange uptimeRange = new TimeRange(dt.ServiceStartTime, dt.ServiceStartTime + dt.DurationSinceServiceStart, dt);
                if (dt.ServiceStartTime > DateTime.MinValue && (!TimeRange.Overlap(uptimeRange, DowntimeRangesOffline) && !TimeRange.Overlap(uptimeRange, DowntimeRangesMaintenance)))
                {
                    UptimeRanges.Add(uptimeRange);
                }
            }
        }

        /// <summary>
        /// Calculates the downtime and uptime stats from APS service failures. Unhandled exceptions and such.
        /// </summary>
        private void CalculateDowntimeFromFailures()
        {
            foreach(EventViewerLog failLog in m_appFailureLogs)
            {
                var nextStart = FindNextStartLog(failLog);
                var previousStart = FindPreviousStartLog(failLog);

                string failureMessage = failLog.Entry.Message.Split(new char[] { '\n' })[0];
                failureMessage = Regex.Replace(failureMessage, "service", failLog.FullSourceName);

                Downtime dt = new Downtime()
                {
                    Reason = failureMessage,
                    StartTime = failLog.Entry.TimeGenerated,
                    StoppedLog = failLog
                };

                //If the next start log was not found we can assume we are still in a downtime period so calculate the duration from DateTime.Now
                dt.Duration = nextStart == null ? DateTime.Now - dt.StartTime
                                                : nextStart.Entry.TimeGenerated - dt.StartTime;

                //Parse the uptime duration that is included in the stop log
                string uptimeDurationStr = failLog.Entry.Message.Split('\n').Last().Replace("Uptime Duration: ", "");
                TimeSpan uptimeDuration = TimeSpan.Zero;
                if (TimeSpan.TryParse(uptimeDurationStr, out uptimeDuration))
                {
                    dt.ServiceStartTime = dt.StartTime - uptimeDuration;
                    dt.DurationSinceServiceStart = uptimeDuration;
                }
                //If we cannot parse an uptime string then we will attempt to calculate it from the previousStart log
                else
                {
                    if (previousStart != null)
                    {
                        dt.ServiceStartTime = previousStart.Entry.TimeGenerated;
                        dt.DurationSinceServiceStart = failLog.Entry.TimeGenerated - previousStart.Entry.TimeGenerated;
                    }
                    //In theory the only way we wouldn't have a previous successful start log would be if the service failed to start originally so we should check for any errors at this point
                    else
                    {
                        dt.ServiceStartTime = DateTime.MinValue;
                        dt.DurationSinceServiceStart = TimeSpan.Zero;
                    }
                }

                DowntimeRangesOffline.Add(new TimeRange(dt.StartTime, dt.StartTime + dt.Duration, dt));

                TimeRange uptimeRange = new TimeRange(dt.ServiceStartTime, dt.ServiceStartTime + dt.DurationSinceServiceStart, dt);
                if (dt.ServiceStartTime > DateTime.MinValue && (!TimeRange.Overlap(uptimeRange, DowntimeRangesOffline) && !TimeRange.Overlap(uptimeRange, DowntimeRangesMaintenance)))
                {
                    UptimeRanges.Add(uptimeRange);
                }
            }
        }

        /// <summary>
        /// Calculates the downtime and uptime stats from windows events
        /// </summary>
        private void CalculateDowntimeFromWindowsEvents()
        {
            var windowsEventLogs = SystemEventLogs.Where(x => m_windowsEventIds.Keys.Contains(x.Entry.EventID))
                                                    .OrderBy( x => x.Entry.TimeGenerated)
                                                    .ToList();

            foreach(EventViewerLog windowsLog in windowsEventLogs)
            {
                var nextStart = m_startedLogs.Where(x => x.Entry.TimeGenerated > windowsLog.Entry.TimeGenerated).FirstOrDefault();
                var previousStart = m_startedLogs.Where(x => x.Entry.TimeGenerated < windowsLog.Entry.TimeGenerated).LastOrDefault();

                Downtime dt = new Downtime()
                {
                    Reason = m_windowsEventIds[windowsLog.Entry.EventID],
                    StartTime = windowsLog.Entry.TimeGenerated,
                    StoppedLog = windowsLog
                };

                //First check if the windowsLog type is an unexpected shutdown, if so either find an updated start time in the logs or flag the downtime for manual input later on. 
                if (windowsLog.Entry.EventID == 41)
                {
                    var updatedRecordLog = m_updatedLogs.Where(x => x.Entry.Message.Contains(windowsLog.Entry.TimeWritten.ToString())).LastOrDefault();

                    if (updatedRecordLog == null)
                    {
                        dt.RequiresManualInput = true;
                    }
                    else
                    {
                        DateTime timeFoundInUpdateLog = DateTime.MinValue;
                        string timeStr = updatedRecordLog.Entry.Message.Split('\n').Last().Replace("Approx. StartTime: ", "");

                        if (!DateTime.TryParse(timeStr, out timeFoundInUpdateLog))
                        {
                            throw new Exception($"Failed to parse start time of updated event log. EventId: {updatedRecordLog.Entry.EventID} @ {updatedRecordLog.Entry.TimeWritten}");
                        }

                        dt.StartTime = timeFoundInUpdateLog;

                        dt.Duration = nextStart == null ? DateTime.Now - timeFoundInUpdateLog 
                                                        : nextStart.Entry.TimeGenerated - timeFoundInUpdateLog;
                    }
                }
                else
                {
                    dt.Duration = nextStart == null ? DateTime.Now - windowsLog.Entry.TimeGenerated 
                                                    : nextStart.Entry.TimeGenerated - windowsLog.Entry.TimeGenerated;
                }
                
                //Attempt to calculate the uptime duration and starttime of the APS system before the windows event
                if (previousStart != null)
                {
                    dt.ServiceStartTime = previousStart.Entry.TimeGenerated;
                    dt.DurationSinceServiceStart = windowsLog.Entry.TimeGenerated - previousStart.Entry.TimeGenerated;
                }
                //In theory the only way we wouldn't have a previous successful start log would be if the service failed to start originally so we should check for any errors at this point
                else
                {
                    dt.ServiceStartTime = DateTime.MinValue;
                    dt.DurationSinceServiceStart = TimeSpan.Zero;
                }

                DowntimeRangesMaintenance.Add(new TimeRange(dt.StartTime, dt.StartTime + dt.Duration, dt));

                TimeRange uptimeRange = new TimeRange(dt.ServiceStartTime, dt.ServiceStartTime + dt.DurationSinceServiceStart, dt);
                if (dt.ServiceStartTime > DateTime.MinValue && (!TimeRange.Overlap(uptimeRange, DowntimeRangesOffline) && !TimeRange.Overlap(uptimeRange, DowntimeRangesMaintenance)))
                {
                    UptimeRanges.Add(uptimeRange);
                }
            }
        }

        /// <summary>
        /// Checks if service names are from the same service but with different version numbers indicating that the Instance version was upgraded.
        /// </summary>
        /// <param name="a_oldServiceName">Full service name. Example: APS Debug 0.0 System</param>
        /// <param name="a_newServiceName">Full service name. Example: APS Debug 1.0 System</param>
        /// <returns></returns>
        private bool WasVersionUpgraded(string a_oldServiceName, string a_newServiceName)
        {
            if (Helper.GetInstanceNameFromServiceName(a_oldServiceName) == Helper.GetInstanceNameFromServiceName(a_newServiceName) && 
                Helper.GetServiceNameFromServiceName(a_oldServiceName) == Helper.GetServiceNameFromServiceName(a_newServiceName) &&
                Helper.GetVersionNumberFromServiceName(a_oldServiceName) != Helper.GetVersionNumberFromServiceName(a_newServiceName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Used to check if the given service name is valid regardless of version number
        /// </summary>
        /// <param name="a_serviceName">Full service name including instance name and version number</param>
        /// <returns></returns>
        private bool ServiceNameMatches(string a_serviceNameA, string a_serviceNameB)
        {
            string instanceNameA = Helper.GetInstanceNameFromServiceName(a_serviceNameA);
            string serviceNameA = Helper.GetServiceNameFromServiceName(a_serviceNameA);
            string instanceNameB = Helper.GetInstanceNameFromServiceName(a_serviceNameB);
            string serviceNameB = Helper.GetServiceNameFromServiceName(a_serviceNameB);

            return instanceNameA == instanceNameB && serviceNameA == serviceNameB;
        }

        /// <summary>
        /// Finds the first log in the collection of m_startedLogs that comes after the input log.
        /// </summary>
        /// <param name="a_startedLog">Log used as a point of reference to find the previous start log</param>
        /// <returns></returns>
        private EventViewerLog FindNextStartLog(EventViewerLog a_lastStoppedEntry)
        {
            EventViewerLog nextStart = m_startedLogs.Where(x => x.Entry.TimeGenerated > a_lastStoppedEntry.Entry.TimeGenerated && ServiceNameMatches(x.FullSourceName, a_lastStoppedEntry.FullSourceName)).FirstOrDefault();
            return nextStart;
        }

        /// <summary>
        /// Finds the last log in the collection of m_startedLogs that comes BEFORE the input log.
        /// </summary>
        /// <param name="a_startedLog">Log used as a point of reference to find the previous start log</param>
        /// <returns></returns>
        private EventViewerLog FindPreviousStartLog(EventViewerLog a_stoppedEntry)
        {
            EventViewerLog previousStart = m_startedLogs.Where(x => x.Entry.TimeGenerated < a_stoppedEntry.Entry.TimeGenerated && ServiceNameMatches(x.FullSourceName, a_stoppedEntry.FullSourceName)).LastOrDefault();
            return previousStart;
        }

        /// <summary>
        /// Finds the first log in the collection of m_stoppedLogs that comes AFTER the input log.
        /// </summary>
        /// <param name="a_startedLog">Log used as a point of reference to find the next stop log</param>
        /// <returns></returns>
        private EventViewerLog FindNextStopLog(EventViewerLog a_startedLog)
        {
            EventViewerLog nextStop = m_stoppedLogs.Where(x => x.Entry.TimeGenerated > a_startedLog.Entry.TimeGenerated && ServiceNameMatches(x.FullSourceName, a_startedLog.FullSourceName)).FirstOrDefault();
            return nextStop;
        }

        /// <summary>
        /// Finds the first log in the collection of m_failureLogs that comes AFTER the input log.
        /// </summary>
        /// <param name="a_startedLog">Log used as a point of reference to find the next failure log</param>
        private EventViewerLog FindNextFailureLog(EventViewerLog a_startedLog)
        {
            EventViewerLog nextFailure = m_appFailureLogs.Where(x => x.Entry.TimeGenerated > a_startedLog.Entry.TimeGenerated && ServiceNameMatches(x.FullSourceName, a_startedLog.FullSourceName)).FirstOrDefault();
            return nextFailure;
        }

        /// <summary>
        /// Removes all overlapping TimeRanges in a list of TimeRanges
        /// </summary>
        /// <param name="a_timeRanges">List of TimeRange objects</param>
        private void RemoveOverlappingTimeRanges(List<TimeRange> a_timeRanges)
        {
            for (int i = 0; i < a_timeRanges.Count; i++)
            {
                for (int j = i + 1; j < a_timeRanges.Count; j++)
                {
                    if (TimeRange.Overlap(a_timeRanges[i], a_timeRanges[j]))
                    {
                        a_timeRanges[i] = TimeRange.Merge(a_timeRanges[i], a_timeRanges[j]);
                        a_timeRanges.RemoveAt(j);
                        j--;
                    }
                }
            }
        }
    }
}
