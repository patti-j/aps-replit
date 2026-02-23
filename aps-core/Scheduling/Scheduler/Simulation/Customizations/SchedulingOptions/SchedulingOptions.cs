namespace PT.Scheduler.Simulation.Customizations;

public struct SchedulingOptions
{
    //public bool _adjustNbrOfCyclesEnabled;
    //public bool _joinEnabled;
    //public bool _splitJobEnabled;
    //public bool _splitMOEnabled;

    public bool _similarActIneligibilityEnabled;

    public static SchedulingOptions GetDefaultSchedulingOptions()
    {
        SchedulingOptions so;
        //so._adjustNbrOfCyclesEnabled = true;
        //so._joinEnabled = true;
        so._similarActIneligibilityEnabled = false;
        //so._splitJobEnabled = true;
        //so._splitMOEnabled = true;
        return so;
    }
}