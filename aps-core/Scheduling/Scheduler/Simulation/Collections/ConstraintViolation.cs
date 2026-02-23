namespace PT.Scheduler;

/// <summary>
/// Summary description for ConstraintViolation.
/// </summary>
internal class ConstraintViolation
{
    internal InternalActivity m_activity;
    internal long m_constraintTime;
    internal string m_constraintName;

    internal ConstraintViolation(InternalActivity a_activity, long a_constraintTime, string a_constraintName)
    {
        m_activity = a_activity;
        m_constraintTime = a_constraintTime;
        m_constraintName = a_constraintName;
    }
}