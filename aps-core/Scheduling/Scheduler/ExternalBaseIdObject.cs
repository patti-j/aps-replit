using System.ComponentModel;

using PT.APSCommon;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Abstract class for all objects in the system that can be created from an external system.
/// </summary>
public abstract class ExternalBaseIdObject : BaseIdObject
{
    //public new const int UNIQUE_ID = 15;

    #region IPTSerializable Members
    protected ExternalBaseIdObject(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out m_externalId);
        }
    }

    // This extra constructor is needed for backwards compatibility of DispatcherDefinitions.
    // The bool parameter is for providing a different function signature since its
    // value is based on the version of the reader. 
    protected ExternalBaseIdObject(IReader a_reader, bool a_isFromOlderScenario)
        : base(a_reader)
    {
        if (!a_isFromOlderScenario)
        {
            a_reader.Read(out m_externalId);
        }

        // to make sure we don't have a null or empty externalId whether from an older scenario
        // or a weird situation where the external id was not serialized (for example, item reported in #26550
        // even though the version of the reader was not an older version)
        if (string.IsNullOrEmpty(m_externalId))
        {
            SetExternalId(string.Empty, Id.ToBaseType());
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_externalId);
    }

    [Browsable(false)]
    public override int UniqueId => 15;
    #endregion

    #region Construction
    protected ExternalBaseIdObject(string a_externalId, BaseId a_id)
        : base(a_id)
    {
        SetExternalId(a_externalId, a_id.ToBaseType());
    }

    /// <summary>
    /// This is for internally created objects where no External Id was specified.
    /// </summary>
    protected ExternalBaseIdObject(BaseId a_id)
        : base(a_id)
    {
        SetExternalId("", a_id.ToBaseType());
    }

    protected ExternalBaseIdObject(long a_externalIdPTDefaultNbr, BaseId a_id)
        : base(a_id)
    {
        SetExternalId("", a_externalIdPTDefaultNbr);
    }

    private void SetExternalId(string a_externalId, long a_externalIdPTDefaultNbr)
    {
        //No blank external id's allowed so fill it in by creating a unique external id from the base id.
        if (a_externalId == "")
        {
            m_externalId = MakeExternalId(a_externalIdPTDefaultNbr);
        }
        else
        {
            m_externalId = a_externalId;
        }
    }

    protected ExternalBaseIdObject(PTObjectIdBase a_ptObjectIdBase)
    {
        m_externalId = a_ptObjectIdBase.ExternalId;
    }

    protected ExternalBaseIdObject(BaseId a_newId, ExternalBaseIdObject a_sourceObject)
        : base(a_newId, a_sourceObject)
    {
        SetExternalId(a_sourceObject.ExternalId, a_newId.ToBaseType());
    }

    internal static string MakeExternalId(long a_idNbr)
    {
        return "PT" + a_idNbr.ToString().PadLeft(5, '0');
    }
    #endregion

    #region Transmission Functionality
    internal bool Update(ExternalBaseIdObject a_o)
    {
        if (ExternalId != a_o.ExternalId)
        {
            ExternalId = a_o.ExternalId;
            return true;
        }

        return false;
    }

    internal virtual bool Update(PTObjectBase a_o)
    {
        if (ExternalId != a_o.ExternalId)
        {
            ExternalId = a_o.ExternalId;
            return true;
        }

        return false;
    }
    #endregion

    #region Shared Properties
    private string m_externalId = "";

    /// <summary>
    /// Identifier for external system references.
    /// </summary>
    public virtual string ExternalId
    {
        get => m_externalId;
        internal set => m_externalId = value;
    }
    #endregion
}