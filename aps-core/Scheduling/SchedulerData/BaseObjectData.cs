using PT.Scheduler;
using PT.SchedulerDefinitions;
using System.Data;

namespace PT.SchedulerData;

public static class BaseObjectData
{
    public static void PtDbPopulateUserFields(this BaseObject a_base, DataRow a_row, ScenarioDetail a_sd, Dictionary<string, IUserFieldDefinition> a_udfDefinitions)
    {
        if (a_base.UserFields != null)
        {
            a_row.Table.BeginLoadData();
            
            foreach (UserField udf in a_base.UserFields.UserFields.Values)
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
                        a_row[col] = GetUserFieldValue(udf, a_sd);
                    }
                }
            }

            a_row.Table.EndLoadData();
        }
    }

    public static object GetUserFieldValue(UserField a_userField, ScenarioDetail a_sd)
    {
        object v = null;

        if (a_userField.DataType == typeof(DateTime))
        {
            v = SQLServerConversions.GetValidDateTime((DateTime)a_userField.DataValue);
        }
        else if (a_userField.DataType == typeof(double))
        {
            v = a_sd.ScenarioOptions.RoundQty((double)a_userField.DataValue);
        }
        else if (a_userField.DataType == typeof(decimal))
        {
            v = a_sd.ScenarioOptions.RoundQty((decimal)a_userField.DataValue);
        }
        else if (a_userField.DataType == typeof(TimeSpan))
        {
            v = ((TimeSpan)a_userField.DataValue).Ticks;
        }
        else
        {
            v = a_userField.DataValue;
        }

        return v;
    }
}