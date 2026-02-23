using System.Collections;
using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.Localization;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// Summary description for JobT.
/// </summary>
public partial class JobT : ERPMaintenanceTransmission<JobT.Job>, IPTSerializable
{
    public new const int UNIQUE_ID = 222;

    #region PT Serialization
    public JobT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12437)
        {
            m_bools = new BoolVector32(reader);

            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Job node = new (reader);
                Add(node);
            }
        }
        else if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Job node = new(reader);
                Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_bools.Serialize(writer);

        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    private BoolVector32 m_bools;

    private const short c_includeCustomerAssociations = 0;
    private const short c_autoDeleteCustomerAssociations = 1;
    private const short c_autoDeleteOperationAttributes = 2;


    public bool AutoDeleteOperationAttributes
    {
        get => m_bools[c_autoDeleteOperationAttributes];
        set => m_bools[c_autoDeleteOperationAttributes] = value;
    } 
    public bool AutoDeleteCustomerAssociations
    {
        get => m_bools[c_autoDeleteCustomerAssociations];
        set => m_bools[c_autoDeleteCustomerAssociations] = value;
    }
    public bool IncludeCustomerAssociations
    {
        get => m_bools[c_includeCustomerAssociations];
        set => m_bools[c_includeCustomerAssociations] = value;
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public JobT() { }

    #region Database Loading
    public static JobDataSet FetchJobDataSet(System.Data.IDbCommand jobCmd,
                                             System.Data.IDbCommand moCmd,
                                             System.Data.IDbCommand opCmd,
                                             System.Data.IDbCommand resReqtCmd,
                                             System.Data.IDbCommand resReqtCapCmd,
                                             System.Data.IDbCommand activityCmd,
                                             System.Data.IDbCommand matlCmd,
                                             System.Data.IDbCommand prodCmd,
                                             System.Data.IDbCommand opAttributeCmd,
                                             System.Data.IDbCommand sucMoCmd,
                                             System.Data.IDbCommand altPathCmd,
                                             System.Data.IDbCommand altPathNodecmd,
                                             System.Data.IDbCommand a_customerConnectionCmd,
                                             bool includeResourceRequirementsQuery,
                                             bool includeMaterialRequirements,
                                             bool includeProducts,
                                             bool includeActivities,
                                             bool includeOpAttributes,
                                             bool includeSuccMOs,
                                             bool includePaths,
                                             bool a_includeCustomerConnections,
                                             bool includeOperations,
                                             bool includeManufacturingOrders)
    {
        JobDataSet ds = new();
        string typeName = typeof(JobT).Name;
        string fillType = "";

        try
        {
            fillType = "Jobs";
            FillDataTable(ds.Job, jobCmd, typeName);

            if (includeManufacturingOrders)
            {
                fillType = "Manufacturing Orders";
                FillDataTable(ds.ManufacturingOrder, moCmd, typeName);
            }

            if (includeOperations)
            {
                fillType = "Resource Operations";
                FillDataTable(ds.ResourceOperation, opCmd, typeName);
            }

            if (includeResourceRequirementsQuery)
            {
                fillType = "Resource Requirements";
                FillDataTable(ds.ResourceRequirement, resReqtCmd, typeName);
                fillType = "Required Capabiliites";
                FillDataTable(ds.Capability, resReqtCapCmd, typeName);
            }

            if (includeActivities)
            {
                fillType = "Internal Activities";
                FillDataTable(ds.Activity, activityCmd, typeName);
            }

            if (includeMaterialRequirements)
            {
                fillType = "Materials";
                FillDataTable(ds.MaterialRequirement, matlCmd, typeName);
            }

            if (includeProducts)
            {
                fillType = "Products";
                FillDataTable(ds.Product, prodCmd, typeName);
            }

            if (includeOpAttributes)
            {
                fillType = "Operation Attributes";
                FillDataTable(ds.ResourceOperationAttributes, opAttributeCmd, typeName);
            }

            if (includeSuccMOs)
            {
                fillType = "Successor Manufacturing Orders";
                FillDataTable(ds.SuccessorMO, sucMoCmd, typeName);
            }

            if (includePaths)
            {
                fillType = "Alternate Paths";
                FillDataTable(ds.AlternatePath, altPathCmd, typeName);
                fillType = "Alternate Path Nodes";
                FillDataTable(ds.AlternatePathNode, altPathNodecmd, typeName);
            }

            if (a_includeCustomerConnections)
            {
                fillType = "Customer Connections";
                FillDataTable(ds.Customer, a_customerConnectionCmd, typeName);
            }
        }
        catch (Exception e)
        {
            throw new PTException("4050", e, new object[] { fillType, e.Message });
        }

        return ds;
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    public void Fill(ref ApplicationExceptionList errors, JobDataSet ds, bool useLinearPath, HashSet<string> a_actUniqueKeys = null)
    {
        for (int jobI = 0; jobI < ds.Job.Count; jobI++)
        {
            JobDataSet.JobRow jobRow = (JobDataSet.JobRow)ds.Job.Rows[jobI];
            try
            {
                Job job = null;

                if (!jobRow.IsClassificationNull() && jobRow.Classification == "Order")
                {
                    jobRow.Classification = JobDefs.classifications.ProductionOrder.ToString();
                    errors.Add(new PTHandleableException("2843", new object[] { jobRow.ExternalId }));
                }

                job = new Job(jobRow, jobRow.ExternalId);
                AddMOs(job, jobRow, useLinearPath, a_actUniqueKeys);
                job.Validate();
                Add(job);
            }
            catch (CommonException ve)
            {
                //Add to list so some bad jobs are tolerated.
                errors.Add(new PTException("4052", ve.InnerException, new object[] { jobRow.ExternalId, jobRow.Name, ve.Message, ve.StackTrace }));
            }
            catch (Exception e)
            {
                //Add to list so some bad jobs are tolerated.
                errors.Add(new PTException("4053", e, new object[] { jobRow.ExternalId, jobRow.Name }));
            }
        }
    }

    /// <summary>
    /// Fill the transmission with a single Job's data from a DataSet.
    /// </summary>
    public void FillForJobCopy(JobDataSet ds, JobDataSet.JobRow jobRow, string newJobExternalId)
    {
        Job job = new(jobRow, newJobExternalId);
        AddMOs(job, jobRow, false, null);
        Add(job);
    }

    private void AddMOs(Job job, JobDataSet.JobRow jobRow, bool useLinearPath, HashSet<string> a_actUniqueKeys)
    {
        foreach (JobDataSet.ManufacturingOrderRow moRow in jobRow.GetManufacturingOrderRows())
        {
            try
            {
                ManufacturingOrder mo = new(moRow);
                if (moRow.IsCopyRoutingFromTemplateNull() || !moRow.CopyRoutingFromTemplate)
                {
                    AddResourceOperations(moRow, mo, a_actUniqueKeys);
                    if (!useLinearPath)
                    {
                        AddPaths(moRow, mo);
                    }
                    else
                    {
                        AddLinearPath(moRow, mo);
                    }

                    AddSuccessorMos(moRow, mo);
                }

                //Validate TransferQuantity
                //TODO: Could be moved into mo.Validate() but it seems to be more complicated without the rows
                HashSet<string> tranferQtyOverlapOperations = new();
                foreach (JobDataSet.AlternatePathRow pathRow in moRow.GetAlternatePathRows())
                {
                    foreach (JobDataSet.AlternatePathNodeRow nodeRow in pathRow.GetAlternatePathNodeRows())
                    {
                        if (!nodeRow.IsOverlapTypeNull() && nodeRow.OverlapType == InternalOperationDefs.overlapTypes.TransferQty.ToString())
                        {
                            tranferQtyOverlapOperations.Add(nodeRow.PredecessorOperationExternalId);
                        }
                    }
                }

                JobDataSet.ResourceOperationRow[] operationRows = moRow.GetResourceOperationRows();
                foreach (JobDataSet.ResourceOperationRow operationRow in operationRows)
                {
                    if (tranferQtyOverlapOperations.Contains(operationRow.ExternalId))
                    {
                        if (operationRow.IsOverlapTransferQtyNull() || operationRow.OverlapTransferQty <= 0)
                        {
                            throw new PTValidationException("4134", new object[] { jobRow.ExternalId, moRow.ExternalId, operationRow.ExternalId });
                        }
                    }
                }

                mo.Validate();
                job.Add(mo);
            }
            catch (CommonException ve)
            {
                throw new PTException("4054", ve.InnerException, new object[] { moRow.ExternalId, moRow.Name, ve.Message }); //store inner exception to get the root cause
            }
            catch (Exception e)
            {
                throw new PTException("4055", e, new object[] { moRow.ExternalId, moRow.Name });
            }
        }
    }

    private void AddLinearPath(JobDataSet.ManufacturingOrderRow moRow, ManufacturingOrder mo)
    {
        AlternatePath linearDefaultPath = new("1", "Default Linear Path", 0); //0 is preference            

        try
        {
            // Add a new node to the path
            JobDataSet.ResourceOperationRow[] opRows = moRow.GetResourceOperationRows();
            for (int opI = 0; opI < opRows.Length; opI++)
            {
                JobDataSet.ResourceOperationRow opRow = (JobDataSet.ResourceOperationRow)opRows.GetValue(opI);
                ResourceOperation op = new(opRow);
                AlternateNode node = new(op.ExternalId);
                linearDefaultPath.Add(node, mo);
            }

            mo.AddPath(linearDefaultPath);

            if (mo.DefaultPath == null)
            {
                mo.DefaultPath = linearDefaultPath;
            }
        }
        catch (CommonException ve)
        {
            throw new PTException("4056", ve.InnerException, new object[] { ve.Message });
        }
        catch (Exception e)
        {
            throw new PTException("4057", e);
        }
    }

    private void AddResourceOperations(JobDataSet.ManufacturingOrderRow moRow, ManufacturingOrder mo, HashSet<string> a_actUniqueKeys)
    {
        JobDataSet.ResourceOperationRow[] opRows = moRow.GetResourceOperationRows();
        for (int opI = 0; opI < opRows.Length; opI++)
        {
            JobDataSet.ResourceOperationRow opRow = (JobDataSet.ResourceOperationRow)opRows.GetValue(opI);
            ResourceOperation op = null;
            try
            {
                op = new ResourceOperation(opRow);
            }
            catch (CommonException ve)
            {
                throw new PTException("4058", ve.InnerException, new object[] { opRow.ExternalId, opRow.Name, ve.Message });
            }
            catch (Exception e)
            {
                throw new PTException("4059", e, new object[] { opRow.ExternalId, opRow.Name });
            }

            try
            {
                //Add ResourceRequirements
                AddResourceRequirements(opRow, op);
                AddAutoCreateRequirements(opRow, op);
            }
            catch (CommonException ve)
            {
                throw new PTException("4060", ve.InnerException, new object[] { opRow.ExternalId, opRow.Name, ve.Message });
            }
            catch (Exception e)
            {
                throw new PTException("4061", e, new object[] { opRow.ExternalId, opRow.Name });
            }

            try
            {
                //Add Activities
                AddActivities(opRow, op, a_actUniqueKeys);
            }
            catch (CommonException ve)
            {
                throw new PTException("4062", ve.InnerException, new object[] { opRow.ExternalId, opRow.Name, ve.Message });
            }
            catch (Exception e)
            {
                throw new PTException("4063", e, new object[] { opRow.ExternalId, opRow.Name });
            }

            try
            {
                //Add Materials
                AddMaterials(opRow, op);
            }
            catch (CommonException ve)
            {
                throw new PTException("4064", ve.InnerException, new object[] { opRow.ExternalId, opRow.Name, ve.Message });
            }
            catch (Exception e)
            {
                throw new PTException("4065", e, new object[] { opRow.ExternalId, opRow.Name });
            }

            try
            {
                //Add Products
                AddProducts(opRow, op);
            }
            catch (CommonException ve)
            {
                throw new PTException("4066", ve.InnerException, new object[] { opRow.ExternalId, opRow.Name, ve.Message });
            }
            catch (Exception e)
            {
                throw new PTException("4067", e, new object[] { opRow.ExternalId, opRow.Name });
            }

            try
            {
                //Add Resource Operation Attributes
                AddOpAttributes(opRow, op);
            }
            catch (CommonException ve)
            {
                throw new PTException("4068", ve.InnerException, new object[] { opRow.ExternalId, opRow.Name, ve.Message });
            }
            catch (Exception e)
            {
                throw new PTException("4069", e, new object[] { opRow.ExternalId, opRow.Name });
            }

            try
            {
                op.Validate();
                mo.AddOperation(op);
                ValidateProductionStatusAndOmitted(opRow, mo.ExternalId);
            }
            catch (Exception err)
            {
                throw new PTException("4058", err.InnerException, new object[] { opRow.ExternalId, opRow.Name, err.Message });
            }
        }
    }

    private void ValidateProductionStatusAndOmitted(JobDataSet.ResourceOperationRow a_opRow, string a_moExternalId)
    {
        StringBuilder sb = new();
        ResourceOperation op = new(a_opRow);
        foreach (JobDataSet.ActivityRow actRow in a_opRow.GetActivityRows())
        {
            InternalActivity act = new(actRow);
            if ((act.ProductionStatus == InternalActivityDefs.productionStatuses.SettingUp ||
                 act.ProductionStatus == InternalActivityDefs.productionStatuses.Running ||
                 act.ProductionStatus == InternalActivityDefs.productionStatuses.PostProcessing) &&
                (op.Omitted == BaseOperationDefs.omitStatuses.OmittedByUser || op.Omitted == BaseOperationDefs.omitStatuses.OmittedAutomatically))
            {
                sb.AppendFormat(Localizer.GetErrorString("2962", new object[] { a_opRow.JobExternalId, a_moExternalId, op.ExternalId, act.ExternalId, act.ProductionStatus.Localize() }));
                sb.AppendLine();
            }
        }

        if (sb.Length > 0)
        {
            throw new ValidationException(sb.ToString());
        }
    }

    /// <summary>
    /// Can be used to create Resource Requirements for an Operation.
    /// If used, then AutoCreatedCapabilityExternalId field must also be used and the Capabilities referenced
    /// by the Resource Requirements must be pre-defined or the System Option to auto-create Capabilities must be enabled.
    /// </summary>
    public enum autoCreateRequirementsType
    {
        /// <summary>
        /// Specify 'None' to avoid creating any Resource Requirements here.  They'll need to be specified explicitly in some other way.
        /// </summary>
        None,

        /// <summary>
        /// Create a single Resource Requirement for the Operation and use the AutoCreatedCapabilityExternalId as its required Capability.
        /// </summary>
        Resource,

        /// <summary>
        /// Create two Resource Requirements for the Operation, one for Labor and one for a Machine.  The Labor in this case is marked
        /// as the Primary Requirment (the one whose Capacity Intervals are used to calculate duration.  See Primary Resource Requirment.
        /// The Capability assigned to the Labor Requirement is named AutoCreatedCapabilityExternalId+'(L)'.
        /// The Capability assigned to the Machine Requirement is named AutoCreatedCapabilityExternalId+'(M)'.
        /// </summary>
        LaborAndMachine,

        /// <summary>
        /// Create two Resource Requirements for the Operation, one for Labor and one for a Machine.  The Machine in this case is marked
        /// as the Primary Requirment (the one whose Capacity Intervals are used to calculate duration.  See Primary Resource Requirment.
        /// The Capability assigned to the Machine Requirement is named AutoCreatedCapabilityExternalId+'(M)'.
        /// The Capability assigned to the Labor Requirement is named AutoCreatedCapabilityExternalId+'(L)'.
        /// </summary>
        MachineAndLabor
    }

    private autoCreateRequirementsType autoCreateRequirements = autoCreateRequirementsType.None;

    /// <summary>
    /// Can be used to create ResourceRequirements and Required Capabilities automatically instead of using the Resource Requirement and Capability Mappings.
    /// If used, the AutoCreatedCapabilityExternalId field must also be used to specify what values to use for Capabilty External Ids.
    /// </summary>
    public autoCreateRequirementsType AutoCreateRequirements
    {
        get => autoCreateRequirements;
        set => autoCreateRequirements = value;
    }

    private void AddPaths(JobDataSet.ManufacturingOrderRow moRow, ManufacturingOrder mo)
    {
        //Get a Hashtable of the OpExternalIds for the validation of successors
        JobDataSet.ResourceOperationRow[] ops = moRow.GetResourceOperationRows();
        Hashtable allOpsExternalIdsHash = new();
        for (int i = 0; i < ops.Length; i++)
        {
            allOpsExternalIdsHash.Add(ops[i].ExternalId, null);
        }

        //Add each of the Paths and each of the nodes for each path.
        JobDataSet.AlternatePathRow[] pathRows = moRow.GetAlternatePathRows();

        JobDataSetValidation.ValidateAlternatePaths(pathRows);

        for (int i = 0; i < pathRows.Length; i++)
        {
            JobDataSet.AlternatePathRow pathRow = (JobDataSet.AlternatePathRow)pathRows.GetValue(i);

            Hashtable nodesAdded = new();

            AlternatePath path = new(pathRow);
            JobDataSet.AlternatePathNodeRow[] nodeRows = pathRow.GetAlternatePathNodeRows();
            for (int n = 0; n < nodeRows.Length; n++)
            {
                JobDataSet.AlternatePathNodeRow nodeRow = (JobDataSet.AlternatePathNodeRow)nodeRows.GetValue(n);
                
                AlternateNode pathNode;
                if (nodesAdded.Contains(nodeRow.PredecessorOperationExternalId)) //already created the node for this operation.
                {
                    pathNode = (AlternateNode)nodesAdded[nodeRow.PredecessorOperationExternalId];
                }
                else //new node
                {
                    pathNode = new AlternateNode(nodeRow.PredecessorOperationExternalId);
                    nodesAdded.Add(pathNode.OperationExternalId, pathNode);
                    path.Add(pathNode);
                }

                if (!pathRow.IsAutoBuildLinearPathNull() && pathRow.AutoBuildLinearPath && n + 1 < nodeRows.Length)
                {
                    JobDataSet.AlternatePathNodeRow nextNodeRow = (JobDataSet.AlternatePathNodeRow)nodeRows.GetValue(n + 1);
                    PredecessorOperationAttributes predAtts = new(nodeRow, nextNodeRow.PredecessorOperationExternalId); //set the Successor to the next row
                    pathNode.AddSuccessor(predAtts);
                }
                else //use explicit path to create precedence relationships
                {
                    if (!nodeRow.IsSuccessorOperationExternalIdNull() && nodeRow.SuccessorOperationExternalId != "") //have a Successor.  Make sure it's a string as it can be dbnull
                    {
                        //Add the node if it's valid
                        if (allOpsExternalIdsHash.ContainsKey(nodeRow.SuccessorOperationExternalId)) //The successor is not a valid op in the MO.  It appears in the AltPath construction in scheduler that if there is no NODE it will be create, but the Op must exist.
                        {
                            PredecessorOperationAttributes predAtts = new(nodeRow, nodeRow.SuccessorOperationExternalId);
                            pathNode.AddSuccessor(predAtts);
                        }
                        else if (nodeRow.IsIgnoreInvalidSuccessorOperationExternalIdsNull() || !nodeRow.IgnoreInvalidSuccessorOperationExternalIds) //warn since it's not valid and they're not ignoring
                        {
                            throw new PTValidationException("2087", new object[] { nodeRow.PredecessorOperationExternalId, nodeRow.SuccessorOperationExternalId, nodeRow.PathExternalId });
                        }
                    }
                }

                //Validate OverlapTransferHrs
                if (!nodeRow.IsOverlapTransferHrsNull())
                {
                    if (nodeRow.OverlapTransferHrs >= TimeSpan.MaxValue.TotalHours || TimeSpan.FromHours(nodeRow.OverlapTransferHrs) > TimeSpan.FromDays(3650))
                    {
                        throw new PTValidationException("3010", new object[] { nodeRow.PredecessorOperationExternalId, nodeRow.SuccessorOperationExternalId });
                    }
                }
            }

            mo.AddPath(path);
            if (!moRow.IsDefaultPathExternalIdNull() && moRow.DefaultPathExternalId == path.ExternalId)
            {
                mo.DefaultPath = path;
            }
        }
    }

    private void AddSuccessorMos(JobDataSet.ManufacturingOrderRow moRow, ManufacturingOrder mo)
    {
        JobDataSet.SuccessorMORow[] sucMoRows = moRow.GetSuccessorMORows();
        for (int sucI = 0; sucI < sucMoRows.Length; sucI++)
        {
            JobDataSet.SuccessorMORow sucMoRow = (JobDataSet.SuccessorMORow)sucMoRows.GetValue(sucI);
            SuccessorMO sucMo = new(sucMoRow);

            mo.AddSuccessorMO(sucMo);
        }
    }

    private BaseOperation GetMoOperation(ManufacturingOrder mo, string opExternalId)
    {
        for (int i = 0; i < mo.OperationCount; i++)
        {
            if (mo.GetOperation(i).ExternalId == opExternalId)
            {
                return mo.GetOperation(i);
            }
        }

        throw new PTException("4070", new object[] { opExternalId });
    }

    private void AddResourceRequirements(JobDataSet.ResourceOperationRow a_opRow, ResourceOperation a_jobTOp)
    {
        JobDataSet.ResourceRequirementRow[] rrRows = a_opRow.GetResourceRequirementRows();
        for (int i = 0; i < rrRows.Length; i++)
        {
            JobDataSet.ResourceRequirementRow rrRow = (JobDataSet.ResourceRequirementRow)rrRows.GetValue(i);

            InternalOperation.ResourceRequirement rr = new(rrRow);
            a_jobTOp.AddResourceRequirement(rr);
            if (!rrRow.IsPrimaryRequirementNull() && rrRow.PrimaryRequirement) //PrimaryResourceRequirement
            {
                if (!rrRow.IsUsageStartNull() && rrRow.UsageStart != MainResourceDefs.usageEnum.Setup.ToString() && rrRow.UsageStart != MainResourceDefs.usageEnum.Run.ToString())
                {
                    throw new PTValidationException("4475", new object[] { a_opRow.ExternalId, a_opRow.JobExternalId });
                }

                a_jobTOp.PrimaryResourceRequirement = rr;
            }

            //Add Required Capabilities
            JobDataSet.CapabilityRow[] capRows = rrRow.GetCapabilityRows();
            for (int c = 0; c < capRows.Length; c++)
            {
                JobDataSet.CapabilityRow capabilityRow = (JobDataSet.CapabilityRow)capRows.GetValue(c);
                rr.AddRequiredCapability(capabilityRow.CapabilityExternalId);
            }

            if (!rrRow.IsCopyMaterialsToCapabilitiesNull() && rrRow.CopyMaterialsToCapabilities)
            {
                JobDataSet.MaterialRequirementRow[] matReqts = a_opRow.GetMaterialRequirementRows();
                for (int materialI = 0; materialI < matReqts.Length; materialI++)
                {
                    JobDataSet.MaterialRequirementRow material = (JobDataSet.MaterialRequirementRow)matReqts.GetValue(materialI);
                    rr.AddRequiredCapability(material.ExternalId);
                }
            }

            if (!rrRow.IsNumberOfResourcesRequiredNull() && rrRow.NumberOfResourcesRequired > 1)
            {
                int nbrOfExtraReqts = rrRow.NumberOfResourcesRequired - 1; //One is created to begin with
                for (int extraRqtsI = 0; extraRqtsI < nbrOfExtraReqts; extraRqtsI++)
                {
                    InternalOperation.ResourceRequirement extraRR = new(string.Format("{0}-RRAuto{1}", rrRow.ExternalId, extraRqtsI + 1)); //need to include the original rrexternalid to avoid duplicates if multiple RRs to begin with.
                    for (int c = 0; c < capRows.Length; c++)
                    {
                        JobDataSet.CapabilityRow capabilityRow = (JobDataSet.CapabilityRow)capRows.GetValue(c);
                        extraRR.AddRequiredCapability(capabilityRow.CapabilityExternalId);
                    }

                    extraRR.UsageStart = rr.UsageStart;
                    extraRR.UsageEnd = rr.UsageEnd;
                    extraRR.AttentionPercent = rr.AttentionPercent;
                    extraRR.Description = rr.Description;

                    a_jobTOp.AddResourceRequirement(extraRR);
                }
            }
        }
    }

    private void AddAutoCreateRequirements(JobDataSet.ResourceOperationRow a_opRow, ResourceOperation a_jobTOp)
    {
        //AutoCreate ResourceRequirements and Capabilities
        if (!a_opRow.IsAutoCreateRequirementsNull() && a_opRow.AutoCreateRequirements.ToUpper() == autoCreateRequirementsType.Resource.ToString().ToUpper())
        {
            InternalOperation.ResourceRequirement rr = new("AutoRR");
            rr.AddRequiredCapability(a_opRow.AutoCreatedCapabilityExternalId);

            a_jobTOp.AddResourceRequirement(rr);
        }
        else if (!a_opRow.IsAutoCreateRequirementsNull() && a_opRow.AutoCreateRequirements.ToUpper() == autoCreateRequirementsType.LaborAndMachine.ToString().ToUpper())
        {
            //Labor is Primary RR
            InternalOperation.ResourceRequirement rr = new("AutoRRLabor");
            rr.AddRequiredCapability(a_opRow.AutoCreatedCapabilityExternalId + "_Labor");
            a_jobTOp.AddResourceRequirement(rr);

            rr = new InternalOperation.ResourceRequirement("AutoRRMachine");
            rr.AddRequiredCapability(a_opRow.AutoCreatedCapabilityExternalId + "_Machine");
            a_jobTOp.AddResourceRequirement(rr);
        }
        else if (!a_opRow.IsAutoCreateRequirementsNull() && a_opRow.AutoCreateRequirements.ToUpper() == autoCreateRequirementsType.MachineAndLabor.ToString().ToUpper())
        {
            //Machine is Primary RR
            InternalOperation.ResourceRequirement rr = new("AutoRRMachine");
            rr.AddRequiredCapability(a_opRow.AutoCreatedCapabilityExternalId + "_Machine");
            a_jobTOp.AddResourceRequirement(rr);

            rr = new InternalOperation.ResourceRequirement("AutoRRLabor");
            rr.AddRequiredCapability(a_opRow.AutoCreatedCapabilityExternalId + "_Labor");
            a_jobTOp.AddResourceRequirement(rr);
        }
    }

    private void AddActivities(JobDataSet.ResourceOperationRow opRow, ResourceOperation jobTOp, HashSet<string> a_actUniqueKeys)
    {
        JobDataSet.ActivityRow[] activityRows = opRow.GetActivityRows();

        for (int i = 0; i < activityRows.Length; i++)
        {
            JobDataSet.ActivityRow activityRow = (JobDataSet.ActivityRow)activityRows.GetValue(i);
            InternalActivity activity = new(activityRow);
            jobTOp.AddInternalActivity(activity);

            string key = Activity.GetUniqueKey(activityRow);
            if (a_actUniqueKeys != null && !a_actUniqueKeys.Contains(key))
            {
                a_actUniqueKeys.Add(key);
            }
        }

        if (!opRow.IsAutoSplitNull() && opRow.AutoSplit && jobTOp.InternalActivityCount <= 1) //ignore if there are multiple activities already imported in case they've imported the splits to the ERP and are now importing qties, etc.
        {
            decimal qtyPerActivity = opRow.MinAutoSplitAmount;
            decimal qtyRemaining = opRow.RequiredFinishQty;
            if (qtyRemaining > 2 * qtyPerActivity)
            {
                if (jobTOp.InternalActivityCount == 1)
                {
                    InternalActivity activity1 = jobTOp.GetInternalActivity(0);
                    activity1.RequiredFinishQty = qtyPerActivity;
                    qtyRemaining = qtyRemaining - qtyPerActivity;
                }

                int splitNbr = 1;
                while (qtyRemaining > 2 * qtyPerActivity)
                {
                    string newExternalId = string.Format("AUTOSPLIT-{0}", splitNbr);
                    InternalActivity newActivity = new(newExternalId);
                    newActivity.RequiredFinishQty = qtyPerActivity;
                    qtyRemaining = qtyRemaining - qtyPerActivity;
                    jobTOp.AddInternalActivity(newActivity);
                    splitNbr++;
                }

                if (qtyRemaining > 0 && jobTOp.InternalActivityCount > 0)
                {
                    InternalActivity activity1 = jobTOp.GetInternalActivity(0);
                    activity1.RequiredFinishQty = activity1.RequiredFinishQty + qtyRemaining;
                }
            }
        }
    }

    private void AddMaterials(JobDataSet.ResourceOperationRow opRow, ResourceOperation jobTOp)
    {
        JobDataSet.MaterialRequirementRow[] stockMaterialRows = opRow.GetMaterialRequirementRows();
        for (int i = 0; i < stockMaterialRows.Length; i++)
        {
            JobDataSet.MaterialRequirementRow materialRow = (JobDataSet.MaterialRequirementRow)stockMaterialRows.GetValue(i);
            MaterialRequirement material = new(materialRow);

            jobTOp.Add(material);
        }
    }

    private void AddProducts(JobDataSet.ResourceOperationRow opRow, ResourceOperation jobTOp)
    {
        JobDataSet.ProductRow[] productRows = opRow.GetProductRows();
        for (int i = 0; i < productRows.Length; i++)
        {
            JobDataSet.ProductRow productRow = (JobDataSet.ProductRow)productRows.GetValue(i);
            Product product = new(productRow);
            jobTOp.Add(product);
        }
    }

    private void AddOpAttributes(JobDataSet.ResourceOperationRow a_opRow, ResourceOperation a_jobTOp)
    {
        JobDataSet.ResourceOperationAttributesRow[] attRows = a_opRow.GetResourceOperationAttributesRows();
        HashSet<string> ptAttributeExternalId = new();
        HashSet<string> attributeExternalIds = new();

        foreach (JobDataSet.ResourceOperationAttributesRow attRow in attRows)
        {
            if (!ptAttributeExternalId.Add(attRow.AttributeExternalId))
            {
                throw new PTValidationException("2858", new object[] { "PtAttributeExternalId", attRow.AttributeExternalId });
            }

            OperationAttribute att = new(attRow.AttributeExternalId);

            if (!attRow.IsCodeNull())
            {
                att.Code = attRow.Code;
            }

            if (!attRow.IsNumberNull())
            {
                att.Number = attRow.Number;
            }

            if (!attRow.IsCostNull())
            {
                att.Cost = attRow.Cost;
            }

            if (!attRow.IsDurationHrsNull())
            {
                att.Duration = TimeSpan.FromHours(attRow.DurationHrs);
            }

            if (!attRow.IsCodeManualUpdateOnlyNull())
            {
                att.CodeManualUpdateOnly = attRow.CodeManualUpdateOnly;
            }

            if (!attRow.IsNumberManualUpdateOnlyNull())
            {
                att.NumberManualUpdateOnly = attRow.NumberManualUpdateOnly;
            }

            if (!attRow.IsCostManualUpdateOnlyNull())
            {
                att.CostManualUpdateOnly = attRow.CostManualUpdateOnly;
            }

            if (!attRow.IsDurationHrsManualUpdateOnlyNull())
            {
                att.DurationManualUpdateOnly = attRow.DurationHrsManualUpdateOnly;
            }

            if (!attRow.IsColorCodeNull())
            {
                att.ColorCode = ColorUtils.GetColorFromHexString(attRow.ColorCode);
            }

            if (!attRow.IsColorCodeManualUpdateOnlyNull())
            {
                att.ColorCodeManualUpdateOnly = attRow.ColorCodeManualUpdateOnly;
            }

            if (!attRow.IsCostOverrideNull())
            {
                att.CostOverride = attRow.CostOverride;
            }

            if (!attRow.IsDurationOverrideNull())
            {
                att.DurationOverride = attRow.DurationOverride;
            }

            if (!attRow.IsColorOverrideNull())
            {
                att.ColorOverride = attRow.ColorOverride;
            }

            if (!attRow.IsShowInGanttOverrideNull())
            {
                att.ShowInGanttOverride = attRow.ShowInGanttOverride;
            }

            if (!attRow.IsShowInGanttNull())
            {
                att.ShowInGantt = attRow.ShowInGantt;
            }

            if (!attRow.IsShowInGanttManualUpdateOnlyNull())
            {
                att.ShowInGanttOverride = attRow.ShowInGanttManualUpdateOnly;
            }

            a_jobTOp.ResourceAttributes.Add(att);
        }
    }
    #endregion Database Loading

    public class BaseActivity : PTObjectIdBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 244;

        #region PT Serialization
        public BaseActivity(IReader reader)
            : base(reader)
        {
            #region 729
            if (reader.VersionNumber >= 729)
            {
                reader.Read(out reportedStartDateTicks);
                reader.Read(out reportedFinishDateTicks);
                reader.Read(out requiredFinishQty);
                reader.Read(out m_anchor);
                reader.Read(out m_anchorStartDate);
                reader.Read(out m_batchAmount);
                bools = new BoolVector32(reader);
            }
            #endregion

            #region 474
            else if (reader.VersionNumber >= 474)
            {
                reader.Read(out reportedStartDateTicks);
                reader.Read(out reportedFinishDateTicks);
                reader.Read(out requiredFinishQty);
                reader.Read(out m_anchor);
                reader.Read(out m_anchorStartDate);
                bools = new BoolVector32(reader);
            }
            #endregion

            #region Version 345
            else if (reader.VersionNumber >= 345)
            {
                reader.Read(out reportedStartDateTicks);
                reader.Read(out reportedFinishDateTicks);
                reader.Read(out requiredFinishQty);
                reader.Read(out m_anchor);
                reader.Read(out m_anchorStartDate);
            }
            else if (reader.VersionNumber >= 108)
            {
                reader.Read(out reportedFinishDateTicks);
                reader.Read(out requiredFinishQty);
                reader.Read(out m_anchor);
                reader.Read(out m_anchorStartDate);
            }
            else if (reader.VersionNumber >= 70)
            {
                reader.Read(out reportedFinishDateTicks);
                reader.Read(out requiredFinishQty);
            }
            #endregion

            #region 1
            else if (reader.VersionNumber >= 1)
            {
                reader.Read(out requiredFinishQty);
            }
            #endregion
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write(reportedStartDateTicks);
            writer.Write(reportedFinishDateTicks);
            writer.Write(requiredFinishQty);
            writer.Write(m_anchor);
            writer.Write(m_anchorStartDate);
            writer.Write(m_batchAmount);
            bools.Serialize(writer);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public BaseActivity() { }

        public BaseActivity(string externalId)
            : base(externalId) { }

        public BaseActivity(JobDataSet.ActivityRow row)
            : base(row.ExternalId)
        {
            requiredFinishQty = row.RequiredFinishQty;
            if (!row.IsReportedStartDateNull() && row.ReportedStartDate != PTDateTime.MinValue.ToDateTime())
            {
                ReportedStartDate = row.ReportedStartDate.ToServerTime();
            }

            if (!row.IsReportedFinishDateNull() && row.ReportedFinishDate != PTDateTime.MinValue.ToDateTime())
            {
                ReportedFinishDate = row.ReportedFinishDate.ToServerTime();
            }

            if (!row.IsAnchorNull())
            {
                Anchor = row.Anchor;
            }

            if (!row.IsAnchorStartDateNull())
            {
                m_anchorStartDate = row.AnchorStartDate.ToServerTime().Ticks;
            }

            if (!row.IsBatchAmountNull())
            {
                m_batchAmount = row.BatchAmount;
            }
        }

        #region Shared Properties
        private BoolVector32 bools;
        private const int c_reportedFinishDateSetIdx = 0;
        private const int c_anchorSetIdx = 1;
        private const int c_batchAmountSetIdx = 2;
        private const int c_reportedStartDateIsSetIdx = 3;

        private decimal requiredFinishQty;

        [Required(true)]
        public decimal RequiredFinishQty
        {
            get => requiredFinishQty;
            set => requiredFinishQty = value;
        }

        private long reportedStartDateTicks;

        public long ReportedStartDateTicks => reportedStartDateTicks;

        [Required(false)]
        public DateTime ReportedStartDate
        {
            get => new(reportedStartDateTicks);

            set
            {
                reportedStartDateTicks = value.Ticks;
                bools[c_reportedStartDateIsSetIdx] = true;
            }
        }

        public bool ReportedStartDateIsSet => bools[c_reportedStartDateIsSetIdx];

        private long reportedFinishDateTicks;

        public long ReportedFinishDateTicks => reportedFinishDateTicks;

        [Required(false)]
        public DateTime ReportedFinishDate
        {
            get => new(reportedFinishDateTicks);

            set
            {
                if (reportedFinishDateTicks != value.Ticks)
                {
                    reportedFinishDateTicks = value.Ticks;
                    bools[c_reportedFinishDateSetIdx] = true;
                }
            }
        }

        public bool ReportedFinishDateSet => bools[c_reportedFinishDateSetIdx];

        private bool m_anchor;

        public bool AnchorSet => bools[c_anchorSetIdx];

        /// <summary>
        /// Whether to Anchor the Activity using the specified Achor Date when the Activity is scheduled.
        /// </summary>
        public bool Anchor
        {
            get => m_anchor;
            set
            {
                m_anchor = value;
                bools[c_anchorSetIdx] = true;
            }
        }

        private long m_anchorStartDate;

        /// <summary>
        /// The Date and time where the Activity should be Anchored to start when the Anchor option is set to 'true'.
        /// </summary>
        public DateTime AnchorStartDate
        {
            get => new(m_anchorStartDate);
            set => m_anchorStartDate = value.Ticks;
        }

        private decimal m_batchAmount;

        /// <summary>
        /// Alternative field to use for batching
        /// </summary>
        public decimal BatchAmount
        {
            get => m_batchAmount;
            internal set
            {
                m_batchAmount = value;
                BatchAmountSet = true;
            }
        }

        public bool BatchAmountSet
        {
            get => bools[c_batchAmountSetIdx];
            private set => bools[c_batchAmountSetIdx] = value;
        }
        #endregion

        public override void Validate()
        {
            base.Validate();
            if (RequiredFinishQty < 0)
            {
                throw new ValidationException("2091", new object[] { RequiredFinishQty, ExternalId }); //JMC TGN Fix
            }
        }
    }

    public class InternalActivity : BaseActivity, IPTSerializable
    {
        public new const int UNIQUE_ID = 243;

        #region PT Serialization
        public InternalActivity(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 12536)
            {
                m_bools = new BoolVector32(reader);
                m_bools2 = new BoolVector32(reader);

                reader.Read(out int val);
                productionStatus = (InternalActivityDefs.productionStatuses)val;
                reader.Read(out reportedGoodQty);
                reader.Read(out reportedScrapQty);
                reader.Read(out paused);
                reader.Read(out val);
                peopleUsage = (InternalActivityDefs.peopleUsages)val;
                reader.Read(out nbrOfPeople);
                reader.Read(out comments);

                reader.Read(out reportedRunSpan);
                reader.Read(out reportedEndOfRunDate);

                reader.Read(out reportedSetupSpan);
                reader.Read(out reportedPostProcessingSpan);
                reader.Read(out m_reportedStorageTicks); // new in 12522

                reader.Read(out m_reportedEndOfPostProcessingDate); // new in 12523
                reader.Read(out m_reportedEndOfStorageDate);// new in 12523

                reader.Read(out m_reportedCleanSpan);
                reader.Read(out m_reportedCleanOutGrade);

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out cycleSpan);
                reader.Read(out qtyPerCycle);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out m_cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out m_endOfStoragePostProcessing);
                reader.Read(out long ticks);
                m_reportedStartOfProcessingDate = new DateTime(ticks);
                reader.Read(out comments2);
                reader.Read(out m_actualResourcesUsedCsv);
                reader.Read(out m_nowFinishUtcTime);
                reader.Read(out m_productionSetupCost);
                reader.Read(out m_cleanoutCost);
            }
            else if (reader.VersionNumber >= 12523)
            {
                m_bools = new BoolVector32(reader);
                m_bools2 = new BoolVector32(reader);

                reader.Read(out int val);
                productionStatus = (InternalActivityDefs.productionStatuses)val;
                reader.Read(out reportedGoodQty);
                reader.Read(out reportedScrapQty);
                reader.Read(out paused);
                reader.Read(out val);
                peopleUsage = (InternalActivityDefs.peopleUsages)val;
                reader.Read(out nbrOfPeople);
                reader.Read(out comments);

                reader.Read(out reportedRunSpan);
                reader.Read(out reportedEndOfRunDate);

                reader.Read(out reportedSetupSpan);
                reader.Read(out reportedPostProcessingSpan);
                reader.Read(out m_reportedStorageTicks); // new in 12522

                reader.Read(out m_reportedEndOfPostProcessingDate); // new in 12523
                reader.Read(out m_reportedEndOfStorageDate);// new in 12523

                reader.Read(out m_reportedCleanSpan);
                reader.Read(out m_reportedCleanOutGrade);

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out cycleSpan);
                reader.Read(out qtyPerCycle);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out m_cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out m_endOfStoragePostProcessing);
                reader.Read(out long ticks);
                m_reportedStartOfProcessingDate = new DateTime(ticks);
                reader.Read(out comments2);
                reader.Read(out m_actualResourcesUsedCsv);
                reader.Read(out m_nowFinishUtcTime);
            }
            else if (reader.VersionNumber >= 12522)
            {
                m_bools = new BoolVector32(reader);

                reader.Read(out int val);
                productionStatus = (InternalActivityDefs.productionStatuses)val;
                reader.Read(out reportedGoodQty);
                reader.Read(out reportedScrapQty);
                reader.Read(out paused);
                reader.Read(out val);
                peopleUsage = (InternalActivityDefs.peopleUsages)val;
                reader.Read(out nbrOfPeople);
                reader.Read(out comments);

                reader.Read(out reportedRunSpan);
                reader.Read(out reportedEndOfRunDate);

                reader.Read(out reportedSetupSpan);
                reader.Read(out reportedPostProcessingSpan);
                reader.Read(out m_reportedStorageTicks); // new in 12522

                reader.Read(out m_reportedCleanSpan);
                reader.Read(out m_reportedCleanOutGrade);

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out cycleSpan);
                reader.Read(out qtyPerCycle);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out m_cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out m_endOfStoragePostProcessing);
                reader.Read(out long ticks);
                m_reportedStartOfProcessingDate = new DateTime(ticks);
                reader.Read(out comments2);
                reader.Read(out m_actualResourcesUsedCsv);
                reader.Read(out m_nowFinishUtcTime);
            }
            else if (reader.VersionNumber >= 12416)
            {
                m_bools = new BoolVector32(reader);

                reader.Read(out int val);
                productionStatus = (InternalActivityDefs.productionStatuses)val;
                reader.Read(out reportedGoodQty);
                reader.Read(out reportedScrapQty);
                reader.Read(out paused);
                reader.Read(out val);
                peopleUsage = (InternalActivityDefs.peopleUsages)val;
                reader.Read(out nbrOfPeople);
                reader.Read(out comments);

                reader.Read(out reportedRunSpan);
                reader.Read(out reportedEndOfRunDate);

                reader.Read(out reportedSetupSpan);
                reader.Read(out reportedPostProcessingSpan);

                reader.Read(out m_reportedCleanSpan);
                reader.Read(out m_reportedCleanOutGrade);

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out cycleSpan);
                reader.Read(out qtyPerCycle);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out m_cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out m_endOfStoragePostProcessing);
                reader.Read(out long ticks);
                m_reportedStartOfProcessingDate = new DateTime(ticks);
                reader.Read(out comments2);
                reader.Read(out m_actualResourcesUsedCsv);
                reader.Read(out m_nowFinishUtcTime);
            }
            else if (reader.VersionNumber >= 12302)
            {
                reader.Read(out int val);
                productionStatus = (InternalActivityDefs.productionStatuses)val;
                reader.Read(out reportedGoodQty);
                reader.Read(out reportedScrapQty);
                reader.Read(out paused);
                reader.Read(out val);
                peopleUsage = (InternalActivityDefs.peopleUsages)val;
                reader.Read(out nbrOfPeople);
                reader.Read(out comments);

                reader.Read(out reportedRunSpan);
                reader.Read(out reportedEndOfRunDate);

                reader.Read(out reportedSetupSpan);
                reader.Read(out reportedPostProcessingSpan);
                m_bools = new BoolVector32(reader);

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out cycleSpan);
                reader.Read(out qtyPerCycle);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out m_cleanSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out m_endOfStoragePostProcessing);
                reader.Read(out long ticks);
                m_reportedStartOfProcessingDate = new DateTime(ticks);
                reader.Read(out comments2);
                reader.Read(out m_actualResourcesUsedCsv);
                reader.Read(out m_nowFinishUtcTime);
            }
            else if (reader.VersionNumber >= 745)
            {
                int val;
                reader.Read(out val);
                productionStatus = (InternalActivityDefs.productionStatuses)val;
                reader.Read(out reportedGoodQty);
                reader.Read(out reportedScrapQty);
                reader.Read(out paused);
                reader.Read(out val);
                peopleUsage = (InternalActivityDefs.peopleUsages)val;
                reader.Read(out nbrOfPeople);
                reader.Read(out comments);

                reader.Read(out reportedRunSpan);
                reader.Read(out reportedEndOfRunDate);

                reader.Read(out reportedSetupSpan);
                reader.Read(out reportedPostProcessingSpan);
                m_bools = new BoolVector32(reader);

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out cycleSpan);
                reader.Read(out qtyPerCycle);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out m_endOfStoragePostProcessing);
                long ticks;
                reader.Read(out ticks);
                m_reportedStartOfProcessingDate = new DateTime(ticks);
                reader.Read(out comments2);
                reader.Read(out m_actualResourcesUsedCsv);
                reader.Read(out m_nowFinishUtcTime);
            }

            #region 730
            else if (reader.VersionNumber >= 730)
            {
                int val;
                reader.Read(out val);
                productionStatus = (InternalActivityDefs.productionStatuses)val;
                reader.Read(out reportedGoodQty);
                reader.Read(out reportedScrapQty);
                reader.Read(out paused);
                reader.Read(out val);
                peopleUsage = (InternalActivityDefs.peopleUsages)val;
                reader.Read(out nbrOfPeople);
                reader.Read(out comments);

                reader.Read(out reportedRunSpan);
                reader.Read(out reportedEndOfRunDate);

                reader.Read(out reportedSetupSpan);
                reader.Read(out reportedPostProcessingSpan);
                m_bools = new BoolVector32(reader);

                _productionInfoFlags = new ProductionInfoFlags(reader);
                reader.Read(out cycleSpan);
                reader.Read(out qtyPerCycle);
                reader.Read(out setupSpan);
                reader.Read(out postProcessingSpan);
                reader.Read(out planningScrapPercent);
                reader.Read(out m_endOfStoragePostProcessing);
                long ticks;
                reader.Read(out ticks);
                m_reportedStartOfProcessingDate = new DateTime(ticks);
                reader.Read(out comments2);
                reader.Read(out m_actualResourcesUsedCsv);
            }
            #endregion
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);
            m_bools.Serialize(writer);
            m_bools2.Serialize(writer);

            writer.Write((int)productionStatus);
            writer.Write(reportedGoodQty);
            writer.Write(reportedScrapQty);
            writer.Write(paused);
            writer.Write((int)peopleUsage);
            writer.Write(nbrOfPeople);
            writer.Write(comments);

            writer.Write(reportedRunSpan);
            writer.Write(reportedEndOfRunDate);

            writer.Write(reportedSetupSpan);
            writer.Write(reportedPostProcessingSpan);
            writer.Write(m_reportedStorageTicks); // new in 12522
            writer.Write(m_reportedEndOfPostProcessingDate); // new in 12523
            writer.Write(m_reportedEndOfStorageDate); // new in 12523
            writer.Write(m_reportedCleanSpan);
            writer.Write(m_reportedCleanOutGrade);

            _productionInfoFlags.Serialize(writer);
            writer.Write(cycleSpan);
            writer.Write(qtyPerCycle);
            writer.Write(setupSpan);
            writer.Write(postProcessingSpan);
            writer.Write(m_cleanSpan);
            writer.Write(planningScrapPercent);
            writer.Write(m_endOfStoragePostProcessing);
            writer.Write(m_reportedStartOfProcessingDate.Ticks);
            writer.Write(comments2);
            writer.Write(m_actualResourcesUsedCsv);
            writer.Write(m_nowFinishUtcTime);
            writer.Write(m_productionSetupCost);
            writer.Write(m_cleanoutCost);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public InternalActivity(string externalId)
            : base(externalId)
        {
            m_nowFinishUtcTime = DateTime.UtcNow;
        }

        public InternalActivity(JobDataSet.ActivityRow row)
            : base(row)
        {
            m_nowFinishUtcTime = DateTime.UtcNow;

            if (!row.IsProductionStatusNull())
            {
                try
                {
                    ProductionStatus = (InternalActivityDefs.productionStatuses)Enum.Parse(typeof(InternalActivityDefs.productionStatuses), row.ProductionStatus);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            row.ProductionStatus, "Activity", "ProductionStatus",
                            string.Join(", ", Enum.GetNames(typeof(InternalActivityDefs.productionStatuses)))
                        });
                }
            }

            if (!row.IsReportedGoodQtyNull())
            {
                ReportedGoodQty = row.ReportedGoodQty;
            }

            if (!row.IsReportedPostProcessingHrsNull())
            {
                ReportedPostProcessingSpan = PTDateTime.GetSafeTimeSpan(row.ReportedPostProcessingHrs);
            }

            if (!row.IsReportedRunHrsNull())
            {
                ReportedRunSpan = PTDateTime.GetSafeTimeSpan(row.ReportedRunHrs);
            }

            if (!row.IsReportedScrapQtyNull())
            {
                ReportedScrapQty = row.ReportedScrapQty;
            }

            if (!row.IsReportedSetupHrsNull())
            {
                ReportedSetupSpan = PTDateTime.GetSafeTimeSpan(row.ReportedSetupHrs);
            }

            if (row.RequiredFinishQty == 0)
            {
                throw new PTValidationException("2992", new object[] { "RequiredFinishQty", row.ExternalId });
            }

            RequiredFinishQty = row.RequiredFinishQty;
            if (!row.IsPausedNull())
            {
                Paused = row.Paused;
            }

            if (!row.IsPeopleUsageNull())
            {
                try
                {
                    PeopleUsage = (InternalActivityDefs.peopleUsages)Enum.Parse(typeof(InternalActivityDefs.peopleUsages), row.PeopleUsage);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            row.PeopleUsage, "Activity", "PeopleUsage",
                            string.Join(", ", Enum.GetNames(typeof(InternalActivityDefs.peopleUsages)))
                        });
                }
            }

            if (!row.IsNbrOfPeopleNull())
            {
                NbrOfPeople = row.NbrOfPeople;
            }

            if (!row.IsCommentsNull())
            {
                Comments = row.Comments;
            }

            if (!row.IsComments2Null())
            {
                Comments2 = row.Comments2;
            }

            if (!row.IsCycleHrsNull())
            {
                if (row.CycleHrs == 0)
                {
                    throw new PTValidationException("2992", new object[] { "CycleHrs", row.ExternalId });
                }

                CycleSpan = row.CycleHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(row.CycleHrs); //else overflows
            }

            if (!row.IsQtyPerCycleNull())
            {
                if (row.QtyPerCycle == 0)
                {
                    throw new PTValidationException("2992", new object[] { "QtyPerCycle", row.ExternalId });
                }

                QtyPerCycle = row.QtyPerCycle;
            }

            if (!row.IsSetupHrsNull())
            {
                SetupSpan = row.SetupHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(row.SetupHrs); //else overflows
            }

            if (!row.IsPostProcessingHrsNull())
            {
                PostProcessingSpan = row.PostProcessingHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(row.PostProcessingHrs); //else overflows
            }

            if (!row.IsCleanHrsNull())
            {
                CleanSpan = row.CleanHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(row.CleanHrs); //else overflows
            }

            if (!row.IsCleanOutGradeNull())
            {
                m_cleanOutGrade = row.CleanOutGrade;
            }

            if (!row.IsStorageHrsNull())
            {
                EndOfStoragePostProcessingTimeSpan = row.StorageHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(row.StorageHrs); //else overflows
            }

            if (!row.IsPlanningScrapPercentNull())
            {
                if (row.PlanningScrapPercent >= 0 && row.PlanningScrapPercent < 1)
                {
                    PlanningScrapPercent = row.PlanningScrapPercent;
                }
                else
                {
                    throw new PTValidationException("2239", new object[] { row.PlanningScrapPercent });
                }
            }

            if (!row.IsCycleSpanManualUpdateOnlyNull())
            {
                ProductionInfoFlags.CycleManualUpdateOnly = row.CycleSpanManualUpdateOnly;
            }

            if (!row.IsQtyPerCycleManualUpdateOnlyNull())
            {
                ProductionInfoFlags.QtyPerCycle = row.QtyPerCycleManualUpdateOnly;
            }

            if (!row.IsSetupTimeManualUpdateOnlyNull())
            {
                ProductionInfoFlags.SetupManualUpdateOnly = row.SetupTimeManualUpdateOnly;
            }

            if (!row.IsPostProcessManualUpdateOnlyNull())
            {
                ProductionInfoFlags.PostProcessingManualUpdateOnly = row.PostProcessManualUpdateOnly;
            }
            
            if (!row.IsStorageManualUpdateOnlyNull())
            {
                ProductionInfoFlags.StorageManualUpdateOnly = row.StorageManualUpdateOnly;
            }

            if (!row.IsScrapPercentManualUpdateOnlyNull())
            {
                ProductionInfoFlags.PlanningScrapPercent = row.ScrapPercentManualUpdateOnly;
            }

            if (!row.IsReportedStartOfProcessingDateNull() && row.ReportedStartOfProcessingDate != PTDateTime.MinValue.ToDateTime())
            {
                ReportedStartOfProcessingDate = row.ReportedStartOfProcessingDate.ToServerTime();
            }

            if (!row.IsReportedEndOfProcessingDateNull() && row.ReportedEndOfProcessingDate != PTDateTime.MinDateTime)
            {
                ReportedEndOfRunDate = row.ReportedEndOfProcessingDate.ToServerTime();
            }

            if (!row.IsBatchAmountNull())
            {
                BatchAmount = row.BatchAmount;
            }

            if (!row.IsActualResourcesUsedNull())
            {
                ActualResourcesUsedCSV = row.ActualResourcesUsed;
            }

            if (!row.IsReportedStorageHrsNull())
            {
                ReportedStorageTicks = TimeSpan.FromHours(row.ReportedStorageHrs).Ticks;
            }

            if (!row.IsReportedCleanHrsNull())
            {
                ReportedCleanSpan = TimeSpan.FromHours(row.ReportedCleanHrs);
            }

            if (!row.IsReportedCleanoutGradeNull())
            {
                ReportedCleanoutGrade = row.ReportedCleanoutGrade;
            }

            if (!row.IsReportedEndOfStorageDateNull())
            {
                ReportedEndOfStorageDate = row.ReportedEndOfStorageDate.ToServerTime();
            }

            if (!row.IsActivityManualUpdateOnlyNull())
            {
                ActivityManualUpdateOnly = row.ActivityManualUpdateOnly;
            }

            if (!row.IsReportedEndOfPostProcessingDateNull())
            {
                ReportedEndOfPostProcessingDate = row.ReportedEndOfPostProcessingDate.ToServerTime();
            }

            if (!row.IsCycleSpanOverrideNull())
            {
                ProductionInfoFlags.CycleSpanOverride = row.CycleSpanOverride;
            }

            if (!row.IsQtyPerCycleOverrideNull())
            {
                ProductionInfoFlags.QtyPerCycleOverride = row.QtyPerCycleOverride;
            }

            if (!row.IsSetupTimeOverrideNull())
            {
                ProductionInfoFlags.SetupSpanOverride = row.SetupTimeOverride;
            }

            if (!row.IsPostProcessOverrideNull())
            {
                ProductionInfoFlags.PostProcessingSpanOverride = row.PostProcessOverride;
            }

            if (!row.IsCleanTimeOverrideNull())
            {
                ProductionInfoFlags.CleanSpanOverride = row.CleanTimeOverride;
            }

            if (!row.IsScrapPercentOverrideNull())
            {
                ProductionInfoFlags.PlanningScrapPercentOverride = row.ScrapPercentOverride;
            }

            if (!row.IsStorageHrsOverrideNull())
            {
                ProductionInfoFlags.StorageSpanOverride = row.StorageHrsOverride;
            }

            if (!row.IsProductionSetupCostNull())
            {
                ProductionSetupCost = row.ProductionSetupCost;
            }

            if (!row.IsCleanoutCostNull())
            {
                CleanoutCost = row.CleanoutCost;
            }
        }

        #region Bools & Setter flags
        private BoolVector32 m_bools;

        // Bool array indecies
        private const int reportedEndOfRunDateSetIdx = 0;
        private const int pausedSetIdx = 1;
        private const int PeopleUsageSetIdx = 2;
        private const int NbrOfPeopleSetIdx = 3;
        private const int CommentsSetIdx = 4;
        private const int productionStatusSetIdx = 5;
        private const int reportedGoodQtySetIdx = 6;
        private const int reportedRunSpanSetIdx = 7;
        private const int reportedScrapQtySetIdx = 8;
        private const int reportedSetupSpanSetIdx = 9;
        private const int reportedPostProcessingSpanSetIdx = 10;

        private const int c_cycleSpanSetIdx = 11;
        private const int c_qtyPerCycleSetIdx = 12;
        private const int c_setupSpanSetIdx = 13;
        private const int c_postProcessingSpanSetIdx = 14;
        private const int c_planningScrapPercentSetIdx = 15;
        private const int c_endOfStoragePostProcessingTimeSpanSetIdx = 16;
        private const int c_reportedStartOfProcessingDateIdx = 17;
        private const int Comments2SetIdx = 18;

        private const int c_storageSpanSetIdx = 19;
        private const int c_usePostProcessingSpanOverrideIdx = 20;
        private const int c_useCipSpanOverrideIdx = 21;

        private const int c_setupSpanOverrideSetIdx = 22;
        private const int c_useSetupSpanOverrideSetIdx = 23;
        private const int c_postProcessingSpanOverrideSetIdx = 24;
        private const int c_usePostProcessingSpanOverrideSetIdx = 25;
        private const int c_cipSpanOverrideSetIdx = 26;
        private const int c_useCipSpanOverrideSetIdx = 27;
        private const int c_cleanSpanSetIdx = 28;
        private const int c_cleanOutGradeIsSetIdx = 29;

        private const int c_reportedCleanIsSetIdx = 30;
        private const int c_reportedCleanGradeIsSetIdx = 31;

        #endregion

        #region Bool Vector 2
        internal BoolVector32 m_bools2;

        private const int c_reportedEndOfPostProcessingSetIdx = 0;
        private const int c_reportedEndOfStorageSetIdx = 1;
        private const int c_productionSetupCostIsSetIdx = 2;
        private const int c_cleanoutCostIsSetIdx = 3;
        private const int c_activityManualMoveOnlyIsSetIdx = 4;
        private const int c_activityManualMoveOnlyIdx = 5;

        #endregion
        #region Shared Properties
        private InternalActivityDefs.productionStatuses productionStatus;

        [Required(true)]
        /// <summary>
        /// Indicates the current state of the Activity in production. Setting this to Finished prevents it from scheduling.  Values are: Finished, Waiting, Ready, SettingUp, Running, PostProcessing, or Transferring.
        /// </summary>
        public InternalActivityDefs.productionStatuses ProductionStatus
        {
            get => productionStatus;
            set
            {
                productionStatus = value;
                ProductionStatusIsSet = true;
            }
        }

        public bool ProductionStatusIsSet
        {
            get => m_bools[productionStatusSetIdx];
            set => m_bools[productionStatusSetIdx] = value;
        }

        private decimal reportedGoodQty;

        public decimal ReportedGoodQty
        {
            get => reportedGoodQty;
            set
            {
                reportedGoodQty = value;
                ReportedGoodQtyIsSet = true;
            }
        }

        public bool ReportedGoodQtyIsSet
        {
            get => m_bools[reportedGoodQtySetIdx];
            set => m_bools[reportedGoodQtySetIdx] = value;
        }

        private TimeSpan reportedRunSpan;

        public TimeSpan ReportedRunSpan
        {
            get => reportedRunSpan;
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new PTValidationException("2982", new object[] { ExternalId });
                }

                reportedRunSpan = value;
                ReportedRunSpanIsSet = true;
            }
        }

        public bool ReportedRunSpanIsSet
        {
            get => m_bools[reportedRunSpanSetIdx];
            set => m_bools[reportedRunSpanSetIdx] = value;
        }

        public bool CycleSpanSet
        {
            get => m_bools[c_cycleSpanSetIdx];
            private set => m_bools[c_cycleSpanSetIdx] = value;
        }

        public bool QtyPerCycleSet
        {
            get => m_bools[c_qtyPerCycleSetIdx];
            private set => m_bools[c_qtyPerCycleSetIdx] = value;
        }

        public bool SetupSpanSet
        {
            get => m_bools[c_setupSpanSetIdx];
            private set => m_bools[c_setupSpanSetIdx] = value;
        }

        public bool PostProcessingSpanSet
        {
            get => m_bools[c_postProcessingSpanSetIdx];
            private set => m_bools[c_postProcessingSpanSetIdx] = value;
        } 
        public bool StorageSpanSet
        {
            get => m_bools[c_storageSpanSetIdx];
            private set => m_bools[c_storageSpanSetIdx] = value;
        }

        public bool CleanSpanSet
        {
            get => m_bools[c_cleanSpanSetIdx];
            private set => m_bools[c_cleanSpanSetIdx] = value;
        }

        public bool CleanOutGradeIsSet
        {
            get => m_bools[c_cleanOutGradeIsSetIdx];
            private set => m_bools[c_cleanOutGradeIsSetIdx] = value;
        }

        public bool PlanningScrapPercentSet
        {
            get => m_bools[c_planningScrapPercentSetIdx];
            private set => m_bools[c_planningScrapPercentSetIdx] = value;
        }

        public bool EndOfStoragePostProcessingTimeSpanSet
        {
            get => m_bools[c_endOfStoragePostProcessingTimeSpanSetIdx];
            private set => m_bools[c_endOfStoragePostProcessingTimeSpanSetIdx] = value;
        }

        private DateTime reportedEndOfRunDate;

        /// <summary>
        /// If the activity has some run time scheduled then this is the time it is scheduled to end. If the activity is in the post-processing state or is finished then this is the time run was scheduled to
        /// finish or is the time run was reported to be finished.
        /// </summary>
        public DateTime ReportedEndOfRunDate
        {
            get => reportedEndOfRunDate;

            set
            {
                reportedEndOfRunDate = value;
                ReportedEndOfRunDateSet = true;
            }
        }

        public bool ReportedEndOfRunDateSet
        {
            get => m_bools[reportedEndOfRunDateSetIdx];

            set => m_bools[reportedEndOfRunDateSetIdx] = value;
        }
        
        private DateTime m_reportedEndOfPostProcessingDate;

        public DateTime ReportedEndOfPostProcessingDate
        {
            get => m_reportedEndOfPostProcessingDate;

            set
            {
                m_reportedEndOfPostProcessingDate = value;
                ReportedEndOfPostProcessingDateSet = true;
            }
        }

        public bool ReportedEndOfPostProcessingDateSet
        {
            get => m_bools[c_reportedEndOfPostProcessingSetIdx];

            set => m_bools[c_reportedEndOfPostProcessingSetIdx] = value;
        }
        private DateTime m_reportedEndOfStorageDate;

        public DateTime ReportedEndOfStorageDate
        {
            get => m_reportedEndOfStorageDate;

            set
            {
                m_reportedEndOfStorageDate = value;
                ReportedEndOfStorageDateSet = true;
            }
        }

        public bool ReportedEndOfStorageDateSet
        {
            get => m_bools[c_reportedEndOfStorageSetIdx];

            set => m_bools[c_reportedEndOfStorageSetIdx] = value;
        }

        private decimal reportedScrapQty;

        /// <summary>
        /// Quantity of scrapped product reported to have been finished.  For display only.
        /// </summary>
        public decimal ReportedScrapQty
        {
            get => reportedScrapQty;
            set
            {
                reportedScrapQty = value;
                ReportedScrapQtyIsSet = true;
            }
        }

        public bool ReportedScrapQtyIsSet
        {
            get => m_bools[reportedScrapQtySetIdx];
            set => m_bools[reportedScrapQtySetIdx] = value;
        }

        private TimeSpan reportedSetupSpan;

        public TimeSpan ReportedSetupSpan
        {
            get => reportedSetupSpan;
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new PTValidationException("2981", new object[] { ExternalId });
                }

                reportedSetupSpan = value;
                ReportedSetupSpanIsSet = true;
            }
        }

        public bool ReportedSetupSpanIsSet
        {
            get => m_bools[reportedSetupSpanSetIdx];
            set => m_bools[reportedSetupSpanSetIdx] = value;
        }

        private TimeSpan reportedPostProcessingSpan;

        /// <summary>
        /// PostProcessing time reported to have been spent so far. For display only.
        /// </summary>
        public TimeSpan ReportedPostProcessingSpan
        {
            get => reportedPostProcessingSpan;
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new PTValidationException("2983", new object[] { ExternalId });
                }

                reportedPostProcessingSpan = value;
                ReportedPostProcessingSpanIsSet = true;
            }
        }

        public bool ReportedPostProcessingSpanIsSet
        {
            get => m_bools[reportedPostProcessingSpanSetIdx];
            set => m_bools[reportedPostProcessingSpanSetIdx] = value;
        }

        private DateTime m_reportedStartOfProcessingDate;

        public DateTime ReportedStartOfProcessingDate
        {
            get => m_reportedStartOfProcessingDate;
            set
            {
                m_reportedStartOfProcessingDate = value;
                m_bools[c_reportedStartOfProcessingDateIdx] = true;
            }
        }

        public bool ReportedStartOfProcessingDateSet => m_bools[c_reportedStartOfProcessingDateIdx];

        private long m_reportedStorageTicks;
        /// <summary>
        /// Storage time reported to have been spent so far in ticks.
        /// </summary>
        public long ReportedStorageTicks
        {
            get => m_reportedStorageTicks;

            internal set
            {
                if (m_reportedStorageTicks != value)
                {
                    m_reportedStorageTicks = value;
                    m_bools[c_reportedCleanIsSetIdx] = true;
                }
            }
        }

        private TimeSpan m_reportedCleanSpan;
        /// <summary>
        /// Clean time reported to have been spent so far in ticks.
        /// </summary>
        public TimeSpan ReportedCleanSpan
        {
            get => m_reportedCleanSpan;

            internal set
            {
                if (m_reportedCleanSpan != value)
                {
                    m_reportedCleanSpan = value;
                    m_bools[c_reportedCleanIsSetIdx] = true;
                }
            }
        }

        public bool ReportedCleanIsSet => m_bools[c_reportedCleanIsSetIdx];

        private int m_reportedCleanOutGrade;
        public int ReportedCleanoutGrade
        {
            get => m_reportedCleanOutGrade;
            private set
            {
                m_reportedCleanOutGrade = value;
                m_bools[c_reportedCleanGradeIsSetIdx] = true;
            }
        }

        public bool ReportedCleanGradeIsSet => m_bools[c_reportedCleanGradeIsSetIdx];

        public bool ActivityManualUpdateOnlyIsSet => m_bools2[c_activityManualMoveOnlyIsSetIdx];

        public bool ActivityManualUpdateOnly
        {
            get => m_bools2[c_activityManualMoveOnlyIdx];
            set
            {
                m_bools2[c_activityManualMoveOnlyIdx] = value;
                m_bools2[c_activityManualMoveOnlyIsSetIdx] = true;
            }
        }

        private bool paused;

        /// <summary>
        /// Indicates that the current setup or run process has been temporarily suspended due to something like an operator break, end of shift, etc.
        /// Pausing the Activity does not necessarily mean it will be reschedulable.  That depends upon the Activity's Production Status and Hold status.
        /// This is primarily a visual indicator that the Activity is not currently being worked on.
        /// </summary>
        public bool Paused
        {
            get => paused;
            set
            {
                paused = value;
                m_bools[pausedSetIdx] = true;
            }
        }

        public bool PausedIsSet => m_bools[pausedSetIdx];

        private InternalActivityDefs.peopleUsages peopleUsage = InternalActivityDefs.peopleUsages.UseAllAvailable;

        /// <summary>
        /// Determines how many people are allocated to an Activity in the schedule.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
        public InternalActivityDefs.peopleUsages PeopleUsage
        {
            get => peopleUsage;
            set
            {
                peopleUsage = value;
                m_bools[PeopleUsageSetIdx] = true;
            }
        }

        public bool PeopleUsageIsSet => m_bools[PeopleUsageSetIdx];

        private decimal nbrOfPeople = 1;

        /// <summary>
        /// If PeopleUsage is set to UseSpecifiedNbr then this is the maximum number of people that will be allocated to the Activity.
        /// Fewer than this number will be allocated during time periods over which the Primary Resource's Nbr Of People is less than this value.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)] //IN THE FUTURE, MAKE EDITABLE FOR WHATIF JOBS.
        public decimal NbrOfPeople
        {
            get => nbrOfPeople;
            set
            {
                if (value <= 0)
                {
                    throw new PTValidationException("2050");
                }

                nbrOfPeople = value;
                m_bools[NbrOfPeopleSetIdx] = true;
            }
        }

        public bool NbrOfPeopleIsSet => m_bools[NbrOfPeopleSetIdx];

        private string comments;

        /// <summary>
        /// Comments about the work being performed, usually entered by planners or operators.
        /// </summary>
        public string Comments
        {
            get => comments;
            set
            {
                comments = value;
                m_bools[CommentsSetIdx] = true;
            }
        }

        public bool CommentsIsSet => m_bools[CommentsSetIdx];

        private string comments2;

        /// <summary>
        /// Comments about the work being performed, usually entered by planners or operators.
        /// </summary>
        public string Comments2
        {
            get => comments2;
            set
            {
                comments2 = value;
                m_bools[Comments2SetIdx] = true;
            }
        }

        public bool Comments2IsSet => m_bools[Comments2SetIdx];

        private TimeSpan setupSpan = new(0);

        /// <summary>
        /// Time for setting up each Resource that is used during the Operation's setup time.
        /// </summary>
        public TimeSpan SetupSpan
        {
            get => setupSpan;
            set
            {
                setupSpan = value;
                SetupSpanSet = true;
            }
        }

        private TimeSpan cycleSpan = new(0);

        /// <summary>
        /// Time to perform one production cycle.
        /// </summary>
        public TimeSpan CycleSpan
        {
            get => cycleSpan;
            set
            {
                cycleSpan = value;
                CycleSpanSet = true;
            }
        }

        private decimal qtyPerCycle;

        public decimal QtyPerCycle
        {
            get => qtyPerCycle;
            set
            {
                qtyPerCycle = value;
                QtyPerCycleSet = true;
            }
        }

        private TimeSpan postProcessingSpan = new(0);

        /// <summary>
        /// Specifies a TimeSpan for which product must wait before they are considered complete at the operation.  The resources may or may not be occupied during this period (as specified in
        /// postProcessingConsumesResource).
        /// </summary>
        public TimeSpan PostProcessingSpan
        {
            get => postProcessingSpan;
            set
            {
                postProcessingSpan = value;
                PostProcessingSpanSet = true;
            }
        }

        private TimeSpan m_cleanSpan = new(0);

        /// <summary>
        /// Specifies a TimeSpan for which product must wait before they are considered complete at the operation.  The resources may or may not be occupied during this period (as specified in
        /// postProcessingConsumesResource).
        /// </summary>
        public TimeSpan CleanSpan
        {
            get => m_cleanSpan;
            set
            {
                m_cleanSpan = value;
                CleanSpanSet = true;
            }
        }

        private int m_cleanOutGrade;

        public int CleanOutGrade
        {
            get => m_cleanOutGrade;
            set
            {
                m_cleanOutGrade = value;
                CleanOutGradeIsSet = true;
            }
        }
        private TimeSpan m_storageSpan = new(0);

        /// <summary>
        /// Specifies a TimeSpan for which product must wait before they are considered complete at the operation.  The resources may or may not be occupied during this period (as specified in
        /// postProcessingConsumesResource).
        /// </summary>
        public TimeSpan StorageSpan
        {
            get => m_storageSpan;
            set
            {
                m_storageSpan = value;
                StorageSpanSet = true;
            }
        }
        private TimeSpan m_endOfStoragePostProcessing;

        public TimeSpan EndOfStoragePostProcessingTimeSpan
        {
            get => m_endOfStoragePostProcessing;
            set
            {
                m_endOfStoragePostProcessing = value;
                EndOfStoragePostProcessingTimeSpanSet = true;
            }
        }

        private decimal planningScrapPercent;

        /// <summary>
        /// Percent of parts expected to be scrapped.  Used to calculate Expected Good Qty and Exptected Scrap Qty.
        /// </summary>
        public decimal PlanningScrapPercent
        {
            get => planningScrapPercent;
            set
            {
                planningScrapPercent = value;
                PlanningScrapPercentSet = true;
            }
        }

        private string m_actualResourcesUsedCsv;

        /// <summary>
        /// For Finished Activities these are the Resources that were if any.  Null if finished while unscheduled.
        /// </summary>
        public string ActualResourcesUsedCSV
        {
            get => m_actualResourcesUsedCsv;
            internal set => m_actualResourcesUsedCsv = value;
        }

        private decimal m_productionSetupCost = 0;
        public decimal ProductionSetupCost
        {
            get { return m_productionSetupCost; }
            set
            {
                m_productionSetupCost = value;
                m_bools2[c_productionSetupCostIsSetIdx] = true;
            }
        }

        public bool ProductionSetupCostIsSet => m_bools2[c_productionSetupCostIsSetIdx];

        private decimal m_cleanoutCost = 0;
        public decimal CleanoutCost
        {
            get { return m_cleanoutCost; }
            set
            {
                m_cleanoutCost = value;
                m_bools2[c_cleanoutCostIsSetIdx] = true;
            }
        }

        public bool CleanoutCostIsSet => m_bools2[c_cleanoutCostIsSetIdx];
        #endregion Shared Properties

        private readonly ProductionInfoFlags _productionInfoFlags = new();

        public ProductionInfoFlags ProductionInfoFlags => _productionInfoFlags;

        public override void Validate()
        {
            base.Validate();

            if (ReportedGoodQty < 0)
            {
                throw new ValidationException("3020", new object[] { ExternalId });
            }

            if (ReportedFinishDate > PTDateTime.MinDateTime && ReportedStartDateTicks > ReportedFinishDateTicks)
            {
                ReportedFinishDate = ReportedStartDate;
            }
        }

        public class DuplicateInternalActivityException : ValidationException
        {
            public DuplicateInternalActivityException(string msg, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
                : base(msg, a_stringParameters, a_appendHelpUrl) { }
        }

        /// <summary>
        /// The time this object was created. Used for updating finish date when it isn't provided
        /// </summary>
        public DateTime NowFinishUtcTime => m_nowFinishUtcTime;
        
        private readonly DateTime m_nowFinishUtcTime;
    }

    public class SubcontractorActivity : BaseActivity, IPTSerializable
    {
        public new const int UNIQUE_ID = 248;

        #region PT Serialization
        public SubcontractorActivity(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int val;
                reader.Read(out val);
                status = (SubcontractorActivityDefs.statuses)val;

                reader.Read(out scheduledReceiptDate);
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)status);
            writer.Write(scheduledReceiptDate);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region Shared Properties
        private SubcontractorActivityDefs.statuses status = SubcontractorActivityDefs.statuses.NotSent;

        /// <summary>
        /// Indicates whether the material has already been sent to the contractor.  If the material has not yet been sent then the leadTime should be used.  After the material has been sent then the
        /// expectedReceiptDate should be used until it's received.
        /// </summary>
        public SubcontractorActivityDefs.statuses Status
        {
            get => status;
            set => status = value;
        }

        private DateTime scheduledReceiptDate;

        /// <summary>
        /// If specified, this overrides the LeadTimeSpan to determine when the material will be available.
        /// </summary>
        public DateTime ScheduledReceiptDate
        {
            get => scheduledReceiptDate;
            set => scheduledReceiptDate = value;
        }
        #endregion

        public SubcontractorActivity(string externalId)
            : base(externalId) { }
    }

    public class BatchProcessorOperation : InternalOperation
    {
        public BatchProcessorOperation(string externalId, string name, decimal requiredStartQty, TimeSpan cycleSpan, decimal qtyPerContainer) :
            base(externalId, name, requiredStartQty, cycleSpan)
        {
            QtyPerContainer = qtyPerContainer;
            ValidateQtyPerContainer();
        }

        #region Shared Properties
        private string batchingCode = "";

        /// <summary>
        /// Only Operations with the same BatchingCode are allowed to be run together in the same batch.
        /// </summary>
        public string BatchingCode
        {
            get => batchingCode;
            set => batchingCode = value;
        }

        private TimeSpan loadSpanPerContainer = new(0);

        /// <summary>
        /// The timespan it takes to load one container of product into the resource.   The resource is considered busy during this period.
        /// </summary>
        public TimeSpan LoadSpanPerContainer
        {
            get => loadSpanPerContainer;
            set => loadSpanPerContainer = value;
        }

        private decimal qtyPerContainer = 1;

        /// <summary>
        /// The amount of parts that fit in each of the containers used for processing.
        /// </summary>
        public decimal QtyPerContainer
        {
            get => qtyPerContainer;
            set => qtyPerContainer = value;
        }

        private bool splitOperationsToFillBatches;

        /// <summary>
        /// If true then if a batch is not full and a compatible Operation is ready but will not fit then process whatever will fit and keep the rest for a subsequent batch.	If false, then only add an Operation
        /// to a partially filled batch if it can entirely fit.  Note that an Operation will still be split into multiple batches if it is too many containers to fit entirely in its own batch.
        /// </summary>
        public bool SplitOperationsToFillBatches
        {
            get => splitOperationsToFillBatches;
            set => splitOperationsToFillBatches = value;
        }

        private TimeSpan unloadSpanPerContainer = new(0);

        /// <summary>
        /// The amount of time it takes to unload one container of product from the resource. The resource is considered busy during this period.
        /// </summary>
        public TimeSpan UnloadSpanPerContainer
        {
            get => unloadSpanPerContainer;
            set => unloadSpanPerContainer = value;
        }
        #endregion Shared Properties

        private void ValidateQtyPerContainer()
        {
            if (QtyPerContainer <= 0)
            {
                throw new ValidationException("2093", new object[] { ExternalId });
            }
        }

        public override void Validate()
        {
            base.Validate();
            ValidateQtyPerContainer();
        }
    }

    public class SubcontractorOperation : BaseOperation, IPTSerializable
    {
        public new const int UNIQUE_ID = 249;

        #region PT Serialization
        public SubcontractorOperation(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 1)
            {
                int val;
                reader.Read(out val);
                status = (SubcontractorActivityDefs.statuses)val;

                reader.Read(out leadTimeSpan);
                reader.Read(out scheduledReceiptDate);
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)status);

            writer.Write(leadTimeSpan);
            writer.Write(scheduledReceiptDate);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public SubcontractorOperation(string externalId, string name, decimal requiredStartQty, string subcontractorExternalId, TimeSpan leadTimeSpan)
            : base(externalId, name, requiredStartQty)
        {
            LeadTimeSpan = leadTimeSpan;
            this.subcontractorExternalId = subcontractorExternalId;
            ValidateLeadTimeSpan();
        }

        private readonly string subcontractorExternalId;

        public string SubcontractorExternalId => subcontractorExternalId;

        #region Shared Properties
        private TimeSpan leadTimeSpan = new(0);

        /// <summary>
        /// leadTimeSpan
        /// </summary>
        public TimeSpan LeadTimeSpan
        {
            get => leadTimeSpan;
            set => leadTimeSpan = value;
        }
        #endregion Shared Properties

        private SubcontractorActivityDefs.statuses status = SubcontractorActivityDefs.statuses.NotSent;

        /// <summary>
        /// Specifies the status related information for a Subcontractor Operation.
        /// </summary>
        public SubcontractorActivityDefs.statuses Status
        {
            get => status;
            set => status = value;
        }

        private DateTime scheduledReceiptDate = PTDateTime.UtcNow.RemoveSeconds().DateTime;

        /// <summary>
        /// If specified, this overrides the LeadTimeSpan to determine when the material will be available.
        /// </summary>
        private DateTime ScheduledReceiptDate
        {
            get => scheduledReceiptDate;
            set => scheduledReceiptDate = value;
        }

        private void ValidateLeadTimeSpan()
        {
            if (LeadTimeSpan.Ticks <= 0)
            {
                throw new ValidationException("2094", new object[] { ExternalId });
            }
        }

        private void ValidateSubcontractorExternalId()
        {
            if (SubcontractorExternalId.Length == 0)
            {
                throw new ValidationException("2095", new object[] { ExternalId });
            }
        }

        public override void Validate()
        {
            base.Validate();
            ValidateLeadTimeSpan();
        }
    }

    public class MaterialRequirement : PTObjectIdBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 247;

        #region PT Serialization
        public MaterialRequirement(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12524)
            {
                a_reader.Read(out int val);
                constraintType = (MaterialRequirementDefs.constraintTypes)val;
                a_reader.Read(out issuedQty);
                a_reader.Read(out materialDescription);
                a_reader.Read(out materialName);
                a_reader.Read(out source);
                a_reader.Read(out totalCost);
                a_reader.Read(out totalRequiredQty);
                a_reader.Read(out uom);
                a_reader.Read(out availableDateTime);
                a_reader.Read(out leadTimeSpan);
                a_reader.Read(out itemExternalId);
                a_reader.Read(out val);
                requirementType = (MaterialRequirementDefs.requirementTypes)val;
                a_reader.Read(out warehouseExternalId);
                a_reader.Read(out warehouseExternalIdSet);
                a_reader.Read(out val);
                m_tankStorageReleaseTiming = (MaterialRequirementDefs.TankStorageReleaseTimingEnum)val;
                a_reader.Read(out val);
                m_materialUsedTiming = (MaterialRequirementDefs.EMaterialUsedTiming)val;
                a_reader.Read(out m_lotAllocationRule);

                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_plannedScrapQty);
                a_reader.Read(out val);
                m_allowedLotCodes = new List<string>();
                for (int i = 0; i < val; i++)
                {
                    string lotCode;
                    a_reader.Read(out lotCode);
                    m_allowedLotCodes.Add(lotCode);
                }

                a_reader.Read(out m_minAge);
                a_reader.Read(out m_minRemainingShelfLife);
                a_reader.Read(out m_maxEligibleWearAmount);
                a_reader.Read(out m_productRelease);

                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                a_reader.Read(out m_minSourceQty);
                a_reader.Read(out m_maxSourceQty);

                a_reader.Read(out val);
                m_materialSourcing = (ItemDefs.MaterialSourcing)val;

                a_reader.Read(out m_storageAreaExternalIdIsSet);
                a_reader.Read(out m_storageAreaExternalId);
            }
            else if (a_reader.VersionNumber >= 12511)
            {
                a_reader.Read(out int val);
                constraintType = (MaterialRequirementDefs.constraintTypes)val;
                a_reader.Read(out issuedQty);
                a_reader.Read(out materialDescription);
                a_reader.Read(out materialName);
                a_reader.Read(out source);
                a_reader.Read(out totalCost);
                a_reader.Read(out totalRequiredQty);
                a_reader.Read(out uom);
                a_reader.Read(out availableDateTime);
                a_reader.Read(out leadTimeSpan);
                a_reader.Read(out itemExternalId);
                a_reader.Read(out val);
                requirementType = (MaterialRequirementDefs.requirementTypes)val;
                a_reader.Read(out warehouseExternalId);
                a_reader.Read(out warehouseExternalIdSet);
                a_reader.Read(out val);
                m_tankStorageReleaseTiming = (MaterialRequirementDefs.TankStorageReleaseTimingEnum)val;
                a_reader.Read(out m_lotAllocationRule);

                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_plannedScrapQty);
                a_reader.Read(out val);
                m_allowedLotCodes = new List<string>();
                for (int i = 0; i < val; i++)
                {
                    string lotCode;
                    a_reader.Read(out lotCode);
                    m_allowedLotCodes.Add(lotCode);
                }

                a_reader.Read(out m_minAge);
                a_reader.Read(out m_minRemainingShelfLife);
                a_reader.Read(out m_maxEligibleWearAmount);
                a_reader.Read(out m_productRelease);

                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                a_reader.Read(out m_minSourceQty);
                a_reader.Read(out m_maxSourceQty);

                a_reader.Read(out val);
                m_materialSourcing = (ItemDefs.MaterialSourcing)val;

                a_reader.Read(out m_storageAreaExternalIdIsSet);
                a_reader.Read(out m_storageAreaExternalId);
            }
            #region 729
            else if (a_reader.VersionNumber >= 731)
            {
                a_reader.Read(out int val);
                constraintType = (MaterialRequirementDefs.constraintTypes)val;
                a_reader.Read(out issuedQty);
                a_reader.Read(out materialDescription);
                a_reader.Read(out materialName);
                a_reader.Read(out source);
                a_reader.Read(out totalCost);
                a_reader.Read(out totalRequiredQty);
                a_reader.Read(out uom);
                a_reader.Read(out availableDateTime);
                a_reader.Read(out leadTimeSpan);
                a_reader.Read(out itemExternalId);
                a_reader.Read(out val);
                requirementType = (MaterialRequirementDefs.requirementTypes)val;
                a_reader.Read(out warehouseExternalId);
                a_reader.Read(out warehouseExternalIdSet);
                a_reader.Read(out val);
                m_tankStorageReleaseTiming = (MaterialRequirementDefs.TankStorageReleaseTimingEnum)val;
                a_reader.Read(out m_lotAllocationRule);

                a_reader.Read(out int lotUsabilityVal);

                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_plannedScrapQty);
                a_reader.Read(out val);
                m_allowedLotCodes = new List<string>();
                for (int i = 0; i < val; i++)
                {
                    string lotCode;
                    a_reader.Read(out lotCode);
                    m_allowedLotCodes.Add(lotCode);
                }

                a_reader.Read(out m_minAge);
                a_reader.Read(out m_minRemainingShelfLife);
                a_reader.Read(out m_maxEligibleWearAmount);
                a_reader.Read(out m_productRelease);

                a_reader.Read(out val);
                m_materialAllocation = (ItemDefs.MaterialAllocation)val;
                a_reader.Read(out m_minSourceQty);
                a_reader.Read(out m_maxSourceQty);

                a_reader.Read(out val);
                m_materialSourcing = (ItemDefs.MaterialSourcing)val;
            }
            #endregion
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)constraintType);
            writer.Write(issuedQty);
            writer.Write(materialDescription);
            writer.Write(materialName);
            writer.Write(source);
            writer.Write(totalCost);
            writer.Write(totalRequiredQty);
            writer.Write(uom);
            writer.Write(availableDateTime);
            writer.Write(leadTimeSpan);
            writer.Write(itemExternalId);
            writer.Write((int)requirementType);
            writer.Write(warehouseExternalId);
            writer.Write(warehouseExternalIdSet);
            writer.Write((int)m_tankStorageReleaseTiming);
            writer.Write((int)m_materialUsedTiming);
            writer.Write(LotAllocationRule);
            m_bools.Serialize(writer);
            writer.Write(m_plannedScrapQty);
            writer.Write(m_allowedLotCodes.Count);
            foreach (string allowedLotCode in m_allowedLotCodes)
            {
                writer.Write(allowedLotCode);
            }

            writer.Write(m_minAge);
            writer.Write(m_minRemainingShelfLife);
            writer.Write(m_maxEligibleWearAmount);
            writer.Write(m_productRelease);
            writer.Write((int)m_materialAllocation);
            writer.Write(m_minSourceQty);
            writer.Write(m_maxSourceQty);
            writer.Write((int)m_materialSourcing);
            writer.Write(m_storageAreaExternalIdIsSet);
            writer.Write(m_storageAreaExternalId);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region BoolVector32
        private BoolVector32 m_bools;
        //private const int c_useOverlapActivitiesIdx = 0; Obsolete
        private const int c_useOverlapPOsIdx = 1;
        private const int c_multipleWarehouseSupplyAllowedIdx = 2;
        private const int c_fixedQty = 3;
        private const int c_allowPartialSupplyIdx = 4;
        private const int c_allowedLotCodesSet = 5;
        private const int c_multipleStorageAreaSupplyAllowedIdx = 6;
        private const int c_requireFirstTransferAtSetupIdx = 7;
        private const int c_allowExpiredSupplyIdx = 8;
        #endregion

        public MaterialRequirement()
        {
            //Set defaults
            AllowPartialSupply = true;
        }

        public MaterialRequirement(string externalId, string materialName, DateTime availableDateTime, TimeSpan leadTimeSpan, decimal totalRequiredQty)
            : base(externalId)
        {
            //Set defaults
            AllowPartialSupply = true;
            MaterialName = materialName;
            AvailableDateTime = availableDateTime;
            LeadTimeSpan = leadTimeSpan;
            requirementType = MaterialRequirementDefs.requirementTypes.BuyDirect; //no item set
            this.totalRequiredQty = totalRequiredQty;
        }

        /// <summary>
        /// Creates a Material Requirement that is planned automatically using the Material Planner feature.
        /// </summary>
        /// <param name="externalId">Identifies the Material Requirement.</param>
        /// <param name="itemExternalId">Identifies the Item whose Inventory will be consumed to satisfy the requirement.</param>
        /// <param name="totalRequiredQty"></param>
        public MaterialRequirement(string externalId, string itemExternalId, decimal totalRequiredQty)
            : base(externalId)
        {
            //Set defaults
            AllowPartialSupply = true;
            this.itemExternalId = itemExternalId;
            requirementType = MaterialRequirementDefs.requirementTypes.FromStock;
            this.totalRequiredQty = totalRequiredQty;
        }

        public MaterialRequirement(JobDataSet.MaterialRequirementRow row)
            : base(row.ExternalId)
        {
            //Set defaults
            AllowPartialSupply = true;

            if (!row.IsConstraintTypeNull())
            {
                try
                {
                    ConstraintType = (MaterialRequirementDefs.constraintTypes)Enum.Parse(typeof(MaterialRequirementDefs.constraintTypes), row.ConstraintType);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            row.ConstraintType, "MaterialRequirement", "ConstraintType",
                            string.Join(", ", Enum.GetNames(typeof(MaterialRequirementDefs.constraintTypes)))
                        });
                }
            }

            if (!row.IsIssuedQtyNull())
            {
                IssuedQty = row.IssuedQty;
            }

            if (!row.IsTotalCostNull())
            {
                TotalCost = row.TotalCost;
            }

            if (!row.IsMaterialAllocationNull())
            {
                MaterialAllocation = (ItemDefs.MaterialAllocation)Enum.Parse(typeof(ItemDefs.MaterialAllocation), row.MaterialAllocation);
            }

            if (!row.IsMinSourceQtyNull())
            {
                MinSourceQty = row.MinSourceQty;
            }

            if (!row.IsMaxSourceQtyNull())
            {
                MaxSourceQty = row.MaxSourceQty;
            }

            if (!row.IsMaterialSourcingNull())
            {
                MaterialSourcing = (ItemDefs.MaterialSourcing)Enum.Parse(typeof(ItemDefs.MaterialSourcing), row.MaterialSourcing);
            }

            if (!row.IsTotalRequiredQtyNull())
            {
                TotalRequiredQty = row.TotalRequiredQty;
            }

            if (!row.IsFixedQtyNull())
            {
                FixedQty = row.FixedQty;
            }

            if (!row.IsUOMNull())
            {
                UOM = row.UOM;
            }

            if (!row.IsPlannedScrapQtyNull())
            {
                PlannedScrapQty = row.PlannedScrapQty;
            }

            if (!row.IsRequirementTypeNull())
            {
                try
                {
                    RequirementType = (MaterialRequirementDefs.requirementTypes)Enum.Parse(typeof(MaterialRequirementDefs.requirementTypes), row.RequirementType);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            row.RequirementType, "MaterialRequirement", "RequirementType",
                            string.Join(", ", Enum.GetNames(typeof(MaterialRequirementDefs.requirementTypes)))
                        });
                }
            }
            else
            {
                RequirementType = MaterialRequirementDefs.requirementTypes.FromStock; // default
            }

            if (RequirementType == MaterialRequirementDefs.requirementTypes.BuyDirect)
            {
                if (!row.IsLatestSourceDateTimeNull())
                {
                    AvailableDateTime = row.LatestSourceDateTime.ToServerTime();
                }

                if (!row.IsLeadTimeHrsNull())
                {
                    LeadTimeSpan = PTDateTime.GetSafeTimeSpan(row.LeadTimeHrs);
                }

                if (!row.IsMaterialDescriptionNull())
                {
                    MaterialDescription = row.MaterialDescription;
                }

                if (!row.IsMaterialNameNull())
                {
                    MaterialName = row.MaterialName;
                }

                if (!row.IsSourceNull())
                {
                    Source = row.Source;
                }
            }
            else
            {
                if (!row.IsItemExternalIdNull())
                {
                    ItemExternalId = row.ItemExternalId;
                }
                else
                {
                    throw new PTValidationException("2860", new object[] { row.ExternalId });
                }

                if (!row.IsIssuedQtyNull())
                {
                    IssuedQty = row.IssuedQty;
                }

                if (!row.IsIssuedQtyNull())
                {
                    IssuedQty = row.IssuedQty;
                }

                if (!row.IsWarehouseExternalIdNull() && row.WarehouseExternalId.Length > 0) //optional, could be blank.
                {
                    WarehouseExternalId = row.WarehouseExternalId;
                }

                if (!row.IsUseOverlapPurchasesNull())
                {
                    UseOverlapPOs = row.UseOverlapPurchases;
                }

                if (!row.IsTankStorageReleaseTimingNull())
                {
                    try
                    {
                        TankStorageReleaseTiming = (MaterialRequirementDefs.TankStorageReleaseTimingEnum)Enum.Parse(typeof(MaterialRequirementDefs.TankStorageReleaseTimingEnum), row.TankStorageReleaseTiming);
                    }
                    catch (Exception err)
                    {
                        throw new PTValidationException("2854",
                            err,
                            false,
                            new object[]
                            {
                                row.TankStorageReleaseTiming, "MaterialRequirement", "TankStorageReleaseTiming",
                                string.Join(", ", Enum.GetNames(typeof(MaterialRequirementDefs.TankStorageReleaseTimingEnum)))
                            });
                    }
                }

                if (!row.IsMultipleWarehouseSupplyAllowedNull())
                {
                    MultipleWarehouseSupplyAllowed = row.MultipleWarehouseSupplyAllowed;
                }

                if (!row.IsAllowExpiredSupplyNull())
                {
                    AllowExpiredSupply = row.AllowExpiredSupply;
                }

                if (!row.IsMultipleStorageAreaSupplyAllowedNull())
                {
                    MultipleStorageAreaSupplyAllowed = row.MultipleStorageAreaSupplyAllowed;
                }

                if (!row.IsAllowPartialSupplyNull())
                {
                    AllowPartialSupply = row.AllowPartialSupply;
                }

                MinAge = row.IsMinAgeHrsNull() ? TimeSpan.Zero : TimeSpan.FromHours(row.MinAgeHrs);

                MinRemainingShelfLife = row.IsMinRemainingShelfLifeHrsNull() ? TimeSpan.Zero : TimeSpan.FromHours(row.MinRemainingShelfLifeHrs);

                if (!row.IsMaxEligibleWearAmountNull())
                {
                    MaxEligibleWearAmount = row.MaxEligibleWearAmount;
                }

                if (!row.IsProductReleaseNull())
                {
                    ProductRelease = row.ProductRelease;
                }

                //Might need some tweaking
                if ((!row.IsMaxEligibleWearAmountNull() || !row.IsProductReleaseNull()) && row.IsMinAgeHrsNull() && row.IsMinRemainingShelfLifeHrsNull())
                {
                    //TODO: hiding Wear Lot Usability for now
                    //LotUsability = ItemDefs.LotUsability.Wear;
                    throw new PTValidationException(string.Format("MR '{0}' for Item '{1}' on Job '{2}', MO '{3}', OP '{4}', could not be updated because LotUsability can't be set to 'Wear' as the Wear Lot Control feature is not currently enabled., row.Ex",
                        row.ExternalId,
                        row.ItemExternalId,
                        row.JobExternalId,
                        row.MoExternalId,
                        row.OpExternalId));
                }
                
                if (!row.IsAllowedLotCodesNull())
                {
                    if (row.AllowedLotCodes != string.Empty)
                    {
                        AllowedLotCodes.AddRange(row.AllowedLotCodes.Split(','));
                    }

                    AllowedLotCodesIsSet = true;
                }
                else
                {
                    AllowedLotCodesIsSet = false;
                }

                if (!row.IsStorageAreaExternalIdNull())
                {
                    StorageAreaExternalId = row.StorageAreaExternalId;
                }

                if (!row.IsMaterialUsedTimingNull())
                {
                    MaterialUsedTiming = (MaterialRequirementDefs.EMaterialUsedTiming)Enum.Parse(typeof(MaterialRequirementDefs.EMaterialUsedTiming), row.MaterialUsedTiming);
                }

                if (!row.IsRequireFirstTransferAtSetupNull())
                {
                    RequireFirstTransferAtSetup = row.RequireFirstTransferAtSetup;
                }
            }
        }

        /// <summary>
        /// Adds the details from the specified MR into the existing MR.
        /// </summary>
        internal void AbsorbRequirement(MaterialRequirement mr)
        {
            TotalCost += mr.TotalCost;
            TotalRequiredQty += mr.TotalRequiredQty;
            if (mr.AvailableDateTime.Ticks > AvailableDateTime.Ticks)
            {
                AvailableDateTime = mr.AvailableDateTime;
            }

            IssuedQty += mr.IssuedQty;
            if (mr.LeadTimeSpan.Ticks > LeadTimeSpan.Ticks)
            {
                LeadTimeSpan = mr.LeadTimeSpan;
            }
        }

        private MaterialRequirementDefs.EMaterialUsedTiming m_materialUsedTiming = MaterialRequirementDefs.EMaterialUsedTiming.SetupStart;

        public MaterialRequirementDefs.EMaterialUsedTiming MaterialUsedTiming
        {
            get => m_materialUsedTiming;
            set => m_materialUsedTiming = value;
        }

        private MaterialRequirementDefs.TankStorageReleaseTimingEnum m_tankStorageReleaseTiming = MaterialRequirementDefs.TankStorageReleaseTimingEnum.NotTank;

        public MaterialRequirementDefs.TankStorageReleaseTimingEnum TankStorageReleaseTiming
        {
            get => m_tankStorageReleaseTiming;
            set => m_tankStorageReleaseTiming = value;
        }

        #region Shared Properties
        /// <summary>
        /// Indicates the Qty is not changed based on qty changes on MO or Operation
        /// </summary>
        public bool FixedQty
        {
            get => m_bools[c_fixedQty];
            set => m_bools[c_fixedQty] = value;
        }

        public bool AllowedLotCodesIsSet
        {
            get => m_bools[c_allowedLotCodesSet];
            set => m_bools[c_allowedLotCodesSet] = value;
        }

        private List<string> m_allowedLotCodes = new();

        public List<string> AllowedLotCodes
        {
            get => m_allowedLotCodes;
            set => m_allowedLotCodes = value ?? new List<string>();
        }

        private DateTime availableDateTime;

        /// <summary>
        /// When the material is expected to be available for use in production.
        /// If the Constraint Type is Constraint then the minimum of Available Date and Now + Lead Time is used as the constraint date.
        /// If the Constraint Type is Confirmed Constraint then the Available Date is used instead.
        /// </summary>
        public DateTime AvailableDateTime
        {
            get => availableDateTime;
            set => availableDateTime = value;
        }

        private MaterialRequirementDefs.constraintTypes constraintType;

        /// <summary>
        /// Indicates whether the Material Requirment should prevent its Operation from starting before the material arrives.	NonConstraint: Doesn't have any affect on its Operation. Constraint: Operation can't
        /// start until the earlier of the Material's AvailableDate and Clock+LeadTimeSpan. ConfirmedConstraint: Overrides Resource.CanPreEmptMaterials thus forcing the Operation to treat this as a constraint.
        /// </summary>
        public MaterialRequirementDefs.constraintTypes ConstraintType
        {
            get => constraintType;
            set => constraintType = value;
        }

        private decimal issuedQty;

        public decimal IssuedQty
        {
            get => issuedQty;
            set => issuedQty = value;
        }

        private TimeSpan leadTimeSpan;

        /// <summary>
        /// Minimum time span needed to procure the material.
        /// If the Constraint Type is Constraint then the minimum of Available Date and Now + Lead Time is used as the constraint date.
        /// If the Constraint Type is Confirmed Constraint then the Available Date is used instead.
        /// </summary>
        public TimeSpan LeadTimeSpan
        {
            get => leadTimeSpan;
            set => leadTimeSpan = value;
        }

        private string materialDescription;

        /// <summary>
        /// Description of the required material.
        /// </summary>
        public string MaterialDescription
        {
            get => materialDescription;
            set => materialDescription = value;
        }

        private string materialName;

        /// <summary>
        /// Name of the required material.
        /// </summary>
        public string MaterialName
        {
            get => materialName;
            set => materialName = value;
        }

        private string source;

        /// <summary>
        /// Can be used to describe where this material is coming from. ('Purchase Order XYZ' or 'from stock', etc.)
        /// </summary>
        public string Source
        {
            get => source;
            set => source = value;
        }

        private decimal totalCost;

        /// <summary>
        /// Can be used in KPIs and simulation rules to calculate WIP cost.
        /// </summary>
        public decimal TotalCost
        {
            get => totalCost;
            set => totalCost = value;
        }

        private ItemDefs.MaterialAllocation m_materialAllocation = ItemDefs.MaterialAllocation.NotSet;

        public ItemDefs.MaterialAllocation MaterialAllocation
        {
            get => m_materialAllocation;
            set => m_materialAllocation = value;
        }

        private ItemDefs.MaterialSourcing m_materialSourcing = ItemDefs.MaterialSourcing.NotSet;

        public ItemDefs.MaterialSourcing MaterialSourcing
        {
            get => m_materialSourcing;
            set => m_materialSourcing = value;
        }

        private decimal m_minSourceQty;

        public decimal MinSourceQty
        {
            get => m_minSourceQty;
            set => m_minSourceQty = value;
        }

        private decimal m_maxSourceQty;

        public decimal MaxSourceQty
        {
            get => m_maxSourceQty;
            set => m_maxSourceQty = value;
        }

        private decimal totalRequiredQty;

        /// <summary>
        /// The quantity of material to be consumed by the Operation for this requirment.  For display only.
        /// </summary>
        public decimal TotalRequiredQty
        {
            get => totalRequiredQty;
            set => totalRequiredQty = value;
        }

        private string uom;

        public string UOM
        {
            get => uom;
            set => uom = value;
        }

        private string itemExternalId;

        /// <summary>
        /// The External Id of the Item to be used.  This must be a valid Item if Use Item Inventory is set to true.  If not using Item Inventory then this field is for information only.
        /// </summary>
        public string ItemExternalId
        {
            get => itemExternalId;
            set => itemExternalId = value;
        }

        private MaterialRequirementDefs.requirementTypes requirementType;

        /// <summary>
        /// If From Stock then the this Material Requirement will be planned using  Material Planner feature and tracked against the Item specified by the ItemExternalId.
        /// In this case, the Material Requirements values will be updated automatically by Material Planner to dynamically manage materials.
        /// If Buy Direct, this Material Requirement will only use the values specified manually in the Material Requirment and will not interact with inventory.
        /// </summary>
        public MaterialRequirementDefs.requirementTypes RequirementType
        {
            get => requirementType;
            set => requirementType = value;
        }

        private string warehouseExternalId;

        /// <summary>
        /// (optional) The Warehouse from which this material must be supplied.  If omitted then the Material Requirement can be satisfied from any Warehouse accessible by the Plant of the Primary Resource
        /// performing the work.
        /// </summary>
        public string WarehouseExternalId
        {
            get => warehouseExternalId;
            set
            {
                warehouseExternalId = value;
                warehouseExternalIdSet = true;
            }
        }

        public UsabilityRequirement UsabilityReq { get; internal set; }

        private bool warehouseExternalIdSet;

        public bool WarehouseExternalIdSet => warehouseExternalIdSet;

        private string m_storageAreaExternalId;
        /// <summary>
        /// (optional) The Storage Area in the Warehouse from which this material must be supplied.  If omitted then the Material Requirement can be satisfied from any Storage Area from any Warehouse accessible by the Plant of the Primary Resource
        /// performing the work.
        /// </summary>
        public string StorageAreaExternalId
        {
            get => m_storageAreaExternalId;
            set
            {
                m_storageAreaExternalId = value;
                m_storageAreaExternalIdIsSet = true;
            }
        }

        private bool m_storageAreaExternalIdIsSet;
        public bool StorageAreaExternalIdIsSet => m_storageAreaExternalIdIsSet;

        private long m_lotAllocationRule;

        public long LotAllocationRule
        {
            get => m_lotAllocationRule;
            set => m_lotAllocationRule = value;
        }

        /// <summary>
        /// Whether to allow this Material Request to depend on material from Purchase Orders that haven't arrived yet but whose material is
        /// projected to arrive in stock on time to satisfy the cycles of the operation. This may allow the operation and other operations to start earlier.
        /// If this is not checked then this Material Request may end up waiting until the expected receive date of Purchase Order whose material is needed.
        /// </summary>
        public bool UseOverlapPOs
        {
            get => m_bools[c_useOverlapPOsIdx];

            set => m_bools[c_useOverlapPOsIdx] = value;
        }

        /// <summary>
        /// Whether multiple warehouses can supply the material for this requirement.
        /// </summary>
        public bool MultipleWarehouseSupplyAllowed
        {
            get => m_bools[c_multipleWarehouseSupplyAllowedIdx];
            set => m_bools[c_multipleWarehouseSupplyAllowedIdx] = value;
        }
        
        /// <summary>
        /// Whether multiple storage areas can supply the material for this requirement.
        /// </summary>
        public bool MultipleStorageAreaSupplyAllowed
        {
            get => m_bools[c_multipleStorageAreaSupplyAllowedIdx];
            set => m_bools[c_multipleStorageAreaSupplyAllowedIdx] = value;
        }

        public bool AllowPartialSupply
        {
            get => m_bools[c_allowPartialSupplyIdx];
            set => m_bools[c_allowPartialSupplyIdx] = value;
        }
        
        public bool RequireFirstTransferAtSetup
        {
            get => m_bools[c_requireFirstTransferAtSetupIdx];
            set => m_bools[c_requireFirstTransferAtSetupIdx] = value;
        }

        private decimal m_plannedScrapQty;

        public decimal PlannedScrapQty
        {
            get => m_plannedScrapQty;
            set => m_plannedScrapQty = value;
        }

        private TimeSpan m_minRemainingShelfLife;

        public TimeSpan MinRemainingShelfLife
        {
            get => m_minRemainingShelfLife;
            private set => m_minRemainingShelfLife = value;
        }

        private TimeSpan m_minAge;

        /// <summary>
        /// The minimum length of time that must have elapsed since the material finished production.
        /// </summary>
        public TimeSpan MinAge
        {
            get => m_minAge;
            set => m_minAge = value;
        }

        private int m_maxEligibleWearAmount;

        public int MaxEligibleWearAmount
        {
            get => m_maxEligibleWearAmount;
            private set => m_maxEligibleWearAmount = value;
        }

        private string m_productRelease;

        public string ProductRelease
        {
            get => m_productRelease;
            private set => m_productRelease = value;
        }

        /// <summary>
        /// Whether material requirements can source from material that has expired.
        /// </summary>
        public bool AllowExpiredSupply
        {
            get => m_bools[c_allowExpiredSupplyIdx];
            set => m_bools[c_allowExpiredSupplyIdx] = value;
        }

        #endregion Shared Properties

        public class UsabilityRequirement : PTObjectIdBase { }

        public class WearRequirement : UsabilityRequirement
        {
            #region Serialization
            internal WearRequirement(IReader reader)
            {
                if (reader.VersionNumber >= 427)
                {
                    reader.Read(out m_maxEligibleWearAmount);
                }
                else
                {
                    reader.Read(out m_maxEligibleWearAmount);
                    int thisValueWasRemoved;
                    reader.Read(out thisValueWasRemoved);
                }
            }

            public override void Serialize(IWriter writer)
            {
                writer.Write(MaxEligibleWearAmount);
            }

            public new const int UNIQUE_ID = 752;

            public override int UniqueId => UNIQUE_ID;
            #endregion

            internal WearRequirement(int a_maxEligibleWearAmount, string ProductRelease)
            {
                MaxEligibleWearAmount = a_maxEligibleWearAmount;
            }

            private int m_maxEligibleWearAmount;

            public int MaxEligibleWearAmount
            {
                get => m_maxEligibleWearAmount;
                private set => m_maxEligibleWearAmount = value;
            }

            private string m_productRelease;

            public string ProdcutRelease
            {
                get => m_productRelease;
                private set => m_productRelease = value;
            }
        }

        public class ShelfLifeRequirement : UsabilityRequirement
        {
            #region Serialization
            internal ShelfLifeRequirement(IReader reader)
            {
                if (reader.VersionNumber >= 698)
                {
                    reader.Read(out m_minRemainingShelfLife);
                    reader.Read(out m_minAge);
                }

                #region 1
                else
                {
                    reader.Read(out m_minRemainingShelfLife);
                }
                #endregion
            }

            public override void Serialize(IWriter writer)
            {
                writer.Write(MinRemainingShelfLife);
                writer.Write(m_minAge);
            }

            internal ShelfLifeRequirement(TimeSpan a_minRemainingShelfLife, TimeSpan a_minAge)
            {
                m_minRemainingShelfLife = a_minRemainingShelfLife;
                m_minAge = a_minAge;
            }

            public new const int UNIQUE_ID = 753;

            public override int UniqueId => UNIQUE_ID;
            #endregion

            private TimeSpan m_minRemainingShelfLife;

            public TimeSpan MinRemainingShelfLife
            {
                get => m_minRemainingShelfLife;
                private set => m_minRemainingShelfLife = value;
            }

            private TimeSpan m_minAge;

            /// <summary>
            /// The minimum length of time that must have elapsed since the material finished production.
            /// </summary>
            public TimeSpan MinAge
            {
                get => m_minAge;
                set => m_minAge = value;
            }
        }
    }

    public class Product : PTObjectIdBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 534;

        #region PT Serialization
        public Product(IReader reader)
            : base(reader)
        {
            if (reader.VersionNumber >= 12511)
            {
                m_bools = new BoolVector32(reader);

                reader.Read(out itemExternalId);
                reader.Read(out totalOutputQty);
                reader.Read(out warehouseExternalId);
                reader.Read(out int val);
                m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)val;
                reader.Read(out m_materialPostProcessingTicks);
                reader.Read(out m_lotCode);
                reader.Read(out m_completedQty);

                reader.Read(out m_unitVolumeOverride);
                reader.Read(out m_storageAreaExternalId);

                reader.Read(out m_wearDurability);
            }
            else if (reader.VersionNumber >= 12106)
            {
                reader.Read(out itemExternalId);
                reader.Read(out totalOutputQty);
                reader.Read(out warehouseExternalId);
                int val;
                reader.Read(out val);
                m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)val;
                reader.Read(out m_materialPostProcessingTicks);
                m_bools = new BoolVector32(reader);
                reader.Read(out m_lotCode);
                reader.Read(out m_completedQty);

                reader.Read(out m_unitVolumeOverride);
            }
            else if (reader.VersionNumber >= 12028)
            {
                reader.Read(out itemExternalId);
                reader.Read(out totalOutputQty);
                reader.Read(out warehouseExternalId);
                int val;
                reader.Read(out val);
                m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)val;
                reader.Read(out m_materialPostProcessingTicks);
                m_bools = new BoolVector32(reader);
                reader.Read(out m_lotCode);
                reader.Read(out m_completedQty);
            }
            else if (reader.VersionNumber >= 12000)
            {
                reader.Read(out itemExternalId);
                reader.Read(out totalOutputQty);
                reader.Read(out warehouseExternalId);
                int val;
                reader.Read(out val);
                m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)val;
                reader.Read(out m_materialPostProcessingTicks);
                m_bools = new BoolVector32(reader);
                reader.Read(out m_lotCode);
            }
            else
            {
                #region 747
                if (reader.VersionNumber >= 747)
                {
                    reader.Read(out itemExternalId);
                    reader.Read(out totalOutputQty);
                    reader.Read(out warehouseExternalId);
                    int val;
                    reader.Read(out val);
                    m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)val;
                    reader.Read(out m_materialPostProcessingTicks);
                    m_bools = new BoolVector32(reader);
                    reader.Read(out m_lotCode);
                    reader.Read(out m_completedQty);
                }
                #endregion 747

                #region 663
                else if (reader.VersionNumber >= 663)
                {
                    reader.Read(out itemExternalId);
                    reader.Read(out totalOutputQty);
                    reader.Read(out warehouseExternalId);
                    int val;
                    reader.Read(out val);
                    m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)val;
                    reader.Read(out m_materialPostProcessingTicks);
                    m_bools = new BoolVector32(reader);
                    reader.Read(out m_lotCode);
                }
                #endregion 663
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);
            m_bools.Serialize(writer);

            writer.Write(itemExternalId);
            writer.Write(totalOutputQty);
            writer.Write(warehouseExternalId);
            writer.Write((int)m_inventoryAvailableTiming);
            writer.Write(m_materialPostProcessingTicks);
            writer.Write(m_lotCode);
            writer.Write(m_completedQty);

            writer.Write(m_unitVolumeOverride);
            writer.Write(m_storageAreaExternalId);

            writer.Write(m_wearDurability);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region BoolVector32
        private BoolVector32 m_bools;
        //private const int StoreInTankIDX = 0; Dreprecated
        private const int SetWarehouseDuringMRPIdx = 1;
        private const int c_fixedQtyIdx = 2;
        private const int c_limitMatlSrcToEligibleLotsIdx = 3;
        private const int c_useLimitMatlSrcToEligibleLotsIdx = 4;
        private const int c_requireEmptyStorageAreaIdx = 5;
        #endregion

        public Product() { }

        public new void Validate()
        {
            if (string.IsNullOrEmpty(WarehouseExternalId))
            {
                throw new PTValidationException("2821", new object[] { ExternalId });
            }
        }

        /// <summary>
        /// Specifies a quantity of an Item to be produced.
        /// </summary>
        /// <param name="externalId">Unique identifier of the Product record within an Operation.</param>
        /// <param name="itemExternalId">The Item to be made by the Operation.</param>
        /// <param name="totalOutputQty">The full quantity of this Item to be made by this Product record for this Operation.</param>
        /// <param name="warehouseExternalId">
        /// The Warehouse where the full quantity of the Item made will be stored.  This can be any Warehouse in the system and is not limited to the supply Warehouses for a
        /// Plant.
        /// </param>
        public Product(string externalId, string itemExternalId, decimal totalOutputQty, string warehouseExternalId)
            : base(externalId)
        {
            this.totalOutputQty = totalOutputQty;
            this.itemExternalId = itemExternalId;
            this.warehouseExternalId = warehouseExternalId;
        }

        public Product(JobDataSet.ProductRow row)
            : base(row.ExternalId)
        {
            itemExternalId = row.ItemExternalId;
            warehouseExternalId = row.WarehouseExternalId;

            if (!row.IsTotalOutputQtyNull())
            {
                totalOutputQty = row.TotalOutputQty;
            }

            if (!row.IsInventoryAvailableTimingNull())
            {
                try
                {
                    if (row.InventoryAvailableTiming == "BasedOnTransferQty")
                    {
                        //For backwards compatibility before rename
                        m_inventoryAvailableTiming = ProductDefs.EInventoryAvailableTimings.ByProductionCycle;
                    }
                    else
                    {
                        m_inventoryAvailableTiming = (ProductDefs.EInventoryAvailableTimings)Enum.Parse(typeof(ProductDefs.EInventoryAvailableTimings), row.InventoryAvailableTiming);
                    }
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[] {
                            row.InventoryAvailableTiming,
                            "Product",
                            "InventoryAvailableTiming",
                            string.Join(", ", Enum.GetNames(typeof(ProductDefs.EInventoryAvailableTimings)))
                        });
                }
            }

            if (!row.IsSetWarehouseDuringMRPNull())
            {
                SetWarehouseDuringMRP = row.SetWarehouseDuringMRP;
            }

            if (!row.IsMaterialPostProcessingHrsNull())
            {
                m_materialPostProcessingTicks = TimeSpan.FromHours(row.MaterialPostProcessingHrs).Ticks;
            }

            if (!row.IsFixedQtyNull())
            {
                FixedQty = row.FixedQty;
            }

            if (!row.IsLotCodeNull())
            {
                LotCode = row.LotCode;
            }

            if (!row.IsCompletedQtyNull())
            {
                CompletedQty = row.CompletedQty;
            }

            if (!row.IsUseLimitMatlSrcToEligibleLotsNull())
            {
                UseLimitMatlSrcToEligibleLots = row.UseLimitMatlSrcToEligibleLots;
            }

            if (!row.IsLimitMatlSrcToEligibleLotsNull())
            {
                LimitMatlSrcToEligibleLots = row.LimitMatlSrcToEligibleLots;
            }

            if (!row.IsUnitVolumeOverrideNull())
            {
                UnitVolumeOverride = row.UnitVolumeOverride;
            }

            if (!row.IsStorageAreaExternalIdNull())
            {
                StorageAreaExternalId = row.StorageAreaExternalId;
            }
            
            if (!row.IsRequireEmptyStorageAreaNull())
            {
                RequireEmptyStorageArea = row.RequireEmptyStorageArea;
            }
        }

        #region Shared Properties
        public bool RequireEmptyStorageArea
        {
            get => m_bools[c_requireEmptyStorageAreaIdx];
            set => m_bools[c_requireEmptyStorageAreaIdx] = value;
        }
        /// <summary>
        /// Inficates TotalOutputQty is in regards to qty adjustments on MO or Operation.
        /// </summary>
        public bool FixedQty
        {
            get => m_bools[c_fixedQtyIdx];
            set => m_bools[c_fixedQtyIdx] = value;
        }

        private string m_lotCode;

        /// <summary>
        /// Inficates TotalOutputQty is in regards to qty adjustments on MO or Operation.
        /// </summary>
        public string LotCode
        {
            get => m_lotCode;
            set => m_lotCode = value;
        }

        private decimal totalOutputQty;

        /// <summary>
        /// The total amount of the specified Item to be made by the Operation.
        /// </summary>
        public decimal TotalOutputQty
        {
            get => totalOutputQty;
            set => totalOutputQty = value;
        }

        private string itemExternalId;

        /// <summary>
        /// The ExternalId of the Item to be produced by the Operation.
        /// </summary>
        public string ItemExternalId
        {
            get => itemExternalId;
            set => itemExternalId = value;
        }

        private string warehouseExternalId;

        /// <summary>
        /// The Warehouse where the parts will be inventoried when complete.
        /// This can be any Warehouse in the system even if the Plant where the product is being made does not have
        /// the Warehouse specified in its list of supplying Warehouses.
        /// </summary>
        public string WarehouseExternalId
        {
            get => warehouseExternalId;
            set => warehouseExternalId = value;
        }

        private string m_storageAreaExternalId;

        /// <summary>
        /// The Storage Area in the Warehouse where the parts will be inventoried when complete.
        /// </summary>
        public string StorageAreaExternalId
        {
            get => m_storageAreaExternalId;
            set => m_storageAreaExternalId = value;
        }

        private ProductDefs.EInventoryAvailableTimings m_inventoryAvailableTiming = ProductDefs.EInventoryAvailableTimings.AtOperationResourcePostProcessingEnd;

        /// <summary>
        /// Controls when inventory that is created is considered available in stock for use by a consuming Material Requirement.
        /// </summary>
        public ProductDefs.EInventoryAvailableTimings InventoryAvailableTiming
        {
            get => m_inventoryAvailableTiming;
            set => m_inventoryAvailableTiming = value;
        }

        /// <summary>
        /// If true then during MRP the Warehouse is set to the Warehouse where the demand occurrs (provided that the Item is stocked in the demand's Warehouse).
        /// </summary>
        public bool SetWarehouseDuringMRP
        {
            get => m_bools[SetWarehouseDuringMRPIdx];
            set => m_bools[SetWarehouseDuringMRPIdx] = value;
        }

        private long m_materialPostProcessingTicks;

        /// <summary>
        /// This is similar to Operation.MaterialPostProcessing. It's added to the time when product will become available to determine a
        /// new availability time.
        /// </summary>
        public long MaterialPostProcessingTicks
        {
            get => m_materialPostProcessingTicks;
            set => m_materialPostProcessingTicks = value;
        }

        private decimal m_completedQty;

        /// <summary>
        /// The total amount of the specified Item that has already been produced.
        /// </summary>
        public decimal CompletedQty
        {
            get => m_completedQty;
            internal set => m_completedQty = value;
        }

        public bool LimitMatlSrcToEligibleLots
        {
            get => m_bools[c_limitMatlSrcToEligibleLotsIdx];
            set => m_bools[c_limitMatlSrcToEligibleLotsIdx] = value;
        }

        public bool UseLimitMatlSrcToEligibleLots
        {
            get => m_bools[c_useLimitMatlSrcToEligibleLotsIdx];
            set => m_bools[c_useLimitMatlSrcToEligibleLotsIdx] = value;
        }

        private decimal m_unitVolumeOverride;

        /// <summary>
        /// The total volume to use for this product instead of using item Unit Volume
        /// </summary>
        public decimal UnitVolumeOverride
        {
            get => m_unitVolumeOverride;
            internal set => m_unitVolumeOverride = value;
        }

        private int m_wearDurability;

        public int WearDurability
        {
            get => m_wearDurability;
            private set => m_wearDurability = value;
        }
        #endregion Shared Properties
    }

    public new Job this[int i] => Nodes[i];

    public override void Validate()
    {
        HashSet<string> addedHash = new();

        for (int i = 0; i < Count; i++)
        {
            Job job = this[i];
            if (addedHash.Contains(job.ExternalId))
            {
                throw new ValidationException("2096", new object[] { job.ExternalId });
            }

            addedHash.Add(job.ExternalId);
        }
    }

    public override string Description => string.Format("Jobs updated ({0})".Localize(), Count);
}

public class LotAllocationRuleValues
{
    public LotAllocationRuleValues(IReader reader)
    {
        BoolVector32 depcredateBoolVector = new(reader);
    }
}