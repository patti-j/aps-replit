using System.ComponentModel;

using PT.APSCommon;
using PT.SystemDefinitions.Interfaces;

namespace PT.Scheduler;

/// <summary>
/// A generic collection that stores BaseObjects (similar to BaseObjectManager) and implements IScenarioRef meaning it has reference to Scenario and ScenarioDetail it belongs to.
/// </summary>
public abstract class ScenarioBaseObjectManager<T> : BaseObjectManager<T>, IScenarioRef, IPTSerializable where T : BaseObject
{
    #region IPTSerializable Members
    protected ScenarioBaseObjectManager(BaseIdGenerator a_baseIdGenerator)
        : base(a_baseIdGenerator) { }

    protected ScenarioBaseObjectManager(ScenarioBaseObjectManager<T> a_sdm, BaseIdGenerator a_baseIdGenerator)
        : base(a_sdm, a_baseIdGenerator) { }

    public new const int UNIQUE_ID = 18;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    protected ISystemLogger m_errorReporter;

    internal virtual void RestoreReferences(Scenario a_scenario, ScenarioDetail a_sd, ISystemLogger a_errorReporter)
    {
        m_errorReporter = a_errorReporter;
        m_scenario = a_scenario;
        m_scenarioDetail = a_sd;
    }
    #endregion

    #region IScenarioRef
    protected Scenario m_scenario;
    protected ScenarioDetail m_scenarioDetail;

    void IScenarioRef.SetReferences(Scenario a_scenario, ScenarioDetail a_scenarioDetail)
    {
        if (m_scenario == null)
        {
            m_scenario = a_scenario;
            m_scenarioDetail = a_scenarioDetail;
            ScenarioRef.SetRef(this, a_scenario, a_scenarioDetail);
        }
    }
    #endregion
}