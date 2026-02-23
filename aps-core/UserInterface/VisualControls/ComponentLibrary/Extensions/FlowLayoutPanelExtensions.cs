using System.Drawing;
using System.Windows.Forms;

namespace PT.ComponentLibrary.Extensions;

public static class FlowLayoutPanelExtensions
{
    /// <summary>
    /// Find the largest width from the controls contained in the flowlayout panel and sets all the other controls to that width
    /// to make sure they are all the same size.
    /// </summary>
    /// <param name="a_panel"></param>
    public static void AdjustControlWidths(this FlowLayoutPanel a_panel)
    {
        int largestWidth = 0;

        foreach (Control panelControl in a_panel.Controls)
        {
            if (panelControl.Width > largestWidth)
            {
                largestWidth = panelControl.Width;
            }
        }

        foreach (Control panelControl in a_panel.Controls)
        {
            panelControl.Width = largestWidth;
        }
    }

    /// <summary>
    /// Gets the width, and adds up the heights of the controls in the flowlayout panel so that
    /// the popup control container can match that size.
    /// </summary>
    /// <param name="a_panel"></param>
    /// <returns></returns>
    public static Size GetPanelContentSize(this FlowLayoutPanel a_panel)
    {
        int width = 0;
        int height = 0;

        foreach (Control panelControl in a_panel.Controls)
        {
            if (width < panelControl.Width)
            {
                width = panelControl.Width + panelControl.Margin.Left + panelControl.Margin.Right;
            }

            height += panelControl.Height + panelControl.Margin.Top + panelControl.Margin.Bottom;
        }

        return new Size(width, height);
    }
}