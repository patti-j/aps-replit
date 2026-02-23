using DevExpress.Utils;
using DevExpress.XtraEditors;

using PT.APSCommon.Extensions;
using PT.APSCommon.Windows.Extensions;
using PT.Common.Localization;

namespace PT.ComponentLibrary.Controls;

public partial class HelpLinkLabel : HyperlinkLabelControl, ILocalizable
{
    public HelpLinkLabel()
    {
        InitializeComponent();
        HyperlinkClick += new HyperlinkClickEventHandler(LinkClicked);
    }

    private void LinkClicked(object a_sender, HyperlinkClickEventArgs a_e)
    {
        if (string.IsNullOrWhiteSpace(SpecifcWebPage))
        {
            HelpTopic.ShowHelp();
            LinkVisited = true;
        }
        else
        {
            APSCommon.Windows.WebUtility.OpenWebPage(m_specificWebPage);
        }
    }

    private string m_helpTopic;

    public string HelpTopic
    {
        get => m_helpTopic;
        set => m_helpTopic = value;
    }

    private string m_specificWebPage;

    public string SpecifcWebPage
    {
        get => m_specificWebPage;
        set => m_specificWebPage = value;
    }

    public void Localize()
    {
        Text = Text.Localize();
    }
}