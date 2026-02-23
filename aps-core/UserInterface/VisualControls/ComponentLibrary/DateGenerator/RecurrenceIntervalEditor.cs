using System.Windows.Forms;

namespace PT.ComponentLibrary.DateGenerator;

public partial class RecurrenceIntervalEditor : UserControl
{
    public delegate void SelectedIntervalChangedHandler(TimeSpan a_timespan);

    public event SelectedIntervalChangedHandler SelectedIntervalChanged;

    public RecurrenceIntervalEditor()
    {
        InitializeComponent();

        SetRecurrenceInterval(DateInterval.EInterval.Day);
    }

    public void SetRecurrenceInterval(DateInterval.EInterval a_recurrence)
    {
        GetRadioButtonFromInterval(a_recurrence).Checked = true;
    }

    public DateInterval.EInterval GetRecurrenceInterval()
    {
        if (radioButton_day.Checked)
        {
            return DateInterval.EInterval.Day;
        }

        if (radioButton_week.Checked)
        {
            return DateInterval.EInterval.Week;
        }

        if (radioButton_month.Checked)
        {
            return DateInterval.EInterval.Month;
        }

        return DateInterval.EInterval.Quarter;
    }

    public static TimeSpan GetTimeSpanFromInterval(DateInterval.EInterval a_interval)
    {
        if (a_interval == DateInterval.EInterval.Day)
        {
            return TimeSpan.FromDays(1);
        }

        if (a_interval == DateInterval.EInterval.Week)
        {
            return TimeSpan.FromDays(7);
        }

        if (a_interval == DateInterval.EInterval.Month)
        {
            return TimeSpan.FromDays(30);
        }

        return TimeSpan.FromDays(90);
    }

    private RadioButton GetRadioButtonFromInterval(DateInterval.EInterval a_timeBucket)
    {
        if (a_timeBucket == DateInterval.EInterval.Day)
        {
            return radioButton_day;
        }

        if (a_timeBucket == DateInterval.EInterval.Week)
        {
            return radioButton_week;
        }

        if (a_timeBucket == DateInterval.EInterval.Month)
        {
            return radioButton_month;
        }

        return radioButton_quarter;
    }

    internal void SetIntervalCount(DateInterval.EInterval a_interval, int a_count, bool a_disableIfZero = true)
    {
        RadioButton rb = GetRadioButtonFromInterval(a_interval);
        rb.Text = string.Format((string)rb.Tag, a_count);
        rb.Enabled = true;

        if (a_count == 0 && a_disableIfZero)
        {
            if (rb.Checked)
            {
                radioButton_day.Checked = true;
            }

            rb.Enabled = false;
        }
    }

    private void RadioButton_CheckedChanged(object sender, EventArgs e)
    {
        RadioButton rb = sender as RadioButton;
        if (rb != null && rb.Checked && SelectedIntervalChanged != null)
        {
            SelectedIntervalChanged(GetTimeSpanFromInterval(GetRecurrenceInterval()));
        }
    }
}