using System.Drawing;

using DevExpress.XtraEditors;

namespace PT.ComponentLibrary.Extensions;

public static class ButtonExtensions
{
    /// <summary>
    /// Modify the button so the user knows they clicked the button and there was a warning.
    /// </summary>
    /// <param name="a_button"></param>
    public static void ApplyWarningToButton(this SimpleButton a_button)
    {
        a_button.BackColor = Color.Yellow;
    }

    /// <summary>
    /// Modify the button so the user knows they clicked the button and there was a warning.
    /// </summary>
    /// <param name="a_button"></param>
    public static void ApplyDefaultToButton(this SimpleButton a_button)
    {
        a_button.ResetBackColor();
    }
}