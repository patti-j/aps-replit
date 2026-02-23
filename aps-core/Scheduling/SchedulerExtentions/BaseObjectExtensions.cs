using PT.Scheduler;

namespace PT.SchedulerExtensions;

public static class BaseObjectExtensions
{
    /// <summary>
    /// method for getting value of a userfield. If userfield does not exist, it returns false and o_value
    /// will be the default value of the data type.
    /// </summary>
    public static bool GetUserFieldValue<T>(this BaseObject a_object, string a_fieldName, out T o_value)
    {
        object ufVal = a_object.UserFields?.Find(a_fieldName)?.DataValue;
        if (ufVal is T val)
        {
            o_value = val;
            return true;
        }

        o_value = default(T);
        return false;
    }
}