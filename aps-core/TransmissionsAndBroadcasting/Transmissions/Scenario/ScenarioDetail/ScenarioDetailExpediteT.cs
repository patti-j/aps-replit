using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;

namespace PT.Transmissions;

public abstract class ScenarioDetailExpediteBaseT : ScenarioIdBaseT, IPTSerializable
{
    #region Serialization
    /// <summary>
    /// Base class for expedites
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="a_v1HasAllFields">
    /// Kludge to handle both child classes. Whether the child classes version 1 had the date, anchor, and lock fields. The first class created didn't (for jobs), the second
    /// class did (for MOs).
    /// </param>
    public ScenarioDetailExpediteBaseT(IReader reader, bool a_v1HasAllFields) :
        base(reader)
    {
        if (reader.VersionNumber >= 12509)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out m_date);
            reader.Read(out int tmpInt);
            m_startDateType = (ExpediteStartDateType)tmpInt;
            reader.Read(out m_alternatePathExternalId);
        }
        else if(reader.VersionNumber >= 12500)
        {
            reader.Read(out m_date);
            reader.Read(out bool anchor);
            Anchor = anchor;

            reader.Read(out bool lockToResources);
            LockToResources = lockToResources;

            reader.Read(out int tmpInt);
            m_startDateType = (ExpediteStartDateType)tmpInt;
            reader.Read(out m_alternatePathExternalId);
        }
        else if(reader.VersionNumber >= 12425)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out m_date);
            reader.Read(out int tmpInt);
            m_startDateType = (ExpediteStartDateType)tmpInt;
            reader.Read(out m_alternatePathExternalId);
        }
        else
        {
            bool anchor = false;
            bool lockToResources = false;

            if (reader.VersionNumber >= 12046)
            {
                reader.Read(out m_date);
                reader.Read(out anchor);
                reader.Read(out lockToResources);
                reader.Read(out int tmpInt);
                m_startDateType = (ExpediteStartDateType)tmpInt;
                reader.Read(out m_alternatePathExternalId);
            }
            else if (reader.VersionNumber >= 12000)
            {
                reader.Read(out m_date);
                reader.Read(out anchor);
                reader.Read(out lockToResources);
                reader.Read(out int tmpInt);
                m_startDateType = (ExpediteStartDateType)tmpInt;
            }
            else if (reader.VersionNumber >= 753)
            {
                reader.Read(out m_date);
                reader.Read(out anchor);
                reader.Read(out lockToResources);
                reader.Read(out int tmpInt);
                m_startDateType = (ExpediteStartDateType)tmpInt;
                reader.Read(out m_alternatePathExternalId);
            }
            else if (reader.VersionNumber >= 447)
            {
                reader.Read(out m_date);
                reader.Read(out anchor);
                reader.Read(out lockToResources);
                reader.Read(out int tmpInt);
                m_startDateType = (ExpediteStartDateType)tmpInt;
            }

            //Set new BoolVector values from older serialization versions
            Anchor = anchor;
            LockToResources = lockToResources;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_bools.Serialize(writer);
        writer.Write(m_date);
        writer.Write((int)m_startDateType);
        writer.Write(m_alternatePathExternalId);
    }

    public const int UNIQUE_ID = 765;

    int IPTSerializable.UniqueId => UNIQUE_ID;
    #endregion

    public enum ExpediteStartDateType { Clock, EndOfFrozenSpan, SpecificDateTime, EndOfStableSpan }

    protected ScenarioDetailExpediteBaseT() { }

    protected ScenarioDetailExpediteBaseT(BaseId a_scenarioId, ExpediteStartDateType a_expediteStartType, long a_expediteTicks, bool a_anchorAfterExpedite, bool a_lockToResourceAfterExpedite, bool a_manuallyTriggered) :
        base(a_scenarioId)
    {
        StartDateType = a_expediteStartType;

        if (StartDateType != ExpediteStartDateType.SpecificDateTime)
        {
            Date = PTDateTime.MinDateTime.Ticks;
        }
        else
        {
            Date = a_expediteTicks;
        }

        Anchor = a_anchorAfterExpedite;
        LockToResources = a_lockToResourceAfterExpedite;
        ManuallyTriggered = a_manuallyTriggered;
    }

    private const short c_anchorIdx = 0;
    private const short c_lockIdx = 1;
    private const short c_manualIdx = 2;
    private BoolVector32 m_bools;

    private long m_date;

    /// <summary>
    /// The date to start the expedite. The value is set to PT.ConstantDefinitions.PtDateTime.MIN_DATE if the StartType!=SpecificDateTime && StartType!=EndOfStableSpan.
    /// Date is set for EndOfStableSpan because the specific plant used to calculate the date isn't passed by the scheduler.
    /// </summary>
    public long Date
    {
        get => m_date;
        set => m_date = value;
    }

    /// <summary>
    /// Whether to anchor the expedited activities after the expedite.
    /// </summary>
    public bool Anchor
    {
        get => m_bools[c_anchorIdx];
        set => m_bools[c_anchorIdx] = value;
    }

    /// <summary>
    /// Whether to lock the expedited activities to the resources they've been expedited to.
    /// </summary>
    public bool LockToResources
    {
        get => m_bools[c_lockIdx];
        set => m_bools[c_lockIdx] = value;
    }

    /// <summary>
    /// Whether the Expedite was triggered via a drag-drop
    /// </summary>
    public bool ManuallyTriggered
    {
        get => m_bools[c_manualIdx];
        set => m_bools[c_manualIdx] = value;
    }

    private ExpediteStartDateType m_startDateType;

    /// <summary>
    /// The earliest the expedite can affect the schedule.
    /// </summary>
    public ExpediteStartDateType StartDateType
    {
        get => m_startDateType;
        set => m_startDateType = value;
    }

    private string m_alternatePathExternalId;

    /// <summary>
    /// A target path ExternalId for expedite
    /// </summary>
    public string AlternatePathExternalId
    {
        get => m_alternatePathExternalId;
        set => m_alternatePathExternalId = value;
    }
}

/// <summary>
/// Transmission for expediting a list of Jobs.
/// </summary>
public class ScenarioDetailExpediteJobsT : ScenarioDetailExpediteBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public ScenarioDetailExpediteJobsT(IReader reader)
        : base(reader, false)
    {
        if (reader.VersionNumber >= 273)
        {
            _resToScheduleEligibleLeadActivitiesOn = new BaseId(reader);
            m_jobs = new BaseIdList(reader);
        }

        #region 261
        else if (reader.VersionNumber >= 261)
        {
            m_jobs = new BaseIdList(reader);
        }
        #endregion

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            m_jobs = new BaseIdList(reader);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        _resToScheduleEligibleLeadActivitiesOn.Serialize(writer);
        m_jobs.Serialize(writer);
    }

    public new const int UNIQUE_ID = 126;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailExpediteJobsT() { }

    public ScenarioDetailExpediteJobsT(BaseId a_scenarioId, BaseIdList a_jobs, ExpediteStartDateType a_expediteStartDateType, long a_date, bool a_anchorAfterExpedite, bool a_lockAfterExpedite, bool a_manuallyTriggered)
        : base(a_scenarioId, a_expediteStartDateType, a_date, a_anchorAfterExpedite, a_lockAfterExpedite, a_manuallyTriggered)
    {
        m_jobs = a_jobs;
    }

    private readonly BaseIdList m_jobs;

    public BaseIdList Jobs => m_jobs;

    private BaseId _resToScheduleEligibleLeadActivitiesOn = BaseId.NULL_ID;

    public BaseId ResToScheduleEligibleLeadActivitiesOn
    {
        get => _resToScheduleEligibleLeadActivitiesOn;
        set => _resToScheduleEligibleLeadActivitiesOn = value;
    }

    public override string Description => "Jobs Expedited";
}

/// <summary>
/// Transmission for expediting a list of Manufacturing Orders.
/// </summary>
public class ScenarioDetailExpediteMOsT : ScenarioDetailExpediteBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public ScenarioDetailExpediteMOsT(IReader reader)
        : base(reader, true)
    {
        if (reader.VersionNumber >= 356)
        {
            m_mos = new MOKeyList(reader);
        }

        #region 274
        else if (reader.VersionNumber >= 274)
        {
            BaseId _DELETED_UNUSED_resToScheduleEligibleLeadActivitiesOn = new (reader);
            m_mos = new MOKeyList(reader);
        }
        #endregion

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            m_mos = new MOKeyList(reader);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        m_mos.Serialize(writer);
    }

    public new const int UNIQUE_ID = 270;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailExpediteMOsT() { }

    public ScenarioDetailExpediteMOsT(BaseId a_scenarioId, MOKeyList a_mos, ExpediteStartDateType a_expediteStartDateType, long a_date, bool a_anchorAfterExpedite, bool a_lockAfterExpedite, bool a_manuallyTriggered)
        : base(a_scenarioId, a_expediteStartDateType, a_date, a_anchorAfterExpedite, a_lockAfterExpedite, a_manuallyTriggered)
    {
        m_mos = a_mos;
    }

    private readonly MOKeyList m_mos;

    public MOKeyList MOs => m_mos;

    public override string Description => MOs.Count > 1 ? string.Format("MOs Expedited ({0})".Localize(), MOs.Count) : "MO Expedited";
}