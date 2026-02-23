namespace PT.ERPTransmissions { }

namespace PT.ERPTransmissions
{
    public partial class SalesOrderTDataSet
    {
        public void DeleteObject(string a_soExternalId)
        {
            if (SalesOrder.Rows.Count == 0)
            {
                return; //nothing
            }

            for (int i = 0; i < SalesOrder.Count; i++)
            {
                SalesOrderRow dataRow = (SalesOrderRow)SalesOrder.Rows[i];
                if ((string)dataRow[SalesOrder.ExternalIdColumn] == a_soExternalId)
                {
                    SalesOrder.RemoveSalesOrderRow(dataRow);
                    break;
                }
            }
        }

        public void DeleteLineObject(string a_soExternalId)
        {
            if (SalesOrderLine.Rows.Count == 0)
            {
                return; //nothing
            }

            for (int i = 0; i < SalesOrderLine.Count; i++)
            {
                SalesOrderLineRow dataRow = (SalesOrderLineRow)SalesOrderLine.Rows[i];
                if ((string)dataRow[SalesOrderLine.SalesOrderExternalIdColumn] == a_soExternalId)
                {
                    SalesOrderLine.RemoveSalesOrderLineRow(dataRow);
                    break;
                }
            }
        }

        public void DeleteDistObject(string a_soExternalId)
        {
            if (SalesOrderLineDist.Rows.Count == 0)
            {
                return; //nothing
            }

            for (int i = 0; i < SalesOrderLineDist.Count; i++)
            {
                SalesOrderLineDistRow dataRow = (SalesOrderLineDistRow)SalesOrderLineDist.Rows[i];
                if ((string)dataRow[SalesOrderLineDist.SalesOrderExternalIdColumn] == a_soExternalId)
                {
                    SalesOrderLineDist.RemoveSalesOrderLineDistRow(dataRow);
                    break;
                }
            }
        }
    }
}

namespace PT.ERPTransmissions
{
    public partial class SalesOrderTDataSet { }
}