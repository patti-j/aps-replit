using PT.APSCommon;
using PT.Transmissions;

namespace PT.Transmissions2
{
    /// <summary>
    /// This transmission is used to determine what PT Objects from the source Scenario
    /// should be merged into the target Scenario.
    /// </summary>
    public class MergeScenarioDataT : ScenarioBaseT
    {

        #region IPTSerializable Members
        public MergeScenarioDataT(IReader a_reader) : base(a_reader)
        {
            m_targetScenarioId = new BaseId(a_reader);
            m_sourceScenarioId = new BaseId(a_reader);
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            m_targetScenarioId.Serialize(a_writer);
            m_sourceScenarioId.Serialize(a_writer);
        }
        #endregion

        public MergeScenarioDataT() { }

        public MergeScenarioDataT(
            BaseId a_targetScenario,
            BaseId a_sourceScenario
        )
        {
            m_targetScenarioId = a_targetScenario;
            m_sourceScenarioId = a_sourceScenario;
        }

        //public MergeScenarioDataT(
        //    BaseId a_targetScenario,
        //    BaseId a_sourceScenario,
        //    IEnumerable<BaseId> a_resourceIds,
        //    IEnumerable<BaseId> a_plantIds = null,
        //    IEnumerable<BaseId> a_departmentIds = null
        //)
        //{
        //    m_targetScenarioId = a_targetScenario;
        //    m_sourceScenarioId = a_sourceScenario;

        //    m_resourceIds = a_resourceIds;
        //    m_plantIds = a_plantIds;
        //    m_departmentIds = a_departmentIds;
        //}

        public BaseId TargetScenarioId => m_targetScenarioId;
        public BaseId SourceScenarioId => m_sourceScenarioId;
        
        private BaseId m_targetScenarioId;
        private BaseId m_sourceScenarioId;

        //public IEnumerable<BaseId> PlantIds => m_plantIds;
        //public IEnumerable<BaseId> DepartmentIds => m_departmentIds;
        //public IEnumerable<BaseId> ResourceIds => m_resourceIds;

        //private IEnumerable<BaseId> m_plantIds;
        //private IEnumerable<BaseId> m_departmentIds;
        //private IEnumerable<BaseId> m_resourceIds;

        public override int UniqueId => UNIQUE_ID;

        public const int UNIQUE_ID = 1113;
    }
}
