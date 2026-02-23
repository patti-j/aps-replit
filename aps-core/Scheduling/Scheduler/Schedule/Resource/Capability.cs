using PT.APSCommon;
using PT.Common.Attributes;
using PT.Common.Compression;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Used to indicate which Resources can perform which Requirements.
/// </summary>
public partial class Capability : BaseCapability, ICloneable, IPTSerializable
{
    public new const int UNIQUE_ID = 333;

    #region IPTSerializable Members
    public Capability(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    internal Capability(BaseId a_id)
        : base(a_id) { }

    internal class CapabilityException : PTException
    {
        internal CapabilityException(string e)
            : base(e) { }
    }

    /// <summary>
    /// Sets the field values for an ERP transmission.
    /// </summary>
    /// <param name="a_id"></param>
    /// <param name="wcn"></param>
    internal Capability(BaseId a_id, CapabilityT.Capability a_capability, PTTransmission a_t)
        : base(a_id, a_capability)
    {
        Update(a_capability, a_t);
    }
    #endregion

    #region Transmission functionality
    /// <summary>
    /// Cleanup associations to prepare for delete.
    /// </summary>
    internal void Deleting(ProductRuleManager a_productRuleManager)
    {
        RemoveResourceAssociations(a_productRuleManager);
    }

    internal void Update(CapabilityT.Capability c, PTTransmission t, ScenarioDetail a_sd)
    {
        Update(c, t);
        if (c.ResourcesIsSet)
        {
            //Find the resources that have been removed
            for (int i = Resources.Count - 1; i >= 0; --i)
            {
                InternalResource resource = Resources.GetByIndex(i);
                if (!c.Resources.Contains(resource.GetKey()))
                {
                    //We need to remote associations
                    resource.DisassociateCapability(this, a_sd.ProductRuleManager);
                    RemoveResourceAssociation(resource);
                }
            }

            //Find the resources that have been added
            for (int i = c.Resources.Count - 1; i >= 0; --i)
            {
                InternalResource resource = a_sd.PlantManager.GetResource(c.Resources[i]);
                if (!Resources.Contains(resource.GetKey()))
                {
                    //This resource is new
                    AddResourceAssociation(resource);
                    resource.AddCapability(this);
                }
            }

            //Compute eligibility will called in transmission post processing.
        }
    }

    /// <summary>
    /// Returns the number of Active Resources with this Capability.
    /// </summary>
    /// <returns></returns>
    internal int GetActiveResourcesCount()
    {
        int count = 0;
        for (int i = 0; i < ResourceCount; i++)
        {
            InternalResource res = Resources.GetByIndex(i);
            if (res.Active)
            {
                count++;
            }
        }

        return count;
    }
    #endregion

    #region Overrides
    //Override ExternalId so it can be made editable for Capability so that the user can set it to match their 
    //   system ids even if they create the Capability in PT manually.
    /// <summary>
    /// Identifier for external system references.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.AllowEdit)]
    public override string ExternalId
    {
        get => base.ExternalId;
        internal set => base.ExternalId = value;
    }

    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public override string DefaultNamePrefix => "Capability";
    #endregion

    #region Cloning
    public Capability Clone()
    {
        Capability copy;
        using (BinaryMemoryWriter writer = new (ECompressionType.Fast))
        {
            Serialize(writer);
            using (BinaryMemoryReader reader = new (writer.GetBuffer()))
            {
                copy = new Capability(reader);
            }
        }

        return copy;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    #region Object Accessors
    //Note that this list is deserialized from InternalResource.
    private InternalResourceList m_resources = new ();

    [System.ComponentModel.Browsable(false)]
    [DoNotAuditProperty]
    public InternalResourceList Resources
    {
        get => m_resources;
        private set => m_resources = value;
    }

    [ListSource(ListSourceAttribute.ListSources.Resource, true)]
    /// <summary>
    /// The Resources that have this Capability.
    /// </summary>
    [System.ComponentModel.ParenthesizePropertyName(true)]
    public int ResourceCount => m_resources.Count;

    /// <summary>
    /// The number of Active Resources that have this Capability.
    /// </summary>
    public int ActiveResourceCount
    {
        get
        {
            int activeCount = 0;
            for (int i = 0; i < Resources.Count; i++)
            {
                if (Resources.GetByIndex(i).Active)
                {
                    activeCount++;
                }
            }

            return activeCount;
        }
    }
    #endregion

    #region Resource Associations
    public class ResourceAssociationException : PTException
    {
        public ResourceAssociationException(string r)
            : base(r) { }
    }

    /// <summary>
    /// Adds this Resource to the Capability's list of Resources.
    /// Does NOT set the Reference from the Resource to the Capability.
    /// </summary>
    /// <param name="r"></param>
    internal void AddResourceAssociation(InternalResource r)
    {
        if (r == null)
        {
            throw new ResourceAssociationException("2813");
        }

        if (!m_resources.Contains(r.GetKey()))
        {
            m_resources.Add(r);
        }
    }

    internal void RemoveResourceAssociation(InternalResource r)
    {
        if (r == null)
        {
            throw new ResourceAssociationException("2813");
        }

        if (m_resources.Contains(r.GetKey()))
        {
            m_resources.Remove(r.GetKey());
        }
    }

    internal void RemoveResourceAssociations(ProductRuleManager a_productRuleManager)
    {
        for (int i = Resources.Count - 1; i >= 0; --i)
        {
            InternalResource r = Resources.GetByIndex(i);
            r.DisassociateCapability(this, a_productRuleManager);

            if (m_resources.Contains(r.GetKey()))
            {
                m_resources.Remove(r.GetKey());
            }
        }
    }
    #endregion Resource Associations
}