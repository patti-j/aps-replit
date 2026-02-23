using System.Drawing;
using System.IO;

using DevExpress.Utils;
using DevExpress.XtraBars.Docking2010.Views.Widget;

using PT.ComponentLibrary;

namespace PT.ScenarioControls.Tiles;

public partial class TileDocumentManager : PTBaseControl
{
    public TileDocumentManager()
    {
        InitializeComponent();
        View.UseDocumentSelector = DefaultBoolean.True;
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeUserControl(this);
    }

    internal Color PaneColor
    {
        set => tileViewMain.Appearance.BackColor = value;
        get => tileViewMain.Appearance.BackColor;
    }

    internal WidgetView View => documentManager1.ViewCollection[0] as WidgetView;

    internal byte[] SaveLayout()
    {
        using (MemoryStream s = new ())
        {
            documentManager1.View.SaveLayoutToStream(s);
            return s.ToArray();
        }
    }

    internal void LoadLayout(byte[] a_layoutBytes)
    {
        documentManager1.View.BeginUpdate();

        using (MemoryStream loadStream = new (a_layoutBytes))
        {
            documentManager1.View.RestoreLayoutFromStream(loadStream);
        }

        documentManager1.View.EndUpdate();
    }
}