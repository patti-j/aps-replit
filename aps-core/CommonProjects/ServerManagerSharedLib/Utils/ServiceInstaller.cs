using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using PT.Common;
using PT.Common.Http;
using PT.ServerManagerSharedLib.Definitions;
using static PT.ServerManagerSharedLib.Definitions.ServiceStatus;

namespace PT.ServerManagerSharedLib.Utils
{
    public class ServiceInstaller
    {
        public static async Task<ServiceStatus> GetServiceStatus(string a_serviceAddress, string a_serviceName)
        {
            ServiceStatus serviceStatus = new ServiceStatus
            {
                LastActionTime = PTDateTime.MinDateTime,
                LastLogon = PTDateTime.MinDateTime,
                UsersOnline = 0,
                StartTime = PTDateTime.MinDateTime,
                State = EServiceState.Unknown
            };

            try
            {
                bool requestServiceStatusAPI = false;
                using (ServiceController controller = new ServiceController(a_serviceName.Replace(@"/", string.Empty)))
                {
                    controller.Refresh();
                    // Map ServiceControllerStatus values to ServiceState values
                    switch (controller.Status)
                    {
                        case ServiceControllerStatus.ContinuePending:
                        case ServiceControllerStatus.PausePending:
                        case ServiceControllerStatus.Paused:
                            serviceStatus.State = EServiceState.Unknown;
                            break;
                        case ServiceControllerStatus.StartPending:
                            serviceStatus.State = EServiceState.Starting;
                            break;
                        case ServiceControllerStatus.Running:
                            requestServiceStatusAPI = true;
                            serviceStatus.State = EServiceState.Started;
                            break;
                        case ServiceControllerStatus.StopPending:
                            serviceStatus.State = EServiceState.Stopping;
                            break;
                        case ServiceControllerStatus.Stopped:
                            serviceStatus.State = EServiceState.Stopped;
                            break;
                        default:
                            serviceStatus.State = EServiceState.Unknown;
                            break;
                    }
                }

                if (requestServiceStatusAPI)
                {
                    PTHttpClient client = new("api/SystemServerActions", a_serviceAddress, TimeSpan.FromSeconds(2), null);
                    serviceStatus = await client.MakeGetRequestAsync<ServiceStatus>("GetInstanceStatus");
                }

                return serviceStatus;
            }
            catch (Exception e)
            {
                //Service didn't response
                return serviceStatus;
            }
        }

        public static bool IsInstalled(string a_serviceName)
        {
            // This used to check for the line header "SERVICE_NAME:", but this wasn't present for servers running in spanish, but the param name is present iff it is installed.
            // The keywords outputted by sc process below ("START", "STOP", etc), however, are the same.
            // NOTE We used System.ServiceProcess.ServiceController until 2022, which seems correct, but it was purposefully removed and the reason has been lost to time. If we ever decide to change back, test cautiously.
            string o = RunSCCommand($"query \"{a_serviceName}\"");
            return TestCmdForText(o, a_serviceName);
        }

        public static bool IsRunning(string a_serviceName)
        {
            string o = RunSCCommand($"query \"{a_serviceName}\"");
            return TestCmdForText(o, "RUNNING");
        }

        public static bool IsStopped(string a_serviceName)
        {
            string o = RunSCCommand($"query \"{a_serviceName}\"");
            return TestCmdForText(o, "STOPPED");
        }

        public static void InstallService(string a_servicefileName,
            string a_serviceName,
            string a_displayName,
            bool a_automaticStart,
            string a_serviceUserName = "LocalSystem",
            string a_password = "")
        {
            string startType = a_automaticStart ? "auto" : "demand";
            //The command should look like this:
            //create "PlanetTogether Good Service 12.0.15 System" binpath="C:\ProgramData\PlanetTogetherServerManagerDev\Software\12.0.15\ProgramFiles\System\PlanetTogether System.exe" DisplayName="PlanetTogether Good Service 12.0.15 System" 
            string arguments =
                $"create \"{a_serviceName}\" binpath=\"\\\"{a_servicefileName}\\\" \\\"{a_serviceName}\\\"\"  DisplayName=\"{a_displayName}\" start={startType}";
            RunSCCommand(arguments);


            arguments = $"description \"{a_serviceName}\" \"{a_displayName}\" ";
            RunSCCommand(arguments);

            SetServiceUserForInstalledService(a_serviceName, a_serviceUserName, a_password);

            if(a_automaticStart)
                RunSCCommand($"start \"{a_serviceName}\"");
        }

        public static void SetServiceUserForInstalledService(string a_serviceName, string a_serviceUserName, string a_password)
        {
            const string c_defaultServiceUser = "LocalSystem";
            const string c_badCredentialsErrorCode = "1057:";

            if (a_serviceUserName != c_defaultServiceUser)
            {
                string arguments = $"config \"{a_serviceName}\" obj=\"{a_serviceUserName}\" password=\"{a_password}\"";
                string result = RunSCCommand(arguments);

                if (result.Contains(c_badCredentialsErrorCode)) //Bad credentials
                {
                    // Windows domain logins require a ".\" prepend; try to see if that works instead
                    a_serviceUserName = $".\\{a_serviceUserName}"; 
                    arguments = $"config \"{a_serviceName}\" obj=\"{a_serviceUserName}\" password=\"{a_password}\"";
                    RunSCCommand(arguments);
                }
            }
        }

        private static string RunSCCommand(string a_args)
        {
            return RunCommand("sc.exe", a_args);
        }

        //Search the output from the command text and return true if found
        private static bool TestCmdForText(string a_o, string a_findText)
        {
            if (!string.IsNullOrWhiteSpace(a_o) && a_o.Contains(a_findText))
            {
                return true;
            }
            else
            {
             return false;
            }
        }

        private static string RunNetCommand(string a_args)
        {
            return RunCommand("net.exe", a_args);
        }

        public static string RunCommand(string a_processName, string a_args)
        {
            Process p = new Process();
            p.StartInfo.FileName = a_processName;
            p.StartInfo.Arguments = a_args;
            p.StartInfo.Verb = "runas";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }

        public static void UninstallService(string a_serviceName)
        {
            if (!IsInstalled(a_serviceName))
            {
                return;
            }
            
            if (!IsStopped(a_serviceName))
            {
                StopService(a_serviceName);
            }

            string arguments = $"delete \"{a_serviceName}\"";
            RunSCCommand(arguments);
        }

        /// <summary>
        /// Starts the provided service.
        /// </summary>
        /// <param name="a_serviceName">The service to start.</param>
        /// <param name="port">The port the service would start on. Check this if there may be a differently named service on that port (e.g. for upgraded instances)</param>
        /// <param name="a_maxRetryAttempts">How many times to attempt the start. May avoid issues where attempting to start a service that is stopping or starting.</param>
        /// <returns></returns>
        public static bool StartService(string a_serviceName, int? port = null, int a_maxRetryAttempts = 1)
        {
            // Check if the port is occupied
            if(port.HasValue)
                if (IsPortOccupied(port.Value))
                {
                    ErrorDescription = $"Port {port.Value} is already occupied.";
                    return false;
                }


            if (!IsInstalled(a_serviceName))
            {
                ErrorDescription = $"Failed to start the service: service not registering as installed.";
                return false;
            }

            if (IsRunning(a_serviceName))
            {
                return true;
            }

            string result = RunSCCommand($"start \"{a_serviceName}\"");

            if (a_serviceName == Utils.c_ServerAgentServiceName)
            {
                RunSCCommand($"config \"{a_serviceName}\" start=auto");
            }

            List<string> acceptableStartErrorCodes = new List<string>()
            {
                "1056" // An instance of the service is already running.
            };

            if (result.Contains("RUNNING") ||
                result.Contains("START_PENDING") ||
                IsErrorAcceptable(acceptableStartErrorCodes, result))
            {
                return true;
            }
            else
            {
                if (a_maxRetryAttempts > 0)
                {
                    Thread.Sleep(1000);
                    StopService(a_serviceName, a_maxRetryAttempts - 1);
                    ErrorDescription = result.Trim();
                    return false;
                }
                else
                {
                    ErrorDescription = result.Trim();
                    return false;
                }
            }
        }
        public static bool IsPortOccupied(int port)
        {
            try
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] activeListeners = ipGlobalProperties.GetActiveTcpListeners();

                foreach (IPEndPoint listener in activeListeners)
                {
                    if (listener.Port == port)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                // Handle any exceptions
                return false;
            }
        }


        public static string ErrorDescription = "";

        public static bool StopService(string a_serviceName, int a_maxRetryAttempts = 1)
        {
            if (!IsInstalled(a_serviceName))
            {
                return false;
            }

            if (IsStopped(a_serviceName))
            {
                return true;
            }
            
            string result = RunSCCommand($"stop \"{a_serviceName}\"");

            List<string> acceptableStopErrorCodes = new List<string>()
            {
                "1062" // "The service has not been started."
            };

            if (result.Contains("STOPPED") || 
                result.Contains("STOP_PENDING") || 
                IsErrorAcceptable(acceptableStopErrorCodes, result))
            {
                return true;
            }
            else
            {
                if (a_maxRetryAttempts > 0)
                {
                    Thread.Sleep(3000);
                    return StopService(a_serviceName, a_maxRetryAttempts - 1);
                }
                else
                {
                    throw new Exception(result.Trim());
                }
            }
        }

        /// <summary>
        /// Checks sc.exe output to see if it has a line like: "[SC] StartService:" ending with a particular error code.
        /// This is a bit roundabout, but came from an issue where expected words in this output aren't guaranteed to be there when installed on non-english computers.
        /// It does seem as though the process names, and keyed outputs (e.g. "START" "STOP", but not "FAILED") remain the same across languages, so that is what we should use.
        /// </summary>
        /// <param name="acceptableErrorCodes"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static bool IsErrorAcceptable(List<string> acceptableErrorCodes, string result)
        {
            // In English, error outputs look like eg "[SC] StartService: OpenService FAILED: {code}", in Spanish "[SC] StartService: OpenService ERROR: {code}".
            // So, we can't trust the capitalized word here, but the general structure of a line starting with [SC] and ending with the error code is more resilient.
            return acceptableErrorCodes.Any(code =>
                new Regex(@"\[SC\].*" + code).IsMatch(result));
        }
    }
}