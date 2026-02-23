using System.Windows.Forms;

namespace PT.ComponentLibrary.DateGenerator;

public partial class DateRangeEditor : UserControl
{
    private bool m_ignoreValueChangedEvent;

    public DateRangeEditor()
    {
        InitializeComponent();

        SetRange(PTDateTime.UserDateTimeNow.ToDateNoTime(), PTDateTime.UserDateTimeNow.ToDateNoTime().AddDays(7));
    }

    public void SetRange(DateTimeOffset a_start, DateTimeOffset a_end)
    {
        if (a_start < a_end)
        {
            m_ignoreValueChangedEvent = true;

            dateTimeEditor_start.EditValue = a_start.ToDateTime();
            dateTimeEditor_end.EditValue = a_end.ToDateTime();

            DateRangeChanged?.Invoke(RangeStart, RangeEnd);

            m_ignoreValueChangedEvent = false;
        }
    }

    public DateTime RangeStart => (DateTime)dateTimeEditor_start.EditValue;

    public DateTime RangeEnd => (DateTime)dateTimeEditor_end.EditValue;

    public TimeSpan Range => RangeEnd - RangeStart;

    public int GetNumberOfIntervals(DateInterval.EInterval a_interval)
    {
        long remainder;
        return (int)Math.DivRem(Range.Ticks, RecurrenceIntervalEditor.GetTimeSpanFromInterval(a_interval).Ticks, out remainder);
    }

    public bool IsGreaterOrEqToInterval(DateInterval.EInterval a_interval)
    {
        return Range >= RecurrenceIntervalEditor.GetTimeSpanFromInterval(a_interval);
    }

    public delegate void DateRangeChangedDelegate(DateTime a_startdate, DateTime a_endDate);

    public event DateRangeChangedDelegate DateRangeChanged;

    private void dateTimeEditor_start_ValueChanged(object sender, EventArgs e)
    {
        if (m_ignoreValueChangedEvent)
        {
            return;
        }

        if (RangeEnd <= RangeStart)
        {
            dateTimeEditor_end.EditValue = RangeStart.AddDays(1);
            return; // setting end will cause event
        }

        DateRangeChanged?.Invoke(RangeStart, RangeEnd);
    }

    private void dateTimeEditor_end_ValueChanged(object sender, EventArgs e)
    {
        if (m_ignoreValueChangedEvent)
        {
            return;
        }

        if (RangeEnd <= RangeStart)
        {
            dateTimeEditor_start.EditValue = RangeEnd.Subtract(TimeSpan.FromDays(1));
            return;
        }

        DateRangeChanged?.Invoke(RangeStart, RangeEnd);
    }
}