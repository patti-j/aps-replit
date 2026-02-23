
namespace PT.SchedulerDefinitions
{
    public class ImportStatuses
    {
        public enum EImportProgressStep : short
        {
            Started,
            Cancelled,
            Error,
            PreImportWebhook,
            PreImportProgram,
            PreImportSql,
            Connecting,
            #region Import Steps - Use object name to simplify translation. Notifications show Importing: {LocalizedStepName}
            UserFields,
            Customers,
            Users,
            Plants,
            Departments,
            Capabilities,
            Cells,
            Resources,
            ResourceConnectors,
            Items,
            Warehouses,
            Inventory,
            Lots,
            Capacity,
            Demands,
            ProductRules,
            Attributes,
            CleanoutIntervals,
            Compatibility,
            PurchaseToStock,
            SalesOrders,
            Forecasts,
            TransferOrders,
            Jobs,
            Activities,
            Misc, // For unspecified import step
            #endregion
            Complete,
            PostImportWebhook,
            CustomImportProcedure
        }

        public bool IsImportingData(EImportProgressStep a_step)
        {
            return a_step >= EImportProgressStep.UserFields && a_step <= EImportProgressStep.Misc;
        }

    }
}
