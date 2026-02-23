using System.Data;

using PT.APSCommon;
using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// Summary description for ERPMaintenanceTransmission.
/// </summary>
public class ERPMaintenanceTransmission<T> : ERPTransmission, IPTSerializable
{
    public new const int UNIQUE_ID = 210;

    #region IPTSerializable Members
    public ERPMaintenanceTransmission(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 157)
        {
            a_reader.Read(out m_autoDeleteMode);
            Instigator = new BaseId(a_reader);
        }

        #region 1
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out m_autoDeleteMode);

            a_reader.Read(out bool tempSequenced);

            Instigator = new BaseId(a_reader);
        }
        #endregion
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_autoDeleteMode);
        Instigator.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ERPMaintenanceTransmission() { }

    // SGS
//        protected List<T> m_nodes = new List<T>();
    private List<T> m_nodes = new ();

    public List<T> Nodes
    {
        get => m_nodes;
        set => m_nodes = value;
    }

    private bool m_autoDeleteMode;

    public bool AutoDeleteMode
    {
        get => m_autoDeleteMode;
        set => m_autoDeleteMode = value;
    }

    public int Add(T node)
    {
        m_nodes.Add(node);
        return m_nodes.LastIndexOf(node);
    }

    public void AddRange(IEnumerable<T> a_collection)
    {
        m_nodes.AddRange(a_collection);
    }

    public virtual int Count => m_nodes.Count;

    public void Clear()
    {
        m_nodes.Clear();
    }

    public T this[int i] => m_nodes[i];

    public virtual void Validate()
    {
        HashSet<string> externalIds = new ();

        foreach (object n in m_nodes)
        {
            PTObjectBase ptObj = n as PTObjectBase;
            if (ptObj == null)
            {
                continue;
            }

            if (externalIds.Contains(ptObj.ExternalId))
            {
                throw new ValidationException("2045", new object[] { n.GetType().FullName, ptObj.ExternalId });
            }

            ptObj.Validate();
            externalIds.Add(ptObj.ExternalId);
        }
    }

    #region Database Loading
    protected void FillTable(DataTable a_table, IDbCommand a_cmd)
    {
        FillDataTable(a_table, a_cmd, GetType().Name);
    }
    #endregion Database Loading
}