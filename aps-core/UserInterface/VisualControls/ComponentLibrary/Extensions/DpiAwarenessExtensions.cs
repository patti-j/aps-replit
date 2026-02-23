using System.Drawing;

namespace PT.ComponentLibrary.Extensions;

public static class DpiAwarenessExtensions
{
    public static Size GetScaledSize(this Size a_size, decimal a_scaleFactor, int a_width, int a_height)
    {
        decimal scalingFactor = a_scaleFactor != 0 ? a_scaleFactor : 1;

        int width = Convert.ToInt32(a_width / scalingFactor);
        int height = Convert.ToInt32(a_height / scalingFactor);

        return new Size(width, height);
    }

    public static int GetScaledValue(this int a_value, decimal a_scaleFactor)
    {
        decimal scalingFactor = a_scaleFactor != 0 ? a_scaleFactor : 1;

        return Convert.ToInt32(a_value / scalingFactor);
    }
}