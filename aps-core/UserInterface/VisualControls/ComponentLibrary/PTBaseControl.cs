using System.Windows.Forms;

using DevExpress.XtraEditors;

using PT.APSCommon.Extensions;
using PT.APSCommon.Windows.Extensions;
using PT.Common.Debugging;
using PT.Common.Localization;
using PT.ComponentLibrary.Controls;
using PT.PackageDefinitionsUI.Interfaces;
using PT.UIDefinitions;
using PT.UIDefinitions.Interfaces;

namespace PT.ComponentLibrary;

public partial class PTBaseControl : XtraUserControl, ILocalizable
{
    protected static IMessageProvider m_messageProvider;
    protected static IExceptionManager m_exceptionManager;
    protected static IDynamicSkin m_theme;
    private NoDataSplash m_noDataSplash;
    protected string m_helpKey;

    public PTBaseControl()
    {
        InitializeComponent();
        if (RuntimeStatus.IsRuntime)
        {
            Enter += SetLastAction;
            MultiLevelHourglass.UseWaitCursor += MultiLevelHourglassOnUseWaitCursor;
            HandleCreated += InternalHandleCreated; //Register localization
            HelpRequested += OnHelpRequested;
        }
    }

    private void InternalHandleCreated(object a_sender, EventArgs a_args)
    {
        Localizer.RegisterLocalizableControl(this);
        HandleCreated -= InternalHandleCreated;
    }

    /// <summary>
    /// Last action is used by the autoLogout timer found in Mainform.Users.  This, along with LastAction variables from UIBroadcaster and PTStyledForm are used to determine when the last action
    /// from the users was executed and log them off if autoLogOut is turned on and the timeout has elapsed.
    /// </summary>
    private static DateTime s_lastAction = DateTime.MinValue;

    public static DateTime LastAction => s_lastAction;

    private void SetLastAction(object a_sender, EventArgs a_e)
    {
        s_lastAction = DateTime.UtcNow;
    }

    private void MultiLevelHourglassOnUseWaitCursor(bool a_useWaitCursor)
    {
        UseWaitCursor = a_useWaitCursor;
    }

    public static void SetMessageProvider(IMessageProvider a_messageProvider)
    {
        m_messageProvider = a_messageProvider;
    }

    public static void SetExceptionManager(IExceptionManager a_exceptionManager)
    {
        m_exceptionManager = a_exceptionManager;
    }

    public static void SetSkin(IDynamicSkin a_skin)
    {
        if (m_theme != null)
        {
            m_theme.ThemeChanged -= ThemeOnThemeChanged;
        }

        m_theme = a_skin;
        m_theme.ThemeChanged += ThemeOnThemeChanged;
    }

    protected static event Action ThemeChanged;

    protected static void ThemeOnThemeChanged()
    {
        ThemeChanged?.Invoke();
    }

    public virtual void Localize()
    {
        UILocalizationHelper.LocalizeUserControl(this);
    }

    protected void ToggleNoDataSplash(bool a_hasData, bool a_overrideText, string a_text)
    {
        if (IsHandleCreated)
        {
            if (m_noDataSplash == null)
            {
                Invoke(new Action(GenerateNoDataSplash));
            }

            if (a_overrideText)
            {
                m_noDataSplash.OverrideText(a_text);
            }
            else
            {
                if (!string.IsNullOrEmpty(a_text))
                {
                    m_noDataSplash.SetLabelObjectText(a_text);
                }
            }

            if (a_hasData)
            {
                m_noDataSplash.HideSplash();
            }
            else
            {
                m_noDataSplash.ShowSplash();
            }

            m_noDataSplash.RefreshSplash();
        }
    }

    private void GenerateNoDataSplash()
    {
        m_noDataSplash = new NoDataSplash();
        m_noDataSplash.Visible = false;
        m_noDataSplash.Dock = DockStyle.Fill;
        Controls.Add(m_noDataSplash);
        m_noDataSplash.BringToFront();
        m_noDataSplash.Refresh();
    }

    private void OnHelpRequested(object a_sender, HelpEventArgs a_helpEvent)
    {
        OpenPtHelp();
    }

    protected void OpenPtHelp()
    {
        if (!string.IsNullOrEmpty(m_helpKey))
        {
            m_helpKey.ShowHelp();
            return;
        }

        if (Parent != null)
        {
            Control parent = Parent;
            while (parent != null)
            {
                if (parent is PTBaseControl baseControl)
                {
                    baseControl.OpenPtHelp();
                    return;
                }

                parent = parent.Parent;
            }
        }

        DebugException.ThrowInDebug("Help requested but help key was not specified");
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="a_disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool a_disposing)
    {
        if (a_disposing && components != null)
        {
            components.Dispose();
        }

        MultiLevelHourglass.UseWaitCursor -= MultiLevelHourglassOnUseWaitCursor;
        if (m_theme != null)
        {
            m_theme.ThemeChanged -= ThemeOnThemeChanged;
        }

        base.Dispose(a_disposing);
    }
}