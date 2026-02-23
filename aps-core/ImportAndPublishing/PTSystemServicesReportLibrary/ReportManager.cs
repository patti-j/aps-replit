using System.Diagnostics;

using PT.Common.Sql.SqlServer;
using PT.ServerManagerAPIProxy.APIClients;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.SystemServicesReportLibrary;

public class ReportManager
{
    private string m_connectionString = null;
    private DatabaseConnections m_db = null;
    private readonly List<InstancePublicInfo> m_instances = new ();
    private List<string> m_serviceNames = new ();
    private string m_serviceName = "APS Debug 0.0 System";

    private const string c_smService = "http://localhost:7992";
    private const string c_managementAddress = "http://localhost:7993";
    public static string s_eventServiceNameFormat = "APS {0} {1} {2}";

    private InstanceActionsClient m_proxy = null;
    private Instance m_instance = null;

    public EventLog ApplicationEventLog { get; set; }
    public EventLog SystemEventLog { get; set; }
    public List<EventViewerLog> SystemEventViewerLogs { get; set; }
    public List<EventViewerLog> ApplicationEventViewerLogs { get; set; }
    public List<Downtime> PreviousCrashes { get; set; }
    public List<InstanceReport> Reports { get; set; }

    public DateTime ReportStart;
    public DateTime ReportEnd;

    public ReportManager(InstancePublicInfo[] a_instanceArray)
    {
        ApplicationEventLog = new EventLog("Application");
        SystemEventLog = new EventLog("System");
        SystemEventViewerLogs = new List<EventViewerLog>();
        ApplicationEventViewerLogs = new List<EventViewerLog>();
        Reports = new List<InstanceReport>();
        PreviousCrashes = new List<Downtime>();

        m_instances = new List<InstancePublicInfo>(a_instanceArray).OrderBy(a_i => a_i.InstanceName).ThenBy(a_i => a_i.SoftwareVersion).ToList();
    }

    public void GenerateReportsData(DateTime a_reportStart, DateTime a_reportEnd)
    {
        ReportStart = a_reportStart;
        ReportEnd = a_reportEnd;

        //Get a full list of all services to gather data for.
        Reports.Clear();
        ApplicationEventViewerLogs.Clear();
        SystemEventViewerLogs.Clear();
        PreviousCrashes.Clear();
        foreach (InstancePublicInfo publicInfo in m_instances)
        {
            InstanceReport report = new (this, ReportStart, ReportEnd);
            report.InstancePublicInfo = publicInfo;

            foreach (string service in InstanceReport.s_services)
            {
                string tmpServiceName = string.Format(s_eventServiceNameFormat, publicInfo.InstanceName, publicInfo.SoftwareVersion, service);
                report.FullServiceNames.Add(tmpServiceName);
            }

            report.Generate();
            Reports.Add(report);
        }
    }
}