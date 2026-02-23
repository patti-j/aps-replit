using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Represents the work of one Resource on an Activity or Subcontract Operation.
/// </summary>
public abstract class Block : BaseIdObject, IPTSerializable
{
    public new const int UNIQUE_ID = 10;

    #region IPTSerializable Members
    protected Block(IReader reader)
        : base(reader)
    {
        m_restoreReferences = new RefRestorer();

        if (reader.VersionNumber >= 407)
        {
            reader.Read(out m_scheduled);
            m_restoreReferences.satisfiedRequirementId = new BaseId(reader);
        }

        #region 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_scheduled);

            long tmp;
            reader.Read(out tmp); // Removed m_scheduledFinishDateTicks; it's now in the batch
            reader.Read(out tmp); // Removed m_scheduledStartDate
            m_restoreReferences.satisfiedRequirementId = new BaseId(reader);
        }
        #endregion
    }

    private RefRestorer m_restoreReferences;

    private class RefRestorer
    {
        [Obsolete("This might be obsolete since a block can now satisfy multiple resource requirements. Perhaps store a reference to the index.")]
        public BaseId satisfiedRequirementId;
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_scheduled);

        m_satisfiedRequirement.Id.Serialize(writer);
    }

    internal void RestoreReferences(InternalOperation operation)
    {
        m_satisfiedRequirement = operation.ResourceRequirements[m_restoreReferences.satisfiedRequirementId];
        m_restoreReferences = null;
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    internal Block(BaseId a_id, ResourceRequirement a_rr)
        : base(a_id)
    {
        m_satisfiedRequirement = a_rr;
        m_scheduled = true;
    }

    //#if DEBUG
    //        // this is just some test code to try to find all the places where id of  a block is being used.
    //        BaseId ___idDelete;
    //        public new BaseId Id
    //        {
    //            get { return ___idDelete; }
    //            internal set { ___idDelete = value; }
    //        }
    //#else
    //        delete debug code
    //#endif

    /// <summary>
    /// Returns the cost of using the Resource for the Block.
    /// </summary>
    /// <returns></returns>
    public abstract decimal GetResourceCost();

    [Obsolete("This may be deleted soon or changed soon.")]
    public abstract BlockKey GetKey();

    #region Properties
    [Obsolete("This may be obsolete since a block can satisfy multiple resource requirements.")]
    private ResourceRequirement m_satisfiedRequirement;

    [Obsolete("This may be obsolete since a block can satisfy multiple resource requirements.")]
    [System.ComponentModel.Browsable(false)]
    public ResourceRequirement SatisfiedRequirement => m_satisfiedRequirement;

    /// <summary>
    /// This is the index of the satisfied ResourceRequirement in the list of Operation ResourceRequirements.
    /// </summary>
    public int ResourceRequirementIndex => (int)Id.ToBaseType();

    protected bool m_scheduled;

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public bool Scheduled => m_scheduled;

    public string RequiredCapabilities
    {
        get
        {
            System.Text.StringBuilder builder = new ();

            for (int i = 0; i < SatisfiedRequirement.CapabilityManager.Count; i++)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(SatisfiedRequirement.CapabilityManager[i].Name);
            }

            return builder.ToString();
        }
    }
    #endregion

    //public class BlockComparer : System.Collections.IComparer
    //{
    //    #region IComparer Members

    //    public int Compare(object x, object y)
    //    {
    //        Block blockX = (Block)x;
    //        Block blockY = (Block)y;
    //        if (blockX.Id == blockY.Id)
    //            return 0;
    //        else if (blockX.Id.CompareTo(blockY.Id) == -1)
    //            return -1;
    //        else
    //            return 1;
    //    }

    //    #endregion

    //}
}