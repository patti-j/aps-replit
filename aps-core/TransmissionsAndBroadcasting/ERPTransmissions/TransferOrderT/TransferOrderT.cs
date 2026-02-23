using System.ComponentModel;

using PT.SchedulerDefinitions;

namespace PT.ERPTransmissions;

public partial class TransferOrderT : ERPMaintenanceTransmission<TransferOrderT.TransferOrder>, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 651;

    public TransferOrderT(IReader reader) : base(reader)
    {
        int toCount;
        reader.Read(out toCount);
        for (int i = 0; i < toCount; i++)
        {
            transferOrders.Add(new TransferOrder(reader));
        }
    }

    public TransferOrderT(IReader reader, object A_PREVIOUS_VERSION_AT_DEIDRICH_DIDNT_HAVE_CONSTRUCTOR_BASE_CALL)
    {
        int toCount;
        reader.Read(out toCount);
        for (int i = 0; i < toCount; i++)
        {
            transferOrders.Add(new TransferOrder(reader));
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(transferOrders.Count);
        for (int i = 0; i < transferOrders.Count; i++)
        {
            transferOrders[i].Serialize(writer);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion IPTSerializable

    public TransferOrderT() { }

    private readonly List<TransferOrder> transferOrders = new ();

    public List<TransferOrder> TransferOrders => transferOrders;

    public override int Count => TransferOrders.Count;

    #region Database Loading
    public void Fill(System.Data.IDbCommand toTableCmd, System.Data.IDbCommand toDistCmd, JobDefs.EMaintenanceMethod maintenanceMethod)
    {
        TransferOrderTDataSet ds = new ();
        FillTable(ds.TransferOrder, toTableCmd);
        FillTable(ds.TransferOrderDistribution, toDistCmd);

        Fill(ds, maintenanceMethod);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="ds"></param>
    public void Fill(TransferOrderTDataSet ds, JobDefs.EMaintenanceMethod maintenanceMethod)
    {
        HashSet<string> externalIds = new ();

        for (int i = 0; i < ds.TransferOrder.Count; i++)
        {
            TransferOrder to = new (ds.TransferOrder[i], maintenanceMethod);
            to.Validate();
            if (externalIds.Contains(to.ExternalId))
            {
                throw new APSCommon.PTValidationException("2879", new object[] { to.ExternalId });
            }

            transferOrders.Add(to);
            externalIds.Add(to.ExternalId);
        }
    }
    #endregion Database Loading
}