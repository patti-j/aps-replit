using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using PT.APSCommon.Extensions;
using PT.APSCommon.Serialization;
using PT.APSCommon.Windows.Extensions;
using PT.PackageDefinitions;
using PT.UIDefinitions.Interfaces;

namespace PT.ComponentLibrary.Forms;

/// <summary>
/// Summary description for BaseResizableForm.
/// </summary>
public class BaseResizableForm : PTStyledForm
{
    /// <summary>
    /// Required designer variable.
    /// To implement:
    /// 1. Call in the child form's constructor: base.SetDefaultLayout(ref DefaultLayout);
    /// 2. Declare this variable in the child form: internal static FormLayout DefaultLayout;
    /// OR
    /// 1. If the a BaseResizableForm is created as an "on the fly" form use the constructor with the FormLayout which was declared outside the form as a static to
    /// remember the form size info between form openings.
    /// </summary>
    private Container components;

    private readonly FormLayout m_currentLayout;
    private static Dictionary<string, FormLayout> s_formLayoutDict = new ();
    private static Form s_parentForm;
    private readonly bool m_allowResize = true;
    protected static IMessageProvider m_messageProvider;
    private readonly string m_helpTopic;
    public bool FormLayoutLoaded;
    protected bool m_loading;

    /// <summary>
    /// Designer Only
    /// </summary>
    public BaseResizableForm()
    {
        InitializeComponent();
        #if TEST
            DebugException.ThrowInTest("Control is using a designer only constructor");
        #endif
    }

    protected BaseResizableForm(bool a_allowResize)
    {
        InitializeComponent();
        m_allowResize = a_allowResize;

        Load += new EventHandler(BaseResizableForm_Load);
        HelpRequested += new HelpEventHandler(HelpRequestedByUser);
        FormClosing += new FormClosingEventHandler(FormClosingHandler);
    }

    protected virtual void FormClosingHandler(object a_sender, FormClosingEventArgs a_e)
    {
        if (m_loading)
        {
            a_e.Cancel = true;
        }
    }

    private const string s_formPrefix = "formLayout_";

    /// <summary>
    /// Loads a predefined form layout. If one doesn't exist, it will be created.
    /// </summary>
    public BaseResizableForm(string a_layoutId, bool a_allowResize = true) : this(a_allowResize)
    {
        m_helpTopic = a_layoutId;

        if (s_userSettingsManager == null)
        {
            //The UI has not been fully loaded, but a message needs to be shown. Ignore the layout for this message.
            m_currentLayout = new FormLayout(a_layoutId);
            return;
        }

        SettingData settingsData = s_userSettingsManager.LoadSetting(s_formPrefix + a_layoutId);

        if (settingsData != null)
        {
            settingsData.SettingsGroup = SettingGroupConstants.LayoutSettingsGroup;
            settingsData.SettingsGroupCategory = SettingGroupConstants.FormLayouts;
            settingsData.SettingCaption = a_layoutId + " " + "layout".Localize();

            using (BinaryMemoryReader reader = new (settingsData.Data))
            {
                m_currentLayout = new FormLayout(reader);
            }

            FormLayoutLoaded = true;
        }
        else
        {
            using (BinaryMemoryWriter writer = new ())
            {
                m_currentLayout = new FormLayout(a_layoutId);
                m_currentLayout.Serialize(writer);

                byte[] bytes = writer.GetBuffer();

                SettingData settingData = new (s_formPrefix + a_layoutId, bytes, "Form Positions", SettingGroupConstants.LayoutSettingsGroup, SettingGroupConstants.FormLayouts, a_layoutId + " " + "layout".Localize());
                s_userSettingsManager.SaveSetting(settingData, false);
            }

            FormLayoutLoaded = false;
        }
    }

    private void HelpRequestedByUser(object a_sender, HelpEventArgs a_hlpevent)
    {
        m_helpTopic.ShowHelp();
        a_hlpevent.Handled = true;
    }

    public new void Show()
    {
        if (!Visible && Owner != null)
        {
            Show(Owner);
        }
        else if (!Visible && s_parentForm != null)
        {
            Show(s_parentForm);
        }
        else
        {
            base.Show();
        }
    }

    public new DialogResult ShowDialog()
    {
        if (Owner == null && s_parentForm != null)
        {
            return ShowDialog(s_parentForm);
        }

        return base.ShowDialog();
    }

    public static void SetMessageProvider(IMessageProvider a_messageProvider)
    {
        m_messageProvider = a_messageProvider;
    }

    protected FormLayout CurrentFormLayout => m_currentLayout;

    public static List<FormLayout> GetModifiedLayouts()
    {
        List<FormLayout> modifiedLayouts = new ();
        foreach (FormLayout layout in modifiedLayouts)
        {
            if (layout.Modified)
            {
                modifiedLayouts.Add(layout);
            }
        }

        return modifiedLayouts;
    }

    private static IWorkspaceInfo s_userSettingsManager;

    public static void SetSettingsManager(IWorkspaceInfo a_userSettingsManager)
    {
        s_userSettingsManager = a_userSettingsManager;
    }

    public static void SetDefaultParentForm(Form a_parent)
    {
        s_parentForm = a_parent;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool a_disposing)
    {
        if (a_disposing)
        {
            components?.Dispose();
        }

        StoreLayout();

        base.Dispose(a_disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.Size = new System.Drawing.Size(300, 300);
        this.Text = "BaseResizableForm";
    }
    #endregion

    private void AutoSizeForm()
    {
        LoadFormLayout(this, m_currentLayout.LayoutId, m_allowResize);
    }

    /// <summary>
    /// Store the current layout back to the derived form's layout object if there was one set.
    /// </summary>
    protected void StoreLayout()
    {
        Update(m_currentLayout, this);
        //Store the current layout if the settings manager has been established.
        if (s_userSettingsManager != null)
        {
            using (BinaryMemoryWriter writer = new ())
            {
                m_currentLayout.Serialize(writer);
                byte[] bytes = writer.GetBuffer();

                SettingData settingData = new (s_formPrefix + m_currentLayout.LayoutId, bytes, "Form Positions", SettingGroupConstants.LayoutSettingsGroup, SettingGroupConstants.FormLayouts, m_currentLayout.LayoutId + " " + "layout".Localize());
                s_userSettingsManager.SaveSetting(settingData, false);
            }
        }
    }

    public static void Update(FormLayout a_layout, Form a_form)
    {
        if (a_layout == null || a_form.WindowState == FormWindowState.Minimized)
        {
            return; //We cannot get accurate size and location for this window.
        }

        a_layout.Maximized = a_form.WindowState == FormWindowState.Maximized;

        //Only reset the dimensions if the form is in a normal mode
        if (a_form.WindowState == FormWindowState.Normal || (a_layout.Left == 0 && a_layout.Height == 0))
        {
            a_layout.SetLocation(a_form.Left, a_form.Top);
            a_layout.SetSize(a_form.Width, a_form.Height);
        }
    }

    public static void LoadFormLayout(Form a_form, string a_layoutId, bool a_allowResize)
    {
        //Use the saved layout as long as the form is fully on the screen
        //StartPosition = FormStartPosition.CenterParent;

        SettingData settingsData = s_userSettingsManager?.LoadSetting(s_formPrefix + a_layoutId);

        FormLayout layout;
        if (settingsData != null)
        {
            using (BinaryMemoryReader reader = new (settingsData.Data))
            {
                layout = new FormLayout(reader);
            }

            LoadFormLayout(a_form, layout, a_allowResize);
        }
    }

    public static void LoadFormLayout(Form a_form, FormLayout a_layout, bool a_allowResize)
    {
        //Use the saved layout as long as the form is fully on the screen
        //StartPosition = FormStartPosition.CenterParent;

        if (a_layout != null && a_layout.Width != 0 && a_layout.Height != 0) // defaultLayout Width and Height are zero the first time they're openned.
        {
            Rectangle[] screens = Screen.AllScreens.Select(s => s.WorkingArea).ToArray();
            a_layout.ValidateWindowOnScreen(Screen.PrimaryScreen.WorkingArea, screens);

            if (!a_layout.Maximized)
            {
                //Sometimes a maximized form will reset its state to Maximized when changing size or position. So we need to set it to normal before and after each change.
                a_form.WindowState = FormWindowState.Normal;
                if (a_allowResize)
                {
                    a_form.Size = new Size(a_layout.Width, a_layout.Height);
                    a_form.WindowState = FormWindowState.Normal;
                }

                a_form.Location = new Point(a_layout.Left, a_layout.Top);
                a_form.WindowState = FormWindowState.Normal;
            }
            else
            {
                if (a_allowResize)
                {
                    a_form.Size = new Size(a_layout.Width, a_layout.Height);
                }

                a_form.Location = new Point(a_layout.Left, a_layout.Top);
                a_form.WindowState = FormWindowState.Maximized;
            }
        }
        //TODO: The inherited control should set the owner during consturction
    }

    private void BaseResizableForm_Load(object sender, EventArgs e)
    {
        if (!RuntimeStatus.IsRuntime) //DesignMode only works in forms, not usercontrols!  Use this instead.
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new VoidDelegate(AutoSizeForm));
        }
        else
        {
            AutoSizeForm();
        }
    }

    private delegate void VoidDelegate();
}