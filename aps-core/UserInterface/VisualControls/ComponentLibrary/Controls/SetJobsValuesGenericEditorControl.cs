using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask;

using PT.APSCommon.Extensions;
using PT.UIDefinitions;

namespace PT.ComponentLibrary.Controls;

public partial class SetJobsValuesGenericEditorControl : PTBaseControl
{
    public SetJobsValuesGenericEditorControl()
    {
        InitializeComponent();
        ultraCheckEditor_Set.CheckedChanged += new EventHandler(EditorUsageChanged);
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeUserControl(this);
    }

    private enum EEditorControlType
    {
        Enum,
        Numeric,
        Bool,
        String,
        DateTime,
        List,
        Color
    }

    private EEditorControlType m_controlType;
    private Control m_baseEditorControl;
    private string m_customWarningText;

    public void InitializeControl(string a_name, Type a_type, string a_warning = null, params object[] a_listValues)
    {
        ultraCheckEditor_Set.Text = a_name.Localize();
        if (a_warning != null)
        {
            m_customWarningText = a_warning.Localize();
        }

        Label_Warning.Text = s_keepValueText;

        if (a_type.IsEnum)
        {
            ImageComboBoxEdit editorControl = new ();
            ultraPanel_EditorControl.Controls.Add(editorControl);
            editorControl.Properties.Items.AddEnum(a_type);
            editorControl.AutoSize = false;
            editorControl.SelectedIndex = 0;
            editorControl.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            editorControl.EditValueChanged += new EventHandler(EnumValueChanged);
            m_baseEditorControl = editorControl;
            m_controlType = EEditorControlType.Enum;
        }
        else if (a_type == typeof(bool))
        {
            CheckEdit editorControl = new ();
            ultraPanel_EditorControl.Controls.Add(editorControl);
            editorControl.AutoSize = false;
            editorControl.Checked = false;
            editorControl.Text = "";
            //editorControl.Properties.DefaultAlignment = HorzAlignment.Center;
            editorControl.CheckedChanged += new EventHandler(ultraCheckEditor_Value_CheckedChanged);
            m_baseEditorControl = editorControl;
            m_controlType = EEditorControlType.Bool;
            ultraCheckEditor_Set_CheckedChanged(editorControl, null);
        }
        else if (a_type == typeof(int))
        {
            SpinEdit editorControl = CreateNewNumericEditor();
            editorControl.Properties.Mask.MaskType = MaskType.Numeric;
            editorControl.Properties.EditMask = Formatting.WholeNumberFormat;
            editorControl.ValueChanged += new EventHandler(NumericEditorValueChange);
        }
        else if (a_type == typeof(long))
        {
            SpinEdit editorControl = CreateNewNumericEditor();
            editorControl.Properties.Buttons[0].Visible = false;
            editorControl.Properties.Mask.MaskType = MaskType.Numeric;
            editorControl.Properties.EditMask = Formatting.WholeNumberFormat;
            editorControl.ValueChanged += new EventHandler(NumericEditorValueChange);
        }
        else if (a_type == typeof(double))
        {
            SpinEdit editorControl = CreateNewNumericEditor();
            editorControl.Properties.Buttons[0].Visible = false;
            editorControl.Properties.Mask.MaskType = MaskType.Numeric;
            editorControl.Properties.EditMask = Formatting.NumberFormat;
            editorControl.ValueChanged += new EventHandler(NumericEditorValueChange);
        }
        else if (a_type == typeof(decimal))
        {
            SpinEdit editorControl = CreateNewNumericEditor();
            editorControl.Properties.Buttons[0].Visible = false;
            editorControl.Properties.MinValue = 1;
            editorControl.Properties.MaxValue = int.MaxValue;
            editorControl.Properties.Mask.MaskType = MaskType.Numeric;
            editorControl.Properties.EditMask = Formatting.NumberFormat;
            editorControl.ValueChanged += new EventHandler(NumericEditorValueChange);
        }
        else if (a_type == typeof(string))
        {
            TextEdit editorControl = new ();
            ultraPanel_EditorControl.Controls.Add(editorControl);
            editorControl.AutoSize = false;
            editorControl.Text = "";
            editorControl.EditValueChanged += new EventHandler(TextEditorValueChange);
            m_baseEditorControl = editorControl;
            tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Percent;
            tableLayoutPanel1.ColumnStyles[1].Width = 100;
            tableLayoutPanel1.ColumnStyles[2].SizeType = SizeType.Absolute;
            tableLayoutPanel1.ColumnStyles[2].Width = 1;
            m_controlType = EEditorControlType.String;
        }
        else if (a_type == typeof(List<string>))
        {
            ImageComboBoxEdit editorControl = new ();
            ultraPanel_EditorControl.Controls.Add(editorControl);
            foreach (object item in a_listValues)
            {
                editorControl.Properties.Items.Add(new ImageComboBoxItem(item.ToString(), item));
            }

            editorControl.SelectedIndex = 0;
            editorControl.AutoSize = false;
            editorControl.Properties.TextEditStyle = TextEditStyles.DisableTextEditor;
            m_baseEditorControl = editorControl;
            m_controlType = EEditorControlType.List;
            editorControl.EditValueChanged += new EventHandler(ListValueChanged);
        }
        else if (a_type == typeof(DateTime))
        {
            DatePickerControl editorControl = new ();
            editorControl.LabelText = string.Empty;
            ultraPanel_EditorControl.Controls.Add(editorControl);
            editorControl.Anchor = AnchorStyles.Left;
            editorControl.AutoSize = false;
            editorControl.SelectedDateTime = PTDateTime.UserDateTimeNow;
            m_baseEditorControl = editorControl;
            m_controlType = EEditorControlType.DateTime;
            tableLayoutPanel1.ColumnStyles[1].Width = 300;

            editorControl.TimeChanged += new DatePickerControl.TimeChangedDelegate(DateTimeValueChange);
        }
        else if (a_type == typeof(Color))
        {
            ColorPickEdit editorControl = new ();
            editorControl.ColorChanged += new EventHandler(ColorEditorValueChange);
            m_controlType = EEditorControlType.Color;
            m_baseEditorControl = editorControl;
            ultraPanel_EditorControl.Controls.Add(editorControl);
        }

        //Set base control properties
        m_baseEditorControl.Dock = DockStyle.Fill;
    }

    private SpinEdit CreateNewNumericEditor()
    {
        SpinEdit editorControl = new ();
        ultraPanel_EditorControl.Controls.Add(editorControl);
        editorControl.AutoSize = false;
        editorControl.Value = 0;
        editorControl.Properties.MinValue = 0;
        m_baseEditorControl = editorControl;
        m_controlType = EEditorControlType.Numeric;
        return editorControl;
    }

    private bool m_autoChecking;

    private void AutoCheck()
    {
        m_autoChecking = true;
        ultraCheckEditor_Set.Checked = true;
        m_autoChecking = false;
    }

    public string OverrideName
    {
        get => ultraCheckEditor_Set.Text;
        set => ultraCheckEditor_Set.Text = value;
    }

    public bool ValueSet => ultraCheckEditor_Set.Checked;

    public object Value
    {
        get
        {
            switch (m_controlType)
            {
                case EEditorControlType.Enum:
                    return (m_baseEditorControl as ImageComboBoxEdit).EditValue;
                case EEditorControlType.List:
                    return (m_baseEditorControl as ComboBoxEdit).EditValue;
                case EEditorControlType.Numeric:
                    return (m_baseEditorControl as SpinEdit).Value;
                case EEditorControlType.Bool:
                    return (m_baseEditorControl as CheckEdit).Checked;
                case EEditorControlType.String:
                    return (m_baseEditorControl as TextEdit).Text;
                case EEditorControlType.DateTime:
                    return (m_baseEditorControl as DatePickerControl).SelectedDateTime;
                case EEditorControlType.Color:
                    return (m_baseEditorControl as ColorPickEdit).Color;
                default:
                    return null;
            }
        }
    }

    private static readonly string s_keepValueText = "Keeping current value".Localize();
    private static readonly string s_noFlagText = "Remove {0}".Localize();
    private static readonly string s_flagText = "Set {0}".Localize();
    private static readonly string s_valueSetText = "Set to {0}".Localize();

    private void EditorUsageChanged(object a_sender, EventArgs a_e)
    {
        if (!ultraCheckEditor_Set.Checked)
        {
            Label_Warning.Text = s_keepValueText;
        }
        else if (!string.IsNullOrEmpty(m_customWarningText))
        {
            Label_Warning.Text = m_customWarningText;
        }
        else
        {
            if (m_baseEditorControl is ImageComboBoxEdit)
            {
                EnumValueChanged(m_baseEditorControl, null);
            }

            if (m_baseEditorControl is SpinEdit)
            {
                NumericEditorValueChange(m_baseEditorControl, null);
            }

            if (m_baseEditorControl is DatePickerControl)
            {
                DateTimeValueChange(PTDateTime.MinValue);
            }

            if (m_baseEditorControl is CheckEdit)
            {
                ultraCheckEditor_Set_CheckedChanged(m_baseEditorControl, null);
            }

            if (m_baseEditorControl is ColorPickEdit)
            {
                ColorEditorValueChange(m_baseEditorControl, null);
            }
        }
    }

    private void ultraCheckEditor_Set_CheckedChanged(object sender, EventArgs e)
    {
        if (m_controlType != EEditorControlType.Bool || m_autoChecking)
        {
            return;
        }

        if (ultraCheckEditor_Set.Checked)
        {
            if ((m_baseEditorControl as CheckEdit).Checked)
            {
                Label_Warning.Text = string.Format(s_flagText, ultraCheckEditor_Set.Text);
            }
            else
            {
                Label_Warning.Text = string.Format(s_noFlagText, ultraCheckEditor_Set.Text);
            }
        }
        else
        {
            Label_Warning.Text = s_keepValueText;
        }
    }

    private void EnumValueChanged(object a_sender, EventArgs a_e)
    {
        if (!string.IsNullOrEmpty(m_customWarningText))
        {
            Label_Warning.Text = m_customWarningText;
        }
        else
        {
            ImageComboBoxEdit combo = a_sender as ImageComboBoxEdit;
            Label_Warning.Text = string.Format(s_valueSetText, combo.EditValue);
        }

        AutoCheck();
    }

    private void ListValueChanged(object a_sender, EventArgs a_e)
    {
        if (!string.IsNullOrEmpty(m_customWarningText))
        {
            Label_Warning.Text = m_customWarningText;
        }
        else
        {
            ComboBoxEdit combo = a_sender as ComboBoxEdit;
            Label_Warning.Text = string.Format(s_valueSetText, combo.EditValue);
        }

        AutoCheck();
    }

    private void NumericEditorValueChange(object a_sender, EventArgs a_e)
    {
        if (!string.IsNullOrEmpty(m_customWarningText))
        {
            Label_Warning.Text = m_customWarningText;
        }
        else
        {
            SpinEdit editor = a_sender as SpinEdit;
            Label_Warning.Text = string.Format(s_valueSetText, editor.Value);
        }

        AutoCheck();
    }

    private void TextEditorValueChange(object a_sender, EventArgs a_e)
    {
        if (!string.IsNullOrEmpty(m_customWarningText))
        {
            Label_Warning.Text = m_customWarningText;
        }
        else
        {
            Label_Warning.Text = string.Format(s_valueSetText, ultraCheckEditor_Set.Text);
        }

        AutoCheck();
    }

    private void ColorEditorValueChange(object a_sender, EventArgs a_e)
    {
        if (!string.IsNullOrEmpty(m_customWarningText))
        {
            Label_Warning.Text = m_customWarningText;
        }
        else
        {
            ColorPickEdit editor = a_sender as ColorPickEdit;
            Label_Warning.Text = string.Format(s_valueSetText, editor.Color);
        }

        AutoCheck();
    }

    private void DateTimeValueChange(DateTimeOffset a_newTime)
    {
        if (!string.IsNullOrEmpty(m_customWarningText))
        {
            Label_Warning.Text = m_customWarningText;
        }
        else
        {
            Label_Warning.Text = string.Format(s_flagText, ultraCheckEditor_Set.Text);
        }

        AutoCheck();
    }

    private void ultraCheckEditor_Value_CheckedChanged(object sender, EventArgs e)
    {
        if (m_controlType != EEditorControlType.Bool)
        {
            return;
        }

        if (!string.IsNullOrEmpty(m_customWarningText))
        {
            Label_Warning.Text = m_customWarningText;
        }
        else
        {
            CheckEdit editor = sender as CheckEdit;
            Label_Warning.Text = editor.Checked ? string.Format(s_flagText, ultraCheckEditor_Set.Text) : string.Format(s_noFlagText, ultraCheckEditor_Set.Text);
        }

        AutoCheck();
    }
}