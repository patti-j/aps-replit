namespace PipelinesPublisher.Definitions;

internal class PackagePath
{
    public string PackageRootPath;
    public string PackageDll;
    public string PackageSourcePath;
    public string PackageDestinationPath;
    public List<string> AssembliesToInclude; // This doesn't seem to be used
    public List<string> Categories; //This doesn't seem to be used either

    public PackagePath(string a_packageDll, string a_packageRootPath, string a_packageSourcePath, string a_packageDestinationPath, List<string> a_assembliesToInclude, List<string> a_categories)
    {
        PackageDll = a_packageDll;
        PackageRootPath = a_packageRootPath;
        PackageSourcePath = a_packageSourcePath;
        PackageDestinationPath = a_packageDestinationPath;
        AssembliesToInclude = a_assembliesToInclude;
        Categories = a_categories;
    }
}