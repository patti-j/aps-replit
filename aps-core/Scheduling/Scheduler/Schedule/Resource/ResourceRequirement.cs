using System.ComponentModel;
using System.Text;

using PT.APSCommon;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;

namespace PT.Scheduler;

/// <summary>
/// Defines a set of requirements that a resource must have.
/// </summary>
public partial class ResourceRequirement : ExternalBaseIdObject
{
    #region IPTSerializable Members
    public ResourceRequirement(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12405)
        {
            a_reader.Read(out m_description);
            a_reader.Read(out m_attentionPercent);
            a_reader.Read(out m_blockFillImageFile);
            a_reader.Read(out int val);
            m_blockFillPattern = val;
            a_reader.Read(out val);
            m_blockFillType = (ResourceRequirementDefs.blockFillTypes)val;

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.CapabilityIds = new BaseIdList(a_reader);

            a_reader.Read(out m_referenceInfo.HaveDefaultResource);
            if (m_referenceInfo.HaveDefaultResource)
            {
                m_referenceInfo.DefaultResourceId = new BaseId(a_reader);
                m_referenceInfo.DefaultResourceDepartmentId = new BaseId(a_reader);
                m_referenceInfo.DefaultResourcePlantId = new BaseId(a_reader);
            }

            a_reader.Read(out m_defaultResource_JITLimitTicks);

            m_bools = new BoolVector32(a_reader);

            DeserializeUsage(a_reader);

            a_reader.Read(out m_capacityCode);
        }
        else if (a_reader.VersionNumber >= 601)
        {
            a_reader.Read(out m_description);
            a_reader.Read(out m_attentionPercent);
            a_reader.Read(out m_blockFillImageFile);
            a_reader.Read(out int val);
            m_blockFillPattern = val;
            a_reader.Read(out val);
            m_blockFillType = (ResourceRequirementDefs.blockFillTypes)val;

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.CapabilityIds = new BaseIdList(a_reader);

            a_reader.Read(out m_referenceInfo.HaveDefaultResource);
            if (m_referenceInfo.HaveDefaultResource)
            {
                m_referenceInfo.DefaultResourceId = new BaseId(a_reader);
                m_referenceInfo.DefaultResourceDepartmentId = new BaseId(a_reader);
                m_referenceInfo.DefaultResourcePlantId = new BaseId(a_reader);
            }

            a_reader.Read(out m_defaultResource_JITLimitTicks);

            m_bools = new BoolVector32(a_reader);

            DeserializeUsage(a_reader);
        }
    }

    private void DeserializeUsage(IReader a_reader)
    {
        a_reader.Read(out int usageStart);
        if (usageStart == 65536)
        {
            m_usageStart = MainResourceDefs.usageEnum.Clean;
        }
        else if (usageStart == 16777216)
        {
            m_usageStart = MainResourceDefs.usageEnum.Storage;
        }
        else
        {
            m_usageStart = (MainResourceDefs.usageEnum)usageStart;
        }

        a_reader.Read(out int usageEnd);
        if (usageEnd == 65536)
        {
            m_usageEnd = MainResourceDefs.usageEnum.Clean;
        }
        else if (usageEnd == 16777216)
        {
            m_usageEnd = MainResourceDefs.usageEnum.Storage;
        }
        else
        {
            m_usageEnd = (MainResourceDefs.usageEnum)usageEnd;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_description);
        writer.Write(m_attentionPercent);
        writer.Write(m_blockFillImageFile);
        writer.Write((int)m_blockFillPattern);
        writer.Write((int)m_blockFillType);

        BaseIdList capabilityList = new ();
        for (int i = 0; i < m_capabilityManager.Count; i++)
        {
            //This used to add after the first node, which didn't make any sense. If this now causes an issue, check that.
            capabilityList.Add(m_capabilityManager.GetByIndex(i).Id);
        }

        capabilityList.Serialize(writer);

        writer.Write(DefaultResource != null);
        if (DefaultResource != null)
        {
            DefaultResource.Id.Serialize(writer);
            DefaultResource.Department.Id.Serialize(writer);
            DefaultResource.Department.Plant.Id.Serialize(writer);
        }

        writer.Write(m_defaultResource_JITLimitTicks);

        m_bools.Serialize(writer);

        writer.Write((int)m_usageStart);
        writer.Write((int)m_usageEnd);

        writer.Write(m_capacityCode);
    }

    public new const int UNIQUE_ID = 314;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo
    {
        internal BaseIdList CapabilityIds;

        //Default Resource			
        internal bool HaveDefaultResource;
        internal BaseId DefaultResourceId;
        internal BaseId DefaultResourceDepartmentId;
        internal BaseId DefaultResourcePlantId;
    }

    internal void RestoreReferences(CapabilityManager capabilities, InternalOperation operation, PlantManager plants)
    {
        m_operation = operation;

        foreach (BaseId capId in m_referenceInfo.CapabilityIds)
        {
            m_capabilityManager.Add(capabilities.GetById(capId));
        }

        //Restore Default Resource
        if (m_referenceInfo.HaveDefaultResource)
        {
            Plant plant = plants.GetById(m_referenceInfo.DefaultResourcePlantId);
            Department dept = plant.Departments.GetById(m_referenceInfo.DefaultResourceDepartmentId);
            Resource resource = dept.Resources.GetById(m_referenceInfo.DefaultResourceId);
            DefaultResource = resource;
        }

        m_referenceInfo = null;
    }

    public override void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        base.AfterRestoreReferences_2(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        m_capabilityManager.ReinsertObjects(false);
    }
    #endregion

    #region Construction
    public ResourceRequirement(BaseId id, JobT.InternalOperation.ResourceRequirement jobTResourceRequirement, CapabilityManager allCapabilities, ScenarioDetail sd, InternalOperation parentOperation, IScenarioDataChanges a_dataChanges, ISystemLogger a_errorReporter)
        : base(jobTResourceRequirement.ExternalId, id)
    {
        m_operation = parentOperation;
        UsageStart = jobTResourceRequirement.UsageStart;
        UsageEnd = jobTResourceRequirement.UsageEnd;
        m_description = jobTResourceRequirement.Description;
        m_attentionPercent = jobTResourceRequirement.AttentionPercent;
        m_blockFillImageFile = jobTResourceRequirement.BlockFillImageFile;
        m_blockFillPattern = jobTResourceRequirement.BlockFillPattern;
        m_blockFillType = jobTResourceRequirement.BlockFillType;
        DefaultResource_JITLimitTicks = jobTResourceRequirement.DefaultResourceJITLimit.Ticks;
        DefaultResource_UseJITLimitTicks = jobTResourceRequirement.UseDefaultResourceJITLimit;
        CapacityCode = jobTResourceRequirement.CapacityCode;

        if (jobTResourceRequirement.DefaultResourcePlantExternalId != null && jobTResourceRequirement.DefaultResourcePlantExternalId != "" && jobTResourceRequirement.DefaultResourceDepartmentExternalId != null && jobTResourceRequirement.DefaultResourceDepartmentExternalId != "" && jobTResourceRequirement.DefaultResourceExternalId != null && jobTResourceRequirement.DefaultResourceExternalId != "")
        {
            Plant plant = sd.PlantManager.GetByExternalId(jobTResourceRequirement.DefaultResourcePlantExternalId);
            if (plant == null)
            {
                throw new PTValidationException("2796", new object[] { jobTResourceRequirement.ExternalId, jobTResourceRequirement.DefaultResourcePlantExternalId });
            }

            Department dept = plant.Departments.GetByExternalId(jobTResourceRequirement.DefaultResourceDepartmentExternalId);
            if (dept == null)
            {
                throw new PTValidationException("2797", new object[] { jobTResourceRequirement.ExternalId, jobTResourceRequirement.DefaultResourceDepartmentExternalId, plant.ExternalId });
            }

            Resource resource = dept.Resources.GetByExternalId(jobTResourceRequirement.DefaultResourceExternalId);
            if (resource == null)
            {
                throw new PTValidationException("2798", new object[] { jobTResourceRequirement.ExternalId, jobTResourceRequirement.DefaultResourceExternalId, plant.ExternalId, dept.ExternalId });
            }

            DefaultResource = resource;
        }

        AddCapabilities(jobTResourceRequirement, allCapabilities, sd, a_dataChanges, a_errorReporter);
        ValidateCapabilityCount();
    }

    internal ResourceRequirement(BaseId id, ResourceRequirement rr, InternalOperation parentOperation)
        : base(rr.ExternalId, id)
    {
        m_operation = parentOperation;
        UsageStart = rr.UsageStart;
        UsageEnd = rr.UsageEnd;
        m_description = rr.Description;
        m_attentionPercent = rr.AttentionPercent;
        m_blockFillImageFile = rr.BlockFillImageFile;
        m_blockFillPattern = rr.BlockFillPattern;
        m_blockFillType = rr.BlockFillType;
        DefaultResource_JITLimitTicks = rr.DefaultResource_JITLimitTicks;
        DefaultResource_UseJITLimitTicks = rr.DefaultResource_UseJITLimitTicks;
        CapacityCode = rr.CapacityCode;

        m_capabilityManager = (CapabilityManager)rr.CapabilityManager.Clone();
        if (rr.DefaultResource != null)
        {
            DefaultResource = rr.DefaultResource;
        }
    }

    /// <summary>
    /// Create a new ResourceRequirement based on an existing one.
    /// </summary>
    internal ResourceRequirement(ResourceRequirement sourceRR, InternalOperation parentOperation)
        : base(sourceRR.ExternalId, sourceRR.Id)
    {
        m_operation = parentOperation;
        UsageStart = sourceRR.UsageStart;
        UsageEnd = sourceRR.UsageEnd;
        m_description = sourceRR.Description;
        m_attentionPercent = sourceRR.AttentionPercent;
        m_blockFillImageFile = sourceRR.BlockFillImageFile;
        m_blockFillPattern = sourceRR.BlockFillPattern;
        m_blockFillType = sourceRR.BlockFillType;
        DefaultResource_JITLimitTicks = sourceRR.DefaultResource_JITLimitTicks;
        DefaultResource_UseJITLimitTicks = sourceRR.DefaultResource_UseJITLimitTicks;
        CapacityCode = sourceRR.CapacityCode;

        m_capabilityManager = (CapabilityManager)sourceRR.CapabilityManager.Clone();
        if (sourceRR.DefaultResource != null)
        {
            DefaultResource = sourceRR.DefaultResource;
        }
    }
    #endregion

    #region Properties
    public bool PostProcessingUsesResource => UsageEnd >= MainResourceDefs.usageEnum.PostProcessing;
    public bool StorageUsesResource => UsageEnd >= MainResourceDefs.usageEnum.Storage;
    public bool CleanUsesResource => UsageEnd >= MainResourceDefs.usageEnum.Clean;

    #region Default Resource
    private BaseResource m_defaultResource;

    /// <summary>
    /// Specifies the Resource that will be used to satisfy the Resource Requirement whenever the Operation is scheduled.
    /// If the Operation is manually moved to a different Resource then it will be Locked to the new Resource automatically
    /// to preserve the change.  If unlocked, then the next Optimize will reschedule it to the Default Resource again.
    /// This Resource is always considered eligible for the Operation even if it doesn't posess the required Capabilities.
    /// This is optional and may be null, in which case the Capabilities are used to choose a Resource.
    /// </summary>
    [Browsable(false)]
    public BaseResource DefaultResource
    {
        get => m_defaultResource;
        private set => m_defaultResource = value;
    }

    //public BaseResource DefaultResource_Set
    //{
    //    private set { m_defaultResource = value; }
    //}

    private long m_defaultResource_JITLimitTicks = TimeSpan.FromDays(7).Ticks;

    internal long DefaultResource_JITLimitTicks
    {
        get => m_defaultResource_JITLimitTicks;
        private set => m_defaultResource_JITLimitTicks = value;
    }

    /// <summary>
    /// This can be used to limit the enforcement of the Default Resource within this amount of time from the Activity's JIT Start time.
    /// This will give flexibilty during Optimization to choose other Resources if the Default Resource is unable to start the Activity on time thus minimizing the chance of it being late.
    /// </summary>
    public TimeSpan DefaultResource_JITLimit => new (DefaultResource_JITLimitTicks);

    internal void DefaultResource_Clear()
    {
        DefaultResource = null;
        DefaultResource_UseJITLimitTicks = false;
        DefaultResource_JITLimitTicks = 0;
    }

    internal void DefaultResource_Set(BaseResource a_defaultResource, bool a_useJITLimitTicks, long a_JITLimitTicks)
    {
        DefaultResource = a_defaultResource;
        DefaultResource_UseJITLimitTicks = a_useJITLimitTicks;
        DefaultResource_JITLimitTicks = a_JITLimitTicks;
    }

    internal void DefaultResource_Set(DefaultResourceValues a_defaultValues)
    {
        DefaultResource = a_defaultValues.m_defaultResource;
        DefaultResource_UseJITLimitTicks = a_defaultValues.m_useJITLimitTicks;
        DefaultResource_JITLimitTicks = a_defaultValues.m_jitLimitTicks;
    }

    internal class DefaultResourceValues
    {
        internal readonly BaseResource m_defaultResource;
        internal readonly bool m_useJITLimitTicks;
        internal readonly long m_jitLimitTicks;

        internal DefaultResourceValues(ResourceRequirement a_rr)
        {
            m_defaultResource = a_rr.DefaultResource;
            m_useJITLimitTicks = a_rr.DefaultResource_UseJITLimitTicks;
            m_jitLimitTicks = a_rr.DefaultResource_JITLimitTicks;
        }
    }
    #endregion

    #region BOOLS
    private BoolVector32 m_bools;

    private const int c_defaultResource_UseJITLimitTicks_Idx = 0;
    private const int c_allowMultipleResources = 1;

    /// <summary>
    /// If true then the DefaultResourceJITLimit will be used.  If false then the Default Resource is enforced over all times regardless of whether it will make the Activity late.
    /// </summary>
    public bool DefaultResource_UseJITLimitTicks
    {
        get => m_bools[c_defaultResource_UseJITLimitTicks_Idx];
        private set => m_bools[c_defaultResource_UseJITLimitTicks_Idx] = value;
    }

    /// <summary>
    /// Whether multiple resources may be used to satisfy this requirement.
    /// </summary>
    public bool AllowMultipleResources
    {
        get => m_bools[c_allowMultipleResources];
        private set => m_bools[c_allowMultipleResources] = value;
    }
    #endregion
    #endregion

    #region Shared properties
    // [USAGE_CODE] GetUsage()
    /// <summary>
    /// Get the requirement's usage which includes the start and end.
    /// </summary>
    /// <returns></returns>
    internal MainResourceDefs.Usage GetUsage()
    {
        return new MainResourceDefs.Usage(UsageStart, UsageEnd);
    }

    // [USAGE_CODE] SetUsage()
    /// <summary>
    /// Set the UsageStart and UsageEnd.
    /// </summary>
    /// <param name="a_usage">It is presumed the usage's end is greater than or equal to its start.</param>
    private void SetUsage(MainResourceDefs.Usage a_usage)
    {
        UsageStart = a_usage.Start;
        UsageEnd = a_usage.End;
    }

    private MainResourceDefs.usageEnum m_usageStart;

    // [USAGE_CODE] UsageStart: The first usage required.
    /// <summary>
    /// The first processing step this requirement is defined to take place in.
    /// </summary>
    public MainResourceDefs.usageEnum UsageStart
    {
        get => m_usageStart;
        private set => m_usageStart = value;
    }

    private MainResourceDefs.usageEnum m_usageEnd;

    // [USAGE_CODE] UsageEnd: The final usage required.
    /// <summary>
    /// The last processing step this requirement is defined to take place in.
    /// It must be greater than or equal to the UsageStart.
    /// </summary>
    public MainResourceDefs.usageEnum UsageEnd
    {
        get => m_usageEnd;
        private set => m_usageEnd = value;
    }

    private string m_description = "";

    /// <summary>
    /// Text that describes the purpose or source of the Resource Requirement.
    /// </summary>
    public string Description => m_description;

    private int m_attentionPercent = 100;

    /// <summary>
    /// The percent of the Resource's attention consumed by this Resource Requirement.
    /// For example, two Operations with Attention Percents of 60% and 40% could schedule "stacked" on each other in the Gantt -- running simultaneously.
    /// A maximum of 100% summed across all simultaneous Operations is enforced.
    /// For this to have an effect, the Resource's Capacity Type must be set to Multi-Tasking and the Activity's PeopleUsage must be set to Use All Available.
    /// </summary>
    public int AttentionPercent
    {
        get => m_attentionPercent;
        private set => m_attentionPercent = value;
    }

    private ResourceRequirementDefs.blockFillTypes m_blockFillType = ResourceRequirementDefs.blockFillTypes.Default;

    /// <summary>
    /// Specifies how the blocks in the Gantt should be filled.
    /// </summary>
    public ResourceRequirementDefs.blockFillTypes BlockFillType
    {
        get => m_blockFillType;
        private set => m_blockFillType = value;
    }

    private int m_blockFillPattern;

    /// <summary>
    /// If BlockFillType is set to Pattern, then this pattern is used to fill blocks in the Gantt.
    /// This can be used to visually differentiate different types of operations or resources.
    /// </summary>
    public int BlockFillPattern
    {
        get => m_blockFillPattern;
        private set => m_blockFillPattern = value;
    }

    private string m_blockFillImageFile;

    /// <summary>
    /// If BlockFillType is set to Image, then this image is used to fill blocks in the Gantt (or the Resource Image is used is this is left blank).
    /// This can be used to visually differentiate different types of operations or resources.
    /// The values specified is the full name of the file such as: myfile.png.
    /// These image files are in the ResourceImages folder under the client executable (with the images displayed in the Resource grid).
    /// </summary>
    public string BlockFillImageFile
    {
        get => m_blockFillImageFile;
        private set => m_blockFillImageFile = value;
    }

    private string m_capacityCode;

    /// <summary>
    /// A capacity group code that can be constrained to run only on capacity intervals with the same code
    /// </summary>
    public string CapacityCode
    {
        get => m_capacityCode;
        private set => m_capacityCode = value;
    }
    #endregion

    #region Capabilities
    private CapabilityManager m_capabilityManager = new (null);

    [Browsable(false)]
    /// <summary>
    /// The capabilities a machine must have to satisfy this resource requirement.
    /// </summary>
    public CapabilityManager CapabilityManager => m_capabilityManager;

    /// <summary>
    /// Resets the capabilities to the current requirements
    /// </summary>
    /// <param name="a_capabilities"></param>
    public void AddNewCapabilities(List<Capability> a_capabilities)
    {
        m_capabilityManager = new CapabilityManager(null);
        foreach (Capability capability in a_capabilities)
        {
            m_capabilityManager.Add(capability);
        }
    }

    private void AddCapabilities(JobT.InternalOperation.ResourceRequirement a_jobTResourceRequirement, CapabilityManager a_allCapabilities, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, ISystemLogger a_errorReporter)
    {
        for (int i = 0; i < a_jobTResourceRequirement.RequiredCapabilityCount; i++)
        {
            string newCapabilityExternalId = a_jobTResourceRequirement.GetRequiredCapability(i);
            Capability newCapability = a_allCapabilities.GetByExternalId(newCapabilityExternalId);

            if (newCapability != null)
            {
                CapabilityManager.Add(newCapability);
            }
            else
            {
                if (a_sd.ScenarioOptions.ActionOnOperationBadCapability == EActionOnOperationBadCapability.CreateCapability)
                {
                    //Create and add the Capability to the main list of Capabilities	
                    newCapability = a_allCapabilities.AutoAdd(newCapabilityExternalId, newCapabilityExternalId, a_dataChanges);

                    //Now add it to the ResourceRequirements required Capabilities
                    CapabilityManager.Add(newCapability);
                }
                else
                {
                    if (a_sd.ScenarioOptions.ActionOnOperationBadCapability == EActionOnOperationBadCapability.OmitOperation)
                    {
                        ScenarioExceptionInfo sei = new ();
                        sei.Create(a_sd);
                        m_operation.Omitted = BaseOperationDefs.omitStatuses.OmittedAutomatically;
                        a_errorReporter.LogException(new InvalidCapabilityException("2799", new object[] { newCapabilityExternalId, m_operation.ManufacturingOrder.Job.ExternalId, m_operation.ManufacturingOrder.ExternalId, m_operation.ExternalId }), sei);
                    }
                    else
                    {
                        throw new InvalidCapabilityException("2800", new object[] { newCapabilityExternalId, m_operation.ManufacturingOrder.Job.ExternalId, m_operation.ManufacturingOrder.ExternalId, m_operation.ExternalId, a_sd._scenario.Id.ToString() });
                    }
                }
            }
        }
    }

    public class InvalidCapabilityException : Transmissions.ValidationException
    {
        public InvalidCapabilityException(string msg, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(msg, a_stringParameters, a_appendHelpUrl) { }
    }

    private void ValidateCapabilityCount()
    {
        if (CapabilityManager.Count < 1)
        {
            throw new Transmissions.ValidationException("2801");
        }
    }

    private InternalOperation m_operation;

    [Browsable(false)]
    public InternalOperation Operation => m_operation;

    /// <summary>
    /// Determine whether this resource requirement requires a specified capability.
    /// </summary>
    /// <param name="c"></param>
    /// <returns>true if the capability is required.</returns>
    internal bool UsesCapability(Capability c)
    {
        return CapabilityManager.Contains(c);
    }

    public string RequiredCapabilitiesList
    {
        get
        {
            StringBuilder builder = new ();
            for (int i = 0; i < CapabilityManager.Count; i++)
            {
                builder.Append($"'{CapabilityManager.GetByIndex(i).Name}' ");
            }

            return builder.ToString();
        }
    }
    #endregion

    #region UI/Interfacing/Database/Reporting
    internal void PopulateJobDataSet(ref JobDataSet dataSet, bool primary)
    {
        JobDataSet.ResourceRequirementRow row = dataSet.ResourceRequirement.NewResourceRequirementRow();
        row.JobExternalId = Operation.ManufacturingOrder.Job.ExternalId;
        row.Description = Description;
        row.ExternalId = ExternalId;
        row.Id = Id.ToBaseType();
        row.MoExternalId = Operation.ManufacturingOrder.ExternalId;
        row.OpExternalId = Operation.ExternalId;
        row.PrimaryRequirement = primary;
        row.AttentionPercent = AttentionPercent;
        row.BlockFillImageFile = BlockFillImageFile;
        row.BlockFillPattern = BlockFillPattern.ToString();
        row.BlockFillType = BlockFillType.ToString();
        row.DefaultResourcePlantExternalId = DefaultResource != null ? DefaultResource.Department.Plant.ExternalId : "";
        row.DefaultResourceDepartmentExternalId = DefaultResource != null ? DefaultResource.Department.ExternalId : "";
        row.DefaultResourceExternalId = DefaultResource != null ? DefaultResource.ExternalId : "";
        row.DefaultResourceJITLimitHrs = DefaultResource_JITLimit.TotalHours;
        row.UseDefaultResourceJITLimit = DefaultResource_UseJITLimitTicks;
        row.UsageStart = UsageStart.ToString();
        row.UsageEnd = UsageEnd.ToString();
        row.CapacityCode = CapacityCode;

        dataSet.ResourceRequirement.AddResourceRequirementRow(row);

        //Add Capabilities
        for (int i = 0; i < CapabilityManager.Count; i++)
        {
            Capability cap = CapabilityManager.GetByIndex(i);
            JobDataSet.CapabilityRow capRow = dataSet.Capability.NewCapabilityRow();
            capRow.JobExternalId = Operation.ManufacturingOrder.Job.ExternalId;
            capRow.MoExternalId = row.MoExternalId;
            capRow.OpExternalId = row.OpExternalId;
            capRow.ResourceRequirementExternalId = row.ExternalId;
            capRow.CapabilityExternalId = cap.ExternalId;
            capRow.ResourceCount = cap.ResourceCount;
            capRow.CapabilityName = cap.Name;

            dataSet.Capability.AddCapabilityRow(capRow);
        }
    }

    public override string Analysis
    {
        get
        {
            //TODO: Maybe there is a better way to get this information? Why are we creating these sets outside of simulation. This is not threadsafe.
            //lock (m_eligibleResources)
            //{
            //    CreateResourceEligibilitySet();
            //}

            string analysis = "";
            if (m_eligibleResources.ResourceCountInAllEligibleResourceSets == 0)
            {
                analysis = string.Format("There are no Eligible Resources for Resource Requirement {0} {1}.".Localize(), ExternalId, Description);
                //See which Capabilities are problems.
                string capabilityString = Environment.NewLine + "\t\t\t To be eligible a Resource must satisfy any Setup Table Eligibility and Resource quantity constraints and have ALL of these Capabilities:".Localize() + " ";
                for (int capI = 0; capI < CapabilityManager.Count; capI++)
                {
                    Capability c = CapabilityManager.GetByIndex(capI);
                    if (c.ResourceCount == 0)
                    {
                        analysis += string.Format("{2}\t\t\t No eligible Resources for Capability: ExternalId '{0}', Name '{1}'".Localize(), c.ExternalId, c.Name, Environment.NewLine);
                    }

                    if (capI > 0)
                    {
                        capabilityString += "," + c.Name;
                    }
                    else
                    {
                        capabilityString += c.Name;
                    }
                }

                analysis += capabilityString;
            }

            return analysis;
        }
    }

    /// <summary>
    /// Returns an array list of Capabilities that have no Active eligible resources.
    /// </summary>
    /// <returns></returns>
    public SortedList<BaseId, Capability> GetCapabilitiesWithoutActiveResources()
    {
        SortedList<BaseId, Capability> problemCapabilities = new ();
        //See which Capabilities are problems.
        for (int capI = 0; capI < CapabilityManager.Count; capI++)
        {
            Capability c = CapabilityManager.GetByIndex(capI);
            if (c.ActiveResourceCount == 0)
            {
                if (!problemCapabilities.ContainsKey(c.Id))
                {
                    problemCapabilities.Add(c.Id, c);
                }
            }
        }

        return problemCapabilities;
    }
    #endregion

    #region Functions
    #region Comparison
    /// <summary>
    /// Used to describe how two resource requirements differ.
    /// </summary>
    internal enum IdenticleEnum
    {
        /// <summary>
        /// The ResourceRequirements are identicle.
        /// </summary>
        Identicle,

        /// <summary>
        /// The ResourceRequirements differ only by their descriptions.
        /// </summary>
        DifferentDescriptions,

        /// <summary>
        /// The ResourceRequirements differ in some other way. The possibilities are by: Used During, AttentionPercent, and required capabilities.
        /// </summary>
        Different
    }

    /// <summary>
    /// Determine in what ways two ResourceRequirements differ.
    /// </summary>
    /// <param name="primaryRR">The comparee.</param>
    /// <returns>How the ResourceRequirements differ.</returns>
    internal IdenticleEnum Identicle(ResourceRequirement rr)
    {
        IdenticleEnum ie = IdenticleEnum.Identicle;

        if (Description != rr.Description)
        {
            ie = IdenticleEnum.DifferentDescriptions;
        }

        if (AttentionPercent != rr.AttentionPercent)
        {
            return IdenticleEnum.Different;
        }

        if (UsageStart != rr.UsageStart)
        {
            return IdenticleEnum.Different;
        }

        if (UsageEnd != rr.UsageEnd)
        {
            return IdenticleEnum.Different;
        }

        return ie;
    }
    #endregion
    #endregion

    #region Update
    /// <summary>
    /// Update based on another ResourceRequirement. The function performs some analysis on the update and returns the analysis in the form of a
    /// ResourceRequirement.UpdateResult enumeration.
    /// </summary>
    /// <param name="a_rr"></param>
    /// <param name="a_dataChanges"></param>
    /// <param name="o_jitChanges"></param>
    /// <returns>Analysis of the update.</returns>
    internal bool Update(ResourceRequirement a_rr, IScenarioDataChanges a_dataChanges, out bool o_jitChanges)
    {
        bool updated = false;
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;
        o_jitChanges = false;

        if (ExternalId != a_rr.ExternalId)
        {
            base.Update(a_rr);
            updated = true;
        }

        if (m_attentionPercent != a_rr.m_attentionPercent)
        {
            m_attentionPercent = a_rr.m_attentionPercent;
            flagConstraintChanges = true;
            updated = true;
        }

        if (m_description != a_rr.m_description)
        {
            m_description = a_rr.m_description;
            updated = true;
        }
        
        if (UsageStart != a_rr.UsageStart)
        {
            UsageStart = a_rr.UsageStart;
            o_jitChanges = true;
            flagConstraintChanges = true;
            updated = true;
        }
        
        if (UsageEnd != a_rr.UsageEnd)
        {
            UsageEnd = a_rr.UsageEnd;
            updated = true;
            flagConstraintChanges = true;
        }
        
        if (DefaultResource != a_rr.DefaultResource)
        {
            DefaultResource = a_rr.DefaultResource;
            a_dataChanges.FlagEligibilityChanges(Id);
            o_jitChanges = true;
            updated = true;
        }
        
        if (DefaultResource_JITLimitTicks != a_rr.DefaultResource_JITLimitTicks)
        {
            DefaultResource_JITLimitTicks = a_rr.DefaultResource_JITLimitTicks;
            o_jitChanges = true;
            updated = true;
        }
        
        if (DefaultResource_UseJITLimitTicks != a_rr.DefaultResource_UseJITLimitTicks)
        {
            DefaultResource_UseJITLimitTicks = a_rr.DefaultResource_UseJITLimitTicks;
            o_jitChanges = true;
            updated = true;
        }
        
        if (m_blockFillImageFile != a_rr.m_blockFillImageFile)
        {
            m_blockFillImageFile = a_rr.m_blockFillImageFile;
            updated = true;
        }
        
        if (m_blockFillPattern != a_rr.m_blockFillPattern)
        {
            m_blockFillPattern = a_rr.m_blockFillPattern;
            updated = true;
        }
        
        if (m_blockFillType != a_rr.m_blockFillType)
        {
            m_blockFillType = a_rr.m_blockFillType;
            updated = true;
        }
        
        if (m_capacityCode != a_rr.CapacityCode)
        {
            m_capacityCode = a_rr.CapacityCode;
            flagProductionChanges = true;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        int similarityCount = 0; // The number of capabilities the old and new CapabilityManagers have in common.

        // Check whether the new capability manager contains something the old one doesn't.
        for (int capabilityI = 0; capabilityI < a_rr.CapabilityManager.Count; ++capabilityI)
        {
            Capability cRR = a_rr.CapabilityManager[capabilityI];
            Capability c = m_capabilityManager.GetByExternalId(cRR.ExternalId);

            if (c != null)
            {
                ++similarityCount;
            }
        }

        if (similarityCount == a_rr.CapabilityManager.Count && a_rr.CapabilityManager.Count == CapabilityManager.Count)
        {
            //Nothing to do
        }
        else
        {
            // Some capabilities are no longer necessary. Some capabilities have been removed.
            m_capabilityManager = (CapabilityManager)a_rr.CapabilityManager.Clone();
            updated = true;
            a_dataChanges.FlagEligibilityChanges(m_operation.Job.Id);
        }


        if (m_operation.Scheduled)
        {
            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(m_operation.Job.Id);
            }

            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(m_operation.Job.Id);
            }
        }

        return updated;
    }
    #endregion

    #region Deletion Validation
    internal void ResourceDeleteNotification(BaseResource r)
    {
        if (DefaultResource == r)
        {
            DefaultResource_Clear();
        }
    }
    #endregion

    public override string ToString()
    {
        StringBuilder sb = new ();

        sb.Append(string.Format("{0} Required Capabilities: {1}".Localize(), CapabilityManager.Count, RequiredCapabilitiesList));
        sb.Append(string.Format("; UsageStart: {0}".Localize(), UsageStart));
        sb.Append(string.Format("; UsageEnd: {0}".Localize(), UsageEnd));
        sb.Append("; Attention Percent:".Localize() + AttentionPercent);
        if (DefaultResource != null)
        {
            sb.Append("; DefaultResource:".Localize() + DefaultResource);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Whether the primary resource requirement has a default resource.
    /// </summary>
    public bool HasDefaultResource => DefaultResource != null;

    /// <summary>
    /// Whether this resource requirement needs to to use the specified usage
    /// </summary>
    /// <param name="a_usage"></param>
    /// <returns></returns>
    public bool ContainsUsage(MainResourceDefs.usageEnum a_usage)
    {
        return UsageStart <= a_usage && UsageEnd >= a_usage;
    }
}