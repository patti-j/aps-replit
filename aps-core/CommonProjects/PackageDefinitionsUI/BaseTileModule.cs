namespace PT.PackageDefinitionsUI.Packages;

public class BaseTileModule
{
    public BaseTileModule(IMainForm a_mainForm, IPackageManagerUI a_pm)
    {
        m_mainForm = a_mainForm;
        m_packageManager = a_pm;
        m_tileInfos = new List<TileInfo>();
    }

    protected readonly IMainForm m_mainForm;
    protected readonly IPackageManagerUI m_packageManager;
    protected List<TileInfo> m_tileInfos;
    protected string m_boardKey;
    public string BoardKey => m_boardKey;
}