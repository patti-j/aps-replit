using PT.APSCommon;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Abstract base class for Jobs and Manufacturing Orders.
/// </summary>
public abstract class BaseOrder : BaseObject, IPTSerializable
{
    public new const int UNIQUE_ID = 7;

    #region IPTSerializable Members
    public BaseOrder(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_canSpanPlants);

            m_elligibleUsers = new EligibleUsersCollection(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_canSpanPlants);

        m_elligibleUsers.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(ScenarioDetail a_sd)
    {
        m_sd = a_sd;
    }
    #endregion

    #region Construction
    protected BaseOrder(BaseId a_id, ScenarioDetail a_sd)
        : base(a_id)
    {
        m_sd = a_sd;
    }

    protected BaseOrder(BaseId id, PTObjectBase a_ptObject, ScenarioDetail a_sd, UserFieldDefinitionManager a_udfManager, UserField.EUDFObjectType a_manufacturingOrders)
        : base(id, a_ptObject, a_udfManager, a_manufacturingOrders)
    {
        m_sd = a_sd;
    }

    protected BaseOrder(BaseId a_newId, BaseOrder a_bo)
        : base(a_newId, a_bo)
    {
        m_sd = a_bo.ScenarioDetail;
    }

    public class BaseOrderException : PTException
    {
        public BaseOrderException(string e)
            : base(e) { }
    }
    #endregion Construction

    #region Shared Properties
    private bool m_canSpanPlants; // faster default and more likely how companies will operate

    /// <summary>
    /// If true, then the Operations can schedule in more than one plant.  Otherwise, all operations must be scheduled in only one Plant.
    /// </summary>
    public virtual bool CanSpanPlants
    {
        get => m_canSpanPlants;
        internal set => m_canSpanPlants = value;
    }
    #endregion
    
    /// <summary>
    /// Whether the Order has been completed in production.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public abstract bool Finished { get; }

    //COLLECTIONS
    private EligibleUsersCollection m_elligibleUsers = new ();

    /// <summary>
    /// Specifies User Rights to the object.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public EligibleUsersCollection ElligibleUsers //NEEDPT
    {
        get => m_elligibleUsers;
        set => m_elligibleUsers = value;
    }

    #region Properties
    private ScenarioDetail m_sd;

    internal ScenarioDetail ScenarioDetail => m_sd;
    #endregion

    #region ERP transmission status update
    /// <summary>
    /// Call this function before handling a JobT or some other transmission that updates the status of jobs.
    /// It resets the activity variables that indicate the type of updates that have occurred.
    /// </summary>
    internal virtual void ResetERPStatusUpdateVariables() { }
    #endregion
}