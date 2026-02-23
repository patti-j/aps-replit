select t.id "PO InternalID", t.shipdate, t.tranid "PO Name", tl.item "Item InternalID",
ABS(tl.quantity) "Quantity"
 from transaction t
join transactionline tl on t.id = tl.transaction
WHERE
  t.type = 'PurchOrd'
  AND REPLACE(BUILTIN.DF(t.status), ' ', '') IN (
        'PurchaseOrder:PartiallyReceived',
        'PurchaseOrder:PendingBilling/PartiallyReceived',
        'PurchaseOrder:PendingReceipt',
        'PurchaseOrder:PendingBillingPartiallyReceived'
      )
AND ABS(tl.quantity) > 0