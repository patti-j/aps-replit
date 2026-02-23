namespace PT.Common.ObjectHelpers;

public static class HashCodeHelper
{
    public static int GetHashCode(params object[] a_objects)
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)
            foreach (object o in a_objects)
            {
                hash = hash * 23 + o.GetHashCode();
            }

            return hash;
        }
    }
}