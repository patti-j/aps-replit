using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

/// <summary>
/// Shared operations that are performed on JobDataSets.
/// </summary>
public class JobDataSetFunctions
{
    #region Row Finder Functions
    public JobDataSet.ResourceOperationRow GetOpRow(JobDataSet a_jobDataSet, string a_moExternalId, string a_opExternalId)
    {
        a_jobDataSet.AcceptChanges();

        //Find the dataset row that corresponds
        for (int i = 0; i < a_jobDataSet.ResourceOperation.Count; i++)
        {
            JobDataSet.ResourceOperationRow row = a_jobDataSet.ResourceOperation[i];
            if (row.MoExternalId == a_moExternalId && row.ExternalId == a_opExternalId)
            {
                return row;
            }
        }

        return null;
    }

    public JobDataSet.ResourceOperationRow GetOpRow(JobDataSet a_jobDataSet, BaseId moId, BaseId opId)
    {
        a_jobDataSet.AcceptChanges();

        for (int moI = 0; moI < a_jobDataSet.ManufacturingOrder.Count; moI++)
        {
            JobDataSet.ManufacturingOrderRow moRow = a_jobDataSet.ManufacturingOrder[moI];
            if (moRow.Id == moId.ToBaseType())
            {
                for (int i = 0; i < a_jobDataSet.ResourceOperation.Count; i++)
                {
                    JobDataSet.ResourceOperationRow row = a_jobDataSet.ResourceOperation[i];
                    if (row.MoExternalId == moRow.ExternalId && row.Id == opId.ToBaseType())
                    {
                        return row;
                    }
                }
            }
        }

        return null;
    }

    public JobDataSet.MaterialRequirementRow GetStockMaterialRow(JobDataSet a_jobDataSet, string moExternalId, string opExternalId, string externalId)
    {
        a_jobDataSet.AcceptChanges();

        //Find the dataset row that corresponds
        for (int i = 0; i < a_jobDataSet.MaterialRequirement.Count; i++)
        {
            JobDataSet.MaterialRequirementRow row = a_jobDataSet.MaterialRequirement[i];
            if (row.MoExternalId == moExternalId && row.OpExternalId == opExternalId && row.ExternalId == externalId)
            {
                return row;
            }
        }

        return null;
    }

    public JobDataSet.ProductRow GetProductRow(JobDataSet a_jobDataSet, string moExternalId, string opExternalId, string externalId)
    {
        a_jobDataSet.AcceptChanges();

        //Find the dataset row that corresponds
        for (int i = 0; i < a_jobDataSet.Product.Count; i++)
        {
            JobDataSet.ProductRow row = a_jobDataSet.Product[i];
            if (row.MoExternalId == moExternalId && row.OpExternalId == opExternalId && row.ExternalId == externalId)
            {
                return row;
            }
        }

        return null;
    }

    public JobDataSet.ResourceOperationAttributesRow GetOpAttributesRow(JobDataSet a_jobDataSet, string a_moExternalId, string a_opExternalId, string a_attributeExternalId)
    {
        a_jobDataSet.AcceptChanges();

        //Find the dataset row that corresponds
        for (int i = 0; i < a_jobDataSet.ResourceOperationAttributes.Count; i++)
        {
            JobDataSet.ResourceOperationAttributesRow row = a_jobDataSet.ResourceOperationAttributes[i];
            if (row.MoExternalId == a_moExternalId && row.OpExternalId == a_opExternalId && row.AttributeExternalId.ToUpper() == a_attributeExternalId.ToUpper()) //use ToUpper becuase otherwise duplicate rows differing just in Name case can occur and this causes an error when saving the attributes.
            {
                return row;
            }
        }

        return null;
    }

    public JobDataSet.ResourceRequirementRow GetResourceRequirementRow(JobDataSet a_jobDataSet, string moExternalId, string opExternalId, string externalId)
    {
        a_jobDataSet.AcceptChanges();
        //Find the dataset row that corresponds
        for (int i = 0; i < a_jobDataSet.ResourceRequirement.Count; i++)
        {
            JobDataSet.ResourceRequirementRow row = a_jobDataSet.ResourceRequirement[i];
            if (row.MoExternalId == moExternalId && row.OpExternalId == opExternalId && row.ExternalId == externalId)
            {
                return row;
            }
        }

        return null;
    }

    public JobDataSet.CapabilityRow GetCapabilityRow(JobDataSet a_jobDataSet, string moExternalId, string opExternalId, string rrExternalId, string externalId)
    {
        a_jobDataSet.AcceptChanges();

        //Find the dataset row that corresponds
        for (int i = 0; i < a_jobDataSet.Capability.DefaultView.Count; i++)
        {
            JobDataSet.CapabilityRow row = (JobDataSet.CapabilityRow)a_jobDataSet.Capability.DefaultView[i].Row;
            if (row.MoExternalId == moExternalId && row.OpExternalId == opExternalId && row.ResourceRequirementExternalId == rrExternalId && row.CapabilityExternalId == externalId)
            {
                return row;
            }
        }

        return null;
    }

    public JobDataSet.ActivityRow GetActivityRow(JobDataSet a_jobDataSet, string moExternalId, string opExternalId, string externalId)
    {
        a_jobDataSet.AcceptChanges();

        //Find the dataset row that corresponds
        for (int i = 0; i < a_jobDataSet.Activity.Count; i++)
        {
            JobDataSet.ActivityRow row = a_jobDataSet.Activity[i];
            if (row.MoExternalId == moExternalId && row.OpExternalId == opExternalId && row.ExternalId == externalId)
            {
                return row;
            }
        }

        return null;
    }
    #endregion
}