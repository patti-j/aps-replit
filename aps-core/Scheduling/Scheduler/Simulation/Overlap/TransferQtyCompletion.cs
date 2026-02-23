namespace PT.Scheduler;

public class TransferQtyCompletion
{
    internal TransferQtyCompletion(long aCompletionDate, decimal aTotalQtyAvailable)
    {
        m_completionDate = aCompletionDate;
        m_totalQtyAvailable = aTotalQtyAvailable;
    }

    public long m_completionDate;
    public decimal m_totalQtyAvailable;

    public override string ToString()
    {
        string s = string.Format("{0} at {1}", m_totalQtyAvailable, DateTimeHelper.ToLocalTimeFromUTCTicks(m_completionDate));
        return s;
    }
}