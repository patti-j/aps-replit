using System.ComponentModel;

using PT.APSCommon.Extensions;

namespace PT.ERPTransmissions;

public partial class SalesOrderT : ERPMaintenanceTransmission<SalesOrderT.SalesOrder>, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 646;

    public SalesOrderT(IReader reader) : base(reader)
    {
        int soCount;
        reader.Read(out soCount);
        for (int i = 0; i < soCount; i++)
        {
            salesOrders.Add(new SalesOrder(reader));
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(salesOrders.Count);
        for (int i = 0; i < salesOrders.Count; i++)
        {
            salesOrders[i].Serialize(writer);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    public SalesOrderT() { }

    private readonly List<SalesOrder> salesOrders = new ();

    public List<SalesOrder> SalesOrders => salesOrders;

    public override int Count => SalesOrders.Count;

    #region Database Loading
    public void Fill(System.Data.IDbCommand soTableCmd, System.Data.IDbCommand soLineCmd, System.Data.IDbCommand soLineDistCmd)
    {
        SalesOrderTDataSet ds = new ();
        FillTable(ds.SalesOrder, soTableCmd);
        FillTable(ds.SalesOrderLine, soLineCmd);
        FillTable(ds.SalesOrderLineDist, soLineDistCmd);

        Fill(ds);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="ds"></param>
    public void Fill(SalesOrderTDataSet ds)
    {
        for (int i = 0; i < ds.SalesOrder.Count; i++)
        {
            salesOrders.Add(new SalesOrder(ds.SalesOrder[i]));
        }
    }
    #endregion Database Loading

    public override string Description => string.Format("Sales orders updated ({0})".Localize(), Count);
}