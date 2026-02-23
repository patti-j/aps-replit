namespace PT.APSCommon.Extensions;

public static class GenericExtentions
{
    public static List<T> Clone<T>(this List<T> a_listToClone) where T : ICloneable
    {
        return a_listToClone.Select(a_item => (T)a_item.Clone()).ToList();
    }
}