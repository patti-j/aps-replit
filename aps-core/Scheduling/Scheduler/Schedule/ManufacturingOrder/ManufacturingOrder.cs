using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.Localization;
using PT.Common.Text;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.Schedule;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Request to produce a specified qty of a specified product at a specified time.
/// </summary>
public partial class ManufacturingOrder : BaseOrder, ICloneable, IPTSerializable
{
    #region IPTSerializable Members
    internal ManufacturingOrder(IReader reader, BaseIdGenerator idGen)
        : base(reader)
    {
        if (reader.VersionNumber >= 13007) //Removed Release Date
        {
            reader.Read(out m_expectedFinishQty);
            reader.Read(out m_family);
            reader.Read(out m_productDescription);
            reader.Read(out m_productName);
            reader.Read(out m_copyRoutingFromTemplate);

            reader.Read(out m_requiredQty);
            reader.Read(out m_requestedQty);
            reader.Read(out m_uom);
            reader.Read(out m_holdReason);
            reader.Read(out m_holdUntilDateTicks);
            reader.Read(out m_hold);
            reader.Read(out m_moNeedDate); //new in 51
            reader.Read(out m_needDateTicks); //new in 51
            reader.Read(out m_shippingBufferOverrideTicks);
            reader.Read(out m_productColor);
            m_moBools = new BoolVector32(reader);

            m_breakOffSourceMoId = new BaseId(reader);

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.defaultPathId = new BaseId(reader);
            m_referenceInfo.currentPathId = new BaseId(reader);

            m_eligiblePlants = new EligiblePlantsCollection(reader);

            m_baseOperationManager = new BaseOperationManager(reader, idGen);
            m_alternatePaths = new AlternatePathsCollection(reader, this);

            reader.Read(out bool hasSuccessorMOs);
            if (hasSuccessorMOs)
            {
                m_successorMOs = new SuccessorMOArrayList(reader, this);
            }

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(reader);

            m_referenceInfo.productId = new BaseId(reader);
            m_referenceInfo.lockedPlantId = new BaseId(reader);
            m_bools = new BoolVector32(reader);

            reader.Read(out int splitUpdateModeTemp);
            m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;
            m_splitFromMOId = new BaseId(reader);
            reader.Read(out m_autoJoinGroup);
            reader.Read(out m_originalQty);
        }
        else if (reader.VersionNumber >= 13005) //Removed Release Date
        {
            reader.Read(out m_expectedFinishQty);
            reader.Read(out m_family);
            reader.Read(out string batchGroupName);
            reader.Read(out string batchDefinitionName);
            reader.Read(out m_productDescription);
            reader.Read(out m_productName);
            reader.Read(out m_copyRoutingFromTemplate);

            reader.Read(out m_requiredQty);
            reader.Read(out m_requestedQty);
            reader.Read(out m_uom);
            reader.Read(out m_holdReason);
            reader.Read(out m_holdUntilDateTicks);
            reader.Read(out m_hold);
            reader.Read(out m_moNeedDate); //new in 51
            reader.Read(out m_needDateTicks); //new in 51
            reader.Read(out m_shippingBufferOverrideTicks);
            reader.Read(out m_productColor);
            m_moBools = new BoolVector32(reader);

            m_breakOffSourceMoId = new BaseId(reader);

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.defaultPathId = new BaseId(reader);
            m_referenceInfo.currentPathId = new BaseId(reader);

            m_eligiblePlants = new EligiblePlantsCollection(reader);

            m_baseOperationManager = new BaseOperationManager(reader, idGen);
            m_alternatePaths = new AlternatePathsCollection(reader, this);

            reader.Read(out bool hasSuccessorMOs);
            if (hasSuccessorMOs)
            {
                m_successorMOs = new SuccessorMOArrayList(reader, this);
            }

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(reader);

            m_referenceInfo.productId = new BaseId(reader);
            m_referenceInfo.lockedPlantId = new BaseId(reader);
            m_bools = new BoolVector32(reader);

            reader.Read(out int splitUpdateModeTemp);
            m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;
            m_splitFromMOId = new BaseId(reader);
            reader.Read(out m_autoJoinGroup);
            reader.Read(out m_originalQty);
        }
        else if (reader.VersionNumber >= 12519)
        {
            reader.Read(out m_expectedFinishQty);
            reader.Read(out m_family);
            reader.Read(out string batchGroupName);
            reader.Read(out string batchDefinitionName);
            reader.Read(out bool m_isReleased);
            reader.Read(out m_productDescription);
            reader.Read(out m_productName);
            reader.Read(out m_copyRoutingFromTemplate);
            reader.Read(out long m_releaseDateTicks);

            reader.Read(out m_requiredQty);
            reader.Read(out m_requestedQty);
            reader.Read(out m_uom);
            reader.Read(out m_holdReason);
            reader.Read(out m_holdUntilDateTicks);
            reader.Read(out m_hold);
            reader.Read(out m_moNeedDate); //new in 51
            reader.Read(out m_needDateTicks); //new in 51
            reader.Read(out m_shippingBufferOverrideTicks);
            reader.Read(out m_productColor);
            m_moBools = new BoolVector32(reader);

            m_breakOffSourceMoId = new BaseId(reader);

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.defaultPathId = new BaseId(reader);
            m_referenceInfo.currentPathId = new BaseId(reader);

            m_eligiblePlants = new EligiblePlantsCollection(reader);

            m_baseOperationManager = new BaseOperationManager(reader, idGen);
            m_alternatePaths = new AlternatePathsCollection(reader, this);

            reader.Read(out bool hasSuccessorMOs);
            if (hasSuccessorMOs)
            {
                m_successorMOs = new SuccessorMOArrayList(reader, this);
            }

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(reader);

            m_referenceInfo.productId = new BaseId(reader);
            m_referenceInfo.lockedPlantId = new BaseId(reader);
            m_bools = new BoolVector32(reader);

            reader.Read(out int splitUpdateModeTemp);
            m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;
            m_splitFromMOId = new BaseId(reader);
            reader.Read(out m_autoJoinGroup);
            reader.Read(out m_originalQty);
        }
        else if (reader.VersionNumber >= 439)
        {
            reader.Read(out m_expectedFinishQty);
            reader.Read(out m_family);
            reader.Read(out string batchGroupName);
            reader.Read(out string batchDefinitionName);
            reader.Read(out bool m_isReleased);
            reader.Read(out m_productDescription);
            reader.Read(out m_productName);
            reader.Read(out m_copyRoutingFromTemplate);
            reader.Read(out long m_releaseDateTicks);

            reader.Read(out m_requiredQty);
            reader.Read(out m_requestedQty);
            reader.Read(out m_uom);
            reader.Read(out m_holdReason);
            reader.Read(out m_holdUntilDateTicks);
            reader.Read(out m_hold);
            reader.Read(out m_moNeedDate); //new in 51
            reader.Read(out m_needDateTicks); //new in 51
            reader.Read(out m_shippingBufferOverrideTicks);
            reader.Read(out m_productColor);
            m_moBools = new BoolVector32(reader);

            m_breakOffSourceMoId = new BaseId(reader);

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.defaultPathId = new BaseId(reader);
            m_referenceInfo.currentPathId = new BaseId(reader);

            m_eligiblePlants = new EligiblePlantsCollection(reader);

            m_baseOperationManager = new BaseOperationManager(reader, idGen);
            m_alternatePaths = new AlternatePathsCollection(reader, this);

            bool hasSuccessorMOs;
            reader.Read(out hasSuccessorMOs);
            if (hasSuccessorMOs)
            {
                m_successorMOs = new SuccessorMOArrayList(reader, this);
            }

            m_finishedPredecessorMOReleaseInfoManager = new FinishedPredecessorMOReleaseInfoManager(reader);

            m_referenceInfo.productId = new BaseId(reader);
            m_referenceInfo.lockedPlantId = new BaseId(reader);
            m_bools = new BoolVector32(reader);

            int splitUpdateModeTemp;
            reader.Read(out splitUpdateModeTemp);
            m_splitUpdateMode = (ManufacturingOrderDefs.SplitUpdateModes)splitUpdateModeTemp;
            m_splitFromMOId = new BaseId(reader);
            reader.Read(out m_autoJoinGroup);
            m_originalQty = RequiredQty;
        }

        m_holdUntilDateTicks = PTDateTime.GetValidDateTime(m_holdUntilDateTicks);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_expectedFinishQty);
        writer.Write(m_family);
        writer.Write(m_productDescription);
        writer.Write(m_productName);
        writer.Write(m_copyRoutingFromTemplate);
        writer.Write(m_requiredQty);
        writer.Write(m_requestedQty);
        writer.Write(m_uom);
        writer.Write(m_holdReason);
        writer.Write(m_holdUntilDateTicks);
        writer.Write(m_hold);
        writer.Write(m_moNeedDate);
        writer.Write(m_needDateTicks);
        writer.Write(m_shippingBufferOverrideTicks);
        writer.Write(m_productColor);
        m_moBools.Serialize(writer);

        m_breakOffSourceMoId.Serialize(writer);

        DefaultPath.Id.Serialize(writer);
        CurrentPath.Id.Serialize(writer);

        m_eligiblePlants.Serialize(writer);

        m_baseOperationManager.Serialize(writer);
        m_alternatePaths.Serialize(writer);

        if (m_successorMOs != null)
        {
            writer.Write(true);
            m_successorMOs.Serialize(writer);
        }
        else
        {
            writer.Write(false);
        }

        m_finishedPredecessorMOReleaseInfoManager.Serialize(writer);

        if (m_product == null)
        {
            BaseId.NULL_ID.Serialize(writer);
        }
        else
        {
            m_product.Id.Serialize(writer);
        }

        if (LockedPlant == null)
        {
            BaseId.NULL_ID.Serialize(writer);
        }
        else
        {
            LockedPlant.Id.Serialize(writer);
        }

        m_bools.Serialize(writer);

        writer.Write((int)m_splitUpdateMode);
        m_splitFromMOId.Serialize(writer);
        writer.Write(m_autoJoinGroup);
        writer.Write(m_originalQty);
    }

    public new const int UNIQUE_ID = 408;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo
    {
        internal BaseId defaultPathId;
        internal BaseId currentPathId;

        internal BaseId productId;
        internal BaseId lockedPlantId;
    }

    internal void RestoreReferences(Job parentJob, PlantManager plants, CapabilityManager capabilities, ScenarioDetail sd, WarehouseManager aWarehouses, ItemManager aItems)
    {
        base.RestoreReferences(sd);

        for (int i = 0; i < m_alternatePaths.Count; i++)
        {
            AlternatePath path = m_alternatePaths[i];
            if (path.Id == m_referenceInfo.defaultPathId)
            {
                DefaultPath = path;
                break;
            }
        }

        m_job = parentJob;
        OperationManager.RestoreReferences(this, plants, capabilities, aWarehouses, aItems, sd);
        AssociateOpsWithPaths(true);

        for (int i = 0; i < m_alternatePaths.Count; i++)
        {
            AlternatePath path = m_alternatePaths[i];
            if (path.Id == m_referenceInfo.currentPathId)
            {
                CurrentPath_setter = path;
                break;
            }
        }

        m_eligiblePlants.RestoreReferences(plants);
        if (m_referenceInfo.productId != BaseId.NULL_ID)
        {
            m_product = sd.ItemManager.GetById(m_referenceInfo.productId);
        }

        DetermineProduct();

        if (m_referenceInfo.lockedPlantId != BaseId.NULL_ID)
        {
            LockedPlant = plants.GetById(m_referenceInfo.lockedPlantId);
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        a_udfManager.RestoreReferences(this, UserField.EUDFObjectType.ManufacturingOrders);

        OperationManager.RestoreReferences(a_udfManager);
    }

    /// <summary>
    /// Work that can only be done after RestoreReferences() has been called on every job.
    /// </summary>
    internal void RestoreReferences2()
    {
        LinkSuccessorMOs();
        m_referenceInfo = null;
    }
    #endregion

    #region Construction
    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="job"></param>
    /// <param name="tMO"></param>
    /// <param name="machineCapabilities"></param>
    /// <param name="scenarioDetail"></param>
    /// <param name="a_isErpUpdate"></param>
    /// <param name="a_dataChanges"></param>
    /// <param name="a_udfManager"></param>
    /// <param name="a_errorReporter"></param>
    /// <param name="a_defaultValuesMo">[Nullable] For values not set in the transmission, use the default MO values</param>
    /// <param name="a_defaultActivity"></param>
    /// <param name="a_createDefaultActivity"></param>
    /// <param name="a_autoDeleteOperationAttributes"></param>
    /// <exception cref="ManufacturingOrderException"></exception>
    /// <exception cref="PTValidationException"></exception>
    internal ManufacturingOrder(BaseId id, Job job, JobT.ManufacturingOrder tMO, CapabilityManager machineCapabilities, ScenarioDetail scenarioDetail, bool a_isErpUpdate, IScenarioDataChanges a_dataChanges, UserFieldDefinitionManager a_udfManager, ISystemLogger a_errorReporter, ManufacturingOrder a_defaultValuesMo, bool a_createDefaultActivity, bool a_autoDeleteOperationAttributes, bool a_newMo = false)
        : base(id, tMO, scenarioDetail, a_udfManager, UserField.EUDFObjectType.ManufacturingOrders)
    {
        m_job = job;
        if (tMO.CanSpanPlantsSet)
        {
            CanSpanPlants = tMO.CanSpanPlants;
        }
        else if (a_defaultValuesMo != null)
        {
            CanSpanPlants = a_defaultValuesMo.CanSpanPlants;
        }

        ExpectedFinishQty = scenarioDetail.ScenarioOptions.RoundQty(tMO.ExpectedFinishQty);
        Family = tMO.Family;
        MoNeedDate = tMO.MoNeedDate;
        if (MoNeedDate) //The NeedDate might not be set, in which case the Job Need Date is used as the MO Need Date.
        {
            NeedDateTicks = tMO.NeedDate.Ticks;
        }

        ProductDescription = tMO.ProductDescription;
        ProductName = tMO.ProductName;
        ProductColor = tMO.ProductColor;

        ShippingBufferOverrideTicks = tMO.ShippingBufferOverride;

        //Hold
        if (tMO.HoldReasonSet)
        {
            HoldReason = tMO.HoldReason;
        }
        else if (a_defaultValuesMo != null)
        {
            HoldReason = a_defaultValuesMo.HoldReason;
        }

        if (tMO.HoldUntilSet)
        {
            HoldUntil = tMO.HoldUntilDate;
        }
        else if (a_defaultValuesMo != null)
        {
            HoldUntil = a_defaultValuesMo.HoldUntil;
        }

        if (tMO.HoldSet) //do last since it clears date/time if unheld.
        {
            Hold = tMO.Hold;
        }
        else if (a_defaultValuesMo != null)
        {
            Hold = a_defaultValuesMo.Hold;
        }

        RequiredQty = scenarioDetail.ScenarioOptions.RoundQty(tMO.RequiredQty);
        RequestedQty = scenarioDetail.ScenarioOptions.RoundQty(tMO.RequiredQty); //Requested Qty is equal to Required Qty unless RequiredQty is changed internally.
        OriginalQty = RequiredQty;

        if (tMO.PreserveRequiredQtySet)
        {
            PreserveRequiredQty = tMO.PreserveRequiredQty;
        }
        else if (a_defaultValuesMo != null)
        {
            PreserveRequiredQty = a_defaultValuesMo.PreserveRequiredQty;
        }

        UOM = tMO.UOM;
        if (tMO.AutoJoinGroupSet)
        {
            AutoJoinGroup = tMO.AutoJoinGroup;
        }
        else if (a_defaultValuesMo != null)
        {
            AutoJoinGroup = a_defaultValuesMo.AutoJoinGroup;
        }

        if (tMO.ResizeForStorageIsSet && ResizeForStorage != tMO.ResizeForStorage)
        {
            ResizeForStorage = tMO.ResizeForStorage;
        }

        LockToCurrentAlternatePath = tMO.LockToCurrentAlternatePath;
        SplitUpdateMode = tMO.SplitUpdateMode;

        if (tMO.CopyRoutingFromTemplate)
        {
            CopyRoutingFromTemplateMO(scenarioDetail.JobManager, tMO.ProductName, a_dataChanges);

            return;
        }

        m_baseOperationManager = new BaseOperationManager(this, scenarioDetail.IdGen);
        m_eligiblePlants.MakeEligible(scenarioDetail.PlantManager);

        if (tMO.LockedPlantExternalId != null && tMO.LockedPlantExternalId.Trim() != "")
        {
            Plant plant = scenarioDetail.PlantManager.GetByExternalId(tMO.LockedPlantExternalId);
            if (plant == null)
            {
                throw new ManufacturingOrderException("2794", new object[] { tMO.LockedPlantExternalId, job.ExternalId });
            }

            LockedPlant = plant;
        }

        m_baseOperationManager.Receive(tMO, machineCapabilities, scenarioDetail, a_isErpUpdate, a_dataChanges, a_udfManager, a_errorReporter, a_createDefaultActivity, a_autoDeleteOperationAttributes, a_newMo);

        List<KeyValuePair<int, JobT.AlternatePath>> apList = new();
        for (int alternatePathI = 0; alternatePathI < tMO.PathCount; alternatePathI++)
        {
            JobT.AlternatePath jap = tMO.GetAlternatePath(alternatePathI);
            apList.Add(new KeyValuePair<int, JobT.AlternatePath>(jap.Preference, jap));
        }

        apList.Sort(CompareAPPreferenceKVPair);

        for (int alternatePathI = 0; alternatePathI < apList.Count; alternatePathI++)
        {
            AlternatePath path = new(apList[alternatePathI].Value, m_baseOperationManager.OperationsHash, this);
            m_alternatePaths.Add(path);
            path.Id = scenarioDetail.IdGen.NextID();

            if (tMO.DefaultPath != null && tMO.DefaultPath.ExternalId == path.ExternalId)
            {
                DefaultPath = path;
            }
        }

        if (m_alternatePaths.Count <= 0)
        {
            throw new PTValidationException("2214");
        }

        AssociateOpsWithPaths(false);

        if (DefaultPath == null)
        {
            //Try regular release first since these are preferred
            for (int i = 0; i < m_alternatePaths.Count; i++)
            {
                AlternatePath path = m_alternatePaths[i];
                if (path.AutoUse == AlternatePathDefs.AutoUsePathEnum.RegularRelease)
                {
                    DefaultPath = path;
                    break;
                }
            }

            //Try offset 
            if (DefaultPath == null)
            {
                for (int i = 0; i < m_alternatePaths.Count; i++)
                {
                    AlternatePath path = m_alternatePaths[i];
                    if (path.AutoUse == AlternatePathDefs.AutoUsePathEnum.ReleaseOffsetFromDefaultPathsLatestRelease)
                    {
                        DefaultPath = path;
                        break;
                    }
                }
            }

            if (DefaultPath == null)
            {
                DefaultPath = m_alternatePaths[0];
            }
        }

        CurrentPath_setter = DefaultPath;

        //			DetermineEligiblePlants();
        m_successorMOs = new SuccessorMOArrayList(tMO.SuccessorMOs, this);
        LinkSuccessorMOs();
        CurrentPath.DoAnyAutoPredecessorFinishes(); //may have operations finished from the start

        DetermineProductAndLinkProductRules();
    }

    private static int CompareAPPreferenceKVPair(KeyValuePair<int, JobT.AlternatePath> x, KeyValuePair<int, JobT.AlternatePath> y)
    {
        if (x.Key > y.Key)
        {
            return 1;
        }

        if (x.Key < y.Key)
        {
            return -1;
        }

        return 0;
    }

    public class ManufacturingOrderException : PTValidationException
    {
        public ManufacturingOrderException(string e, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(e, a_stringParameters, a_appendHelpUrl) { }
    }

    private void CopyRoutingFromTemplateMO(JobManager jobs, string productName, IScenarioDataChanges a_dataChanges)
    {
        if (productName == "")
        {
            throw new PTValidationException("2215", new object[] { Job.Name });
        }

        //Find a source MO.
        ManufacturingOrder moToCopy = null;
        for (int i = 0; i < jobs.Count; i++)
        {
            Job job = jobs[i];
            if (job.Template) //only look in Template Jobs.
            {
                for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
                {
                    ManufacturingOrder mo = job.ManufacturingOrders[moI];
                    if (mo.ProductName == productName) //use this mo
                    {
                        moToCopy = mo;
                        break;
                    }
                }
            }
            else
            {
                continue;
            }

            if (moToCopy != null)
            {
                break;
            }
        }

        if (moToCopy == null)
        {
            throw new PTValidationException("2216", new object[] { productName, Job.Name });
        }

        //Copy eligible plants
        for (int i = 0; i < moToCopy.EligiblePlants.Count; i++)
        {
            EligiblePlants.Add(new EligiblePlant(moToCopy.EligiblePlants[i].Plant));
        }

        //Copy Operations
        m_baseOperationManager = new BaseOperationManager(this, ScenarioDetail.IdGen);
        m_baseOperationManager.Copy(ScenarioDetail.IdGen, moToCopy.OperationManager, moToCopy, this, a_dataChanges);

        //Copy the Alternate Paths
        AlternatePath currentpath = null;
        AlternatePath defaultPath = null;

        for (int i = 0; i < moToCopy.AlternatePaths.Count; i++)
        {
            AlternatePath sourcePath = moToCopy.AlternatePaths[i];
            AlternatePath newPath = new(ScenarioDetail.IdGen.NextID(), sourcePath, this);
            AlternatePaths.Add(newPath);

            if (sourcePath == moToCopy.CurrentPath)
            {
                currentpath = newPath;
            }

            if (sourcePath == moToCopy.DefaultPath)
            {
                defaultPath = newPath;
            }
        }

        AssociateOpsWithPaths(false);

        CurrentPath_setter = currentpath;
        DefaultPath = defaultPath;

        //DetermineEligiblePlants();
        m_successorMOs = new SuccessorMOArrayList(moToCopy.SuccessorMOs, this);
    }
    #endregion

    #region Object Accessors
    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private BaseOperationManager m_baseOperationManager;

    [Browsable(false)]
    public BaseOperationManager OperationManager => m_baseOperationManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly AlternatePathsCollection m_alternatePaths = new();

    [Browsable(false)]
    public AlternatePathsCollection AlternatePaths => m_alternatePaths;

    private AlternatePath m_currentPath;

    /// <summary>
    /// The Path to be used or being used currently. This is always set.
    /// </summary>
    [Browsable(false)]
    public AlternatePath CurrentPath => m_currentPath;

    public string CurrentPathName => CurrentPath.Name;

    public int CurrentPathPreference => CurrentPath.Preference;

    public int PathCount => AlternatePaths.Count;

    /// <summary>
    /// Created to make it easy to find where CurrentPath is set.
    /// </summary>
    internal AlternatePath CurrentPath_setter
    {
        set => m_currentPath = value;
    }

    private readonly EligiblePlantsCollection m_eligiblePlants = new();

    [Browsable(false)]
    public EligiblePlantsCollection EligiblePlants => m_eligiblePlants;

    private Plant m_lockedPlant;

    /// <summary>
    /// If specified then all work must be scheduled in this Plant.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    [Browsable(false)]
    public Plant LockedPlant
    {
        get => m_lockedPlant;
        internal set => m_lockedPlant = value;
    }

    /// <summary>
    /// If specified then all work must be scheduled in this Plant.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public string LockedPlantName
    {
        get
        {
            if (LockedPlant != null)
            {
                return LockedPlant.Name;
            }

            return "";
        }
    }

    /// <summary>
    /// Returns the Operation or null if there is no Block.
    /// </summary>
    public InternalOperation FindOperation(BaseId opId)
    {
        if (OperationManager.Contains(opId))
        {
            return (InternalOperation)OperationManager[opId];
        }

        return null;
    }

    /// A list of the Plants that the MO currently has Activities scheduled in.
    /// </summary>
    public List<Plant> GetScheduledPlants()
    {
        List<Plant> plants = new();
        List<InternalResource> resources = GetResourcesScheduled();
        Dictionary<BaseId, BaseId> plantsAlreadyAdded = new();
        for (int i = 0; i < resources.Count; i++)
        {
            InternalResource res = resources[i];
            if (!plantsAlreadyAdded.ContainsKey(res.PlantId))
            {
                plants.Add(res.Plant);
                plantsAlreadyAdded.Add(res.PlantId, res.PlantId);
            }
        }

        return plants;
    }
    #endregion Object Accessors

    #region Properties
    #region Boolvector
    private BoolVector32 m_bools;
    private const int c_preserveHoldDateIdx = 0;
    private const int LockToCurrentAlternatePathIdx = 1;
    private const int c_resizeForStorageIdx = 2;

    /// <summary>
    /// Set to true if the Release Date is set internally in order to prevent changes from subsequent imports.
    /// </summary>
    public bool PreserveHoldDate
    {
        get => m_bools[c_preserveHoldDateIdx];
        private set => m_bools[c_preserveHoldDateIdx] = value;
    }

    /// <summary>
    /// Whether the CurrentPath is the only path the MO can be scheduled with.
    /// </summary>
    public bool LockToCurrentAlternatePath
    {
        get => m_bools[LockToCurrentAlternatePathIdx];
        set => m_bools[LockToCurrentAlternatePathIdx] = value;
    }
    #endregion Boolvector

    /// <summary>
    /// Whether the MO must be scheduled using the current path.
    /// During an Optimize, ManufacturingOrders any of the following conditions will cause the MO to be scheduled on it's CurrentPath:
    /// • Any Progress Reported
    /// • Finished Activities
    /// • Started Activities
    /// • The MO's Locked to CurrentPath flag is set
    /// • Any blocks are Resource Locked
    /// • Any activities are Anchored
    /// • The MO is scheduled to start before the optimize start time
    /// An Expedite is similar to an Optimize with the following Exceptions:
    /// • Resource Locks won't prevent the MO from being scheduled on different Alternate Paths.
    /// • An Anchor won't restrict an MO to the Current Path if the Anchor After Expedite Setting is turned on
    /// </summary>
    /// <returns>true if CurrentPath must be used to schedule the MO.</returns>
    public bool IsLockedToCurrentPath()
    {
        return AlternatePaths.Count == 1 || LockToCurrentAlternatePath || IsLocked() || IsAnchored() || ReanchorAfterBeingExpedited;
    }

    /// <summary>
    /// Whether the MO is locked to the current path and the reasons it's locked. If the reason isn't equal to
    /// LockedToCurrentAlternatePathStateReasonEnum.Free, you can determine the applicable reasons by performing
    /// the bitwise OR operator on the return value. For instance,
    /// if((reason|LockedToCurrentAlternatePathStateReasonEnum.Anchored)!=0),
    /// then LockedToCurrentAlternatePathStateReasonEnum.Anchored is one of the reasons the MO is locked to
    /// CurrentPath.
    /// During an Optimize, ManufacturingOrders any of the following conditions will cause the MO to be scheduled on it's CurrentPath:
    /// • Any Progress Reported
    /// • Finished Activities
    /// • Started Activities
    /// • The MO's Locked to CurrentPath flag is set
    /// • Any blocks are Resource Locked
    /// • Any activities are Anchored
    /// • The MO is scheduled to start before the optimize start time
    /// An Expedite is similar to an Optimize with the following Exceptions:
    /// • Resource Locks won't prevent the MO from being scheduled on different Alternate Paths.
    /// • An Anchor won't restrict an MO to the Current Path if the Anchor After Expedite Setting is turned on
    /// </summary>
    /// <returns></returns>
    public long GetLockedToCurrentPathReasons()
    {
        long reasons = (long)ManufacturingOrderDefs.LockedToCurrentAlternatePathStateReasonEnum.Free;

        if (AlternatePaths.Count == 1)
        {
            reasons |= (long)ManufacturingOrderDefs.LockedToCurrentAlternatePathStateReasonEnum.SinglePath;
        }

        if (LockToCurrentAlternatePath)
        {
            reasons |= (long)ManufacturingOrderDefs.LockedToCurrentAlternatePathStateReasonEnum.LockToCurrentPathIsSet;
        }

        if (Started)
        {
            reasons |= (long)ManufacturingOrderDefs.LockedToCurrentAlternatePathStateReasonEnum.Started;
        }

        if (IsLocked())
        {
            reasons |= (long)ManufacturingOrderDefs.LockedToCurrentAlternatePathStateReasonEnum.ResourceLocked;
        }

        if (IsAnchored())
        {
            reasons |= (long)ManufacturingOrderDefs.LockedToCurrentAlternatePathStateReasonEnum.Anchored;
        }

        return reasons;
    }

    /// <summary>
    /// Special summary or troubleshooting information.
    /// </summary>
    [Browsable(false)]
    public override string Analysis
    {
        get
        {
            System.Text.StringBuilder analysis = new();

            analysis.Append(GetProblemAnalysis());

            if (Late)
            {
                analysis.Append(string.Format("LATE {0}".Localize(), Lateness.ToReadableStringHourPrecision()));
            }

            if (Finished)
            {
                analysis.Append(string.Format("{2}Finished.  {0} {1} expected".Localize(), ExpectedFinishQty, UOM, Environment.NewLine));
            }

            if (LockedPlant != null)
            {
                analysis.Append(string.Format("{1}Locked to Plant {0}".Localize(), LockedPlant.Name, Environment.NewLine));
            }

            ResourceOperation resOp = GetLeadOperation() as ResourceOperation;
            if (resOp != null)
            {
                analysis.Append(string.Format("{5} Lead Operation: {0} {1} scheduled on {2} to start on {3} at {4}".Localize(),
                    resOp.Name,
                    resOp.Description,
                    resOp.ResourcesUsed,
                    resOp.StartDateTime.ToDisplayTime().ToLongDateString(),
                    resOp.StartDateTime.ToDisplayTime().ToShortTimeString(),
                    Environment.NewLine));
            }

            //Get analysis for Operations
            string opnAnalysis = "";
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                if (op.Analysis != "")
                {
                    opnAnalysis += string.Format("{2}\t{0}: {1}", op.Name, op.Analysis, Environment.NewLine);
                }
            }

            if (opnAnalysis != "")
            {
                analysis.Append(Environment.NewLine + Environment.NewLine + "\t--" + "Operations".Localize() + "--" + opnAnalysis);
            }

            return analysis.ToString();
        }
    }

    public string Bottlenecks
    {
        get
        {
            System.Text.StringBuilder builder = new();
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                if (op.Bottleneck)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append("; ");
                    }

                    builder.Append(op.Name);
                    if (op.Description.Trim().Length > 0)
                    {
                        builder.Append(string.Format(" ({0})", op.Description));
                    }
                }
            }

            return builder.ToString();
        }
    }

    //public List<BaseOperation> GetBottleneckOperations()
    //{
    //    List<BaseOperation> ops = new List<BaseOperation>();

    //    IDictionaryEnumerator operationsEnumerator = this.OperationManager.OperationsHash.GetEnumerator();
    //    while (operationsEnumerator.MoveNext())
    //    {
    //        DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
    //        BaseOperation op = (BaseOperation)de.Value;
    //        if (op.Bottleneck)
    //            ops.Add(op);
    //    }
    //    return ops;
    //}

    public string MaterialBottlenecks
    {
        get
        {
            System.Text.StringBuilder builder = new();
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                if (op is InternalOperation && ((InternalOperation)op).MaterialBottleneck)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append("; ");
                    }

                    builder.Append(op.Name);
                    if (op.Description.Trim().Length > 0)
                    {
                        builder.Append(string.Format(" ({0})", op.Description));
                    }
                }
            }

            return builder.ToString();
        }
    }

    public List<MaterialRequirement.MaterialShortage> GetMaterialShortages(ScenarioDetail sd)
    {
        List<MaterialRequirement.MaterialShortage> moMaterialShortages = new();

        IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
        while (operationsEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            List<MaterialRequirement.MaterialShortage> opMaterialShortages = op.GetMaterialShortages(sd);
            for (int msI = 0; msI < opMaterialShortages.Count; msI++)
            {
                moMaterialShortages.Add(opMaterialShortages[msI]);
            }
        }

        return moMaterialShortages;
    }

    public string CapacityBottlenecks
    {
        get
        {
            System.Text.StringBuilder builder = new();
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                if (op is InternalOperation && ((InternalOperation)op).CapacityBottleneck)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append("; ");
                    }

                    builder.Append(op.Name);
                    if (op.Description.Trim().Length > 0)
                    {
                        builder.Append(string.Format(" ({0}) {1}", op.Description, ((InternalOperation)op).ResourcesUsed));
                    }
                }
            }

            return builder.ToString();
        }
    }

    private string GetProblemAnalysis()
    {
        System.Text.StringBuilder analysis = new();
        if (!EligibleResources_IsSatisfiable())
        {
            analysis.Append(string.Format("This Manufacturing Order can't be scheduled. {0}This is usually due to a Capability not having enough Active Resources linked to it.  (Your ERP system may have a WorkCenter with no Machines.){0}Another possible cause is that the Job or Manufacturing Order's CanSpanPlants setting is interfering.".Localize(), Environment.NewLine));

            AlternatePath.NodeCollection badNodes = GetUnsatisfiableNodes();
            if (badNodes.Count > 0)
            {
                string nodeProblems = Environment.NewLine + "The following Operations cannot be scheduled:".Localize() + " ";
                for (int i = 0; i < badNodes.Count; i++)
                {
                    AlternatePath.Node node = badNodes[i];
                    string nxtNodeTxt = node.Operation.Name;
                    if (i > 0)
                    {
                        nxtNodeTxt = ", " + nxtNodeTxt;
                    }

                    nodeProblems += nxtNodeTxt;
                }

                analysis.Append(nodeProblems);
            }
        }

        return analysis.ToString();
    }

    /// <summary>
    /// Gets an ArrayList of all Capabilities that have no active resources across all operations.
    /// </summary>
    /// <returns></returns>
    public SortedList<BaseId, Capability> GetCapabilitiesWithoutActiveResources()
    {
        SortedList<BaseId, Capability> problemCapabilities = new();
        IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
        while (operationsEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            if (op is InternalOperation)
            {
                SortedList<BaseId, Capability> opCaps = ((InternalOperation)op).GetCapabilitiesWithoutActiveResources();
                for (int i = 0; i < opCaps.Count; i++)
                {
                    Capability cap = opCaps.Values[i];
                    if (!problemCapabilities.ContainsKey(cap.Id))
                    {
                        problemCapabilities.Add(cap.Id, cap);
                    }
                }
            }
        }

        return problemCapabilities;
    }

    //This is so that property can be ReadOnly.
    /// <summary>
    /// If true, then the Operations can schedule in more than one plant.  Otherwise, all operations must be scheduled in only one Plant.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public override bool CanSpanPlants
    {
        get => base.CanSpanPlants;
        internal set
        {
            if (!PTSystem.LicenseKey.IncludeCrossPlantPlanning && value)
            {
                throw new AuthorizationException("2888", new object[] { Job.Name, Name });
            }

            base.CanSpanPlants = value;
        }
    }

    /// <summary>
    /// The amount of time between the earliest scheduled operation's start and the latest scheduled operation's finish.
    /// </summary>
    public TimeSpan LeadTime => ScheduledEndDate.Subtract(ScheduledStartDate);

    /// <summary>
    /// Locked Blocks cannot be moved to different Resources during Optimizations. Manual moves can be made to different Resources.
    /// </summary>
    public lockTypes Locked
    {
        get
        {
            int lockedCount = 0;
            int internalOpCount = 0;
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                if (op is InternalOperation internalOp && !internalOp.IsFinishedOrOmitted)
                {
                    internalOpCount++;
                    if (internalOp.Locked == lockTypes.SomeBlocksLocked)
                    {
                        return lockTypes.SomeBlocksLocked; //No need to go further.  If one Operation is partially locked then the MO is partially locked.
                    }

                    if (internalOp.Locked == lockTypes.Locked)
                    {
                        lockedCount++;
                    }
                }
            }

            if (lockedCount == 0)
            {
                return lockTypes.Unlocked;
            }

            if (lockedCount == internalOpCount)
            {
                return lockTypes.Locked;
            }

            return lockTypes.SomeBlocksLocked;
        }
    }

    /// <summary>
    /// Locks/Unlocks all Internal Operations in the Manufacturing Order.
    /// </summary>
    /// <param name="lock"></param>
    public void Lock(bool lockit)
    {
        IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
        while (operationsEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            if (op is InternalOperation)
            {
                ((InternalOperation)op).Lock(lockit);
            }
        }
    }

    /// <summary>
    /// Operations that are OnHold are scheduled to start after their Hold Until Date.
    /// </summary>
    public holdTypes OnHold
    {
        get
        {
            if (Hold)
            {
                return holdTypes.OnHold;
            }

            int opCount = OperationManager.OperationsHash.Count;
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                if (op.OnHold)
                {
                    return holdTypes.PartiallyOnHold;
                }
            }

            return holdTypes.Released;
        }
    }

    /// <summary>
    /// Holds/Releases all Operations in the Manufacturing Order.
    /// </summary>
    public void HoldIt(bool holdit, DateTime holdUntil, string holdReason)
    {
        HoldReason = holdReason;
        HoldUntil = holdUntil;
        Hold = holdit;
    }

    private bool m_hold;

    /// <summary>
    /// Whether the Manufacturing Order was placed On-hold and work should not be done on it.
    /// </summary>
    public bool Hold
    {
        get => m_hold;
        private set
        {
            m_hold = value;
            if (!m_hold)
            {
                HoldReason = "";
                HoldUntil = PTDateTime.MinDateTime;
            }
        }
    }

    public static string HoldReasonFieldName => "HoldReason";

    private string m_holdReason;

    /// <summary>
    /// The reason the Manufacturing Order was placed On-Hold
    /// </summary>
    public string HoldReason
    {
        get => m_holdReason;
        private set => m_holdReason = value;
    }

    private long m_holdUntilDateTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// The date and time specified when the MO was placed on hold.
    /// All Operations have their Hold Until Date set to this value as well.
    /// </summary>
    public DateTime HoldUntil
    {
        get => new(m_holdUntilDateTicks);
        private set => m_holdUntilDateTicks = SQLServerConversions.GetValidDateTime(value.Ticks);
    }

    internal long HoldUntilTicks
    {
        get => m_holdUntilDateTicks;

        set => m_holdUntilDateTicks = SQLServerConversions.GetValidDateTime(value);
    }

    /// <summary>
    /// The latest Scheduled End Date of all the Manufacturing Orders's Operations.
    /// </summary>
    internal long ScheduledEnd => CurrentPath.ScheduledFinish;

    /// <summary>
    /// Cached ScheduledEndDate value to improve performance for models with a large number of MOs.
    /// The value is cleared on SimulationInitialization. The value would only change during a simulation since the calculated fields are based on Scheduled dates
    /// This value is not serialized and as far as I can tell is not retrieved during simulation.
    /// </summary>
    private ICalculatedValueCache<DateTime> m_cachedScheduledEndDate;

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// The latest Scheduled End Date of all the Manufacturing Orders's Operations.
    /// </summary>
    public DateTime ScheduledEndDate => GetScheduledEndDate();

    public DateTime GetScheduledEndDate()
    {
        if (m_cachedScheduledEndDate != null && m_cachedScheduledEndDate.TryGetValue(out DateTime cachedValue))
        {
            return cachedValue;
        }

        DateTime endDate;
        if (Scheduled)
        {
            endDate = new DateTime(ScheduledEnd);
        }
        else
        {
            long scheduledEnd = PTDateTime.MinDateTime.Ticks;
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                if (op is InternalOperation internalOp)
                {
                    if (internalOp.GetReportedFinishDate(out long reportedEnd) && reportedEnd > scheduledEnd)
                    {
                        scheduledEnd = reportedEnd;
                    }
                }
            }

            if (scheduledEnd.Equals(PTDateTime.MinDateTime.Ticks))
            {
                endDate = PTDateTime.MaxDateTime;
            }
            else
            {
                endDate = new DateTime(scheduledEnd);
            }
        }

        m_cachedScheduledEndDate?.CacheValue(endDate);
        return endDate;
    }

    /// <summary>
    /// Returns the Activity that is scheduled with the latest End Date or null if nothing is scheduled.
    /// </summary>
    /// <returns></returns>
    public BaseActivity GetLatestScheduledActivity()
    {
        if (!Scheduled)
        {
            return null;
        }

        long scheduledEnd = PTDateTime.MinDateTime.Ticks;
        InternalActivity latestActivity = null;
        IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
        while (operationsEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
            InternalOperation op = (InternalOperation)de.Value;
            if (op.Scheduled)
            {
                InternalActivity curActivity = op.GetLatestEndingScheduledActivity();
                if (curActivity.Batch.PostProcessingEndDateTime.Ticks >= scheduledEnd)
                {
                    scheduledEnd = curActivity.Batch.PostProcessingEndTicks;
                    latestActivity = curActivity;
                }
            }
        }

        return latestActivity;
    }

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// The earliest Scheduled Start Date of all the Manufacturing Orders's Operations. If you need to to access this value multiple times, try to store the result, otherwise
    /// it will be recalculated every time the property is accessed.
    /// </summary>
    public DateTime ScheduledStartDate => GetScheduledStartDate();

    internal long GetScheduledStartTicks()
    {
        return GetScheduledStartDate().Ticks;
    }

    public DateTime GetScheduledStartDate()
    {
        if (Scheduled)
        {
            DateTime scheduledStart = PTDateTime.MaxDateTime;
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                InternalOperation internalOp;
                if (op is InternalOperation)
                {
                    internalOp = (InternalOperation)op;
                    if (internalOp.StartDateTime < scheduledStart && internalOp.StartDateTime != PTDateTime.MinDateTime)
                    {
                        scheduledStart = internalOp.StartDateTime;
                    }
                }
            }

            return scheduledStart;
        }
        else
        {
            long scheduledStart = PTDateTime.MaxValue.Ticks;
            IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
            while (operationsEnumerator.MoveNext())
            {
                DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
                BaseOperation op = (BaseOperation)de.Value;
                if (op is InternalOperation)
                {
                    InternalOperation internalOp = (InternalOperation)op;
                    if (internalOp.GetReportedStartDate(out long reportedStart) && reportedStart < scheduledStart)
                    {
                        scheduledStart = reportedStart;
                    }
                }
            }

            if (scheduledStart.Equals(PTDateTime.MaxValue.Ticks))
            {
                return PTDateTime.MinDateTime;
            }

            return new DateTime(scheduledStart, DateTimeKind.Utc);
        }
    }

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// True if the Job ends after its Need Date.
    /// </summary>
    public bool Late
    {
        get
        {
            (InternalOperation primaryProductOperation, Product _) = GetPrimaryProductAndItsOp();
            if (primaryProductOperation != null)
            {
                return primaryProductOperation.Late;
            }

            if (CurrentPath.AlternateNodeSortedList.Count == 0)
            {
                #if DEBUG
                throw new DebugException("ManufacturingOrder's CurrentPath has no nodes.");
                #endif
                return false;
            }

            AlternatePath.Node lastNode = CurrentPath.AlternateNodeSortedList.Values[CurrentPath.AlternateNodeSortedList.Count - 1];

            // This check here might be excessive since we have the if condition above that checks count
            if (lastNode.Operation == null)
            {
                #if DEBUG
                throw new DebugException("ManufacturingOrder's CurrentPath's last node does not have an operation associated with it.");
                #endif
                return false;
            }

            return lastNode.Operation.Late;

        }
    }

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// The Scheduled End Date minus the Need Date.
    /// </summary>
    public TimeSpan Lateness => ScheduledEndDate.Subtract(NeedDate);

    /// <summary>
    /// The scheduled unfinished Operation with the earliest Scheduled Start Date.
    /// </summary>
    public BaseOperation GetLeadOperation()
    {
        DateTime scheduledStart = PTDateTime.MaxDateTime;
        BaseOperation leadOperation = null;

        IDictionaryEnumerator operationsEnumerator = OperationManager.OperationsHash.GetEnumerator();
        while (operationsEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)operationsEnumerator.Current;
            BaseOperation op = (BaseOperation)de.Value;
            InternalOperation internalOp = op as InternalOperation;

            if (internalOp.Scheduled && !internalOp.Finished)
            {
                InternalActivity leadActivity = internalOp.GetLeadActivity();

                if (leadActivity != null && leadActivity.ScheduledStartDate < scheduledStart)
                {
                    leadOperation = op;
                    scheduledStart = leadActivity.ScheduledStartDate;
                }
            }
        }

        return leadOperation;
    }

    private Job m_job;

    [Browsable(false)]
    public Job Job => m_job;

    public ManufacturingOrderKey GetKey()
    {
        return new ManufacturingOrderKey(Job.Id, Id);
    }

    /// <summary>
    /// The initial path to be used. The a scheduler user may select a different path.
    /// </summary>
    public string DefaultPathName => m_defaultPath.Name;

    //TODO: Change to Double/Decimal
    /// <summary>
    /// The average Percent Finished of the Manufacturing Order's Operations weighted by their Standard Hours.
    /// This doesn't include omitted operations.
    /// </summary>
    public int PercentFinished
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            decimal schedulingHoursComplete = 0;
            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;
                if (baseOperation.IsNotOmitted)
                {
                    if (baseOperation is InternalOperation)
                    {
                        InternalOperation iOp = (InternalOperation)baseOperation;
                        schedulingHoursComplete += (decimal)iOp.PercentFinished / 100 * iOp.SchedulingHours;
                    }
                    else if (baseOperation.Finished)
                    {
                        schedulingHoursComplete += baseOperation.SchedulingHours; //no Percent Finished for other Operation types yet.
                    }
                }
            }

            decimal moSchedulingHours = SchedulingHours;
            if (moSchedulingHours > 0)
            {
                return Convert.ToInt32(Math.Floor(schedulingHoursComplete / moSchedulingHours * 100)); //When returning int, we shouldn't show 100% unless it is completely finished
            }

            return 100;
        }
    }

    public bool Started
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;

                if (baseOperation is InternalOperation iOp && iOp.Started)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Whether any non-finished activity has a started or higher production status
    /// </summary>
    public bool InProcess
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;

                if (baseOperation is InternalOperation iOp && iOp.InProcess)
                {
                    return true;
                }
            }

            return false;
        }
    }

    // !ALTERNATE_PATH!; IsLocked New fx
    /// <summary>
    /// True if at least one block is Locked.
    /// </summary>
    public bool IsLocked()
    {
        return Locked != lockTypes.Unlocked;
    }

    // !ALTERNATE_PATH!; IsAnchored new fx
    /// <summary>
    /// True if at least one block is Anchored.
    /// </summary>
    public bool IsAnchored()
    {
        return CurrentPath.IsAnchored();
    }

    /// <summary>
    /// Sum of all Operation Expected Setup Hours.
    /// </summary>
    public decimal ExpectedSetupHours
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            decimal total = 0;
            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;

                if (baseOperation is InternalOperation iOp)
                {
                    total += iOp.ExpectedSetupHours;
                }
            }

            return total;
        }
    }
    /// <summary>
    /// Sum of all Operation Expected Cleans Hours.
    /// </summary>
    public decimal ExpectedCleansHours
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            decimal total = 0;
            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;

                if (baseOperation is InternalOperation iOp)
                {
                    total += iOp.ExpectedCleanHours;
                }
            }

            return total;
        }
    }
    /// <summary>
    /// Sum of all Operation Expected Run Hours.
    /// </summary>
    public decimal ExpectedRunHours
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            decimal total = 0;
            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;

                if (baseOperation is InternalOperation iOp)
                {
                    total += iOp.ExpectedRunHours;
                }
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Reported Setup Hours for all Operations in the Current Path.
    /// </summary>
    public decimal ReportedSetupHours
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            decimal total = 0;
            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;

                if (baseOperation is InternalOperation iOp)
                {
                    total += iOp.ReportedSetupHours;
                }
            }

            return total;
        }
    }/// <summary>
     /// The sum of the Reported Cleans Hours for all Operations in the Current Path.
     /// </summary>
    public decimal ReportedCleansHours
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            decimal total = 0;
            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;

                if (baseOperation is InternalOperation iOp)
                {
                    total += iOp.ReportedCleanHours;
                }
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Reported Run Hours for all Operations in the Current Path.
    /// </summary>
    public decimal ReportedRunHours
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            decimal total = 0;
            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;

                if (baseOperation is InternalOperation iOp)
                {
                    total += iOp.ReportedRunHours;
                }
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Scheduling Hours for all Operations.
    /// This doesn't include omitted operations.
    /// </summary>
    public decimal SchedulingHours
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            decimal total = 0;
            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;
                if (baseOperation.IsNotOmitted)
                {
                    total += baseOperation.SchedulingHours;
                }
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Standard Hours for all Operations.
    /// Only the hours from the operations that haven't been omitted are included in this total.
    /// </summary>
    public decimal StandardHours
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            decimal total = 0;
            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation baseOperation = node.Operation;
                if (baseOperation.IsNotOmitted)
                {
                    total += baseOperation.StandardHours;
                }
            }

            return total;
        }
    }
    #endregion

    #region Status
    /// <summary>
    /// Whether all Operations are Finished.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public override bool Finished => CurrentPath.Finished;

    public bool IsFinishedOrOmitted => CurrentPath.All(op => op.IsFinishedOrOmitted);

    /// <summary>
    /// If the MO has been finished gets the reported finish date, which is the maximum reported finish date of all the activities of the Operations.
    /// </summary>
    /// <param name="aReportedFinishDate"></param>
    /// <returns>Whether the MO has been finished.</returns>
    internal bool GetReportedFinishDate(out long aReportedFinishDate)
    {
        return CurrentPath.GetReportedFinishDate(out aReportedFinishDate);
    }
    /// <summary>
    /// If the MO has been finished gets the reported start date, which is the earliest reported start date of all the activities of the Operations.
    /// </summary>
    /// <param name="a_reportedStartDate"></param>
    /// <returns>Whether the MO has been finished.</returns>
    internal bool GetReportedStartDate(out long a_reportedStartDate)
    {
        return CurrentPath.GetReportedStartDate(out a_reportedStartDate);
    }

    /// <summary>
    /// Call this function when an operation is finished to allow the MO to perform any special processing.
    /// </summary>
    internal void OperationFinished()
    {
        if (Finished)
        {
            NotifySuccessorMOsOfFinish();
            m_job.MOFinished();
            ScenarioDetail.ManufacturingOrdersFinished();
        }
    }

    internal void Finish() { }
    #endregion

    #region Shared Properties
    private decimal m_expectedFinishQty;

    /// <summary>
    /// Usually the same as Required Qty but production problems may result in more or less than required.  This is usually externally specified.  However, changing quantities in the Job Dialog will update
    /// this value based on Operation completion quantities and whether the setting for Deduct Scrap from Required is set.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public decimal ExpectedFinishQty
    {
        get => m_expectedFinishQty;
        internal set => m_expectedFinishQty = value;
    }

    private string m_family = "";

    /// <summary>
    /// For display only.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public string Family
    {
        get => m_family;
        private set => m_family = value;
    }

    private bool m_moNeedDate;

    /// <summary>
    /// If true then the Need Date can is set explicityly for the M.O..  Otherwise, the M.O. Need Date is set to the Job Need Date.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public bool MoNeedDate
    {
        get => m_moNeedDate;
        internal set => m_moNeedDate = value;
    }

    private long m_needDateTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// The date and time when the M.O. should be finished to be considered on-time.
    /// If MoNeedDate is true then this is the value stored for the M.O..  Otherwise, this is the same as the Job Need Date.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public DateTime NeedDate => new(GetNeedDateTicksOfMOForScheduling());

    //Tracks whether the need date has been updated for this simulation.
    private bool m_subComponentNeedDateSet;

    /// <summary>
    /// Update MO need date based on new demand date. Follows a similar pattern to how Job NeedDates are updated.
    /// </summary>
    /// <returns>Whether NeedDate was set</returns>
    internal bool UpdateSubMoNeedDate(long a_newNeedDateTicks)
    {
        if (m_subComponentNeedDateSet)
        {
            if (a_newNeedDateTicks < NeedDateTicks)
            {
                NeedDateTicks = a_newNeedDateTicks;
                return true;
            }
        }
        else
        {
            m_subComponentNeedDateSet = true;
            if (NeedDateTicks != a_newNeedDateTicks)
            {
                NeedDateTicks = a_newNeedDateTicks;
                return true;
            }
        }

        return false;
    }

    internal long GetNeedDateTicksOfMOForScheduling()
    {
        return CalculateNeedDateTicksOfMOForScheduling();
    }

    private long CalculateNeedDateTicksOfMOForScheduling()
    {
        if (m_moNeedDate)
        {
            return m_needDateTicks;
        }

        //Use Successor MOs to drive the Need Date if there are any.  This way it's similar to the way JIT dates are calculated within one MO.
        if (SuccessorMOs.Count > 0)
        {
            long tightestSucMoJITDateTicks = GetEarliestSuccessorJITDateTicks();
            if (tightestSucMoJITDateTicks != PTDateTime.MaxDateTimeTicks)
            {
                return tightestSucMoJITDateTicks;
            }

            return Job.NeedDateTicks;
        }

        return Job.NeedDateTicks;
    }

    internal long NeedDateTicks
    {
        get => m_needDateTicks;

        set => m_needDateTicks = SQLServerConversions.GetValidDateTime(value);
    }

    /// <summary>
    /// Returns the earliest JIT start of all successor MOs or successor MO Operations.
    /// Returns PTMaxDate if there are no scheduled successor MOs.
    /// </summary>
    /// <returns></returns>
    private long GetEarliestSuccessorJITDateTicks()
    {
        long earliestStartTicks = PTDateTime.MaxDateTimeTicks;
        for (int i = 0; i < SuccessorMOs.Count; i++)
        {
            SuccessorMO sucMo = SuccessorMOs[i];
            //May be tied to a specific Op or to the MO
            if (sucMo.Operation != null)
            {
                if (sucMo.Operation.DbrJitStartDate.Ticks < earliestStartTicks)
                {
                    earliestStartTicks = sucMo.Operation.DbrJitStartDate.Ticks - sucMo.TransferSpan;
                }
            }
            else if (sucMo.SuccessorManufacturingOrder != null) //can be null when the Job is being created.
            {
                BaseOperation op = sucMo.SuccessorManufacturingOrder.GetLeadOperation();
                if (op != null && op.DbrJitStartDate.Ticks < earliestStartTicks)
                {
                    earliestStartTicks = op.DbrJitStartDate.Ticks - sucMo.TransferSpan;
                }
            }
        }

        return earliestStartTicks;
    }

    /// <summary>
    /// If set, this is the shipping buffer of the order.
    /// </summary>
    private long? m_shippingBufferOverrideTicks;

    /// <summary>
    /// If set, this is the shipping buffer of the order.
    /// </summary>
    internal long? ShippingBufferOverrideTicks
    {
        get => m_shippingBufferOverrideTicks;
        private set => m_shippingBufferOverrideTicks = value;
    }

    /// <summary>
    /// If specified, this is the shipping buffer of the order. Returns a Timespan? which will contain a value if this field has been set.
    /// </summary>
    public TimeSpan? ShippingBufferOverride
    {
        get
        {
            if (m_shippingBufferOverrideTicks.HasValue)
            {
                return new TimeSpan(m_shippingBufferOverrideTicks.Value);
            }

            return null;
        }
    }

    private Item m_product;

    /// <summary>
    /// Call this function after a manufacturing order has been created or updated. It will attempt to find a product in the operation list
    /// that will serve as the product that the operation makes.
    /// Also sets the operations up for product rules
    /// </summary>
    private void DetermineProduct()
    {
        Product product = GetPrimaryProduct();
        m_product = product?.Item;
    }

    /// <summary>
    /// The Primary Product of the Current Path or Null
    /// </summary>
    /// <returns></returns>
    public Product GetPrimaryProduct()
    {
        return CurrentPath.GetPrimaryProduct();
    }

    public Tuple<InternalOperation, Product> GetPrimaryProductAndItsOp()
    {
        return CurrentPath.GetPrimaryProductAndItsOp();
    }

    private void DetermineProductAndLinkProductRules()
    {
        DetermineProduct();
    }

    /// <summary>
    /// The Item produced by the Manufacturing Order.  This is null if no Operations make any Products or if the Operations make more than one Product.
    /// This is used by the Resource Product Rules tables.
    /// </summary>
    [Browsable(false)]
    public Item Product => m_product;

    /// <summary>
    /// DateTime when the primary product is first in shortage
    /// </summary>
    public DateTime FirstShortage
    {
        get
        {
            DateTime shortageDate;
            Product primaryProduct = GetPrimaryProduct();
            if (primaryProduct != null && primaryProduct.Inventory != null)
            {
                bool shortageWithinLeadTime;
                decimal shortageAmount;
                primaryProduct.Inventory.GetFirstShortage(out shortageDate, out shortageAmount, ScenarioDetail, true, out shortageWithinLeadTime);
            }
            else
            {
                return DateTime.MaxValue;
            }

            return shortageDate;
        }
    }

    /// <summary>
    /// DateTime when the primary product is first in shortage without taking forecasts into account
    /// </summary>
    public DateTime FirstShortageExcludingForecasts
    {
        get
        {
            DateTime shortageDate;
            Product primaryProduct = GetPrimaryProduct();
            if (primaryProduct != null && primaryProduct.Inventory != null)
            {
                bool shortageWithinLeadTime;
                decimal shortageAmount;
                primaryProduct.Inventory.GetFirstShortage(out shortageDate, out shortageAmount, ScenarioDetail, false, out shortageWithinLeadTime);
            }
            else
            {
                return DateTime.MaxValue;
            }

            return shortageDate;
        }
    }

    /// <summary>
    /// The remaining days from the APS clock until the primary product will be in shortage
    /// </summary>
    public decimal DaysOfStock
    {
        get
        {
            Product primaryProduct = GetPrimaryProduct();
            if (primaryProduct != null && primaryProduct.Inventory != null)
            {
                return primaryProduct.Inventory.DaysOfStock(ScenarioDetail);
            }

            return 0;
        }
    }

    /// <summary>
    /// The remaining days from the APS clock until the primary product will be in shortage without taking forecasts into account
    /// </summary>
    public decimal DaysOfStockExcludingForecasts
    {
        get
        {
            Product primaryProduct = GetPrimaryProduct();
            if (primaryProduct != null && primaryProduct.Inventory != null)
            {
                DateTime shortageDate;
                decimal shortageAmount;
                bool shortageWithinLeadTime;
                bool shortage = primaryProduct.Inventory.GetFirstShortage(out shortageDate, out shortageAmount, ScenarioDetail, false, out shortageWithinLeadTime);
                if (shortage)
                {
                    return (decimal)(shortageDate - ScenarioDetail.ClockDate).TotalDays;
                }

                return (decimal)(ScenarioDetail.GetPlanningHorizonEnd() - ScenarioDetail.ClockDate).TotalDays;
            }

            return 0;
        }
    }

    private string m_productDescription = "";

    /// <summary>
    /// Description of the product being made.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public string ProductDescription
    {
        get
        {
            if (m_product != null)
            {
                return m_product.Description;
            }

            return m_productDescription;
        }
        private set => m_productDescription = value;
    }

    private string m_productName = "";

    /// <summary>
    /// Name of the product being made.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public string ProductName
    {
        get
        {
            if (m_product != null)
            {
                return m_product.Name;
            }

            return m_productName;
        }
        internal set => m_productName = value;
    }

    private System.Drawing.Color m_productColor = System.Drawing.Color.Empty;

    /// <summary>
    /// Can be used to visually indicate the Product being made.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public System.Drawing.Color ProductColor
    {
        get => m_productColor;
        internal set => m_productColor = value;
    }

    private decimal m_requiredQty;

    /// <summary>
    /// The target quantity of good Product to be made.
    /// Don't use the setter of this function unless you plan on setting the operation and activities quantities manually.
    /// Instead use SetRequiredQty(decimal).
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public decimal RequiredQty
    {
        get => m_requiredQty;

        internal set => m_requiredQty = value;
    }


    private decimal m_originalQty;

    /// <summary>
    /// The original quantity of good Product to be made.
    /// This is set when the Manufacturing Order gets resized to fill Storage
    /// </summary>
    public decimal OriginalQty
    {
        get => m_originalQty;

        internal set => m_originalQty = value;
    }

    private string m_uom = "units".Localize();

    /// <summary>
    /// Unit of measure.  For display only.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
    public string UOM
    {
        get => m_uom;
        private set => m_uom = value;
    }

    private string m_autoJoinGroup;

    /// <summary>
    /// Specifies which Manufacturing Orders can be AutoJoined.  They must have non-blank, matching AutoJoinGroup values.
    /// </summary>
    public string AutoJoinGroup
    {
        get => m_autoJoinGroup;
        set => m_autoJoinGroup = value;
    }

    private AlternatePath m_defaultPath;

    /// <summary>
    /// The initial path to be used. The user may select a different path.
    /// </summary>
    [Browsable(false)]
    public AlternatePath DefaultPath
    {
        get => m_defaultPath;
        internal set => m_defaultPath = value;
    }

    private bool m_copyRoutingFromTemplate;

    /// <summary>
    /// If true then any Path information is ignored.  Instead, the Path information for an existing Template Job is copied and used.
    /// The Product Name specifies which Product to copy the Paths from.  If there is more than one Template for the same Product then the first one found by the system will be used.			///
    /// Setting this to true on an existing Manufacturing Order will cause any current routing information to be lost and copied in from a Template when the Manufacturing Order is saved.
    /// </summary>
    public bool CopyRoutingFromTemplate
    {
        get => m_copyRoutingFromTemplate;
        private set => m_copyRoutingFromTemplate = value;
    }

    /// <summary>
    /// The total number of Operations.
    /// </summary>
    public int OperationCount => OperationManager.Count;

    /// <summary>
    /// The total number of Finished Operations.
    /// </summary>
    public int FinishedOperationCount => OperationManager.FinishedOperationCount;

    #region ResizeForStorage
    /// <summary>
    /// Whether the Manufacturing Order was resized to fill Storage
    /// </summary>
    public bool Resized => RequiredQty != OriginalQty;

    public bool ResizeForStorage
    {
        get { return m_bools[c_resizeForStorageIdx]; }
        set { m_bools[c_resizeForStorageIdx] = value; }
    }
    #endregion

    #endregion Shared Properties

    #region Transmission functionality
    internal void ChangeRequiredQty(decimal newQty)
    {
        //If the operations have the same qty as the mo qty then reset them.  Otherwise we need new logic to calculate the qties.
        IDictionaryEnumerator hashEnumerator = m_baseOperationManager.OperationsHash.GetEnumerator();

        while (hashEnumerator.MoveNext())
        {
            BaseOperation op = (BaseOperation)hashEnumerator.Value;
            if (op.RequiredFinishQty == RequiredQty) //otherwise we don't know how they've set the op qties since they're independent
            {
                if (op is InternalOperation)
                {
                    ((InternalOperation)op).UpdateRequiredFinishQuantity(newQty);
                }
                else
                {
                    op.RequiredFinishQty = newQty;
                }
            }
        }

        //Now set the mo qty
        RequiredQty = newQty;
    }

    private decimal m_requestedQty;

    /// <summary>
    /// This is the Required Qty as specified from the ERP imported values.  This is usually the same as the Required Qty unless the
    /// Required Qty has been adjusted internally or PreserveRequiredQty is true.
    /// </summary>
    public decimal RequestedQty
    {
        get => m_requestedQty;
        internal set => m_requestedQty = value;
    }

    internal void Receive(ManufacturingOrderIdBaseT t, ProductRuleManager a_productRuleManager, IScenarioDataChanges a_dataChanges)
    {
        if (t is OperationIdBaseT)
        {
            OperationIdBaseT opT = (OperationIdBaseT)t;
            BaseOperation op = OperationManager[opT.operationId];
            if (t is OperationDeleteT)
            {
                DeleteOperation(op, a_dataChanges);
            }
            else
            {
                op.Receive(opT, a_productRuleManager, a_dataChanges);
                if (opT is InternalActivityFinishT)
                {
                    CurrentPath.DoAnyAutoPredecessorFinishes();
                }
            }
        }
    }

    private void DeleteOperation(BaseOperation a_op, IScenarioDataChanges a_dataChanges)
    {
        if (OperationManager.Count == 1)
        {
            throw new PTException("At least one Operation is required".Localize());
        }

        AlternatePath path = FindPathForOperation(a_op);
        OperationManager.ClearOpPathNodeAssociations();
        OperationManager.Remove(ScenarioDetail.PlantManager, a_op, a_dataChanges);
        AssociateOpsWithPaths(true);
    }

    internal bool Receive(InternalActivityUpdateT.ActivityStatusUpdate update, IScenarioDataChanges a_dataChanges)
    {
        if (update.OperationExternalIdSet)
        {
            BaseOperation op = OperationManager[update.OperationExternalId];
            if (op == null)
            {
                throw new PTValidationException("2217", new object[] { update.OperationExternalId, ExternalId });
            }

            if (op is InternalOperation)
            {
                bool updated = ((InternalOperation)op).Receive(update, a_dataChanges);
                CurrentPath.DoAnyAutoPredecessorFinishes();
                return updated;
            }
            else
            {
                throw new PTValidationException("2218", new object[] { update.OperationExternalId });
            }
        }
        else
        {
            throw new PTValidationException("2219"); //JMC TODO
        }
    }

    // !ALTERNATE_PATH!; This is where LockToCurrentAlternatePath is set when a ScenarioDetailAlternatePathLockT is sent.
    /// <summary>
    /// Set of clear LockToCurrentAlternatePath. Returns true if the lock results in a situation where the ManufacturingOrder should be unscheduled.
    /// </summary>
    /// <param name="a_t"></param>
    /// <returns></returns>
    internal void Receive(ScenarioDetailAlternatePathLockT a_t)
    {
        AlternatePath path = AlternatePaths.FindById(a_t.CurrentAlternatePathId);

        if (path == null)
        {
            throw new PTValidationException("2220");
        }

        if (CurrentPath != path)
        {
            throw new PTValidationException("2221");
        }

        if (LockToCurrentAlternatePath != a_t.Lock)
        {
            LockToCurrentAlternatePath = a_t.Lock;
        }
    }

    internal bool Update(UserFieldDefinitionManager a_udfManager, PlantManager a_plantManager, bool a_erpUpdate, JobT.ManufacturingOrder a_jobTmo, ManufacturingOrder a_tempMO, ScenarioOptions a_options, bool a_alternatePathChange, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        bool updated = base.Update(a_jobTmo, a_t, a_udfManager, UserField.EUDFObjectType.ManufacturingOrders);
        bool isScheduled = Scheduled;
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;

        if (CanSpanPlants != a_tempMO.CanSpanPlants)
        {
            CanSpanPlants = a_tempMO.CanSpanPlants;
            if (!CanSpanPlants)
            {
                if (isScheduled)
                {
                    List<Plant> scheduledPlants = GetScheduledPlants();
                    if (scheduledPlants.Count > 1)
                    {
                        Job.Unschedule();
                    }
                }
            }
            else
            {
                flagProductionChanges = true;
            }

            updated = true;
        }

        if (ExpectedFinishQty != a_tempMO.ExpectedFinishQty)
        {
            ExpectedFinishQty = a_options.RoundQty(a_tempMO.ExpectedFinishQty);
            updated = true;
        }

        if (Family != a_tempMO.Family)
        {
            Family = a_tempMO.Family;
            updated = true;
        }

        if (MoNeedDate != a_tempMO.MoNeedDate)
        {
            MoNeedDate = a_tempMO.MoNeedDate;
            a_dataChanges.FlagJitChanges(Job.Id);
            updated = true;
        }

        if (NeedDate != a_tempMO.NeedDate)
        {
            NeedDateTicks = a_tempMO.NeedDate.Ticks;
            a_dataChanges.FlagJitChanges(Job.Id);
            updated = true;
        }

        //Hold
        if (HoldReason != a_tempMO.HoldReason && (!PreserveHoldDate || !a_erpUpdate))
        {
            HoldReason = a_tempMO.HoldReason;
            updated = true;
        }

        if (OnHold != a_tempMO.OnHold && (!PreserveHoldDate || !a_erpUpdate))
        {
            Hold = a_tempMO.Hold;
            updated = true;
            if (Hold)
            {
                flagConstraintChanges = true;
            }
            else
            {
                flagProductionChanges = true;
            }
        }

        if (Hold && HoldUntil != a_tempMO.HoldUntil && (!PreserveHoldDate || !a_erpUpdate))
        {
            if (m_holdUntilDateTicks < a_tempMO.m_holdUntilDateTicks)
            {
                m_holdUntilDateTicks = a_tempMO.m_holdUntilDateTicks;
                updated = true;

                if (isScheduled && ScheduledStartDate.Ticks < m_holdUntilDateTicks)
                {
                    flagConstraintChanges = true;
                }
            }
            else if (m_holdUntilDateTicks > a_tempMO.m_holdUntilDateTicks)
            {
                m_holdUntilDateTicks = a_tempMO.m_holdUntilDateTicks;
                updated = true;
            }

            if (!a_erpUpdate && !PreserveHoldDate)
            {
                PreserveHoldDate = true;
                updated = true;
            }
        }

        if (ProductDescription != a_tempMO.ProductDescription)
        {
            ProductDescription = a_tempMO.ProductDescription;
            updated = true;
        }

        if (ProductName != a_tempMO.ProductName)
        {
            ProductName = a_tempMO.ProductName;
            updated = true;
            flagConstraintChanges = true;
        }

        if (ProductColor != a_tempMO.ProductColor)
        {
            ProductColor = a_tempMO.ProductColor;
            updated = true;
        }

        if (ShippingBufferOverrideTicks != a_tempMO.ShippingBufferOverrideTicks)
        {
            ShippingBufferOverrideTicks = a_tempMO.ShippingBufferOverrideTicks;
            updated = true;
            a_dataChanges.FlagJitChanges(Job.Id);
        }
        
        //Set the value for PreserveRequiredQty before the logic to set the RequiredQty since it depends on PreserveRequiredQty.
        if (PreserveRequiredQty != a_tempMO.PreserveRequiredQty)
        {
            PreserveRequiredQty = a_tempMO.PreserveRequiredQty;
            updated = true;
        }

        if (RequiredQty != a_tempMO.RequiredQty)
        {
            if (!(a_erpUpdate && PreserveRequiredQty))
            {
                //Changing the Required Qty will scale the whole MO. If it was resized by the Resize for Storage feature, we want to reset the Original Qty 
                RequiredQty = a_options.RoundQty(a_tempMO.RequiredQty);
                OriginalQty = RequiredQty;
                updated = true;
                flagProductionChanges = true;
                a_dataChanges.FlagEligibilityChanges(Job.Id);
            }
        }

        if (a_erpUpdate && RequestedQty != a_tempMO.RequiredQty)
        {
            RequestedQty = a_options.RoundQty(a_tempMO.RequiredQty); //this is used to track the qty specified by the import at all times.
            updated = true;
        }


        if (UOM != a_tempMO.UOM)
        {
            UOM = a_tempMO.UOM;
            updated = true;
        }

        if (AutoJoinGroup != a_tempMO.AutoJoinGroup)
        {
            AutoJoinGroup = a_tempMO.AutoJoinGroup;
            updated = true;
        }

        if (ResizeForStorage != a_tempMO.ResizeForStorage)
        {
            ResizeForStorage = a_tempMO.ResizeForStorage;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        if (a_jobTmo.CopyRoutingFromTemplate)
        {
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            return updated;
        }

        //Update Operations
        if (OperationManager.Update(a_udfManager, ScenarioDetail.PlantManager, a_tempMO.OperationManager, a_jobTmo, m_job.ScenarioDetail, a_erpUpdate, a_t, a_dataChanges))
        {
            updated = true;
        }

        //Update AlternatePaths
        if (a_alternatePathChange)
        {
            if (AlternatePaths.Update(a_tempMO.AlternatePaths, this, a_dataChanges))
            {
                a_dataChanges.FlagEligibilityChanges(Job.Id);
                updated = true;
            }
        }
        else
        {
            if (m_alternatePaths.Update(a_tempMO.AlternatePaths, a_dataChanges))
            {
                a_dataChanges.FlagEligibilityChanges(Job.Id);
                updated = true;
            }
        }

        DetermineProductAndLinkProductRules();

        m_successorMOs.Update(this, a_tempMO.m_successorMOs, ScenarioDetail, a_dataChanges);
        LinkSuccessorMOs();

        if (DefaultPath.ExternalId != a_tempMO.DefaultPath.ExternalId)
        {
            DefaultPath = AlternatePaths.FindByExternalId(a_tempMO.DefaultPath.ExternalId);
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        AssociateOpsWithPaths(false);

        if (CurrentPath.ExternalId != a_tempMO.CurrentPath.ExternalId && !isScheduled)
        {
            CurrentPath_setter = AlternatePaths.FindByExternalId(a_tempMO.CurrentPath.ExternalId);
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
        }

        // Update OperationStatuses on activities in all paths
        for (int i = 0; i < AlternatePaths.Count; i++)
        {
            AlternatePath ap = AlternatePaths[i];
            updated |= ap.UpdateOperationStatuses(a_erpUpdate, a_tempMO.AlternatePaths.FindByExternalId(AlternatePaths[i].ExternalId), a_options, a_jobTmo, a_dataChanges);
            ap.StatusesUpdated(); // Performed in case some Finished operation's status is changed to an unfinished state which might affect the EffectiveLeaves. For bug 4823 and it's related bugs; omitted and finished & ineligible operations cause jobs to not schedule.
        }

        if (a_tempMO.LockedPlant == null && LockedPlant != null) //unlocking
        {
            LockedPlant = null;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }
        else if (LockedPlant != a_tempMO.LockedPlant && a_tempMO.LockedPlant != null)
        {
            //Check to see if the MO is scheduled only in this new Plant.  If not then it needs to be rescheduled.
            bool foundDifferentPlant = false;
            for (int i = 0; i < CurrentPath.NodeCount; i++)
            {
                AlternatePath.Node node = CurrentPath.GetNodeByIndex(i);
                if (node.Operation is InternalOperation)
                {
                    InternalOperation iOp = (InternalOperation)node.Operation;
                    List<InternalResource> resources = iOp.GetResourcesScheduled();
                    for (int resI = 0; resI < resources.Count; resI++)
                    {
                        if (resources[resI].Department.Plant.Id != a_tempMO.LockedPlant.Id)
                        {
                            m_job.Unschedule();
                            foundDifferentPlant = true;
                            break;
                        }
                    }
                }

                if (foundDifferentPlant)
                {
                    a_dataChanges.FlagEligibilityChanges(Job.Id);
                    break;
                }
            }

            LockedPlant = a_tempMO.LockedPlant;
            updated = true;
        }

        if (a_tempMO.LockToCurrentAlternatePath != LockToCurrentAlternatePath)
        {
            LockToCurrentAlternatePath = a_tempMO.LockToCurrentAlternatePath;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        if (a_tempMO.SplitUpdateMode != SplitUpdateMode)
        {
            SplitUpdateMode = a_tempMO.SplitUpdateMode;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        if (isScheduled)
        {
            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(Job.Id);
            }

            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(Job.Id);
            }
        }
        
        return updated;
    }

    public bool Edit(ScenarioDetail a_sd, ManufacturingOrderEdit a_edit, IScenarioDataChanges a_dataChanges)
    {
        bool updated = base.Edit(a_edit);

        if (a_edit.CanSpanPlantsSet && CanSpanPlants != a_edit.CanSpanPlants)
        {
            CanSpanPlants = a_edit.CanSpanPlants;
            updated = true;
        }

        if (a_edit.ExpectedFinishQtySet && ExpectedFinishQty != a_edit.ExpectedFinishQty)
        {
            ExpectedFinishQty = a_sd.ScenarioOptions.RoundQty(a_edit.ExpectedFinishQty);
            updated = true;
        }

        if (a_edit.FamilySet && Family != a_edit.Family)
        {
            Family = a_edit.Family;
            updated = true;
        }

        if (a_edit.MoNeedDateSet && MoNeedDate != a_edit.MoNeedDate)
        {
            MoNeedDate = a_edit.MoNeedDate;
            a_dataChanges.FlagJitChanges(Job.Id);
            updated = true;
        }

        if (a_edit.NeedDateSet && NeedDate != a_edit.NeedDate)
        {
            NeedDateTicks = a_edit.NeedDate.Ticks;
            a_dataChanges.FlagJitChanges(Job.Id);
            updated = true;
        }

        //Hold
        if (a_edit.HoldReasonSet && HoldReason != a_edit.HoldReason)
        {
            HoldReason = a_edit.HoldReason;
            updated = true;
        }

        if (a_edit.HoldUntilSet && HoldUntil != a_edit.HoldUntil)
        {
            HoldUntil = a_edit.HoldUntil;
            a_dataChanges.FlagConstraintChanges(Job.Id);
            updated = true;
        }

        if (a_edit.HoldSet && Hold != a_edit.Hold)
        {
            Hold = a_edit.Hold;
            a_dataChanges.FlagConstraintChanges(Job.Id);
            updated = true;
        }

        if (a_edit.ProductDescriptionSet && ProductDescription != a_edit.ProductDescription)
        {
            ProductDescription = a_edit.ProductDescription;
            updated = true;
        }

        if (a_edit.ProductNameSet && ProductName != a_edit.ProductName)
        {
            ProductName = a_edit.ProductName;
            a_dataChanges.FlagConstraintChanges(Job.Id);
            updated = true;
        }

        if (a_edit.ProductColorSet && ProductColor != a_edit.ProductColor)
        {
            ProductColor = a_edit.ProductColor;
            updated = true;
        }

        if (a_edit.ShippingBufferOverrideTicksSet && ShippingBufferOverrideTicks != a_edit.ShippingBufferOverrideTicks)
        {
            ShippingBufferOverrideTicks = a_edit.ShippingBufferOverrideTicks;
            a_dataChanges.FlagJitChanges(Job.Id);
            updated = true;
        }
        
        //Set the value for PreserveRequiredQty before the logic to set the RequiredQty since it depends on PreserveRequiredQty.
        if (a_edit.PreserveRequiredQtySet && PreserveRequiredQty != a_edit.PreserveRequiredQty)
        {
            PreserveRequiredQty = a_edit.PreserveRequiredQty;
            updated = true;
        }

        if (a_edit.RequiredQtySet && RequiredQty != a_edit.RequiredQty)
        {
            SetRequiredQty(a_sd.Clock, a_sd.ScenarioOptions.RoundQty(a_edit.RequiredQty), a_sd.ProductRuleManager);
            a_dataChanges.FlagProductionChanges(Job.Id);
            
            // Qty changes can invalidate routing/resources, so also flag eligibility.
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        if (a_edit.RequestedQtySet && RequestedQty != a_edit.RequestedQty)
        {
            RequestedQty = a_sd.ScenarioOptions.RoundQty(a_edit.RequestedQty);
            updated = true;
        }


        if (a_edit.UOMSet && UOM != a_edit.UOM)
        {
            UOM = a_edit.UOM;
            updated = true;
        }

        if (a_edit.AutoJoinGroupSet && AutoJoinGroup != a_edit.AutoJoinGroup)
        {
            AutoJoinGroup = a_edit.AutoJoinGroup;
            updated = true;
        }

        if (a_edit.ResizeForStorageIsSet && ResizeForStorage != a_edit.ResizeForStorage)
        {
            ResizeForStorage = a_edit.ResizeForStorage;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        DetermineProductAndLinkProductRules();

        AssociateOpsWithPaths(false);

        if (a_edit.LockedPlantIdSet)
        {
            if (LockedPlant != null && (a_edit.LockedPlantId == null || a_edit.LockedPlantId == BaseId.NULL_ID))
            {
                LockedPlant = null;
                a_dataChanges.FlagEligibilityChanges(Job.Id);
                updated = true;
            }
            else if (LockedPlant?.Id != a_edit.LockedPlantId && a_edit.LockedPlantId != BaseId.NULL_ID)
            {
                //Check to see if the MO is scheduled only in this new Plant.  If not then it needs to be rescheduled.
                bool foundDifferentPlant = false;
                for (int i = 0; i < CurrentPath.NodeCount; i++)
                {
                    AlternatePath.Node node = CurrentPath.GetNodeByIndex(i);
                    if (node.Operation is InternalOperation)
                    {
                        InternalOperation iOp = (InternalOperation)node.Operation;
                        List<InternalResource> resources = iOp.GetResourcesScheduled();
                        for (int resI = 0; resI < resources.Count; resI++)
                        {
                            if (resources[resI].Department.Plant.Id != a_edit.LockedPlantId)
                            {
                                m_job.Unschedule();
                                foundDifferentPlant = true;
                                break;
                            }
                        }
                    }

                    if (foundDifferentPlant)
                    {
                        break;
                    }
                }

                LockedPlant = a_sd.PlantManager.GetById(a_edit.LockedPlantId);
                a_dataChanges.FlagEligibilityChanges(Job.Id);
                updated = true;
            }
        }

        if (a_edit.LockedToCurrentAlternatePathSet && LockToCurrentAlternatePath != a_edit.LockedToCurrentAlternatePath)
        {
            LockToCurrentAlternatePath = a_edit.LockedToCurrentAlternatePath;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        if (a_edit.SplitUpdateModeSet && a_edit.SplitUpdateMode != SplitUpdateMode)
        {
            SplitUpdateMode = a_edit.SplitUpdateMode;
            a_dataChanges.FlagEligibilityChanges(Job.Id);
            updated = true;
        }

        return updated;
    }

    /// <summary>
    /// Report to the MO that the clock has advanced.
    /// </summary>
    internal void AdvanceClock(TimeSpan clockAdvancedBy, DateTime newClock, bool autoFinishAllActivities, bool autoReportProgressOnAllActivities)
    {
        CurrentPath.AdvancingClock(clockAdvancedBy, newClock, autoFinishAllActivities, autoReportProgressOnAllActivities);
    }

    private void SetOperationStatusChangeFlags(ulong statusChanges, ref ulong changes)
    {
        if ((statusChanges &= (ulong)AlternatePath.RoutingChanges.ActivitiesFinished) > 0)
        {
            changes |= (ulong)MOUpdate.ActivitiesFinished;
        }
    }

    /// <summary>
    /// Checks to make sure the Warehouse is not in use.
    /// </summary>
    internal void ValidateWarehouseDelete(Warehouse warehouse)
    {
        OperationManager.ValidateWarehouseDelete(warehouse);
    }
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_deleteProfile)
    {
        OperationManager.ValidateStorageAreaDelete(a_storageArea, a_deleteProfile);
    }

    /// <summary>
    /// Checks to make sure the Inventory is not in use.
    /// </summary>
    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        OperationManager.ValidateInventoryDelete(a_deleteProfile);
    }

    /// <summary>
    /// Checks to make sure the Item is not in use.
    /// </summary>
    internal void ValidateItemDelete(ItemDeleteProfile a_itemDeleteProfile)
    {
        OperationManager.ValidateItemDelete(a_itemDeleteProfile);
    }
    /// <summary>
    /// Checks to make sure the Storage Area is not in use.
    /// </summary>
    internal void ValidateItemStorageDelete(ItemStorageDeleteProfile a_itemStorageDeleteProfile)
    {
        OperationManager.ValidateItemStorageDelete(a_itemStorageDeleteProfile);
    }

    internal void Commit(bool commit, DateTime clock, Dictionary<BaseId, BaseId> resourcesToInclude)
    {
        OperationManager.Commit(commit, clock, resourcesToInclude);
    }

    #region ERP transmission status update
    /// <summary>
    /// Call this function before handling a JobT or some other transmission that updates the status of jobs.
    /// It resets the activity variables that indicate the type of updates that have occurred.
    /// </summary>
    internal override void ResetERPStatusUpdateVariables()
    {
        base.ResetERPStatusUpdateVariables();
        OperationManager.ResetERPStatusUpdateVariables();
        //			JobTExpediteNewlyInProcess=false;
    }

    //		/// <summary>
    //		/// Used to mark whether the MO should be expedited after recieving a JobT.
    //		/// </summary>
    //		internal bool JobTExpediteNewlyInProcess
    //		{
    //			get
    //			{
    //				return simFlags[JobTExpediteNewlyInProcessIdx];
    //			}
    //			set
    //			{
    //				simFlags[JobTExpediteNewlyInProcessIdx]=value;
    //			}
    //		}
    #endregion
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "ManufacturingOrder";
    #endregion

    #region Cloning
    public ManufacturingOrder Clone()
    {
        return (ManufacturingOrder)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    #region Copy
    /// <summary>
    /// Call Job.InitGenerated on the copy after you have set its values.
    /// </summary>
    /// <returns></returns>
    internal ManufacturingOrder CreateUnitializedCopy()
    {
        long sizeOfCopy;
        ManufacturingOrder copy = (ManufacturingOrder)Serialization.CopyInMemory(this, new Serialization.CopyCreatorDelegate(MOCreator), out sizeOfCopy);
        return copy;
    }

    /// <summary>
    /// Helper of GenerateJobs().
    /// </summary>
    internal object MOCreator(IReader reader)
    {
        ManufacturingOrder mo = new(reader, ScenarioDetail.IdGen);
        return mo;
    }
    #endregion

    #region PredecessorMOs
    public enum SubassemblyType { NotSubassembly, SubassemblyOfMo, SubassemblyOfPath, SubassemblyOfOperation }

    /// <summary>
    /// Determines whether an MO has the specified possibleSuccessorMO as a Successor.
    /// </summary>
    /// <param name="possibleParentMO"></param>
    /// <returns></returns>
    public bool IsPredecessorMO(ManufacturingOrder possibleSuccessorMO)
    {
        for (int i = 0; i < SuccessorMOs.Count; i++)
        {
            SuccessorMO sucMO = SuccessorMOs[i];
            if (Equals(sucMO.SuccessorManufacturingOrder, possibleSuccessorMO))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get a list of all ManufacturingOrders that specify this Manufacturing Order as one of their immediate SuccessorMOs.
    /// </summary>
    public List<ManufacturingOrder> GetPredecessorMOs(JobManager jobManager)
    {
        HashSet<BaseId> moIdsAddedHash = new();
        List<ManufacturingOrder> predMOs = new();

        for (int jobI = 0; jobI < jobManager.Count; jobI++)
        {
            Job job = jobManager[jobI];
            for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder possiblePredMO = job.ManufacturingOrders[moI];
                if (possiblePredMO.IsPredecessorMO(this))
                {
                    if (!moIdsAddedHash.Contains(possiblePredMO.Id))
                    {
                        moIdsAddedHash.Add(possiblePredMO.Id);
                        predMOs.Add(possiblePredMO);
                    }
                }
            }
        }

        return predMOs;
    }

    /// <summary>
    /// Returns a list of all ManufacturingOrders that are have any of these Jobs as their Successor, any number of levels away.
    /// </summary>
    public static List<ManufacturingOrder> GetPredecessorMOsRecursively(JobManager jobManager, BaseIdList jobKeyList, out MOKeyList predMoKeyList, bool includeOriginalMOsInReturnList, bool a_includeUnscheduledJobs)
    {
        Hashtable predMoIdsAddedHash = new();
        List<ManufacturingOrder> predMOs = new();
        foreach (BaseId jobId in jobKeyList)
        {
            Job job = jobManager.GetById(jobId);
            if (job != null)
            {
                for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
                {
                    ManufacturingOrder mo = job.ManufacturingOrders[moI];
                    if (includeOriginalMOsInReturnList && !predMoIdsAddedHash.ContainsKey(mo.Id))
                    {
                        predMoIdsAddedHash.Add(mo.Id, null);
                        predMOs.Add(mo);
                    }

                    GetPredecessorMOsRecursivelyHelper(mo, jobManager, ref predMOs, ref predMoIdsAddedHash, a_includeUnscheduledJobs);
                }
            }
        }

        //Copy to list
        predMoKeyList = new MOKeyList();
        for (int moI = 0; moI < predMOs.Count; moI++)
        {
            ManufacturingOrder mo = predMOs[moI];
            predMoKeyList.Add(new ManufacturingOrderKey(mo.Job.Id, mo.Id));
        }

        return predMOs;
    }

    private static void GetPredecessorMOsRecursivelyHelper(ManufacturingOrder sucMO, JobManager jobManager, ref List<ManufacturingOrder> predMOs, ref Hashtable predMoIdsAddedHash, bool a_includeUnscheduledJobs)
    {
        List<ManufacturingOrder> thisMoPredList = sucMO.GetPredecessorMOs(jobManager);
        //Add any predecessors to the list not already there
        for (int i = 0; i < thisMoPredList.Count; i++)
        {
            ManufacturingOrder nextPred = thisMoPredList[i];
            if (!predMoIdsAddedHash.ContainsKey(nextPred.Id))
            {
                predMoIdsAddedHash.Add(nextPred.Id, null);
                if (a_includeUnscheduledJobs || nextPred.Scheduled)
                {
                    //Exclude non scheduled MOs but keep looking for predecessors
                    predMOs.Add(nextPred);
                }

                GetPredecessorMOsRecursivelyHelper(nextPred, jobManager, ref predMOs, ref predMoIdsAddedHash, a_includeUnscheduledJobs);
            }
        }
    }

    /// <summary>
    /// Returns the type of subassembly a given operation is.
    /// </summary>
    /// <param name="op">A possible successor operation of an MO.</param>
    /// <returns>The manner in which the given operation is a successor of this MO.</returns>
    public SubassemblyType IsSubassembly(BaseOperation op)
    {
        SubassemblyType returnType = SubassemblyType.NotSubassembly;

        for (int i = 0; i < SuccessorMOs.Count; i++)
        {
            SuccessorMO sucMO = SuccessorMOs[i];

            // Determine the relationship between this MO and the current successor MO.
            SubassemblyType sucMOType = SubassemblyType.NotSubassembly;

            if (sucMO.SuccessorJobExternalId == op.ManufacturingOrder.Job.ExternalId &&
                sucMO.SuccessorManufacturingOrderExternalId == op.ManufacturingOrder.ExternalId) //Operation's MO is SuccessorMO
            {
                sucMOType = SubassemblyType.SubassemblyOfMo;

                // Determine if the Successor relationship specifies a specific Operation or if it's just at the MO level.
                if (TextUtil.Length(sucMO.AlternatePathExternalId) > 0)
                {
                    AlternatePath alternatePath = AlternatePaths.FindByExternalId(sucMO.AlternatePathExternalId);

                    if (alternatePath != null)
                    {
                        sucMOType = SubassemblyType.SubassemblyOfPath;

                        if (TextUtil.Length(sucMO.OperationExternalId) > 0)
                        {
                            BaseOperation baseOperation = null;

                            if (alternatePath.AlternateNodeSortedList.TryGetValue(sucMO.OperationExternalId, out AlternatePath.Node node))
                            {
                                baseOperation = node.Operation;
                            }

                            //If the operation is not found, revert back to MO sub-assembly.
                            sucMOType = baseOperation != null ? SubassemblyType.SubassemblyOfOperation : SubassemblyType.SubassemblyOfMo;
                        }
                    }
                }
            }

            // Adjust the return type of the function if a better result has been found.
            int retTypeTemp = (int)returnType;
            int sucMORetType = (int)sucMOType;
            if (sucMOType > returnType)
            {
                returnType = (SubassemblyType)sucMORetType;
            }
        }

        //Operation's MO is not one of the SuccessorMOs
        return returnType;
    }
    #endregion Predecessor MOs

    #region PT Database/UI/
    internal void PopulateJobDataSet(JobManager jobs, ref JobDataSet dataSet)
    {
        JobDataSet.ManufacturingOrderRow moRow = dataSet.ManufacturingOrder.NewManufacturingOrderRow();
        moRow.JobExternalId = Job.ExternalId;
        moRow.Id = Id.ToBaseType();
        moRow.ExternalId = ExternalId;
        moRow.Name = Name;
        moRow.Description = Description;
        moRow.Notes = Notes;
        moRow.CanSpanPlants = CanSpanPlants;
        moRow.CurrentPath = CurrentPath.ExternalId;
        moRow.DefaultPathExternalId = DefaultPath.ExternalId;
        moRow.ExpectedFinishQty = ExpectedFinishQty;
        moRow.Family = Family;
        moRow.Finished = Finished;
        moRow.Anchored = (int)Anchored;
        moRow.OnHold = (int)OnHold;
        moRow.Hold = Hold;
        moRow.HoldReason = HoldReason;
        moRow.HoldUntilDate = HoldUntil.ToDisplayTime().ToDateTime();
        moRow.MoNeedDate = MoNeedDate;
        moRow.NeedDate = NeedDate.ToDisplayTime().ToDateTime();
        moRow.Late = Late;
        moRow.Lateness = Lateness;
        moRow.LeadTimeDays = LeadTime.TotalDays;
        moRow.Locked = (int)Locked;
        if (LockedPlant != null)
        {
            moRow.LockedPlantExternalId = LockedPlant.ExternalId;
        }
        else
        {
            moRow.LockedPlantExternalId = "";
        }

        moRow.ProductName = ProductName;
        moRow.ProductDescription = ProductDescription;
        moRow.CopyRoutingFromTemplate = CopyRoutingFromTemplate;
        moRow.ProductColor = ColorUtils.ConvertColorToHexString(ProductColor);
        moRow.RequiredQty = RequiredQty;
        moRow.RequestedQty = RequestedQty;
        moRow.PreserveRequiredQty = PreserveRequiredQty;
        moRow.Scheduled = Scheduled;
        moRow.ScheduledEnd = ScheduledEndDate.ToDisplayTime().ToDateTime();
        moRow.ScheduledStart = ScheduledStartDate.ToDisplayTime().ToDateTime();
        moRow.UOM = UOM;
        moRow.PercentFinished = PercentFinished;
        moRow.LockToCurrentAlternatePath = LockToCurrentAlternatePath;
        moRow.UserFields = UserFields == null ? "" : UserFields.GetUserFieldImportString();
        moRow.AutoJoinGroup = AutoJoinGroup;
        moRow.SplitUpdateMode = SplitUpdateMode.ToString();
        moRow.SplitCount = SplitCount;
        moRow.DeepSplitCount = DeepSplitCount;
        moRow.TemplateForInventoriesCount = InventoriesUsingMoAsTemplate;
        moRow.Split = Split;
        if (ShippingBufferOverrideTicks.HasValue)
        {
            moRow.ShippingBufferOverrideDays = TimeSpan.FromTicks(ShippingBufferOverrideTicks.Value).TotalDays;
        }

        moRow.ShippingBufferDueDate = ShippingDueDate.ToDisplayTime().ToDateTime();
        moRow.ReleaseDate = GetReleaseDate().ToDisplayTime().ToDateTime();
        moRow.DaysOfStock = DaysOfStock;
        moRow.DaysOfStockExcludingForecasts = DaysOfStockExcludingForecasts;
        moRow.FirstShortage = FirstShortage;
        moRow.FirstShortageExcludingForecasts = FirstShortageExcludingForecasts;

        if (Split) //otherwise leave blank rather than showing NULL ID large value
        {
            moRow.SplitFromManufacturingOrderId = SplitFromManufacturingOrderId.ToBaseType();
        }

        moRow.ResizeForStorage = ResizeForStorage;

        dataSet.ManufacturingOrder.AddManufacturingOrderRow(moRow);

        //Add Paths
        for (int pathI = 0; pathI < AlternatePaths.Count; pathI++)
        {
            AlternatePaths[pathI].PopulateJobDataSet(moRow, ref dataSet, this);
        }

        //Add Operations
        OperationManager.PopulateJobDataSet(moRow, jobs, ref dataSet);

        //Add Successor MOs
        for (int i = 0; i < SuccessorMOs.Count; i++)
        {
            SuccessorMO sucMO = SuccessorMOs[i];
            JobDataSet.SuccessorMORow sucMoRow = dataSet.SuccessorMO.NewSuccessorMORow();
            sucMoRow.JobExternalId = Job.ExternalId;
            sucMoRow.ExternalId = sucMO.ExternalId;
            sucMoRow.MoExternalId = ExternalId;

            sucMoRow.SuccessorJobExternalId = sucMO.SuccessorJobExternalId;
            sucMoRow.SuccessorManufacturingOrderExternalId = sucMO.SuccessorManufacturingOrderExternalId;

            sucMoRow.TransferHrs = new TimeSpan(sucMO.TransferSpan).TotalHours;

            sucMoRow.SuccessorPathExternalId = sucMO.AlternatePathExternalId;

            sucMoRow.SuccessorOperationExternalId = sucMO.OperationExternalId;
            sucMoRow.UsageQtyPerCycle = sucMO.UsageQtyPerCycle;

            //JMC TODO Should eventaully list multipler operations as successor constrained.
            dataSet.SuccessorMO.AddSuccessorMORow(sucMoRow);
        }
    }
    #endregion

    #region Other functions.
    /// <summary>
    /// Whether all schedulable operations have been scheduled. This function examines each operation in the alternate path for its scheduled state.
    /// </summary>
    public bool Scheduled
    {
        get
        {
            using IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();
            bool scheduled = false;

            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation operation = node.Operation;

                if (operation.IsNotFinishedAndNotOmitted)
                {
                    if (!operation.Scheduled)
                    {
                        return false;
                    }

                    scheduled = true;
                }
            }

            return scheduled;
        }
    }

    // Determine whether the paths have the same structure and operation types.
    internal RoutingChanges IdenticalPathStructureAndObjectTypes(ManufacturingOrder a_mo2)
    {
        RoutingChanges routingChanges = AlternatePaths.IdenticlePathStructures(a_mo2.AlternatePaths);
        if (routingChanges.ScheduledRoutingChanged)
        {
            return routingChanges;
        }

        for (int opI = 0; opI < OperationManager.Count; ++opI)
        {
            BaseOperation op = OperationManager.GetByIndex(opI);
            BaseOperation mo2Op = a_mo2.OperationManager[op.ExternalId];

            //Op Was deleted. If it was scheduled there is a routing change.
            if (mo2Op == null)
            {
                if (op.Scheduled)
                {
                    routingChanges.ScheduledRoutingChanged = true;
                    routingChanges.RoutingChangeCause = RoutingChanges.RoutingChangeCauses.ScheduledOperationRemoved;
                    return routingChanges;
                }

                routingChanges.AlternatePathChanged = true;
                continue;
            }

            if (op.UniqueId != mo2Op.UniqueId)
            {
                // The operations have different types.
                routingChanges.ScheduledRoutingChanged = true;
                routingChanges.RoutingChangeCause = RoutingChanges.RoutingChangeCauses.ScheduledOperationChangedType;
                return routingChanges;
            }
        }

        return routingChanges;
    }

    /// <summary>
    /// Totals the Setup Hours for all Activities scheduled to start before or on the specified date.
    /// </summary>
    /// <returns></returns>
    public long SumOfSetupHoursTicks(DateTime dt, bool includeLaborResourcesOnly)
    {
        using IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();
        long total = 0;

        while (alternateNodesEnumerator.MoveNext())
        {
            AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
            BaseOperation operation = node.Operation;
            if (operation is InternalOperation && operation.Scheduled)
            {
                InternalOperation iOp = (InternalOperation)operation;
                total += iOp.SumOfSetupHoursTicks(dt, includeLaborResourcesOnly);
            }
        }

        return total;
    }

    /// <summary>
    /// Totals the Setup Costs for all Activities scheduled to start before or on the specified date.
    /// </summary>
    /// <returns></returns>
    public decimal SumOfSetupCost(DateTime dt, bool includeLaborResourcesOnly)
    {
        IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();
        decimal total = 0;

        while (alternateNodesEnumerator.MoveNext())
        {
            AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
            BaseOperation operation = node.Operation;
            if (operation is InternalOperation && operation.Scheduled)
            {
                InternalOperation iOp = (InternalOperation)operation;
                total += iOp.SumOfSetupCost(dt, includeLaborResourcesOnly);
            }
        }

        return total;
    }
    /// <summary>
    /// Totals the Clean Hours for all Activities scheduled to start before or on the specified date.
    /// </summary>
    /// <returns></returns>
    public long SumOfCleanHoursTicks(DateTime dt, bool includeLaborResourcesOnly)
    {
        using IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();
        long total = 0;

        while (alternateNodesEnumerator.MoveNext())
        {
            AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
            BaseOperation operation = node.Operation;
            if (operation is InternalOperation && operation.Scheduled)
            {
                InternalOperation iOp = (InternalOperation)operation;
                total += iOp.SumOfCleanHoursTicks(dt, includeLaborResourcesOnly);
            }
        }

        return total;
    }
    /// <summary>
    /// Returns a unique list of Resources used by scheduled operations.
    /// </summary>
    /// <returns></returns>
    public List<InternalResource> GetResourcesScheduled()
    {
        List<InternalResource> resourcesUsedForMO = new();
        HashSet<BaseId> resourceIdsAdded = new();

        IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
            BaseOperation operation = node.Operation;
            if (operation is InternalOperation && operation.Scheduled)
            {
                InternalOperation iOp = (InternalOperation)operation;
                List<InternalResource> resourcesUsed = iOp.GetResourcesScheduled();
                for (int rI = 0; rI < resourcesUsed.Count; rI++)
                {
                    InternalResource resource = resourcesUsed[rI];
                    if (!resourceIdsAdded.Contains(resource.Id))
                    {
                        resourcesUsedForMO.Add(resource);
                        resourceIdsAdded.Add(resource.Id);
                    }
                }
            }
        }

        return resourcesUsedForMO;
    }
    #endregion

    #region Bottleneck operations
    //Starting at the leaves. When you find a bottleneck operation mark it as a bottleneck and recursively mark all successor operations as not being bottlenecks.
    //When you process an operation and find that it is not a bottleneck operation mark it as having been tested.
    //
    //The first is the BottleneckFlag.
    //The second is the BottleneckTestedFlag.
    //
    //The possible states within are:
    //
    //1 (This is the initial condition)
    //BottleneckFlag=false;
    //BottleneckTestedFlag=false;
    //
    //2 (A bottleneck operation can be set to this condition and then be set to condition 3)
    //BottleneckFlag=true;
    //BottleneckTestedFlag=true;
    //
    //3 (This is the final state; Once in it an operation never leaves this state).
    //BottleneckFlag=false;
    //BottleneckTestedFlag=true;

    /// <summary>
    /// Returns a list of operations that are preventing this order from finishing ontime.
    /// </summary>
    /// <returns></returns>
    public List<BaseOperation> GetBottleneckOperations()
    {
        // Initialize the InternalOperation.Bottleneck flags.
        IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
            node.Operation.BottleneckFlag = false;
            node.Operation.BottleneckTestedFlag = false;
        }

        // Perform the bottleneck processing starting at the leaves and working to the end of the routings.
        AlternatePath.NodeCollection leaves = CurrentPath.Leaves;
        for (int leaveI = 0; leaveI < leaves.Count; ++leaveI)
        {
            AlternatePath.Node node = leaves[leaveI];
            ProcessNode(node);
        }

        // Return the bottleneck operations.
        alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();
        List<BaseOperation> bottleneckOps = new();
        while (alternateNodesEnumerator.MoveNext())
        {
            AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
            if (node.Operation.BottleneckFlag)
            {
                bottleneckOps.Add(node.Operation);
            }
        }

        return bottleneckOps;
    }

    /// <summary>
    /// A helper for GetBottleneckOperations().
    /// Attempt to find bottleneck operations in the passed in node or the successors.
    /// </summary>
    /// <param name="bottleneckOperations"></param>
    /// <param name="processedNodes"></param>
    private void ProcessNode(AlternatePath.Node node)
    {
        node.Operation.BottleneckTestedFlag = true;

        if (node.Operation.Bottleneck)
        {
            node.Operation.BottleneckFlag = true;
            MarkSuccessorsAsNotBeingBottlenecks(node.Operation);
        }
        else
        {
            // Process the untested sucessor nodes.
            AlternatePath.AssociationCollection successorAssociations = node.Successors;
            for (int successorI = 0; successorI < successorAssociations.Count; ++successorI)
            {
                AlternatePath.Association association = successorAssociations[successorI];
                AlternatePath.Node sucNode = association.Successor;
                if (!sucNode.Operation.BottleneckTestedFlag)
                {
                    ProcessNode(sucNode);
                }
            }
        }
    }

    /// <summary>
    /// When you find a bottleneck operation call this function to mark all the successor operations as having been processed.
    /// A helper for GetBottleneckOperations().
    /// </summary>
    /// <param name="bo">The operations whose successors will be marked as not bottlenecks.</param>
    private void MarkSuccessorsAsNotBeingBottlenecks(BaseOperation bo)
    {
        AlternatePath.AssociationCollection successorAssociations = bo.AlternatePathNode.Successors;
        for (int successorI = 0; successorI < successorAssociations.Count; ++successorI)
        {
            // Mark the operation as not being a bottleneck
            AlternatePath.Association association = successorAssociations[successorI];
            AlternatePath.Node sucNode = association.Successor;
            sucNode.Operation.BottleneckFlag = false;
            sucNode.Operation.BottleneckTestedFlag = true;
            MarkSuccessorsAsNotBeingBottlenecks(sucNode.Operation);
        }
    }
    #endregion

    #region Definitions, enumeration, etc...
    public enum MOUpdate : ulong
    {
        Unchanged = 0,
        CanSpanPlants = 1, //  1 
        Family = 2, //  2
        IsReleased = 4, //  3
        ProductDescription = 8, //  4
        ProductName = 16, //  5
        ReleaseDateIncrease = 32, //  6
        RequiredQty = 64, //  7
        UOM = 128, //  8
        ActivitiesFinished = 256, //  9
        OperationStatus = 512, // 10
        ReleaseDateDecrease = 1024, // 11
        AlternatePath = 2048, // 12
        LockedPlant = 4096 // 13
    }
    #endregion

    #region Constraint Violations
    /// <summary>
    /// Find constraint violations of the MO's activities and add them to the list parameter.
    /// </summary>
    /// <param name="violations">All constraint violations are added to this list.</param>
    internal void GetConstraintViolations(ConstraintViolationList violations)
    {
        IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();
        ConstraintViolationList tempViolations = new();

        while (enumerator.MoveNext())
        {
            AlternatePath.Node apNode = enumerator.Current.Value;
            BaseOperation operation = apNode.Operation;
            operation.GetConstraintViolations(tempViolations);
        }

        ResetHitCount();

        ConstraintViolationList.Node v = tempViolations.First;
        while (v != null)
        {
            InternalActivity ia = v.Data.m_activity;
            InternalOperation io = ia.Operation;
            HitOperation(io, true);
            v = v.Next;
        }

        ConstraintViolationList.Node currentCopyNode = tempViolations.First;
        while (currentCopyNode != null)
        {
            if (currentCopyNode.Data.m_activity.Operation.m_hitCount == 0)
            {
                violations.Add(currentCopyNode.Data);
            }

            currentCopyNode = currentCopyNode.Next;
        }
    }

    internal void ResetHitCount()
    {
        IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

        while (enumerator.MoveNext())
        {
            AlternatePath.Node apNode = enumerator.Current.Value;
            BaseOperation operation = apNode.Operation;
            operation.m_hitCount = 0;
        }
    }

    private void HitOperation(BaseOperation io, bool startSeq)
    {
        if (!startSeq)
        {
            io.m_hitCount++;
        }

        AlternatePath.AssociationCollection ac = io.Successors;

        for (int i = 0; i < ac.Count; ++i)
        {
            AlternatePath.Association asso = ac[i];
            AlternatePath.Node node = asso.Successor;
            HitOperation(node.Operation, false);
        }
    }
    #endregion

    #region Breakoff
    private readonly BaseId m_breakOffSourceMoId = BaseId.NULL_ID;

    internal BaseId BreakOffSourceMoId => m_breakOffSourceMoId;

    /// <summary>
    /// Name of the Manufacturing Order from which this Manufacturing Order was Broken Off, if this is a Break Off.
    /// If this is not a Break Off or if the original Manufacturing Order was deleted, this is an empty string.
    /// </summary>
    public string BreakOffSourceMoName
    {
        get
        {
            if (!IsBreakOff)
            {
                return "";
            }

            ManufacturingOrder mo = Job.ManufacturingOrders.GetById(BreakOffSourceMoId);
            if (mo != null) //may have been deleted
            {
                return mo.Name;
            }

            return "";
        }
    }

    /// <summary>
    /// Indicates whether this Manufacturing Order is the result of a Break Off from another Manufacturing Order.
    /// </summary>
    public bool IsBreakOff => BreakOffSourceMoId != BaseId.NULL_ID;

    internal void ValidateBreakOff(decimal breakOffQty)
    {
        //Validate
        if (breakOffQty <= 0)
        {
            throw new PTValidationException("2222", new object[] { breakOffQty });
        }

        if (breakOffQty >= RequiredQty)
        {
            throw new PTValidationException("2223", new object[] { breakOffQty, RequiredQty });
        }
    }
    #endregion

    #region Demo Data
    /// <summary>
    /// Adjust values to update Demo Data for clock advance so good relative dates are maintained.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long clockAdvanceTicks)
    {
        //Update Release Date, Hold, and NeedDate
        NeedDateTicks += clockAdvanceTicks;
        if (Hold)
        {
            HoldUntilTicks += clockAdvanceTicks;
        }

        OperationManager.AdjustDemoDataForClockAdvance(clockAdvanceTicks);
    }
    #endregion

    #region manufacturingOrderBools
    private BoolVector32 m_moBools;

    private const int EligiblePlantsSpecifiedIdx = 0;
    private const int PreserveRequiredQtyIdx = 1;
    private const int SplitIdx = 2;
    private const int UsedAsSubComponentIdx = 3;

    /// <summary>
    /// Whether eligible plants have been specified in the ManufacturingOrder Transmission or whether any plant might be eligible.
    /// </summary>
    private bool EligiblePlantsSpecified
    {
        get => m_moBools[EligiblePlantsSpecifiedIdx];

        set => m_moBools[EligiblePlantsSpecifiedIdx] = value;
    }

    /// <summary>
    /// If true then the Required Qty can only be set manually, not via ERP imports.
    /// This value is set to true automatically when the Required Qty is set internally by a planner in order to preserve the change.
    /// </summary>
    public bool PreserveRequiredQty
    {
        get => m_moBools[PreserveRequiredQtyIdx];
        internal set => m_moBools[PreserveRequiredQtyIdx] = value;
    }

    /// <summary>
    /// Whether some of the product generated by this ManufactingOrder is being used as a constraint of another ManufacturingOrder.
    /// </summary>
    public bool UsedAsSubComponent
    {
        get => m_moBools[UsedAsSubComponentIdx];

        internal set => m_moBools[UsedAsSubComponentIdx] = value;
    }

    /// <summary>
    /// Returns true if any of the operations are scheduled on an Inbox Type Resource.
    /// </summary>
    public bool Inbox
    {
        get
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> alternateNodesEnumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (alternateNodesEnumerator.MoveNext())
            {
                AlternatePath.Node node = alternateNodesEnumerator.Current.Value;
                BaseOperation operation = node.Operation;
                if (operation is InternalOperation && operation.Scheduled)
                {
                    InternalOperation iOp = (InternalOperation)operation;
                    List<InternalResource> resourcesUsed = iOp.GetResourcesScheduled();
                    for (int rI = 0; rI < resourcesUsed.Count; rI++)
                    {
                        InternalResource resource = resourcesUsed[rI];
                        if (resource.ResourceType == BaseResourceDefs.resourceTypes.Inbox)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
    #endregion

    #region Delete Validation
    internal void ResourceDeleteNotification(BaseResource r)
    {
        for (int i = 0; i < OperationManager.Count; ++i)
        {
            InternalOperation io = OperationManager.OperationsHash.GetByIndex(i) as InternalOperation;
            if (io != null)
            {
                io.ResourceDeleteNotification(r);
            }
        }
    }
    #endregion

    #region Buffer Stock
    /// <summary>
    /// The Buffer Stock Penetration Percent of the Primary Product.
    /// 0 if no Primary Product.
    /// </summary>
    public decimal BufferStockPenetrationPercent
    {
        get
        {
            Product primaryProduct = GetPrimaryProduct();
            if (primaryProduct != null && primaryProduct.Inventory != null)
            {
                return primaryProduct.Inventory.BufferPenetrationPercent;
            }

            return 0;
        }
    }

    /// <summary>
    /// Inventory.BufferStockWarningLevel. (OK if no Primary Product)
    /// </summary>
    public BufferDefs.WarningLevels BufferStockWarningLevel
    {
        get
        {
            Product primaryProduct = GetPrimaryProduct();
            if (primaryProduct != null && primaryProduct.Inventory != null)
            {
                return primaryProduct.Inventory.BufferWarningLevel;
            }

            return BufferDefs.WarningLevels.OK;
        }
    }

    /// <summary>
    /// Inventory.BufferStock (0 if no Primary Product).
    /// </summary>
    public decimal BufferStock
    {
        get
        {
            Product primaryProduct = GetPrimaryProduct();
            if (primaryProduct != null && primaryProduct.Inventory != null)
            {
                return primaryProduct.Inventory.BufferStock;
            }

            return 0;
        }
    }

    /// <summary>
    /// Inventory.BufferStock (0 if no Primary Product).
    /// </summary>
    public decimal ProductOnHandInventory
    {
        get
        {
            Product primaryProduct = GetPrimaryProduct();
            if (primaryProduct != null && primaryProduct.Inventory != null)
            {
                return primaryProduct.Inventory.OnHandQty;
            }

            return 0;
        }
    }
    #endregion Buffer Stock

    #region Cost
    /// <summary>
    /// The sum of the Material Cost for all Operations in the Current Path.
    /// </summary>
    public decimal MaterialCost
    {
        get
        {
            decimal cost = 0;
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                AlternatePath.Node apNode = enumerator.Current.Value;
                BaseOperation operation = apNode.Operation;
                cost += operation.MaterialCost;
            }

            return cost;
        }
    }

    /// <summary>
    /// Returns the Materials Requirements from all Operations in all Paths.
    /// </summary>
    /// <returns></returns>
    public List<MaterialRequirement> GetMaterialRequirements()
    {
        List<MaterialRequirement> materialsList = new();

        IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

        while (enumerator.MoveNext())
        {
            AlternatePath.Node apNode = enumerator.Current.Value;
            BaseOperation operation = apNode.Operation;
            for (int mI = 0; mI < operation.MaterialRequirements.Count; mI++)
            {
                materialsList.Add(operation.MaterialRequirements[mI]);
            }
        }

        return materialsList;
    }

    /// <summary>
    /// Labor Cost + Machine Cost + Material Cost
    /// </summary>
    public decimal TotalCost => LaborCost + MachineCost + MaterialCost;

    /// <summary>
    /// The sum of the LaborCost for all Operations in the Current Path.
    /// </summary>
    public decimal LaborCost
    {
        get
        {
            decimal cost = 0;
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                AlternatePath.Node apNode = enumerator.Current.Value;
                BaseOperation operation = apNode.Operation;
                cost += operation.LaborCost;
            }

            return cost;
        }
    }

    /// <summary>
    /// The sum of the Machine Cost for all Operations in the Current Path.
    /// </summary>
    public decimal MachineCost
    {
        get
        {
            decimal cost = 0;
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                AlternatePath.Node apNode = enumerator.Current.Value;
                BaseOperation operation = apNode.Operation;
                cost += operation.MachineCost;
            }

            return cost;
        }
    }

    /// <summary>
    /// The sum of the Subcontract Cost for all Operations in the Current Path.
    /// </summary>
    public decimal SubcontractCost
    {
        get
        {
            decimal cost = 0;
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                AlternatePath.Node apNode = enumerator.Current.Value;
                BaseOperation operation = apNode.Operation;
                cost += operation.SubcontractCost;
            }

            return cost;
        }
    }

    public decimal TotalSetupCost
    {
        get
        {
            decimal cost = 0;
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                AlternatePath.Node apNode = enumerator.Current.Value;
                ResourceOperation operation = (ResourceOperation)apNode.Operation;
                if (operation != null)
                {
                    cost += operation.TotalSetupCost;
                }
            }

            return cost;
        }
    }
    public decimal CleansCost
    {
        get
        {
            decimal cost = 0;
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                AlternatePath.Node apNode = enumerator.Current.Value;
                ResourceOperation operation = (ResourceOperation)apNode.Operation;
                if (operation != null)
                {
                    cost += operation.TotalCleanoutCost;
                }
            }

            return cost;
        }
    }
    #endregion Cost

    #region Products
    /// <summary>
    /// Gets a list of Item Ids indicating which Item(s) have an Operation Product for the Item.
    /// </summary>
    /// <returns></returns>
    internal HashSet<BaseId> GetItemsProduced()
    {
        HashSet<BaseId> productItemsHash = new();

        for (int altPathI = 0; altPathI < AlternatePaths.Count; altPathI++)
        {
            AlternatePath path = AlternatePaths[altPathI];
            for (int nodeI = 0; nodeI < path.AlternateNodeSortedList.Count; nodeI++)
            {
                AlternatePath.Node node = path.AlternateNodeSortedList.Values[nodeI];
                for (int prodI = 0; prodI < node.Operation.Products.Count; prodI++)
                {
                    BaseId prodId = node.Operation.Products[prodI].Item.Id;
                    productItemsHash.AddIfNew(prodId);
                }
            }
        }

        return productItemsHash;
    }

    /// <summary>
    /// returns the list of products associated with this MO.
    /// </summary>
    /// <param name="a_currentPathOnly">if true, only products under the current path are returned.</param>
    /// <returns></returns>
    public List<Product> GetProducts(bool a_currentPathOnly)
    {
        if (a_currentPathOnly)
        {
            return CurrentPath.GetProducts();
        }

        List<Product> products = new();
        for (int pathI = 0; pathI < AlternatePaths.Count; pathI++)
        {
            products.AddRange(AlternatePaths[pathI].GetProducts());
        }

        return products;
    }

    /// <summary>
    /// Returns the first Product that produces the ManufacturingOrder.Product (Item) in the current path.
    /// Returns null if no such Product exists in the current Path.
    /// </summary>
    /// <returns></returns>
    internal Product GetFirstProductInCurrentPath()
    {
        if (Product == null)
        {
            return null;
        }

        AlternatePath path = CurrentPath;
        for (int nodeI = 0; nodeI < path.AlternateNodeSortedList.Count; nodeI++)
        {
            AlternatePath.Node node = path.AlternateNodeSortedList.Values[nodeI];
            for (int prodI = 0; prodI < node.Operation.Products.Count; prodI++)
            {
                Product product = node.Operation.Products[prodI];
                if (product.Item.Id == Product.Id)
                {
                    return product;
                }
            }
        }

        return null;
    }

    public List<AlternatePath> FindPathsThatProduceItem(BaseId itemId)
    {
        List<AlternatePath> pathList = new();
        for (int altPathI = 0; altPathI < AlternatePaths.Count; altPathI++)
        {
            AlternatePath path = AlternatePaths[altPathI];
            bool pathAdded = false;
            for (int nodeI = 0; nodeI < path.AlternateNodeSortedList.Count; nodeI++)
            {
                AlternatePath.Node node = path.AlternateNodeSortedList.Values[nodeI];
                for (int prodI = 0; prodI < node.Operation.Products.Count; prodI++)
                {
                    if (node.Operation.Products[prodI].Item.Id == itemId)
                    {
                        pathList.Add(path);
                        pathAdded = true;
                        break;
                    }
                }

                if (pathAdded)
                {
                    break;
                }
            }
        }

        return pathList;
    }

    /// <summary>
    /// Returns the Path containing the operation or null if no Path contains the operation.
    /// </summary>
    /// <param name="aOp"></param>
    /// <returns></returns>
    public AlternatePath FindPathForOperation(BaseOperation aOp)
    {
        for (int altPathI = 0; altPathI < AlternatePaths.Count; altPathI++)
        {
            AlternatePath path = AlternatePaths[altPathI];
            if (path.ContainsOperation(aOp.ExternalId))
            {
                return path;
            }
        }

        return null;
    }
    #endregion

    private void SplitBlocks(ManufacturingOrder copyMO)
    {
        for (int opI = 0; opI < OperationManager.Count; ++opI)
        {
            ResourceOperation sourceOp = (ResourceOperation)OperationManager.GetByIndex(opI);
            ResourceOperation copyOp = (ResourceOperation)copyMO.OperationManager.GetByIndex(opI);
            SplitBlocks(sourceOp, copyOp);
        }
    }

    private void SplitBlocks(ResourceOperation sourceOp, ResourceOperation copyOp)
    {
        for (int actI = 0; actI < sourceOp.Activities.Count; ++actI)
        {
            InternalActivity sourceAct = sourceOp.Activities.GetByIndex(actI);
            InternalActivity copyAct = copyOp.Activities.GetByIndex(actI);

            copyAct.ResetProductionStatus();

            if (sourceAct.Finished)
            {
                copyAct.ProductionStatus = InternalActivityDefs.productionStatuses.Finished;
            }

            if (sourceAct.ReportedGoodQty > sourceAct.RequiredFinishQty)
            {
                decimal reportedGoodQty = sourceAct.ReportedGoodQty;
                sourceAct.ReportedGoodQty = sourceAct.RequiredFinishQty;

                decimal remainingGoodQty = reportedGoodQty - sourceAct.ReportedGoodQty;
                copyAct.ReportedGoodQty = remainingGoodQty;
            }

            copyOp.ScheduleSplitActivity(sourceAct, copyAct);
        }
    }

    #region MO Splitting
    /// <summary>
    /// Whether this ManufacutingOrder was Split from another ManufacturingOrder.
    /// </summary>
    public bool Split
    {
        get => m_moBools[SplitIdx];

        internal set => m_moBools[SplitIdx] = value;
    }

    private BaseId m_splitFromMOId = BaseId.NULL_ID;

    /// <summary>
    /// If this Manufacturing Order is Split then this value specifies the Id of the Manufacturing Order from which it was Split.
    /// If this Manufacturing Order is not Split then this is the value of Null_ID.
    /// </summary>
    public BaseId SplitFromManufacturingOrderId
    {
        get => m_splitFromMOId;
        internal set => m_splitFromMOId = value;
    }

    /// <summary>
    /// The number of Manufacturing Orders that were Split directly from this Manufacturing Order.
    /// </summary>
    public int SplitCount => GetSplits().Count;

    /// <summary>
    /// The number of Manufacturing Orders that were Split directly or indirectly from this Manufacturing Order.
    /// </summary>
    public int DeepSplitCount => GetSplitsRecursively().Count;

    /// <summary>
    /// Get all Manufacturing Orders that were split from this MO, directly or indirectly.
    /// </summary>
    internal List<ManufacturingOrder> GetSplitsRecursively()
    {
        List<ManufacturingOrder> allSplits = new();
        Dictionary<BaseId, BaseId> moIdsAdded = new();
        List<ManufacturingOrder> topLevelSplits = GetSplits();
        for (int s = 0; s < topLevelSplits.Count; s++)
        {
            ManufacturingOrder topLevelSplitMO = topLevelSplits[s];
            if (!moIdsAdded.ContainsKey(topLevelSplitMO.Id))
            {
                allSplits.Add(topLevelSplitMO);
                moIdsAdded.Add(topLevelSplitMO.Id, topLevelSplitMO.Id);
            }

            List<ManufacturingOrder> curLevelSplits = topLevelSplitMO.GetSplitsRecursively();
            for (int c = 0; c < curLevelSplits.Count; c++)
            {
                ManufacturingOrder mo = curLevelSplits[c];
                if (!moIdsAdded.ContainsKey(mo.Id))
                {
                    allSplits.Add(mo);
                    moIdsAdded.Add(mo.Id, mo.Id);
                }
            }
        }

        return allSplits;
    }

    /// <summary>
    /// The Manufacturing Orders that were Split directly from this Manufacturing Order.
    /// </summary>
    /// <returns></returns>
    public List<ManufacturingOrder> GetSplits()
    {
        List<ManufacturingOrder> mos = new();
        for (int moI = 0; moI < Job.ManufacturingOrders.Count; moI++)
        {
            ManufacturingOrder mo = Job.ManufacturingOrders[moI];
            if (mo.SplitFromManufacturingOrderId == Id)
            {
                mos.Add(mo);
            }
        }

        return mos;
    }

    private ManufacturingOrderDefs.SplitUpdateModes m_splitUpdateMode;

    /// <summary>
    /// Specifies how updates to the Manufacturing Order are processed when the MO has been previously split and an update is imported.
    /// This has no effect on status changes made from within the system (as opposed to being imported).
    /// </summary>
    public ManufacturingOrderDefs.SplitUpdateModes SplitUpdateMode
    {
        get => m_splitUpdateMode;
        internal set => m_splitUpdateMode = value;
    }

    /// <summary>
    /// Sets the Reported quantities and times for the MO and its splits based on the MOs SplitUpdateMode.
    /// Assumes the MO is updated with the total Reported quantities and times and the splits are already created prior to calling this function.
    /// </summary>
    /// <param name="aMO"></param>
    internal void AllocateStatusAcrossSplits(decimal aTotalMoQty)
    {
        //Allocate to the splits. The remainder will be left in the original MO.
        List<ManufacturingOrder> moSplits = GetSplitsRecursively();
        for (int moI = 0; moI < moSplits.Count; moI++)
        {
            ManufacturingOrder split = moSplits[moI];
            AllocateStatusToSplit(split, moSplits, aTotalMoQty);
        }
    }

    private void AllocateStatusToSplit(ManufacturingOrder aSplit, List<ManufacturingOrder> aSplits, decimal aTotalMoQty)
    {
        for (int opI = 0; opI < OperationCount; opI++)
        {
            InternalOperation originalOp = (InternalOperation)OperationManager.GetByIndex(opI);
            if (originalOp != null)
            {
                InternalOperation splitOp = (InternalOperation)aSplit.OperationManager.Find(originalOp.Name);
                if (splitOp != null)
                {
                    decimal splitMoPctOfTotalQty = aSplit.RequiredQty / aTotalMoQty;
                    originalOp.AllocateStatusToMoSplit(splitOp, SplitUpdateMode, splitMoPctOfTotalQty);
                }
            }
        }
    }
    #endregion MO Splitting

    #region Templates
    /// <summary>
    /// Call before deleting the MO to give it a chance to clear references.
    /// </summary>
    /// <param name="a_dataChanges"></param>
    internal void Deleting(IScenarioDataChanges a_dataChanges)
    {
        if (m_usedAsTemplateInInvetoriesList != null)
        {
            List<Inventory> inventoriesAssociatedTemp = new();
            Dictionary<BaseId, Inventory>.Enumerator usedInInventories = m_usedAsTemplateInInvetoriesList.GetEnumerator();
            while (usedInInventories.MoveNext())
            {
                Inventory inv = usedInInventories.Current.Value;
                inventoriesAssociatedTemp.Add(inv);
            }

            //Now remove from Inventories.  Can't do it above because the call to the Inventory will access the MO's list which is iterating above.
            for (int i = 0; i < inventoriesAssociatedTemp.Count; i++)
            {
                inventoriesAssociatedTemp[i].DisassociateTemplateMO();
            }
        }

        OperationManager.Deleting(ScenarioDetail.PlantManager, a_dataChanges);
    }

    private UsedAsTemplateInInventoriesList m_usedAsTemplateInInvetoriesList;

    /// <summary>
    /// If the MO is used by one or more Inventories then this lists them.
    /// This NULL if there are no Inventories using this MO as a Template.
    /// </summary>
    public UsedAsTemplateInInventoriesList UsedAsTemplateInInventories => m_usedAsTemplateInInvetoriesList;

    public int InventoriesUsingMoAsTemplate
    {
        get
        {
            if (m_usedAsTemplateInInvetoriesList != null)
            {
                return m_usedAsTemplateInInvetoriesList.Count;
            }

            return 0;
        }
    }

    internal void UsedAsTemplateForInventories(UsedAsTemplateInInventoriesList originalMoInventories)
    {
        Dictionary<BaseId, Inventory>.Enumerator usedInInventories = originalMoInventories.GetEnumerator();
        List<Inventory> usedInInventoriesList = new();
        while (usedInInventories.MoveNext())
        {
            Inventory inv = usedInInventories.Current.Value;
            usedInInventoriesList.Add(inv);
        }

        //Iterate this list.  Can't modify collection above while iterating
        for (int i = 0; i < usedInInventoriesList.Count; i++)
        {
            Inventory inv = usedInInventoriesList[i];
            inv.DisassociateTemplateMO();
            inv.SetTemplateMoReference(this);
        }
    }

    internal void UsedAsTemplateForInventory(Inventory aInventory)
    {
        if (UsedAsTemplateInInventories == null)
        {
            m_usedAsTemplateInInvetoriesList = new UsedAsTemplateInInventoriesList();
        }

        if (!UsedAsTemplateInInventories.Contains(aInventory))
        {
            UsedAsTemplateInInventories.Add(aInventory);
        }
    }

    internal void NoLongerUsedAsTemplateForInventory(Inventory aInventory)
    {
        if ((UsedAsTemplateInInventories != null) & UsedAsTemplateInInventories.Contains(aInventory))
        {
            UsedAsTemplateInInventories.Remove(aInventory);
        }
    }

    public class UsedAsTemplateInInventoriesList
    {
        private readonly Dictionary<BaseId, Inventory> m_usedInInventories = new();

        internal bool Contains(Inventory aInventory)
        {
            return m_usedInInventories.ContainsKey(aInventory.Id);
        }

        internal void Add(Inventory aInventory)
        {
            m_usedInInventories.Add(aInventory.Id, aInventory);
        }

        internal void Remove(Inventory aInventory)
        {
            m_usedInInventories.Remove(aInventory.Id);
        }

        internal Dictionary<BaseId, Inventory>.Enumerator GetEnumerator()
        {
            return m_usedInInventories.GetEnumerator();
        }

        public int Count => m_usedInInventories.Count;
    }
    #endregion Templates

    #region MRP
    /// <summary>
    /// Flag Items that are produced or consumed as needing to be included in Net Change MRP since the delete will affect their inventory plans.
    /// </summary>
    internal void FlagAffectedItemsForNetChangeMRP(WarehouseManager aWarehouseManager)
    {
        for (int opI = 0; opI < OperationManager.Count; opI++)
        {
            BaseOperation op = OperationManager.GetByIndex(opI);
            for (int prodI = 0; prodI < op.Products.Count; prodI++)
            {
                op.Products[prodI].FlagItemForNetChangeMRP();
            }

            for (int mrI = 0; mrI < op.MaterialRequirements.Count; mrI++)
            {
                op.MaterialRequirements[mrI].FlagItemForNetChangeMRP(aWarehouseManager);
            }
        }
    }
    #endregion MRP

    /// <summary>
    /// The total number of Sales Orders satisfied by the Manufacturing Order's Products.
    /// </summary>
    public int SalesOrderCount
    {
        get
        {
            List<Product> products = GetProducts(true);
            int count = 0;
            for (int p = 0; p < products.Count; p++)
            {
                Product product = products[p];
                if (product.Demands != null)
                {
                    if (product.Demands.SalesOrderDemands != null)
                    {
                        count += product.Demands.SalesOrderDemands.Count;
                    }
                }
            }

            return count;
        }
    }

    /// <summary>
    /// The total number of Forecasts satisfied by the Manufacturing Order's Products.
    /// </summary>
    public int ForecastCount
    {
        get
        {
            List<Product> products = GetProducts(true);
            int count = 0;
            for (int p = 0; p < products.Count; p++)
            {
                Product product = products[p];
                if (product.Demands != null)
                {
                    if (product.Demands.ForecastDemands != null)
                    {
                        count += product.Demands.ForecastDemands.Count;
                    }
                }
            }

            return count;
        }
    }

    /// <summary>
    /// The total of all Required Qties for Sales Orders Demands.
    /// </summary>
    public decimal QtyToSalesOrders
    {
        get
        {
            List<Product> products = GetProducts(true);
            decimal qty = 0;
            for (int p = 0; p < products.Count; p++)
            {
                Product product = products[p];
                if (product.Demands != null)
                {
                    qty += product.Demands.QtyToSalesOrders;
                }
            }

            return qty;
        }
    }

    /// <summary>
    /// The total of all Required Qties for Forecast Demands.
    /// </summary>
    public decimal QtyToForecasts
    {
        get
        {
            List<Product> products = GetProducts(true);
            decimal qty = 0;
            for (int p = 0; p < products.Count; p++)
            {
                Product product = products[p];
                if (product.Demands != null)
                {
                    qty += product.Demands.QtyToForecasts;
                }
            }

            return qty;
        }
    }

    /// <summary>
    /// The total of all Required Qties for Safety Stock Demands.
    /// </summary>
    public decimal QtyToSafetyStock
    {
        get
        {
            List<Product> products = GetProducts(true);
            decimal qty = 0;
            for (int p = 0; p < products.Count; p++)
            {
                Product product = products[p];
                if (product.Demands != null)
                {
                    if (product.Demands.SafetyStockDemands != null)
                    {
                        qty += product.Demands.QtyToSafetyStock;
                    }
                }
            }

            return qty;
        }
    }

    #region Quantity update
    /// <summary>
    /// Set the Required qty and calculate JIT Start Date (using the new qty).
    /// Updates all the paths, operation, activities, and products with quantities and costs.
    /// </summary>
    /// <param name="newQty"></param>
    internal void SetRequiredQty(long a_simClock, decimal a_newQty, ProductRuleManager a_productRuleManager)
    {
        if (ScenarioDetail.ScenarioOptions.IsApproximatelyZeroOrLess(a_newQty))
        {
            throw new Exception(Localizer.GetErrorString("2795")); //TODO: Localize in a pt exception
        }

        decimal origQty = RequiredQty;
        RequiredQty = a_newQty;
        RequestedQty = RequiredQty; //makes more sense to keep them the same so they're consistant.
        OriginalQty = RequiredQty;

        decimal ratio = a_newQty / origQty;
        Product primaryProduct = GetPrimaryProduct();

        for (int pathI = 0; pathI < m_alternatePaths.Count; ++pathI)
        {
            AlternatePath path = (AlternatePath)m_alternatePaths.GetRow(pathI);
            if (path != CurrentPath)
            {
                path.AdjustRequiredQty(ratio, origQty, primaryProduct, a_productRuleManager);
            }
        }

        // The current path is done last in case multiple paths have common operations; this insures that the quantities in the current path are correct.
        CurrentPath.AdjustRequiredQty(ratio, a_newQty, primaryProduct, a_productRuleManager);
        CalculateJitTimes(a_simClock, false);
    }
    #endregion

    #region Related MOs
    public class ManufacturingOrderLevel
    {
        public ManufacturingOrderLevel(ManufacturingOrder a_relatedMO, int a_bomLevelOfRelation)
        {
            m_mo = a_relatedMO;
            BomLevels.Add(a_bomLevelOfRelation);
        }

        private readonly ManufacturingOrder m_mo;

        /// <summary>
        /// The Manufacturing Order being used or supplied.
        /// </summary>
        public ManufacturingOrder MO => m_mo;

        /// <summary>
        /// Indicates the relation to the original MO queried.  If the same MO is related at muliple levels, all the levels are listed here.
        /// The same level may appears multiple times.
        /// Level 0 means the MO is connected directly.
        /// </summary>
        private readonly List<int> m_bomLevels = new();

        public List<int> BomLevels => m_bomLevels;
    }

    /// <summary>
    /// Returns a list of all Manufacturing Order that are Successors to this one including any number of Successor MO Levels away.
    /// </summary>
    /// <returns></returns>
    public List<ManufacturingOrderLevel> GetSuccessorsRecursively()
    {
        Hashtable sucMoIdsHash = new();
        AddSuccessorsRecursively(sucMoIdsHash, 0);

        //Create return list
        IDictionaryEnumerator enumerator = sucMoIdsHash.GetEnumerator();
        List<ManufacturingOrderLevel> moLevels = new();
        while (enumerator.MoveNext())
        {
            ManufacturingOrderLevel moLvl = (ManufacturingOrderLevel)enumerator.Value;
            moLevels.Add(moLvl);
        }

        return moLevels;
    }

    /// <summary>
    /// Returns a list of Order Number for all Jobs that this Manufacturing Order goes into via Successor Manufacturing Orders.
    /// </summary>
    public string SuccessorOrderNumbers
    {
        get
        {
            System.Text.StringBuilder builder = new();
            List<ManufacturingOrderLevel> moSucLevels = GetSuccessorsRecursively();
            Hashtable orderNumbersHash = new();
            for (int moI = 0; moI < moSucLevels.Count; moI++)
            {
                ManufacturingOrderLevel mo = moSucLevels[moI];
                if (mo.MO.Job.OrderNumber != null && mo.MO.Job.OrderNumber.Trim().Length > 0)
                {
                    string nextOrderNumber = mo.MO.Job.OrderNumber.Trim();
                    if (!orderNumbersHash.ContainsKey(nextOrderNumber))
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(", ");
                        }

                        builder.Append(nextOrderNumber);
                        orderNumbersHash.Add(nextOrderNumber, null);
                    }
                }
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Returns a list of Customer Names for all Jobs that this Manufacturing Order goes into via Successor Manufacturing Orders.
    /// </summary>
    public string SuccessorMOCustomerNames
    {
        get
        {
            System.Text.StringBuilder builder = new();
            List<ManufacturingOrderLevel> moSucLevels = GetSuccessorsRecursively();
            Hashtable customerNamesHash = new();
            for (int moI = 0; moI < moSucLevels.Count; moI++)
            {
                ManufacturingOrderLevel mo = moSucLevels[moI];
                string customersList = mo.MO.Job.Customers.GetCustomerExternalIdsList();
                if (customersList.Length > 0)
                {
                    builder.Append(customersList);
                }
            }

            return builder.ToString();
        }
    }

    internal void AddSuccessorsRecursively(Hashtable a_sucMoIdsHash, int a_currentBomLevel)
    {
        for (int i = 0; i < SuccessorMOs.Count; i++)
        {
            SuccessorMO sucMO = SuccessorMOs[i];
            if (sucMO.SuccessorManufacturingOrder != null)
            {
                if (a_sucMoIdsHash.Contains(sucMO.SuccessorManufacturingOrder.Id))
                {
                    //foreach(SuccessorMO mo in SuccessorMOs)
                    //{
                    //    if(a_sucMoIdsHash.Contains()
                    //}
                    //if(this.Id == sucMO.ExternalId)
                    //{
                    //    throw new PTValidationException("MO '{0}' is referencing itselft as a successor MO in job '{1}'");
                    //}
                    ManufacturingOrderLevel moLvl = (ManufacturingOrderLevel)a_sucMoIdsHash[sucMO.SuccessorManufacturingOrder.Id];
                    moLvl.BomLevels.Add(a_currentBomLevel);
                }
                else
                {
                    ManufacturingOrderLevel moLvl = new(sucMO.SuccessorManufacturingOrder, a_currentBomLevel);
                    a_sucMoIdsHash.Add(sucMO.SuccessorManufacturingOrder.Id, moLvl);
                    sucMO.SuccessorManufacturingOrder.AddSuccessorsRecursively(a_sucMoIdsHash, a_currentBomLevel + 1);
                }
            }
        }
    }

    /// <summary>
    /// Returns a list of all Manufacturing Order that use Material from this one including any number of BOM Levels away.
    /// POTENTIALLY SLOW -- LOOKS THROUGH ALL JOBS
    /// </summary>
    /// <returns></returns>
    public List<ManufacturingOrderLevel> GetMaterialSuccessorsRecursively()
    {
        Hashtable sucMoIdsHash = new();
        AddMaterialSuccessorsRecursively(sucMoIdsHash, 0);

        //Create return list
        IDictionaryEnumerator enumerator = sucMoIdsHash.GetEnumerator();
        List<ManufacturingOrderLevel> moLevels = new();
        while (enumerator.MoveNext())
        {
            ManufacturingOrderLevel moLvl = (ManufacturingOrderLevel)enumerator.Value;
            moLevels.Add(moLvl);
        }

        return moLevels;
    }

    internal void AddMaterialSuccessorsRecursively(Hashtable a_sucMoIdsHash, int a_currentBomLevel)
    {
        for (int opI = 0; opI < OperationManager.Count; opI++)
        {
            BaseOperation op = OperationManager.GetByIndex(opI);
            if (op is InternalOperation)
            {
                InternalOperation iOp = (InternalOperation)op;
                for (int actI = 0; actI < iOp.Activities.Count; actI++)
                {
                    InternalActivity activity = iOp.Activities.GetByIndex(actI);
                    List<MaterialRequirement.MaterialRequirementSupply> supplies = activity.GetMaterialRequirementsSupplied();
                    for (int mrI = 0; mrI < supplies.Count; mrI++)
                    {
                        MaterialRequirement.MaterialRequirementSupply supply = supplies[mrI];
                        ManufacturingOrder suppliedMO = supply.SuppliedOperation.ManufacturingOrder;
                        if (!a_sucMoIdsHash.Contains(suppliedMO.Id))
                        {
                            ManufacturingOrderLevel moLvl = new(suppliedMO, a_currentBomLevel);
                            a_sucMoIdsHash.Add(suppliedMO.Id, moLvl);
                            suppliedMO.AddMaterialSuccessorsRecursively(a_sucMoIdsHash, a_currentBomLevel + 1);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns a list of all Manufacturing Order that use Material from this one or are Successor MOs including any number of BOM Levels away.
    /// POTENTIALLY SLOW -- LOOKS THROUGH ALL JOBS
    /// The same MO will occur multiple times in the list if it's both a Material Successor and a Successor MO.
    /// </summary>
    public List<ManufacturingOrderLevel> GetMaterialAndSucMOSuccessorsRecursively()
    {
        List<ManufacturingOrderLevel> sucMOs = GetSuccessorsRecursively();
        List<ManufacturingOrderLevel> matlSucMOs = GetMaterialSuccessorsRecursively();

        //Append the lists.
        sucMOs.AddRange(matlSucMOs);
        return sucMOs;
    }
    #endregion Related MOs

    /// <summary>
    /// The MO NeedDate minus the Shipping Buffer.
    /// The MO should be completed by this time to avoid Shipping Buffer penetration.
    /// </summary>
    public DateTime ShippingDueDate => NeedDate - GetProductShippingBuffer();

    public TimeSpan GetProductShippingBuffer()
    {
        if (ShippingBufferOverride.HasValue)
        {
            return ShippingBufferOverride.Value;
        }

        Product product = GetFirstProductInCurrentPath();
        if (product != null && product.Inventory != null)
        {
            return product.Inventory.ShippingBuffer;
        }

        return new TimeSpan(0);
    }

    /// <summary>
    /// The Release Date of the Current Path.
    /// </summary>
    public DateTime GetReleaseDate()
    {
        if (Finished)
        {
            return PTDateTime.MinDateTime;
        }

        BaseOperation leadOperation = GetLeadOperation();
        if (leadOperation is InternalOperation iOp)
        {
            InternalActivity leadAct = iOp.GetLeadActivity();
            return new DateTime(leadAct.BufferInfo.EarliestJitBufferInfo.ReleaseDate);
        }

        return PTDateTime.MinDateTime;
    }

    /// <summary>
    /// The percentage into the Shipping Buffer based on the specified time, or the Reported Finish Date if already Finished.
    /// </summary>
    /// <param name="a_currentTime"></param>
    /// <returns></returns>
    public decimal GetShippingBufferPenetration(DateTime a_currentTime)
    {
        DateTime aTime;
        if (Finished)
        {
            GetReportedFinishDate(out long reportedFinishDate);
            aTime = new DateTime(reportedFinishDate);
        }
        else
        {
            aTime = a_currentTime;
        }

        decimal hoursIntoBuffer = (decimal)aTime.Subtract(ShippingDueDate).TotalHours;
        if (GetProductShippingBuffer().TotalHours == 0)
        {
            return 0;
        }

        return 100 * hoursIntoBuffer / (decimal)GetProductShippingBuffer().TotalHours;
    }

    /// <summary>
    /// Calculates the Shipping Buffer Penetration at the APS Clock.
    /// </summary>
    public decimal ShippingBufferCurrentPenetrationPercent => GetShippingBufferPenetration(Job.ScenarioDetail.ClockDate);

    /// <summary>
    /// Calculates the Shipping Buffer Penetration using the MO's scheduled end..
    /// </summary>
    public decimal ShippingBufferProjectedPenetrationPercent => GetShippingBufferPenetration(ScheduledEndDate);

    /// <summary>
    /// Returns the sum of the Operation resource and Material costs for the current scheduled Path.
    /// </summary>
    /// <returns></returns>
    public void GetWipCosts(ref decimal r_resourceCost, ref decimal r_materialCost)
    {
        if (!Scheduled)
        {
            return;
        }

        IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();
        while (enumerator.MoveNext())
        {
            AlternatePath.Node node = enumerator.Current.Value;
            decimal rCost = 0;
            decimal mCost = 0;
            node.Operation.GetWipCosts(ref rCost, ref mCost);
            r_resourceCost += rCost;
            r_materialCost += mCost;
        }
    }

    /// <summary>
    /// Returns the sum of the Operation Carrying Costs for the current scheduled Path over the time they're being carried.
    /// </summary>
    /// <returns></returns>
    public decimal GetWipCarryingCost()
    {
        decimal cost = 0;
        if (Scheduled)
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AlternatePath.Node node = enumerator.Current.Value;
                if (node.Operation.Scheduled && node.Operation is InternalOperation)
                {
                    InternalOperation iOp = (InternalOperation)node.Operation;
                    long wipDaysTicks = ScheduledEnd - node.Operation.ScheduledStartDate.Ticks;

                    cost += (decimal)TimeSpan.FromTicks(wipDaysTicks).TotalDays * iOp.CarryingCost;
                }
            }
        }

        return cost;
    }

    /// <summary>
    /// Returns the sum of the Operation Carrying Cost for the current scheduled Path.
    /// </summary>
    /// <returns></returns>
    public decimal GetOpCarryingCost()
    {
        decimal cost = 0;
        if (Scheduled)
        {
            IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = CurrentPath.AlternateNodeSortedList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                AlternatePath.Node node = enumerator.Current.Value;
                InternalOperation iOp = (InternalOperation)node.Operation;

                if (iOp != null)
                {
                    cost += iOp.CarryingCost;
                }
            }
        }

        return cost;
    }

    /// <summary>
    /// Returns a list of all Demands from all Products.
    /// </summary>
    /// <returns></returns>
    public List<Demand.BaseDemand> GetDemands()
    {
        List<Demand.BaseDemand> demandsList = new();

        List<Product> products = GetProducts(true);
        for (int p = 0; p < products.Count; p++)
        {
            Product product = products[p];
            if (product.Demands != null)
            {
                if (product.Demands.SalesOrderDemands != null)
                {
                    foreach (SalesOrderDemand sod in product.Demands.SalesOrderDemands)
                    {
                        demandsList.Add(sod);
                    }
                }

                if (product.Demands.ForecastDemands != null)
                {
                    foreach (ForecastDemand fcd in product.Demands.ForecastDemands)
                    {
                        demandsList.Add(fcd);
                    }
                }

                if (product.Demands.TransferOrderDemands != null)
                {
                    foreach (TransferOrderDemand tod in product.Demands.TransferOrderDemands)
                    {
                        demandsList.Add(tod);
                    }
                }

                if (product.Demands.SafetyStockDemands != null)
                {
                    foreach (SafetyStockDemand sftyD in product.Demands.SafetyStockDemands)
                    {
                        demandsList.Add(sftyD);
                    }
                }

                if (product.Demands.DeletedDemands != null)
                {
                    foreach (DeletedDemand dd in product.Demands.DeletedDemands)
                    {
                        demandsList.Add(dd);
                    }
                }
            }
        }

        return demandsList;
    }

    /// <summary>
    /// A summary of the Demands that the MO is satisfying.
    /// </summary>
    public string DemandSummary
    {
        get
        {
            List<Product> products = GetProducts(true);
            System.Text.StringBuilder soBuilder = new();
            System.Text.StringBuilder fcastBuilder = new();
            System.Text.StringBuilder finalBuilder = new();

            //Sales Orders
            for (int p = 0; p < products.Count; p++)
            {
                Product product = products[p];
                if (product.Demands != null)
                {
                    if (product.Demands.SalesOrderDemands != null)
                    {
                        foreach (SalesOrderDemand sod in product.Demands.SalesOrderDemands)
                        {
                            soBuilder.AppendLine(string.Format("{0}\t{1}\t\t{2:N2}\t\t\t{3:N2}\t\t{4}",
                                sod.SalesOrderLineDistribution.SalesOrderLine.SalesOrder.Name,
                                sod.SalesOrderLineDistribution.SalesOrderLine.SalesOrder.Customer != null ? sod.SalesOrderLineDistribution.SalesOrderLine.SalesOrder.Customer.ExternalId : "",
                                sod.RequiredQty,
                                sod.SalesOrderLineDistribution.QtyOrdered,
                                sod.RequiredDate.ToDisplayTime()));
                        }
                    }
                }
            }

            if (soBuilder.Length > 0)
            {
                finalBuilder.AppendLine(string.Format("{0}\t{1}\t\t{2}\t\t{3}\t\t{4}", Localizer.GetString("Sales Order"), Localizer.GetString("Customer"), Localizer.GetString("Required Qty"), Localizer.GetString("Order Qty"), Localizer.GetString("Required Date")));
                finalBuilder.Append(soBuilder.ToString());
                finalBuilder.Append(Environment.NewLine);
            }

            //Forecasts
            for (int p = 0; p < products.Count; p++)
            {
                Product product = products[p];
                if (product.Demands != null)
                {
                    if (product.Demands.ForecastDemands != null)
                    {
                        foreach (ForecastDemand fDemand in product.Demands.ForecastDemands)
                            foreach (ForecastDemand kvp in product.Demands.ForecastDemands)
                            {
                                fcastBuilder.AppendLine(string.Format("{0}\t\t{1}\t\t{2:N2}\t\t\t{3:N2}\t\t{4}", fDemand.ForecastShipment.Forecast.Name, fDemand.ForecastShipment.Forecast.Customer.ExternalId, fDemand.RequiredQty, fDemand.ForecastShipment.RequiredQty, fDemand.ForecastShipment.RequiredDate.ToDisplayTime()));
                            }
                    }
                }
            }

            if (fcastBuilder.Length > 0)
            {
                finalBuilder.AppendLine(string.Format("{0}\t\t{1}\t\t{2}\t\t{3}\t{4}", Localizer.GetString("Forecast"), Localizer.GetString("Customer"), Localizer.GetString("Required Qty"), Localizer.GetString("Shipment Qty"), Localizer.GetString("Required Date")));
                finalBuilder.Append(fcastBuilder.ToString());
                finalBuilder.Append(Environment.NewLine);
            }

            if (QtyToSafetyStock > 0)
            {
                finalBuilder.Append(string.Format("Safety Stock: {0:N2}".Localize(), QtyToSafetyStock));
            }

            return finalBuilder.ToString();
        }
    }

    /// <summary>
    /// Whether any activity has any of its blocks scheduled within its Stable span.
    /// </summary>
    /// <param name="a_spanCalc">Used to calculate the resource stable span.</param>
    /// <returns></returns>
    internal bool AnyActivityInStableSpan()
    {
        return OperationManager.AnyActivityInStableSpan();
    }

    internal void DeletingDemand(BaseIdObject a_demand, PTTransmissionBase a_t, BaseIdList a_distributionsToDelete = null)
    {
        for (int i = 0; i < OperationManager.Count; i++)
        {
            OperationManager.GetByIndex(i).DeletingDemand(a_demand, a_t, a_distributionsToDelete);
        }
    }

    /// <summary>
    /// Apply planned scrap quantities to all operations
    /// </summary>
    public void ApplyPlannedScrapQty()
    {
        for (int i = 0; i < OperationManager.Count; i++)
        {
            BaseOperation baseOperation = OperationManager.GetByIndex(i);
            baseOperation.ApplyPlannedScrapQty();
        }
    }

    /// <summary>
    /// Delete any serialized references that are not needed for a template
    /// </summary>
    internal void ClearAllJobReferences()
    {
        for (int i = 0; i < OperationManager.Count; i++)
        {
            OperationManager.GetByIndex(i).ClearAllJobReferences();
        }
    }
}

public class SupplyingMoList
{
    /// <summary>
    /// Creates a list of MOs that are supplying material to the specified Jobs.
    /// </summary>
    public SupplyingMoList(JobManager jobManager, BaseIdList suppliedJobList, bool includeOriginalMOsInReturnList, bool recurse)
    {
        HashSet<BaseId> addedMoIdsHash = new();
        foreach (BaseId jobId in suppliedJobList)
        {
            Job job = jobManager.GetById(jobId);
            if (job != null)
            {
                for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
                {
                    ManufacturingOrder mo = job.ManufacturingOrders[moI];
                    if (includeOriginalMOsInReturnList && !addedMoIdsHash.Contains(mo.Id))
                    {
                        addedMoIdsHash.Add(mo.Id);
                        supplyingMoList.Add(mo);
                    }

                    AddSupplyingMOs(mo, addedMoIdsHash, recurse);
                }
            }
        }
    }

    /// <summary>
    /// Creates a list of MOs that are supplying material to a Manufacturing Order.
    /// </summary>
    public SupplyingMoList(ManufacturingOrder aSuppliedMO, bool recurse)
    {
        HashSet<BaseId> addedMoIdsHash = new();
        AddSupplyingMOs(aSuppliedMO, addedMoIdsHash, recurse);
    }

    /// <summary>
    /// Add an MO for each MO that supplies this MO.
    /// </summary>
    private void AddSupplyingMOs(ManufacturingOrder mo, HashSet<BaseId> addedMoIdsHash, bool recurse)
    {
        //Get a list of all MOs that supply this MO.
        for (int nodeI = 0; nodeI < mo.CurrentPath.NodeCount; nodeI++)
        {
            AlternatePath.Node node = mo.CurrentPath.GetNodeByIndex(nodeI);
            BaseOperation bOp = node.Operation;
            for (int matI = 0; matI < bOp.MaterialRequirements.Count; matI++)
            {
                MaterialRequirement mr = bOp.MaterialRequirements[matI];
                //Exclude materials with tooling connections
                if (!mr.BuyDirect && mr.Item.ItemType != ItemDefs.itemTypes.Resource)
                {
                    foreach (InternalActivity supplyingActivity in mr.SupplyingActivities)
                    {
                        ManufacturingOrder supplyingMO = supplyingActivity.Operation.ManufacturingOrder;
                        if (addedMoIdsHash.Add(supplyingMO.Id))
                        {
                            supplyingMoList.Add(supplyingMO);
                            if (recurse)
                            {
                                AddSupplyingMOs(supplyingMO, addedMoIdsHash, recurse);
                            }
                        }
                    }
                }
            }
        }
    }

    private readonly List<ManufacturingOrder> supplyingMoList = new();

    /// <summary>
    /// Returns a unique list containing all MOs that supplied the SuppliedMO.
    /// </summary>
    /// <returns></returns>
    public List<ManufacturingOrder> GetSupplyingMOList()
    {
        return supplyingMoList;
    }

    /// <summary>
    /// Returns an MOKeyList that contains a unique list of all the MOs passed in on the ManufacturingOrders lists.
    /// </summary>
    /// <param name="a_mosList"></param>
    /// <returns></returns>
    public static MOKeyList ConvertToMOKeyList(List<ManufacturingOrder> a_mosList)
    {
        MOKeyList moKeyList = new();
        Hashtable moIdsAdded = new();
        for (int listI = 0; listI < a_mosList.Count; listI++)
        {
            ManufacturingOrder mo = a_mosList[listI];
            if (!moIdsAdded.ContainsKey(mo.Id))
            {
                moIdsAdded.Add(mo.Id, null);
                moKeyList.Add(new ManufacturingOrderKey(mo.Job.Id, mo.Id));
            }
        }

        return moKeyList;
    }
}