using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using PT.SchedulerDefinitions;
using PT.Common;
using PT.Common.SqlServer;
using ServerManagerProxyLib.ServerManagerService;
using ServerManagerProxyLib.ServerManagerManagement;

using ServerManagerSettings = ServerManagerProxyLib.ServerManagerManagement.ServerManagerSettings;
using APSInstance = ServerManagerProxyLib.ServerManagerManagement.APSInstance;
using ServerManagerManagementV1Client = ServerManagerProxyLib.ServerManagerManagement.ServerManagerManagementV1Client;

namespace PT.SystemServiceReporter
{
    public class ReportManager
    {
        private string m_connectionString = null;
        private DatabaseConnections m_db = null;
        private List<APSInstance> m_instances = new List<APSInstance>();
        private List<string> m_serviceNames = new List<string>();
        private string m_serviceName = "APS Debug 0.0 System";


        private const string c_smService = "http://localhost:7992";
        private const string c_managementAddress = "http://localhost:7993";
        public static string s_eventServiceNameFormat = "APS {0} {1} {2}";

        private ServerManagerManagementV1Client m_managementProxy = null;
        private ServerManagerProxyLib.ServerManagerService.ServerManagerServiceV1Client m_proxy = null;
        private ServerManagerProxyLib.ServerManagerService.APSInstance m_instance = null;


        public List<DatabaseLog> DbLogs { get; set; }
        public List<EventViewerLog> SystemEventLogs { get; set; }
        public List<EventViewerLog> ApplicationEventLogs { get; set; }
        public List<InstanceReport> Reports { get; set; }

        public ReportManager()
        {
            DbLogs = new List<DatabaseLog>();
            SystemEventLogs = new List<EventViewerLog>();
            ApplicationEventLogs = new List<EventViewerLog>();
            Reports = new List<InstanceReport>();

            m_proxy = ServerManagerProxyLib.ServerManagerServiceProxy.CreateServerManagerServiceProxy(c_smService);
            m_instance = m_proxy.GetInstanceByServiceName(m_serviceName);
            m_managementProxy = ServerManagerProxyLib.ServerManagerManagementProxy.CreateServerManagerServiceProxy(c_managementAddress);
            BroadcasterConstructorValues constructorValues = Helper.GetConstructorValues(m_instance, m_proxy, m_serviceName);

            m_connectionString = constructorValues.LogDbConnectionString;
            m_db = new DatabaseConnections(m_connectionString);

            try
            {
                ServerManagerSettings smSettings = m_managementProxy.GetSettings();
                m_instances = smSettings.APSInstances.Values.ToList();
            }
            catch (Exception err)
            {
                throw err;
            }

            m_instances = m_instances.OrderBy(a_i => a_i.PublicInfo.InstanceName).ThenBy(a_i => a_i.PublicInfo.SoftwareVersion).ToList();
        }

        public void GenerateReportsData()
        {
            //Get a full list of all services to gather data for.
            Reports.Clear();
            foreach (APSInstance instance in m_instances)
            {
                InstanceReport report = new InstanceReport()
                {
                    Instance = instance
                };

                foreach (string service in InstanceReport.s_services)
                {
                    string tmpServiceName = string.Format(s_eventServiceNameFormat, instance.PublicInfo.InstanceName, instance.PublicInfo.SoftwareVersion, service);
                    report.FullServiceNames.Add(tmpServiceName);
                }

                report.Generate();
                Reports.Add(report);
            }
        }
    }
}
