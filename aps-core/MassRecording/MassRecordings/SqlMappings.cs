using System.Data;

namespace MassRecordings;

/// <summary>
/// Maps a SQL data row into a class used by MR
/// </summary>
public class PlayerInstanceRow
{
    public PlayerInstanceRow(DataRow a_dr)
    {
        RecordingPath = a_dr["RecordingPath"].ToString();
        StartTime = Convert.ToDateTime(a_dr["StartTime"].ToString());
        ErrorFiles = Convert.ToBoolean(a_dr["ErrorFiles"].ToString());
        WarningFiles = Convert.ToBoolean(a_dr["WarningFiles"].ToString());
        if (a_dr["EndTime"] != DBNull.Value)
        {
            EndTime = Convert.ToDateTime(a_dr["EndTime"].ToString());
        }

        if (a_dr["PeakMemoryUsage"] != DBNull.Value)
        {
            MemUsage = Convert.ToInt64(a_dr["PeakMemoryUsage"].ToString());
        }

        if (a_dr["CpuTime"] != DBNull.Value)
        {
            CpuTime = new TimeSpan(Convert.ToInt64(a_dr["CpuTime"].ToString()));
        }

        if (a_dr["CpuUsage"] != DBNull.Value)
        {
            CpuUsage = Convert.ToDouble(a_dr["CpuUsage"].ToString());
        }

        if (a_dr["ExitCode"] != DBNull.Value)
        {
            ExitCode = Convert.ToInt32(a_dr["ExitCode"].ToString());
        }
    }

    public string RecordingPath;
    public DateTime StartTime;
    public DateTime EndTime = DateTime.MinValue;
    public long MemUsage;
    public TimeSpan CpuTime;
    public double CpuUsage;
    public bool ErrorFiles;
    public bool WarningFiles;
    public int ExitCode = int.MinValue;
}