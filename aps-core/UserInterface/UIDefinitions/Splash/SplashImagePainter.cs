using System.Drawing;
using System.Drawing.Text;

using DevExpress.Utils.Drawing;
using DevExpress.XtraSplashScreen;

namespace PT.UIDefinitions.Splash;

public class SplashImagePainter : ICustomImagePainter
{
    static SplashImagePainter()
    {
        Painter = new SplashImagePainter();
    }

    protected SplashImagePainter() { }
    public static SplashImagePainter Painter { get; private set; }

    private ViewInfo info;

    public ViewInfo ViewInfo
    {
        get
        {
            if (info == null)
            {
                info = new ViewInfo();
            }

            return info;
        }
    }

    #region Drawing
    public void Draw(GraphicsCache a_cache, Rectangle a_bounds)
    {
        PointF statusPoint = ViewInfo.CalcStatusLabelPoint(a_cache, a_bounds);
        PointF warningPoint = ViewInfo.CalcWarningLabelPoint(a_cache, a_bounds);
        PointF versionPoint = ViewInfo.CalcVersionLabelPoint(a_cache, a_bounds);
        a_cache.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        a_cache.Graphics.DrawString(ViewInfo.StatusText, ViewInfo.StatusFont, ViewInfo.StatusBrush, statusPoint);
        a_cache.Graphics.DrawString(ViewInfo.WarningText, ViewInfo.WarningFont, ViewInfo.WarningBrush, warningPoint);
        a_cache.Graphics.DrawString(ViewInfo.InstanceNameAndVersionText, ViewInfo.VersionFont, ViewInfo.VersionBrush, versionPoint);
    }
    #endregion
}