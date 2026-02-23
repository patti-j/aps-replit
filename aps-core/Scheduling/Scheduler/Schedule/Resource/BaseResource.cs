
using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.APSCommon.Extensions;
using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.Scheduler.Schedule.Resource;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Abstract class used to derive all Resources.
/// </summary>
public abstract partial class BaseResource : BaseObject, IPTSerializable, IDepartmentChild, IConnectedResourceSource
{
    /// <summary>
    /// This is how long zero-length operations should be scheduled as.
    /// Currently this is hard coded to be 1 minute.
    /// </summary>
    internal const long OverrunTicks = TimeSpan.TicksPerMinute;

    #region IPTSerializable Members
    protected BaseResource(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12324)
        {
            bools = new BoolVector32(a_reader);
            a_reader.Read(out workcenter);
            a_reader.Read(out workcenterExternalId);

            a_reader.Read(out int val);
            resourceType = (BaseResourceDefs.resourceTypes)val;
            a_reader.Read(out transferSpan);
            a_reader.Read(out ganttRowHeightFactor);
            a_reader.Read(out imageFileName);
            a_reader.Read(out maxCumulativeQty);

            elligibleUsers = new EligibleUsersCollection(a_reader);
            shopViewUsers = new ShopViewUsers(a_reader);
            m_shopViewResourceOptionsRefId = new BaseId(a_reader);

            a_reader.Read(out referenceInfo.haveCell);
            if (referenceInfo.haveCell)
            {
                referenceInfo.cellId = new BaseId(a_reader);
            }

            a_reader.Read(out m_v_simSort);
        }
        else if (a_reader.VersionNumber >= 12315)
        {
            bools = new BoolVector32(a_reader);
            a_reader.Read(out workcenter);
            a_reader.Read(out workcenterExternalId);

            a_reader.Read(out int val);
            resourceType = (BaseResourceDefs.resourceTypes)val;
            a_reader.Read(out transferSpan);
            a_reader.Read(out ganttRowHeightFactor);
            a_reader.Read(out imageFileName);
            a_reader.Read(out maxCumulativeQty);

            elligibleUsers = new EligibleUsersCollection(a_reader);
            shopViewUsers = new ShopViewUsers(a_reader);
            m_shopViewResourceOptionsRefId = new BaseId(a_reader);

            a_reader.Read(out referenceInfo.haveCell);
            if (referenceInfo.haveCell)
            {
                referenceInfo.cellId = new BaseId(a_reader);
            }

            a_reader.Read(out m_v_simSort);

            new AttributesCollection(a_reader);
        }
        else if (a_reader.VersionNumber >= 12303)
        {
            bools = new BoolVector32(a_reader);
            a_reader.Read(out workcenter);
            a_reader.Read(out workcenterExternalId);

            a_reader.Read(out int val);
            resourceType = (BaseResourceDefs.resourceTypes)val;
            a_reader.Read(out transferSpan);
            a_reader.Read(out ganttRowHeightFactor);
            a_reader.Read(out imageFileName);
            a_reader.Read(out maxCumulativeQty);

            elligibleUsers = new EligibleUsersCollection(a_reader);
            shopViewUsers = new ShopViewUsers(a_reader);
            m_shopViewResourceOptionsRefId = new BaseId(a_reader);

            a_reader.Read(out referenceInfo.haveCell);
            if (referenceInfo.haveCell)
            {
                referenceInfo.cellId = new BaseId(a_reader);
            }

            a_reader.Read(out m_v_simSort);

            new ConnectorList(a_reader);
            new AttributesCollection(a_reader);
        }
        else if (a_reader.VersionNumber >= 12054)
        {
            bools = new BoolVector32(a_reader);
            a_reader.Read(out workcenter);
            a_reader.Read(out workcenterExternalId);

            int val;
            a_reader.Read(out val);
            resourceType = (BaseResourceDefs.resourceTypes)val;
            a_reader.Read(out transferSpan);
            a_reader.Read(out ganttRowHeightFactor);
            a_reader.Read(out imageFileName);
            a_reader.Read(out maxCumulativeQty);

            elligibleUsers = new EligibleUsersCollection(a_reader);
            shopViewUsers = new ShopViewUsers(a_reader);
            m_shopViewResourceOptionsRefId = new BaseId(a_reader);

            a_reader.Read(out referenceInfo.haveCell);
            if (referenceInfo.haveCell)
            {
                referenceInfo.cellId = new BaseId(a_reader);
            }

            a_reader.Read(out m_v_simSort);

            new ConnectorList(a_reader);
        }
        else if (a_reader.VersionNumber >= 410)
        {
            bools = new BoolVector32(a_reader);
            a_reader.Read(out workcenter);
            a_reader.Read(out workcenterExternalId);

            int val;
            a_reader.Read(out val);
            resourceType = (BaseResourceDefs.resourceTypes)val;
            a_reader.Read(out int sort);
            a_reader.Read(out transferSpan);
            a_reader.Read(out ganttRowHeightFactor);
            a_reader.Read(out imageFileName);
            a_reader.Read(out maxCumulativeQty);

            elligibleUsers = new EligibleUsersCollection(a_reader);
            shopViewUsers = new ShopViewUsers(a_reader);
            m_shopViewResourceOptionsRefId = new BaseId(a_reader);

            a_reader.Read(out referenceInfo.haveCell);
            if (referenceInfo.haveCell)
            {
                referenceInfo.cellId = new BaseId(a_reader);
            }

            a_reader.Read(out m_v_simSort);

            new ConnectorList(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        bools.Serialize(a_writer);

        a_writer.Write(workcenter);
        a_writer.Write(workcenterExternalId);
        a_writer.Write((int)resourceType);
        a_writer.Write(transferSpan);
        a_writer.Write(ganttRowHeightFactor);
        a_writer.Write(imageFileName);
        a_writer.Write(maxCumulativeQty);

        elligibleUsers.Serialize(a_writer);
        shopViewUsers.Serialize(a_writer);
        m_shopViewResourceOptionsRefId.Serialize(a_writer);

        a_writer.Write(Cell != null);
        if (Cell != null)
        {
            Cell.Id.Serialize(a_writer);
        }

        a_writer.Write(m_v_simSort);
    }

    public new const int UNIQUE_ID = 8;

    public override int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(ScenarioDetail sd)
    {
        elligibleUsers.RestoreReferences();
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        a_udfManager.RestoreReferences(this, UserField.EUDFObjectType.Resources);
    }

    private ReferenceInfo referenceInfo = new ();

    private class ReferenceInfo
    {
        internal bool haveCell;
        internal BaseId cellId;
    }

    internal void RestoreReferences(CellManager cells)
    {
        if (referenceInfo.haveCell)
        {
            Cell cell = cells.GetById(referenceInfo.cellId);
            cell.AddResourceAssociation(this);
            AssociateCell(cell);
        }

        referenceInfo = null;
    }

    internal void RestoreShopViewOptionsReferences(ShopViewResourceOptionsManager mgr)
    {
        if (m_shopViewResourceOptionsRefId != BaseId.NULL_ID)
        {
            ShopViewResourceOptions options = mgr[m_shopViewResourceOptionsRefId];
            shopViewResourceOptions = options;
        }
        else
        {
            shopViewResourceOptions = mgr.DefaultOptions;
        }
    }
    #endregion

    #region Construction
    public BaseResource(BaseId id, Department w, ShopViewResourceOptions resourceOptions)
        : base(id)
    {
        Department = w;

        //set default values
        Active = true;

        shopViewResourceOptions = resourceOptions;
    }

    protected BaseResource(BaseResource origRes, BaseId newId)
        : base(newId, origRes)
    {
        Common.Cloning.PrimitiveCloning.PrimitiveClone(origRes,
            this,
            typeof(BaseResource),
            Common.Cloning.PrimitiveCloning.Depth.Shallow,
            Common.Cloning.PrimitiveCloning.OtherIncludedTypes.All);
        Department = origRes.Department;
    }

    /// <summary>
    /// This is used for maintaining CustomOrderedListOptimized, don't use to instantiate an instance for other use.
    /// </summary>
    internal BaseResource()
        : base(new BaseId(0)) { }
    #endregion

    #region Transmission Functionality
    public bool BaseResourceReceive(ResourceIdBaseT t)
    {
        bool criticalChange = false;
        if (t is ResourceSetShopViewersT)
        {
            SetResourceShopViewers((ResourceSetShopViewersT)t);
        }

        return criticalChange;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_am"></param>
    /// <param name="a_res"></param>
    /// <param name="a_dept"></param>
    /// <param name="a_plantManager"></param>
    /// <param name="a_updateConnectors"></param>
    /// <param name="a_autoDeleteConnectors"></param>
    /// <param name="a_t"></param>
    /// <returns>Whether any updates that require a time adjustment have occurred.</returns>
    internal bool Update(UserFieldDefinitionManager a_udfManager, PT.ERPTransmissions.BaseResource a_res, Department a_dept, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;

        if (a_res.ActiveSet && a_res.Active != Active)
        {
            Active = a_res.Active;
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Id);
        }

        if (a_res.WorkcenterSet && a_res.Workcenter != Workcenter)
        {
            Workcenter = a_res.Workcenter;
            updated = true;
        }

        if (a_res.WorkcenterExternalIdSet && a_res.WorkcenterExternalId != WorkcenterExternalId)
        {
            WorkcenterExternalId = a_res.WorkcenterExternalId;
            updated = true;
        }

        if (a_res.ResourceTypeSet && a_res.ResourceType != ResourceType)
        {
            ResourceType = a_res.ResourceType;
            updated = true;
        }

        if (a_res.GanttRowHeightFactorSet && a_res.GanttRowHeightFactor != GanttRowHeightFactor)
        {
            GanttRowHeightFactor = a_res.GanttRowHeightFactor;
            updated = true;
        }

        if (a_res.DisallowDragAndDropsSet && a_res.DisallowDragAndDrops != DisallowDragAndDrops)
        {
            DisallowDragAndDrops = a_res.DisallowDragAndDrops;
            updated = true;
        }

        if (a_res.ImageFileNameSet && a_res.ImageFileName != ImageFileName)
        {
            ImageFileName = a_res.ImageFileName;
            updated = true;
        }

        if (a_res.ExcludeFromGanttsSet && ExcludeFromGantts != a_res.ExcludeFromGantts)
        {
            ExcludeFromGantts = a_res.ExcludeFromGantts;
            updated = true;
        }

        if (a_res.ManualAssignmentOnlySet && ManualSchedulingOnly != a_res.ManualAssignmentOnly)
        {
            ManualSchedulingOnly = a_res.ManualAssignmentOnly;
            updated = true;
        }

        if (a_res.MaxCumulativeQtySet && a_res.MaxCumulativeQty != MaxCumulativeQty)
        {
            MaxCumulativeQty = a_res.MaxCumulativeQty;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }
        Department = a_dept;

        updated |= Update(a_res, a_t, a_udfManager, UserField.EUDFObjectType.Resources);

        return updated;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_resEdit"></param>
    /// <returns>Whether any updates that require a time adjustment have occurred.</returns>
    internal bool Update(ResourceEdit a_resEdit, IScenarioDataChanges a_dataChanges)
    {
        bool updated = Edit(a_resEdit);

        if (a_resEdit.ActiveSet && a_resEdit.Active != Active)
        {
            Active = a_resEdit.Active;
            updated = true;
            a_dataChanges.FlagEligibilityChanges(Id);
        }

        if (a_resEdit.WorkCenterSet && a_resEdit.WorkCenter != Workcenter)
        {
            Workcenter = a_resEdit.WorkCenter;
            updated = true;
        }

        if (a_resEdit.WorkCenterExternalIdSet && a_resEdit.WorkCenterExternalId != WorkcenterExternalId)
        {
            WorkcenterExternalId = a_resEdit.WorkCenterExternalId;
            updated = true;
        }

        if (a_resEdit.ResourceTypeSet && a_resEdit.ResourceType != ResourceType)
        {
            ResourceType = a_resEdit.ResourceType;
            updated = true;
        }

        if (a_resEdit.GanttRowHeightFactorSet && a_resEdit.GanttRowHeightFactor != GanttRowHeightFactor)
        {
            GanttRowHeightFactor = a_resEdit.GanttRowHeightFactor;
            updated = true;
        }

        if (a_resEdit.DisallowDragAndDropsSet && a_resEdit.DisallowDragAndDrops != DisallowDragAndDrops)
        {
            DisallowDragAndDrops = a_resEdit.DisallowDragAndDrops;
            updated = true;
        }

        if (a_resEdit.ImageSet && a_resEdit.Image != ImageFileName)
        {
            ImageFileName = a_resEdit.Image;
            updated = true;
        }

        if (a_resEdit.ExcludeFromGanttsSet && ExcludeFromGantts != a_resEdit.ExcludeFromGantts)
        {
            ExcludeFromGantts = a_resEdit.ExcludeFromGantts;
            updated = true;
        }

        if (a_resEdit.ManualSchedulingOnlySet && ManualSchedulingOnly != a_resEdit.ManualSchedulingOnly)
        {
            ManualSchedulingOnly = a_resEdit.ManualSchedulingOnly;
            updated = true;
        }

        if (a_resEdit.MaxCumulativeQtySet && a_resEdit.MaxCumulativeQty != MaxCumulativeQty)
        {
            MaxCumulativeQty = a_resEdit.MaxCumulativeQty;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }
        
        return updated;
    }

    protected virtual bool CriticalFieldChange(string propertyName)
    {
        if (propertyName == Active_CriticalFieldChangeName)
        {
            return true;
        }

        return false;
    }
    #endregion

    #region Shared Properties
    #region Bools
    private BoolVector32 bools;
    private const int ActiveIdx = 0;

    private const int ExcludeFromGanttsIdx = 1;

    //const int ExcludeFromReportsIdx = 2; // no longer used
    private const int DisallowDragAndDropsIdx = 3;

    //const int ExcludeFromCapacityPlanIdx = 4; // no longer use, can be deleted
    private const int ManualSchedulingOnlyIdx = 5;
    private const int ExcludeExceptFromDepartmentIdx = 6;
    #endregion bools

    #region Active
    /// <summary>
    /// If not Active, then no new Activities can be scheduled to use this Resource. This does not affect Activiites already scheduled to use the Resource.
    /// This flag can only be set to false if there are no activities scheduled on the resource. Otherwise an PTValidationException will be thrown and the flags value will not be changed.
    /// </summary>
    private const string Active_CriticalFieldChangeName = "Active";

    public bool Active
    {
        get => bools[ActiveIdx];

        private set
        {
            if (value != bools[ActiveIdx])
            {
                if (value == false)
                {
                    string validationFailedMsg;
                    if (!IsActiveSettableToFalse(out validationFailedMsg))
                    {
                        throw new PTValidationException(validationFailedMsg);
                    }
                }

                bools[ActiveIdx] = value;
            }
        }
    }

    protected abstract bool IsActiveSettableToFalse(out string o_msg);
    #endregion

    /// <summary>
    /// Whether the Resource should be excluded from Resource Gantts.
    /// This is often set to true for inactive resources, etc.
    /// </summary>
    public bool ExcludeFromGantts
    {
        get => bools[ExcludeFromGanttsIdx];
        private set => bools[ExcludeFromGanttsIdx] = value;
    }

    /// <summary>
    /// Whether the Resource only be assigned manually and not during Optimizations.
    /// </summary>
    public bool ManualSchedulingOnly
    {
        get => bools[ManualSchedulingOnlyIdx];
        private set => bools[ManualSchedulingOnlyIdx] = value;
    }

    //Override ExternalId so it can be made editable so that the user can set it to match their 
    //   system ids even if they create the object in PT manually.
    /// <summary>
    /// Identifier for external system references.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.AllowEdit)]
    public override string ExternalId
    {
        get => base.ExternalId;
        internal set => base.ExternalId = value;
    }

    private string workcenter = "";

    /// <summary>
    /// The name of the workcenter in the ERP system (or Department Name if not set).  For display only.
    /// </summary>
    public string Workcenter
    {
        get
        {
            if (string.IsNullOrEmpty(workcenter)) //Return Department name. This is useful for Capacity Plan.
            {
                return Department.Name;
            }

            return workcenter;
        }

        private set => workcenter = value;
    }

    private string workcenterExternalId;

    /// <summary>
    /// The Id of the WorkCenter in the ERP system.
    /// </summary>
    public string WorkcenterExternalId
    {
        get => workcenterExternalId;
        private set => workcenterExternalId = value;
    }

    private BaseResourceDefs.resourceTypes resourceType = BaseResourceDefs.resourceTypes.Machine;

    /// <summary>
    /// The type of physical entity represented by the Resource.  Used for custom reports and other display groupings.
    /// </summary>
    public BaseResourceDefs.resourceTypes ResourceType
    {
        get => resourceType;
        private set => resourceType = value;
    }

    internal bool IsSubcontract()
    {
        return ResourceType == BaseResourceDefs.resourceTypes.Subcontractor;
    }

    internal bool IsLabor()
    {
        return ResourceType == BaseResourceDefs.resourceTypes.Employee || ResourceType == BaseResourceDefs.resourceTypes.Engineer || ResourceType == BaseResourceDefs.resourceTypes.Inspector || ResourceType == BaseResourceDefs.resourceTypes.Labor || ResourceType == BaseResourceDefs.resourceTypes.Operator || ResourceType == BaseResourceDefs.resourceTypes.Supervisor || ResourceType == BaseResourceDefs.resourceTypes.Team || ResourceType == BaseResourceDefs.resourceTypes.Technician;
    }

    private long transferSpan;

    /// <summary>
    /// Successor Operations cannot start before this time passes after finishing the Operation.  Resource is not consumed during this time.
    /// </summary>
    public TimeSpan TransferSpan
    {
        get => new (transferSpan);

        internal set => transferSpan = value.Ticks;
    }

    internal long TransferSpanTicks => transferSpan;

    /// <summary>
    /// If true then drag and drop of Activities on the Resource are not allowed.
    /// </summary>
    public bool DisallowDragAndDrops
    {
        get => bools[DisallowDragAndDropsIdx];

        private set => bools[DisallowDragAndDropsIdx] = value;
    }

    private int ganttRowHeightFactor = 10;

    /// <summary>
    /// Controls how tall the row is relative to other rows.  Valid ranges is from 1 (shortest) to 10 (tallest).
    /// </summary>
    public int GanttRowHeightFactor
    {
        get => ganttRowHeightFactor;
        private set
        {
            if (value < 1 || value > 10)
            {
                throw new PTValidationException("2270");
            }

            ganttRowHeightFactor = value;
        }
    }

    private string imageFileName;

    /// <summary>
    /// File name (with suffix but not path) of which image to use in the Gantt, etc to represent this Resource.
    /// </summary>
    public string ImageFileName
    {
        get
        {
            if (imageFileName == "" || imageFileName == null)
            {
                //Set the default picture if not already set                
                if (ResourceType == BaseResourceDefs.resourceTypes.Employee || ResourceType == BaseResourceDefs.resourceTypes.Engineer || ResourceType == BaseResourceDefs.resourceTypes.Inspector || ResourceType == BaseResourceDefs.resourceTypes.Labor || ResourceType == BaseResourceDefs.resourceTypes.Operator || ResourceType == BaseResourceDefs.resourceTypes.Supervisor || ResourceType == BaseResourceDefs.resourceTypes.Technician)
                {
                    return "inspector";
                }

                if (ResourceType == BaseResourceDefs.resourceTypes.Team)
                {
                    return "team";
                }

                if (ResourceType == BaseResourceDefs.resourceTypes.Bay || ResourceType == BaseResourceDefs.resourceTypes.Cell || ResourceType == BaseResourceDefs.resourceTypes.WorkArea || ResourceType == BaseResourceDefs.resourceTypes.WorkCenter || ResourceType == BaseResourceDefs.resourceTypes.Container)
                {
                    return "cells";
                }

                if (ResourceType == BaseResourceDefs.resourceTypes.Equipment || ResourceType == BaseResourceDefs.resourceTypes.Fixture)
                {
                    return "machine";
                }

                if (ResourceType == BaseResourceDefs.resourceTypes.Transport || ResourceType == BaseResourceDefs.resourceTypes.Subcontractor)
                {
                    return "jitExpedite";
                }

                if (ResourceType == BaseResourceDefs.resourceTypes.Tool)
                {
                    return "tools";
                }

                if (ResourceType == BaseResourceDefs.resourceTypes.Tank)
                {
                    return "water_tank";
                }

                if (ResourceType == BaseResourceDefs.resourceTypes.Inbox)
                {
                    return "infoNoticeBubble";
                }

                return "gear";
            }

            return imageFileName;
        }
        private set => imageFileName = value;
    }

    public const string MaxCumulativeQty_FieldName = "MaxCumulativeQty"; //must match Property name below
    private decimal maxCumulativeQty;

    /// <summary>
    /// Often used to model "tank capacity" or "subcontractor capacity", this is the maximum quantity of simultaneous Activities allowed on the Resource.
    /// Violations create Block Flags. A Block is Flagged if the total Start Quantity of Activities beginning but not yet ending on the Resource exceeds this quantity.
    /// Set this value to zero to turn it off.
    /// This is not a constraint -- it is a Flag only.
    /// </summary>
    public decimal MaxCumulativeQty
    {
        get => maxCumulativeQty;
        private set => maxCumulativeQty = value;
    }
    #endregion Shared Properties

    #region Properties
    /// <summary>
    /// If more than the Plant's Bottleneck Threshold of Activites on the Resource's schedule are Capacity Bottlenecked then the Resource is flagged as a Bottleneck.
    /// </summary>
    public abstract bool Bottleneck { get; }

    /// <summary>
    /// The percent of the Resource's scheduled Activities that are Capacity Bottlenecked.
    /// </summary>
    public abstract decimal BottleneckPercent { get; }
    #endregion Properties

    public ResourceKey GetKey()
    {
        return new ResourceKey(Department.Plant.Id, Department.Id, Id);
    }

    private EligibleUsersCollection elligibleUsers = new ();

    /// <summary>
    /// Specifies User Rights to the object.
    /// </summary>
    [Browsable(false)]
    public EligibleUsersCollection ElligibleUsers //NEEDPT
    {
        get => elligibleUsers;
        private set => elligibleUsers = value;
    }

    #region Department & Plant
    private Department department;

    [Browsable(false)]
    public Department Department
    {
        get => department;
        internal set => department = value;
    }

    /// <summary>
    /// The plant that this resource is in.
    /// </summary>
    internal Plant Plant => department.Plant;
    #endregion

    #region Cell
    private Cell cell;

    [Browsable(false)]
    /// <summary>
    /// Usually represents a physical grouping of Resources in an autonomous production line.  
    /// Once an operation is scheduled in a cell successors will be scheduled within the same cell if possible during Optimizes.
    /// </summary>
    public Cell Cell
    {
        get => cell;
        private set => cell = value;
    }

    public const string CELL_NAME = "CellName"; //Must match property name below!

    /// <summary>
    /// Usually represents a physical grouping of Resources in an autonomous production line.  Once an operation is scheduled in a cell successors will be scheduled within the same cell if possible.
    /// If a Resource is assigned a Cell then when an Operation is scheduled in the Cell all successor Operations attempt to schedule on Resources in the same Cell.
    /// </summary>
    public string CellName
    {
        get
        {
            if (Cell != null)
            {
                return Cell.Name;
            }

            return "";
        }
    }

    /// <summary>
    /// Call to give the resource a chance to do any cleanup before being deleted.
    /// </summary>
    public void Deleting()
    {
        if (Cell != null)
        {
            DissassociateCell();
        }
    }

    internal void AssociateCell(Cell cell)
    {
        this.cell = cell;
    }

    public void DissassociateCell()
    {
        if (cell != null)
        {
            cell.RemoveResourceAssociation(this);
        }

        cell = null;
    }
    #endregion

    #region IDepartmentChild
    [Browsable(false)]
    [PartOfKey(true)]
    /// <summary>
    /// The Plant this Resource is in.
    /// </summary>
    public BaseId PlantId => Department.Plant.Id;

    [Browsable(true)]
    //		[ListSource(ListSourceAttribute.ListSources.Plant,false)]
    /// <summary>
    /// The Plant this Resource is in.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public string PlantName => Department.Plant.Name;

    [Browsable(false)]
    [PartOfKey(true)]
    /// <summary>
    /// The Department this Resource is in.
    /// </summary>
    public BaseId DepartmentId => department.Id;

    [Browsable(true)]
    //		[ListSource(ListSourceAttribute.ListSources.Department,false)]
    /// <summary>
    /// The Department this Resource is in.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public string DepartmentName => department.Name;
    #endregion

    #region Shop Views
    private BaseId m_shopViewResourceOptionsRefId = BaseId.NULL_ID; //For deserialization.

    public BaseId ShopViewResourceOptionsRefId
    {
        get => m_shopViewResourceOptionsRefId;
        internal set => m_shopViewResourceOptionsRefId = value;
    }

    private ShopViewResourceOptions shopViewResourceOptions;

    [Browsable(false)]
    public ShopViewResourceOptions ShopViewResourceOptions
    {
        get => shopViewResourceOptions;
        internal set => shopViewResourceOptions = value;
    }

    private void SetResourceShopViewers(ResourceSetShopViewersT t)
    {
        shopViewUsers = t.shopViewUsers;
    }

    private ShopViewUsers shopViewUsers = new ();

    [Browsable(false)]
    public ShopViewUsers ShopViewUsers => shopViewUsers;

    public int ShopViewUsersCount => shopViewUsers.Count;
    #endregion

    public override string ToString()
    {
        return string.Format("Resource: Name={0}; Description={1}; ExternalId={2}; Id={3}", Name, Description, ExternalId, Id.ToBaseType());
    }

    public override int GetHashCode()
    {
        return (int)Id.Value;
    }

    /// <summary>
    /// Creates a string key representing the resource based on the long id of the resource its department and its plant.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="d"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static string CreateResourceKey(Plant p, Department d, BaseResource r)
    {
        return CreateResourceKey(p.Id, d.Id, r.Id);
    }

    public static string CreateResourceKey(BaseId p, BaseId d, BaseId r)
    {
        return string.Format("{0};{1};{2}", p, d, r);
    }

    /// <summary>
    /// List of Connectors. Elements are of type Connector and Key is the BaseId Connetor.DownstreamResource.ID
    /// </summary>
    public class ConnectorList : CustomSortedList<ConnectorOld>
    {
        public ConnectorList(IReader a_reader)
            : base(a_reader, new ConnectorComparer()) { }

        protected override ConnectorOld CreateInstance(IReader a_reader)
        {
            return new ConnectorOld(a_reader);
        }

        private class ConnectorComparer : IKeyObjectComparer<ConnectorOld>
        {
            public int Compare(ConnectorOld a_con, ConnectorOld a_anotherCon)
            {
                return a_con.Id.CompareTo(a_anotherCon.Id);
            }

            public object GetKey(ConnectorOld a_con)
            {
                return a_con.Id;
            }
        }
    }

    /// <summary>
    /// Connected Resource Interface property
    /// </summary>
    public BaseId ResourceId => Id;
}