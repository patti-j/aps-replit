namespace PT.PackageDefinitionsUI;

public interface ICoreModule
{
    bool Singleton { get; }

    bool AllowOverrides { get; }
}