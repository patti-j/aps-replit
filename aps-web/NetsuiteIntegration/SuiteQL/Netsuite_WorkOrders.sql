SELECT DISTINCT
  t.id                      AS "WO InternalID",
  t.tranid                  AS "WO Name",
  t.enddate                 AS "WO DueDate",
  
  tl.item                   AS "Produced Item InternalID",
  i.itemid                  AS "Produced Part No",

  mot.id                    AS "Operation InternalID",
  mot.operationsequence     AS "Operation ID",
  mot.title                 AS "Operation Name",
  mot.runrate               AS "Operation Runrate",
  mot.inputquantity         AS "Quantity",

  t.manufacturingrouting    AS "MFGRouting InternalID",
  mr.name                   AS "MFGRouting Name",
  t.billofmaterials         AS "BOM InternalID",

  t.billofmaterialsrevision AS "BOMRevision InternalID"

FROM transaction t
JOIN manufacturingoperationtask mot
  ON mot.workorder = t.id
JOIN manufacturingrouting mr
  ON t.manufacturingrouting = mr.id
LEFT JOIN transactionline tl
  ON tl.transaction = t.id
 AND tl.mainline    = 'T'
LEFT JOIN item i
  ON i.id = tl.item
WHERE
  t.type = 'WorkOrd'
  AND REPLACE(BUILTIN.DF(t.status), ' ', '') IN (
    'WorkOrder:Released',
    'WorkOrder:InProcess',
    'WorkOrder:Planned'
  )
ORDER BY t.tranid, mot.operationsequence