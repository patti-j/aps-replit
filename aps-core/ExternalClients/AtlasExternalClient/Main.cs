using System;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using AtlasExternalClient.ApsWebServiceRef;

using PT.Common.Extensions;

using PT.ServerManagerProxyLib.ServerManagerService;

namespace AtlasExternalClient
{
    class Program
    {
        private static bool s_trace;
        private static bool s_log;
        private static string s_logPath;

        static void Main(string[] args)
        {
            CommandLineArguments parameters;
            try
            {
                parameters = new CommandLineArguments(args);
                s_trace = parameters.Trace.ArgumentFound;
                s_log = parameters.Log.ArgumentFound;

                if (s_log)
                {
                    s_logPath = Path.Combine(Environment.CurrentDirectory, "log.txt");
                    if (!string.IsNullOrWhiteSpace(parameters.Log.Value))
                    {
                        s_logPath = parameters.Log.Value;
                    }
                    Trace($"APS Web Services Client Log: {DateTime.Now}");
                    Trace($"Arguments: {parameters.CreateArgumentString()}");
                }
            }
            catch (Exception e)
            {
                LogError(e);
                Environment.Exit(-1);
                return;
            }

            const string c_smBaseUrl = "http://{0}:{1}";
            const string c_baseUrl = "http://{0}:{1}/APSWebService";

            string smPort = "7992";
            if (parameters.ServerPort.ArgumentFound)
            {
                smPort = parameters.ServerPort.Value;
            }
            string smUrl = String.Format(c_smBaseUrl, parameters.ServerAddress.Value, smPort);

            ServerManagerServiceV1Client smProxy;
            try
            {
                //TODO: The port on the next line will not necessarily be 7992. FIX
                Trace($"Connecting to ServerManager: {smUrl}");
                smProxy = PT.ServerManagerProxyLib.ServerManagerServiceProxy.CreateServerManagerServiceProxy();
                Trace($"Connected");
            }
            catch (Exception e)
            {
                LogError(e);
                ExitApp(1);
                return;
            }

            APSInstance instance;
            try
            {
                Trace($"Collecting Instance Information: '{parameters.Instance.Value}' '{parameters.SoftwareVersion.Value}'");
                instance = smProxy.GetInstance(parameters.Instance.Value, parameters.SoftwareVersion.Value);
            }
            catch (Exception e)
            {
                LogError(e);
                ExitApp(2);
                return;
            }
            
            Trace($"Collecting Extra Service settings");
            bool acceptWebTransmissions = instance.Settings.ApiSettings.Enabled;
            Trace($"Extra Services is accepting web transmissions: {acceptWebTransmissions}");
            if (!acceptWebTransmissions)
            {
                Trace($"Exiting");
                LogError(new Exception("Extra Services Accept Web Transmission is disabled. Enable from Instance Manager under the Data Publish tab."));
                ExitApp(3);
                return;
            }

            int hostPort = instance.Settings.ApiSettings.Port;
            Trace($"Web Service port configured to: {hostPort}");
            
            string extraServicesUrl = string.Format(c_baseUrl, parameters.ServerAddress.Value, hostPort);

            //Soap ApsWebServiceRef
            Trace($"Connecting to Extra Services: '{extraServicesUrl}'");
            ExtraServicesWebServiceClient client;

            TimeSpan timeout = TimeSpan.FromMinutes(.5);
            if (parameters.TimeoutMinutes.ArgumentFound && !string.IsNullOrWhiteSpace(parameters.TimeoutMinutes.Value))
            {
                double timoutMins = double.Parse(parameters.TimeoutMinutes.Value);
                timeout = TimeSpan.FromMinutes(timoutMins);
            }

            try
            {
                //Setup the binding. This is done in code so the config file is not needed
                BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
                binding.BypassProxyOnLocal = true;
                binding.CloseTimeout = new TimeSpan(0, 2, 0);
                binding.OpenTimeout = new TimeSpan(0, 0, 30);
                binding.SendTimeout = timeout;
                binding.AllowCookies = false;
                binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
                binding.MaxBufferPoolSize = 524288;
                binding.MaxBufferSize = int.MaxValue;
                binding.MaxReceivedMessageSize = int.MaxValue;
                binding.TextEncoding = Encoding.UTF8;
                binding.TransferMode = TransferMode.Buffered;
                binding.MessageEncoding = WSMessageEncoding.Text;

                client = new ExtraServicesWebServiceClient(binding, new EndpointAddress(extraServicesUrl));
                Trace($"Connected");
                Trace($"Connection Open Time Out: {binding.OpenTimeout}");
                Trace($"Connection Send Time Out: {binding.SendTimeout}");
            }
            catch (Exception e)
            {
                LogError(e);
                ExitApp(4);
                return;
            }

            //Soap ApsWebServiceRef
            string password = string.Empty;
            if(parameters.Password.ArgumentFound && !string.IsNullOrWhiteSpace(parameters.Password.Value))
            {
                password = parameters.Password.Value;
            }

            //Note; use a timeout of 0 to not wait for the result.

            //Create a typed request
            Trace($"Creating Import Request");
            ImportRequest importRequest = new ImportRequest
            {
                UserName = parameters.UserName.Value, Password = password, TimeoutDuration = timeout,
            };

            //Create a typed request
            OptimizeRequest optimizeRequest = new OptimizeRequest
            {
                UserName = parameters.UserName.Value, Password = password, MRP = true, TimeoutDuration = timeout,
            };

            //Create a typed request
            PublishRequest publishRequest = new PublishRequest
            {
                UserName = parameters.UserName.Value, Password = password, TimeoutDuration = timeout, ScenarioId = long.MinValue
            };

            int exitCode = 0;

            DateTime start = DateTime.Now;
            Trace("");
            Trace(importRequest);
            Trace($"Sending Import request at '{start}'");
            //Sending a request requires the username and password of an APS user with appropriate permissions, generally an admin.
            Task<ApsWebServiceResponseBase> importTask = client.ImportAsync(importRequest);
            ApsWebServiceResponseBase importResponse = importTask.Result;
            Trace($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(importResponse);
            if (importResponse.Exception)
            {
                exitCode = exitCode | 1;
            }

            start = DateTime.Now;
            Trace("");
            Trace(optimizeRequest);
            Trace($"Sending Optimize request at '{start}'");
            Task<ApsWebServiceResponseBase> optimizeTask = client.OptimizeAsync(optimizeRequest);
            ApsWebServiceResponseBase optimizeResponse = optimizeTask.Result;
            Trace($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(optimizeResponse);
            if (importResponse.Exception)
            {
                exitCode = exitCode | 2;
            }

            start = DateTime.Now;
            Trace("");
            Trace(publishRequest);
            Trace($"Sending Publish request at '{start}'");
            Task<ApsWebServiceResponseBase> publishTask = client.PublishAsync(publishRequest);
            ApsWebServiceResponseBase publishResponse = publishTask.Result;
            Trace($"Response received after '{DateTime.Now.Subtract(start)}'");
            Write(publishResponse);
            if (importResponse.Exception)
            {
                exitCode = exitCode | 4;
            }
            
            ExitApp(exitCode);
        }

        private static void ExitApp(int a_exitCode)
        {
            Trace($"Exiting with code: {a_exitCode}");

            if (s_trace && !s_log)
            {
                //Writing only to console, keep open so the user can read the result
                Trace("Trace Complete. Press any key to exit.");
                Console.ReadKey();
            }
        }

        private static void Write(ApsWebServiceResponseBase a_response)
        {
            Trace($"Operation code: {a_response.ResponseCode}");
            if (!a_response.Exception)
            {
                Trace($"Completed with Zero Errors");
            }
            else
            {
                Trace($"Completed with Errors");
                Trace(a_response.FullExceptionText);
            }
        }

        private static void LogError(Exception a_e)
        {
            string fullMessage = a_e.GetExceptionFullMessage();
            Trace(fullMessage);
        }

        private static void Trace(ImportRequest a_request)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Import Request:");
            sb.AppendLine($"Waiting for result, timeout: {a_request.TimeoutDuration}");

            Trace(sb.ToString());
        }

        private static void Trace(OptimizeRequest a_request)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Optimize Request:");
            sb.AppendLine($"Run MRP: {a_request.MRP}");
            sb.AppendLine($"Optimize Scenario ID: {a_request.ScenarioId}");
            sb.AppendLine($"Waiting for result, timeout: {a_request.TimeoutDuration}");

            Trace(sb.ToString());
        }

        private static void Trace(PublishRequest a_request)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Publish Request:");
            sb.AppendLine($"Publish Type: {a_request.PublishType}");
            if (a_request.ScenarioId != long.MinValue)
            {
                sb.AppendLine($"Scenario Id: {a_request.ScenarioId}");
            }
            else
            {
                sb.AppendLine("Live Scenario");
            }
            sb.AppendLine($"Waiting for result, timeout: {a_request.TimeoutDuration}");

            Trace(sb.ToString());
        }

        private static void Trace(string a_message)
        {
            if (s_trace)
            {
                Console.WriteLine(a_message);
            }

            if (s_log)
            {
                File.AppendAllText(s_logPath, a_message + Environment.NewLine);
            }
        }
    }
}
