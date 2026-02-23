SELECT
  i.totalquantityonhand,
  i.stockunit,
  i.itemid,
  i.itemtype,
  i.fullname,
  i.description,
  i.id AS "Item InternalID",
  i.isinactive
FROM item i
WHERE i.isinactive = 'F'
ORDER BY i.itemid