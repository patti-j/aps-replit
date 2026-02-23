using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class ScenarioDetailChangeMOQtyT : ScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public ScenarioDetailChangeMOQtyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 362)
        {
            ActivityKey = new ActivityKey(reader);
            reader.Read(out m_qty);
        }
        else if (reader.VersionNumber >= 361)
        {
            ActivityKey = new ActivityKey(reader);
            long qty;
            reader.Read(out qty);
            Qty = qty;
        }
        else if (reader.VersionNumber >= 1)
        {
            BaseId jobId = new (reader);
            BaseId moId = new (reader);
            ActivityKey = new ActivityKey(jobId, moId, BaseId.NULL_ID, BaseId.NULL_ID);

            long qty;
            reader.Read(out qty);
            Qty = qty;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        ActivityKey.Serialize(writer);
        writer.Write(m_qty);
    }

    public const int UNIQUE_ID = 585;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailChangeMOQtyT() { }

    public ScenarioDetailChangeMOQtyT(BaseId a_scenarioId, ActivityKey a_activityKey, decimal a_qty)
        : base(a_scenarioId)
    {
        ActivityKey = a_activityKey;
        Qty = a_qty;
    }

    private ActivityKey m_activity;

    /// <summary>
    /// The activity whose number of cycles you want to change.
    /// </summary>
    public ActivityKey ActivityKey
    {
        get => m_activity;
        private set => m_activity = value;
    }

    private decimal m_qty;

    /// <summary>
    /// The new number of cycles the activity should be.
    /// </summary>
    public decimal Qty
    {
        get => m_qty;
        private set => m_qty = value;
    }

    public override string Description => "Activity cycles changed";
}