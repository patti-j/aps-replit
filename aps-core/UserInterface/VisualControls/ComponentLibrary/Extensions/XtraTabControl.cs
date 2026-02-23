using DevExpress.XtraBars.Navigation;
using DevExpress.XtraTab;

namespace PT.ComponentLibrary.Extensions;

public static class TabControl
{
    public static XtraTabPage TabPageByName(this XtraTabControl a_tabControl, string a_name)
    {
        foreach (XtraTabPage tabPage in a_tabControl.TabPages)
        {
            if (tabPage.Name == a_name)
            {
                return tabPage;
            }
        }

        return null;
    }

    public static XtraTabPage AddTabPage(this XtraTabControl a_tabControl, string a_name, string a_text)
    {
        XtraTabPage newPage = new ();
        newPage.Name = a_name;
        newPage.Text = a_text;
        a_tabControl.TabPages.Add(newPage);
        return newPage;
    }

    public static NavigationPageBase TabPageByName(this TabPane a_tabControl, string a_name)
    {
        foreach (NavigationPageBase tabPage in a_tabControl.Pages)
        {
            if (tabPage.Name == a_name)
            {
                return tabPage;
            }
        }

        return null;
    }

    public static NavigationPageBase PageByName(this NavigationPane a_navigationPane, string a_name)
    {
        foreach (NavigationPageBase tabPage in a_navigationPane.Pages)
        {
            if (tabPage.Name == a_name)
            {
                return tabPage;
            }
        }

        return null;
    }
}