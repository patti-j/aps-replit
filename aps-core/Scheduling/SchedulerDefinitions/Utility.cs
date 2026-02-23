using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Various utility functions.
/// </summary>
public class Utilities
{
    public static string GetArrayString(BaseId[] array)
    {
        object[] objectArray = new object[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            objectArray[i] = array[i].ToString();
        }

        return GetArrayString(objectArray);
    }

    public static string GetArrayString(object[] array)
    {
        string s = "";
        for (int i = 0; i < array.Length; i++)
        {
            if (i > 1)
            {
                s = s + ",";
            }

            s = s + array[i];
        }

        return s;
    }
}