using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.UIDefinitions;

namespace PT.PackageDefinitionsUI;

public interface ITileBoard : IBoardControl
{
    ITile TileActivated(string a_tileKey);

    void LoadTileModule(ITileModule a_tilesModule);

    ITile GetPrimaryTile();

    void OpenTile(string a_tileKey, UINavigationEvent a_navEvent);
}