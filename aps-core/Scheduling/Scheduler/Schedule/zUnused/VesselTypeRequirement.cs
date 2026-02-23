using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Requirement for a VesselType HelperResource.
/// </summary>
[Serializable]
public class VesselTypeRequirement : ExternalBaseIdObject, ICloneable, IPTSerializable
{
    public new const int UNIQUE_ID = 318;

    #region IPTSerializable Members
    public VesselTypeRequirement(IReader reader)
        : base(reader)
    {
        m_referenceInfo = new ReferenceInfo();

        if (reader.VersionNumber >= 1)
        {
            int val;
            reader.Read(out val);
            claimTiming = (VesselRequirementDefs.claimTimings)val;
            reader.Read(out val);
            releaseTiming = (VesselRequirementDefs.releaseTimings)val;
            reader.Read(out requiredQty);

            m_referenceInfo.requiredVesselTypeId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)claimTiming);
        writer.Write((int)releaseTiming);
        writer.Write(requiredQty);

        requiredVesselType.Id.Serialize(writer);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    [NonSerialized] private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo
    {
        internal BaseId requiredVesselTypeId;
    }

    internal void RestoreReferences(VesselTypeManager vesselTypes)
    {
        requiredVesselType = vesselTypes.GetById(m_referenceInfo.requiredVesselTypeId);

        m_referenceInfo = null;
    }
    #endregion

    #region Declarations
    public class VesselTypeRequirementException : PTException
    {
        public VesselTypeRequirementException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    public VesselTypeRequirement(BaseId id, InternalOperation claimingOperation, VesselRequirementDefs.claimTimings claimTiming, InternalOperation releasingOperation, VesselRequirementDefs.releaseTimings releaseTiming, decimal requiredQty)
        : base(id)
    {
        claimingOperation.VesselTypeRequirementClaims.Add(this);
        releasingOperation.VesselTypeRequirementReleases.Add(this);
        ReleaseTiming = releaseTiming;
        RequiredQty = requiredQty;
    }
    #endregion

    #region Shared Properties
    private VesselRequirementDefs.claimTimings claimTiming = VesselRequirementDefs.claimTimings.SetupStart;

    /// <summary>
    /// When, based on the claimingOperation to claim the HelperResource.
    /// </summary>
    public VesselRequirementDefs.claimTimings ClaimTiming
    {
        get => claimTiming;
        set => claimTiming = value;
    }

    private VesselRequirementDefs.releaseTimings releaseTiming = VesselRequirementDefs.releaseTimings.RunEnd;

    /// <summary>
    /// When, based on the releasingOperation to release the HelperResource.
    /// </summary>
    public VesselRequirementDefs.releaseTimings ReleaseTiming
    {
        get => releaseTiming;
        set => releaseTiming = value;
    }

    private decimal requiredQty = 1;

    /// <summary>
    /// The number of VesselTypes needed to perform the Activity.
    /// </summary>
    public decimal RequiredQty
    {
        get => requiredQty;
        set => requiredQty = value;
    }
    #endregion

    #region Object References
    private VesselType requiredVesselType;

    [Browsable(false)]
    public VesselType RequiredVesselType
    {
        get => requiredVesselType;
        set => requiredVesselType = value;
    }
    #endregion

    #region Transmission functionality
    #endregion

    #region Cloning
    public VesselTypeRequirement Clone()
    {
        return (VesselTypeRequirement)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion
}

#region VesselTypeRequirementsCollection
/// <summary>
/// List of all the VesselType Requirments for the Operation.
/// </summary>
public class VesselTypeRequirementsCollection : IPTSerializable
{
    public const int UNIQUE_ID = 309;

    #region IPTSerializable Members
    public VesselTypeRequirementsCollection(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                VesselTypeRequirement v = new (reader);
                Add(v);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif

        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).Serialize(writer);
        }
    }

    public int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    private readonly ArrayList vesselTypeRequirements = new ();

    public class VesselTypeRequirementsCollectionException : PTException
    {
        public VesselTypeRequirementsCollectionException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    public VesselTypeRequirementsCollection() { }
    #endregion

    #region Properties and Methods
    public Type ElementType => typeof(VesselTypeRequirement);

    public VesselTypeRequirement Add(VesselTypeRequirement vesselTypeRequirement)
    {
        vesselTypeRequirements.Add(vesselTypeRequirement);
        return vesselTypeRequirement;
    }

    public void Remove(int index)
    {
        vesselTypeRequirements.RemoveAt(index);
    }

    public VesselTypeRequirement GetByIndex(int index)
    {
        return (VesselTypeRequirement)vesselTypeRequirements[index];
    }

    public int Count => vesselTypeRequirements.Count;
    #endregion
}
#endregion