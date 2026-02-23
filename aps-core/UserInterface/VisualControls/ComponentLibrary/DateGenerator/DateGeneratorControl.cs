using System.Windows.Forms;

namespace PT.ComponentLibrary.DateGenerator;

public partial class DateGeneratorControl : UserControl
{
    public delegate void DatesChangedDelegate();

    public event DatesChangedDelegate DatesChanged;

    public DateGeneratorControl()
    {
        InitializeComponent();

        dateRangeEditor.DateRangeChanged += dateRangeEditor_DateRangeChanged;
        recurrenceIntervalEditor.SelectedIntervalChanged += timeBucketEditor_TimeBucketChanged;
    }

    public void SetRange(DateTime a_startDate, DateTime a_endDate)
    {
        dateRangeEditor.SetRange(a_startDate, a_endDate);

        RefreshIntervalOptions();
    }

    public void SetInterval(DateInterval.EInterval a_recurrence)
    {
        recurrenceIntervalEditor.SetRecurrenceInterval(a_recurrence);
    }

    public DateInterval.EInterval GetInterval()
    {
        return recurrenceIntervalEditor.GetRecurrenceInterval();
    }

    private void timeBucketEditor_TimeBucketChanged(TimeSpan a_timespan)
    {
        RecalculateAndFireChangedEvent();
    }

    private void dateRangeEditor_DateRangeChanged(DateTime a_startdate, DateTime a_endDate)
    {
        RefreshIntervalOptions();
        RecalculateAndFireChangedEvent();
    }

    private void RecalculateAndFireChangedEvent()
    {
        if (DatesChanged != null)
        {
            DatesChanged();
        }
    }

    private void RefreshIntervalOptions()
    {
        recurrenceIntervalEditor.SetIntervalCount(DateInterval.EInterval.Day, dateRangeEditor.GetNumberOfIntervals(DateInterval.EInterval.Day));
        recurrenceIntervalEditor.SetIntervalCount(DateInterval.EInterval.Week, dateRangeEditor.GetNumberOfIntervals(DateInterval.EInterval.Week));
        recurrenceIntervalEditor.SetIntervalCount(DateInterval.EInterval.Month, dateRangeEditor.GetNumberOfIntervals(DateInterval.EInterval.Month));
        recurrenceIntervalEditor.SetIntervalCount(DateInterval.EInterval.Quarter, dateRangeEditor.GetNumberOfIntervals(DateInterval.EInterval.Quarter));
    }

    public List<DateTime> GetDates()
    {
        List<DateTime> dates = new ();

        DateTime currentDate = dateRangeEditor.RangeStart;
        DateTime endDate = dateRangeEditor.RangeEnd;
        TimeSpan interval = DateInterval.GetIntervalTimeSpan(recurrenceIntervalEditor.GetRecurrenceInterval());

        // break up the range into interval
        currentDate = currentDate.Add(interval);
        while (currentDate < endDate)
        {
            dates.Add(currentDate);
            currentDate = currentDate.Add(interval);
        }

        return dates;
    }
}