using System.Drawing;

using DevExpress.XtraBars;
using DevExpress.XtraEditors;

namespace PT.ComponentLibrary;

public static class DisposeUtilities
{
    /// <summary>
    /// Diposes of resources that are not automatically released from a button
    /// </summary>
    public static void DisposeImages(SimpleButton a_button)
    {
        if (a_button.Image is Bitmap)
        {
            (a_button.Image as Bitmap).Dispose();
            a_button.Image = null;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="a_barManager"></param>
    public static void DisposeImages(BarManager a_barManager)
    {
        if (a_barManager != null)
        {
            foreach (BarItem barItem in a_barManager.Items)
            {
                if (barItem.IsImageExist)
                {
                    barItem.Glyph.Dispose();
                }
            }
        }
    }
}