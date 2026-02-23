using PT.APSCommon;

namespace PT.Scheduler.CoPilot.Pruning.Modules;

internal class BaseDeleteModule
{
    protected int m_groupModifier = 2;
    protected int m_listIdx;
    protected List<BaseIdList> m_jobKeyGroupList;
    protected bool m_lastGroupSplit;

    public bool InitialConfiguration(Scenario a_scenario)
    {
        m_groupModifier = 2;
        m_listIdx = 0;
        m_jobKeyGroupList = new List<BaseIdList>();
        return PopulateGroupLists(a_scenario);
    }

    protected virtual bool PopulateGroupLists(Scenario a_scenario)
    {
        return true;
    }

    public bool Reconfigure(Scenario a_scenario)
    {
        m_listIdx++;
        if (m_listIdx == m_jobKeyGroupList.Count)
        {
            //We have run all of the groups in the list. Reset
            m_listIdx = 0;
            m_jobKeyGroupList.Clear();
            m_groupModifier *= 2; //Break into smaller groups               
            return PopulateGroupLists(a_scenario);
        }

        return true;
    }
}

internal class BaseJobModule
{
    protected readonly BaseId JobId;

    public BaseJobModule(Job a_job)
    {
        JobId = a_job.Id;
    }

    public Job FindJob(Scenario a_scenario)
    {
        ScenarioDetail sd;
        using (a_scenario.ScenarioDetailLock.EnterRead(out sd))
        {
            return sd.JobManager.GetById(JobId);
        }
    }
}

internal class BaseManufacturingOrderModule : BaseJobModule
{
    protected readonly BaseId ManufacturingOrderId;

    public BaseManufacturingOrderModule(ManufacturingOrder a_mo)
        : base(a_mo.Job)
    {
        ManufacturingOrderId = a_mo.Id;
    }

    public ManufacturingOrder FindManufacturingOrder(Scenario a_scenario)
    {
        Job j = FindJob(a_scenario);
        if (j != null)
        {
            return j.ManufacturingOrders.GetById(ManufacturingOrderId);
        }

        return null;
    }
}

internal class BaseOperationModule : BaseManufacturingOrderModule
{
    protected readonly BaseId OperationId;

    public BaseOperationModule(BaseOperation a_op)
        : base(a_op.ManufacturingOrder)
    {
        OperationId = a_op.Id;
    }

    public BaseOperation FindOperation(Scenario a_scenario)
    {
        ManufacturingOrder mo = FindManufacturingOrder(a_scenario);
        if (mo != null)
        {
            return mo.OperationManager[OperationId];
        }

        return null;
    }
}