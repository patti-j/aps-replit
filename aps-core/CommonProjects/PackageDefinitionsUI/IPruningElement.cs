using PT.PackageDefinitions;
using PT.Transmissions;

namespace PT.PackageDefinitionsUI;

public interface IPruningElement : IPackageElement
{
    string Name { get; }
    string Description { get; } //Maybe not necessary

    bool Show { get; }

    PTTransmission GetTransmission();
}