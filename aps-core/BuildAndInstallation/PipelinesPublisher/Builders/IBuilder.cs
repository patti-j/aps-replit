namespace PipelinesPublisher.Builder;

internal interface IBuilder
{
    public string BaseFolder { get; }
    public string Version { get; }
    public BuildPaths Paths { get; }

    void Run();

    #region Default Methods
    public void CleanOutputFiles()
    {
        string[] filesToDelete = Directory.GetFiles(BaseFolder, "*PT.*.xml", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*DevExpress*.xml", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*map*.xml", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*jobdataset.xml", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*.pdb", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*.msm", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*vshost.exe*", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*.manifest", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*.application", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*ErrOr*.txt", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*ErrOr*.log", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*.svclog", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*readLockCounts.txt", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*Timing.txt", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
        filesToDelete = Directory.GetFiles(BaseFolder, "*appsettings.Development.json", SearchOption.AllDirectories);
        FileUtil.DeleteFiles(filesToDelete);
    }

    public bool ValidateArgs(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Please enter build type, source path, cert path, and cert password");
            return false;
        }

        if (!Directory.Exists(args[1]))
        {
            Console.WriteLine($"Source path: {args[1]} does not exist.");
            return false;
        }

        if (!File.Exists(args[2]))
        {
            Console.WriteLine($"Key file : {args[2]} does not exist.");
            return false;
        }

        if (string.IsNullOrEmpty(args[3]))
        {
            Console.WriteLine("Cert password is empty.");
            return false;
        }

        return true;
    }
    #endregion
}