using System.Reflection;
using System.Runtime.Loader;

namespace PT.Scheduler.PackageDefs;

/// <summary>
/// The purpose of this context is to load packages that may reference PT assemblies that are from a different version
/// </summary>
internal class PackageAssemblyContext : AssemblyLoadContext
{
    public PackageAssemblyContext(string a_contextName, bool a_isCollectible)
        : base(a_contextName, a_isCollectible)
    {
    }

    /// <summary>
    /// Override assembly resolution to check by name only instaed of matching on Name + Version + Culture + PublicToken
    /// </summary>
    /// <returns>An already loaded Assembly, or null to load the default</returns>
    protected override Assembly? Load(AssemblyName a_assemblyName)
    {
        //Use an existing assembly (from all contexts)
        Assembly loaded = AppDomain.CurrentDomain
                                   .GetAssemblies()
                                   .FirstOrDefault(a => a.GetName().Name == a_assemblyName.Name);

        if (loaded != null)
        {
            //Only match on name, ignore version and token if it's already loaded
            return loaded;
        }

        //Load using the default context
        Assembly existingReference = Default.LoadFromAssemblyName(a_assemblyName);
        if (existingReference != null)
        {
            //Skips checking version
            return existingReference;
        }

        // Nothing more to do—fallback to default resolution
        return null;
    }
}
