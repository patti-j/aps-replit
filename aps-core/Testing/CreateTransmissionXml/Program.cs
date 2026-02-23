using System.Text;
using System.Xml;
using System.Xml.Serialization;

using PT.ERPTransmissions;

namespace ToXmlTest;

internal class Program
{
    private static void Main(string[] args)
    {
        SerializeTransmissionObject<PlantT, PlantT.Plant>();
        SerializeTransmissionObject<ForecastT, ForecastT.Forecast>();
        SerializeTransmissionObject<SalesOrderT, SalesOrderT.SalesOrder>();
        SerializeObject<SalesOrderT.SalesOrder.SalesOrderLine>();
        SerializeObject<SalesOrderT.SalesOrder.SalesOrderLine.SalesOrderLineDistribution>();
        SerializeTransmissionObject<TransferOrderT, TransferOrderT.TransferOrder>();
        SerializeObject<TransferOrderT.TransferOrder.TransferOrderDistribution>();
        SerializeTransmissionObject<CapabilityT, CapabilityT.Capability>();
        SerializeTransmissionObject<CapacityIntervalT, CapacityIntervalT.CapacityIntervalDef>();
        SerializeTransmissionObject<CellT, CellT.Cell>();
        SerializeTransmissionObject<DepartmentT, DepartmentT.Department>();
        SerializeTransmissionObject<ItemT, WarehouseT.Item>();
        SerializeTransmissionObject<JobT, JobT.Job>();
        SerializeObject<JobT.ManufacturingOrder>();
        SerializeObject<JobT.Product>();
        SerializeObject<JobT.MaterialRequirement>();
        SerializeObject<JobT.AlternatePath>();
        SerializeObject<JobT.ResourceOperation>();
        SerializeTransmissionObject<ProductRulesT, ProductRulesT.ProductRule>();
        SerializeTransmissionObject<PurchaseToStockT, PurchaseToStockT.PurchaseToStock>();
        SerializeTransmissionObject<RecurringCapacityIntervalT, RecurringCapacityIntervalT.RecurringCapacityIntervalDef>();
        SerializeTransmissionObject<ResourceT, ResourceT.Resource>();
        SerializeTransmissionObject<UserT, UserT.User>();
        SerializeTransmissionObject<VesselTypeT, VesselTypeT.VesselType>();
        SerializeTransmissionObject<WarehouseT, WarehouseT.Warehouse>();
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    private static void SerializeTransmissionObject<T, U>()
        where T : ERPMaintenanceTransmission<U>, new()
        where U : new()
    {
        Console.WriteLine("Creating " + typeof(U).Name + ".xml.");
        T transmission = new ();
        U transObj = new ();
        transmission.Add(transObj);
        XmlSerializer xmlSer = new (typeof(T));
        XmlTextWriter writer = new (typeof(U).Name + ".xml", Encoding.Unicode);
        xmlSer.Serialize(writer, transmission);
        writer.Close();
    }

    private static void SerializeObject<U>() where U : new()
    {
        Console.WriteLine("Creating " + typeof(U).Name + ".xml.");
        U obj = new ();
        XmlSerializer xmlSer = new (typeof(U));
        XmlTextWriter writer = new (typeof(U).Name + ".xml", Encoding.Unicode);
        xmlSer.Serialize(writer, obj);
        writer.Close();
    }
}