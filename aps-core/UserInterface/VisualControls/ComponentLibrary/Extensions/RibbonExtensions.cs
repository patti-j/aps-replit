using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;

namespace PT.ComponentLibrary.Extensions;

public static class RibbonExtensions
{
    public static BarItem GetByKey(this RibbonControl a_ribbon, string a_key)
    {
        return a_ribbon.Items.GetByKey(a_key);
    }

    public static BarItem GetByKey(this BarItems a_itemCollections, string a_key)
    {
        foreach (BarItem item in a_itemCollections)
        {
            if (item.Name == a_key)
            {
                return item;
            }
        }

        return null;
    }

    public static List<BarItem> GetItemsWithPrefix(this BarItems a_itemCollections, string a_key)
    {
        List<BarItem> barItems = new ();
        foreach (BarItem barItem in a_itemCollections)
        {
            if (barItem.Name.StartsWith(a_key))
            {
                barItems.Add(barItem);
            }
        }

        return barItems;
    }
}