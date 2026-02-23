using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

using PT.APSCommon.Extensions;
using PT.Common.Localization;

namespace PT.ComponentLibrary.Controls;

/// <summary>
/// Summary description for DurationCombo.
/// </summary>
[ToolboxItem(true)]
public class DurationCombo : DevExpress.XtraEditors.ComboBoxEdit, ILocalizable
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private readonly Container m_components = null;

    public DurationCombo()
    {
        // This call is required by the Windows.Forms Form Designer.
        InitializeComponent();
        if (RuntimeStatus.IsRuntime)
        {
            m_duration = TimeSpan.FromDays(1); //should match text value set in constructor.
            Text = GetDisplayText(m_duration);
            Properties.AllowMouseWheel = false;
            EditValueChanged += new EventHandler(DurationCombo_ValueChanged);
            Leave += new EventHandler(LeaveHandler);
        }
    }

    public sealed override string Text
    {
        get => base.Text;
        set => base.Text = value;
    }

    private bool m_showInHours;

    /// <summary>
    /// If true then values remain in hours even if more than 24 hours.
    /// </summary>
    public bool ShowInHours
    {
        get => m_showInHours;
        set => m_showInHours = value;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            m_components?.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Component Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        ((System.ComponentModel.ISupportInitialize)(this.Properties)).BeginInit();
        this.SuspendLayout();

        ((System.ComponentModel.ISupportInitialize)(this.Properties)).EndInit();
        this.ResumeLayout(false);
    }
    #endregion

    public void AddEditorButton()
    {
        SuspendLayout();
        Properties.Buttons.AddRange(new[]
        {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)
        });
        Size = new System.Drawing.Size(100, 20);
        TabIndex = 0;
        Name = "ComboEdit";
        ResumeLayout();
    }

    protected override void OnPaint(PaintEventArgs pe)
    {
        // TODO: Add custom paint code here

        // Calling the base class OnPaint
        base.OnPaint(pe);
    }

    public class InvalidInputException : ApplicationException
    {
        public InvalidInputException(string a_inputValue) : base(string.Format("The input value,{0}, could not be converted to a TimeSpan.  The value should be in a format such as '2 days' or '1 week 2.2 hours'.".Localize(), a_inputValue)) { }
    }

    private readonly Dictionary<string, TimeSpan> m_customDurationDict = new ();
    private readonly Dictionary<TimeSpan, string> m_customDurationReversedDict = new ();

    public void AddCustomDuration(TimeSpan a_duration, string a_customCaption)
    {
        //To avoid confusion, don't add duplicate values
        if (!m_customDurationReversedDict.ContainsKey(a_duration))
        {
            m_customDurationReversedDict.Add(a_duration, a_customCaption);
            m_customDurationDict.Add(a_customCaption, a_duration);
            Properties.Items.Insert(0, a_customCaption);
        }
    }

    public void ClearCustomDictionary()
    {
        m_customDurationDict.Clear();
        m_customDurationReversedDict.Clear();
    }

    public string DisplayText(TimeSpan a_duration)
    {
        return GetDisplayText(a_duration);
    }

    private void AddTime(string a_display)
    {
        Properties.Items.Add(a_display.Localize());
    }

    private void Clear()
    {
        Properties.Items.Clear();
    }

    /// <summary>
    /// Returns a TimeSpan based on the value of 'text'.
    /// Can contain a pair of timespan descriptors like '2 days' or '3.2 years'.
    /// Case insensitive.
    /// </summary>
    /// <param name="a_text"></param>
    /// <returns></returns>
    internal TimeSpan SpanFromText(string a_text)
    {
        try
        {
            if (m_customDurationDict.TryGetValue(a_text, out TimeSpan customSpan))
            {
                return customSpan;
            }

            if (a_text.Trim() == "")
            {
                return TimeSpan.MinValue; //indicate invalid
            }

            char[] sepChar = " ".ToCharArray();
            string[] segments = a_text.Split(sepChar, 2);

            // things to look for
            // 1) char array segments [] length equals 1
            // 2) char array segments [] 0th index is an empty string
            if (segments.Length == 1 || (segments.Length == 2 && segments[0] == ""))
            {
                return TimeSpan.MinValue;
            }

            double nbrSegment = double.Parse(segments[0]);
            if (segments.Length < 2) //default to hours interpretation
            {
                return TimeSpan.MinValue; //indicates it's no good
            }

            string spanType = segments[1];
            return SpanFromText(nbrSegment, spanType);
        }
        catch (Exception)
        {
            return TimeSpan.MinValue; //indicate no good
        }
    }

    private static TimeSpan SpanFromText(double a_amount, string a_spanType)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (a_amount == 0)
        {
            return new TimeSpan(0);
        }

        a_spanType = a_spanType.ToLower().Trim();

        if (a_spanType == LOCALIZE_STR_YEAR)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerDay * 365 * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_YEARS)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerDay * 365 * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_MONTH)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerDay * 31 * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_MONTHS)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerDay * 31 * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_WEEK)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerDay * 7 * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_WEEKS)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerDay * 7 * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_DAY)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerDay * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_DAYS)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerDay * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_HOUR)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerHour * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_HOURS)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerHour * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_MINUTE)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerMinute * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_MINUTES)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerMinute * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_SECOND)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerSecond * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_SECONDS)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerSecond * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_MILLISECOND)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerMillisecond * a_amount));
        }

        if (a_spanType == LOCALIZE_STR_MILLISECONDS)
        {
            return new TimeSpan(Convert.ToInt64(TimeSpan.TicksPerMillisecond * a_amount));
        }

        return TimeSpan.MinValue; //indicates it's no good.
    }

    /// <summary>
    /// Define default duration with minimum 8 hours.  Prevents small buckets from being used.
    /// </summary>
    public void SetDefaultDurationsWithMin8Hours()
    {
        Clear();
        AddTime("8 hours");
        AddTime("12 hours");
        AddTime("16 hours");
        if (ShowInHours)
        {
            AddTime("24 hours");
        }
        else
        {
            AddTime("1 day");
            AddTime("2 days");
            AddTime("3 days");
            AddTime("4 days");
            AddTime("5 days");
            AddTime("6 days");
            AddTime("1 week");
            AddTime("2 weeks");
            AddTime("3 weeks");
            AddTime("4 weeks");
            AddTime("1 month");
            AddTime("2 months");
            AddTime("3 months");
            AddTime("4 months");
            AddTime("5 months");
            AddTime("6 months");
            AddTime("9 months");
            AddTime("1 year");
            AddTime("2 years");
        }

        Text = (string)Properties.Items[0];
    }

    /// <summary>
    /// Set DurationCombo values for one week window duration in KPIControlPlot
    /// </summary>
    public void SetKPIControlDurationComboValues()
    {
        Clear();
        AddTime("30 minutes");
        AddTime("1 hour");
        AddTime("2 hours");
        AddTime("4 hours");
        AddTime("8 hours");
        AddTime("12 hours");
        AddTime("16 hours");
        if (ShowInHours)
        {
            AddTime("24 hours");
        }
        else
        {
            AddTime("1 day");
            AddTime("2 days");
            AddTime("3 days");
            AddTime("4 days");
            AddTime("5 days");
            AddTime("6 days");
            AddTime("1 week");
            AddTime("2 weeks");
            AddTime("3 weeks");
            AddTime("4 weeks");
            AddTime("1 month");
            AddTime("2 months");
            AddTime("3 months");
            AddTime("6 months");
            AddTime("1 year");
        }
    }

    public void SetDefaultDurations()
    {
        //Define the default list of durations
        Clear();
        //AddTime("0 minutes");
        AddTime("1 minute");
        AddTime("5 minutes");
        AddTime("10 minutes");
        AddTime("15 minutes");
        AddTime("30 minutes");
        AddTime("45 minutes");
        AddTime("1 hour");
        AddTime("2 hours");
        AddTime("4 hours");
        AddTime("8 hours");
        AddTime("12 hours");
        AddTime("16 hours");
        if (ShowInHours)
        {
            AddTime("24 hours");
        }
        else
        {
            AddTime("1 day");
            AddTime("2 days");
            AddTime("3 days");
            AddTime("4 days");
            AddTime("5 days");
            AddTime("6 days");
            AddTime("1 week");
            AddTime("2 weeks");
            AddTime("3 weeks");
            AddTime("4 weeks");
            AddTime("1 month");
            AddTime("2 months");
            AddTime("3 months");
            AddTime("4 months");
            AddTime("5 months");
            AddTime("6 months");
            AddTime("9 months");
            AddTime("1 year");
            AddTime("2 years");
        }

        Text = (string)Properties.Items[0];
    }

    public void SetDefaultDurationsStartingAtOneDay()
    {
        //Define the default list of durations
        Clear();
        AddTime("1 day");
        AddTime("2 days");
        AddTime("3 days");
        AddTime("4 days");
        AddTime("5 days");
        AddTime("6 days");
        AddTime("1 week");
        AddTime("2 weeks");
        AddTime("3 weeks");
        AddTime("4 weeks");
        AddTime("1 month");
        AddTime("2 months");
        AddTime("3 months");
        AddTime("4 months");
        AddTime("5 months");
        AddTime("6 months");
        AddTime("9 months");
        AddTime("1 year");
        AddTime("2 years");
        Text = (string)Properties.Items[0];
    }

    public void SetDefaultDurationsUnderOneDay()
    {
        //Define the default list of durations
        Clear();
        AddTime("5 seconds");
        AddTime("15 seconds");
        AddTime("30 seconds");
        AddTime("1 minute");
        AddTime("5 minutes");
        AddTime("10 minutes");
        AddTime("15 minutes");
        AddTime("30 minutes");
        AddTime("45 minutes");
        AddTime("1 hour");
        AddTime("2 hours");
        AddTime("4 hours");
        AddTime("8 hours");
        AddTime("12 hours");
        AddTime("16 hours");
        Text = (string)Properties.Items[0];
    }

    public void RemoveZeroDuration()
    {
        for (int i = Properties.Items.Count - 1; i >= 0; i--)
        {
            string vlItem = (string)Properties.Items[i];
            TimeSpan span = SpanFromText(vlItem);
            if (span.Ticks == 0)
            {
                Properties.Items.Remove(vlItem);
            }
        }
    }

    private TimeSpan m_minimumDuration = TimeSpan.Zero;

    /// <summary>
    /// Removes invalid values from the dropdown. Also enforces the minimum so the duration cannot be set less than this value
    /// </summary>
    /// <param name="a_min"></param>
    public void SetMinimum(TimeSpan a_min)
    {
        m_minimumDuration = a_min;
        for (int i = Properties.Items.Count - 1; i >= 0; i--)
        {
            string vlItem = (string)Properties.Items[i];
            TimeSpan span = SpanFromText(vlItem);
            if (span < m_minimumDuration)
            {
                Properties.Items.Remove(vlItem);
            }
        }
    }

    private TimeSpan m_maximumDuration = TimeSpan.MaxValue;

    /// <summary>
    /// Removes invalid values from the dropdown. Also enforces the maximum so the duration cannot be set less than this value
    /// </summary>
    /// <param name="a_max"></param>
    public void SetMaximum(TimeSpan a_max)
    {
        m_maximumDuration = a_max;
        for (int i = Properties.Items.Count - 1; i >= 0; i--)
        {
            string vlItem = (string)Properties.Items[i];
            TimeSpan span = SpanFromText(vlItem);
            if (span > m_maximumDuration)
            {
                if ((string)Properties.Items[i] != "No Bucket")
                {
                    Properties.Items.Remove(vlItem);
                }
            }
        }
    }

    /// <summary>
    /// Finds the smallest time span in the duration combo box
    /// </summary>
    /// <returns> returns the index</returns>
    public int GetLowestDuration()
    {
        TimeSpan minSpan = TimeSpan.MaxValue;
        int index = -1;
        for (int i = 0; i < Properties.Items.Count; i++)
        {
            string vlItem = (string)Properties.Items[i];
            TimeSpan span = SpanFromText(vlItem);
            if (span < minSpan)
            {
                minSpan = span;
                index = i;
            }
        }

        return index;
    }

    public delegate void DurationChangedDelegate(object sender, TimeSpan newDuration);

    public event DurationChangedDelegate DurationChangedEvent;

    private TimeSpan m_duration;

    public TimeSpan Duration //don't use this internally to set the duration.  Only for external use.
    {
        get => m_duration;
        set => Text = GetDisplayText(new TimeSpan(Math.Max(m_minimumDuration.Ticks, value.Ticks))); //value changed event will set the duration
    }

    private void SetDuration(TimeSpan newDuration)
    {
        newDuration = PTDateTime.Max(newDuration, m_minimumDuration);
        if (newDuration != m_duration)
        {
            TimeSpan previousDuration = m_duration;
            m_duration = newDuration;
            if (DurationChangedEvent != null && newDuration != previousDuration) //screws up embedded control in grids!
            {
                DurationChangedEvent(this, newDuration);
            }
        }
    }

    public void Localize()
    {
        UILocalizationHelper.LocalizeControlsRecursively(Controls);
    }

    private static readonly string LOCALIZE_STR_YEAR = "year".Localize().ToLower();
    private static readonly string LOCALIZE_STR_YEARS = "years".Localize().ToLower();
    private static readonly string LOCALIZE_STR_MONTH = "month".Localize().ToLower();
    private static readonly string LOCALIZE_STR_MONTHS = "months".Localize().ToLower();
    private static readonly string LOCALIZE_STR_WEEK = "week".Localize().ToLower();
    private static readonly string LOCALIZE_STR_WEEKS = "weeks".Localize().ToLower();
    private static readonly string LOCALIZE_STR_DAY = "day".Localize().ToLower();
    private static readonly string LOCALIZE_STR_DAYS = "days".Localize().ToLower();
    private static readonly string LOCALIZE_STR_HOUR = "hour".Localize().ToLower();
    private static readonly string LOCALIZE_STR_HOURS = "hours".Localize().ToLower();
    private static readonly string LOCALIZE_STR_MINUTE = "minute".Localize().ToLower();
    private static readonly string LOCALIZE_STR_MINUTES = "minutes".Localize().ToLower();
    private static readonly string LOCALIZE_STR_SECOND = "second".Localize().ToLower();
    private static readonly string LOCALIZE_STR_SECONDS = "seconds".Localize().ToLower();
    private static readonly string LOCALIZE_STR_MILLISECOND = "millisecond".Localize().ToLower();
    private static readonly string LOCALIZE_STR_MILLISECONDS = "milliseconds".Localize().ToLower();

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    internal string GetDisplayText(TimeSpan a_duration)
    {
        if (m_customDurationReversedDict.TryGetValue(a_duration, out string customText))
        {
            return customText;
        }

        if (a_duration.Ticks == 0)
        {
            return "0 " + LOCALIZE_STR_HOURS;
        }

        //Need to handle negative and positive values
        string signString = "";
        if (a_duration.Ticks < 0)
        {
            signString = "-";
            a_duration = new TimeSpan(Math.Abs(a_duration.Ticks));
        }

        if (ShowInHours && a_duration.TotalHours <= 48) //added for Abbot to show larger values in days
        {
            if (a_duration.TotalHours == 1)
            {
                return string.Format("1 {0}", LOCALIZE_STR_HOUR);
            }

            if (a_duration.TotalHours > 1)
            {
                return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalHours, 2), LOCALIZE_STR_HOURS);
            }

            if (a_duration.TotalMinutes == 1)
            {
                return string.Format("1 {0}", LOCALIZE_STR_MINUTE);
            }

            if (a_duration.TotalMinutes > 1)
            {
                return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalMinutes, 2), LOCALIZE_STR_MINUTES);
            }

            if (a_duration.TotalSeconds == 1)
            {
                return string.Format("1 {0}", LOCALIZE_STR_SECOND);
            }

            if (a_duration.TotalSeconds > 1)
            {
                return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalSeconds, 2), LOCALIZE_STR_SECONDS);
            }

            if (a_duration.TotalMilliseconds == 1)
            {
                return string.Format("1 {0}", LOCALIZE_STR_MILLISECOND);
            }

            return string.Format("{0} {1}", Math.Round(a_duration.TotalMilliseconds, 2), LOCALIZE_STR_MILLISECONDS);
        }

        if (a_duration.TotalDays == 365)
        {
            return string.Format("1 {0}", LOCALIZE_STR_YEAR);
        }

        if (a_duration.TotalDays > 365)
        {
            return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalDays / 365.0, 2), LOCALIZE_STR_YEARS);
        }

        if (a_duration.TotalDays == 31)
        {
            return string.Format("1 {0}", LOCALIZE_STR_MONTH);
        }

        if (a_duration.TotalDays > 31)
        {
            return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalDays / 31.0, 2), LOCALIZE_STR_MONTHS);
        }

        if (a_duration.TotalDays == 7)
        {
            return string.Format("1 {0}", LOCALIZE_STR_WEEK);
        }

        if (a_duration.TotalDays > 7)
        {
            return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalDays / 7.0, 2), LOCALIZE_STR_WEEKS);
        }

        if (a_duration.TotalDays == 1)
        {
            return string.Format("1 {0}", LOCALIZE_STR_DAY);
        }

        if (a_duration.TotalDays > 1)
        {
            return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalDays, 2), LOCALIZE_STR_DAYS);
        }

        if (a_duration.TotalHours == 1)
        {
            return string.Format("1 {0}", LOCALIZE_STR_HOUR);
        }

        if (a_duration.TotalHours > 1)
        {
            return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalHours, 2), LOCALIZE_STR_HOURS);
        }

        if (a_duration.TotalMinutes == 1)
        {
            return string.Format("1 {0}", LOCALIZE_STR_MINUTE);
        }

        if (a_duration.TotalMinutes > 1)
        {
            return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalMinutes, 2), LOCALIZE_STR_MINUTES);
        }

        if (a_duration.TotalSeconds == 1)
        {
            return string.Format("1 {0}", LOCALIZE_STR_SECOND);
        }

        if (a_duration.TotalSeconds > 1)
        {
            return string.Format("{0}{1} {2}", signString, Math.Round(a_duration.TotalSeconds, 2), LOCALIZE_STR_SECONDS);
        }

        if (a_duration.TotalMilliseconds == 1)
        {
            return string.Format("1 {0}", LOCALIZE_STR_MILLISECOND);
        }

        return string.Format("{0} {1}", Math.Round(a_duration.TotalMilliseconds, 2), LOCALIZE_STR_MILLISECONDS);
    }

    private void DurationCombo_ValueChanged(object sender, EventArgs e)
    {
        try
        {
            TimeSpan newSpan = SpanFromText(Text);
            if (newSpan != TimeSpan.MinValue) //valid span
            {
                SetDuration(newSpan);
            }
        }
        catch (Exception) { }
    }

    private void LeaveHandler(object a_sender, EventArgs a_e)
    {
        try
        {
            TimeSpan newSpan = SpanFromText(Text);
            if (newSpan == TimeSpan.MinValue) //valid span
            {
                Text = GetDisplayText(m_duration);
            }
        }
        catch (Exception) { }
    }
}