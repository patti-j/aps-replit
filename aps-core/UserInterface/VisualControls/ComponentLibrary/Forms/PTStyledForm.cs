using System.Windows.Forms;

using DevExpress.XtraEditors;

using PT.Common.Debugging;
using PT.Common.Localization;
using PT.PackageDefinitionsUI.Interfaces;
using PT.Transmissions.Interfaces;

namespace PT.ComponentLibrary.Forms;

public partial class PTStyledForm : XtraForm, ILocalizable
{
    public PTStyledForm()
    {
        //Style the form
        //Appearance appearance = new Appearance();
        //m_formManager = appearance.GetStyledFormManager(this, components);
        InitializeComponent();

        if (RuntimeStatus.IsRuntime)
        {
            Icon = s_icon;
            FormBorderEffect = FormBorderEffect.Glow;
            //ActiveGlowColor = PTAppearance.PTBlue;
            //InactiveGlowColor = PTAppearance.PTGrey;

            //Load += new EventHandler(FormLoaded); this base form is never "loaded"

            Shown += new EventHandler(FormShown);
            FormClosing += PTStyledForm_FormClosing;
            UpdateLastAction();
        }
    }

    private void PTStyledForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        FormClosing -= PTStyledForm_FormClosing;
        UpdateLastAction();
    }

    public static void SetBrand(IBrand a_brand)
    {
        s_icon = a_brand.Icon;
        s_theme = a_brand.ActiveTheme;
    }

    public static void SetClientSession(IClientSession a_clientSession)
    {
        m_clientSession = a_clientSession;
    }
    public static void SetInstanceInfo(string a_instanceInfoInstanceName, string a_instanceInfoInstanceVersion)
    {
        m_instanceName = a_instanceInfoInstanceName;
        m_instanceVersion = a_instanceInfoInstanceVersion;
    }

    protected static string m_instanceName;
    protected static string m_instanceVersion;
    private static System.Drawing.Icon s_icon;
    protected static IClientSession m_clientSession;
    protected static IDynamicSkin s_theme;

    /// <summary>
    /// Last action is used by the autoLogout timer found in Mainform.Users.  This, along with LastAction variables from UIBroadcaster and PTBaseControl are used to determine when the last action
    /// from the users was executed and log them off if autoLogOut is turned on and the timeout has elapsed.
    /// </summary>
    private static DateTime s_lastAction = DateTime.MinValue;

    public static DateTime LastAction => s_lastAction;

    private void UpdateLastAction()
    {
        if (!(this is MessageDialog))
        {
            s_lastAction = DateTime.UtcNow;
        }
    }

    protected virtual void FormShown(object a_sender, EventArgs a_e)
    {
        #if DEBUG
        if (Owner == null && Name != "MainForm" && !RuntimeStatus.IsRuntime)
        {
            throw new DebugException($"Form {Name} shown without an owner!");
        }
        #endif

        Localize();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="a_disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool a_disposing)
    {
        if (a_disposing)
        {
            components?.Dispose();
        }

        //Icon?.Dispose();
        base.Dispose(a_disposing);
    }

    public virtual void Localize()
    {
        DebugException.ThrowInTest("This form did not override Localize()");
    }
}