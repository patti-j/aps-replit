using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI;

public interface IGridTile<T> : IScenarioTile<T>
{
    event Action<ITile, List<T>> ObjectsSelectedEvent;
    event Action<ITile, List<T>> ObjectsLoadedEvent;
}