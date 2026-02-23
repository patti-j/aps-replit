using System.Windows.Forms;

using DevExpress.XtraBars.Navigation;

using PT.APSCommon.Extensions;
using PT.Common.Localization;

namespace PT.ComponentLibrary.Features;

public partial class FeatureControlFrame : PTBaseControl, ILocalizable
{
    public FeatureControlFrame()
    {
        InitializeComponent();

        Localize();
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeControlsRecursively(Controls);
    }

    private readonly Dictionary<string, NavigationPage> m_featuresDictionary = new ();

    public void AddFeature(string a_feature, Control a_editor)
    {
        if (m_featuresDictionary.TryGetValue(a_feature, out NavigationPage navPage))
        {
            AddControlToNavPage(a_editor, navPage);
        }
        else
        {
            NavigationPage newFeature = new ();
            newFeature.PageText = a_feature.Localize();
            newFeature.Name = a_feature;
            AddControlToNavPage(a_editor, newFeature);
            featuresNavFrame.Pages.Add(newFeature);
            AddAccordionElement(a_feature);
        }
    }

    public void AddControlToNavPage(Control a_control, NavigationPage a_page)
    {
        a_page.Controls.Add(a_control);
        a_control.Dock = DockStyle.Fill;
    }

    private void accordionControl_ElementClick(object sender, ElementClickEventArgs e)
    {
        if (e.Element.Level == 1)
        {
            int pageIndex = (int)e.Element.Tag;
            featuresNavFrame.SelectedPageIndex = pageIndex;
        }
    }

    private void AddAccordionElement(string a_elementKey)
    {
        //Add an element
        AccordionControlElement newElement = new ()
        {
            Style = ElementStyle.Item,
            Name = a_elementKey,
            Text = a_elementKey.Localize(),
            Height = FontHeight,
            Tag = featuresNavFrame.Pages.Count - 1
        };

        accordionControl.BeginUpdate();
        accordionGroupElement.Elements.Add(newElement);
        accordionControl.EndUpdate();
    }
}