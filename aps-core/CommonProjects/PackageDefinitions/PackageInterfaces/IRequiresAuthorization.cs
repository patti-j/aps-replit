namespace PT.PackageDefinitions.PackageInterfaces;

/// <summary>
/// This object requires authorization and will validate against user permissions
/// </summary>
public interface IRequiresAuthorization
{
    IEnumerable<string> Permissions { get; }
}