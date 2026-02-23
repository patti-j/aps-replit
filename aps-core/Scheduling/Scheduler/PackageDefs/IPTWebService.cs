namespace PT.Scheduler.PackageDefs;

public interface IPTWebService : IStartStop
{
    public void LoadPackageManager(IPackageManager a_packageManager);
}