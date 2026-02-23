using System.ComponentModel;
using System.Data;

using PT.Common.Localization;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

/// <summary>
/// Methods in this class use Reflection for setting the Properties of objects created by the ERP interface.
/// </summary>
public class PropertySetter
{
    /// <summary>
    /// Copies the properties from the reader into the transmission.
    /// </summary>
    public static void SetProperties(IDataReader reader, object o)
    {
        Type type = o.GetType();
        int propCount = TypeDescriptor.GetProperties(type).Count;
        int index = 0;
        //Set each property to the column's value
        for (int i = 0; i < propCount; i++)
        {
            PropertyDescriptor pd;

            pd = TypeDescriptor.GetProperties(type).Sort()[i]; //.Sort(GetSortStringArray(type))[i]; //sort by default sort specified in the object
            DisplayAttribute displayAttribute = null;
            if (pd.Attributes != null)
            {
                displayAttribute = (DisplayAttribute)pd.Attributes[typeof(DisplayAttribute)];
            }

            if (!pd.IsReadOnly && pd.IsBrowsable)
            {
                object newValue = null;
                try
                {
                    newValue = reader.GetValue(index); //Can't use 'i' since properties are skipped if read-only!
                    index++;
                    if (newValue != null && newValue.ToString().Trim() != "") //if no value then don't set the property.
                    {
                        ChangeProperty(pd, o, pd.Name, newValue); //TODO Might be faster to check type and get specfic type.	
                    }
                }
                catch (Exception e)
                {
                    string newValueString = "null";
                    Type newValueType = typeof(DBNull);
                    if (newValue != null)
                    {
                        newValueString = newValue.ToString();
                        newValueType = newValue.GetType();
                    }

                    string msg = Localizer.GetErrorString("2601", new object[] { e.Message, pd.Name, pd.PropertyType, newValueType, newValueString });
                    throw new PTObjectBase.PTObjectBaseCreationException(msg, pd.Name, i, pd.PropertyType, newValueString, newValueType);
                }
            }
        }
    }

    /// <summary>
    /// Sets the specified Property in the specifed BaseObject to the specified new value.
    /// Throws an error if the Property is not found.
    /// </summary>
    public static void ChangeProperty(PropertyDescriptor pd, object o, string propertyName, object newValue)
    {
        if (pd.PropertyType.BaseType == typeof(Enum))
        {
            pd.SetValue(o, Enum.Parse(pd.PropertyType, newValue.ToString(), true));
        }
        else if (pd.PropertyType == typeof(bool))
        {
            if (newValue.ToString().ToUpper() == "TRUE" || newValue.ToString().ToUpper() == "T" || newValue.ToString() == "-1" || newValue.ToString() == "1")
            {
                pd.SetValue(o, true);
            }
            else
            {
                pd.SetValue(o, false);
            }
        }
        else if (pd.PropertyType == typeof(string))
        {
            pd.SetValue(o, newValue.ToString()); //Do conversion automatically so users don't have to use specify CStr() each time.
        }
        else if (pd.PropertyType == typeof(DateTime))
        {
            pd.SetValue(o, Convert.ToDateTime(newValue).ToServerTime()); //JMC Remove in the future when we convert in the transmission
        }
        else if (pd.PropertyType == typeof(System.Drawing.Color))
        {
            pd.SetValue(o, System.Drawing.Color.FromName(newValue.ToString()));
        }
        else if (pd.PropertyType == typeof(UniqueStringArrayList))
        {
            pd.SetValue(o, GetUniqueStringArrayList(newValue.ToString()));
        }
        else if (pd.PropertyType == typeof(decimal) && !(newValue is decimal)) //these parses were added for Anspach since we couldn't convert their numeric types
        {
            pd.SetValue(o, decimal.Parse(newValue.ToString()));
        }
        else if (pd.PropertyType == typeof(decimal) && !(newValue is decimal))
        {
            pd.SetValue(o, decimal.Parse(newValue.ToString()));
        }
        else if (pd.PropertyType == typeof(int) && !(newValue is int))
        {
            pd.SetValue(o, int.Parse(newValue.ToString()));
        }
        else if (pd.PropertyType == typeof(long) && !(newValue is long))
        {
            pd.SetValue(o, long.Parse(newValue.ToString()));
        }
        else
        {
            pd.SetValue(o, newValue);
        }
    }

    /// <summary>
    /// Returns a UniqueStringArrayList constructed by splitting the input string at each comma.
    /// </summary>
    /// <param name="commaSeparatedList"></param>
    /// <returns></returns>
    private static UniqueStringArrayList GetUniqueStringArrayList(string commaSeparatedList)
    {
        UniqueStringArrayList arrayList = new ();

        string[] items = commaSeparatedList.Split(",".ToCharArray());
        for (int i = 0; i < items.Length; i++)
        {
            arrayList.Add(items[i]);
        }

        return arrayList;
    }
}