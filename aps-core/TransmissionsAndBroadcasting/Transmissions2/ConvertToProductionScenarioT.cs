using PT.APSCommon;
using PT.Transmissions;

namespace PT.Transmissions2
{
    /// <summary>
    /// This transmission is used when swapping Production scenarios. The non-Production scenario is the target scenario
    /// while the Production scenario is the source scenario. The ScenarioManager will currently ignore the value
    /// for Production stored on the transmission and force the nonProductionScenario to become a Production Scenario.
    /// We may change this behavior in the future when we add a setting to allow for multiple Production scenarios so
    /// I left the Production field in for the time being.
    /// </summary>
    /// All the commented out code is stuff that might be useful if we add the setting to allow for multiple Production scenarios
    public class ConvertToProductionScenarioT : ScenarioBaseT
    {
        public const int UNIQUE_ID = 1114;
        #region IPTSerializable Members
        public override int UniqueId => UNIQUE_ID;

        public ConvertToProductionScenarioT(IReader a_reader) : base(a_reader)
        {
            m_nonProductionScenarioId = new BaseId(a_reader);
            m_bools = new BoolVector32(a_reader);
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);
            m_nonProductionScenarioId.Serialize(a_writer);
            m_bools.Serialize(a_writer);
        }
        #endregion

        public ConvertToProductionScenarioT() { }

        public ConvertToProductionScenarioT(BaseId a_nonProductionScenarioId, bool a_deleteNonProductionScenario)
        {
            m_nonProductionScenarioId = a_nonProductionScenarioId;
            m_bools[c_autoDeleteNonProductionScenarioIdx] = a_deleteNonProductionScenario;
        }

        public BaseId NonProductionScenarioId => m_nonProductionScenarioId;

        public bool AutoDeleteNonProdScenario => m_bools[c_autoDeleteNonProductionScenarioIdx];

        private readonly BaseId m_nonProductionScenarioId; // target scenario
        private BoolVector32 m_bools;

        //Constants
        private const short c_autoDeleteNonProductionScenarioIdx = 0;

    }
}
