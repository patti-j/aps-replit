using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Attributes;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule;
using PT.Scheduler.Schedule.Customers;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.Json.Serialization;

namespace PT.Scheduler;

/// <summary>
/// A collection of Manufacturing Orders that are being produced usually for a specific customer.
/// </summary>
public partial class Job : BaseOrder, ICloneable, IPTDeserializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 409;

    public Job(IReader reader, BaseIdGenerator idGen)
        : base(reader)
    {
        if (reader.VersionNumber >= 12209)
        {
            reader.Read(out int val);
            m_maintenanceMethod = (JobDefs.EMaintenanceMethod)val;
            reader.Read(out val);
            m_scheduledStatus = (JobDefs.scheduledStatuses)val;
            reader.Read(out m_failedToScheduleReason);
            reader.Read(out val);
            m_classification = (JobDefs.classifications)val;
            reader.Read(out val);
            m_commitment = (JobDefs.commitmentTypes)val;
            reader.Read(out m_hotReason);
            reader.Read(out m_importance);
            reader.Read(out m_cancelled);
            reader.Read(out m_hot);
            reader.Read(out m_latePenaltyCost);
            reader.Read(out m_priority);
            reader.Read(out m_revenue);
            reader.Read(out m_type);
            reader.Read(out m_holdReason);
            reader.Read(out m_hold);
            reader.Read(out m_holdUntilDateTicks);
            reader.Read(out m_doNotDelete);
            reader.Read(out m_doNotSchedule);
            reader.Read(out m_template);

            reader.Read(out m_entryDateTicks);
            reader.Read(out m_maxEarlyDeliverySpanTicks);
            reader.Read(out m_almostLateSpanTicks);
            reader.Read(out m_needDateTicks);
            reader.Read(out m_colorCode);
            reader.Read(out m_orderNumber);
            reader.Read(out m_customerEmail);
            reader.Read(out m_agentEmail);

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.CustomerIds = new BaseIdList(reader);
            m_referenceInfo.ForSalesOrderId = new BaseId(reader);

            m_moManager = new ManufacturingOrderManager(reader, idGen);
            m_jobBools = new BoolVector32(reader);
            reader.Read(out val);
            m_shipped = (JobDefs.ShippedStatuses)val;
            reader.Read(out m_destination);
            reader.Read(out m_printedDate);
            m_creator = new BaseId(reader);
            reader.Read(out m_shippingCost);
            reader.Read(out val);
            m_excludedReasons = (JobDefs.ExcludedReasons)val;
            m_jobTFlags = new BoolVector32(reader);
            reader.Read(out m_lowLevelCode);
        }
        else if (reader.VersionNumber >= 12054)
        {
            reader.Read(out int val);
            m_maintenanceMethod = (JobDefs.EMaintenanceMethod)val;
            reader.Read(out val);
            m_scheduledStatus = (JobDefs.scheduledStatuses)val;
            reader.Read(out m_failedToScheduleReason);
            reader.Read(out val);
            m_classification = (JobDefs.classifications)val;
            reader.Read(out val);
            m_commitment = (JobDefs.commitmentTypes)val;
            reader.Read(out m_hotReason);
            reader.Read(out m_importance);
            reader.Read(out m_cancelled);
            reader.Read(out m_hot);
            reader.Read(out m_latePenaltyCost);
            reader.Read(out m_priority);
            reader.Read(out m_revenue);
            reader.Read(out m_type);
            reader.Read(out m_holdReason);
            reader.Read(out m_hold);
            reader.Read(out m_holdUntilDateTicks);
            reader.Read(out m_doNotDelete);
            reader.Read(out m_doNotSchedule);
            reader.Read(out m_template);

            reader.Read(out m_entryDateTicks);
            reader.Read(out m_maxEarlyDeliverySpanTicks);
            reader.Read(out m_almostLateSpanTicks);
            reader.Read(out m_needDateTicks);
            reader.Read(out m_colorCode);
            reader.Read(out m_orderNumber);
            reader.Read(out m_customerEmail);
            reader.Read(out m_agentEmail);

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.CustomerIds = new BaseIdList(reader);
            m_referenceInfo.ForSalesOrderId = new BaseId(reader);

            m_moManager = new ManufacturingOrderManager(reader, idGen);
            new ChangeOrderList(reader, this);
            m_jobBools = new BoolVector32(reader);
            reader.Read(out val);
            m_shipped = (JobDefs.ShippedStatuses)val;
            reader.Read(out m_destination);
            reader.Read(out m_printedDate);
            m_creator = new BaseId(reader);
            reader.Read(out m_shippingCost);
            reader.Read(out val);
            m_excludedReasons = (JobDefs.ExcludedReasons)val;
            m_jobTFlags = new BoolVector32(reader);
            reader.Read(out m_lowLevelCode);
        }
        else if (reader.VersionNumber >= 12025)
        {
            reader.Read(out int val);
            m_maintenanceMethod = (JobDefs.EMaintenanceMethod)val;
            reader.Read(out val);
            m_scheduledStatus = (JobDefs.scheduledStatuses)val;
            reader.Read(out m_failedToScheduleReason);
            reader.Read(out val);
            m_classification = (JobDefs.classifications)val;
            reader.Read(out val);
            m_commitment = (JobDefs.commitmentTypes)val;
            reader.Read(out m_hotReason);
            reader.Read(out m_importance);
            reader.Read(out m_cancelled);
            reader.Read(out m_hot);
            reader.Read(out m_latePenaltyCost);
            reader.Read(out m_priority);
            reader.Read(out m_revenue);
            reader.Read(out m_type);
            reader.Read(out m_holdReason);
            reader.Read(out m_hold);
            reader.Read(out m_holdUntilDateTicks);
            reader.Read(out m_doNotDelete);
            reader.Read(out m_doNotSchedule);
            reader.Read(out m_template);

            reader.Read(out m_entryDateTicks);
            reader.Read(out m_maxEarlyDeliverySpanTicks);
            reader.Read(out m_almostLateSpanTicks);
            reader.Read(out m_needDateTicks);
            reader.Read(out m_colorCode);
            reader.Read(out m_orderNumber);
            reader.Read(out m_customerEmail);
            reader.Read(out m_agentEmail);

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.CustomerIds = new BaseIdList(reader);
            m_referenceInfo.ForSalesOrderId = new BaseId(reader);

            m_moManager = new ManufacturingOrderManager(reader, idGen);
            new ChangeOrderList(reader, this);
            m_jobBools = new BoolVector32(reader);
            reader.Read(out val);
            m_shipped = (JobDefs.ShippedStatuses)val;
            reader.Read(out m_destination);
            // _travelerReport is obsolete now, just discard
            reader.Read(out string _travelerReport);
            reader.Read(out m_printedDate);
            m_creator = new BaseId(reader);
            reader.Read(out m_shippingCost);
            reader.Read(out val);
            m_excludedReasons = (JobDefs.ExcludedReasons)val;
            m_jobTFlags = new BoolVector32(reader);
            reader.Read(out m_lowLevelCode);
        }
        else if (reader.VersionNumber >= 12001)
        {
            reader.Read(out int val);
            m_maintenanceMethod = (JobDefs.EMaintenanceMethod)val;
            reader.Read(out val);
            m_scheduledStatus = (JobDefs.scheduledStatuses)val;
            reader.Read(out m_failedToScheduleReason);
            reader.Read(out val);
            m_classification = (JobDefs.classifications)val;
            reader.Read(out val);
            m_commitment = (JobDefs.commitmentTypes)val;
            reader.Read(out m_hotReason);
            reader.Read(out m_importance);
            reader.Read(out m_cancelled);
            reader.Read(out m_hot);
            reader.Read(out m_latePenaltyCost);
            reader.Read(out m_priority);
            reader.Read(out m_revenue);
            reader.Read(out m_type);
            reader.Read(out m_holdReason);
            reader.Read(out m_hold);
            reader.Read(out m_holdUntilDateTicks);
            reader.Read(out m_doNotDelete);
            reader.Read(out m_doNotSchedule);
            reader.Read(out m_template);

            reader.Read(out m_entryDateTicks);
            reader.Read(out m_maxEarlyDeliverySpanTicks);
            reader.Read(out m_almostLateSpanTicks);
            reader.Read(out m_needDateTicks);
            reader.Read(out m_colorCode);
            reader.Read(out m_orderNumber);
            reader.Read(out m_customerEmail);
            reader.Read(out m_agentEmail);
            reader.Read(out int customerAlerts);
            reader.Read(out int agentAlerts);
            //Obsolete JobAlertList
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                reader.Read(out DateTime createDate);
                reader.Read(out DateTime endDate);
                reader.Read(out string agentEmail);
                reader.Read(out string customerEmail);
                reader.Read(out bool onHold);
                reader.Read(out string holdReason);
                reader.Read(out bool late);

                reader.Read(out int jobStatus);
                reader.Read(out int recipients);
            }

            m_referenceInfo = new ReferenceInfo();
            m_referenceInfo.CustomerIds = new BaseIdList(reader);
            m_referenceInfo.ForSalesOrderId = new BaseId(reader);

            m_moManager = new ManufacturingOrderManager(reader, idGen);
            new ChangeOrderList(reader, this);
            m_jobBools = new BoolVector32(reader);
            reader.Read(out val);
            m_shipped = (JobDefs.ShippedStatuses)val;
            reader.Read(out m_destination);
            // _travelerReport is obsolete now, just discard
            reader.Read(out string _travelerReport);
            reader.Read(out m_printedDate);
            m_creator = new BaseId(reader);
            reader.Read(out m_shippingCost);
            reader.Read(out val);
            m_excludedReasons = (JobDefs.ExcludedReasons)val;
            m_jobTFlags = new BoolVector32(reader);
            reader.Read(out m_lowLevelCode);
        }
        else
        {
            m_referenceInfo = new ReferenceInfo();
            string customerExternalId;
            BaseId customerId = BaseId.NULL_ID;
            if (reader.VersionNumber >= 726)
            {
                int val;
                reader.Read(out val);
                m_maintenanceMethod = (JobDefs.EMaintenanceMethod)val;
                reader.Read(out val);
                m_scheduledStatus = (JobDefs.scheduledStatuses)val;
                reader.Read(out m_failedToScheduleReason);
                reader.Read(out val);
                m_classification = (JobDefs.classifications)val;
                reader.Read(out val);
                m_commitment = (JobDefs.commitmentTypes)val;
                reader.Read(out customerExternalId);
                reader.Read(out m_hotReason);
                reader.Read(out m_importance);
                reader.Read(out m_cancelled);
                reader.Read(out m_hot);
                reader.Read(out m_latePenaltyCost);
                reader.Read(out m_priority);
                reader.Read(out m_revenue);
                reader.Read(out m_type);
                reader.Read(out m_holdReason);
                reader.Read(out m_hold);
                reader.Read(out m_holdUntilDateTicks);
                reader.Read(out m_doNotDelete);
                reader.Read(out m_doNotSchedule);
                reader.Read(out m_template);

                reader.Read(out m_entryDateTicks);
                reader.Read(out m_maxEarlyDeliverySpanTicks);
                reader.Read(out m_almostLateSpanTicks);
                reader.Read(out m_needDateTicks);
                reader.Read(out m_colorCode);
                reader.Read(out m_orderNumber);
                reader.Read(out m_customerEmail);
                reader.Read(out m_agentEmail);
                reader.Read(out int customerAlerts);
                reader.Read(out int agentAlerts);

                //Obsolete JobAlertList
                reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    reader.Read(out DateTime createDate);
                    reader.Read(out DateTime endDate);
                    reader.Read(out string agentEmail);
                    reader.Read(out string customerEmail);
                    reader.Read(out bool onHold);
                    reader.Read(out string holdReason);
                    reader.Read(out bool late);

                    reader.Read(out int jobStatus);
                    reader.Read(out int recipients);
                }

                m_referenceInfo = new ReferenceInfo();
                customerId = new BaseId(reader);
                m_referenceInfo.ForSalesOrderId = new BaseId(reader);

                m_moManager = new ManufacturingOrderManager(reader, idGen);
                new ChangeOrderList(reader, this);
                m_jobBools = new BoolVector32(reader);
                reader.Read(out val);
                m_shipped = (JobDefs.ShippedStatuses)val;
                reader.Read(out m_destination);
                // _travelerReport is obsolete now, just discard
                reader.Read(out string _travelerReport);
                reader.Read(out m_printedDate);
                m_creator = new BaseId(reader);
                reader.Read(out m_shippingCost);
                reader.Read(out val);
                m_excludedReasons = (JobDefs.ExcludedReasons)val;
                m_jobTFlags = new BoolVector32(reader);
                reader.Read(out m_lowLevelCode);
            }

            m_referenceInfo.CustomerIds = new BaseIdList();
            if (customerId != BaseId.NULL_ID)
            {
                m_referenceInfo.CustomerIds.Add(customerId);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write((int)m_maintenanceMethod);
        writer.Write((int)m_scheduledStatus);
        writer.Write(m_failedToScheduleReason);
        writer.Write((int)m_classification);
        writer.Write((int)m_commitment);
        writer.Write(m_hotReason);
        writer.Write(m_importance);
        writer.Write(m_cancelled);
        writer.Write(m_hot);
        writer.Write(m_latePenaltyCost);
        writer.Write(m_priority);
        writer.Write(m_revenue);
        writer.Write(m_type);
        writer.Write(m_holdReason);
        writer.Write(m_hold);
        writer.Write(m_holdUntilDateTicks);
        writer.Write(m_doNotDelete);
        writer.Write(m_doNotSchedule);
        writer.Write(m_template);

        writer.Write(m_entryDateTicks);
        writer.Write(m_maxEarlyDeliverySpanTicks);
        writer.Write(m_almostLateSpanTicks);
        writer.Write(m_needDateTicks);
        writer.Write(m_colorCode);
        writer.Write(m_orderNumber);
        writer.Write(m_customerEmail);
        writer.Write(m_agentEmail);

        BaseIdList customerList = new();
        foreach (Customer customer in m_customerCollection)
        {
            customerList.Add(customer.Id);
        }

        customerList.Serialize(writer);

        if (m_ctpSalesOrder != null)
        {
            m_ctpSalesOrder.Id.Serialize(writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(writer);
        }

        m_moManager.Serialize(writer);
        m_jobBools.Serialize(writer);

        writer.Write((int)m_shipped);
        writer.Write(m_destination);
        writer.Write(m_printedDate);
        m_creator.Serialize(writer);
        writer.Write(m_shippingCost);
        writer.Write((int)m_excludedReasons);
        m_jobTFlags.Serialize(writer);
        writer.Write(m_lowLevelCode);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly CustomerCollection m_customerCollection = new();

    [Browsable(false)]
    public CustomerCollection Customers => m_customerCollection;

    [NonSerialized] private ReferenceInfo m_referenceInfo;

    private class ReferenceInfo
    {
        internal BaseIdList CustomerIds;
        internal BaseId ForSalesOrderId;
    }

    internal void RestoreReferences(CustomerManager a_customers, PlantManager a_plants, CapabilityManager a_capabilities, ScenarioDetail a_sd, WarehouseManager a_warehouses, ItemManager a_items, ISystemLogger a_errorReporter)
    {
        foreach (BaseId customerId in m_referenceInfo.CustomerIds)
        {
            Customer customer = a_customers.GetById(customerId);
            AddCustomer(customer);
        }

        if (m_referenceInfo.ForSalesOrderId != BaseId.NULL_ID)
        {
            Demand.SalesOrder so = a_sd.SalesOrderManager.GetById(m_referenceInfo.ForSalesOrderId);
            m_ctpSalesOrder = so;
            CTP = true; // CTP used to be true if m_ctpSalesOrder != null. Now, this is stored independently, so even if SO is deleted, Job remains a CTP Job.
        }

        RestoreReferences(a_sd);
        m_moManager.RestoreReferences(a_sd, this, a_plants, a_capabilities, a_warehouses, a_items, a_errorReporter);
        m_referenceInfo = null;
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        a_udfManager.RestoreReferences(this, UserField.EUDFObjectType.Jobs);
        m_moManager.RestoreReferences(a_udfManager);
    }

    internal void ClearCustomers()
    {
        for (int i = m_customerCollection.Count - 1; i >= 0; i--)
        {
            m_customerCollection.Remove(i);
        }
    }

    internal void AddCustomer(Customer a_customer)
    {
        if (!m_customerCollection.Contains(a_customer.Id))
        {
            m_customerCollection.Add(a_customer);
        }
    }

    /// <summary>
    /// Work that can only be done after RestoreReferences() has been called on every job.
    /// </summary>
    internal void RestoreReferences2()
    {
        m_moManager.RestoreReferences2();
        m_referenceInfo = null;
    }

    #region Construction
    internal Job(BaseId a_id, ScenarioDetail a_sd, DateTimeOffset a_entryDate, BaseId a_instigator, ISystemLogger a_errorReporter)
        : base(a_id, a_sd)
    {
        m_creator = a_instigator;
        NeedDateTicks = a_sd.ClockDate.Date.Ticks;
        EntryDate = a_entryDate;
        m_moManager = new ManufacturingOrderManager(this, a_sd, a_sd.IdGen, a_errorReporter);
        m_referenceInfo = new ReferenceInfo();
        m_referenceInfo.CustomerIds = new BaseIdList();
        m_referenceInfo.ForSalesOrderId = BaseId.NULL_ID;
    }

    internal Job(bool erpUpdate, BaseId id, JobT.Job jobTJob, DateTimeOffset jobTTimeStamp, CapabilityManager machineCapabilities, ScenarioDetail scenarioDetail, BaseId instigator, PTTransmission t, IScenarioDataChanges a_dataChanges, ISystemLogger a_errorReporter, UserFieldDefinitionManager a_udfManager)
        : base(id, jobTJob, scenarioDetail, a_udfManager, UserField.EUDFObjectType.Jobs)
    {
        try
        {
            CreatingJob = true; //flag to prevent the Job from being set to Finished when still adding Operations.

            m_creator = erpUpdate ? BaseId.ERP_ID : instigator;

            NeedDateTicks = scenarioDetail.ClockDate.Date.Ticks;
            EntryDate = jobTTimeStamp;

            m_moManager = new ManufacturingOrderManager(this, scenarioDetail, scenarioDetail.IdGen, a_errorReporter);
            Id = id;
            SetSharedProperties(jobTJob, erpUpdate, new ScenarioDataChanges()); //true to update planner fields since creating the job, not updating

            JobTMOProcessing(a_udfManager, scenarioDetail.Clock, erpUpdate, jobTJob, scenarioDetail, t, a_dataChanges);

            CreatingJob = false; //clear flag set above; This must be set only after doing the JobTMOProcessing.
        }
        catch (PTValidationException e)
        {
            //need to get job external id in
            throw new PTValidationException("2199", e, true, new object[] { jobTJob.ExternalId });
        }
    }

    private static readonly string s_notScheduledLocalized = "<Not Scheduled>".Localize();
    private static readonly string s_notScheduledOrLockedLocalized = "<Not Scheduled or PlantLocked>".Localize();

    private void ValidateHasProductForTemplates()
    {
        if (Template && GetPrimaryProduct() == null)
        {
            throw new PTValidationException("2862", new object[] { ExternalId });
        }
    }

    private readonly bool m_creatingJob;

    /// <summary>
    /// Flag to indicate that the Job is in the process of being created.
    /// </summary>
    private bool CreatingJob
    {
        get => m_creatingJob;
        init => m_creatingJob = value;
    }

    public class JobException : PTException
    {
        public JobException(string e)
            : base(e) { }
    }

    /// <summary>
    /// Call Job.InitGenerated on the copy after you have set its values.
    /// </summary>
    /// <returns></returns>
    internal Job CreateUnitializedCopy()
    {
        long sizeOfCopy;
        Job jCopy = (Job)Serialization.CopyInMemory(this, new Serialization.CopyCreatorDelegate(JobCreator), out sizeOfCopy);
        return jCopy;
    }

    /// <summary>
    /// Helper of GenerateJobs().
    /// </summary>
    internal object JobCreator(IReader reader)
    {
        Job j = new(reader, ScenarioDetail.IdGen);
        return j;
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "Job".Localize();

    /// <summary>
    /// Special summary or troubleshooting information.
    /// </summary>
    [Browsable(false)]
    public override string Analysis
    {
        get
        {
            string analysis = "";
            if (Overdue)
            {
                analysis += string.Format("OVERDUE {0}".Localize(), (PTDateTime.UtcNow.RemoveSeconds().Subtract(NeedDateTime)).ToReadableStringHourPrecision());
            }

            if (Late)
            {
                analysis += string.Format("{1}LATE {0}".Localize(), Lateness.ToReadableStringHourPrecision(), Environment.NewLine);
            }

            if (Finished)
            {
                analysis += Environment.NewLine + "Finished".Localize();
            }

            if (ScheduledStatus != JobDefs.scheduledStatuses.Scheduled)
            {
                analysis += Environment.NewLine + ScheduledStatus;
            }

            LinkedList<ManufacturingOrder> unsatisfiableMOs = GetUnSatisfiableListOfMOs();
            if (unsatisfiableMOs != null)
            {
                analysis += string.Format("{0}This job contains some ManufacturingOrders that can't be scheduled; some causes may include: the job's CanSpanPlants setting, the ManufacturingOrder's CanSpanPlants setting, unsatisfiable ResourceRequirements, etc...".Localize(), Environment.NewLine);
                LinkedListNode<ManufacturingOrder> currentNode = unsatisfiableMOs.First;
                while (currentNode != null)
                {
                    ManufacturingOrder mo = currentNode.Value;
                    analysis += Environment.NewLine + string.Format("ManufacturingOrder '{0}' isn't schedulable.".Localize(), mo.Name);
                    currentNode = currentNode.Next;
                }
            }

            //Get MO Analysis
            string moAnalysis = "";
            for (int mI = 0; mI < ManufacturingOrders.Count; mI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[mI];
                string tmp = mo.Analysis;
                if (tmp != "")
                {
                    moAnalysis += string.Format("{2}{0}:{2}{1}", mo.Name, tmp, Environment.NewLine);
                }
            }

            if (moAnalysis != "")
            {
                analysis += Environment.NewLine + Environment.NewLine + "--" + "Manufacturing Orders".Localize() + "--" + moAnalysis;
            }

            return analysis.TrimStart(Environment.NewLine.ToCharArray());
        }
    }

    public string Bottlenecks
    {
        get
        {
            if (ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
            {
                System.Text.StringBuilder builder = new();
                for (int i = 0; i < ManufacturingOrders.Count; i++)
                {
                    ManufacturingOrder mo = ManufacturingOrders[i];
                    string nextMoBottleneck = mo.Bottlenecks;
                    if (nextMoBottleneck.Length > 0)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append("; ");
                        }

                        if (ManufacturingOrders.Count > 1) //else no need
                        {
                            builder.Append(mo.Name);
                        }

                        builder.Append(mo.Bottlenecks);
                    }
                }

                return builder.ToString();
            }

            return s_notScheduledLocalized;
        }
    }

    public List<BaseOperation> GetBottleneckOperations()
    {
        List<BaseOperation> ops = new();
        if (ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
        {
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                List<BaseOperation> moBottlenecks = mo.GetBottleneckOperations();
                for (int moOpI = 0; moOpI < moBottlenecks.Count; moOpI++)
                {
                    ops.Add(moBottlenecks[moOpI]);
                }
            }
        }

        return ops;
    }

    /// <summary>
    /// Returns an ArrayList of SystemMessages describing why the Job failed to schedule.
    /// </summary>
    /// <returns></returns>
    public SortedList<BaseId, Capability> GetCapabilitiesWithNoActiveResources()
    {
        SortedList<BaseId, Capability> capabilitiesWithoutActiveResources = new ();
        LinkedList<ManufacturingOrder> unsatisfiableMOs = GetUnSatisfiableListOfMOs();
        if (unsatisfiableMOs != null)
        {
            LinkedListNode<ManufacturingOrder> currentNode = unsatisfiableMOs.First;
            while (currentNode != null)
            {
                ManufacturingOrder mo = currentNode.Value;

                //Copy bad capabilities to master list for job
                SortedList<BaseId, Capability> moBadCaps = mo.GetCapabilitiesWithoutActiveResources();
                for (int i = 0; i < moBadCaps.Count; i++)
                {
                    Capability cap = moBadCaps.Values[i];
                    if (!capabilitiesWithoutActiveResources.ContainsKey(cap.Id))
                    {
                        capabilitiesWithoutActiveResources.Add(cap.Id, cap);
                    }
                }

                currentNode = currentNode.Next;
            }
        }

        return capabilitiesWithoutActiveResources;
    }
    #endregion

    #region Properties
    /// <summary>
    /// True if the Job is neither finished nor cancelled.
    /// </summary>
    [Browsable(false)]
    public bool Open => !Finished && !Cancelled;

    //TODO: Change to double/decimal
    /// <summary>
    /// The average Percent Finished of all of the Job's Manufacturing Orders weighted by their Standard Hours.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public int PercentFinished
    {
        get
        {
            decimal standardHoursFinished = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                standardHoursFinished += (decimal)mo.PercentFinished / 100 * mo.SchedulingHours;
            }

            decimal jobSchedulingHours = SchedulingHours;
            if (jobSchedulingHours > 0)
            {
                return (int)Math.Floor(standardHoursFinished / jobSchedulingHours * 100); //When returning int, we shouldn't show 100% unless it is completely finished
            }

            return 100;
        }
    }

    /// <summary>
    /// Whether any of the Job's Manufacturing orders are started.
    /// </summary>
    public bool Started
    {
        get
        {
            if (ScheduledStatus == JobDefs.scheduledStatuses.New || ScheduledStatus == JobDefs.scheduledStatuses.Template)
            {
                return false;
            }

            if (ScheduledStatus == JobDefs.scheduledStatuses.Finished)
            {
                return true;
            }

            for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];
                if (mo.Started)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Whether any of the Job's Manufacturing orders are InProcess (Started but excluding Finished operations).
    /// </summary>
    public bool InProcess
    {
        get
        {
            if (ScheduledStatus == JobDefs.scheduledStatuses.New || ScheduledStatus == JobDefs.scheduledStatuses.Template || ScheduledStatus == JobDefs.scheduledStatuses.Finished)
            {
                return false;
            }

            for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];
                if (mo.InProcess)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// The Production Status of the most advanced unfinished Activity in the Current Path of all MOs.
    /// Finished if all Activities are Finished.
    /// </summary>
    public InternalActivityDefs.productionStatuses ProductionStatus
    {
        get
        {
            InternalActivityDefs.productionStatuses status = InternalActivityDefs.productionStatuses.Waiting;
            int finishedActivitiesCount = 0;
            int activityCount = 0;
            foreach (InternalOperation op in GetOperationsFromCurrentPaths())
            {
                if (op != null)
                {
                    for (int aI = 0; aI < op.Activities.Count; aI++)
                    {
                        InternalActivity activity = op.Activities.GetByIndex(aI);
                        InternalActivityDefs.productionStatuses actStatus = activity.ProductionStatus;
                        activityCount++;
                        if (actStatus == InternalActivityDefs.productionStatuses.Finished)
                        {
                            finishedActivitiesCount++;
                        }
                        else if (actStatus > status)
                        {
                            status = actStatus;
                        }
                    }
                }
            }

            if (finishedActivitiesCount == activityCount)
            {
                return InternalActivityDefs.productionStatuses.Finished;
            }

            return status;
        }
    }

    public const string PERCENT_OVER_STANDARD_HRS = "PercentOverStandardHrs"; //must match field name

    /// <summary>
    /// If the expected hours exceeds the standard hours (and Standard Hours is not zero) then this is the percent of the Standard
    /// Hours by which the Standard Hours have been exceeded.  For example, if the Job should take 100 hours but is going to take 110 hours then this value is 10%.
    /// </summary>
    public int PercentOverStandardHrs
    {
        get
        {
            int percentOfStd = PercentOfStandardHrs;
            if (percentOfStd > 100)
            {
                return percentOfStd - 100;
            }

            return 0;
        }
    }

    /// <summary>
    /// If Standard Hours are specified then this is the Expected Hours divided by the Standard Hours.  Smaller values mean the Job ran or is running faster than standard.
    /// </summary>
    public int PercentOfStandardHrs
    {
        get
        {
            decimal stdHours = StandardHours;
            decimal expectedHours = ExpectedRunHours + ExpectedSetupHours;
            if (stdHours == 0)
            {
                return 0;
            }

            return (int)(expectedHours / stdHours * 100);
        }
    }

    /// <summary>
    /// Total of the Expected Setup Hours for all of the Manufacturing Orders.
    /// </summary>
    public decimal ExpectedSetupHours
    {
        get
        {
            decimal total = 0;
            for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];
                total += mo.ExpectedSetupHours;
            }

            return total;
        }
    }
    /// <summary>
    /// Total of the Expected Cleans Hours for all of the Manufacturing Orders.
    /// </summary>
    public decimal ExpectedCleansHours
    {
        get
        {
            decimal total = 0;
            for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];
                total += mo.ExpectedCleansHours;
            }

            return total;
        }
    }

    /// <summary>
    /// Total of the Expected Run Hours for all of the Manufacturing Orders.
    /// </summary>
    public decimal ExpectedRunHours
    {
        get
        {
            decimal total = 0;
            for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];
                total += mo.ExpectedRunHours;
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Reported Setup Hours for all Manufacturing Orders in the Job.
    /// </summary>
    public decimal ReportedSetupHours
    {
        get
        {
            decimal total = 0;
            for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];
                total += mo.ReportedSetupHours;
            }

            return total;
        }
    } /// <summary>
      /// The sum of the Reported Cleans Hours for all Manufacturing Orders in the Job.
      /// </summary>
    public decimal ReportedCleansHours
    {
        get
        {
            decimal total = 0;
            for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];
                total += mo.ReportedCleansHours;
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Reported Run Hours for all Manufacturing Orders in the Job.
    /// </summary>
    public decimal ReportedRunHours
    {
        get
        {
            decimal total = 0;
            for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];
                total += mo.ReportedRunHours;
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Scheduling Hours for all Manufacturing Orders.
    /// </summary>
    public decimal SchedulingHours
    {
        get
        {
            decimal total = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                total += ManufacturingOrders[i].SchedulingHours;
            }

            return total;
        }
    }

    /// <summary>
    /// The sum of the Standard Hours for all Manufacturing Orders.
    /// Only the hours from the operations that haven't been omitted are included in this total.
    /// </summary>
    public decimal StandardHours
    {
        get
        {
            decimal total = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                total += ManufacturingOrders[i].StandardHours;
            }

            return total;
        }
    }

    public static string EnteredTodayFieldName => "EnteredToday".Localize(); //Must match name of EnteredToday field below

    /// <summary>
    /// Summary of the Products being made by the Job.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public string Summary
    {
        get
        {
            string summary = "";
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                if (i > 0)
                {
                    summary = summary + ", ";
                }

                summary = string.Format("{0}{1} ({2} {3})", summary, mo.ProductName, mo.RequiredQty.ToString(), mo.UOM);
            }

            return summary;
        }
    }

    /// <summary>
    /// The name of the Product from the first Manufacturing Order.  If a Job contains multiple Manufacturing Orders then the Summary field lists all Product Names.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public string Product
    {
        get
        {
            if (GetFirstMO() != null)
            {
                return GetFirstMO().ProductName; //return the last MO's product since this is probably the "finished good".
            }

            return "";
        }
    }

    /// <summary>
    /// The description of the Product from the first Manufacturing Order.  If a Job contains multiple Manufacturing Orders then the value is blank.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public string ProductDescription
    {
        get
        {
            if (GetFirstMO() != null)
            {
                return GetFirstMO().ProductDescription;
            }

            return "";
        }
    }

    /// <summary>
    /// The sum of all Manufacturing Order Required Qties.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public decimal Qty
    {
        get
        {
            decimal qty = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                qty += ManufacturingOrders[i].RequiredQty;
            }

            return qty;
        }
    }

    /// <summary>
    /// Cached ScheduledEndDate value to improve performance for models with a large number of MOs.
    /// The value is cleared on SimulationInitialization. The value would only change during a simulation since the calculated fields are based on Scheduled dates
    /// This value is not serialized and as far as I can tell is not retrieved during simulation.
    /// </summary>
    private ICalculatedValueCache<DateTime> m_scheduledEndDateCache;

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// The latest Scheduled End Date of all the Job's Manufacturing Orders.
    /// </summary>
    [Browsable(true)]
    [ParenthesizePropertyName(true)]
    public DateTime ScheduledEndDate
    {
        get
        {
            if (m_scheduledEndDateCache != null && m_scheduledEndDateCache.Enabled)
            {
                if (m_scheduledEndDateCache.TryGetValue(out DateTime cachedEndDate))
                {
                    return cachedEndDate;
                }
            }

            //Temp fix for 25736
            // The temp fix being discussed here was that this calculation below use to be done in parallel,
            // but it was causing a deadlock so we do it synchronously now. 
            DateTime scheduledEnd = PTDateTime.MinDateTime;

            foreach (ManufacturingOrder mo in ManufacturingOrders)
            {
                if (mo.Scheduled)
                {
                    DateTime currentMoEndDate = mo.GetScheduledEndDate();
                    if (currentMoEndDate > scheduledEnd)
                    {
                        scheduledEnd = currentMoEndDate;
                    }
                }
            }

            m_scheduledEndDateCache?.CacheValue(scheduledEnd);
            return scheduledEnd;
        }
    }

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// The earliest Scheduled Start Date of all the Job's Manufacturing Orders.
    /// </summary>
    [Browsable(true)] //JMC TESTING SPEED
    [ParenthesizePropertyName(true)]
    public DateTime ScheduledStartDate
    {
        get
        {
            DateTime scheduledStart = PTDateTime.MaxDateTime;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                if (mo.Scheduled && mo.GetScheduledStartDate() < scheduledStart)
                {
                    scheduledStart = mo.GetScheduledStartDate();
                }
            }

            return scheduledStart;
        }
    }

    /// <summary>
    /// Whether any operation in this job is scheduled within a department's frozen span.
    /// Used for audit tracking.
    /// </summary>
    internal bool ScheduledInDepartmentFrozenSpan
    {
        get
        {
            if (ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
            {
                List<BaseOperation> operations = GetOperations();
                foreach (BaseOperation operation in operations)
                {
                    if (operation.Scheduled)
                    {
                        DateTime startDate = operation.ScheduledStartDate;
                        List<InternalResource> scheduledResources = (operation as InternalOperation).GetResourcesScheduled();
                        foreach (InternalResource scheduledResource in scheduledResources)
                        {
                            if (ScenarioDetail.ClockDate + scheduledResource.Department.FrozenSpan > startDate)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Whether any activity in this job is scheduled within the plant's stable span.
    /// Used for audit tracking.
    /// </summary>
    internal bool ScheduledInPlantStableSpan
    {
        get
        {
            return InStableSpan();
        }
    }

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// The earliest Reported Start Date of all the Job's Manufacturing Orders.
    /// </summary>
    [Browsable(true)]
    [ParenthesizePropertyName(true)]
    public DateTime ReportedStartDate
    {
        get
        {
            DateTime reportedStartDate = PTDateTime.MaxDateTime;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                mo.GetReportedStartDate(out long reportedStartTicks);
                if (mo.Finished && reportedStartTicks < reportedStartDate.Ticks)
                {
                    reportedStartDate = new DateTime(reportedStartTicks);
                }
            }

            return reportedStartDate;
        }
    }
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// The earliest Reported Finished Date of all the Job's Manufacturing Orders.
    /// </summary>
    [Browsable(true)]
    [ParenthesizePropertyName(true)]
    public DateTime ReportedFinishedDate
    {
        get
        {
            long finishedTicks = PTDateTime.MinDateTimeTicks;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                mo.GetReportedFinishDate(out long reportedFinishedTicks);
                if (mo.Finished && reportedFinishedTicks > finishedTicks)
                {
                    finishedTicks = reportedFinishedTicks;
                }
            }

            return new DateTime(finishedTicks);
        }
    }

    /// <summary>
    /// The total number of Manufacturing Orders in the Job.
    /// </summary>
    public int ManufacturingOrderCount => ManufacturingOrders.Count;

    /// <summary>
    /// The total number of Operations for all Manufacturing Orders in the Job.
    /// </summary>
    public int OperationCount => ManufacturingOrders.OperationCount;

    /// <summary>
    /// The total number of Operations for all Manufacturing Orders in the Job.
    /// </summary>
    public int FinishedOperationCount => ManufacturingOrders.FinishedOperationCount;

    /// <summary>
    /// The total number of Paths across all Manufacturing Orders.
    /// </summary>
    public int PathsCount
    {
        get
        {
            int pathsCount = 0;
            for (int i = 0; i < ManufacturingOrderCount; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                pathsCount += mo.AlternatePaths.Count;
            }

            return pathsCount;
        }
    }

    /// <summary>
    /// The total number of Paths across all Manufacturing Orders.
    /// </summary>
    public int AutoUsablePathsCount
    {
        get
        {
            int pathsCount = 0;
            for (int i = 0; i < ManufacturingOrderCount; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                for (int pathI = 0; pathI < mo.AlternatePaths.Count; pathI++)
                {
                    AlternatePath path = mo.AlternatePaths[pathI];
                    if (path.AutoUse != AlternatePathDefs.AutoUsePathEnum.IfCurrent)
                    {
                        pathsCount++;
                    }
                }
            }

            return pathsCount;
        }
    }

    /// <summary>
    /// The time between the Scheduled Start and End.
    /// </summary>
    /// <returns></returns>
    public TimeSpan LeadTime
    {
        get
        {
            if (ScheduledStatus != JobDefs.scheduledStatuses.Scheduled)
            {
                return new TimeSpan(0);
            }

            TimeSpan span = ScheduledEndDate.Subtract(ScheduledStartDate);

            if (span.Ticks < 0)
            {
                return new TimeSpan(0); //Can't have negative LeadTime.  Was getting negatives -- probably due to uninitialized dates.
            }

            return span;
        }
    }

    /// <summary>
    /// Returns the sum of the Manufacturing Order Labor and Material costs.
    /// </summary>
    /// <returns></returns>
    public void GetWipCosts(ref decimal resourceCost, ref decimal materialCost)
    {
        if (ScheduledStatus != JobDefs.scheduledStatuses.Scheduled)
        {
            return;
        }

        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            decimal rCost = 0;
            decimal mCost = 0;
            ManufacturingOrders[i].GetWipCosts(ref rCost, ref mCost);
            resourceCost += rCost;
            materialCost += mCost;
        }
    }

    /// <summary>
    /// Returns the sum of the Operation Carrying Costs multiplied by the days from the end of the MO to the MO Need Date.
    /// </summary>
    /// <returns></returns>
    public decimal GetFinishedGoodsCarryingCosts()
    {
        if (ScheduledStatus != JobDefs.scheduledStatuses.Scheduled)
        {
            return 0;
        }

        decimal cost = 0;
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            BaseOperation leadOp = mo.GetLeadOperation(); //shouldn't be null since Job is scheduled.                    
            if (mo.NeedDate.Ticks > mo.ScheduledEnd) //no carrying cost if late
            {
                long ticksEarly = mo.NeedDate.Ticks - mo.ScheduledEnd;
                cost += mo.GetOpCarryingCost() * (decimal)TimeSpan.FromTicks(ticksEarly).TotalDays;
            }
        }

        return cost;
    }

    /// <summary>
    /// Returns the sum of the Operation Carrying Costs multiplied by the days from the start of the Operation to the MO Scheduled End Date.
    /// </summary>
    /// <returns></returns>
    public decimal GetWipCarryingCosts()
    {
        decimal cost = 0;
        if (ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
        {
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                cost += ManufacturingOrders[i].GetWipCarryingCost();
            }
        }

        return cost;
    }

    public const string LATE = "Late"; //must match field name

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// True if the Job's Scheduled End Date is after its Need Date.
    /// If the Job's Classification is "Safety Stock" and the Job has Products associated with it then this value will be set to true only if the Job is later than the Safety Stock Warning Level of the Product's Inventory.
    /// Template Jobs are never Late.
    /// </summary>
    [Browsable(true)]
    public bool Late
    {
        get
        {
            if (Template || !Scheduled)
            {
                return false;
            }

            if (IgnoreLateness())
            {
                return false;
            }

            foreach (ManufacturingOrder manufacturingOrder in ManufacturingOrders)
            {
                if (manufacturingOrder.Lateness > ScenarioDetail.ScenarioOptions.JobLateThreshold)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private bool IgnoreLateness()
    {
        bool ignoreLateJobs = false;
        if (Classification == JobDefs.classifications.SafetyStock)
        {
            List<Product> products = GetProducts(true);
            if (products.Count > 0)
            {
                ignoreLateJobs = true;
                //See if any of the Products' inventories are below their SafetyStockWarningLevel
                for (int prodI = 0; prodI < products.Count; prodI++)
                {
                    Inventory inv = products[prodI].Inventory;
                    if (inv != null && inv.OnHandQty <= inv.SafetyStockWarningLevel)
                    {
                        ignoreLateJobs = false;
                        break;
                    }
                }
            }
        }

        return ignoreLateJobs;
    }

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// If ScheduledEndDate is later than LateDateTime, then it returns the difference between them,
    /// TimeSpan.Zero otherwise. 
    /// </summary>
    [Browsable(true)]
    public TimeSpan Lateness
    {
        get
        {
            //TODO: This tank check should be added to TankBatch class in V12 instead
            //InternalActivity lastActivity = GetLastActivity();
            //if (lastActivity?.Operation?.ProductionInfo is TankProductionInfo)
            //{
            //    return lastActivity.ScheduledEndOfPostProcessing >= NeedDateTime ? lastActivity.ScheduledEndOfPostProcessing.Subtract(NeedDateTime) : TimeSpan.Zero;
            //}

            return ScheduledEndDate >= LateDateTime ? ScheduledEndDate.Subtract(LateDateTime) : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Cached LatestActivity value to improve performance for models with a large number of MOs.
    /// The value is cleared on SimulationInitialization. The value would only change during a simulation since the latest activity is based on Scheduled date
    /// This value is not serialized and as far as I can tell is not retrieved during simulation.
    /// </summary>
    private ICalculatedValueCache<InternalActivity> m_cachedLatestActivity;

    /// <summary>
    /// Get last activity on job with latest ScheduledEndOfPostProcessing
    /// </summary>
    /// <returns></returns>
    private InternalActivity GetLastActivity()
    {
        if (m_cachedLatestActivity != null && m_cachedLatestActivity.TryGetValue(out InternalActivity cachedActivity))
        {
            return cachedActivity;
        }

        long latestScheduledEndOfPostProcessingTicks = PTDateTime.MinDateTime.Ticks;
        InternalActivity latestActivity = null;

        for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
        {
            ManufacturingOrder mo = ManufacturingOrders[moI];
            BaseActivity latestScheduledActivity = mo.GetLatestScheduledActivity();
            if (latestScheduledActivity is InternalActivity internalAct)
            {
                if (internalAct.Batch.PostProcessingEndDateTime.Ticks > latestScheduledEndOfPostProcessingTicks)
                {
                    latestActivity = internalAct;
                }
            }
        }

        m_cachedLatestActivity?.CacheValue(latestActivity);
        return latestActivity;
    }

    /// <summary>
    /// The amount of earliness.  Zero if on-time or late.
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetEarliness()
    {
        return NeedDateTime >= ScheduledEndDate ? NeedDateTime.Subtract(ScheduledEndDate) : TimeSpan.Zero;
    }

    /// <summary>
    /// The NeedDate minus the Scheduled End Date if early.  Zero otherwise.
    /// </summary>
    public TimeSpan Earliness => GetEarliness();

    /// <summary>
    /// True if the Job is scheduled to end earlier than its Need Date minus its Max Early Delivery Span.
    /// False if MaxEarlyDeliverSpan is zero.
    /// Templates are never TooEarly.
    /// </summary>
    public bool TooEarly =>
        !Template &&
        MaxEarlyDeliverySpan.Ticks > 0 &&
        ScheduledEndDate.Ticks < NeedDateTicks - MaxEarlyDeliverySpanTicks;

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// Don't use this field for simulation purposes becuase it uses Datetime.Now.
    /// The amount of time that has passed since the Job's Need Date.
    /// </summary>
    [Browsable(true)]
    public TimeSpan OverdueSpan
    {
        get
        {
            TimeSpan overdueSpan = PTDateTime.UtcNow.Subtract(NeedDateTime);
            if (overdueSpan.Ticks > 0)
            {
                return overdueSpan;
            }

            return new TimeSpan(0);
        }
    }

    /// <summary>
    /// The Primary Resource that the Lead Activity is scheduled on.  Null if there is no scheduled Activity.
    /// </summary>
    public BaseResource GetLeadResource()
    {
        ManufacturingOrder leadMO = GetLeadManufacturingOrder();
        if (leadMO != null)
        {
            InternalOperation io = leadMO.GetLeadOperation() as InternalOperation;

            if (io != null)
            {
                ResourceBlock rb = io.GetLeadActivity().PrimaryResourceRequirementBlock;

                if (rb != null)
                {
                    return rb.ScheduledResource;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// The earliest Start Date of the Job's scheduled Activities.  Minimum system date if there is no scheduled Activity.
    /// </summary>
    [Browsable(false)] //ScheduledStartDate is really the same thing.
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime LeadActivityStart
    {
        get
        {
            InternalActivity activity = LeadActivity;
            return activity?.ScheduledStartDate ?? PTDateTime.MinDateTime;
        }
    }

    [Browsable(false)] //ScheduledStartDate is really the same thing.
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public InternalActivity LeadActivity
    {
        get
        {
            ManufacturingOrder leadMO = GetLeadManufacturingOrder();
            if (leadMO != null)
            {
                InternalOperation leadOp = leadMO.GetLeadOperation() as InternalOperation;
                if (leadOp != null)
                {
                    return leadOp.GetLeadActivity();
                }
            }

            return null;
            ;
        }
    }

    /// <summary>
    /// The scheduled Manufacturing Order with the earliest Scheduled Start Date.  Null if there is no scheduled Manufacturing Order.
    /// </summary>
    public ManufacturingOrder GetLeadManufacturingOrder()
    {
        ManufacturingOrder leadMO = null;
        DateTime earliestStartSoFar = PTDateTime.MaxDateTime;
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            if (mo.GetLeadOperation() is InternalOperation leadOp && leadOp.ScheduledStartDate < earliestStartSoFar)
            {
                earliestStartSoFar = leadOp.ScheduledStartDate;
                leadMO = mo;
            }
        }

        return leadMO;
    }

    private JobDefs.EMaintenanceMethod m_maintenanceMethod = JobDefs.EMaintenanceMethod.ERP;

    /// <summary>
    /// How the Job was entered into the system.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public JobDefs.EMaintenanceMethod MaintenanceMethod
    {
        get => m_maintenanceMethod;
        internal set => m_maintenanceMethod = value;
    }

    private long m_entryDateTicks;

    /// <summary>
    /// Don't use this field for simulation purposes becuase it uses Datetime.Now.
    /// When the Job was entered into the system.
    /// </summary>
    public DateTimeOffset EntryDate
    {
        get => new(m_entryDateTicks, TimeSpan.Zero);
        internal set
        {
            PTDateTime.ValidateUtc(value);
            m_entryDateTicks = value.Ticks;
        }
    }

    internal long EntryDateTicks
    {
        get => m_entryDateTicks;
        private set => m_entryDateTicks = value;
    }

    internal bool Firm => m_commitment == JobDefs.commitmentTypes.Firm;

    internal bool Released => m_commitment == JobDefs.commitmentTypes.Released;

    public const string OVERDUE = "Overdue"; //must match field name

    /// <summary>
    /// Whether the Job's Need Date is in the past.
    /// Don't use this for simulation purposes since it makes use of DateTime's UtcNow.
    /// Templates are never Overdue.
    /// </summary>
    [Browsable(true)]
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public bool Overdue => !Template && Scheduled && NeedDateTime < PTDateTime.UtcNow.ToDateTime() && !Finished;

    public static readonly string ANCHORED = "Anchored"; //must match field name

    /// <summary>
    /// Anchored Activities tend to move less (in time) during Optimizations. Manual moves are allowed.
    /// </summary>
    public anchoredTypes Anchored
    {
        get
        {
            int anchoredCount = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                if (ManufacturingOrders[i].Anchored == anchoredTypes.SomeActivitiesAnchored)
                {
                    return anchoredTypes.SomeActivitiesAnchored; //No need to go further.  If one MO is partially anchored then the Job is partially anchored.
                }

                if (ManufacturingOrders[i].Anchored == anchoredTypes.Anchored)
                {
                    anchoredCount++;
                }
            }

            if (anchoredCount == 0)
            {
                return anchoredTypes.Free;
            }

            if (anchoredCount == ManufacturingOrders.Count)
            {
                return anchoredTypes.Anchored;
            }

            return anchoredTypes.Anchored;
        }
    }

    /// <summary>
    /// Anchor/UnAnchor all Manufacturing Orders in the Job.
    /// </summary>
    /// <param name="anchor"></param>
    public void Anchor(bool anchor, ScenarioOptions scenarioOptions)
    {
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrders[i].Anchor(anchor, scenarioOptions);
        }
    }

    public const string LOCKED = "Locked"; //must match field name

    /// <summary>
    /// Locked Blocks cannot be moved to different Resources during Optimizations. Manual moves can be made to different Resources.
    /// </summary>
    public lockTypes Locked
    {
        get
        {
            int lockedCount = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                if (ManufacturingOrders[i].Locked == lockTypes.SomeBlocksLocked)
                {
                    return lockTypes.SomeBlocksLocked; //No need to go further.  If one MO is partially locked then the Job is partially locked.
                }

                if (ManufacturingOrders[i].Locked == lockTypes.Locked)
                {
                    lockedCount++;
                }
            }

            if (lockedCount == 0)
            {
                return lockTypes.Unlocked;
            }

            if (lockedCount == ManufacturingOrders.Count)
            {
                return lockTypes.Locked;
            }

            return lockTypes.SomeBlocksLocked;
        }
    }

    /// <summary>
    /// Locks/Unlocks all Manufacturing Orders in the Job.
    /// </summary>
    /// <param name="lock"></param>
    public void Lock(bool lockit)
    {
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrders[i].Lock(lockit);
        }
    }

    public const string ONHOLD = "OnHold"; //must match field name

    /// <summary>
    /// Operations that are On-Hold are scheduled to start after the Hold Until Date.
    /// </summary>
    public holdTypes OnHold
    {
        get
        {
            if (Hold)
            {
                return holdTypes.OnHold;
            }

            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                if (ManufacturingOrders[i].OnHold == holdTypes.PartiallyOnHold)
                {
                    return holdTypes.PartiallyOnHold; //No need to go further.  If one MO is partially holded then the Job is partially holded.
                }

                if (ManufacturingOrders[i].OnHold == holdTypes.OnHold)
                {
                    return holdTypes.PartiallyOnHold;
                }
            }

            return holdTypes.Released;
        }
    }

    /// <summary>
    /// Holds/Releases all Manufacturing Orders in the Job.
    /// </summary>
    public void HoldIt(bool holdit, DateTime holdUntil, string holdReason)
    {
        HoldReason = holdReason;
        HoldUntil = holdUntil;
        Hold = holdit;
    }

    /// <summary>
    /// The customer will accept the order on or after this date.
    /// </summary>
    public DateTime EarliestDelivery => NeedDateTime.Subtract(MaxEarlyDeliverySpan);

    /// <summary>
    /// Call this function to determine whether there are any unfinished unsatisfiable orders.
    /// </summary>
    /// <returns>"null" if there are no unfinished unsatisfiable orders. Otherwise all the unsatisfiables are returned in a list. The list is yours.</returns>
    internal LinkedList<ManufacturingOrder> GetUnSatisfiableListOfMOs()
    {
        LinkedList<ManufacturingOrder> mol = null;

        for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
        {
            ManufacturingOrder mo = ManufacturingOrders[moI];
            if (!mo.Finished && !mo.EligibleResources_IsSatisfiable())
            {
                if (mol == null)
                {
                    mol = new LinkedList<ManufacturingOrder>();
                }

                mol.AddLast(mo);
            }
        }

        return mol;
    }

    /// <summary>
    /// Whether the job is Scheduled or Partially scheduled.
    /// </summary>
    public bool ScheduledOrPartiallyScheduled => ScheduledStatus == JobDefs.scheduledStatuses.Scheduled || ScheduledStatus == JobDefs.scheduledStatuses.PartiallyScheduled;

    private int m_lowLevelCode = -1;

    [Browsable(true)]
    public int LowLevelCode
    {
        get => m_lowLevelCode;
        set => m_lowLevelCode = value;
    }
    /// <summary>
    /// Determines if a job is a tracked actual (i.e has at least one activity finished and within the range of the track age age limit)
    /// </summary>
    public bool CalculateIsActual()
    {
        DateTime actualRange = ScenarioDetail.ClockDate.Subtract(ScenarioDetail.ScenarioOptions.TrackActualsAgeLimit);
        foreach (ManufacturingOrder manufacturingOrder in ManufacturingOrders)
        {
            for (int i = 0; i < manufacturingOrder.OperationCount; i++)
            {
                InternalOperation operation = (InternalOperation)manufacturingOrder.OperationManager.GetByIndex(i);

                for (int actIdx = 0; i < operation.Activities.Count; i++)
                {
                    InternalActivity internalActivity = operation.Activities.GetByIndex(actIdx);

                    DateTime reportedEndDate = DateTime.MinValue;

                    if (internalActivity.ReportedStartDateSet)
                    {
                        if (!internalActivity.ReportedFinishDateSet && internalActivity.ReportedSetupProcessingAndPostProcessing > TimeSpan.Zero)
                        {
                            reportedEndDate = internalActivity.ReportedStartDate.Add(internalActivity.ReportedSetupProcessingAndPostProcessing);
                        }
                    }

                    if (internalActivity.ReportedFinishDateSet)
                    {
                        reportedEndDate = internalActivity.ReportedFinishDate;
                    }

                    if (reportedEndDate >= actualRange && internalActivity.ActualResourcesUsed.Count > 0)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion Properties

    #region ChangedInternally Flags
    private BoolVector32 m_jobBools;
    private const int DoNotSchedulePreservedIdx = 0;
    private const int DoNotDeletePreservedIdx = 1;
    private const int CommitmentPreservedIdx = 2;
    private const int SplitIdx = 3;
    private const int ReviewedIdx = 4;
    private const int InvoicedIdx = 5;
    private const int PrintedIdx = 6;
    private const int c_ctpIdx = 7;
    private const int c_trackSequenceSetups = 8;

    /// <summary>
    /// Indicates that the field was changed from within the system (as opposed to from import) and so it will be preserved to avoid future imports from updating it.
    /// </summary>
    public bool DoNotSchedulePreserved
    {
        get => m_jobBools[DoNotSchedulePreservedIdx];
        private set => m_jobBools[DoNotSchedulePreservedIdx] = value;
    }

    /// <summary>
    /// Indicates that the field was changed from within the system (as opposed to from import) and so it will be preserved to avoid future imports from updating it.
    /// </summary>
    public bool DoNotDeletePreserved
    {
        get => m_jobBools[DoNotDeletePreservedIdx];
        private set => m_jobBools[DoNotDeletePreservedIdx] = value;
    }

    /// <summary>
    /// Indicates that the field was changed from within the system (as opposed to from import) and so it will be preserved to avoid future imports from updating it.
    /// </summary>
    public bool CommitmentPreserved
    {
        get => m_jobBools[CommitmentPreservedIdx];
        private set => m_jobBools[CommitmentPreservedIdx] = value;
    }

    /// <summary>
    /// Whether the Job was split from another Job.
    /// </summary>
    internal bool SplitOffFromOtherJob
    {
        get => m_jobBools[SplitIdx];
        set => m_jobBools[SplitIdx] = value;
    }
    #endregion ChangedInternallyFlags

    #region Shared Properties
    public const string CLASSIFICATION = "Classification"; //This must be kept to match the name of the Property.
    private JobDefs.classifications m_classification = JobDefs.classifications.ProductionOrder;

    /// <summary>
    /// Can be used to distinguish the purpose of the work request. For display only.
    /// A value of "Safety Stock" affects the Job Late Property based on the Iventory Safety Stock Warning Level
    /// MRP sets this value based on the reason for the creation of the Job: from Sales Order, Forecast, Transfer Order, or Safety Stock.  If a Job was created with batched requirements then the earliest
    /// requirement determines this setting.
    /// </summary>
    public JobDefs.classifications Classification
    {
        get => m_classification;
        internal set => m_classification = value;
    }

    public const string COMMITMENT = "Commitment"; //This must be kept to match the name of the field.
    private JobDefs.commitmentTypes m_commitment = JobDefs.commitmentTypes.Firm;

    /// <summary>
    /// Indicates the likelihood that the work will be executed.
    /// This value can be updated internally and will be protected from external changes that attempt
    /// to set the value "backwards" (such as from Firm to Planned).  This allows the planner to advance the
    /// Commitment level internally and not have the change undone by an external system that has not been updated.
    /// </summary>
    public JobDefs.commitmentTypes Commitment
    {
        get => m_commitment;
        internal set => m_commitment = value;
    }

    private bool m_doNotDelete;

    /// <summary>
    /// If true then the Job will not be deleted by the system.  This can be used to keep Jobs that will be used as templates for copying to new Jobs.
    /// This value can be set by the interface but not updated.  This is to preserve manual changes by the planner.
    /// </summary>
    public bool DoNotDelete
    {
        get => m_doNotDelete;
        internal set => m_doNotDelete = value;
    }

    public const string DO_NOT_SCHEDULE = "DoNotSchedule"; //must match field name.
    private bool m_doNotSchedule;

    /// <summary>
    /// If true then the Job will not be scheduled.  This can be used to prevent quotes or other un-firm Jobs from scheduling until a planner would like to do so.
    /// This value can be set by the interface but not updated.  This is to preserve manual changes by the planner.
    /// </summary>
    public bool DoNotSchedule
    {
        get => m_doNotSchedule;
        internal set
        {
            m_doNotSchedule = value;
            if (m_doNotSchedule && ScheduledStatus == JobDefs.scheduledStatuses.Scheduled)
            {
                Exclude();
            }
        }
    }

    private string m_hotReason = "";

    /// <summary>
    /// Explanation of why the Job is on Hot.
    /// </summary>
    public string HotReason
    {
        get => m_hotReason;
        internal set => m_hotReason = value;
    }

    private int m_importance;

    /// <summary>
    /// The value of the Job relative to other Jobs.  This can be used by the Balanced Composite Rule.
    /// </summary>
    public int Importance
    {
        get => m_importance;
        internal set => m_importance = value;
    }

    private bool m_cancelled;

    /// <summary>
    /// If cancelled, the Job won't be scheduled.
    /// </summary>
    public bool Cancelled
    {
        get => m_cancelled;

        internal set
        {
            if (m_cancelled != value)
            {
                m_cancelled = value;

                if (m_cancelled)
                {
                    Cancel();
                }
                else
                {
                    if (Finished)
                    {
                        ScheduledStatus_set = JobDefs.scheduledStatuses.Finished;
                    }
                    else
                    {
                        ScheduledStatus_set = JobDefs.scheduledStatuses.Unscheduled;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Call this function to cancel a job. It unschedules the job's MOs, sets the status to Cancelled, and notifies ScenarioDetail that some jobs have been cancelled.
    /// </summary>
    private void Cancel()
    {
        Unschedule(true);
        ScheduledStatus_set = JobDefs.scheduledStatuses.Cancelled;
        ScenarioDetail.JobsUnscheduled();
        m_bools.Clear(); //Canceled jobs do not initialize. We need to clear any past set values.
        ResetSimulationStateVariables(ScenarioDetail);
    }

    private bool m_hot;

    /// <summary>
    /// Means th Job is especially important.  This can be used by the Balanced Composite Rule.
    /// </summary>
    public bool Hot
    {
        get => m_hot;
        internal set => m_hot = value;
    }

    public const string COLOR_CODE = "ColorCode"; //This must be kept to match the name of the field.
    private Color m_colorCode = Color.Empty;

    /// <summary>
    /// A Color that can be used to distinguish the Job from other Jobs in the Gantt.
    /// </summary>
    public Color ColorCode
    {
        get => m_colorCode;
        internal set => m_colorCode = value;
    }

    private decimal m_latePenaltyCost;

    /// <summary>
    /// Optional currency value that specifies the cost (either actual or estimated) per day of finishing late.  For display only.
    /// </summary>
    public decimal LatePenaltyCost
    {
        get => m_latePenaltyCost;
        private set => m_latePenaltyCost = Math.Max(0, value);
    }

    /// <summary>
    /// This is zero if on-time and equal to the LatePenaltyCost if Late.
    /// </summary>
    public decimal ExpectedLatePenaltyCost
    {
        get
        {
            if (Late)
            {
                return LatePenaltyCost;
            }

            return 0;
        }
    }

    private long m_maxEarlyDeliverySpanTicks = TimeSpan.FromDays(7).Ticks;

    /// <summary>
    /// The customer will accept the order this amount of time before the Need Date.
    /// If Operations are scheduled with more than this amount of Slack then they are considered Early.
    /// This does not impose a constraint on Optimizations or Moves.  Jobs can still be scheduled earlier than this but then they are marked as "Early".
    /// If this value is zero then E-mail Alerts are not sent to alert of early Jobs since they cannot be accepted early.
    /// </summary>
    public TimeSpan MaxEarlyDeliverySpan
    {
        get
        {
            if ((decimal)m_maxEarlyDeliverySpanTicks / TimeSpan.TicksPerDay > 1000) //was a bug where this was very large somehow and causing big problem for Anspach and Maximus
            {
                return new TimeSpan();
            }

            return new TimeSpan(m_maxEarlyDeliverySpanTicks);
        }

        private set => m_maxEarlyDeliverySpanTicks = value.Ticks;
    }

    internal long MaxEarlyDeliverySpanTicks
    {
        get => m_maxEarlyDeliverySpanTicks;
        private set => m_maxEarlyDeliverySpanTicks = value;
    }

    private long m_almostLateSpanTicks = TimeSpan.FromDays(1).Ticks;

    /// <summary>
    /// Jobs and Activities are considered AlmostLate if they end within this period from the Need Date.
    /// </summary>
    public TimeSpan AlmostLateSpan
    {
        get => new(m_almostLateSpanTicks);
        private set => m_almostLateSpanTicks = value.Ticks;
    }

    public const string NEEDDATE = "NeedDateTime"; //This must be kept to match the name of the field.
    private long m_needDateTicks;

    /// <summary>
    /// When to finish by to be considered on-time.
    /// </summary>
    [ParenthesizePropertyName(true)]
    public DateTime NeedDateTime
    {
        get => new(m_needDateTicks);
        internal set => m_needDateTicks = value.Ticks;
    }

    internal long NeedDateTicks
    {
        get => m_needDateTicks;
        set => m_needDateTicks = value;
    }

    /// <summary>
    /// Returns the latest release date of the MOs
    /// </summary>
    /// <returns></returns>
    internal long GetMaxMoReleaseDateTicks()
    {
        long maxDate = DateTime.MinValue.Ticks;
        for (int i = 0; i < ManufacturingOrderCount; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            if (mo.Hold)
            {
                if (maxDate < mo.HoldUntilTicks)
                {
                    maxDate = mo.HoldUntilTicks;
                }
            }
        }

        return maxDate;
    }

    /// <summary>
    /// Returns the latest release date of the MOs
    /// </summary>
    /// <returns></returns>
    internal long GetMaxMoBufferReleaseDateTicks()
    {
        DateTime maxDate = PTDateTime.MinDateTime;
        for (int i = 0; i < ManufacturingOrderCount; i++)
        {
            DateTime moReleaseDate = ManufacturingOrders[i].GetReleaseDate();
            if (maxDate < moReleaseDate)
            {
                maxDate = moReleaseDate;
            }
        }

        return maxDate.Ticks;
    }

    private string m_orderNumber = "";

    /// <summary>
    /// Can be used to specify the cutomer order number.  For display only.
    /// </summary>
    public string OrderNumber
    {
        get => m_orderNumber;
        internal set => m_orderNumber = value;
    }

    private int m_priority = 5;

    /// <summary>
    /// Usually used to specify a combination of importance and urgency.  This can be used by the Balanced Composite Rule.  Lower numbers are more urgent/important.
    /// </summary>
    public int Priority
    {
        get => m_priority;
        internal set => m_priority = value;
    }

    private decimal m_revenue;

    /// <summary>
    /// Real or estimated value.  For display only.
    /// </summary>
    public decimal Revenue
    {
        get => m_revenue;
        internal set => m_revenue = value;
    }

    private string m_type = "";

    /// <summary>
    /// Can be used to specify a free-form type for grouping.  For display only.
    /// </summary>
    public string Type
    {
        get => m_type;
        private set => m_type = value;
    }

    private decimal DEPRECATED_PROFIT = 0; //calculating now

    /// <summary>
    /// The expected profit for the Job.  This can be used by the Balanced Composite Rule.
    /// </summary>
    public decimal Profit => Revenue - TotalCost;

    private string m_customerEmail = "";

    /// <summary>
    /// E-mail address of the customer to be alerted when the Job changes.
    /// Separate multiple addresses with semi-colons.
    /// </summary>
    public string CustomerEmail
    {
        get => m_customerEmail;
        private set => m_customerEmail = value;
    }

    private string m_agentEmail = "";

    /// <summary>
    /// E-mail address of the sales or customer service agents to be alerted when the Job changes.
    /// Separate multiple addresses with semi-colons.
    /// </summary>
    public string AgentEmail
    {
        get => m_agentEmail;
        private set => m_agentEmail = value;
    }

    private bool m_template;

    /// <summary>
    /// Indicates that the Job is only used for copying to create new Jobs.  Template Jobs are not scheduled.
    /// </summary>
    public bool Template
    {
        get => m_template;
        internal set
        {
            if (m_template != value)
            {
                m_template = value;
                if (value) //if it's changing to a Template then unschedule it.
                {
                    Unschedule(true);
                    ScheduledStatus_set = JobDefs.scheduledStatuses.Template;
                    ScenarioDetail.JobsUnscheduled();

                    //Delete any references that are no longer needed for templates.
                    ClearAllJobReferences();
                }
                else //changing from a template to a non-Template
                {
                    ScheduledStatus_set = JobDefs.scheduledStatuses.Unscheduled; //Set to Unscheduled so it no longer appears in the Templates Job list.
                }
            }
        }
    }

    /// <summary>
    /// Delete any serialized references that are not needed for a template
    /// </summary>
    private void ClearAllJobReferences()
    {
        foreach (ManufacturingOrder mo in this.ManufacturingOrders)
        {
            mo.ClearAllJobReferences();
        }
    }

    /// <summary>
    private bool m_hold;

    /// <summary>
    /// Whether the Job was placed On-hold and work should not be done on it.
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

    public static string HOLD_REASON_FIELD = "HoldReason"; //this must match the field name below!
    private string m_holdReason = "";

    /// <summary>
    /// The reason the Job was placed On-Hold
    /// </summary>
    public string HoldReason
    {
        get => m_holdReason;
        private set => m_holdReason = value;
    }

    private long m_holdUntilDateTicks = PTDateTime.MinDateTime.Ticks;

    /// <summary>
    /// No Activities are scheduled before this date/time.
    /// This value is only set if the Job itself is placed On Hold, not if Operations, etc. only are placed On Hold.
    /// </summary>
    public DateTime HoldUntil
    {
        get => new(m_holdUntilDateTicks);
        private set => m_holdUntilDateTicks = value.Ticks;
    }

    internal long HoldUntilTicks => m_holdUntilDateTicks;

    public const string REVIEWED_PROPERTY_NAME = "Reviewed";

    /// <summary>
    /// Tracks whether the Job has been reviewed by a planner.
    /// Only set during import for NEW Jobs and thereafter controlled internally.
    /// </summary>
    public bool Reviewed //If this is renamed, rename the static above too.
    {
        get => m_jobBools[ReviewedIdx];
        set => m_jobBools[ReviewedIdx] = value;
    }

    /// <summary>
    /// Whether an invoice has been sent for the Job.
    /// For information only.
    /// </summary>
    public bool Invoiced
    {
        get => m_jobBools[InvoicedIdx];
        internal set => m_jobBools[InvoicedIdx] = value;
    }

    public const string PRINTED = "Printed"; //must match field name

    /// <summary>
    /// Whether the Job's Traveler Report has been Printed.
    /// </summary>
    public bool Printed
    {
        get => m_jobBools[PrintedIdx];
        internal set => m_jobBools[PrintedIdx] = value;
    }

    private DateTime m_printedDate;

    /// <summary>
    /// Set to the current time when a JobsPrintedT is received.
    /// </summary>
    public DateTime PrintedDate
    {
        get => m_printedDate;
        internal set => m_printedDate = value;
    }

    private string m_destination = "";

    /// <summary>
    /// Indicates the geographical region or address where the products will be sent.
    /// For information only.
    /// </summary>
    
    public string Destination
    {
        get => m_destination;
        private set => m_destination = value;
    }

    private JobDefs.ShippedStatuses m_shipped = JobDefs.ShippedStatuses.NotShipped;

    /// <summary>
    /// Whether the Job has been shipped to the recipient.
    /// For informatiion only.
    /// </summary>
    public JobDefs.ShippedStatuses Shipped
    {
        get => m_shipped;
        internal set => m_shipped = value;
    }
    #endregion Shared Properties

    #region Other Properties
    /// <summary>
    /// Returns a color based on the Priority.
    /// Priority Less than <=1=Red, 2=Orange, 3=Yellow.  Greater than 3 is White.
    /// </summary>
    public Color PriorityColor
    {
        get
        {
            if (Priority <= 1)
            {
                return ColorUtils.ColorCodes.Priority1;
            }

            if (Priority == 2)
            {
                return ColorUtils.ColorCodes.Priority2;
            }

            if (Priority == 3)
            {
                return ColorUtils.ColorCodes.Priority3;
            }

            return ColorUtils.ColorCodes.PriorityHigherThan3;
        }
    }

    /// <summary>
    /// Whether the Job finishes too close to the Need Date.  Too close depends on AlmostLateSpan.
    /// Templates are never Almost Late.
    /// </summary>
    public bool AlmostLate
    {
        get
        {
            if (Template || ScheduledStatus != JobDefs.scheduledStatuses.Scheduled)
            {
                return false;
            }

            bool ignoreLateJobs = IgnoreLateness();
            if (ignoreLateJobs)
            {
                return false;
            }

            if (ScheduledEndDate < LateDateTime)
            {
                return LateDateTime - ScheduledEndDate < AlmostLateSpan;
            }

            return false;
        }
    }

    //Calculated late date time based on configured Late Threshold span setting
    private DateTime LateDateTime => NeedDateTime.Add(ScenarioDetail.ScenarioOptions.JobLateThreshold);

    /// <summary>
    /// Color to indicate the lateness of the Job.
    /// Overdue=Red, Late=Orange, TooEarly=LightGreen, AlmostLate==Yellow, On-Time=White.
    /// </summary>
    public Color Timing
    {
        get
        {
            if (Overdue)
            {
                return ColorUtils.ColorCodes.OverdueColor;
            }

            if (Late)
            {
                return ColorUtils.ColorCodes.LateColor;
            }

            if (TooEarly)
            {
                return ColorUtils.ColorCodes.TooEarlyColor;
            }

            if (AlmostLate)
            {
                return ColorUtils.ColorCodes.AlmostLateColor;
            }

            return ColorUtils.ColorCodes.OnTimeColor;
        }
    }

    /// <summary>
    /// Totals the Required Qty for all MOs that are not Finished.
    /// </summary>
    /// <returns></returns>
    public decimal SumOfUnfinishedMORequiredQties()
    {
        decimal total = 0;
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            if (!mo.Finished)
            {
                total += mo.RequiredQty;
            }
        }

        return total;
    }

    /// <summary>
    /// Totals the Setup Hours for all Activities scheduled to start before or on the specified date.
    /// </summary>
    /// <returns></returns>
    public long SumOfSetupHoursTicks(DateTime dt, bool includeLaborResourcesOnly)
    {
        long total = 0;
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            if (mo.Scheduled)
            {
                total += mo.SumOfSetupHoursTicks(dt, includeLaborResourcesOnly);
            }
        }

        return total;
    }

    /// <summary>
    /// Totals the Setup Costs for all Activities scheduled to start before or on the specified date.
    /// </summary>
    /// <returns></returns>
    public decimal SumOfSetupCosts(DateTime dt, bool includeLaborResourcesOnly)
    {
        decimal total = 0;
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            if (mo.Scheduled)
            {
                total += mo.SumOfSetupCost(dt, includeLaborResourcesOnly);
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
        long total = 0;
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            if (mo.Scheduled)
            {
                total += mo.SumOfCleanHoursTicks(dt, includeLaborResourcesOnly);
            }
        }

        return total;
    }
    /// <summary>
    /// The name(s) of the Plant(s) the Job is scheduled in if the Job is scheduled.
    /// If the Job is not scheduled, this is the list of the Plants the MOs are Locked to if Plant Locked.
    /// Each Manufacturing Order can specify whether the MO has a Locked Plant to make work schedule to that Plant.
    /// Each Job has a Can Span Plants option which controls whether the work can span multiple Plants.        ///
    /// </summary>
    public string Plants
    {
        get
        {
            List<Plant> plantList = ScheduledPlants;

            if (plantList.Count == 0)
            {
                return s_notScheduledOrLockedLocalized;
            }

            return GetUniquePlantNames(plantList);
        }
    }

    public List<Plant> ScheduledPlants
    {
        get
        {
            List<Plant> plantList = new();
            if (Scheduled)
            {
                List<InternalResource> resourcesUsed = GetResourcesScheduled();
                for (int resI = 0; resI < resourcesUsed.Count; resI++)
                {
                    plantList.Add(resourcesUsed[resI].Plant);
                }
            }
            else //Use MO Locked Plant if specified
            {
                for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
                {
                    ManufacturingOrder mo = ManufacturingOrders[moI];
                    if (mo.LockedPlant != null)
                    {
                        plantList.Add(mo.LockedPlant);
                    }
                }
            }

            return plantList;
        }
    }

    /// <summary>
    /// The name(s) of the Department(s) the Job is scheduled in if the Job is scheduled.
    /// </summary>
    public string Departments
    {
        get
        {
            List<string> deptsNamesList = new();
            if (Scheduled)
            {
                List<InternalResource> resourcesUsed = GetResourcesScheduled();
                for (int resI = 0; resI < resourcesUsed.Count; resI++)
                {
                    deptsNamesList.Add(resourcesUsed[resI].Department.Name);
                }
            }

            if (deptsNamesList.Count == 0)
            {
                return "<Not Scheduled>".Localize();
            }

            return GetUniqueStrings(deptsNamesList);
        }
    }

    /// <summary>
    /// The name(s) of the Department(s) the Job is scheduled in if the Job is scheduled.
    /// </summary>
    public string WorkCenters
    {
        get
        {
            List<string> wcNamesList = new();
            if (Scheduled)
            {
                List<InternalResource> resourcesUsed = GetResourcesScheduled();
                for (int resI = 0; resI < resourcesUsed.Count; resI++)
                {
                    wcNamesList.Add(resourcesUsed[resI].Workcenter);
                }
            }

            if (wcNamesList.Count == 0)
            {
                return s_notScheduledLocalized;
            }

            return GetUniqueStrings(wcNamesList);
        }
    }

    /// <summary>
    /// Number of different Resources scheduled to be used for all currently scheduled Operations.
    /// </summary>
    public int ResourceCount
    {
        get
        {
            int returnCount = 0;
            if (Scheduled)
            {
                List<InternalResource> resourcesUsed = GetResourcesScheduled();
                returnCount = resourcesUsed.Count;
            }

            return returnCount;
        }
    }

    /// <summary>
    /// Number of different labor Resources scheduled to be used for all currently scheduled Operations.
    /// Includes Resources with ResourceType=Employee, Engineer, Inspector, Labor, Operator, Supervisor, Team,or Technician.
    /// </summary>
    public int ResourceLaborCount
    {
        get
        {
            int returnCount = 0;
            if (Scheduled)
            {
                List<InternalResource> resourcesUsed = GetResourcesScheduled();
                for (int resI = 0; resI < resourcesUsed.Count; resI++)
                {
                    InternalResource resource = resourcesUsed[resI];
                    if (BaseResourceDefs.IsLabor(resource.ResourceType))
                    {
                        returnCount++;
                    }
                }
            }

            return returnCount;
        }
    }

    /// <summary>
    /// The names of the Resource(s) the Job is scheduled on if the Job is scheduled.
    /// </summary>
    public string ResourceNames
    {
        get
        {
            List<string> resList = new();
            if (Scheduled)
            {
                List<InternalResource> resourcesUsed = GetResourcesScheduled();
                for (int resI = 0; resI < resourcesUsed.Count; resI++)
                {
                    resList.Add(resourcesUsed[resI].Name);
                }
            }

            if (resList.Count == 0)
            {
                return s_notScheduledLocalized;
            }

            return GetUniqueStrings(resList);
        }
    }

    /// <summary>
    /// The description of the Resource(s) the Job is scheduled on if the Job is scheduled.
    /// </summary>
    public string ResourceDescriptions
    {
        get
        {
            List<string> resDescriptionsList = new();
            if (Scheduled)
            {
                List<InternalResource> resourcesUsed = GetResourcesScheduled();
                for (int resI = 0; resI < resourcesUsed.Count; resI++)
                {
                    resDescriptionsList.Add(resourcesUsed[resI].Description);
                }
            }

            if (resDescriptionsList.Count == 0)
            {
                return s_notScheduledLocalized;
            }

            return GetUniqueStrings(resDescriptionsList);
        }
    }

    /// <summary>
    /// The note of the Resource(s) the Job is scheduled on if the Job is scheduled.
    /// </summary>
    public string ResourceNotes
    {
        get
        {
            List<string> resNotesList = new();
            if (Scheduled)
            {
                List<InternalResource> resourcesUsed = GetResourcesScheduled();
                for (int resI = 0; resI < resourcesUsed.Count; resI++)
                {
                    resNotesList.Add(resourcesUsed[resI].Notes);
                }
            }

            if (resNotesList.Count == 0)
            {
                return s_notScheduledLocalized;
            }

            return GetUniqueStrings(resNotesList);
        }
    }

    private string GetUniquePlantNames(List<Plant> plants)
    {
        System.Text.StringBuilder builder = new();
        Hashtable addedNames = new();
        for (int i = 0; i < plants.Count; i++)
        {
            Plant plant = plants[i];
            if (!addedNames.ContainsKey(plant.Name))
            {
                addedNames.Add(plant.Name, null);
                if (builder.Length > 0)
                {
                    builder.Append(string.Format(", {0}", plant.Name));
                }
                else
                {
                    builder.Append(plant.Name);
                }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns a comma separated list of the provided strings.
    /// </summary>
    private string GetUniqueStrings(List<string> strings)
    {
        System.Text.StringBuilder builder = new();
        Hashtable addedStringsHash = new();
        for (int i = 0; i < strings.Count; i++)
        {
            string nextString = strings[i];
            if (!addedStringsHash.ContainsKey(nextString))
            {
                addedStringsHash.Add(nextString, null);
                if (builder.Length > 0)
                {
                    builder.Append(string.Format(", {0}", nextString));
                }
                else
                {
                    builder.Append(nextString);
                }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// The Names of the Resources assigned to the earliest scheduled Operation.
    /// </summary>
    public string NextResources
    {
        get
        {
            System.Text.StringBuilder builder = new();
            Hashtable addedNames = new();
            List<InternalResource> resources = GetLeadResources();
            for (int i = 0; i < resources.Count; i++)
            {
                InternalResource resource = resources[i];
                if (!addedNames.ContainsKey(resource.Name))
                {
                    addedNames.Add(resource.Name, null);
                    if (builder.Length > 0)
                    {
                        builder.Append(string.Format(", {0}", resource.Name));
                    }
                    else
                    {
                        builder.Append(resource.Name);
                    }
                }
            }

            if (builder.Length == 0)
            {
                return "<No Resources scheduled>".Localize();
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// The Names of the Workcenters assigned to the earliest scheduled Operation.
    /// </summary>
    public string NextWorkcenters
    {
        get
        {
            System.Text.StringBuilder builder = new();
            Hashtable addedNames = new();
            List<InternalResource> resources = GetLeadResources();
            for (int i = 0; i < resources.Count; i++)
            {
                InternalResource resource = resources[i];
                if (resource.Workcenter != null && resource.Workcenter != "" && !addedNames.ContainsKey(resource.Workcenter))
                {
                    addedNames.Add(resource.Workcenter, null);
                    if (builder.Length > 0)
                    {
                        builder.Append(string.Format(", {0}", resource.Workcenter));
                    }
                    else
                    {
                        builder.Append(resource.Workcenter);
                    }
                }
            }

            if (builder.Length == 0)
            {
                return "<No Resources scheduled or Workcenter field not specified in Resources>".Localize();
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// get a list of Resources that are scheduled to work on LeadOpereation of this Job.
    /// </summary>
    /// <returns></returns>
    private List<InternalResource> GetLeadResources()
    {
        DateTime prevOpStartDate = DateTime.MaxValue;

        ManufacturingOrder leadMO = GetLeadManufacturingOrder();
        if (leadMO != null)
        {
            InternalOperation leadOp = leadMO.GetLeadOperation() as InternalOperation;
            if (leadOp != null)
            {
                return leadOp.GetResourcesScheduled();
            }
        }

        return new List<InternalResource>();
    }

    /// <summary>
    /// Returns a unique list of Resources used by scheduled operations.
    /// </summary>
    /// <returns></returns>
    public List<InternalResource> GetResourcesScheduled()
    {
        List<InternalResource> resourcesUsedForJob = new();
        Hashtable resourceIdsAdded = new();

        for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
        {
            List<InternalResource> resourcesUsedForMO = ManufacturingOrders[moI].GetResourcesScheduled();
            for (int rI = 0; rI < resourcesUsedForMO.Count; rI++)
            {
                InternalResource resource = resourcesUsedForMO[rI];
                if (!resourceIdsAdded.ContainsKey(resource.Id))
                {
                    resourcesUsedForJob.Add(resource);
                    resourceIdsAdded.Add(resource.Id, null);
                }
            }
        }

        return resourcesUsedForJob;
    }

    /// <summary>
    /// Whether the job contains any ManufacturingOrders that are used to constrain other other activities.
    /// </summary>
    /// <returns></returns>
    public bool HasMOUsedAsSubComponent()
    {
        for (int moI = 0; moI < ManufacturingOrders.Count; ++moI)
        {
            ManufacturingOrder mo = ManufacturingOrders[moI];

            if (mo.UsedAsSubComponent)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns a list of Order Number for all Jobs that this Job's Manufacturing Orders go into via Successor Manufacturing Orders.
    /// </summary>
    public string SuccessorOrderNumbers
    {
        get
        {
            System.Text.StringBuilder builder = new();
            Hashtable orderNumbersHash = new();
            for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];
                List<ManufacturingOrder.ManufacturingOrderLevel> moSucLevels = mo.GetSuccessorsRecursively(); //list of all successors, any levels deep
                for (int sucmoI = 0; sucmoI < moSucLevels.Count; sucmoI++)
                {
                    ManufacturingOrder.ManufacturingOrderLevel sucmo = moSucLevels[sucmoI];
                    if (sucmo.MO.Job.OrderNumber != null && sucmo.MO.Job.OrderNumber.Trim().Length > 0)
                    {
                        string nextOrderNumber = sucmo.MO.Job.OrderNumber.Trim();
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
            }

            return builder.ToString();
        }
    }

    public const string IN_INBOX = "Inbox"; //must match field name

    /// <summary>
    /// Returns true if any of the operations are scheduled on an Inbox Type Resource.
    /// </summary>
    public bool Inbox
    {
        get
        {
            for (int moI = 0; moI < ManufacturingOrders.Count; ++moI)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];

                if (mo.Inbox)
                {
                    return true;
                }
            }

            return false;
        }
    }
    #endregion

    #region Status
    public const string SCHEDULED_STATUS = "ScheduledStatus"; //must match field name.

    private JobDefs.scheduledStatuses m_scheduledStatus = JobDefs.scheduledStatuses.New;

    /// <summary>
    /// Indicates the current status of the Operation with respect to it being scheduled.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    [ParenthesizePropertyName(true)]
    public JobDefs.scheduledStatuses ScheduledStatus => m_scheduledStatus;

    internal JobDefs.scheduledStatuses ScheduledStatus_set
    {
        set => m_scheduledStatus = value;
    }

    private string m_failedToScheduleReason;

    /// <summary>
    /// If the status of the job is FailedToSchedule this will be set to a description as to why the job might not be schedulable.
    /// </summary>
    public string FailedToScheduleReason
    {
        get
        {
            if (ScheduledStatus == JobDefs.scheduledStatuses.FailedToSchedule)
            {
                return m_failedToScheduleReason;
            }

            return "";
        }

        internal set => m_failedToScheduleReason = value;
    }

    public const string EXCLUDED_REASONS = "ExcludedReasons"; //must match field name.
    private JobDefs.ExcludedReasons m_excludedReasons;

    /// <summary>
    /// The reasons why the job was excluded. The enumeration is flag enumeration, so multiple
    /// reasons are possible.
    /// </summary>
    public JobDefs.ExcludedReasons ExcludedReasons => m_excludedReasons;

    /// <summary>
    /// Call this function to set the excluded reason flag to NotExcluded.
    /// Other flags are cleared.
    /// </summary>
    internal void ExcluedReasons_SetNotExcluded()
    {
        m_excludedReasons = JobDefs.ExcludedReasons.NotExcluded;
    }

    /// <summary>
    /// Add another excluded reason. Don't try to use this function to add NotExcluded or NotSet.
    /// </summary>
    /// <param name="a_reason"></param>
    internal void ExcludeReasons_AddReason(JobDefs.ExcludedReasons a_reason)
    {
        if (m_excludedReasons == JobDefs.ExcludedReasons.NotExcluded)
        {
            m_excludedReasons = JobDefs.ExcludedReasons.NotSet;
        }
#if DEBUG
        if (a_reason == JobDefs.ExcludedReasons.NotExcluded || a_reason == JobDefs.ExcludedReasons.NotSet)
        {
            throw new Exception("NotExcluded and NotSet can't be added as ExcludedReasons.");
        }
#endif
        m_excludedReasons |= a_reason;
    }

    /// <summary>
    /// Get an array of the exluded reasons.
    /// If none of the bit flags are set, then the returned array will only contain 1 element whose value will equal NotSet.
    /// </summary>
    /// <returns></returns>
    public JobDefs.ExcludedReasons[] GetExcludeReasons()
    {
        long nbrOfBitsSet = BitHelper.CountBitsSet((int)m_excludedReasons);
        JobDefs.ExcludedReasons[] reasons;

        if (nbrOfBitsSet > 0)
        {
            reasons = new JobDefs.ExcludedReasons[nbrOfBitsSet];

            int reasonIdx = 0;

            for (int i = 0; i < JobDefs.ExcludedReasonsLength; ++i)
            {
                int flag = (int)Math.Pow(2, i);
                if ((flag & (int)m_excludedReasons) == flag)
                {
                    reasons[reasonIdx] = (JobDefs.ExcludedReasons)flag;
                }
            }
        }
        else
        {
            reasons = new JobDefs.ExcludedReasons[1];
            reasons[0] = JobDefs.ExcludedReasons.NotSet;
        }

        return reasons;
    }

    public const string PAST_PLANNING_HORIZON = "PastPlanningHorizon"; //MUST MATCH PROPERTY NAME BELOW

    public bool PastPlanningHorizon => Scheduled && ScheduledEndDate.Ticks > ScenarioDetail.GetPlanningHorizonEnd().Ticks;

    internal void SetFailedToScheduleToNotSatisfiable(ManufacturingOrder mo)
    {
        m_failedToScheduleReason = "Operations are lacking Active, Capable Resources.  See the Job 'Analysis' tab for more information.".Localize();

        //Uncomment once the GetUnsatisifedNodesDescription returns the right nodes.  Now all are returned.

        //string moMsg="";            

        //if (ManufacturingOrders.Count > 1)
        //    moMsg = String.Format("Manufacturing Order '{0}': ", mo.Name);

        //string outMsg="";
        //if (mo.CurrentPath != null) //just in case; not sure if this can be null
        //    outMsg = String.Format("{0}{1} {2}", moMsg, mo.CurrentPath.GetUnsatisifedNodesDescription(), "See the Job 'Analysis' tab for more info.");
        //else
        //    outMsg = "No Current Path";

        //_failedToScheduleReason = outMsg;
    }

    internal void SetFailedToScheduleToMissingCustomization()
    {
        m_failedToScheduleReason = "An operation's customization couldn't be found.".Localize();
    }

    internal void SetFailedToScheduleToScheduledActivityLimitReached()
    {
        m_failedToScheduleReason = "Licenced number of Scheduled Activities has been met.".Localize();
    }

    /// <summary>
    /// Updates the Job's scheduled status field to reflect an Activity having just finished.
    /// </summary>
    internal void ActivityFinished()
    {
        if (CreatingJob)
        {
            return; //Don't want to mark the Job as finished since other operations may yet be added.
        }

        if (Finished)
        {
            ScheduledStatus_set = JobDefs.scheduledStatuses.Finished;
            //Record history
            string description = string.Format("Finished Job {0} for {1}".Localize(), Name, Summary);
            ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(new[] { Id }, description, typeof(Job), ScenarioHistory.historyTypes.FinishedJob);
        }
    }

    /// <summary>
    /// Whether all Manufacturing Orders are Finished.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public override bool Finished
    {
        get
        {
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                if (!ManufacturingOrders[i].Finished)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Call this function when an MO is complete so the job can perform any special processing.
    /// </summary>
    internal void MOFinished()
    {
        if (Finished)
        {
            ScheduledStatus_set = JobDefs.scheduledStatuses.Finished;
            ScenarioDetail.JobsFinished();
        }
    }
    #endregion

    #region Transmission functionality
    #region Various flags related to transmission processing.
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    //	Background information on reasoning behind performing unschedules of jobs after the entire JobT has been processed and successor MO have been
    //  linked to their predecessor MOs in ScenarioDetail.cs.
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    //
    //	7.	Finished
    //		Test what happens when the job isn�t deleted but a specific predecessor MO is deleted.
    //
    //		Currently the result is the successor MO is unscheduled.
    //
    //	8.	Finished
    //		When an extra MO is added to the job you need to unscheduled the job since this will result in a state of a partially scheduled job.
    //
    //	9.	Finished
    //		Updated MOs; new successor MOs added or updated. Check as to whether this is already being handled.
    //
    //	10.	Finished
    //		When new jobs that have successor MOs are added: After the JobT has been processed and the successor links have been made, then mark 
    //		the successor MOs� jobs� unscheduled flag.
    //
    //		The above 4 points all need to be handled after the JobT has competed. If one MO were to require that the Job be unscheduled because 
    //		of some change and the JobT wasn�t completely processed, then some successor MO may end up being unscheduled that normally wouldn�t 
    //		have because the link between the predecessor and successor hadn�t been broken yet (imagine the JobT specified the link to be removed).
    //
    //-----------------------------------------------------------------------------------------------------------------------------------------------

    private BoolVector32 m_jobTFlags;

    #region Indexes
    private const int newMOsAddedFlagIndex = 0;
    private const int newSuccessorMOsAddedIndex = 1;
    private const int moReplacedIndex = 2;
    #endregion

    /// <summary>
    /// Before handling a transmission clear the job's transmission flags by calling this function.
    /// </summary>
    internal void InitJobTFlags()
    {
        m_jobTFlags.Clear();
    }

    /// <summary>
    /// This flag is set to true if new MOs are added.
    /// This also handles new jobs with successors since new Jobs always have new MOs.
    /// </summary>
    internal bool NewMOsAddedFlag
    {
        get => m_jobTFlags[newMOsAddedFlagIndex];

        set => m_jobTFlags[newMOsAddedFlagIndex] = value;
    }

    /// <summary>
    /// This flag is set to true if the job has MOs that point to new successor MOs or
    /// an existing successor MO is altered. For instance the successor could be changed
    /// to point to a different successor operation.
    /// </summary>
    internal bool NewOrUpdatedSuccessorMOsAdded
    {
        get => m_jobTFlags[newSuccessorMOsAddedIndex];

        set => m_jobTFlags[newSuccessorMOsAddedIndex] = value;
    }

    /// <summary>
    /// Set this flag when you replace an MO with a new MO.
    /// Instances when this may occur include when an alternate path of an MO is altered in a JobT and
    /// it is easier to replace the existing MO with the new temporary one.
    /// </summary>
    internal bool MOReplaced
    {
        get => m_jobTFlags[moReplacedIndex];

        set => m_jobTFlags[moReplacedIndex] = value;
    }
    #endregion

    #region SuccessorMO
    /// <summary>
    /// returns true if any MOs have any successor MOs.
    /// </summary>
    /// <returns></returns>
    internal bool HasSuccessorMOs()
    {
        return ManufacturingOrders.Any(mo => mo.SuccessorMOs != null && mo.SuccessorMOs.Count > 0);
    }

    // look at every MO's successor MO. Those that are marked need to be unscheduled.
    internal bool UnscheduleMarkedSuccessorMOs()
    {
        bool succMoUnscheduled = false;
        for (int i = 0; i < ManufacturingOrders.Count; ++i)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            succMoUnscheduled |= mo.UnscheduleMarkedSuccessorMOs();
        }

        return succMoUnscheduled;
    }

    /// <summary>
    /// This function needs to be called after a JobT has been processed and after the successor MOs have been linked
    /// to their predecessor MO within the successor MO objects.
    /// JobTJob marks may necessitate unschedules of MOs, successor MO, or posssibly other actions.
    /// </summary>
    internal bool HandleJobTJobMarks()
    {
        bool succMoUnscheduled = false;
        if (NewMOsAddedFlag || MOReplaced)
        {
            if (ScheduledOrPartiallyScheduled)
            {
                Unschedule();
            }
        }

        if (NewOrUpdatedSuccessorMOsAdded)
        {
            succMoUnscheduled |= UnscheduleMarkedSuccessorMOs();
        }

        InitJobTFlags();
        return succMoUnscheduled;
    }
    #endregion

    public void Receive(JobIdBaseT t, JobManager jobManager, ProductRuleManager a_productRuleManager, IScenarioDataChanges a_dataChanges)
    {
        if (t is ManufacturingOrderBaseT manufacturingOrderBaseT)
        {
            ManufacturingOrders.Receive(manufacturingOrderBaseT, a_productRuleManager, a_dataChanges);
        }
    }

    public void Receive(Scenario s, InternalActivityUpdateT.ActivityStatusUpdate update, IScenarioDataChanges a_dataChanges)
    {
        if (update.ManufacturingOrderExternalIdSet)
        {
            ManufacturingOrder mo = ManufacturingOrders.GetByExternalId(update.ManufacturingOrderExternalId);
            if (mo == null)
            {
                throw new PTValidationException("2201", new object[] { update.ManufacturingOrderExternalId }); //JMC TODO				
            }

            mo.Receive(update, a_dataChanges);
        }
        else if (update.OperationExternalIdSet)
        {
            BaseOperation opToUpdate = null;
            //Get all of the Operations that match this id from all MOs 
            //  choose the one with the earliest Scheduled Start Date that is not Finished
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                BaseOperation op = mo.OperationManager[update.OperationExternalId];
                if (op != null && op is InternalOperation && !op.Finished)
                {
                    if (opToUpdate == null) //then use this one
                    {
                        opToUpdate = op;
                    }
                    else if (op.StartDateTime < opToUpdate.StartDateTime) //then use this one
                    {
                        opToUpdate = op;
                    }
                }
            }

            if (opToUpdate is InternalOperation)
            {
                ((InternalOperation)opToUpdate).Receive(update, a_dataChanges);
            }
            else
            {
                throw new PTValidationException("2202", new object[] { ExternalId, update.OperationExternalId });
            }
        }
        else
        {
            throw new PTValidationException("2203"); //JMC TODO
        }
    }

    // !ALTERNATE_PATH!; This is where ScenarioDetailAlternatePathLockT handled at the job level. The job may end up being unscheduled if the locked path isn't equal to the CurrentPath.
    public void Receive(ScenarioDetailAlternatePathLockT a_alternatePathLockT)
    {
        ManufacturingOrder mo = ManufacturingOrders.GetById(a_alternatePathLockT.MOId);

        if (mo == null)
        {
            throw new PTValidationException("2204");
        }

        mo.Receive(a_alternatePathLockT);
    }
    
    public bool Update(UserFieldDefinitionManager a_udfManager, bool a_erpUpdate, JobT.Job a_jobTJob, ScenarioDetail a_sd, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        AuditEntry audit = new AuditEntry(Id, this);
        bool updated = base.Update(a_jobTJob, a_t, a_udfManager, UserField.EUDFObjectType.Jobs);
        RecordChangeHistory(a_jobTJob);
        updated |= SetSharedProperties(a_jobTJob, a_erpUpdate, a_dataChanges);
        updated |= JobTMOProcessing(a_udfManager, ScenarioDetail.Clock, a_erpUpdate, a_jobTJob, a_sd, a_t, a_dataChanges);

        if (updated)
        {
            a_dataChanges.AuditEntry(audit);
        }

        return updated;
    }

    private void RecordChangeHistory(JobT.Job a_jobTJob)
    {
        //Record Histories for changes in key values
        BaseId plantId = BaseId.NULL_ID; //don't have a plant for a job
        if (a_jobTJob.CancelledSet && !Cancelled && a_jobTJob.Cancelled)
        {
            ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("Cancelled: {0} ".Localize(), JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobCancelled);
        }

        if (a_jobTJob.HotSet && Hot != a_jobTJob.Hot)
        {
            if (a_jobTJob.Hot)
            {
                ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("Hot: {0} ".Localize(), JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobHot);
            }
            else
            {
                ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("Not Hot: {0} ".Localize(), JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobNotHot);
            }
        }

        if (a_jobTJob.NeedDateTimeSet)
        {
            if (a_jobTJob.NeedDateTime < NeedDateTime)
            {
                ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("NeedDate Earlier by {0} for: {1} ".Localize(), NeedDateTime.Subtract(a_jobTJob.NeedDateTime).ToReadableStringHourPrecision(), JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobNeedDateEarlier);
            }
            else if (a_jobTJob.NeedDateTime > NeedDateTime)
            {
                ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("NeedDate Later by {0} for: {1} ".Localize(), a_jobTJob.NeedDateTime.Subtract(NeedDateTime).ToReadableStringHourPrecision(), JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobNeedDateLater);
            }
        }

        for (int moI = 0; moI < a_jobTJob.ManufacturingOrderCount; moI++)
        {
            JobT.ManufacturingOrder moT = a_jobTJob[moI];
            ManufacturingOrder mo = ManufacturingOrders.GetByExternalId(moT.ExternalId);
            ;
            if (mo != null)
            {
                if (moT.RequiredQty > mo.RequiredQty)
                {
                    ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("Required Qty Increased by {0} for: MO {1} {2} ".Localize(), Math.Round(moT.RequiredQty - mo.RequiredQty, 3), mo.Name, JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobQtyIncreased);
                }
                else if (moT.RequiredQty < mo.RequiredQty)
                {
                    ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("Required Qty Decreased by {0} for: MO {1} {2} ".Localize(), Math.Round(mo.RequiredQty - moT.RequiredQty, 3), mo.Name, JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobQtyDecreased);
                }
            }
        }

        if (a_jobTJob.PrioritySet)
        {
            if (a_jobTJob.Priority < Priority) //Priority INCREASED, though the number descreased
            {
                ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("Priority Increased from {0} to {1} for: {2} ".Localize(), Priority, a_jobTJob.Priority, JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobPriorityIncreased);
            }
            else if (a_jobTJob.Priority > Priority)
            {
                ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("Priority Decreased from {0} to {1} for: {2} ".Localize(), Priority, a_jobTJob.Priority, JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobPriorityIncreased);
            }
        }

        if (a_jobTJob.CommitmentSet && a_jobTJob.Commitment < Commitment)
        {
            ScenarioDetail.ScenarioHistoryManager.RecordScenarioHistory(plantId, new[] { Id }, string.Format("Commitment changed from {0} to {1} for: {2} ".Localize(), Commitment, a_jobTJob.Commitment, JobHistoryString), GetType(), ScenarioHistory.historyTypes.JobCommitmentChanged);
        }
    }

    private bool SetSharedProperties(JobT.Job a_jobTJob, bool a_erpUpdate, IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        if (a_jobTJob.CanSpanPlantsSet && CanSpanPlants != a_jobTJob.CanSpanPlants)
        {
            CanSpanPlants = a_jobTJob.CanSpanPlants;
            updated = true;
        }

        if (a_jobTJob.ClassificationSet && Classification != a_jobTJob.Classification)
        {
            Classification = a_jobTJob.Classification;
            updated = true;
        }

        if (a_jobTJob.ColorCodeSet && ColorCode != a_jobTJob.ColorCode)
        {
            ColorCode = a_jobTJob.ColorCode;
            updated = true;
        }

        if (a_jobTJob.CommitmentSet && a_jobTJob.Commitment != Commitment)
        {
            if (a_jobTJob.Commitment < Commitment) //trying to go from Firm back to Planned for example
            {
                //Want to preserve the Planner's ability to firm a job in Planet and not have it undone.
                if (!a_erpUpdate || !CommitmentPreserved)
                {
                    Commitment = a_jobTJob.Commitment;
                    updated = true;
                }
            }
            else
            {
                Commitment = a_jobTJob.Commitment;
                updated = true;
            }

            if (!a_erpUpdate) //planner set the value internally so preserve it
            {
                CommitmentPreserved = true;
            }
        }

        if (a_jobTJob.HotReasonSet && HotReason != a_jobTJob.HotReason)
        {
            HotReason = a_jobTJob.HotReason;
            updated = true;
        }

        if (a_jobTJob.HoldReasonSet && HoldReason != a_jobTJob.HoldReason)
        {
            HoldReason = a_jobTJob.HoldReason;
            updated = true;
        }

        if (a_jobTJob.HoldSet && Hold != a_jobTJob.Hold) //set this last as setting to false clears hold date/reason.
        {
            if (!Hold)
            {
                a_dataChanges.FlagConstraintChanges(Id);
            }
            Hold = a_jobTJob.Hold;
            updated = true;
        }

        if (a_jobTJob.HoldUntilSet && HoldUntil != a_jobTJob.HoldUntilDate)
        {
            HoldUntil = a_jobTJob.HoldUntilDate; //TODO: UI sets this on Job save. Should we fix??
            if (Hold)
            {
                updated = true;
                a_dataChanges.FlagConstraintChanges(Id);
            }
        }

        if (a_jobTJob.ImportanceSet && Importance != a_jobTJob.Importance)
        {
            Importance = a_jobTJob.Importance;
            updated = true;
        }

        if (a_jobTJob.DoNotDeleteSet && a_jobTJob.DoNotDelete != DoNotDelete)
        {
            if (!a_erpUpdate || !DoNotDeletePreserved)
            {
                DoNotDelete = a_jobTJob.DoNotDelete;
                updated = true;
            }

            if (!a_erpUpdate) //planner set the value internally so preserve it
            {
                DoNotDeletePreserved = true;
                updated = true;
            }
        }

        if (a_jobTJob.DoNotScheduleSet && a_jobTJob.DoNotSchedule != DoNotSchedule)
        {
            if (!a_erpUpdate || !DoNotSchedulePreserved)
            {
                DoNotSchedule = a_jobTJob.DoNotSchedule;
                updated = true;
                a_dataChanges.FlagEligibilityChanges(Id);
            }

            if (!a_erpUpdate) //planner set the value internally so preserve it
            {
                DoNotSchedulePreserved = true;
                updated = true;
                a_dataChanges.FlagEligibilityChanges(Id);
            }
        }

        if (a_jobTJob.OrderNumberSet && OrderNumber != a_jobTJob.OrderNumber)
        {
            OrderNumber = a_jobTJob.OrderNumber;
            updated = true;
        }

        if (a_jobTJob.CustomerEmailSet && CustomerEmail != a_jobTJob.CustomerEmail)
        {
            CustomerEmail = a_jobTJob.CustomerEmail;
            updated = true;
        }

        if (a_jobTJob.AgentEmailSet && AgentEmail != a_jobTJob.AgentEmail)
        {
            AgentEmail = a_jobTJob.AgentEmail;
            updated = true;
        }

        // *LRH*If the job has already been scheduled then unschedule it.
        // It may also need to be removed from the Job Manager object.
        if (a_jobTJob.CancelledSet && Cancelled != a_jobTJob.Cancelled)
        {
            Cancelled = a_jobTJob.Cancelled;
            updated = true;
        }

        if (a_jobTJob.HotSet && Hot != a_jobTJob.Hot)
        {
            Hot = a_jobTJob.Hot;
            updated = true;
        }

        if (a_jobTJob.LatePenaltyCostSet && LatePenaltyCost != a_jobTJob.LatePenaltyCost)
        {
            LatePenaltyCost = a_jobTJob.LatePenaltyCost;
            updated = true;
        }

        if (a_jobTJob.ShippingCostSet && ShippingCost != a_jobTJob.ShippingCost)
        {
            ShippingCost = a_jobTJob.ShippingCost;
            updated = true;
        }

        if (a_jobTJob.MaxEarlyDeliverySpanSet && MaxEarlyDeliverySpan != a_jobTJob.MaxEarlyDeliverySpan)
        {
            MaxEarlyDeliverySpan = a_jobTJob.MaxEarlyDeliverySpan;
            updated = true;
        }

        if (a_jobTJob.AlmostLateSpanSet && AlmostLateSpan != a_jobTJob.AlmostLateSpan)
        {
            AlmostLateSpan = a_jobTJob.AlmostLateSpan;
            updated = true;
        }

        if (a_jobTJob.NeedDateTimeSet && NeedDateTime != a_jobTJob.NeedDateTime)
        {
            NeedDateTime = a_jobTJob.NeedDateTime;
            a_dataChanges.FlagJitChanges(Id);
            updated = true;
        }

        if (a_jobTJob.PrioritySet && Priority != a_jobTJob.Priority)
        {
            Priority = a_jobTJob.Priority;
            updated = true;
        }

        if (a_jobTJob.RevenueSet && Revenue != a_jobTJob.Revenue)
        {
            Revenue = a_jobTJob.Revenue;
            updated = true;
        }

        if (a_jobTJob.TypeSet && Type != a_jobTJob.Type)
        {
            Type = a_jobTJob.Type;
            updated = true;
        }

        if (a_jobTJob.TemplateSet && Template != a_jobTJob.Template)
        {
            Template = a_jobTJob.Template;
            updated = true;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (!a_erpUpdate || CreatingJob && Reviewed != a_jobTJob.Reviewed) //don't want imports to update this, just set initially. 
        {
            Reviewed = a_jobTJob.Reviewed;
            updated = true;
        }

        if (a_jobTJob.InvoicedSet && Invoiced != a_jobTJob.Invoiced)
        {
            Invoiced = a_jobTJob.Invoiced;
            updated = true;
        }

        if (a_jobTJob.PrintedSet && Printed != a_jobTJob.Printed)
        {
            Printed = a_jobTJob.Printed;
            updated = true;
        }

        if (a_jobTJob.ShippedSet && Shipped != a_jobTJob.Shipped)
        {
            Shipped = a_jobTJob.Shipped;
            updated = true;
        }

        if (a_jobTJob.DestinationSet && Destination != a_jobTJob.Destination)
        {
            Destination = a_jobTJob.Destination;
            updated = true;
        }

        return updated;
    }

    public bool Edit(JobEdit a_edit, IScenarioDataChanges a_dataChanges)
    {
        bool updated = base.Edit(a_edit);

        if (a_edit.CanSpanPlantsSet && CanSpanPlants != a_edit.CanSpanPlants)
        {
            CanSpanPlants = a_edit.CanSpanPlants;
            updated = true;
        }

        if (a_edit.ClassificationSet && Classification != a_edit.Classification)
        {
            Classification = a_edit.Classification;
            updated = true;
        }

        if (a_edit.ColorCodeSet && ColorCode != a_edit.ColorCode)
        {
            ColorCode = a_edit.ColorCode;
            updated = true;
        }

        if (a_edit.CommitmentSet && Commitment != a_edit.Commitment)
        {
            //TODO: Maybe we should check a permission to see if the user can overwrite another user's changes to this field (or other fields as well)
            Commitment = a_edit.Commitment;
            CommitmentPreserved = true;
            updated = true;
        }

        if (a_edit.HotReasonSet && HotReason != a_edit.HotReason)
        {
            HotReason = a_edit.HotReason;
            updated = true;
        }

        if (a_edit.HoldReasonSet && HoldReason != a_edit.HoldReason)
        {
            HoldReason = a_edit.HoldReason;
            updated = true;
        }

        if (a_edit.HoldUntilSet && HoldUntil != a_edit.HoldUntil)
        {
            HoldUntil = a_edit.HoldUntil;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_edit.HoldSet && Hold != a_edit.Hold)
        {
            Hold = a_edit.Hold;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_edit.ImportanceSet && Importance != a_edit.Importance)
        {
            Importance = a_edit.Importance;
            updated = true;
        }

        if (a_edit.DoNotDeleteSet && DoNotDelete != a_edit.DoNotDelete)
        {
            DoNotDelete = a_edit.DoNotDelete;
            DoNotDeletePreserved = true;
            updated = true;
        }

        if (a_edit.DoNotScheduleSet && DoNotSchedule != a_edit.DoNotSchedule)
        {
            a_dataChanges.FlagEligibilityChanges(Id);
            DoNotSchedule = a_edit.DoNotSchedule;
            DoNotSchedulePreserved = true;
            updated = true;
        }

        if (a_edit.OrderNumberSet && OrderNumber != a_edit.OrderNumber)
        {
            OrderNumber = a_edit.OrderNumber;
            updated = true;
        }

        if (a_edit.CustomerEmailSet && CustomerEmail != a_edit.CustomerEmail)
        {
            CustomerEmail = a_edit.CustomerEmail;
            updated = true;
        }

        if (a_edit.AgentEmailSet && AgentEmail != a_edit.AgentEmail)
        {
            AgentEmail = a_edit.AgentEmail;
            updated = true;
        }

        if (a_edit.CancelledSet && Cancelled != a_edit.Cancelled)
        {
            Cancelled = a_edit.Cancelled;
            updated = true;
        }

        if (a_edit.HotSet && Hot != a_edit.Hot)
        {
            Hot = a_edit.Hot;
            updated = true;
        }

        if (a_edit.LatePenaltyCostSet && LatePenaltyCost != a_edit.LatePenaltyCost)
        {
            LatePenaltyCost = a_edit.LatePenaltyCost;
            updated = true;
        }

        if (a_edit.ShippingCostSet && ShippingCost != a_edit.ShippingCost)
        {
            ShippingCost = a_edit.ShippingCost;
            updated = true;
        }

        if (a_edit.MaxEarlyDeliverySpanSet && MaxEarlyDeliverySpan != a_edit.MaxEarlyDeliverySpan)
        {
            MaxEarlyDeliverySpan = a_edit.MaxEarlyDeliverySpan;
            updated = true;
        }

        if (a_edit.AlmostLateSpanSet && AlmostLateSpan != a_edit.AlmostLateSpan)
        {
            AlmostLateSpan = a_edit.AlmostLateSpan;
            updated = true;
        }

        if (a_edit.NeedDateTimeSet && NeedDateTime != a_edit.NeedDateTime)
        {
            NeedDateTime = a_edit.NeedDateTime;
            a_dataChanges.FlagJitChanges(Id);
            updated = true;
        }

        if (a_edit.PrioritySet && Priority != a_edit.Priority)
        {
            Priority = a_edit.Priority;
            updated = true;
        }

        if (a_edit.RevenueSet && Revenue != a_edit.Revenue)
        {
            Revenue = a_edit.Revenue;
            updated = true;
        }

        if (a_edit.TypeSet && Type != a_edit.Type)
        {
            Type = a_edit.Type;
            updated = true;
        }

        if (a_edit.TemplateSet && Template != a_edit.Template)
        {
            Template = a_edit.Template;
            updated = true;
            a_dataChanges.FlagProductionChanges(Id);
        }

        if (a_edit.ReviewedSet && Reviewed != a_edit.Reviewed)
        {
            Reviewed = a_edit.Reviewed;
            updated = true;
        }

        if (a_edit.InvoicedSet && Invoiced != a_edit.Invoiced)
        {
            Invoiced = a_edit.Invoiced;
            updated = true;
        }

        if (a_edit.PrintedSet && Printed != a_edit.Printed)
        {
            Printed = a_edit.Printed;
            updated = true;
        }

        if (a_edit.ShippedSet && Shipped != a_edit.Shipped)
        {
            Shipped = a_edit.Shipped;
            updated = true;
        }

        if (a_edit.DestinationSet && Destination != a_edit.Destination)
        {
            Destination = a_edit.Destination;
            updated = true;
        }

        if (updated)
        {
            a_dataChanges.FlagVisualChanges(Id);
        }
        return updated;
    }

    internal void Commit(bool commit, DateTime clock, Dictionary<BaseId, BaseId> resourcesToInclude)
    {
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrders[i].Commit(commit, clock, resourcesToInclude);
        }
    }

    /// <summary>
    /// Report to the Job that the clock has advanced.
    /// </summary>
    internal void AdvanceClock(TimeSpan clockAdvancedBy, DateTime newClock, bool autoFinishAllActivities, bool autoReportProgressOnAllActivities)
    {
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrders[i].AdvanceClock(clockAdvancedBy, newClock, autoFinishAllActivities, autoReportProgressOnAllActivities);
        }
    }

    /// <summary>
    /// String to use when reporting history about a Job.
    /// </summary>
    internal string JobHistoryString => string.Format("Job {0}".Localize(), Name);

    /// <summary>
    /// Process a JobT's MOs.
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_erpUpdate">Whether the JobT is from an ERP system or generated within the PT UI.</param>
    /// <param name="a_jobTJob"></param>
    /// <param name="a_sd"></param>
    /// <param name="a_udfManager"></param>
    /// <param name="a_t"></param>
    /// <param name="a_dataChanges"></param>
    private bool JobTMOProcessing(UserFieldDefinitionManager a_udfManager, long a_simClock, bool a_erpUpdate, JobT.Job a_jobTJob, ScenarioDetail a_sd, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        bool updated = m_moManager.Receive(a_udfManager, a_sd.PlantManager, a_erpUpdate, a_jobTJob, a_sd, out bool newMOsAdded, a_t, a_dataChanges);

        if (newMOsAdded)
        {
            NewMOsAddedFlag = true;
        }

        if (a_dataChanges.HasEligibilityChanges(Id))
        {
            ComputeEligibilityAndUnscheduleIfIneligible(a_sd.ProductRuleManager);
            UpdateScheduledStatus(); //need to call this in case activities were finished
            CalculateJitTimes(a_simClock, false); //dates and structures may have changed.
        }
        return updated;
    }

    /// <summary>
    /// Checks to make sure the Warehouse is not in use.
    /// </summary>
    internal void ValidateWarehouseDelete(Warehouse warehouse)
    {
        ManufacturingOrders.ValidateWarehouseDelete(warehouse);
    }

    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_deleteProfile)
    {
        ManufacturingOrders.ValidateStorageAreaDelete(a_storageArea, a_deleteProfile);
    }

    /// <summary>
    /// Checks to make sure the Inventory is not in use.
    /// </summary>
    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        ManufacturingOrders.ValidateInventoryDelete(a_deleteProfile);
    }

    /// <summary>
    /// Checks to make sure the Item is not in use.
    /// </summary>
    internal void ValidateItemDelete(ItemDeleteProfile a_itemDeleteProfile)
    {
        ManufacturingOrders.ValidateItemDelete(a_itemDeleteProfile);
    }
    /// <summary>
    /// Checks to make sure the StorageArea is not in use.
    /// </summary>
    internal void ValidateItemStorageDelete(ItemStorageDeleteProfile a_itemStorageDeleteProfile)
    {
        ManufacturingOrders.ValidateItemStorageDelete(a_itemStorageDeleteProfile);
    }
    #endregion

    #region Cloning
    public Job Clone()
    {
        return (Job)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    #region Object Accessors
    //		CustomerManager customers;
    //		[System.ComponentModel.Browsable(false)]
    //		public CustomerManager Customers
    //		{
    //			get{return this.customers;}
    //			set{this.customers=value;}
    //		}

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private ManufacturingOrderManager m_moManager;

    [Browsable(false)]
    public ManufacturingOrderManager ManufacturingOrders
    {
        get => m_moManager;
        private set => m_moManager = value;
    }

    /// <summary>
    /// Get an MOKeyList of all the Job's MOs.
    /// </summary>
    /// <returns></returns>
    public MOKeyList GetMOKeyList()
    {
        MOKeyList mos = new();

        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            mos.Add(mo.GetKey(), null);
        }

        return mos;
    }

    /// <summary>
    /// Returns the zero-based index of this MO or -1 if not found.
    /// </summary>
    /// <param name="aMo"></param>
    /// <returns></returns>
    internal int GetManufacturingOrderIndex(ManufacturingOrder aMo)
    {
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            if (ManufacturingOrders[i].Id == aMo.Id)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns the Operation or null if there is no such Operation.
    /// </summary>
    public InternalOperation FindOperation(BaseId moId, BaseId opId)
    {
        ManufacturingOrder mo = ManufacturingOrders.GetById(moId);
        return mo?.FindOperation(opId);
    }

    public JobDataSet GetJobDataSet(JobManager jobs)
    {
        JobDataSet ds = new();
        PopulateJobDataSet(jobs, ds);
        return ds;
    }

    /// <summary>
    /// The percent of Material Requirements for all Manufacturing Orders that are Available.
    /// </summary>
    public int PercentOfMaterialsAvailable
    {
        get
        {
            int materialCount = 0;
            int materialAvailableCount = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                if (!mo.Scheduled)
                {
                    continue; // no supply or availability is available for unscheduled Operations. Do not include these in the calaculation
                }

                for (int nodeI = 0; nodeI < mo.CurrentPath.NodeCount; nodeI++)
                {
                    AlternatePath.Node node = mo.CurrentPath.GetNodeByIndex(nodeI);
                    BaseOperation bOp = node.Operation;
                    if (!bOp.Scheduled)
                    {
                        continue;
                    }

                    for (int matI = 0; matI < bOp.MaterialRequirements.Count; matI++)
                    {
                        MaterialRequirement mr = bOp.MaterialRequirements[matI];
                        materialCount++;
                        if (mr.Available)
                        {
                            materialAvailableCount++;
                        }
                    }
                }
            }

            if (materialCount == 0)
            {
                return 100;
            }

            return (int)(materialAvailableCount / (float)materialCount * 100);
        }
    }

    internal void PopulateJobDataSet(JobManager jobs, JobDataSet ds)
    {
        JobDataSetFiller filler = new();
        filler.FillDataSet(ds, this, jobs);
    }

    internal string GetHistory()
    {
        string historyString = "";
        for (int i = 0; i < ScenarioDetail.ScenarioHistoryManager.Count; i++)
        {
            ScenarioHistory history = ScenarioDetail.ScenarioHistoryManager.GetByIndex(i);
            if (history.ObjectType == GetType().Name && history.Key == Id.ToString())
            {
                historyString += string.Format("({0}) {1}", history.Timestamp.ToDisplayTime().ToShortDateString(), history.Description);
                historyString += Environment.NewLine;
            }
        }

        return historyString;
    }

    [Browsable(false)]
    public static string CreatorFieldName => "Creator"; //must match field name below

    private BaseId m_creator = BaseId.NULL_ID;

    /// <summary>
    /// The User who created the Job.
    /// </summary>
    public BaseId Creator
    {
        get => m_creator;
        private set => m_creator = value;
    }

    [Browsable(false)]
    public static string CtpFieldName => "CTP".Localize(); //Must match name of CTP field below

    /// <summary>
    /// Whether the Job was created for a Capable To Promise inquiry.
    /// </summary>
    public bool CTP
    {
        get => m_jobBools[c_ctpIdx];
        internal set => m_jobBools[c_ctpIdx] = value;
    }

    private Demand.SalesOrder m_ctpSalesOrder;

    /// <summary>
    /// If this Job is a CTP, this is the Sales Order that it is associated with.
    /// Deleting the Job also deletes the Sales Order.
    /// </summary>
    [Browsable(false)]
    public Demand.SalesOrder CtpSalesOrder
    {
        get => m_ctpSalesOrder;
        internal set => m_ctpSalesOrder = value;
    }

    /// <summary>
    /// If this Job is a CTP, this is the Sales Order that it is associated with.
    /// Deleting the Job also deletes the Sales Order.
    /// </summary>
    public string CtpSalesOrderName
    {
        get
        {
            if (CtpSalesOrder != null)
            {
                return CtpSalesOrder.Name;
            }

            return "";
        }
    }
    #endregion

    #region Constraint Violations
    /// <summary>
    /// Find constraint violations of the Job's activities and add them to the list parameter.
    /// </summary>
    /// <param name="violations">All constraint violations are added to this list.</param>
    internal void GetConstraintViolations(ConstraintViolationList violations)
    {
        for (int i = 0; i < ManufacturingOrders.Count; ++i)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            mo.GetConstraintViolations(violations);
        }
    }
    #endregion

    #region ERP transmission status update
    /// <summary>
    /// Call this function before handling a JobT or some other transmission that updates the status of jobs.
    /// It resets the activity variables that indicate the type of updates that have occurred.
    /// </summary>
    internal override void ResetERPStatusUpdateVariables()
    {
        base.ResetERPStatusUpdateVariables();

        for (int i = 0; i < ManufacturingOrders.Count; ++i)
        {
            ManufacturingOrders[i].ResetERPStatusUpdateVariables();
        }
    }
    #endregion

    #region BreakOffs
    internal void BreakOff(BaseId sourceMoId, decimal breakOffQty, DateTime breakOffNeedDate, DateTime breakOffReleaseDate)
    {
        throw new Exception("No Breakoffs. Use new copy functionality if this is needed.".Localize());
        //this.ComputeEligibility();
    }
    #endregion

    #region Demo Data
    /// <summary>
    /// Adjust values to update Demo Data for clock advance so good relative dates are maintained.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long a_simClock, long clockAdvanceTicks)
    {
        //Update NeedDate, Hold Date, and MOs.
        NeedDateTicks += new TimeSpan(clockAdvanceTicks).Ticks;
        if (Hold)
        {
            HoldUntil += new TimeSpan(clockAdvanceTicks);
        }

        ManufacturingOrders.AdjustDemoDataForClockAdvance(clockAdvanceTicks);
        CalculateJitTimes(a_simClock, false);
    }
    #endregion

    #region Delete validation
    internal void ResourceDeleteNotification(BaseResource r)
    {
        for (int i = 0; i < ManufacturingOrders.Count; ++i)
        {
            ManufacturingOrders[i].ResourceDeleteNotification(r);
        }
    }
    #endregion

    #region Cost
    /// <summary>
    /// Material Cost plus Labor Cost plus Machine Cost.
    /// </summary>
    public decimal TotalCost => MaterialCost + LaborCost + MachineCost + SubcontractCost + ExpectedLatePenaltyCost + ShippingCost;

    /// <summary>
    /// The sum of the Material Cost for all Manufacturing Orders
    /// </summary>
    public decimal MaterialCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                cost += ManufacturingOrders[i].MaterialCost;
            }

            return cost;
        }
    }

    /// <summary>
    /// The sum of the Raw Material Cost for all Manufacturing Orders
    /// </summary>
    public decimal RawMaterialCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                //TODO: Move to MO when it's available.
                ManufacturingOrder mo = ManufacturingOrders[i];
                IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = mo.CurrentPath.AlternateNodeSortedList.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    AlternatePath.Node apNode = enumerator.Current.Value;
                    BaseOperation operation = apNode.Operation;
                    cost += operation.RawMaterialCost;
                }
            }

            return cost;
        }
    }

    /// <summary>
    /// Get a list of all Material Shortages for all Operations.
    /// </summary>
    public List<MaterialRequirement.MaterialShortage> GetMaterialShortages(ScenarioDetail sd)
    {
        List<MaterialRequirement.MaterialShortage> jobMaterialShortages = new();
        for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
        {
            ManufacturingOrder mo = ManufacturingOrders[moI];
            List<MaterialRequirement.MaterialShortage> moMaterialShortages = mo.GetMaterialShortages(sd);
            for (int msI = 0; msI < moMaterialShortages.Count; msI++)
            {
                jobMaterialShortages.Add(moMaterialShortages[msI]);
            }
        }

        return jobMaterialShortages;
    }

    /// <summary>
    /// The sum of the Labor Cost for all Manufacturing Orders
    /// </summary>
    public decimal LaborCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                cost += ManufacturingOrders[i].LaborCost;
            }

            return cost;
        }
    }

    /// <summary>
    /// The sum of the Machine Cost for all Manufacturing Orders
    /// </summary>
    public decimal MachineCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                cost += ManufacturingOrders[i].MachineCost;
            }

            return cost;
        }
    }

    /// <summary>
    /// The sum of the Subcontract Cost for all Manufacturing Orders
    /// </summary>
    public decimal SubcontractCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                cost += ManufacturingOrders[i].SubcontractCost;
            }

            return cost;
        }
    }

    public decimal TotalSetupCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                cost += ManufacturingOrders[i].TotalSetupCost;
            }

            return cost;
        }
    }
    public decimal CleansCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < ManufacturingOrders.Count; i++)
            {
                cost += ManufacturingOrders[i].CleansCost;
            }

            return cost;
        }
    }
    private decimal m_shippingCost;

    /// <summary>
    /// The cost to ship the full qty of the Job.
    /// </summary>
    public decimal ShippingCost
    {
        get => m_shippingCost;
        internal set => m_shippingCost = value;
    }

    /// <summary>
    /// The money generated by the Job.
    /// Revenue - MaterialCost - SubcontractCost - ShippingCost - ExpectedLatePenaltyCost.
    /// Note that this does NOT include Resource costs which are considered to be a fixed cost.
    /// </summary>
    public decimal Throughput => Revenue - SubcontractCost - MaterialCost - ShippingCost - ExpectedLatePenaltyCost;
    #endregion

    #region Templates for CTP
    /// <summary>
    /// Looks through each Manufacturing Order and returns the first Primary Product encountered
    /// </summary>
    /// <returns></returns>
    internal Product GetPrimaryProduct()
    {
        foreach (ManufacturingOrder mo in ManufacturingOrders)
        {
            Product p = mo.GetPrimaryProduct();
            if (p != null)
            {
                return p;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a list of Item Ids indicating which Item(s) that are Products.
    /// </summary>
    /// <returns></returns>
    //internal BaseIdHash GetItemsProduced()
    //{
    //    BaseIdHash jobProductItems = new BaseIdHash();
    //    for (int i = 0; i < this.ManufacturingOrders.Count; i++)
    //    {
    //        ManufacturingOrder mo = this.ManufacturingOrders[i];
    //        BaseIdHash moProductItems = mo.GetItemsProduced();
    //        //Copy unique items to the job hash
    //        IDictionaryEnumerator enumerator = moProductItems.GetEnumerator();
    //        while (enumerator.MoveNext())
    //        {
    //            BaseId itemId = (BaseId)enumerator.Key;
    //            if (!jobProductItems.Contains(itemId))
    //                jobProductItems.Add(itemId);
    //        }
    //    }
    //    return jobProductItems;
    //}

    /// <summary>
    /// returns the list of products associated with this Job.
    /// </summary>
    /// <param name="a_currentPathOnly">if true, only the current path's products are returned</param>
    /// <returns></returns>
    public List<Product> GetProducts(bool a_currentPathOnly)
    {
        List<Product> jobProducts = new();
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            List<Product> moProducts = mo.GetProducts(a_currentPathOnly);
            for (int prodI = 0; prodI < moProducts.Count; prodI++)
            {
                jobProducts.Add(moProducts[prodI]);
            }
        }

        return jobProducts;
    }

    public List<ManufacturingOrder> FindMOsProducingItem(BaseId itemId)
    {
        List<ManufacturingOrder> moList = new();
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            HashSet<BaseId> moProductItems = mo.GetItemsProduced();
            if (moProductItems.Contains(itemId))
            {
                moList.Add(mo);
            }
        }

        return moList;
    }

    /// <summary>
    /// Returns the Operation making one or more Products.
    /// If no Operation has a product then null is returned.
    /// If more than one Operation has a Product then the last one found is returned and the number found is set in the out parameter.
    /// </summary>
    /// <returns></returns>
    public BaseOperation GetOnlyOpProducingProducts(out int opsWithProducts)
    {
        opsWithProducts = 0;
        BaseOperation opWithProduct = null;
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            for (int opI = 0; opI < mo.OperationManager.Count; opI++)
            {
                BaseOperation op = mo.OperationManager.GetByIndex(opI);
                if (op.Products.Count > 0)
                {
                    opsWithProducts++;
                    opWithProduct = op;
                }
            }
        }

        return opWithProduct;
    }

    /// <summary>
    /// Returns a list of all Operations across all Manufacturing Orders.
    /// </summary>
    /// <returns></returns>
    public List<BaseOperation> GetOperations()
    {
        List<BaseOperation> opList = new();
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            for (int opI = 0; opI < mo.OperationManager.Count; opI++)
            {
                BaseOperation op = mo.OperationManager.GetByIndex(opI);
                opList.Add(op);
            }
        }

        return opList;
    }

    public List<InternalActivity> GetActivities()
    {
        List<InternalActivity> acts = new();

        foreach (InternalOperation op in GetOperations())
        {
            for (int i = 0; i < op.Activities.Count; i++)
            {
                acts.Add(op.Activities.GetByIndex(i));
            }
        }

        return acts;
    }

    /// <summary>
    /// Returns a list of all Operations across all Manufacturing Orders' Current Paths.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<InternalOperation> GetOperationsFromCurrentPaths()
    {
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            for (int nodeI = 0; nodeI < mo.CurrentPath.NodeCount; nodeI++)
            {
                InternalOperation op = mo.CurrentPath[nodeI].Operation;
                yield return op;
            }
        }
    }
    #endregion Templates for CTP

    /// <summary>
    /// Call before Deleting the the Job or its Manufacturing Orders to allow dereferencing.
    /// </summary>
    /// <param name="a_dataChanges"></param>
    internal void DeletingJobOrMOs(IScenarioDataChanges a_dataChanges)
    {
        for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
        {
            ManufacturingOrders[moI].Deleting(a_dataChanges);
        }
    }

    /// <summary>
    /// Returns the first MO based on the MO creation index.
    /// </summary>
    /// <returns></returns>
    public ManufacturingOrder GetFirstMO()
    {
        if (ManufacturingOrders.Count > 0)
        {
            return ManufacturingOrders[0];
        }

        return null;
    }

    /// <summary>
    /// Flag Items that are produced or consumed as needing to be included in Net Change MRP since the delete will affect their inventory plans.
    /// </summary>
    internal void FlagAffectedItemsForNetChangeMRP(WarehouseManager aWarehouseManager)
    {
        for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
        {
            ManufacturingOrder mo = ManufacturingOrders[moI];
            mo.FlagAffectedItemsForNetChangeMRP(aWarehouseManager);
        }
    }

    /// <summary>
    /// Whether any activity has any of its blocks scheduled within the stable.
    /// </summary>
    internal bool InStableSpan()
    {
        return ManufacturingOrders.AnyActivityInStableSpan();
    }

    internal void DeletingDemand(BaseIdObject a_demand, PTTransmissionBase a_t, BaseIdList a_distributionsToDelete = null)
    {
        foreach (ManufacturingOrder mo in ManufacturingOrders)
        {
            mo.DeletingDemand(a_demand, a_t, a_distributionsToDelete);
        }
    }

    /// <summary>
    /// Update this job operations' MR Lot codes due to MO join
    /// </summary>
    /// <param name="a_job"></param>
    /// <param name="a_replacementCodes"></param>
    /// <returns></returns>
    internal bool ReplaceJobMRLotCodes(Dictionary<string, string> a_replacementCodes)
    {
        bool jobUpdated = false;
        foreach (ResourceOperation op in GetOperations())
        {
            foreach (MaterialRequirement mr in op.MaterialRequirements)
            {
                if (mr.BuyDirect || !mr.MustUseEligLot)
                {
                    continue;
                }

                HashSet<string> newCodes = new();
                Dictionary<string, EligibleLot>.Enumerator enumerator = mr.GetEligibleLotsEnumerator();
                bool lotsUpdate = false;
                while (enumerator.MoveNext())
                {
                    if (!a_replacementCodes.ContainsKey(enumerator.Current.Key))
                    {
                        newCodes.Add(enumerator.Current.Key);
                    }
                    else
                    {
                        //Replace the code
                        jobUpdated = lotsUpdate = true;
                        newCodes.Add(a_replacementCodes[enumerator.Current.Key]);
                    }
                }

                if (lotsUpdate)
                {
                    mr.ClearEligibleLots();
                    mr.SetEligibleLots(newCodes);
                }
            }
        }

        return jobUpdated;
    }

    public void CollectActivityIds(HashSet<ActivityKey> a_activityKeys)
    {
        foreach (InternalOperation op in GetOperations())
        {
            for (int i = 0; i < op.Activities.Count; i++)
            {
                a_activityKeys.Add(op.Activities.GetByIndex(i).CreateActivityKey());
            }
        }
    }
}
