using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Usually represents a physical grouping of Resources in an autonomous production line.
/// Once an operation is scheduled in a cell successors will be scheduled within the same cell if possible.
/// (if using a simulation rule that recognizes Cells).
/// </summary>
public class Cell : BaseObject, ICloneable, IPTSerializable
{
    public new const int UNIQUE_ID = 11;

    #region IPTSerializable Members
    internal Cell(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12054)
        {
            //nothing to deserialize in these version
        }
        else
        {
            a_reader.Read(out int conWipMoLimit); //deprecated
            a_reader.Read(out bool enforceConWipMoLimit); //deprecated
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    //Property names for DataTables.
    internal class CellException : PTException
    {
        internal CellException(string a_message)
            : base(a_message) { }
    }
    #endregion

    #region Construction
    internal Cell(BaseId a_id)
        : base(a_id) { }

    internal Cell(BaseId a_id, CellT.Cell a_cellNode, UserFieldDefinitionManager a_udfManager)
        : base(a_id, a_cellNode, a_udfManager, UserField.EUDFObjectType.Cells) { }

    internal void Update(UserFieldDefinitionManager a_udfManager, CellT.Cell a_cell, PTTransmission a_t)
    {
        base.Update(a_cell, a_t, a_udfManager, UserField.EUDFObjectType.Cells);
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    public override string DefaultNamePrefix => "Cell";
    #endregion

    #region Resources
    private BaseResourceSortedList m_resources = new ();

    //		[ListSource(ListSourceAttribute.ListSources.Resource,true)]
    /// <summary>
    /// The Resources that have this Capability.
    /// </summary>
    //		[System.ComponentModel.ParenthesizePropertyName(true)]
    //		[ReadOnly(true)]
    //		public int ResourceCount
    //		{
    //			get{return this.resources.Count;}
    ////			set{;}
    //		}
    internal class ResourceAssociationException : PTException
    {
        internal ResourceAssociationException(string r)
            : base(r) { }
    }

    internal void AddResourceAssociation(BaseResource a_r)
    {
        if (a_r == null)
        {
            throw new ResourceAssociationException("2813");
        }

        if (!m_resources.Contains(a_r))
        {
            m_resources.Add(a_r);
        }
    }

    internal void Deleting()
    {
        RemoveResourceAssociations();
    }

    internal void RemoveResourceAssociation(BaseResource a_r)
    {
        if (a_r == null)
        {
            throw new ResourceAssociationException("2813");
        }

        if (m_resources.Contains(a_r))
        {
            m_resources.Remove(a_r);
        }
    }

    internal void RemoveResourceAssociations()
    {
        for (int i = m_resources.Count - 1; i >= 0; --i)
        {
            BaseResource r = m_resources[i];
            r.DissassociateCell();

            if (m_resources.Contains(r))
            {
                m_resources.Remove(r);
            }
        }
    }
    #endregion

    #region Cloning
    public Cell Clone()
    {
        return (Cell)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion
}