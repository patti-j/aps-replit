using PT.ERPTransmissions;

namespace PT.Scheduler.Schedule.InventoryManagement;

public class PurchaseToStockDataSetFiller
{
    public void FillDataSet(PtImportDataSet.PurchaseToStocksDataTable a_ds, PurchaseToStock a_po)
    {
        PtImportDataSet.PurchaseToStocksRow newRow = a_ds.NewPurchaseToStocksRow();
        newRow.ExternalId = a_po.ExternalId;
        newRow.Name = a_po.Name;
        newRow.Description = a_po.Description;
        newRow.Notes = a_po.Notes;
        newRow.ItemExternalId = a_po.Inventory.Item?.ExternalId ?? "";
        newRow.QtyOrdered = a_po.QtyOrdered;
        newRow.ScheduledReceiptDate = a_po.ScheduledReceiptDate;
        newRow.WarehouseExternalId = a_po.Warehouse?.ExternalId ?? "";
        newRow.BuyerExternalId = a_po.BuyerExternalId;
        newRow.TransferHrs = a_po.TransferSpan.TotalHours;
        newRow.UnloadHrs = a_po.UnloadSpan.TotalHours;
        newRow.VendorExternalId = a_po.VendorExternalId;
        newRow.Firm = a_po.Firm;
        newRow.ActualReceiptDate = a_po.ActualReceiptDate;
        newRow.BuyerExternalId = a_po.BuyerExternalId;
        newRow.Closed = a_po.Closed;
        newRow.LotCode = a_po.LotCode;
        newRow.Name = a_po.Name;
        newRow.UserFields = a_po.UserFields?.GetUserFieldImportString() ?? "";
        newRow.QtyReceived = a_po.QtyReceived;

        a_ds.AddPurchaseToStocksRow(newRow);
    }
}