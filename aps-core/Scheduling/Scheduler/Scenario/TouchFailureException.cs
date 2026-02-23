namespace PT.Scheduler;

internal class TouchFailureException : Exception
{
    internal TouchFailureException()
        : base("The checksums didn't match after the Touch.") { }
}