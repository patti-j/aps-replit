using System.Windows.Forms;

using DevExpress.Utils.Extensions;
using DevExpress.XtraBars.Navigation;

using PT.APSCommon.Extensions;
using PT.ComponentLibrary.Extensions;

namespace PT.ComponentLibrary.Navigation;

public partial class PTNavigationFrame : PTBaseControl
{
    public PTNavigationFrame()
    {
        InitializeComponent();

        if (!accordionControl1.IsHandleCreated)
        {
            accordionControl1.EnsureCreateHandle();
        }
    }

    private readonly Dictionary<string, Dictionary<string, NavigationPage>> m_groupDict = new ();

    public void AddToPage(string a_group, string a_permissionKey, Control a_control)
    {
        if (m_groupDict.TryGetValue(a_group, out Dictionary<string, NavigationPage> categoryDict))
        {
            if (categoryDict.TryGetValue(a_permissionKey, out NavigationPage layoutPage))
            {
                layoutPage.Controls.Add(a_control);
            }
            else
            {
                NavigationPage newPage = new ();
                newPage.AutoScroll = true;
                navigationFrame1.Pages.Add(newPage);
                categoryDict.Add(a_permissionKey, newPage);
                a_control.Dock = DockStyle.Fill;
                newPage.Controls.Add(a_control);
            }
        }
        else
        {
            //New group, category, and page
            NavigationPage newPage = new ();
            newPage.AutoScroll = true;
            navigationFrame1.Pages.Add(newPage);
            Dictionary<string, NavigationPage> newCategoryDict = new ();
            newCategoryDict.Add(a_permissionKey, newPage);
            m_groupDict.Add(a_group, newCategoryDict);
            a_control.Dock = DockStyle.Fill;
            newPage.Controls.Add(a_control);
        }
    }

    public void GenerateMenu(bool a_sorted)
    {
        bool noGroup = m_groupDict.Count == 1;

        foreach (KeyValuePair<string, Dictionary<string, NavigationPage>> groupMapping in m_groupDict)
        {
            //First add a new group
            AccordionControlElement group = new ()
            {
                Style = ElementStyle.Group,
                Name = groupMapping.Key,
                Expanded = true,
                //Height = FontHeight,
                Text = groupMapping.Key.Localize()
            };

            accordionControl1.Elements.Add(group);

            accordionControl1.ShowGroupExpandButtons = !noGroup;
            accordionControl1.ExpandGroupOnHeaderClick = !noGroup;

            foreach (KeyValuePair<string, NavigationPage> pageMapping in groupMapping.Value)
            {
                //Add a category element
                AccordionControlElement newCategory = new ()
                {
                    Style = ElementStyle.Item,
                    Name = pageMapping.Key,
                    Text = pageMapping.Key.Localize(),
                    //Height = FontHeight,
                    Tag = pageMapping.Value
                };

                group.Elements.Add(newCategory);
            }
        }

        navigationFrame1.SelectedPageIndex = 0;
        accordionControl1.SelectFirstItemElement();
    }

    public void ClearData()
    {
        for (int i = accordionControl1.Elements.Count - 1; i >= 0; i--)
        {
            AccordionControlElement groupElem = accordionControl1.Elements[i];
            for (int c = groupElem.Elements.Count - 1; c >= 0; c--)
            {
                accordionControl1.Elements.Remove(groupElem.Elements[c]);
            }

            accordionControl1.Elements.Remove(groupElem);
        }

        navigationFrame1.Pages.Clear();
        m_groupDict.Clear();
    }

    private void accordionControl1_ElementClick(object sender, ElementClickEventArgs e)
    {
        if (e.Element.Level == 1)
        {
            NavigationPage page = (NavigationPage)e.Element.Tag;
            navigationFrame1.SelectedPage = page;
        }
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeControlsRecursively(Controls);
    }
}