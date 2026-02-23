select t.totalcostestimate "total cost", t.id "SO InternalID", t.shipdate, t.tranid "SO Name",
tl.item "Item InternalID", ABS(tl.quantity) "Quantity"
 from transaction t
join transactionline tl on t.id = tl.transaction
WHERE
  t.type = 'SalesOrd'
  AND REPLACE(BUILTIN.DF(t.status), ' ', '') NOT IN (
        'SalesOrder:PendingApproval',
        'SalesOrder:Cancelled',
        'SalesOrder:Closed',
        'SalesOrder:Billed'
      )
AND ABS(tl.quantity) > 0