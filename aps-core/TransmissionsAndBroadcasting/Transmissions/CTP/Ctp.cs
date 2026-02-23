using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions.CTP;

public class Ctp : IPTSerializable
{
    public const int UNIQUE_ID = 620;

    #region IPTSerializable Members
    public Ctp(IReader reader)
    {

        #region 12504
        if (reader.VersionNumber >= 12504)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out jobName);
            reader.Read(out description);
            reader.Read(out customer);
            reader.Read(out reserve);
            reader.Read(out cancelReservationAfter);
            reader.Read(out priority);
            reader.Read(out revenue);
            reader.Read(out hotReason);
            reader.Read(out note);
            int val;
            reader.Read(out val);
            m_schedulingType = (CtpDefs.ESchedulingType)val;
            reader.Read(out val);
            warehouseEligibility = (CtpDefs.warehouseEligibilities)val;

            int warehouseIdCount;
            reader.Read(out warehouseIdCount);
            for (int i = 0; i < warehouseIdCount; i++)
            {
                warehouses.Add(new BaseId(reader));
            }

            jobId = new BaseId(reader);

            int ctpLinesCount;
            reader.Read(out ctpLinesCount);
            for (int i = 0; i < ctpLinesCount; i++)
            {
                ctpLineList.Add(new CtpLine(reader));
            }
            reader.Read(out m_requestId);
        }
        #endregion 12000

        #region 12000
        else if (reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out jobName);
            reader.Read(out description);
            reader.Read(out customer);
            reader.Read(out reserve);
            reader.Read(out cancelReservationAfter);
            reader.Read(out priority);
            reader.Read(out revenue);
            reader.Read(out hotReason);
            reader.Read(out note);
            int val;
            reader.Read(out val);
            m_schedulingType = (CtpDefs.ESchedulingType)val;
            reader.Read(out val);
            warehouseEligibility = (CtpDefs.warehouseEligibilities)val;

            int warehouseIdCount;
            reader.Read(out warehouseIdCount);
            for (int i = 0; i < warehouseIdCount; i++)
            {
                warehouses.Add(new BaseId(reader));
            }

            jobId = new BaseId(reader);

            int ctpLinesCount;
            reader.Read(out ctpLinesCount);
            for (int i = 0; i < ctpLinesCount; i++)
            {
                ctpLineList.Add(new CtpLine(reader));
            }
        }
        #endregion 12000

        #region Version 743
        else if (reader.VersionNumber >= 743)
        {
            reader.Read(out jobName);
            reader.Read(out description);
            reader.Read(out customer);
            reader.Read(out reserve);
            reader.Read(out cancelReservationAfter);
            reader.Read(out priority);
            reader.Read(out revenue);
            reader.Read(out bool hot);
            reader.Read(out hotReason);
            reader.Read(out note);
            reader.Read(out bool canSpanPlants);
            CanSpanPlants = canSpanPlants;
            int val;
            reader.Read(out val);
            m_schedulingType = (CtpDefs.ESchedulingType)val;
            reader.Read(out val);
            warehouseEligibility = (CtpDefs.warehouseEligibilities)val;

            int warehouseIdCount;
            reader.Read(out warehouseIdCount);
            for (int i = 0; i < warehouseIdCount; i++)
            {
                warehouses.Add(new BaseId(reader));
            }

            jobId = new BaseId(reader);

            int ctpLinesCount;
            reader.Read(out ctpLinesCount);
            for (int i = 0; i < ctpLinesCount; i++)
            {
                ctpLineList.Add(new CtpLine(reader));
            }
        }
        #endregion

        else if (reader.VersionNumber >= 522)
        {
            reader.Read(out jobName);
            reader.Read(out description);
            reader.Read(out customer);
            reader.Read(out reserve);
            reader.Read(out cancelReservationAfter);
            reader.Read(out priority);
            reader.Read(out revenue);
            reader.Read(out bool hot);
            Hot = hot;
            reader.Read(out hotReason);
            reader.Read(out note);
            int val;
            reader.Read(out val);
            m_schedulingType = (CtpDefs.ESchedulingType)val;
            reader.Read(out val);
            warehouseEligibility = (CtpDefs.warehouseEligibilities)val;

            int warehouseIdCount;
            reader.Read(out warehouseIdCount);
            for (int i = 0; i < warehouseIdCount; i++)
            {
                warehouses.Add(new BaseId(reader));
            }

            jobId = new BaseId(reader);

            int ctpLinesCount;
            reader.Read(out ctpLinesCount);
            for (int i = 0; i < ctpLinesCount; i++)
            {
                ctpLineList.Add(new CtpLine(reader));
            }
        }

        #region Version 275
        else if (reader.VersionNumber >= 275)
        {
            reader.Read(out jobName);
            reader.Read(out description);
            reader.Read(out customer);
            reader.Read(out reserve);
            reader.Read(out cancelReservationAfter);
            reader.Read(out priority);
            reader.Read(out revenue);
            reader.Read(out bool hot);
            Hot = hot;
            reader.Read(out hotReason);
            reader.Read(out note);
            int val;
            reader.Read(out val);
            warehouseEligibility = (CtpDefs.warehouseEligibilities)val;

            int warehouseIdCount;
            reader.Read(out warehouseIdCount);
            for (int i = 0; i < warehouseIdCount; i++)
            {
                warehouses.Add(new BaseId(reader));
            }

            jobId = new BaseId(reader);

            int ctpLinesCount;
            reader.Read(out ctpLinesCount);
            for (int i = 0; i < ctpLinesCount; i++)
            {
                ctpLineList.Add(new CtpLine(reader));
            }
        }
        #endregion

        #region Version 1
        else
        {
            reader.Read(out jobName);
            reader.Read(out description);
            reader.Read(out customer);
            reader.Read(out reserve);
            reader.Read(out cancelReservationAfter);
            reader.Read(out priority);
            reader.Read(out revenue);
            reader.Read(out bool hot);
            Hot = hot;
            reader.Read(out hotReason);
            reader.Read(out note);
            jobId = new BaseId(reader);

            int ctpLinesCount;
            reader.Read(out ctpLinesCount);
            for (int i = 0; i < ctpLinesCount; i++)
            {
                ctpLineList.Add(new CtpLine(reader));
            }
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        m_bools.Serialize(writer);
        writer.Write(jobName);
        writer.Write(description);
        writer.Write(customer);
        writer.Write(reserve);
        writer.Write(cancelReservationAfter);
        writer.Write(priority);
        writer.Write(revenue);
        writer.Write(hotReason);
        writer.Write(note);
        writer.Write((int)m_schedulingType);
        writer.Write((int)warehouseEligibility);
        writer.Write(warehouses.Count);
        for (int i = 0; i < warehouses.Count; i++)
        {
            warehouses[i].Serialize(writer);
        }

        jobId.Serialize(writer);

        writer.Write(ctpLineList.Count);
        for (int i = 0; i < ctpLineList.Count; i++)
        {
            ctpLineList[i].Serialize(writer);
        }
        writer.Write(m_requestId);
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    public Ctp() { }

    private string jobName;

    /// <summary>
    /// Identifies the CTP.
    /// </summary>
    public string JobName
    {
        get => jobName;
        set => jobName = value;
    }

    private BaseId jobId = BaseId.NULL_ID;

    /// <summary>
    /// Id for the Job if it a ready exists.  Null if this is a CTP for a non-existant Job.
    /// </summary>
    public BaseId JobId
    {
        get => jobId;
        set => jobId = value;
    }

    private string description;

    /// <summary>
    /// Description to use as the Job Description.
    /// </summary>
    public string Description
    {
        get => description;
        set => description = value;
    }

    private string customer;

    /// <summary>
    /// Customer to use as Job CustomerExternalID.
    /// </summary>
    public string Customer
    {
        get => customer;
        set => customer = value;
    }

    private bool reserve;

    /// <summary>
    /// If true then a Job should be created in the Live Scenario.
    /// If there are Alternate Paths then the "best" Path should be chosen first using What-If Scenarios then
    /// the Job created in the Live Scenario should use that Path as the Default Path.
    /// The best path is the least cost on-time path.
    /// </summary>
    public bool Reserve
    {
        get => reserve;
        set => reserve = value;
    }

    private DateTime cancelReservationAfter;

    /// <summary>
    /// If Reserve=true then the Job should be Cancelled when the Clock passes this datetime.
    /// </summary>
    public DateTime CancelReservationAfter
    {
        get => cancelReservationAfter;
        set => cancelReservationAfter = value;
    }

    private int priority;

    /// <summary>
    /// Job Priority. Can be used by Optimization.
    /// </summary>
    public int Priority
    {
        get => priority;
        set => priority = value;
    }

    private decimal revenue;

    /// <summary>
    /// Job Revenue.  Can be used by Optimization.
    /// </summary>
    public decimal Revenue
    {
        get => revenue;
        set => revenue = value;
    }

    private BoolVector32 m_bools;

    private const short c_hotIdx = 0;
    private const short c_useUserSettingsIdx = 1;
    private const short c_spanIdx = 2;

    /// <summary>
    /// Job Hot value.  Can be used by Optimization.
    /// </summary>
    public bool Hot
    {
        get => m_bools[c_hotIdx];
        set => m_bools[c_hotIdx] = value;
    }

    public bool UseUserSettings
    {
        get => m_bools[c_useUserSettingsIdx];
        set => m_bools[c_useUserSettingsIdx] = value;
    }

    private string hotReason;

    /// <summary>
    /// Job Hot Reason.
    /// </summary>
    public string HotReason
    {
        get => hotReason;
        set => hotReason = value;
    }

    private string note;

    public string Note
    {
        get => note;
        set => note = value;
    }

    private CtpDefs.ESchedulingType m_schedulingType = CtpDefs.ESchedulingType.Optimize;

    public CtpDefs.ESchedulingType SchedulingType
    {
        get => m_schedulingType;
        set => m_schedulingType = value;
    }

    private readonly List<CtpLine> ctpLineList = new ();

    public List<CtpLine> CtpLineList => ctpLineList;

    private readonly List<BaseId> warehouses = new ();

    /// <summary>
    /// List of Warehouses to consider during CTP.
    /// </summary>
    public List<BaseId> Warehouses => warehouses;

    private CtpDefs.warehouseEligibilities warehouseEligibility = CtpDefs.warehouseEligibilities.IndividualMaterialsFromMultipleWarehouses;

    /// <summary>
    /// Controls how warehouses are used to satisfy material requirements.
    /// </summary>
    public CtpDefs.warehouseEligibilities WarehouseEligibility
    {
        get => warehouseEligibility;
        set => warehouseEligibility = value;
    }

    /// <summary>
    /// If true, then the Operations can schedule in more than one plant.  Otherwise, all operations must be scheduled in only one Plant.
    /// </summary>
    public bool CanSpanPlants
    {
        get => m_bools[c_spanIdx];
        set => m_bools[c_spanIdx] = value;
    }

    private int m_requestId;
    /// <summary>
    /// Optional identifier for the request in the Webapp's system. Used to keep track when communicating back and forth with that system.
    /// </summary>
    public int? RequestId
    {
        get => m_requestId == int.MinValue ? null : m_requestId;
        set => m_requestId = value ?? int.MinValue;
    }
}