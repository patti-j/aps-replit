using PT.APSCommon;
using PT.PackageDefinitions;

namespace PT.PackageDefinitionsUI;

public interface ITileController
{
    void LoadTile(ITile a_tile);

    void ObjectsSelected(List<ITile> a_source, List<BaseId> a_objects);

    List<ITile> GetClosedTileNames();

    void HideTile(ITile a_tile);
    ITile ActivateTile(string a_tileName);
}