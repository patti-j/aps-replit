using DevExpress.XtraEditors.Controls;
using DevExpress.XtraLayout.Utils;

using PT.APSCommon.Extensions;

namespace PT.ComponentLibrary.Controls;

/// <summary>
/// Summary description for DatePickerControl.
/// </summary>
public partial class DatePickerControl : PTBaseControl
{
    #region Construction
    public DatePickerControl()
    {
        // This call is required by the Windows.Forms Form Designer.
        InitializeComponent();

        //Bug 20387. For fixing Invisible or disabled control cannot be activated (Parameter 'value')
        layoutControl_DatePicker.OptionsFocus.ActivateSelectedControlOnGotFocus = false;

        if (RuntimeStatus.IsRuntime)
        {
            simpleLabelItem_Text.Visibility = LayoutVisibility.Never; //default hidden unless the value is set

            //this.ultraTimePicker.FormatString = UIDefinitions.Appearance.TimeFormat;
            //this.ultraTimePicker.MaskInput = UIDefinitions.Appearance.TimeMask;
            //this.ultraDatePicker.FormatString = UIDefinitions.Appearance.DateFormat;
            //this.ultraDatePicker.MaskInput = UIDefinitions.Appearance.DateMask;
            //this.ultraTimePicker.DropDownButtonDisplayStyle = ButtonDisplayStyle.Never;
            //m_dateEditor.Properties.DisplayFormat.FormatString = Appearance.DateFormat;
            //m_dateEditor.Properties.EditFormat.FormatString = Appearance.DateFormat;
            //m_dateEditor.Properties.EditMask = Appearance.DateMask;
            //m_timeEditor.Properties.DisplayFormat.FormatString = Appearance.TimeFormat;
            //m_timeEditor.Properties.EditFormat.FormatString = Appearance.TimeFormat;
            //m_timeEditor.Properties.EditMask = Appearance.TimeMask;
            m_dateEditor.ButtonPressed += new ButtonPressedEventHandler(DateEditButtonPressed);
            m_timeEditor.ButtonPressed += new ButtonPressedEventHandler(DateEditButtonPressed);
            m_dateEditor.DateTime = PTDateTime.MinValue.ToDateTime();
            m_timeEditor.Time = PTDateTime.MinValue.ToDateTime();
            m_dateEditor.Properties.NullDateCalendarValue = PTDateTime.UserDateTimeNow.ToDateTime();
            m_dateEditor.Properties.NullDate = PTDateTime.MinValue.ToDateTime();
            m_dateEditor.Click += new EventHandler(DateTimeClicked);
            EnabledChanged += new EventHandler(EnabledChangedHandler);
            //TODO: V12: This was commented out, do we still need this functionality?
            //foreach (EditorButton button in m_dateEditor.Properties.Buttons)
            //{
            //    button.Appearance.ForeColor = Skins.DefaultSkin.ForeColor;
            //    button.AppearanceDisabled.ForeColor = Skins.DefaultSkin.ForeColorDisabled;
            //}
            //foreach (EditorButton button in m_timeEditor.Properties.Buttons)
            //{
            //    button.Appearance.ForeColor = Skins.DefaultSkin.ForeColor;
            //    button.AppearanceDisabled.ForeColor = Skins.DefaultSkin.ForeColorDisabled;
            //}
        }
    }

    private void EnabledChangedHandler(object a_sender, EventArgs a_e)
    {
        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        //Only show buttons if they are clickable.
        m_dateEditor.Properties.Buttons[0].Visible = !ReadOnly;
        m_dateEditor.Properties.Buttons[1].Visible = !ReadOnly;
        m_timeEditor.Properties.Buttons[0].Visible = !ReadOnly;
        m_timeEditor.Properties.Buttons[1].Visible = !ReadOnly;
        m_timeEditor.Properties.Buttons[2].Visible = !ReadOnly;
    }

    public void ChangeLabelVisibility(bool a_visible)
    {
        simpleLabelItem_Text.Visibility = a_visible ? LayoutVisibility.Always : LayoutVisibility.Never;
    }

    public string LabelText
    {
        get => simpleLabelItem_Text.Text;
        set
        {
            simpleLabelItem_Text.Text = value;

            if (value.Contains("Label:") || string.IsNullOrWhiteSpace(value))
            {
                simpleLabelItem_Text.Visibility = LayoutVisibility.Never;
            }
            else
            {
                simpleLabelItem_Text.Visibility = LayoutVisibility.Always;
            }
        }
    }

    private void DateEditButtonPressed(object a_sender, ButtonPressedEventArgs a_e)
    {
        switch ((string)a_e.Button.Tag)
        {
            case "Today":
                m_dateEditor.DateTime = PTDateTime.LocalDateTimeNow.ToDateTime().Date;
                break;
            case "Now":
                m_timeEditor.Time = PTDateTime.LocalDateTimeNow.ToDateTime();
                break;
            case "Clear":
                m_timeEditor.Time = PTDateTime.UserDateTimeNow.ToDateTime().Date;
                break;
        }
    }
    #endregion

    private bool m_readOnly;

    public bool ReadOnly
    {
        get => m_readOnly;
        set
        {
            m_readOnly = value;
            m_dateEditor.ReadOnly = value;
            m_timeEditor.ReadOnly = value;
            UpdateButtonVisibility();
        }
    }

    public new bool Enabled
    {
        set => ReadOnly = !value;
    }

    #region Events
    public delegate void TimeChangedDelegate(DateTimeOffset a_newDateTime);

    public event TimeChangedDelegate TimeChanged;

    public delegate void TimeChangingDelegate(DateTimeOffset a_oldDateTime, DateTimeOffset a_newDateTime, ref bool a_cancel);

    public event TimeChangingDelegate TimeChanging;

    private void m_timeEditor_EditValueChanging(object sender, ChangingEventArgs e)
    {
        if (TimeChanging != null)
        {
            bool cancel = false;
            if (m_dateEditor.EditValue == null)
            {
                TimeChanging(((DateTime)e.OldValue).ToDisplayTime(), PTDateTime.MinValue, ref cancel);
            }
            else
            {
                TimeChanging((m_dateEditor.DateTime.Date + ((DateTime)e.OldValue).TimeOfDay).ToDisplayTime(), (m_dateEditor.DateTime.Date + ((DateTime)e.NewValue).TimeOfDay).ToDisplayTime(), ref cancel);
            }

            e.Cancel = cancel;
        }
    }

    private void m_dateEditor_EditValueChanging(object sender, ChangingEventArgs e)
    {
        if (e.NewValue == null)
        {
            return;
        }

        if (TimeChanging != null)
        {
            bool cancel = false;
            if (m_dateEditor.EditValue == null)
            {
                TimeChanging(((DateTime)e.OldValue).ToDisplayTime(), PTDateTime.MinValue, ref cancel);
            }
            else
            {
                TimeChanging((((DateTime)e.OldValue).Date + m_timeEditor.Time.TimeOfDay).ToDisplayTime(), (((DateTime)e.NewValue).Date + m_timeEditor.Time.TimeOfDay).ToDisplayTime(), ref cancel);
            }

            e.Cancel = cancel;
        }

        RestrictValueChange((DateTime)e.NewValue);
    }

    private void m_timeEditor_EditValueChanged(object sender, EventArgs e)
    {
        TimeChanged?.Invoke(SelectedDateTime);
    }
    #endregion Events

    #region Public Properties
    /// <summary>
    /// Stores and returns the value in the user's display time
    /// </summary>
    public DateTimeOffset SelectedDateTime
    {
        get
        {
            if (m_dateEditor.EditValue == null)
            {
                return PTDateTime.MinValue;
            }

            return new DateTime(m_dateEditor.DateTime.Date.Ticks + m_timeEditor.Time.TimeOfDay.Ticks).ToDateTimeOffset(TimeZoneAdjuster.CurrentTimeZoneInfo);
        }

        set
        {
            DateTimeOffset restrictedValue = RestrictValueChange(value);
            m_dateEditor.DateTime = restrictedValue.ToDisplayTime().ToDateNoTime().ToDateTime();
            m_timeEditor.Time = restrictedValue.ToDisplayTime().ToDateTime();
        }
    }

    private DateTimeOffset RestrictValueChange(DateTimeOffset a_newValue)
    {
        DateTimeOffset updatedValue = a_newValue;
        if (updatedValue <= PTDateTime.MinValue)
        {
            updatedValue = PTDateTime.MinValue;
            m_timeEditor.Enabled = false;
            //m_dateEditor.EditValue = null;
            //return;
        }
        else if (updatedValue >= PTDateTime.MaxValue)
        {
            updatedValue = PTDateTime.MinValue;
            m_timeEditor.Enabled = false;
            //m_dateEditor.EditValue = null;
            //return;
        }
        else
        {
            m_timeEditor.Enabled = true;
        }

        return updatedValue;
    }

    public new event EventHandler Click;

    private void DateTimeClicked(object sender, EventArgs e)
    {
        Click?.Invoke(sender, e);
    }

    public DateTimeOffset MinDate
    {
        get => new (m_dateEditor.Properties.MinValue, TimeZoneAdjuster.CurrentTimeZoneInfo.GetUtcOffset(m_dateEditor.Properties.MinValue));
        set
        {
            if (value < PTDateTime.MinValue)
            {
                m_dateEditor.Properties.MinValue = PTDateTime.MinValue.ToDateTime();
            }

            if (value > PTDateTime.MaxValue)
            {
                // This assignment is weird. Doesn't seem like any users should do it
                m_dateEditor.Properties.MinValue = PTDateTime.MinValue.ToDateTime();
            }
            else
            {
                DateTimeOffset displayTimeLimit = value.Date.ToDisplayTime();
                m_dateEditor.Properties.MinValue = displayTimeLimit.ToDateTime();
            }
        }
    }

    public DateTimeOffset MaxDate
    {
        get => new (m_dateEditor.Properties.MaxValue, TimeZoneAdjuster.CurrentTimeZoneInfo.GetUtcOffset(m_dateEditor.Properties.MaxValue));
        set
        {
            if (value < PTDateTime.MinValue)
            {
                m_dateEditor.Properties.MaxValue = PTDateTime.MaxValue.ToDateTime();
            }

            if (value > PTDateTime.MaxValue)
            {
                m_dateEditor.Properties.MaxValue = PTDateTime.MaxValue.ToDateTime();
            }
            else
            {
                m_dateEditor.Properties.MaxValue = value.ToDisplayTime().ToDateTime().Date;
            }
        }
    }
    #endregion

    public string InvalidDateText
    {
        get => m_dateEditor.Properties.NullText;
        set => m_dateEditor.Properties.NullText = value;
    }

    public override void Localize()
    {
        m_dateEditor.Properties.NullText = m_dateEditor.Properties.NullText.Localize();
        simpleLabelItem_Text.Text = simpleLabelItem_Text.Text.Localize();
        foreach (EditorButton button in m_dateEditor.Properties.Buttons)
        {
            button.Caption = button.Caption.Localize();
        }

        foreach (EditorButton button in m_timeEditor.Properties.Buttons)
        {
            button.Caption = button.Caption.Localize();
        }
    }

    public void Update()
    {
        m_dateEditor.DoValidate();
        m_timeEditor.DoValidate();
    }
}