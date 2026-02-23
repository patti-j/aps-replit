using System.Diagnostics;

public class ProgressReporter
{
    private readonly Action<string> _updateStatus;
    private readonly Stopwatch _stopwatch;
    private readonly int _totalItems;
    private int _processedItems;
    private double _averageMsPerItem;

    public ProgressReporter(int totalItems, Action<string> updateStatus)
    {
        _totalItems = totalItems;
        _updateStatus = updateStatus;
        _stopwatch = Stopwatch.StartNew();
    }

    public void ItemProcessed(string currentItemName = null)
    {
        _processedItems++;
        if (_processedItems == 100)
        {
            _stopwatch.Stop();
            _averageMsPerItem = _stopwatch.Elapsed.TotalMilliseconds / 100;
            _stopwatch.Start();
        }

        if (_processedItems % 50 == 0 || _processedItems == _totalItems)
        {
            ReportProgress(currentItemName);
        }
    }

    private void ReportProgress(string currentItemName)
    {
        string message = $"Processing items... ({_processedItems} / {_totalItems})";

        if (!string.IsNullOrWhiteSpace(currentItemName))
        {
            message += $" - Currently processing: {currentItemName}";
        }

        if (_processedItems >= 100 && _averageMsPerItem > 0)
        {
            int remainingItems = _totalItems - _processedItems;
            double etaSeconds = (_averageMsPerItem * remainingItems) / 1000;
            string etaFormatted = TimeSpan.FromSeconds(etaSeconds).ToString(@"mm\:ss");
            message += $" - Estimated time remaining: {etaFormatted}";
        }

        _updateStatus?.Invoke(message);
    }
}
