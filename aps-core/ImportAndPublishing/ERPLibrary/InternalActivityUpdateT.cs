using System.Collections;

using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// Summary description for InternalActivityUpdateT.
/// </summary>
public class InternalActivityUpdateT : ERPTransmission
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 477;

    public InternalActivityUpdateT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                ActivityStatusUpdate node = new (a_reader);
                Add(node);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public InternalActivityUpdateT()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    protected ArrayList m_nodes = new ();

    public int Add(ActivityStatusUpdate a_node)
    {
        return m_nodes.Add(a_node);
    }

    public int Count => m_nodes.Count;

    public ActivityStatusUpdate this[int a_i] => (ActivityStatusUpdate)m_nodes[a_i];

    public void Validate()
    {
        if (Count < 1)
        {
            throw new ValidationException("2049");
        }
    }

    public class ActivityStatusUpdate : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 476;

        public ActivityStatusUpdate(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 12532)
            {
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out m_jobExternalId);
                a_reader.Read(out m_moExternalId);
                a_reader.Read(out m_opExternalId);
                a_reader.Read(out m_activityExternalId);
                a_reader.Read(out m_scheduledPlantExternalId);
                a_reader.Read(out m_scheduledDeptExternalId);
                a_reader.Read(out m_scheduledResourceExternalId);
                a_reader.Read(out m_reportedBy);
                a_reader.Read(out m_timestamp);
                a_reader.Read(out m_notes);
                a_reader.Read(out m_paused);
                a_reader.Read(out int val);
                m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);

                a_reader.Read(out val);
                m_productionStatus = (InternalActivityDefs.productionStatuses)val;
                a_reader.Read(out m_reportedSetupSpan);
                a_reader.Read(out m_reportedRunSpan);
                a_reader.Read(out m_reportedPostProcessingSpan);
                a_reader.Read(out m_reportedCleanSpan);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedQtiesAreIncremental);
                a_reader.Read(out m_reportedSpansAreIncremental);
                a_reader.Read(out m_reportedStartDateTicks);
                a_reader.Read(out m_reportedFinishDateTicks);
                a_reader.Read(out m_reportedStorageSpan); // new in 12522
                a_reader.Read(out m_reportedEndOfPostProcessingTicks); // new in 12523
                a_reader.Read(out m_reportedEndOfStorageTicks); // new in 12523
                a_reader.Read(out m_reportedProcessingStartDateTicks); // new in 12532
                a_reader.Read(out m_reportedProcessingEndDateTicks); // new in 12532

                a_reader.Read(out int materialIssueCount);
                for (int i = 0; i < materialIssueCount; i++)
                {
                    m_materialIssues.Add(new MaterialIssue(a_reader));
                }

                a_reader.Read(out m_comments2);
                m_operationUserFields = new UserFieldList(a_reader);
                a_reader.Read(out m_batchAmount);
                a_reader.Read(out m_nowFinishUtcTime);
            }
            #region 12523
            else if (a_reader.VersionNumber >= 12523)
            {
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out m_jobExternalId);
                a_reader.Read(out m_moExternalId);
                a_reader.Read(out m_opExternalId);
                a_reader.Read(out m_activityExternalId);
                a_reader.Read(out m_scheduledPlantExternalId);
                a_reader.Read(out m_scheduledDeptExternalId);
                a_reader.Read(out m_scheduledResourceExternalId);
                a_reader.Read(out m_reportedBy);
                a_reader.Read(out m_timestamp);
                a_reader.Read(out m_notes);
                a_reader.Read(out m_paused);
                a_reader.Read(out int val);
                m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);

                a_reader.Read(out val);
                m_productionStatus = (InternalActivityDefs.productionStatuses)val;
                a_reader.Read(out m_reportedSetupSpan);
                a_reader.Read(out m_reportedRunSpan);
                a_reader.Read(out m_reportedPostProcessingSpan);
                a_reader.Read(out m_reportedCleanSpan);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedQtiesAreIncremental);
                a_reader.Read(out m_reportedSpansAreIncremental);
                a_reader.Read(out m_reportedStartDateTicks);
                a_reader.Read(out m_reportedFinishDateTicks);
                a_reader.Read(out m_reportedStorageSpan); // new in 12522
                a_reader.Read(out m_reportedEndOfPostProcessingTicks); // new in 12523
                a_reader.Read(out m_reportedEndOfStorageTicks); // new in 12523

                a_reader.Read(out int materialIssueCount);
                for (int i = 0; i < materialIssueCount; i++)
                {
                    m_materialIssues.Add(new MaterialIssue(a_reader));
                }

                a_reader.Read(out m_comments2);
                m_operationUserFields = new UserFieldList(a_reader);
                a_reader.Read(out m_batchAmount);
                a_reader.Read(out m_nowFinishUtcTime);
            }
            #endregion
            else if (a_reader.VersionNumber >= 12522)
            {
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out m_jobExternalId);
                a_reader.Read(out m_moExternalId);
                a_reader.Read(out m_opExternalId);
                a_reader.Read(out m_activityExternalId);
                a_reader.Read(out m_scheduledPlantExternalId);
                a_reader.Read(out m_scheduledDeptExternalId);
                a_reader.Read(out m_scheduledResourceExternalId);
                a_reader.Read(out m_reportedBy);
                a_reader.Read(out m_timestamp);
                a_reader.Read(out m_notes);
                a_reader.Read(out m_paused);
                a_reader.Read(out int val);
                m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);

                a_reader.Read(out val);
                m_productionStatus = (InternalActivityDefs.productionStatuses)val;
                a_reader.Read(out m_reportedSetupSpan);
                a_reader.Read(out m_reportedRunSpan);
                a_reader.Read(out m_reportedPostProcessingSpan);
                a_reader.Read(out m_reportedCleanSpan);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedQtiesAreIncremental); 
                a_reader.Read(out m_reportedSpansAreIncremental); 
                a_reader.Read(out m_reportedStartDateTicks);
                a_reader.Read(out m_reportedFinishDateTicks);
                a_reader.Read(out m_reportedStorageSpan); // new in 12522

                a_reader.Read(out int materialIssueCount);
                for (int i = 0; i < materialIssueCount; i++)
                {
                    m_materialIssues.Add(new MaterialIssue(a_reader));
                }

                a_reader.Read(out m_comments2);
                m_operationUserFields = new UserFieldList(a_reader);
                a_reader.Read(out m_batchAmount);
                a_reader.Read(out m_nowFinishUtcTime);
            }
            else if (a_reader.VersionNumber >= 12500)
            {
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out m_jobExternalId);
                a_reader.Read(out m_moExternalId);
                a_reader.Read(out m_opExternalId);
                a_reader.Read(out m_activityExternalId);
                a_reader.Read(out m_scheduledPlantExternalId);
                a_reader.Read(out m_scheduledDeptExternalId);
                a_reader.Read(out m_scheduledResourceExternalId);
                a_reader.Read(out m_reportedBy);
                a_reader.Read(out m_timestamp);
                a_reader.Read(out m_notes);
                a_reader.Read(out m_paused);
                a_reader.Read(out int val);
                m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);

                a_reader.Read(out val);
                m_productionStatus = (InternalActivityDefs.productionStatuses)val;
                a_reader.Read(out m_reportedSetupSpan);
                a_reader.Read(out m_reportedRunSpan);
                a_reader.Read(out m_reportedPostProcessingSpan);
                a_reader.Read(out m_reportedCleanSpan);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedQtiesAreIncremental); //new in 50
                a_reader.Read(out m_reportedSpansAreIncremental); //new in 50
                a_reader.Read(out m_reportedStartDateTicks);
                a_reader.Read(out m_reportedFinishDateTicks);

                a_reader.Read(out int materialIssueCount);
                for (int i = 0; i < materialIssueCount; i++)
                {
                    m_materialIssues.Add(new MaterialIssue(a_reader));
                }

                a_reader.Read(out m_comments2);
                m_operationUserFields = new UserFieldList(a_reader);
                a_reader.Read(out m_batchAmount);
                a_reader.Read(out m_nowFinishUtcTime);
            }
            else if (a_reader.VersionNumber >= 12439)
            {
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out m_jobExternalId);
                a_reader.Read(out m_moExternalId);
                a_reader.Read(out m_opExternalId);
                a_reader.Read(out m_activityExternalId);
                a_reader.Read(out m_scheduledPlantExternalId);
                a_reader.Read(out m_scheduledDeptExternalId);
                a_reader.Read(out m_scheduledResourceExternalId);
                a_reader.Read(out m_reportedBy);
                a_reader.Read(out m_timestamp);
                a_reader.Read(out m_notes);
                a_reader.Read(out m_paused);
                a_reader.Read(out int val);
                m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);

                a_reader.Read(out val);
                m_productionStatus = (InternalActivityDefs.productionStatuses)val;
                a_reader.Read(out m_reportedSetupSpan);
                a_reader.Read(out m_reportedRunSpan);
                a_reader.Read(out m_reportedPostProcessingSpan);
                a_reader.Read(out m_reportedCleanSpan);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedQtiesAreIncremental); //new in 50
                a_reader.Read(out m_reportedSpansAreIncremental); //new in 50
                a_reader.Read(out m_reportedStartDateTicks);
                a_reader.Read(out m_reportedProcessingStartDateTicks);
                a_reader.Read(out m_reportedProcessingEndDateTicks);
                a_reader.Read(out m_reportedFinishDateTicks);

                a_reader.Read(out int materialIssueCount);
                for (int i = 0; i < materialIssueCount; i++)
                {
                    m_materialIssues.Add(new MaterialIssue(a_reader));
                }

                a_reader.Read(out m_comments2);
                m_operationUserFields = new UserFieldList(a_reader);
                a_reader.Read(out m_batchAmount);
                a_reader.Read(out m_nowFinishUtcTime);
            }
            else if (a_reader.VersionNumber >= 12416)
            {
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out m_jobExternalId);
                a_reader.Read(out m_moExternalId);
                a_reader.Read(out m_opExternalId);
                a_reader.Read(out m_activityExternalId);
                a_reader.Read(out m_scheduledPlantExternalId);
                a_reader.Read(out m_scheduledDeptExternalId);
                a_reader.Read(out m_scheduledResourceExternalId);
                a_reader.Read(out m_reportedBy);
                a_reader.Read(out m_timestamp);
                a_reader.Read(out m_notes);
                a_reader.Read(out m_paused);
                a_reader.Read(out int val);
                m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);

                a_reader.Read(out val);
                m_productionStatus = (InternalActivityDefs.productionStatuses)val;
                a_reader.Read(out m_reportedSetupSpan);
                a_reader.Read(out m_reportedRunSpan);
                a_reader.Read(out m_reportedPostProcessingSpan);
                a_reader.Read(out m_reportedCleanSpan);
                a_reader.Read(out m_reportedCleanOutGrade);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedQtiesAreIncremental); //new in 50
                a_reader.Read(out m_reportedSpansAreIncremental); //new in 50
                a_reader.Read(out m_reportedStartDateTicks);
                a_reader.Read(out m_reportedFinishDateTicks);

                a_reader.Read(out int materialIssueCount);
                for (int i = 0; i < materialIssueCount; i++)
                {
                    m_materialIssues.Add(new MaterialIssue(a_reader));
                }

                a_reader.Read(out m_comments2);
                m_operationUserFields = new UserFieldList(a_reader);
                a_reader.Read(out m_batchAmount);
                a_reader.Read(out m_nowFinishUtcTime);
            }
            else if (a_reader.VersionNumber >= 745)
            {
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out m_jobExternalId);
                a_reader.Read(out m_moExternalId);
                a_reader.Read(out m_opExternalId);
                a_reader.Read(out m_activityExternalId);
                a_reader.Read(out m_scheduledPlantExternalId);
                a_reader.Read(out m_scheduledDeptExternalId);
                a_reader.Read(out m_scheduledResourceExternalId);
                a_reader.Read(out m_reportedBy);
                a_reader.Read(out m_timestamp);
                a_reader.Read(out m_notes);
                a_reader.Read(out m_paused);
                a_reader.Read(out int val);
                m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);

                a_reader.Read(out val);
                m_productionStatus = (InternalActivityDefs.productionStatuses)val;
                a_reader.Read(out m_reportedSetupSpan);
                a_reader.Read(out m_reportedRunSpan);
                a_reader.Read(out m_reportedPostProcessingSpan);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedQtiesAreIncremental); //new in 50
                a_reader.Read(out m_reportedSpansAreIncremental); //new in 50
                a_reader.Read(out m_reportedStartDateTicks);
                a_reader.Read(out m_reportedFinishDateTicks);

                a_reader.Read(out int materialIssueCount);
                for (int i = 0; i < materialIssueCount; i++)
                {
                    m_materialIssues.Add(new MaterialIssue(a_reader));
                }

                a_reader.Read(out m_comments2);
                m_operationUserFields = new UserFieldList(a_reader);
                a_reader.Read(out m_batchAmount);
                a_reader.Read(out m_nowFinishUtcTime);
            }
            else if (a_reader.VersionNumber >= 744)
            {
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out m_jobExternalId);
                a_reader.Read(out m_moExternalId);
                a_reader.Read(out m_opExternalId);
                a_reader.Read(out m_activityExternalId);
                a_reader.Read(out m_scheduledPlantExternalId);
                a_reader.Read(out m_scheduledDeptExternalId);
                a_reader.Read(out m_scheduledResourceExternalId);
                a_reader.Read(out m_reportedBy);
                a_reader.Read(out m_timestamp);
                a_reader.Read(out m_notes);
                a_reader.Read(out m_paused);
                a_reader.Read(out int val);
                m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);

                a_reader.Read(out val);
                m_productionStatus = (InternalActivityDefs.productionStatuses)val;
                a_reader.Read(out m_reportedSetupSpan);
                a_reader.Read(out m_reportedRunSpan);
                a_reader.Read(out m_reportedPostProcessingSpan);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedQtiesAreIncremental); //new in 50
                a_reader.Read(out m_reportedSpansAreIncremental); //new in 50
                a_reader.Read(out m_reportedStartDateTicks);
                a_reader.Read(out m_reportedFinishDateTicks);

                a_reader.Read(out int materialIssueCount);
                for (int i = 0; i < materialIssueCount; i++)
                {
                    m_materialIssues.Add(new MaterialIssue(a_reader));
                }

                a_reader.Read(out m_comments2);
                m_operationUserFields = new UserFieldList(a_reader);
                a_reader.Read(out m_batchAmount);
            }
            else if (a_reader.VersionNumber >= 740)
            {
                m_bools = new BoolVector32(a_reader);
                m_isSetBools = new BoolVector32(a_reader);
                a_reader.Read(out m_jobExternalId);
                a_reader.Read(out m_moExternalId);
                a_reader.Read(out m_opExternalId);
                a_reader.Read(out m_activityExternalId);
                a_reader.Read(out m_scheduledPlantExternalId);
                a_reader.Read(out m_scheduledDeptExternalId);
                a_reader.Read(out m_scheduledResourceExternalId);
                a_reader.Read(out m_reportedBy);
                a_reader.Read(out m_timestamp);
                a_reader.Read(out m_notes);
                a_reader.Read(out m_paused);
                a_reader.Read(out int val);
                m_peopleUsage = (InternalActivityDefs.peopleUsages)val;
                a_reader.Read(out m_nbrOfPeople);
                a_reader.Read(out m_comments);
                a_reader.Read(out m_onHold);
                a_reader.Read(out m_holdReason);
                a_reader.Read(out m_holdUntil);

                a_reader.Read(out val);
                m_productionStatus = (InternalActivityDefs.productionStatuses)val;
                a_reader.Read(out m_reportedSetupSpan);
                a_reader.Read(out m_reportedRunSpan);
                a_reader.Read(out m_reportedPostProcessingSpan);
                a_reader.Read(out m_reportedScrapQty);
                a_reader.Read(out m_reportedGoodQty);
                a_reader.Read(out m_reportedQtiesAreIncremental); //new in 50
                a_reader.Read(out m_reportedSpansAreIncremental); //new in 50
                a_reader.Read(out m_reportedStartDateTicks);
                a_reader.Read(out m_reportedFinishDateTicks);

                a_reader.Read(out int materialIssueCount);
                for (int i = 0; i < materialIssueCount; i++)
                {
                    m_materialIssues.Add(new MaterialIssue(a_reader));
                }

                a_reader.Read(out m_comments2);
                m_operationUserFields = new UserFieldList(a_reader);
            }
        }

        public void Serialize(IWriter a_writer)
        {
            m_bools.Serialize(a_writer);
            m_isSetBools.Serialize(a_writer);
            a_writer.Write(m_jobExternalId);
            a_writer.Write(m_moExternalId);
            a_writer.Write(m_opExternalId);
            a_writer.Write(m_activityExternalId);
            a_writer.Write(m_scheduledPlantExternalId);
            a_writer.Write(m_scheduledDeptExternalId);
            a_writer.Write(m_scheduledResourceExternalId);
            a_writer.Write(m_reportedBy);
            a_writer.Write(m_timestamp);
            a_writer.Write(m_notes);
            a_writer.Write(m_paused); //new in 53
            a_writer.Write((int)m_peopleUsage);
            a_writer.Write(m_nbrOfPeople);
            a_writer.Write(m_comments);
            a_writer.Write(m_onHold); //new in 59
            a_writer.Write(m_holdReason);
            a_writer.Write(m_holdUntil);

            a_writer.Write((int)m_productionStatus);
            a_writer.Write(m_reportedSetupSpan);
            a_writer.Write(m_reportedRunSpan);
            a_writer.Write(m_reportedPostProcessingSpan);
            a_writer.Write(m_reportedCleanSpan);
            a_writer.Write(m_reportedCleanOutGrade);
            a_writer.Write(m_reportedScrapQty);
            a_writer.Write(m_reportedGoodQty);
            a_writer.Write(m_reportedQtiesAreIncremental); //new in 50
            a_writer.Write(m_reportedSpansAreIncremental); //new in 50
            a_writer.Write(m_reportedStartDateTicks);
            a_writer.Write(m_reportedFinishDateTicks);
            a_writer.Write(m_reportedStorageSpan); // new in 12522

            a_writer.Write(m_reportedEndOfPostProcessingTicks); // new in 12523
            a_writer.Write(m_reportedEndOfStorageTicks); // new in 12523
            a_writer.Write(m_reportedProcessingStartDateTicks);
            a_writer.Write(m_reportedProcessingEndDateTicks);

            a_writer.Write(m_materialIssues.Count);
            for (int i = 0; i < m_materialIssues.Count; i++)
            {
                m_materialIssues[i].Serialize(a_writer);
            }

            a_writer.Write(m_comments2);
            m_operationUserFields.Serialize(a_writer);
            a_writer.Write(m_batchAmount);
            a_writer.Write(m_nowFinishUtcTime);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        #region Constructors
        /// <summary>
        /// Updates the status of an Activity, fully specifying its identification and scheduled Resource.  If the Activity is not already scheduled on the Scheduled Resource then it will be moved there if the
        /// Resource is eligible.
        /// </summary>
        /// <param name="a_productionStatus">
        /// The current status of the Activity.  Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring,
        /// Finished.
        /// </param>
        /// <param name="a_jobExternalId">The ExternalId of the Job.</param>
        /// <param name="a_manufacturingOrderExternalId">The ExternalId of the Manufacturing Order.</param>
        /// <param name="a_operationExternalId">The ExternalId of the Operation.</param>
        /// <param name="a_activityExternalId">The ExternalId of the Activity.</param>
        /// <param name="a_scheduledPlantExternalId">
        /// The PlantExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        /// <param name="a_scheduledDeptExternalId">
        /// The DepartmentExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        /// <param name="a_scheduledResourceExternalId">
        /// The ExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        public ActivityStatusUpdate(InternalActivityDefs.productionStatuses a_productionStatus,
                                    string a_jobExternalId,
                                    string a_manufacturingOrderExternalId,
                                    string a_operationExternalId,
                                    string a_activityExternalId,
                                    string a_scheduledPlantExternalId,
                                    string a_scheduledDeptExternalId,
                                    string a_scheduledResourceExternalId)
        {
            m_productionStatus = a_productionStatus;
            m_jobExternalId = a_jobExternalId;
            ManufacturingOrderExternalId = a_manufacturingOrderExternalId;
            OperationExternalId = a_operationExternalId;
            ActivityExternalId = a_activityExternalId;
            ScheduledPlantExternalId = a_scheduledPlantExternalId;
            ScheduledDeptExternalId = a_scheduledDeptExternalId;
            ScheduledResourceExternalId = a_scheduledResourceExternalId;
        }

        /// <summary>
        /// Updates the status of an Activity, fully specifying its identification
        /// </summary>
        /// <param name="a_productionStatus">
        /// The current status of the Activity.  Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring,
        /// Finished.
        /// </param>
        /// <param name="a_jobExternalId">The ExternalId of the Job.</param>
        /// <param name="a_manufacturingOrderExternalId">The ExternalId of the Manufacturing Order.</param>
        /// <param name="a_operationExternalId">The ExternalId of the Operation.</param>
        /// <param name="a_activityExternalId">The ExternalId of the Activity.</param>
        public ActivityStatusUpdate(InternalActivityDefs.productionStatuses a_productionStatus,
                                    string a_jobExternalId,
                                    string a_manufacturingOrderExternalId,
                                    string a_operationExternalId,
                                    string a_activityExternalId)
        {
            m_productionStatus = a_productionStatus;
            m_jobExternalId = a_jobExternalId;
            ManufacturingOrderExternalId = a_manufacturingOrderExternalId;
            OperationExternalId = a_operationExternalId;
            ActivityExternalId = a_activityExternalId;
        }

        /// <summary>
        /// Updates the status of an Activity using its scheduled Resource to help identify it.  If more than one Activity qualifies, a validation error will occur and the transmission will have no effect.
        /// </summary>
        /// <param name="a_productionStatus">
        /// The current status of the Activity.  Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring,
        /// Finished.
        /// </param>
        /// <param name="a_jobExternalId">The ExternalId of the Job.</param>
        /// <param name="a_manufacturingOrderExternalId">The ExternalId of the Manufacturing Order.</param>
        /// <param name="a_operationExternalId">The ExternalId of the Operation.</param>
        /// <param name="a_scheduledPlantExternalId">
        /// The PlantExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        /// <param name="a_scheduledDeptExternalId">
        /// The DepartmentExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        /// <param name="a_scheduledResourceExternalId">
        /// The ExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        public ActivityStatusUpdate(InternalActivityDefs.productionStatuses a_productionStatus,
                                    string a_jobExternalId,
                                    string a_manufacturingOrderExternalId,
                                    string a_operationExternalId,
                                    string a_scheduledPlantExternalId,
                                    string a_scheduledDeptExternalId,
                                    string a_scheduledResourceExternalId)
        {
            m_productionStatus = a_productionStatus;
            m_jobExternalId = a_jobExternalId;
            ManufacturingOrderExternalId = a_manufacturingOrderExternalId;
            OperationExternalId = a_operationExternalId;
            ScheduledPlantExternalId = a_scheduledPlantExternalId;
            ScheduledDeptExternalId = a_scheduledDeptExternalId;
            ScheduledResourceExternalId = a_scheduledResourceExternalId;
        }

        /// <summary>
        /// Updates the status of an Activity using its scheduled Resource to help identify it.  If more than one Activity qualifies, a validation error will occur and the transmission will have no effect.
        /// </summary>
        /// <param name="a_productionStatus">
        /// The current status of the Activity.  Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring,
        /// Finished.
        /// </param>
        /// <param name="a_jobExternalId">The ExternalId of the Job.</param>
        /// <param name="a_manufacturingOrderExternalId">The ExternalId of the Manufacturing Order.</param>
        /// <param name="a_scheduledPlantExternalId">
        /// The PlantExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        /// <param name="a_scheduledDeptExternalId">
        /// The DepartmentExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        /// <param name="a_scheduledResourceExternalId">
        /// The ExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        public ActivityStatusUpdate(InternalActivityDefs.productionStatuses a_productionStatus,
                                    string a_jobExternalId,
                                    string a_manufacturingOrderExternalId,
                                    string a_scheduledPlantExternalId,
                                    string a_scheduledDeptExternalId,
                                    string a_scheduledResourceExternalId)
        {
            m_productionStatus = a_productionStatus;
            m_jobExternalId = a_jobExternalId;
            ManufacturingOrderExternalId = a_manufacturingOrderExternalId;
            ScheduledPlantExternalId = a_scheduledPlantExternalId;
            ScheduledDeptExternalId = a_scheduledDeptExternalId;
            ScheduledResourceExternalId = a_scheduledResourceExternalId;
        }

        /// <summary>
        /// Updates the status of an Activity using its scheduled Resource to help identify it.  If more than one Activity qualifies, a validation error will occur and the transmission will have no effect.
        /// </summary>
        /// <param name="a_productionStatus">
        /// The current status of the Activity.  Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring,
        /// Finished.
        /// </param>
        /// <param name="a_jobExternalId">The ExternalId of the Job.</param>
        /// <param name="a_scheduledPlantExternalId">
        /// The PlantExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        /// <param name="a_scheduledDeptExternalId">
        /// The DepartmentExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        /// <param name="a_scheduledResourceExternalId">
        /// The ExternalId of a Resource on which the Activity is currently scheduled. This can be used to identify an Activity if the Operation and Activity
        /// ExternalIds are not specified. If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </param>
        public ActivityStatusUpdate(string a_jobExternalId,
                                    InternalActivityDefs.productionStatuses a_productionStatus,
                                    string a_scheduledPlantExternalId,
                                    string a_scheduledDeptExternalId,
                                    string a_scheduledResourceExternalId)
        {
            m_productionStatus = a_productionStatus;
            m_jobExternalId = a_jobExternalId;
            ScheduledPlantExternalId = a_scheduledPlantExternalId;
            ScheduledDeptExternalId = a_scheduledDeptExternalId;
            ScheduledResourceExternalId = a_scheduledResourceExternalId;
        }

        /// <summary>
        /// Updates the status of an Activity assuming there is only Activity for the specified Operation.  If it has more than one Activity, a validation error will occur and the transmission will have no
        /// effect.
        /// </summary>
        /// <param name="a_productionStatus">
        /// The current status of the Activity.  Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring,
        /// Finished.
        /// </param>
        /// <param name="a_jobExternalId">The ExternalId of the Job.</param>
        /// <param name="a_manufacturingOrderExternalId">The ExternalId of the Manufacturing Order.</param>
        /// <param name="a_operationExternalId">The ExternalId of the Operation.</param>
        public ActivityStatusUpdate(InternalActivityDefs.productionStatuses a_productionStatus,
                                    string a_jobExternalId,
                                    string a_manufacturingOrderExternalId,
                                    string a_operationExternalId)
        {
            m_productionStatus = a_productionStatus;
            m_jobExternalId = a_jobExternalId;
            ManufacturingOrderExternalId = a_manufacturingOrderExternalId;
            OperationExternalId = a_operationExternalId;
        }

        /// <summary>
        /// Updates the status of an Activity assuming there is only Activity for the specified Manufacturing Order.  If it has more than one Activity, a validation error will occur and the transmission will
        /// have no effect.
        /// </summary>
        /// <param name="a_productionStatus">
        /// The current status of the Activity.  Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring,
        /// Finished.
        /// </param>
        /// <param name="a_jobExternalId">The ExternalId of the Job.</param>
        /// <param name="a_manufacturingOrderExternalId">The ExternalId of the Manufacturing Order.</param>
        public ActivityStatusUpdate(InternalActivityDefs.productionStatuses a_productionStatus,
                                    string a_jobExternalId,
                                    string a_manufacturingOrderExternalId)
        {
            m_productionStatus = a_productionStatus;
            m_jobExternalId = a_jobExternalId;
            ManufacturingOrderExternalId = a_manufacturingOrderExternalId;
        }

        /// <summary>
        /// Updates the status of the Operation with the earliest Scheduled Start Date.
        /// </summary>
        /// <param name="a_jobExternalId">The ExternalId of the Job.</param>
        /// <param name="a_operationExternalId"></param>
        /// <param name="a_productionStatus">
        /// The current status of the Activity.  Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring,
        /// Finished.
        /// </param>
        public ActivityStatusUpdate(string a_jobExternalId,
                                    string a_operationExternalId,
                                    InternalActivityDefs.productionStatuses a_productionStatus
        )
        {
            m_productionStatus = a_productionStatus;
            m_jobExternalId = a_jobExternalId;
            OperationExternalId = a_operationExternalId;
        }

        /// <summary>
        /// Updates the status of an Activity assuming there is only Activity for the specified Job.  If it has more than one Activity, a validation error will occur and the transmission will have no effect.
        /// </summary>
        /// <param name="a_productionStatus">
        /// The current status of the Activity.  Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring,
        /// Finished.
        /// </param>
        /// <param name="a_jobExternalId">The ExternalId of the Job.</param>
        public ActivityStatusUpdate(InternalActivityDefs.productionStatuses a_productionStatus,
                                    string a_jobExternalId)
        {
            m_productionStatus = a_productionStatus;
            m_jobExternalId = a_jobExternalId;
        }
        #endregion Constructors

        private BoolVector32 m_isSetBools;

        private const short c_userFieldsSetIdx = 0;
        private const short c_manufacturingOrderExternalIdSetIdx = 1;
        private const short c_opExternalIdSetIdx = 2;
        private const short c_activityExternalIdSetIdx = 3;
        private const short c_schedulePlantExternalIdSetIdx = 4;
        private const short c_scheduledDeptExternalIdSetIdx = 5;
        private const short c_scheduledResourceExternalIdSetIdx = 6;
        private const short c_pausedSetIdx = 7;
        private const short c_onHoldSetIdx = 8;
        private const short c_onHoldUntilSetIdx = 9;
        private const short c_onHoldReasonSetIdx = 10;
        private const short c_peopleUsageSetIdx = 11;
        private const short c_nbrPeopleSetIdx = 12;
        private const short c_commentsSetIdx = 13;
        private const short c_comments2SetIdx = 14;
        private const short c_reportedSetupSpanSetIdx = 15;
        private const short c_reportedRunSpanSetIdx = 17;
        private const short c_reportedPostProcessingSpanSetIdx = 18;
        private const short c_reportedScrapQtySetIdx = 19;
        private const short c_reportedGoodQtySetIdx = 20;
        private const short c_reportedBySetIdx = 21;
        private const short c_batchAmountSetIdx = 22;
        private const short c_reportedCleanSpanIsSetIdx = 23;
        private const short c_reportedCleanoutGradeIsSetIdx = 24;
        private const short c_reportedStorageSpanSetIdx = 25;
        private const short c_reportedEndOfPostProcessingSetIdx = 26;
        private const short c_reportedEndOfStorageSetIdx = 27;

        private BoolVector32 m_bools;
        private const short c_allocateMaterialsFromOnHand = 0;
        private const short c_releaseProductToWarehouse = 1;
        private const short c_activityManualUpdateOnlyIdx = 2;

        private string m_jobExternalId;

        /// <summary>
        /// The ExternalId of the Job.
        /// </summary>
        public string JobExternalId
        {
            get => m_jobExternalId;
            set => m_jobExternalId = value;
        }

        private string m_moExternalId;

        /// <summary>
        /// The ExternalId of the Manufacturing Order.
        /// </summary>
        public string ManufacturingOrderExternalId
        {
            get => m_moExternalId;
            set
            {
                m_moExternalId = value;
                ManufacturingOrderExternalIdSet = true;
            }
        }

        public bool ManufacturingOrderExternalIdSet
        {
            get => m_isSetBools[c_manufacturingOrderExternalIdSetIdx];
            set => m_isSetBools[c_manufacturingOrderExternalIdSetIdx] = value;
        }

        private string m_opExternalId;

        /// <summary>
        /// The ExternalId of the Operation.
        /// </summary>
        public string OperationExternalId
        {
            get => m_opExternalId;
            set
            {
                m_opExternalId = value;
                OperationExternalIdSet = true;
            }
        }

        public bool OperationExternalIdSet
        {
            get => m_isSetBools[c_opExternalIdSetIdx];
            set => m_isSetBools[c_opExternalIdSetIdx] = value;
        }

        private string m_activityExternalId;

        /// <summary>
        /// The ExternalId of the Activity.
        /// </summary>
        public string ActivityExternalId
        {
            get => m_activityExternalId;
            set
            {
                m_activityExternalId = value;
                ActivityExternalIdSet = true;
            }
        }

        public bool ActivityExternalIdSet
        {
            get => m_isSetBools[c_activityExternalIdSetIdx];
            set => m_isSetBools[c_activityExternalIdSetIdx] = value;
        }

        private string m_scheduledPlantExternalId;

        /// <summary>
        /// The PlantExternalId of a Resource on which the Activity is currently scheduled.
        /// This can be used to identify an Activity if the Operation and Activity ExternalIds are not specified.
        /// If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </summary>
        public string ScheduledPlantExternalId
        {
            get => m_scheduledPlantExternalId;
            set
            {
                m_scheduledPlantExternalId = value;
                ScheduledPlantExternalIdSet = true;
            }
        }

        public bool ScheduledPlantExternalIdSet
        {
            get => m_isSetBools[c_schedulePlantExternalIdSetIdx];
            set => m_isSetBools[c_schedulePlantExternalIdSetIdx] = value;
        }

        private string m_scheduledDeptExternalId;

        /// <summary>
        /// The DepartmentExternalId of a Resource on which the Activity is currently scheduled.
        /// This can be used to identify an Activity if the Operation and Activity ExternalIds are not specified.
        /// If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </summary>
        public string ScheduledDeptExternalId
        {
            get => m_scheduledDeptExternalId;
            set
            {
                m_scheduledDeptExternalId = value;
                ScheduledDepartmentExternalIdSet = true;
            }
        }

        public bool ScheduledDepartmentExternalIdSet
        {
            get => m_isSetBools[c_scheduledDeptExternalIdSetIdx];
            set => m_isSetBools[c_scheduledDeptExternalIdSetIdx] = value;
        }

        private string m_scheduledResourceExternalId;

        /// <summary>
        /// The ExternalId of a Resource on which the Activity is currently scheduled.
        /// This can be used to identify an Activity if the Operation and Activity ExternalIds are not specified.
        /// If the Operation and Activity ExternalIds are specified then this can be used to move the Activity to this Resource.
        /// </summary>
        public string ScheduledResourceExternalId
        {
            get => m_scheduledResourceExternalId;
            set
            {
                m_scheduledResourceExternalId = value;
                ScheduledResourceExternalIdSet = true;
            }
        }

        public bool ScheduledResourceExternalIdSet
        {
            get => m_isSetBools[c_scheduledResourceExternalIdSetIdx];
            set => m_isSetBools[c_scheduledResourceExternalIdSetIdx] = value;
        }

        private InternalActivityDefs.productionStatuses m_productionStatus;

        /// <summary>
        /// The current status of the Activity.
        /// Activities cannot have their status updated in reverse of this sequence: SettingUp, Running, PostProcessing, Transferring, Finished.
        /// </summary>
        public InternalActivityDefs.productionStatuses ProductionStatus
        {
            get => m_productionStatus;
            set => m_productionStatus = value;
        }

        private long m_reportedFinishDateTicks;
        public long ReportedFinishDateTicks => m_reportedFinishDateTicks;

        /// <summary>
        /// This value is used only when the the activity has been finished.
        /// </summary>
        public DateTime ReportedFinishDate
        {
            get => new (m_reportedFinishDateTicks);

            set => m_reportedFinishDateTicks = value.Ticks;
        }

        private long m_reportedStartDateTicks;
        public long ReportedStartDateTicks => m_reportedStartDateTicks;

        /// <summary>
        /// This value is used only when the the activity has been finished.
        /// </summary>
        public DateTime ReportedStartDate
        {
            get => new (m_reportedStartDateTicks);

            set => m_reportedStartDateTicks = value.Ticks;
        }

        private long m_reportedProcessingStartDateTicks;
        public long ReportedProcessingStartDateTicks => m_reportedProcessingStartDateTicks;

        /// <summary>
        /// This value is used only when the the activity has been finished.
        /// </summary>
        public DateTime ReportedProcessingStartDate
        {
            get => new(m_reportedProcessingStartDateTicks);

            set => m_reportedProcessingStartDateTicks = value.Ticks;
        }

        private long m_reportedProcessingEndDateTicks;
        public long ReportedProcessingEndDateTicks => m_reportedProcessingEndDateTicks;

        /// <summary>
        /// This value is used only when the the activity has been finished.
        /// </summary>
        public DateTime ReportedProcessingEndDate
        {
            get => new(m_reportedProcessingEndDateTicks);

            set => m_reportedProcessingEndDateTicks = value.Ticks;
        }


        private bool m_paused;

        public bool Paused
        {
            get => m_paused;
            set
            {
                m_paused = value;
                PausedSet = true;
            }
        }

        public bool PausedSet
        {
            get => m_isSetBools[c_pausedSetIdx];
            set => m_isSetBools[c_pausedSetIdx] = value;
        }

        #region hold
        private bool m_onHold;

        public bool OnHold
        {
            get => m_onHold;
            set
            {
                m_onHold = value;
                OnHoldSet = true;
            }
        }

        public bool OnHoldSet
        {
            get => m_isSetBools[c_onHoldSetIdx];
            set => m_isSetBools[c_onHoldSetIdx] = value;
        }

        private DateTime m_holdUntil;

        public DateTime HoldUntil
        {
            get => m_holdUntil;
            set
            {
                m_holdUntil = value;
                HoldUntilSet = true;
            }
        }

        public bool HoldUntilSet
        {
            get => m_isSetBools[c_onHoldUntilSetIdx];
            set => m_isSetBools[c_onHoldUntilSetIdx] = value;
        }

        private string m_holdReason;

        public string HoldReason
        {
            get => m_holdReason;
            set
            {
                m_holdReason = value;
                HoldReasonSet = true;
            }
        }

        public bool HoldReasonSet
        {
            get => m_isSetBools[c_onHoldReasonSetIdx];
            set => m_isSetBools[c_onHoldReasonSetIdx] = value;
        }
        #endregion

        private InternalActivityDefs.peopleUsages m_peopleUsage = InternalActivityDefs.peopleUsages.UseAllAvailable;

        /// <summary>
        /// Determines how many people are allocated to an Activity in the schedule.
        /// </summary>
        public InternalActivityDefs.peopleUsages PeopleUsage
        {
            get => m_peopleUsage;
            set
            {
                m_peopleUsage = value;
                PeopleUsageSet = true;
            }
        }

        public bool PeopleUsageSet
        {
            get => m_isSetBools[c_peopleUsageSetIdx];
            set => m_isSetBools[c_peopleUsageSetIdx] = value;
        }

        private decimal m_nbrOfPeople = 1;

        /// <summary>
        /// If PeopleUsage is set to UseSpecifiedNbr then this is the maximum number of people that will be allocated to the Activity.
        /// Fewer than this number will be allocated during time periods over which the Primary Resource's Nbr Of People is less than this value.
        /// </summary>
        public decimal NbrOfPeople
        {
            get => m_nbrOfPeople;
            set
            {
                if (value <= 0)
                {
                    throw new APSCommon.PTValidationException("2050");
                }

                m_nbrOfPeople = value;
                NbrOfPeopleSet = true;
            }
        }

        public bool NbrOfPeopleSet
        {
            get => m_isSetBools[c_nbrPeopleSetIdx];
            set => m_isSetBools[c_nbrPeopleSetIdx] = value;
        }

        private string m_comments;

        /// <summary>
        /// Text that can be entered by operators or loaded from bar code systems.
        /// </summary>
        public string Comments
        {
            get => m_comments;
            set
            {
                m_comments = value;
                CommentsSet = true;
            }
        }

        public bool CommentsSet
        {
            get => m_isSetBools[c_commentsSetIdx];
            set => m_isSetBools[c_commentsSetIdx] = value;
        }

        private string m_comments2;

        /// <summary>
        /// Text that can be entered by operators or loaded from bar code systems.
        /// </summary>
        public string Comments2
        {
            get => m_comments2;
            set
            {
                m_comments2 = value;
                Comments2Set = true;
            }
        }

        public bool Comments2Set
        {
            get => m_isSetBools[c_comments2SetIdx];
            set => m_isSetBools[c_comments2SetIdx] = value;
        }

        private TimeSpan m_reportedSetupSpan;

        public TimeSpan ReportedSetupSpan
        {
            get => m_reportedSetupSpan;
            set
            {
                m_reportedSetupSpan = value;
                ReportedSetupSpanSet = true;
            }
        }

        public bool ReportedSetupSpanSet
        {
            get => m_isSetBools[c_reportedSetupSpanSetIdx];
            set => m_isSetBools[c_reportedSetupSpanSetIdx] = value;
        }

        private TimeSpan m_reportedRunSpan;

        public TimeSpan ReportedRunSpan
        {
            get => m_reportedRunSpan;
            set
            {
                m_reportedRunSpan = value;
                ReportedRunSpanSet = true;
            }
        }

        public bool ReportedRunSpanSet
        {
            get => m_isSetBools[c_reportedRunSpanSetIdx];
            set => m_isSetBools[c_reportedRunSpanSetIdx] = value;
        }

        private TimeSpan m_reportedPostProcessingSpan;

        public TimeSpan ReportedPostProcessingSpan
        {
            get => m_reportedPostProcessingSpan;
            set
            {
                m_reportedPostProcessingSpan = value;
                ReportedPostProcessingSpanSet = true;
            }
        }

        public bool ReportedPostProcessingSpanSet
        {
            get => m_isSetBools[c_reportedPostProcessingSpanSetIdx];
            set => m_isSetBools[c_reportedPostProcessingSpanSetIdx] = value;
        }
        private TimeSpan m_reportedStorageSpan;
        public TimeSpan ReportedStorageSpan
        {
            get => m_reportedStorageSpan;
            set
            {
                m_reportedStorageSpan = value;
                ReportedStorageSpanSet = true;
            }
        }
        public bool ReportedStorageSpanSet
        {
            get => m_isSetBools[c_reportedStorageSpanSetIdx];
            set => m_isSetBools[c_reportedStorageSpanSetIdx] = value;
        } 
        private long m_reportedEndOfStorageTicks;
        public long ReportedEndOfStorageTicks
        {
            get => m_reportedEndOfStorageTicks;
            set
            {
                m_reportedEndOfStorageTicks = value;
                ReportedEndOfStorageTicksSet = true;
            }
        }
        public bool ReportedEndOfStorageTicksSet
        {
            get => m_isSetBools[c_reportedEndOfStorageSetIdx];
            set => m_isSetBools[c_reportedEndOfStorageSetIdx] = value;
        } 
        private long m_reportedEndOfPostProcessingTicks;
        public long ReportedEndOfPostProcessingTicks
        {
            get => m_reportedEndOfPostProcessingTicks;
            set
            {
                m_reportedEndOfPostProcessingTicks = value;
                ReportedStorageSpanSet = true;
            }
        }
        public bool ReportedEndOfPostProcessingTicksSet
        {
            get => m_isSetBools[c_reportedEndOfPostProcessingSetIdx];
            set => m_isSetBools[c_reportedEndOfPostProcessingSetIdx] = value;
        }

        private TimeSpan m_reportedCleanSpan;
        /// <summary>
        /// Clean time reported to have been spent so far in ticks.
        /// </summary>
        public TimeSpan ReportedCleanSpan
        {
            get => m_reportedCleanSpan;

            set
            {
                m_reportedCleanSpan = value;
                m_isSetBools[c_reportedCleanSpanIsSetIdx] = true;
            }
        }

        public bool ReportedCleanIsSet => m_isSetBools[c_reportedCleanSpanIsSetIdx];

        private int m_reportedCleanOutGrade;
        public int ReportedCleanoutGrade
        {
            get => m_reportedCleanOutGrade;
            set
            {
                m_reportedCleanOutGrade = value;
                m_isSetBools[c_reportedCleanoutGradeIsSetIdx] = true;
            }
        }

        public bool ReportedCleanGradeIsSet => m_isSetBools[c_reportedCleanoutGradeIsSetIdx];

        private decimal m_reportedScrapQty;

        public decimal ReportedScrapQty
        {
            get => m_reportedScrapQty;
            set
            {
                m_reportedScrapQty = value;
                ReportedScrapQtySet = true;
            }
        }

        public bool ReportedScrapQtySet
        {
            get => m_isSetBools[c_reportedScrapQtySetIdx];
            set => m_isSetBools[c_reportedScrapQtySetIdx] = value;
        }

        private decimal m_reportedGoodQty;

        public decimal ReportedGoodQty
        {
            get => m_reportedGoodQty;
            set
            {
                m_reportedGoodQty = value;
                ReportedGoodQtySet = true;
            }
        }

        public bool ReportedGoodQtySet
        {
            get => m_isSetBools[c_reportedGoodQtySetIdx];
            set => m_isSetBools[c_reportedGoodQtySetIdx] = value;
        }

        private bool m_reportedSpansAreIncremental;

        /// <summary>
        /// If true then all Reported Spans are added to the current total for the Activity.  Otherwise, they are treated as replacement values.
        /// </summary>
        public bool ReportedSpansAreIncremental
        {
            get => m_reportedSpansAreIncremental;
            set => m_reportedSpansAreIncremental = value;
        }

        private bool m_reportedQtiesAreIncremental;

        /// <summary>
        /// If true then all Reported Quantities are added to the current total for the Activity.  Otherwise, they are treated as replacement values.
        /// </summary>
        public bool ReportedQtiesAreIncremental
        {
            get => m_reportedQtiesAreIncremental;
            set => m_reportedQtiesAreIncremental = value;
        }

        private string m_reportedBy;

        /// <summary>
        /// The name or identifier of the person who entered the transaction in the data collection system.
        /// </summary>
        public string ReportedBy
        {
            get => m_reportedBy;
            set
            {
                m_reportedBy = value;
                ReportedBySet = true;
            }
        }

        public bool ReportedBySet
        {
            get => m_isSetBools[c_reportedBySetIdx];
            set => m_isSetBools[c_reportedBySetIdx] = value;
        }

        private DateTime m_timestamp;

        /// <summary>
        /// The date/time when the transaction occurred in the data collection system.
        /// </summary>
        public DateTime Timestamp
        {
            get => m_timestamp;
            set => m_timestamp = value;
        }

        private string m_notes;

        /// <summary>
        /// Text to be added to the History, etc.
        /// </summary>
        public string Notes
        {
            get => m_notes;
            set => m_notes = value;
        }

        private readonly List<MaterialIssue> m_materialIssues = new ();
        public List<MaterialIssue> MaterialIssues => m_materialIssues;

        private UserFieldList m_operationUserFields = new ();

        public bool OperationUserFieldsSet => m_bools[c_userFieldsSetIdx];

        public UserFieldList OperationUserFields
        {
            get => m_operationUserFields;
            set
            {
                m_operationUserFields = value;
                m_bools[c_userFieldsSetIdx] = true;
            }
        }

        public bool AllocateMaterialFromOnHand
        {
            get => m_bools[c_allocateMaterialsFromOnHand];
            set => m_bools[c_allocateMaterialsFromOnHand] = value;
        }

        public bool ReleaseProductToWarehouse
        {
            get => m_bools[c_releaseProductToWarehouse];
            set => m_bools[c_releaseProductToWarehouse] = value;
        }

        public bool BatchAmountSet
        {
            get => m_isSetBools[c_batchAmountSetIdx];
            set => m_isSetBools[c_batchAmountSetIdx] = value;
        }

        private decimal m_batchAmount;

        public decimal BatchAmount
        {
            get => m_batchAmount;
            set
            {
                m_batchAmount = value;
                BatchAmountSet = true;
            }
        }

        public DateTime NowFinishUtcTime => m_nowFinishUtcTime;

        public bool ActivityManualUpdateOnlySet
        {
            get => m_isSetBools[c_activityManualUpdateOnlyIdx];
            set => m_isSetBools[c_activityManualUpdateOnlyIdx] = value;
        }

        public bool ActivityManualUpdateOnly
        {
            get => m_isSetBools[c_activityManualUpdateOnlyIdx];
            set
            {
                m_isSetBools[c_activityManualUpdateOnlyIdx] = value;
                ActivityManualUpdateOnlySet = true;
            }
        }

        /// <summary>
        /// The time that this transmission was created for use in setting reported end date if it isn't provided
        /// </summary>
        private readonly DateTime m_nowFinishUtcTime = DateTime.UtcNow;
    }
}