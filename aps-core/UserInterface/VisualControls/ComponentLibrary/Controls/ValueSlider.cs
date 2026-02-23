using System.Windows.Forms;

using PT.Common.Delegates;

namespace PT.ComponentLibrary.Controls;

public partial class ValueSlider : PTBaseControl
{
    private readonly List<Control> m_dontLocalizeThese = new ();

    public ValueSlider()
    {
        InitializeComponent();

        m_dontLocalizeThese.Add(buttonEdit_Value);
    }

    private bool m_eventProcessing;

    private void buttonEdit_Value_ButtonPressed(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs a_e)
    {
        buttonEdit_Value.EditValue = 0;
    }

    public VoidDelegate ValueChangedEvent;

    private void buttonEdit_Value_EditValueChanged(object sender, EventArgs e)
    {
        if (m_eventProcessing)
        {
            return;
        }

        m_eventProcessing = true;

        int editValue = 0;
        try
        {
            editValue = Convert.ToInt32(buttonEdit_Value.EditValue);
        }
        catch (Exception) { }

        editValue = Math.Max(0, editValue);
        editValue = Math.Min(1000, editValue);
        trackBarControl_Main.Value = editValue;
        buttonEdit_Value.EditValue = editValue;

        ValueChangedEvent?.Invoke();

        m_eventProcessing = false;
    }

    private void trackBarControl_Main_EditValueChanged(object sender, EventArgs e)
    {
        if (m_eventProcessing)
        {
            return;
        }

        m_eventProcessing = true;

        buttonEdit_Value.EditValue = trackBarControl_Main.Value;
        ValueChangedEvent?.Invoke();

        m_eventProcessing = false;
    }

    public string LabelText
    {
        get => labelControl_Text.Text;
        set => labelControl_Text.Text = value;
    }

    public int Value
    {
        set => trackBarControl_Main.Value = value;
        get => trackBarControl_Main.Value;
    }

    public int MaxValue
    {
        get => trackBarControl_Main.Properties.Maximum;
        set => trackBarControl_Main.Properties.Maximum = value;
    }

    public int MinValue
    {
        get => trackBarControl_Main.Properties.Minimum;
        set => trackBarControl_Main.Properties.Minimum = value;
    }

    /// <summary>
    /// Can be used to identify the control
    /// </summary>
    public string Key;

    /// <summary>
    /// Value to use when resetting the control
    /// </summary>
    public int DefaultValue;

    /// <summary>
    /// Value to use when reverting the control
    /// </summary>
    private int m_originalValue;

    public void Reset()
    {
        Value = DefaultValue;
    }

    public void Revert()
    {
        Value = m_originalValue;
    }

    private void buttonEdit_Value_Validating(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if ((int)buttonEdit_Value.EditValue > MaxValue || (int)buttonEdit_Value.EditValue < MinValue)
        {
            e.Cancel = true;
        }
    }

    public void SetReadonly()
    {
        trackBarControl_Main.ReadOnly = true;
        buttonEdit_Value.ReadOnly = true;
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeUserControl(this, m_dontLocalizeThese.ToArray());
    }

    /// <summary>
    /// Sets the value and tracks the Original value in case of Revert.
    /// </summary>
    /// <param name="a_newValue"></param>
    public void InitializeValue(int a_newValue)
    {
        m_originalValue = a_newValue;
        Value = a_newValue;
    }
}