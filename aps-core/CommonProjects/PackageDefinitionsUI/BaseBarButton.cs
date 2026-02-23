using DevExpress.XtraBars;

namespace PT.PackageDefinitionsUI;

public class BaseBarButton
{
    public BarButtonStyle ButtonStyle { get; protected set; }
    public bool ActAsDropDown { get; protected set; }
    public bool ButtonEnabled { get; protected set; }
    public string Caption { get; protected set; }
    public string SuperTipContent { get; protected set; }
    public string SuperTipTitle { get; protected set; }
}