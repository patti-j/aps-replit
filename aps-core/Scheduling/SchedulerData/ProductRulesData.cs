using System.Data;

using PT.Database;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData;

public static class ProductRulesData
{
    public static void PopulateUserFields(this ProductRule a_rule, PtDbDataSet.ProductRulesRow a_row, Dictionary<string, IUserFieldDefinition> a_udfDefinitions, ScenarioDetail a_sd)
    {
        if (a_rule.UserFields != null)
        {
            UserFieldList userFieldList = new(a_rule.UserFields);

            foreach (UserField udf in userFieldList.UserFields.Values)
            {
                if (a_udfDefinitions.TryGetValue(udf.ExternalId, out IUserFieldDefinition udfDef))
                {
                    if (!udfDef.Publish)
                    {
                        continue;
                    }
                    
                    //Add column if not yet added
                    DataColumn col;
                    string userFieldColName = udfDef.ExternalId;
                    if (!a_row.Table.Columns.Contains(userFieldColName))
                    {
                        col = a_row.Table.Columns.Add(userFieldColName, udf.DataType == typeof(TimeSpan) ? typeof(long) : udf.DataType);
                    }
                    else
                    {
                        col = a_row.Table.Columns[userFieldColName];
                    }

                    if (udf.DataValue != null)
                    {
                        a_row[col] = a_rule.GetUserFieldValue(udf, a_sd);
                    }
                }
            }
        }
    }
}