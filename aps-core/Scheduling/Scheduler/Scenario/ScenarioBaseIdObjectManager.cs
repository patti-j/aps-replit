using System.ComponentModel;

using PT.APSCommon;
using PT.SystemDefinitions.Interfaces;

namespace PT.Scheduler;

/// <summary>
/// A generic Collection that stores BaseIdObjects (similar to BaseIdObjectManager) and implements IScenarioRef meaning it has reference to Scenario and ScenarioDetail it
/// belongs to.
/// </summary>
public abstract class ScenarioBaseIdObjectManager<T> : ExternalBaseIdObjectManager<T>, IScenarioRef, IPTSerializable where T : ExternalBaseIdObject
{
    #region IPTSerializable Members
    protected ScenarioBaseIdObjectManager(BaseIdGenerator a_baseIdGenerator)
        : base(a_baseIdGenerator) { }
    
    protected ScenarioBaseIdObjectManager(ScenarioBaseIdObjectManager<T> a_sdm, BaseIdGenerator a_baseIdGenerator)
        : base(a_sdm, a_baseIdGenerator) { }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public new const int UNIQUE_ID = 18;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    internal virtual void RestoreReferences(Scenario a_scenario, ScenarioDetail a_sd, ISystemLogger a_errorReporter)
    {
        //TODO: Remove the need for these references
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