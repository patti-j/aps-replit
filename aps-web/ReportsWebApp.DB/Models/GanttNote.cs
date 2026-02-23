using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Hashing;
using System.Text;

namespace ReportsWebApp.DB.Models;

public class GanttNote
{
    public int Id { get; set; }
    [ForeignKey(nameof(Company))]
    public int CompanyId { get; set; }
    public virtual Company Company { get; set; }
    public string NewScenarioId { get; set; }
    public ulong BlockHash { get; set; }
    public string Text { get; set; }

    public static ulong GetHashForEvent(Event ev)
    {
        byte[] data = Encoding.UTF8.GetBytes(ev.ResourceName + ev.Name + ev.MoName + ev.Opname + ev.ActivityExternalId);
        var hash = Crc64.Hash(data);
        return BitConverter.ToUInt64(hash, 0);
    }
}