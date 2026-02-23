using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class ResourceEditT : ScenarioIdBaseT, IPTSerializable
{
    #region PT Serialization
    public static int UNIQUE_ID => 1046;

    public ResourceEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                PlantEdit node = new (a_reader);
                m_plantEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                DepartmentEdit node = new (a_reader);
                m_departmentEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                ResourceEdit node = new (a_reader);
                m_resourceEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_plantEdits.Count);
        foreach (PlantEdit plantEdit in m_plantEdits)
        {
            plantEdit.Serialize(a_writer);
        }

        a_writer.Write(m_departmentEdits.Count);
        foreach (DepartmentEdit deptEdit in m_departmentEdits)
        {
            deptEdit.Serialize(a_writer);
        }

        a_writer.Write(m_resourceEdits.Count);
        foreach (ResourceEdit resEdit in m_resourceEdits)
        {
            resEdit.Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceEditT() { }

    public ResourceEditT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public void Validate()
    {
        foreach (ResourceEdit resEdit in m_resourceEdits)
        {
            resEdit.Validate();
        }

        foreach (DepartmentEdit deptEdit in m_departmentEdits)
        {
            //deptEdit.Validate();
        }

        foreach (PlantEdit plantEdit in m_plantEdits)
        {
            //plantEdit.Validate();
        }
    }

    public override string Description => string.Format("Plants updated ({0}) | Departments updated ({1}) | Resources updated ({2})".Localize(),
        m_plantEdits.Count,
        m_departmentEdits.Count,
        m_resourceEdits.Count);

    private readonly List<ResourceEdit> m_resourceEdits = new ();
    public List<ResourceEdit> ResourceEdits => m_resourceEdits;

    private readonly List<DepartmentEdit> m_departmentEdits = new ();
    public List<DepartmentEdit> DepartmentEdits => m_departmentEdits;

    private readonly List<PlantEdit> m_plantEdits = new ();
    public List<PlantEdit> PlantEdits => m_plantEdits;

    public void Add(ResourceEdit a_pts)
    {
        m_resourceEdits.Add(a_pts);
    }

    public void Add(DepartmentEdit a_pts)
    {
        m_departmentEdits.Add(a_pts);
    }

    public void Add(PlantEdit a_pts)
    {
        m_plantEdits.Add(a_pts);
    }
}

/// <summary>
/// A standard Item to be purchased for stock.  The received Item will go to stock for use by any Job requiring the Item.
/// </summary>
public class ResourceEdit : PTObjectBaseEdit, IPTSerializable
{
    #region PT Serialization
    public ResourceEdit(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12510)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);
            m_setBools3 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_standardCleanoutGrade);
            a_reader.Read(out m_priority);
            a_reader.Read(out m_resourceSetupCost);
            a_reader.Read(out m_resourceCleanoutCost);
        }
        else if (a_reader.VersionNumber >= 12504)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_standardCleanoutGrade);
            a_reader.Read(out m_priority);
        }
        else if (a_reader.VersionNumber >= 12502)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_standardCleanoutGrade);
        }
        else if (a_reader.VersionNumber >= 12500)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                setupIncludedEnum = 9;
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_standardCleanoutGrade);
        }
        else if (a_reader.VersionNumber >= 12420)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                setupIncludedEnum = 9;
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_standardCleanoutGrade);
            a_reader.Read(out m_priority);
        }
        else if (a_reader.VersionNumber >= 12401)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_changeoverSetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_standardCleanoutGrade);
        }
        else if (a_reader.VersionNumber >= 12320)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                setupIncludedEnum = 9;
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_standardCleanoutGrade);
        }        
        else if (a_reader.VersionNumber >= 12317)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                setupIncludedEnum = 9;
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_standardCleanoutGrade);
        }
        else if (a_reader.VersionNumber >= 12302)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                setupIncludedEnum = 9;
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out string m_currentSetupCode);
            a_reader.Read(out decimal m_currentSetupNumber);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
            a_reader.Read(out m_standardCleanSpan);
            a_reader.Read(out m_standardCleanoutGrade);
        }
        else if (a_reader.VersionNumber >= 12208)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                setupIncludedEnum = 9;
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = new BaseId(a_reader);
            m_experimentalSequencingPlanIdThree = new BaseId(a_reader);
            m_experimentalSequencingPlanIdFour = new BaseId(a_reader);
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out string m_currentSetupCode);
            a_reader.Read(out decimal m_currentSetupNumber);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
        }

        #region 12104
        else if (a_reader.VersionNumber >= 12106)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                setupIncludedEnum = 9;
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdThree = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdFour = m_experimentalSequencingPlanIdOne;
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out string m_currentSetupCode);
            a_reader.Read(out decimal m_currentSetupNumber);
            a_reader.Read(out m_ganttRowHeightFactor);
            a_reader.Read(out m_minVolume);
            a_reader.Read(out m_maxVolume);
        }
        #endregion

        #region 12101
        else if (a_reader.VersionNumber >= 12101)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdThree = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdFour = m_experimentalSequencingPlanIdOne;
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out string m_currentSetupCode);
            a_reader.Read(out decimal m_currentSetupNumber);
            a_reader.Read(out m_ganttRowHeightFactor);
        }
        #endregion

        #region 12054
        else if (a_reader.VersionNumber >= 12054)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            a_reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdThree = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdFour = m_experimentalSequencingPlanIdOne;
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out string m_currentSetupCode);
            a_reader.Read(out decimal m_currentSetupNumber);
            a_reader.Read(out m_ganttRowHeightFactor);
        }
        #endregion

        #region 12038
        else if (a_reader.VersionNumber >= 12038)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out int _sortIndex);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            a_reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdThree = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdFour = m_experimentalSequencingPlanIdOne;
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out string m_currentSetupCode);
            a_reader.Read(out decimal m_currentSetupNumber);
            a_reader.Read(out m_ganttRowHeightFactor);
        }
        #endregion

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
            m_setBools2 = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            DepartmentId = new BaseId(a_reader);
            a_reader.Read(out m_workCenter);
            a_reader.Read(out m_workCenterExternalId);
            a_reader.Read(out int _sortIndex);
            a_reader.Read(out m_headStartSpan);
            a_reader.Read(out short resourceTypeEnum);
            m_resourceType = (BaseResourceDefs.resourceTypes)resourceTypeEnum;
            a_reader.Read(out short capacityTypeEnum);
            m_capacityType = (InternalResourceDefs.capacityTypes)capacityTypeEnum;
            a_reader.Read(out m_setupSpan);
            a_reader.Read(out short setupIncludedEnum);
            if (setupIncludedEnum == 8)
            {
                //UseSetupCodeTable has been removed. Change to UseAttributeCodeTable
                UseSequencedSetupTime = true;
            }
            a_reader.Read(out m_image);
            a_reader.Read(out m_standardHourlyCost);
            a_reader.Read(out m_overtimeHourlyCost);
            a_reader.Read(out m_activitySetupEfficiencyMultiplier);
            a_reader.Read(out m_cycleEfficiencyMultiplier);
            a_reader.Read(out short batchTypeEnum);
            m_batchType = (MainResourceDefs.batchType)batchTypeEnum;
            a_reader.Read(out m_batchVolume);
            a_reader.Read(out m_maxQty);
            a_reader.Read(out m_maxQtyPerCycle);
            a_reader.Read(out m_minQty);
            a_reader.Read(out m_minQtyPerCycle);
            a_reader.Read(out m_transferSpan);
            a_reader.Read(out string m_compatibilityGroup);
            a_reader.Read(out m_stage);
            a_reader.Read(out m_minNbrOfPeople);
            a_reader.Read(out m_autoJoinSpan);
            a_reader.Read(out m_maxCumulativeQty);
            a_reader.Read(out TimeSpan maxSameSetupSpan);
            a_reader.Read(out decimal obsolete_sameSetupHeadstartMultiplier);
            m_experimentalSequencingPlanIdOne = new BaseId(a_reader);
            m_experimentalSequencingPlanIdTwo = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdThree = m_experimentalSequencingPlanIdOne;
            m_experimentalSequencingPlanIdFour = m_experimentalSequencingPlanIdOne;
            m_normalSequencingPlanId = new BaseId(a_reader);
            a_reader.Read(out m_cellExternalId);
            a_reader.Read(out m_bufferSpan);
            a_reader.Read(out string m_currentSetupCode);
            a_reader.Read(out decimal m_currentSetupNumber);
            a_reader.Read(out short overlapTypeEnum);
            a_reader.Read(out m_ganttRowHeightFactor);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);
        m_setBools2.Serialize(a_writer);
        m_setBools3.Serialize(a_writer);

        PlantId.Serialize(a_writer);
        DepartmentId.Serialize(a_writer);
        a_writer.Write(m_workCenter);
        a_writer.Write(m_workCenterExternalId);
        a_writer.Write(m_headStartSpan);
        a_writer.Write((short)m_resourceType);
        a_writer.Write((short)m_capacityType);
        a_writer.Write(m_setupSpan);
        a_writer.Write(m_image);
        a_writer.Write(m_standardHourlyCost);
        a_writer.Write(m_overtimeHourlyCost);
        a_writer.Write(m_activitySetupEfficiencyMultiplier);
        a_writer.Write(m_changeoverSetupEfficiencyMultiplier);
        a_writer.Write(m_cycleEfficiencyMultiplier);
        a_writer.Write((short)m_batchType);
        a_writer.Write(m_batchVolume);
        a_writer.Write(m_maxQty);
        a_writer.Write(m_maxQtyPerCycle);
        a_writer.Write(m_minQty);
        a_writer.Write(m_minQtyPerCycle);
        a_writer.Write(m_transferSpan);
        a_writer.Write(m_stage);
        a_writer.Write(m_minNbrOfPeople);
        a_writer.Write(m_autoJoinSpan);
        a_writer.Write(m_maxCumulativeQty);
        m_experimentalSequencingPlanIdOne.Serialize(a_writer);
        m_experimentalSequencingPlanIdTwo.Serialize(a_writer);
        m_experimentalSequencingPlanIdThree.Serialize(a_writer);
        m_experimentalSequencingPlanIdFour.Serialize(a_writer);
        m_normalSequencingPlanId.Serialize(a_writer);
        a_writer.Write(m_cellExternalId);
        a_writer.Write(m_bufferSpan);
        a_writer.Write(m_ganttRowHeightFactor);

        a_writer.Write(m_minVolume);
        a_writer.Write(m_maxVolume);

        a_writer.Write(m_standardCleanSpan);
        a_writer.Write(m_standardCleanoutGrade);
        a_writer.Write(m_priority);
        a_writer.Write(m_resourceSetupCost);
        a_writer.Write(m_resourceCleanoutCost);
    }

    public new int UniqueId => 1042;
    #endregion

    public override bool HasEdits => m_setBools.AnyFlagsSet || m_setBools2.AnyFlagsSet || m_setBools3.AnyFlagsSet || base.HasEdits;

    public ResourceEdit(BaseId a_plantId, BaseId a_departmentId, BaseId a_resId)
    {
        Id = a_resId;
        PlantId = a_plantId;
        DepartmentId = a_departmentId;
        m_externalId = null;
    }

    /// <summary>
    /// The Department this resource is in.
    /// </summary>
    public BaseId DepartmentId { get; set; }

    /// <summary>
    /// The Plant this resource is in.
    /// </summary>
    public BaseId PlantId { get; set; }

    #region Shared Properties
    private string m_workCenter;

    public string WorkCenter
    {
        get => m_workCenter;
        set
        {
            m_workCenter = value;
            m_setBools[c_workCenterSet] = true;
        }
    }

    private string m_workCenterExternalId;

    public string WorkCenterExternalId
    {
        get => m_workCenterExternalId;
        set
        {
            m_workCenterExternalId = value;
            m_setBools2[c_workCenterExternalIdSet] = true;
        }
    }

    public bool DisallowDragAndDrops
    {
        get => m_bools[c_disallowDragAndDrops];
        set
        {
            m_bools[c_disallowDragAndDrops] = value;
            m_setBools[c_disallowDragAndDropsSet] = true;
        }
    }

    private TimeSpan m_headStartSpan;

    public TimeSpan HeadStartSpan
    {
        get => m_headStartSpan;
        set
        {
            m_headStartSpan = value;
            m_setBools[c_headStartSpanSet] = true;
        }
    }

    private BaseResourceDefs.resourceTypes m_resourceType;

    public BaseResourceDefs.resourceTypes ResourceType
    {
        get => m_resourceType;
        set
        {
            m_resourceType = value;
            m_setBools[c_resourceTypeSet] = true;
        }
    }

    private InternalResourceDefs.capacityTypes m_capacityType;

    public InternalResourceDefs.capacityTypes CapacityType
    {
        get => m_capacityType;
        set
        {
            m_capacityType = value;
            m_setBools[c_capacityTypeSet] = true;
        }
    }

    public bool Active
    {
        get => m_bools[c_active];
        set
        {
            m_bools[c_active] = value;
            m_setBools[c_activeSet] = true;
        }
    }

    public bool ExcludeFromGantts
    {
        get => m_bools[c_excludeFromGanttsPlan];
        set
        {
            m_bools[c_excludeFromGanttsPlan] = value;
            m_setBools[c_excludeFromGanttsSet] = true;
        }
    }

    private TimeSpan m_setupSpan;

    public TimeSpan SetupSpan
    {
        get => m_setupSpan;
        set
        {
            m_setupSpan = value;
            m_setBools[c_setupSpanSet] = true;
        }
    }

    private TimeSpan m_standardCleanSpan;

    public TimeSpan StandardCleanSpan
    {
        get => m_standardCleanSpan;
        set
        {
            m_standardCleanSpan = value;
            m_setBools2[c_cleanSpanSetIdx] = true;
        }
    }

    private int m_standardCleanoutGrade;

    public int StandardCleanoutGrade
    {
        get => m_standardCleanoutGrade;
        set
        {
            m_standardCleanoutGrade = value;
            m_setBools2[c_standardCleanoutGradeIsSetIdx] = true;
        }
    }

    public bool UseAttributeCleanouts
    {
        get => m_setBools2[c_useAttributeCleansIdx];
        set
        {
            m_setBools2[c_useAttributeCleansIdx] = value;
            m_setBools2[c_useAttributeCleanoutsIsSetIdx] = true;
        }
    }

    public bool UseOperationCleanout
    {
        get => m_setBools2[c_useOperationCleanoutIdx];
        set
        {
            m_setBools2[c_useOperationCleanoutIdx] = value;
            m_setBools2[c_useOperationCleanoutIsSetIdx] = true;
        }
    }

    public bool ConsecutiveSetupTimes
    {
        get => m_bools[c_consecutiveSetupTimes];
        set
        {
            m_bools[c_consecutiveSetupTimes] = value;
            m_setBools[c_consecutiveSetupTimesSet] = true;
        }
    }

    private string m_image;

    public string Image
    {
        get => m_image;
        set
        {
            m_image = value;
            m_setBools[c_imageSet] = true;
        }
    }

    public bool UseOperationSetupTime
    {
        get => m_bools[c_useOperationSetupTimes];
        set
        {
            m_bools[c_useOperationSetupTimes] = value;
            m_setBools[c_useOperationSetupTimesSet] = true;
        }
    }

    public bool OmitSetupOnFirstActivity
    {
        get => m_bools[c_omitSetupOnFirstActivity];
        set
        {
            m_bools[c_omitSetupOnFirstActivity] = value;
            m_setBools[c_omitSetupOnFirstActivitySet] = true;
        }
    }

    public bool OmitSetupOnFirstActivityInShift
    {
        get => m_bools[c_omitSetupOnFirstActivityInShift];
        set
        {
            m_bools[c_omitSetupOnFirstActivityInShift] = value;
            m_setBools[c_omitSetupOnFirstActivityInShiftSet] = true;
        }
    }

    private decimal m_standardHourlyCost;

    public decimal StandardHourlyCost
    {
        get => m_standardHourlyCost;
        set
        {
            m_standardHourlyCost = value;
            m_setBools[c_standardHourlyCostSet] = true;
        }
    }

    private decimal m_overtimeHourlyCost;

    public decimal OvertimeHourlyCost
    {
        get => m_overtimeHourlyCost;
        set
        {
            m_overtimeHourlyCost = value;
            m_setBools[c_overtimeHourlyCostSet] = true;
        }
    }

    private decimal m_activitySetupEfficiencyMultiplier;

    public decimal ActivitySetupEfficiencyMultiplier
    {
        get => m_activitySetupEfficiencyMultiplier;
        set
        {
            m_activitySetupEfficiencyMultiplier = value;
            m_setBools[c_activitySetupEfficiencyMultiplierSet] = true;
        }
    }

    private decimal m_changeoverSetupEfficiencyMultiplier;

    public decimal ChangeoverSetupEfficiencyMultiplier
    {
        get => m_changeoverSetupEfficiencyMultiplier;
        set
        {
            m_changeoverSetupEfficiencyMultiplier = value;
            m_setBools2[c_changeoverSetupEfficiencyMultiplierSet] = true;
        }
    }

    private decimal m_cycleEfficiencyMultiplier;

    public decimal CycleEfficiencyMultiplier
    {
        get => m_cycleEfficiencyMultiplier;
        set
        {
            m_cycleEfficiencyMultiplier = value;
            m_setBools[c_cycleEfficiencyMultiplierSet] = true;
        }
    }

    private MainResourceDefs.batchType m_batchType;

    public MainResourceDefs.batchType BatchType
    {
        get => m_batchType;
        set
        {
            m_batchType = value;
            m_setBools[c_batchTypeSet] = true;
        }
    }

    private decimal m_batchVolume;

    public decimal BatchVolume
    {
        get => m_batchVolume;
        set
        {
            m_batchVolume = value;
            m_setBools[c_batchVolumeSet] = true;
        }
    }

    private decimal m_maxQty;

    public decimal MaxQty
    {
        get => m_maxQty;
        set
        {
            m_maxQty = value;
            m_setBools[c_maxQtySet] = true;
        }
    }

    private decimal m_maxQtyPerCycle;

    public decimal MaxQtyPerCycle
    {
        get => m_maxQtyPerCycle;
        set
        {
            m_maxQtyPerCycle = value;
            m_setBools[c_maxQtyPerCycleSet] = true;
        }
    }

    private decimal m_minQty;

    public decimal MinQty
    {
        get => m_minQty;
        set
        {
            m_minQty = value;
            m_setBools[c_minQtySet] = true;
        }
    }

    private decimal m_minQtyPerCycle;

    public decimal MinQtyPerCycle
    {
        get => m_minQtyPerCycle;
        set
        {
            m_minQtyPerCycle = value;
            m_setBools[c_minQtyPerCycleSet] = true;
        }
    }

    private TimeSpan m_transferSpan;

    public TimeSpan TransferSpan
    {
        get => m_transferSpan;
        set
        {
            m_transferSpan = value;
            m_setBools[c_transferSpanSet] = true;
        }
    }
    
    private int m_stage;

    public int Stage
    {
        get => m_stage;
        set
        {
            m_stage = value;
            m_setBools[c_stageSet] = true;
        }
    }

    private decimal m_minNbrOfPeople;

    public decimal MinNbrOfPeople
    {
        get => m_minNbrOfPeople;
        set
        {
            m_minNbrOfPeople = value;
            m_setBools[c_minNbrOfPeopleSet] = true;
        }
    }

    public bool Sequential
    {
        get => m_bools[c_sequential];
        set
        {
            m_bools[c_sequential] = value;
            m_setBools2[c_sequentialSet] = true;
        }
    }

    private TimeSpan m_autoJoinSpan;

    public TimeSpan AutoJoinSpan
    {
        get => m_autoJoinSpan;
        set
        {
            m_autoJoinSpan = value;
            m_setBools2[c_autoJoinSpanSet] = true;
        }
    }

    public bool LimitAutoJoinToSameShift
    {
        get => m_bools[c_limitAutoJoinToSameShift];
        set
        {
            m_bools[c_limitAutoJoinToSameShift] = value;
            m_setBools2[c_limitAutoJoinToSameShiftSet] = true;
        }
    }

    private decimal m_maxCumulativeQty;

    public decimal MaxCumulativeQty
    {
        get => m_maxCumulativeQty;
        set
        {
            m_maxCumulativeQty = value;
            m_setBools2[c_maxCumulativeQtySet] = true;
        }
    }

    public bool ManualSchedulingOnly
    {
        get => m_bools[c_manualSchedulingOnly];
        set
        {
            m_bools[c_manualSchedulingOnly] = value;
            m_setBools2[c_manualSchedulingOnlySet] = true;
        }
    }

    public bool IsTankResource
    {
        get => m_bools[c_isTankResource];
        set
        {
            m_bools[c_isTankResource] = value;
            m_setBools2[c_isTankResourceSet] = true;
        }
    }

    private BaseId m_experimentalSequencingPlanIdOne;

    public BaseId ExperimentalSequencingPlanIdOne
    {
        get => m_experimentalSequencingPlanIdOne;
        set
        {
            m_experimentalSequencingPlanIdOne = value;
            m_setBools2[c_experimentalSequencingPlanIdOneSet] = true;
        }
    }

    private BaseId m_experimentalSequencingPlanIdTwo;

    public BaseId ExperimentalSequencingPlanIdTwo
    {
        get => m_experimentalSequencingPlanIdTwo;
        set
        {
            m_experimentalSequencingPlanIdTwo = value;
            m_setBools2[c_experimentalSequencingPlanIdTwoSet] = true;
        }
    }

    private BaseId m_experimentalSequencingPlanIdThree;

    public BaseId ExperimentalSequencingPlanIdThree
    {
        get => m_experimentalSequencingPlanIdThree;
        set
        {
            m_experimentalSequencingPlanIdThree = value;
            m_setBools2[c_experimentalSequencingPlanIdThreeSet] = true;
        }
    }

    private BaseId m_experimentalSequencingPlanIdFour;

    public BaseId ExperimentalSequencingPlanIdFour
    {
        get => m_experimentalSequencingPlanIdFour;
        set
        {
            m_experimentalSequencingPlanIdFour = value;
            m_setBools2[c_experimentalSequencingPlanIdFourSet] = true;
        }
    }

    private BaseId m_normalSequencingPlanId;

    public BaseId NormalSequencingPlanId
    {
        get => m_normalSequencingPlanId;
        set
        {
            m_normalSequencingPlanId = value;
            m_setBools2[c_normalSequencingPlanIdSet] = true;
        }
    }

    private string m_cellExternalId;

    public string CellExternalId
    {
        get => m_cellExternalId;
        set
        {
            m_cellExternalId = value;
            m_setBools2[c_cellExternalIdSet] = true;
        }
    }

    private TimeSpan m_bufferSpan;

    public TimeSpan BufferSpan
    {
        get => m_bufferSpan;
        set
        {
            m_bufferSpan = value;
            m_setBools2[c_bufferSpanSet] = true;
        }
    }

    public bool CanWorkOvertime
    {
        get => m_bools[c_canWorkOvertime];
        set
        {
            m_bools[c_canWorkOvertime] = value;
            m_setBools2[c_canWorkOvertimeSet] = true;
        }
    }

    public bool CanOffload
    {
        get => m_bools[c_canOffload];
        set
        {
            m_bools[c_canOffload] = value;
            m_setBools2[c_canOffloadSet] = true;
        }
    }

    private int m_ganttRowHeightFactor;

    public int GanttRowHeightFactor
    {
        get => m_ganttRowHeightFactor;
        set
        {
            m_ganttRowHeightFactor = value;
            m_setBools2[c_ganttRowHeightFactorSet] = true;
        }
    }

    private decimal m_minVolume;

    public decimal MinVolume
    {
        get => m_minVolume;
        set
        {
            m_minVolume = value;
            m_setBools2[c_minVolumeSetIdx] = true;
        }
    }

    private decimal m_maxVolume;

    public decimal MaxVolume
    {
        get => m_maxVolume;
        set
        {
            m_maxVolume = value;
            m_setBools2[c_maxVolumeSetIdx] = true;
        }
    }

    private int m_priority;
    /// <summary>
    /// Used to determine a priority for scheduling on this Resource
    /// </summary>
    public int Priority
    {
        get => m_priority;
        set
        {
            m_priority = value;
            m_setBools2[c_priorityIsSetIdx] = true;
        }
    }

    public bool UseResourceSetupTime
    {
        get => m_bools[c_useResourceSetupTimeIdx];
        set
        {
            m_bools[c_useResourceSetupTimeIdx] = value;
            m_setBools3[c_useResourceSetupTimeIsSetIdx] = true;
        }
    }

    public bool UseSequencedSetupTime
    {
        get => m_bools[c_useSequencedSetupTimeIdx];
        set
        {
            m_bools[c_useSequencedSetupTimeIdx] = value;
            m_setBools3[c_useSequencedSetupTimeIsSetIdx] = true;
        }
    }

    public bool UseResourceCleanout
    {
        get => m_bools[c_useResourceCleanoutIdx];
        set
        {
            m_bools[c_useResourceCleanoutIdx] = value;
            m_setBools3[c_useResourceCleanoutIsSetIdx] = true;
        }
    }

    private decimal m_resourceSetupCost;
    public decimal ResourceSetupCost
    {
        get => m_resourceSetupCost;
        set
        {
            m_resourceSetupCost = value;
            m_setBools3[c_resourceSetupCostIsSetIdx] = true;
        }
    }  
    
    private decimal m_resourceCleanoutCost;
    public decimal ResourceCleanoutCost
    {
        get => m_resourceCleanoutCost;
        set
        {
            m_resourceCleanoutCost = value;
            m_setBools3[c_resourceCleanoutCostIsSetIdx] = true;
        }
    }
    #endregion Shared properties

    #region Bool Vector
    private BoolVector32 m_bools;

    private const short c_disallowDragAndDrops = 0;

    //1 unused
    private const short c_active = 2;

    //private const short c_excludeFromCapacityPlan = 3;
    private const short c_excludeFromGanttsPlan = 4;

    //private const short c_excludeFromReports = 5;
    private const short c_consecutiveSetupTimes = 6;
    private const short c_useOperationSetupTimes = 7;
    private const short c_omitSetupOnFirstActivity = 8;
    private const short c_omitSetupOnFirstActivityInShift = 9;
    private const short c_sequential = 10;
    private const short c_limitAutoJoinToSameShift = 11;
    private const short c_manualSchedulingOnly = 12;
    private const short c_isTankResource = 13;
    //private const short c_obsoleteDrum = 14;
    private const short c_canWorkOvertime = 15;
    private const short c_canOffload = 16;
    private const short c_excludeExceptFromDepartment = 17;
    private const short c_useResourceSetupTimeIdx = 18;
    private const short c_useSequencedSetupTimeIdx = 19;
    private const short c_useResourceCleanoutIdx = 20;
    #endregion

    #region IsSetBools
    private BoolVector32 m_setBools;
    private const short c_workCenterSet = 0;

    private const short c_disallowDragAndDropsSet = 1;

    // private const short c_sortIndexSet = 2; obsolete 
    //3 unused
    private const short c_headStartSpanSet = 4;
    private const short c_resourceTypeSet = 5;
    private const short c_capacityTypeSet = 6;

    private const short c_activeSet = 7;

    //private const short c_excludeFromCapacityPlanSet = 8;
    private const short c_excludeFromGanttsSet = 9;

    //private const short c_excludeFromReportsSet = 10;
    private const short c_setupSpanSet = 11;
    //private const short c_setupIncludedSet = 12;
    private const short c_consecutiveSetupTimesSet = 13;
    private const short c_imageSet = 14;
    private const short c_useOperationSetupTimesSet = 15;
    private const short c_omitSetupOnFirstActivitySet = 16;
    private const short c_omitSetupOnFirstActivityInShiftSet = 17;
    private const short c_standardHourlyCostSet = 18;
    private const short c_overtimeHourlyCostSet = 19;
    private const short c_activitySetupEfficiencyMultiplierSet = 20;
    private const short c_cycleEfficiencyMultiplierSet = 21;
    private const short c_batchTypeSet = 22;
    private const short c_batchVolumeSet = 23;
    private const short c_maxQtySet = 24;
    private const short c_maxQtyPerCycleSet = 25;
    private const short c_minQtySet = 26;
    private const short c_minQtyPerCycleSet = 27;
    private const short c_transferSpanSet = 28;
    //private const short c_compatibilityGroupSet = 29;
    private const short c_stageSet = 30;
    private const short c_minNbrOfPeopleSet = 31;

    private BoolVector32 m_setBools2;
    private const short c_sequentialSet = 0;
    private const short c_autoJoinSpanSet = 1;
    private const short c_limitAutoJoinToSameShiftSet = 2;
    private const short c_maxCumulativeQtySet = 3;
    private const short c_changeoverSetupEfficiencyMultiplierSet = 4;


    //private const short c_sameSetupHeadstartMultiplierSet = 5;
    // no longer in use, just here as a placekeeper, repurpose as needed
    private const short c_manualSchedulingOnlySet = 6;
    private const short c_isTankResourceSet = 7;
    private const short c_experimentalSequencingPlanIdOneSet = 8;
    private const short c_normalSequencingPlanIdSet = 9;
    private const short c_cellExternalIdSet = 10;
    private const short c_bufferSpanSet = 11;
    //private const short c_obsoleteDrumSet = 12;
    private const short c_canWorkOvertimeSet = 13;
    private const short c_currentSetupCodeSet = 14;
    private const short c_currentSetupNumberSet = 15;
    private const short c_experimentalSequencingPlanIdTwoSet = 16;
    private const short c_canOffloadSet = 17;
    private const short c_workCenterExternalIdSet = 18;
    private const short c_ganttRowHeightFactorSet = 19;
    private const short c_minVolumeSetIdx = 20;
    private const short c_maxVolumeSetIdx = 21;
    private const short c_experimentalSequencingPlanIdThreeSet = 22;
    private const short c_experimentalSequencingPlanIdFourSet = 23;
    private const short c_useAttributeCleansIdx = 24;
    private const short c_cleanSpanSetIdx = 25;
    private const short c_useAttributeCleanoutsIsSetIdx = 26;
    private const short c_useOperationCleanoutIdx = 27;
    private const short c_useOperationCleanoutIsSetIdx = 28;
    private const short c_standardCleanoutGradeIsSetIdx = 29;
    private const short c_priorityIsSetIdx = 30;

    private BoolVector32 m_setBools3;
    private const short c_useResourceSetupTimeIsSetIdx = 0;
    private const short c_useSequencedSetupTimeIsSetIdx = 1;
    private const short c_useResourceCleanoutIsSetIdx = 2;
    private const short c_resourceSetupCostIsSetIdx = 3;
    private const short c_resourceCleanoutCostIsSetIdx = 4;

    public bool WorkCenterSet => m_setBools[c_workCenterSet];
    public bool DisallowDragAndDropsSet => m_setBools[c_disallowDragAndDropsSet];
    public bool HeadStartSpanSet => m_setBools[c_headStartSpanSet];
    public bool ResourceTypeSet => m_setBools[c_resourceTypeSet];
    public bool CapacityTypeSet => m_setBools[c_capacityTypeSet];
    public bool ActiveSet => m_setBools[c_activeSet];
    public bool ExcludeFromGanttsSet => m_setBools[c_excludeFromGanttsSet];
    public bool SetupSpanSet => m_setBools[c_setupSpanSet];
    public bool ConsecutiveSetupTimesSet => m_setBools[c_consecutiveSetupTimesSet];
    public bool ImageSet => m_setBools[c_imageSet];
    public bool UseOperationSetupTimeSet => m_setBools[c_useOperationSetupTimesSet];
    public bool OmitSetupOnFirstActivitySet => m_setBools[c_omitSetupOnFirstActivitySet];
    public bool OmitSetupOnFirstActivityInShiftSet => m_setBools[c_omitSetupOnFirstActivityInShiftSet];
    public bool StandardHourlyCostSet => m_setBools[c_standardHourlyCostSet];
    public bool OvertimeHourlyCostSet => m_setBools[c_overtimeHourlyCostSet];
    public bool ActivitySetupEfficiencyMultiplierSet => m_setBools[c_activitySetupEfficiencyMultiplierSet];
    public bool ChangeoverSetupEfficiencyMultiplierSet => m_setBools2[c_changeoverSetupEfficiencyMultiplierSet];
    public bool CycleEfficiencyMultiplierSet => m_setBools[c_cycleEfficiencyMultiplierSet];
    public bool BatchTypeSet => m_setBools[c_batchTypeSet];
    public bool BatchVolumeSet => m_setBools[c_batchVolumeSet];
    public bool MaxQtySet => m_setBools[c_maxQtySet];
    public bool MaxQtyPerCycleSet => m_setBools[c_maxQtyPerCycleSet];
    public bool MinQtySet => m_setBools[c_minQtySet];
    public bool MinQtyPerCycleSet => m_setBools[c_minQtyPerCycleSet];
    public bool TransferSpanSet => m_setBools[c_transferSpanSet];
    public bool StageSet => m_setBools[c_stageSet];
    public bool MinNbrOfPeopleSet => m_setBools[c_minNbrOfPeopleSet];
    public bool SequentialSet => m_setBools2[c_sequentialSet];
    public bool AutoJoinSpanSet => m_setBools2[c_autoJoinSpanSet];
    public bool LimitAutoJoinToSameShiftSet => m_setBools2[c_limitAutoJoinToSameShiftSet];
    public bool MaxCumulativeQtySet => m_setBools2[c_maxCumulativeQtySet];
    public bool ManualSchedulingOnlySet => m_setBools2[c_manualSchedulingOnlySet];
    public bool IsTankResourceSet => m_setBools2[c_isTankResourceSet];
    public bool ExperimentalSequencingPlanOneSet => m_setBools2[c_experimentalSequencingPlanIdOneSet];
    public bool ExperimentalSequencingPlanTwoSet => m_setBools2[c_experimentalSequencingPlanIdTwoSet];
    public bool ExperimentalSequencingPlanThreeSet => m_setBools2[c_experimentalSequencingPlanIdThreeSet];
    public bool ExperimentalSequencingPlanFourSet => m_setBools2[c_experimentalSequencingPlanIdFourSet];
    public bool NormalSequencingPlanSet => m_setBools2[c_normalSequencingPlanIdSet];
    public bool CellExternalIdSet => m_setBools2[c_cellExternalIdSet];
    public bool BufferSpanSet => m_setBools2[c_bufferSpanSet];
    public bool CanWorkOvertimeSet => m_setBools2[c_canWorkOvertimeSet];
    public bool CurrentSetupCodeSet => m_setBools2[c_currentSetupCodeSet];
    public bool CurrentSetupNumberSet => m_setBools2[c_currentSetupNumberSet];
    public bool CanOffloadSet => m_setBools2[c_canOffloadSet];
    public bool WorkCenterExternalIdSet => m_setBools2[c_workCenterExternalIdSet];
    public bool GanttRowHeightFactorSet => m_setBools2[c_ganttRowHeightFactorSet];
    public bool MinVolumeSet => m_setBools2[c_minVolumeSetIdx];
    public bool MaxVolumeSet => m_setBools2[c_maxVolumeSetIdx];
    public bool StandardCleanSpanSet => m_setBools2[c_cleanSpanSetIdx];
    public bool UseAttributeCleanoutsIsSet => m_setBools2[c_useAttributeCleanoutsIsSetIdx];
    public bool UseOperationCleanoutIsSet => m_setBools2[c_useOperationCleanoutIsSetIdx];
    public bool StandardCleanoutGradeIsSet => m_setBools2[c_standardCleanoutGradeIsSetIdx];
    public bool PriorityIsSet => m_setBools2[c_priorityIsSetIdx];
    public bool UseResourceSetupTimeIsSet => m_setBools3[c_useResourceSetupTimeIsSetIdx];
    public bool UseSequencedSetupTimeIsSet => m_setBools3[c_useSequencedSetupTimeIsSetIdx];
    public bool UseResourceCleanoutIsSet => m_setBools3[c_useResourceCleanoutIsSetIdx];
    public bool ResourceSetupCostIsSet => m_setBools3[c_resourceSetupCostIsSetIdx];
    public bool ResourceCleanoutCostIsSet => m_setBools3[c_resourceCleanoutCostIsSetIdx];

    #endregion

    public void Validate()
    {
        if (DepartmentId == BaseId.NULL_ID)
        {
            throw new ValidationException("4451");
        }

        if (PlantId == BaseId.NULL_ID)
        {
            throw new ValidationException("4452");
        }

        //TODO Validate max volume and other values
    }
}

public class PlantEdit : PTObjectBaseEdit, IPTSerializable
{
    #region PT Serialization
    public PlantEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12054
        if (a_reader.VersionNumber >= 12054)
        {
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_stableSpan);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_dailyOperatingExpense);
            a_reader.Read(out m_investedCapital);
            a_reader.Read(out m_bottleneckThreshold);
            a_reader.Read(out m_heavyLoadThreshold);
        }
        #endregion

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out int _);
            a_reader.Read(out m_stableSpan);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out m_dailyOperatingExpense);
            a_reader.Read(out m_investedCapital);
            a_reader.Read(out m_bottleneckThreshold);
            a_reader.Read(out m_heavyLoadThreshold);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_setBools.Serialize(a_writer);

        a_writer.Write(m_stableSpan);
        a_writer.Write(m_annualPercentageRate);
        a_writer.Write(m_dailyOperatingExpense);
        a_writer.Write(m_investedCapital);
        a_writer.Write(m_bottleneckThreshold);
        a_writer.Write(m_heavyLoadThreshold);
    }

    public new int UniqueId => 1042;
    #endregion

    public override bool HasEdits => m_setBools.AnyFlagsSet || base.HasEdits;

    public PlantEdit(BaseId a_plantId)
    {
        Id = a_plantId;
        m_externalId = null;
    }

    #region Shared Properties
    private TimeSpan m_stableSpan;

    public TimeSpan StableSpan
    {
        get => m_stableSpan;
        set
        {
            m_stableSpan = value;
            m_setBools[c_stableSpanSet] = true;
        }
    }

    private decimal m_annualPercentageRate;

    public decimal AnnualPercentageRate
    {
        get => m_annualPercentageRate;
        set
        {
            m_annualPercentageRate = value;
            m_setBools[c_annualPercentageRateSet] = true;
        }
    }

    private decimal m_dailyOperatingExpense;

    public decimal DailyOperatingExpense
    {
        get => m_dailyOperatingExpense;
        set
        {
            m_dailyOperatingExpense = value;
            m_setBools[c_dailyOperatingExpenseSet] = true;
        }
    }

    private decimal m_investedCapital;

    public decimal InvestedCapital
    {
        get => m_investedCapital;
        set
        {
            m_investedCapital = value;
            m_setBools[c_investedCapitalSet] = true;
        }
    }

    private decimal m_bottleneckThreshold;

    public decimal BottleneckThreshold
    {
        get => m_bottleneckThreshold;
        set
        {
            m_bottleneckThreshold = value;
            m_setBools[c_bottleneckThresholdSet] = true;
        }
    }

    private decimal m_heavyLoadThreshold;

    public decimal HeavyLoadThreshold
    {
        get => m_heavyLoadThreshold;
        set
        {
            m_heavyLoadThreshold = value;
            m_setBools[c_heavyLoadThresholdSet] = true;
        }
    }
    #endregion

    #region IsSetBools
    private BoolVector32 m_setBools;
    private const short c_stableSpanSet = 1;
    private const short c_annualPercentageRateSet = 2;
    private const short c_dailyOperatingExpenseSet = 3;
    private const short c_investedCapitalSet = 4;
    private const short c_bottleneckThresholdSet = 5;
    private const short c_heavyLoadThresholdSet = 5;

    public bool StableSpanSet => m_setBools[c_stableSpanSet];
    public bool AnnualPercentageRateSet => m_setBools[c_annualPercentageRateSet];
    public bool DailyOperatingExpenseSet => m_setBools[c_dailyOperatingExpenseSet];
    public bool InvestedCapitalSet => m_setBools[c_investedCapitalSet];
    public bool BottleneckThresholdSet => m_setBools[c_bottleneckThresholdSet];
    public bool HeavyLoadThresholdSet => m_setBools[c_heavyLoadThresholdSet];
    #endregion
}

public class DepartmentEdit : PTObjectBaseEdit, IPTSerializable
{
    #region PT Serialization
    public DepartmentEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12054
        if (a_reader.VersionNumber >= 12054)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            Id = new BaseId(a_reader);

            a_reader.Read(out m_frozenSpan);
        }
        #endregion

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            PlantId = new BaseId(a_reader);
            Id = new BaseId(a_reader);

            a_reader.Read(out int _);
            a_reader.Read(out m_frozenSpan);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);

        PlantId.Serialize(a_writer);
        Id.Serialize(a_writer);

        a_writer.Write(m_frozenSpan);
    }

    public new int UniqueId => 1042;
    #endregion

    public void Validate()
    {
        if (PlantId == BaseId.NULL_ID)
        {
            throw new ValidationException("4453");
        }
    }

    public override bool HasEdits => m_setBools.AnyFlagsSet || base.HasEdits;

    public BaseId PlantId { get; set; }

    #region Shared Properties
    private TimeSpan m_frozenSpan;

    public TimeSpan FrozenSpan
    {
        get => m_frozenSpan;
        set
        {
            m_frozenSpan = value;
            m_setBools[c_frozenSpanSet] = true;
        }
    }
    #endregion

    private BoolVector32 m_bools;

    #region IsSetBools
    private BoolVector32 m_setBools;

    public DepartmentEdit(BaseId a_plantId, BaseId a_departmentId)
    {
        PlantId = a_plantId;
        Id = a_departmentId;
        m_externalId = null;
    }

    private const short c_frozenSpanSet = 1;

    public bool FrozenSpanSet => m_setBools[c_frozenSpanSet];
    #endregion
}