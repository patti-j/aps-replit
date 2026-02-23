using PT.SchedulerDefinitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.SystemServiceReporter
{
    public class Helper
    {
        public static BroadcasterConstructorValues GetConstructorValues(ServerManagerProxyLib.ServerManagerService.APSInstance a_instance, ServerManagerProxyLib.ServerManagerService.ServerManagerServiceV1Client a_proxy, string a_serviceName)
        {
            BroadcasterConstructorValues newValues = new BroadcasterConstructorValues();
            newValues.AbsorbCustomizations = a_instance.Settings.SystemServiceSettings.AbsorbCustomizations;
            newValues.MaxNbrSessionRecordingsToStore = a_instance.Settings.SystemServiceSettings.MaxNbrSessionRecordingsToStore;
            newValues.MaxNbrSystemBackupsToStorePerSession = a_instance.Settings.SystemServiceSettings.MaxNbrSystemBackupsToStorePerSession;
            newValues.MinutesBetweenSystemBackups = a_instance.Settings.SystemServiceSettings.MinutesBetweenSystemBackups;
            newValues.NonSequencedTransmissionPlayback = a_instance.Settings.SystemServiceSettings.NonSequencedTransmissionPlayback;
            newValues.Port = a_instance.Settings.SystemServiceSettings.Port;
            newValues.Record = a_instance.Settings.SystemServiceSettings.RecordSystem;
            newValues.SingleThreaded = a_instance.Settings.SystemServiceSettings.SingleThreadedTransmissionProcessing;
            newValues.StartingScenarioNumber = a_instance.Settings.SystemServiceSettings.StartingScenarioNumber;
            newValues.StartType = ((StartTypeEnum)Enum.Parse(typeof(ServerManagerProxyLib.ServerManager.StartTypeEnum), a_instance.Settings.SystemServiceSettings.SystemStartType.ToString()));
            newValues.RecordingDirectoryToLoadFromAtStartup = a_instance.Settings.SystemServiceSettings.RecordingsFolderToLoadFrom;
            newValues.WorkingDirectory = a_instance.ServicePaths.SystemServiceWorkingDirectory;
            newValues.ExtraServicesContactIntervalMilliseconds = a_instance.Settings.ExtraServicesSettings.ContactIntervalMilliseconds;
            newValues.SystemServiceUrl = a_proxy.GetSystemServiceURL(a_serviceName);
            newValues.InterfaceServiceUrl = a_proxy.GetInterfaceServiceURL(a_serviceName);
            newValues.ClientTimeoutSeconds = a_instance.Settings.SystemServiceSettings.ClientTimeoutSeconds;
            newValues.InterfaceTimeoutSeconds = a_instance.Settings.SystemServiceSettings.InterfaceTimeoutSeconds;
            newValues.ServiceName = a_serviceName;
            newValues.InstanceName = a_instance.PublicInfo.InstanceName;
            newValues.SoftwareVersion = a_instance.PublicInfo.SoftwareVersion;
            newValues.LogFolder = a_instance.Settings.SystemServiceSettings.LogFolder;
            newValues.KeyFolder = a_instance.Settings.SystemServiceSettings.KeyFolder;
            newValues.LogDbConnectionString = a_instance.Settings.SystemServiceSettings.LogDbConnectionString;

            return newValues;
        }

        public static string GetSystemMessage(int a_errorCode)
        {
            var exception = new Win32Exception(a_errorCode);
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

            if(splitServiceName.Count < 4)
            {
                return string.Empty;
            }

            splitServiceName.RemoveRange(0, 3);

            return string.Join(" ", splitServiceName);
        }
    }
}
