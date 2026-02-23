using System.Data;

namespace PT.SchedulerDefinitions;

public partial class JobDataSet
{
    partial class MaterialRequirementDataTable
    {
        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    partial class SubManufacturingOrdersDataTable
    {
        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    partial class ManufacturingOrderDataTable
    {
        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    partial class CapabilityDataTable
    {
        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    partial class ResourceRequirementDataTable
    {
        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    partial class ProductDataTable
    {
        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    partial class JobDataTable
    {

        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    partial class ActivityDataTable
    {
        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    partial class ResourceOperationAttributesDataTable { }

    private class OperationAttributesDataTable { }

    partial class CustomerDataTable { }

    partial class ResourceOperationDataTable
    {
        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    partial class AlternatePathNodeDataTable
    {
        public override void EndInit()
        {
            base.EndInit();
            SetImportAttributes(Columns);
        }
    }

    public static void SetImportAttributes(DataColumnCollection a_collection)
    {
        foreach (DataColumn column in a_collection)
        {
            if (column.ReadOnly)
            {
                column.ExtendedProperties["Non-Importable"] = true;
                column.ReadOnly = false;
            }
        }
    }
}