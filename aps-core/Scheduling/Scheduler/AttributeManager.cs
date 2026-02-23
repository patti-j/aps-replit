using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Database;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of PTAttribute objects.
/// </summary>
public class AttributeManager : ScenarioBaseObjectManager<PTAttribute>
{
    #region IPTSerializable Members
    public AttributeManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                PTAttribute c = new (a_reader);
                Add(c);
            }
        }
    }
    
    public new const int UNIQUE_ID = 994;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class AttributeManagerException : PTException
    {
        public AttributeManagerException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }
    }
    #endregion

    #region Construction
    public AttributeManager(BaseIdGenerator a_idGen) : base(a_idGen) { }
    #endregion

    #region Attribute Edit Functions
    private PTAttribute AddDefault(PTAttributeDefaultT t)
    {
        PTAttribute newPTAttribute = new (NextID());
        newPTAttribute.ExternalId = NextExternalId("PTAttribute");
        ValidateAdd(newPTAttribute);
        return Add(newPTAttribute);
    }

    private PTAttribute AddCopy(PTAttributeCopyT a_t)
    {
        ValidateCopy(a_t);
        PTAttribute attribute = GetById(a_t.OriginalId);
        PTAttribute copyAttribute = new (attribute, NextID());
        copyAttribute.ExternalId = NextExternalId("PTAttribute");
        copyAttribute.Name = MakeCopyName(attribute.Name);
        return AddCopy(attribute, copyAttribute);
    }

    private void Delete(BaseId a_ptAttributeId, HashSet<BaseId> a_cannotDeleteTheseAttributes)
    {
        PTAttribute ptAttribute = GetById(a_ptAttributeId);
        if (ptAttribute != null)
        {
            if (a_cannotDeleteTheseAttributes.Contains(a_ptAttributeId))
            {
                throw new PTValidationException("4472", new object[] { ptAttribute.ExternalId });
            }

            Remove(ptAttribute.Id); //Now remove it from the Manager.
        }
    }
    #endregion

    #region Transmissions
    private void ValidateAdd(PTAttribute a_ptAttribute)
    {
        if (Contains(a_ptAttribute))
        {
            //TODO: add error code
            throw new AttributeManagerException("", new object[] { a_ptAttribute.Id.ToString() });
        }
    }

    private void ValidateCopy(PTAttributeCopyT a_t)
    {
        ValidateExistence(a_t.OriginalId);
    }

    public void Receive(PTAttributeBaseT a_t, IScenarioDataChanges a_dataChanges)
    {
        PTAttribute ptAttribute;
        if (a_t is PTAttributeDefaultT ptAttributeDefaultT)
        {
            ptAttribute = AddDefault(ptAttributeDefaultT);
            a_dataChanges.PTAttributeChanges.AddedObject(ptAttribute.Id);
        }
        else if (a_t is PTAttributeCopyT ptAttributeCopyT)
        {
            ptAttribute = AddCopy(ptAttributeCopyT);
            a_dataChanges.PTAttributeChanges.AddedObject(ptAttribute.Id);
        }
        else if (a_t is PTAttributeDeleteT ptAttributeDeleteT)
        {
            HashSet<BaseId> inUsePTAttributeIds = GetInUsePTAttributeIds();
            for (int i = 0; i < ptAttributeDeleteT.PTAttributeIds.Count; i++)
            {
                BaseId PTAttributeId = ptAttributeDeleteT.PTAttributeIds[i];
                Delete(PTAttributeId, inUsePTAttributeIds);
                a_dataChanges.PTAttributeChanges.DeletedObject(PTAttributeId);
            }
        }
        else if (a_t is PTAttributeDeleteAllT)
        {
            HashSet<BaseId> inUsePTAttributeIds = GetInUsePTAttributeIds();
            for (int i = Count - 1; i >= 0; i--)
            {
                ptAttribute = GetByIndex(i);
                Delete(ptAttribute.Id, inUsePTAttributeIds);
                a_dataChanges.PTAttributeChanges.DeletedObject(ptAttribute.Id);
            }
        }
    }

    private HashSet<BaseId> GetInUsePTAttributeIds()
    {
        HashSet<BaseId> mappedAttributeIds = new ();
        foreach (Job job in m_scenarioDetail.JobManager)
        {
            List<BaseOperation> baseOps = job.GetOperations();
            foreach (BaseOperation baseOp in baseOps)
            {
                foreach (OperationAttribute opAttribute in baseOp.Attributes)
                {
                    mappedAttributeIds.Add(opAttribute.PTAttribute.Id);
                }
            }
        }

        return mappedAttributeIds;
    }

    /// <summary>
    /// Transmission receive for PTAttribute GridEdits
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_t"></param>
    /// <param name="a_dataChanges"></param>
    /// <exception cref="PTHandleableException"></exception>
    public void Receive(ScenarioDetail a_sd, PTAttributeEditT a_t, IScenarioDataChanges a_dataChanges)
    {
        foreach (PTAttributeEdit edit in a_t)
        {
            PTAttribute ptAttribute = null;
            if (edit.BaseIdSet)
            {
                ptAttribute = GetById(edit.Id);
            }
            else if (edit.ExternalIdSet)
            {
                ptAttribute = GetByExternalId(edit.ExternalId);
            }

            if (ptAttribute == null)
            {
                throw new PTHandleableException("Not found");
            }

            ptAttribute.Edit(edit);
            a_dataChanges.PTAttributeChanges.UpdatedObject(ptAttribute.Id);
        }
    }
    #endregion

    #region ERP Transmissions
    public void Receive(ScenarioDetail a_sd, PTAttributeT a_t, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new();
        List<PostProcessingAction> actions = new();

        try
        {
            for (int i = 0; i < a_t.Count; ++i)
            {
                PTAttributeT.PTAttribute attributeNode = a_t[i];

                PTAttribute ptAttribute;
                if (attributeNode.IdSet)
                {
                    ptAttribute = GetById(attributeNode.Id);
                    if (ptAttribute == null)
                    {
                        //TODO: add error code
                        throw new ValidationException("", new object[] { attributeNode.Id });
                    }
                }
                else
                {
                    ptAttribute = GetByExternalId(attributeNode.ExternalId);
                }

                if (ptAttribute == null)
                {
                    ptAttribute = new PTAttribute(attributeNode, NextID());
                    Add(ptAttribute);
                    a_dataChanges.PTAttributeChanges.AddedObject(ptAttribute.Id);
                }
                else
                {
                    ptAttribute.Update(attributeNode);
                    a_dataChanges.PTAttributeChanges.UpdatedObject(ptAttribute.Id);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                for (int i = Count - 1; i >= 0; i--)
                {
                    BaseId id = GetByIndex(i).Id;
                    if (!a_dataChanges.PTAttributeChanges.Updated.Contains(id) &&
                        !a_dataChanges.PTAttributeChanges.Added.Contains(id))
                    {
                        a_dataChanges.PTAttributeChanges.DeletedObject(id);
                    }
                }

                if (a_dataChanges.PTAttributeChanges.TotalDeletedObjects > 0)
                {
                    HashSet<BaseId> inUsePTAttributeIds = GetInUsePTAttributeIds();
                    ScenarioExceptionInfo sei = new ();
                    sei.Create(a_sd);
                    actions.Add(new PostProcessingAction(a_t, false, () =>
                        {
                            foreach (BaseId id in a_dataChanges.PTAttributeChanges.Deleted)
                            {
                                try
                                {
                                    Delete(id, inUsePTAttributeIds);
                                }
                                catch (PTHandleableException err)
                                {
                                    m_errorReporter.LogException(err, a_t, sei,ELogClassification.PtSystem, false);
                                }
                            }
                        }));
                }
            }
        }
        catch (Exception err)
        {
            errList.Add(err);
        }
        finally
        {
            a_sd.AddProcessingAction(actions);
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(PTAttribute);
    #endregion

    public void PtDbPopulate(ref PtDbDataSet a_ds, PTDatabaseHelper a_dbHelper, PtDbDataSet.SchedulesRow a_schedulesRow)
    {
        foreach (PTAttribute ptAttribute in this)
        {
            ptAttribute.PtDbPopulate(ref a_ds, a_dbHelper, a_schedulesRow);
        }
    }

    public void ClearPTAttributes(ScenarioDetail a_sd, PTTransmission a_t, IScenarioDataChanges a_dataChanges)
    {
        ApplicationExceptionList errList = new();
        HashSet<BaseId> inUsePTAttributeIds = GetInUsePTAttributeIds();

        for (int i = Count - 1; i >= 0; i--)
        {
            try
            {
                BaseId id = GetByIndex(i).Id;
                Delete(id, inUsePTAttributeIds);
                a_dataChanges.PTAttributeChanges.DeletedObject(id);

            }
            catch (Exception err)
            {
                errList.Add(err);
            }
        }

        if (errList.Count > 0)
        {
            ScenarioExceptionInfo sei = new();
            sei.Create(a_sd);
            m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
        }
    }

    /// <summary>
    /// Creates a PTAttribute for converted SetupCodeTables for backwards compatibility
    /// </summary>
    /// <returns></returns>
    internal string AddAttributeForBackwardsCompatibility()
    {
        PTAttribute newPTAttribute = new(NextID());
        newPTAttribute.Name = newPTAttribute.ExternalId = NextExternalId("PTAttribute_SetupCode");
        newPTAttribute.AttributeType = PTAttributeDefs.EIncurAttributeType.Setup;
        newPTAttribute.AttributeTrigger = PTAttributeDefs.EAttributeTriggerOptions.CodeChanges;
        Add(newPTAttribute);

        return newPTAttribute.ExternalId;
    }
}