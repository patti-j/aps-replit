using PT.APSCommon;

namespace PT.SchedulerDefinitions.PermissionTemplates;

public class PlantPermissions : IPTSerializable
{
    #region IPTSerializable
    public PlantPermissions(IReader a_reader)
    {
        m_bools = new BoolVector32(a_reader);
        m_plantId = new BaseId(a_reader);
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        m_plantId.Serialize(a_writer);
    }

    public int UniqueId => UNIQUE_ID;

    private const int UNIQUE_ID = 828;
    #endregion

    public PlantPermissions(BaseId a_plantId, bool a_schedulePlant, bool a_viewJobsInPlant, bool a_viewInventoryInPlant)
    {
        m_plantId = a_plantId;
        SchedulePlant = a_schedulePlant;
        ViewJobsInPlant = a_viewJobsInPlant;
        ViewInventoryInPlant = a_viewInventoryInPlant;
    }

    private BoolVector32 m_bools;

    private readonly short c_schedulePlantIdx = 0;
    private readonly short c_viewJobsInPlantIdx = 1;
    private readonly short c_viewInventoryInPlantWarehousesIdx = 2;

    private readonly BaseId m_plantId;

    public BaseId PlantId => m_plantId;

    public bool SchedulePlant
    {
        get => m_bools[c_schedulePlantIdx];
        set => m_bools[c_schedulePlantIdx] = value;
    }

    public bool ViewJobsInPlant
    {
        get => m_bools[c_viewJobsInPlantIdx];
        set => m_bools[c_viewJobsInPlantIdx] = value;
    }

    public bool ViewInventoryInPlant
    {
        get => m_bools[c_viewInventoryInPlantWarehousesIdx];
        set => m_bools[c_viewInventoryInPlantWarehousesIdx] = value;
    }
}