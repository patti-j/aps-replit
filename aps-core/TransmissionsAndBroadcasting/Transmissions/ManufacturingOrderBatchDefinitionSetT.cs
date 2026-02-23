using PT.APSCommon;

namespace PT.Transmissions;

public class ManufacturingOrderBatchDefinitionSetT : ScenarioIdBaseT, IEnumerable<ManufacturingOrderBatchDefinitionT>
{
    #region IPTSerializable
    public ManufacturingOrderBatchDefinitionSetT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                ManufacturingOrderBatchDefinitionT mobd = new (reader);
                Add(mobd);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_moBatchDefinitions.Count);
        for (int i = 0; i < m_moBatchDefinitions.Count; ++i)
        {
            m_moBatchDefinitions[i].Serialize(writer);
        }
    }

    public const int UNIQUE_ID = 672;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ManufacturingOrderBatchDefinitionSetT() { }

    public ManufacturingOrderBatchDefinitionSetT(BaseId a_id)
        : base(a_id) { }

    internal List<ManufacturingOrderBatchDefinitionT> m_moBatchDefinitions = new ();

    internal int Count => m_moBatchDefinitions.Count;

    public void Add(ManufacturingOrderBatchDefinitionT a_moBatchDefinition)
    {
        m_moBatchDefinitions.Add(a_moBatchDefinition);
    }

    #region IEnumerable<MOBatchDefinition> Members
    public IEnumerator<ManufacturingOrderBatchDefinitionT> GetEnumerator()
    {
        return new MOBDEnumerator(this);
    }
    #endregion

    #region IEnumerable Members
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return new MOBDEnumerator(this);
    }

    private class MOBDEnumerator : IEnumerator<ManufacturingOrderBatchDefinitionT>
    {
        private readonly ManufacturingOrderBatchDefinitionSetT m_moBatchDefSet;
        private int m_currentIdx;

        internal MOBDEnumerator(ManufacturingOrderBatchDefinitionSetT a_mobdSet)
        {
            m_moBatchDefSet = a_mobdSet;
            Reset();
        }

        #region IEnumerator<MOBatchDefinition> Members
        public ManufacturingOrderBatchDefinitionT Current => m_moBatchDefSet.m_moBatchDefinitions[m_currentIdx];
        #endregion

        #region IDisposable Members
        public void Dispose() { }
        #endregion

        #region IEnumerator Members
        object System.Collections.IEnumerator.Current => Current;

        public bool MoveNext()
        {
            ++m_currentIdx;
            return m_currentIdx < m_moBatchDefSet.Count;
        }

        public void Reset()
        {
            m_currentIdx = -1;
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// Verify names are unique.
    /// Verify ids are unique.
    /// </summary>
    public void Validate()
    {
        IEnumerator<ManufacturingOrderBatchDefinitionT> mobdEnum = GetEnumerator();
        HashSet<BaseId> idTest = new ();
        HashSet<string> nameTest = new ();

        while (mobdEnum.MoveNext())
        {
            ManufacturingOrderBatchDefinitionT mobd = mobdEnum.Current;

            if (mobd.m_id != BaseId.NULL_ID)
            {
                if (idTest.Contains(mobd.m_id))
                {
                    throw new ValidationException("2775", new object[] { mobd.m_id.Value });
                }

                idTest.Add(mobd.m_id);
            }

            if (nameTest.Contains(mobd.m_name))
            {
                throw new ValidationException("2776", new object[] { mobd.m_name });
            }

            nameTest.Add(mobd.m_name);
        }
    }

    public override string Description => "MO Batching Rule saved";
}