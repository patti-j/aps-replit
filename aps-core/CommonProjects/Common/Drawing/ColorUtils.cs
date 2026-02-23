using System.Drawing;
using System.Globalization;
using System.Reflection;

namespace PT.Common;

public static class ColorUtils
{
    public static bool GetKnownColor(Color a_inColor, out string a_strKnownColor)
    {
        Array aListofKnownColors = Enum.GetValues(typeof(KnownColor));
        foreach (KnownColor eKnownColor in aListofKnownColors)
        {
            Color someColor = Color.FromKnownColor(eKnownColor);
            if (a_inColor.ToArgb() == someColor.ToArgb() && !someColor.IsSystemColor)
            {
                a_strKnownColor = someColor.Name;
                return true;
            }
        }

        a_strKnownColor = "";
        return false;
    }

    public static List<Color> GetAllColors()
    {
        List<Color> allColors = new ();

        foreach (PropertyInfo property in typeof(Color).GetProperties())
        {
            if (property.PropertyType == typeof(Color))
            {
                allColors.Add((Color)property.GetValue(null));
            }
        }

        return allColors;
    }

    public class ColorCodes
    {
        //Block onTimeStatuses colors
        public static readonly Color OnTimeColor = Color.LightGreen; //Same as OkColor below
        public static readonly Color TooEarlyColor = Color.LightYellow;
        public static readonly Color AlmostLateColor = CalculateColor(Color.Yellow, Color.Yellow, 150); //Same as WarningColor below
        public static readonly Color LateColor = CalculateColor(Color.Orange, Color.Orange, 150); //Same as WarningColor2 below
        public static readonly Color OverdueColor = CalculateColor(Color.Red, Color.White, 150); //same as AlertColor below
        public static readonly Color CapacityBottleneckColor = Color.Red;
        public static readonly Color MaterialBottleneckColor = Color.Magenta;
        public static readonly Color LateReleaseBottleneckColor = Color.Pink;

        //Block Priority Colors
        public static readonly Color Priority1 = Color.Red;
        public static readonly Color Priority2 = Color.Orange;
        public static readonly Color Priority3 = Color.Yellow;
        public static readonly Color PriorityHigherThan3 = Color.White;

        public static readonly Color NotFoundColor = Color.MistyRose;
        public static readonly Color AlertColor = CalculateColor(Color.Red, Color.White, 150);
        public static readonly Color WarningColor = CalculateColor(Color.Yellow, Color.Yellow, 150);
        public static readonly Color WarningColor2 = CalculateColor(Color.Orange, Color.Orange, 150);
        public static readonly Color MinorWarningColor = CalculateColor(Color.Yellow, Color.Yellow, 75);
        public static readonly Color OkColor = Color.LightGreen; // CalculateColor(Color.Green, Color.Green, 75); 
        public static readonly Color DisabledCellColor = Color.WhiteSmoke;

        public static readonly Color SelectionColor = Color.DeepSkyBlue;

        // Capacity Interval Colors
        public static readonly Color CapacityIntervalOnlineColor = Color.FromArgb(144, 220, 144);
        public static readonly Color CapacityIntervalCleanoutColor = ChangeColorBrightness(Color.Blue, .35f);
        public static readonly Color CapacityIntervalOvertimeColor = Color.FromArgb(255, 242, 205);
        public static readonly Color CapacityIntervalPotentialOvertimeColor = Color.PaleVioletRed;
        public static readonly Color CapacityIntervalOfflineColor = Color.FromArgb(210, 200, 200, 200);
        public static readonly Color CapacityIntervalHolidayColor = ChangeColorBrightness(Color.Goldenrod, .35f);
        public static readonly Color CapacityIntervalMaintenanceColor = ChangeColorBrightness(ColorTranslator.FromHtml("#FFC000"), .35f);
    }

    public static Color CalculateColor(Color a_front, Color a_back, int a_alpha)
    {
        // solid color obtained as a result of alpha-blending

        Color frontColor = Color.FromArgb(255, a_front);
        Color backColor = Color.FromArgb(255, a_back);

        double frontRed = frontColor.R;
        double frontGreen = frontColor.G;
        double frontBlue = frontColor.B;
        double backRed = backColor.R;
        double backGreen = backColor.G;
        double backBlue = backColor.B;

        double fRed = frontRed * a_alpha / 255 + backRed * ((double)(255 - a_alpha) / 255);
        byte newRed = (byte)fRed;
        double fGreen = frontGreen * a_alpha / 255 + backGreen * ((double)(255 - a_alpha) / 255);
        byte newGreen = (byte)fGreen;
        double fBlue = frontBlue * a_alpha / 255 + backBlue * ((double)(255 - a_alpha) / 255);
        byte newBlue = (byte)fBlue;

        return Color.FromArgb(255, newRed, newGreen, newBlue);
    }

    //Source: http://www.pvladov.com/2012/09/make-color-lighter-or-darker.html
    /// <summary>
    /// Creates color with corrected brightness.
    /// </summary>
    /// <param name="a_color">Color to correct.</param>
    /// <param name="a_correctionFactor">
    /// The brightness correction factor. Must be between -1 and 1.
    /// Negative values produce darker colors.
    /// </param>
    /// <returns>
    /// Corrected <see cref="Color" /> structure.
    /// </returns>
    public static Color ChangeColorBrightness(Color a_color, float a_correctionFactor)
    {
        float red = a_color.R;
        float green = a_color.G;
        float blue = a_color.B;

        if (a_correctionFactor < 0)
        {
            a_correctionFactor = 1 + a_correctionFactor;
            red *= a_correctionFactor;
            green *= a_correctionFactor;
            blue *= a_correctionFactor;
        }
        else
        {
            red = (255 - red) * a_correctionFactor + red;
            green = (255 - green) * a_correctionFactor + green;
            blue = (255 - blue) * a_correctionFactor + blue;
        }

        return Color.FromArgb(a_color.A, (int)red, (int)green, (int)blue);
    }

    public static Color GetFadedColorFromBaseColor(Color a_baseColor)
    {
        return GetFadedColorFromBaseColor(a_baseColor, 155);
    }

    public static Color GetFadedColorFromBaseColor(Color a_baseColor, int a_alpha)
    {
        int alpha = Math.Max(a_alpha, 0);
        alpha = Math.Min(a_alpha, 255);
        
        return Color.FromArgb(alpha, a_baseColor);
    }

    /// <summary>
    /// Returns Black or White based on the brightness of the color
    /// </summary>
    /// <param name="a_color">The color that will be drawn</param>
    /// <param name="a_brightnessOffset">
    /// Brightness offset. This can be used to prefer a lighter or darker color.
    /// [Negative numbers prefer a black compliment]
    /// [Positive numbers prefer a white compliment]
    /// </param>
    /// <returns></returns>
    public static Color GetVisibleDrawColor(Color a_color, int a_brightnessOffset = 0)
    {
        return GetVisibleDrawColor(a_color, Color.White, Color.Black, a_brightnessOffset);
    }

    /// <summary>
    /// Returns Black or White based on the brightness of the color
    /// </summary>
    /// <param name="a_color">The color that will be drawn</param>
    /// <param name="a_brightnessOffset">
    /// Brightness offset. This can be used to prefer a lighter or darker color.
    /// [Negative numbers prefer a black compliment]
    /// [Positive numbers prefer a white compliment]
    /// </param>
    /// <returns></returns>
    public static Color GetVisibleDrawColor(Color a_color, Color a_whiteColor, Color a_blackColor, int a_brightnessOffset = 0)
    {
        if (a_color == Color.Transparent)
        {
            return a_blackColor;
        }

        int brightnessLimit = 150 + a_brightnessOffset;
        if (Brightness(a_color) < brightnessLimit)
        {
            return a_whiteColor;
        }

        return a_blackColor;
    }

    /// <summary>
    /// Returns 0 for darkest, 255 for lightest.
    /// </summary>
    /// <param name="a_c"></param>
    /// <returns></returns>
    public static int Brightness(Color a_c)
    {
        return (int)Math.Sqrt(
            a_c.R * a_c.R * .241 +
            a_c.G * a_c.G * .691 +
            a_c.B * a_c.B * .068);
    }

    //public static Color GetColorFromHexString(string a_hexCode)
    //{
    //    return ColorTranslator.FromHtml(a_hexCode);
    //}

    //public static string ConvertColorToHexString(Color a_color)
    //{
    //    return ColorTranslator.ToHtml(a_color);
    //}
    public static int ConvertColorToInt32(Color a_color)
    {
        return a_color.ToArgb();
    }

    public static int ConvertColorToInt32(string a_hexString)
    {
        if (!a_hexString.Contains('#'))
        {
            if (int.TryParse(a_hexString, NumberStyles.Integer, null, out int colorInt))
            {
                return colorInt;
            }

            //If try parse failed, it's possible a known color is being imported. Try getting
            //from color name and converting the color to hex string
            Color fromName = Color.FromName(a_hexString);
            string hexValue = ConvertColorToHexString(fromName);
            return int.Parse(hexValue.Substring(1), NumberStyles.HexNumber);
        }

        return int.Parse(a_hexString.Substring(1), NumberStyles.HexNumber);
    }

    public static string ConvertColorToHexString(Color a_color)
    {
        return $"#{a_color.A:X2}{a_color.R:X2}{a_color.G:X2}{a_color.B:X2}";
    }

    public static string ConvertColorToHexString(int a_argb)
    {
        return ConvertColorToHexString(Color.FromArgb(a_argb));
    }

    public static Color GetColorFromHexString(string a_hexString)
    {
        int colorInt = ConvertColorToInt32(a_hexString);
        return Color.FromArgb(colorInt);
    }

    public static bool IsDarkColor(Color a_color)
    {
        const int c_darkcolor = 150;
        if (Brightness(a_color) < c_darkcolor)
        {
            return true;
        }

        return false;
    }
}