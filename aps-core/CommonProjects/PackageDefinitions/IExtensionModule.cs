using PT.APSCommon;

namespace PT.PackageDefinitions;

public interface IExtensionModule : IPackageModule
{
    List<IExtensionElement> GetExtensionElements(ICommonLogger a_errorReporter);
}

public interface IExtensionElement : IPackageElement
{
    string Name { get; }

    string Description { get; }

    /// <summary>
    /// Whether this extension element can run while other elements of the same type are running
    /// All MultiThreaded elements will be run at once and the result will await all elements
    /// Priority will not be used for sequencing and all MultiThreaded extensions will run after SingleThreaded ones.
    /// </summary>
    bool MultiThreaded { get; }

    /// <summary>
    /// The order in which this extension element will run compared to other extension elements of the same type
    /// Lower is higher priority. Extensions with the same priority that are not MultiThreaded will case an exception
    /// </summary>
    int Priority { get; }
}