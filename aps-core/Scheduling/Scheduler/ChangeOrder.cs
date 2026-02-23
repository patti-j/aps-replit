using PT.APSCommon;

namespace PT.Scheduler;

public class ChangeOrder : BaseIdObject
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 460;

    public ChangeOrder(IReader reader, Job job)
        : base(reader)
    {
        if (reader.VersionNumber >= 80)
        {
            reader.Read(out closed);
            reader.Read(out createDate);
            reader.Read(out plannerRemarks);
            reader.Read(out requesterRemarks);
            reader.Read(out applied);

            int val;
            reader.Read(out val); //ChangeOrderDefs.statuses
            reader.Read(out val); //ChangeOrderDefs.priorities

            planner = new BaseId(reader);
            requester = new BaseId(reader);

            //NeedDateChange
            bool haveNeedDateChange;
            reader.Read(out haveNeedDateChange);
            if (haveNeedDateChange)
            {
                new NeedDateChange(reader);
            }

            //Read in QtyChanges
            int qtyChangeCount;
            reader.Read(out qtyChangeCount);
            for (int i = 0; i < qtyChangeCount; i++)
            {
                QtyChange newChange = new (reader, job);
            }

            //Read in BreakOffs
            int breakoffCount;
            reader.Read(out breakoffCount);
            for (int i = 0; i < breakoffCount; i++)
            {
                BreakOff newChange = new (reader, job);
            }
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Properties
    private DateTime createDate;
    private BaseId requester;
    private bool applied;
    #endregion

    #region Shared Properties
    private BaseId planner = BaseId.NULL_ID;
    private string requesterRemarks;
    private string plannerRemarks;
    private bool closed;
    #endregion

    public class NeedDateChange
    {
        public const int UNIQUE_ID = 461;

        #region IPTSerializable Members
        public NeedDateChange(IReader reader)
        {
            if (reader.VersionNumber >= 40)
            {
                reader.Read(out needAsap);
                reader.Read(out newNeedDate);
                reader.Read(out newNeedDateSet);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        private DateTime newNeedDate = PTDateTime.MinDateTime;
        private bool newNeedDateSet;
        private bool needAsap;
    }

    public class QtyChange
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 462;

        public QtyChange(IReader reader, Job job)
        {
            if (reader.VersionNumber >= 40)
            {
                reader.Read(out newQty);
                BaseId moId = new (reader);
                moToChange = job.ManufacturingOrders.GetById(moId);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        private ManufacturingOrder moToChange;
        private decimal newQty;
    }

    public class BreakOff
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 463;

        public BreakOff(IReader reader, Job job)
        {
            if (reader.VersionNumber >= 40)
            {
                reader.Read(out breakOffNeedDate);
                reader.Read(out breakOffQty);

                BaseId moId = new (reader);
                moToBreakOff = job.ManufacturingOrders.GetById(moId);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        private ManufacturingOrder moToBreakOff;
        private decimal breakOffQty;
        private DateTime breakOffNeedDate = PTDateTime.MinDateTime;
    }
}