using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

using MaterialColorUtilities.ColorAppearance;

public static class ColorClassMapper
{
    private static readonly Dictionary<Color, string> _colorToCssClass = new()
    {
        [Color.AliceBlue] = "color-aliceblue",
        [Color.AntiqueWhite] = "color-antiquewhite",
        [Color.Aqua] = "color-aqua",
        [Color.Aquamarine] = "color-aquamarine",
        [Color.Azure] = "color-azure",
        [Color.Beige] = "color-beige",
        [Color.Bisque] = "color-bisque",
        [Color.Black] = "color-black",
        [Color.BlanchedAlmond] = "color-blanchedalmond",
        [Color.Blue] = "color-blue",
        [Color.BlueViolet] = "color-blueviolet",
        [Color.Brown] = "color-brown",
        [Color.BurlyWood] = "color-burlywood",
        [Color.CadetBlue] = "color-cadetblue",
        [Color.Chartreuse] = "color-chartreuse",
        [Color.Chocolate] = "color-chocolate",
        [Color.Coral] = "color-coral",
        [Color.CornflowerBlue] = "color-cornflowerblue",
        [Color.Cornsilk] = "color-cornsilk",
        [Color.Crimson] = "color-crimson",
        [Color.Cyan] = "color-cyan",
        [Color.DarkBlue] = "color-darkblue",
        [Color.DarkCyan] = "color-darkcyan",
        [Color.DarkGoldenrod] = "color-darkgoldenrod",
        [Color.DarkGray] = "color-darkgray",
        [Color.DarkGreen] = "color-darkgreen",
        [Color.DarkKhaki] = "color-darkkhaki",
        [Color.DarkMagenta] = "color-darkmagenta",
        [Color.DarkOliveGreen] = "color-darkolivegreen",
        [Color.DarkOrange] = "color-darkorange",
        [Color.DarkOrchid] = "color-darkorchid",
        [Color.DarkRed] = "color-darkred",
        [Color.DarkSalmon] = "color-darksalmon",
        [Color.DarkSeaGreen] = "color-darkseagreen",
        [Color.DarkSlateBlue] = "color-darkslateblue",
        [Color.DarkSlateGray] = "color-darkslategray",
        [Color.DarkTurquoise] = "color-darkturquoise",
        [Color.DarkViolet] = "color-darkviolet",
        [Color.DeepPink] = "color-deeppink",
        [Color.DeepSkyBlue] = "color-deepskyblue",
        [Color.DimGray] = "color-dimgray",
        [Color.DodgerBlue] = "color-dodgerblue",
        [Color.Firebrick] = "color-firebrick",
        [Color.FloralWhite] = "color-floralwhite",
        [Color.ForestGreen] = "color-forestgreen",
        [Color.Fuchsia] = "color-fuchsia",
        [Color.Gainsboro] = "color-gainsboro",
        [Color.GhostWhite] = "color-ghostwhite",
        [Color.Gold] = "color-gold",
        [Color.Goldenrod] = "color-goldenrod",
        [Color.Gray] = "color-gray",
        [Color.Green] = "color-green",
        [Color.GreenYellow] = "color-greenyellow",
        [Color.Honeydew] = "color-honeydew",
        [Color.HotPink] = "color-hotpink",
        [Color.IndianRed] = "color-indianred",
        [Color.Indigo] = "color-indigo",
        [Color.Ivory] = "color-ivory",
        [Color.Khaki] = "color-khaki",
        [Color.Lavender] = "color-lavender",
        [Color.LavenderBlush] = "color-lavenderblush",
        [Color.LawnGreen] = "color-lawngreen",
        [Color.LemonChiffon] = "color-lemonchiffon",
        [Color.LightBlue] = "color-lightblue",
        [Color.LightCoral] = "color-lightcoral",
        [Color.LightCyan] = "color-lightcyan",
        [Color.LightGoldenrodYellow] = "color-lightgoldenrodyellow",
        [Color.LightGray] = "color-lightgray",
        [Color.LightGreen] = "color-lightgreen",
        [Color.LightPink] = "color-lightpink",
        [Color.LightSalmon] = "color-lightsalmon",
        [Color.LightSeaGreen] = "color-lightseagreen",
        [Color.LightSkyBlue] = "color-lightskyblue",
        [Color.LightSlateGray] = "color-lightslategray",
        [Color.LightSteelBlue] = "color-lightsteelblue",
        [Color.LightYellow] = "color-lightyellow",
        [Color.Lime] = "color-lime",
        [Color.LimeGreen] = "color-limegreen",
        [Color.Linen] = "color-linen",
        [Color.Magenta] = "color-magenta",
        [Color.Maroon] = "color-maroon",
        [Color.MediumAquamarine] = "color-mediumaquamarine",
        [Color.MediumBlue] = "color-mediumblue",
        [Color.MediumOrchid] = "color-mediumorchid",
        [Color.MediumPurple] = "color-mediumpurple",
        [Color.MediumSeaGreen] = "color-mediumseagreen",
        [Color.MediumSlateBlue] = "color-mediumslateblue",
        [Color.MediumSpringGreen] = "color-mediumspringgreen",
        [Color.MediumTurquoise] = "color-mediumturquoise",
        [Color.MediumVioletRed] = "color-mediumvioletred",
        [Color.MidnightBlue] = "color-midnightblue",
        [Color.MintCream] = "color-mintcream",
        [Color.MistyRose] = "color-mistyrose",
        [Color.Moccasin] = "color-moccasin",
        [Color.NavajoWhite] = "color-navajowhite",
        [Color.Navy] = "color-navy",
        [Color.OldLace] = "color-oldlace",
        [Color.Olive] = "color-olive",
        [Color.OliveDrab] = "color-olivedrab",
        [Color.Orange] = "color-orange",
        [Color.OrangeRed] = "color-orangered",
        [Color.Orchid] = "color-orchid",
        [Color.PaleGoldenrod] = "color-palegoldenrod",
        [Color.PaleGreen] = "color-palegreen",
        [Color.PaleTurquoise] = "color-paleturquoise",
        [Color.PaleVioletRed] = "color-palevioletred",
        [Color.PapayaWhip] = "color-papayawhip",
        [Color.PeachPuff] = "color-peachpuff",
        [Color.Peru] = "color-peru",
        [Color.Pink] = "color-pink",
        [Color.Plum] = "color-plum",
        [Color.PowderBlue] = "color-powderblue",
        [Color.Purple] = "color-purple",
        [Color.RebeccaPurple] = "color-rebeccapurple",
        [Color.Red] = "color-red",
        [Color.RosyBrown] = "color-rosybrown",
        [Color.RoyalBlue] = "color-royalblue",
        [Color.SaddleBrown] = "color-saddlebrown",
        [Color.Salmon] = "color-salmon",
        [Color.SandyBrown] = "color-sandybrown",
        [Color.SeaGreen] = "color-seagreen",
        [Color.SeaShell] = "color-seashell",
        [Color.Sienna] = "color-sienna",
        [Color.Silver] = "color-silver",
        [Color.SkyBlue] = "color-skyblue",
        [Color.SlateBlue] = "color-slateblue",
        [Color.SlateGray] = "color-slategray",
        [Color.Snow] = "color-snow",
        [Color.SpringGreen] = "color-springgreen",
        [Color.SteelBlue] = "color-steelblue",
        [Color.Tan] = "color-tan",
        [Color.Teal] = "color-teal",
        [Color.Thistle] = "color-thistle",
        [Color.Tomato] = "color-tomato",
        [Color.Turquoise] = "color-turquoise",
        [Color.Violet] = "color-violet",
        [Color.Wheat] = "color-wheat",
        [Color.White] = "color-white",
        [Color.WhiteSmoke] = "color-whitesmoke",
        [Color.Yellow] = "color-yellow",
        [Color.YellowGreen] = "color-yellowgreen"
    };

    public static string GetCssClass(Color color)
    {
        var cls = _colorToCssClass.TryGetValue(color, out var cssClass) ? cssClass : "default-color-class";
        return cls;
    }

    public static string GetHexValue(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static string GetHexValue(string color)
    {
        foreach (var kvp in _colorToCssClass)
        {
            if (kvp.Value == "color-" + color)
            {
                return GetHexValue(kvp.Key);
            }
        }
        return GetHexValue(Color.Gray);
    }

    public static string GetCssClassFromHex(string hexColor)
    {
        if (hexColor.Length == 9) //actual argb, i dont think this ever happens
        {
            if (hexColor[1..3] == "00") //if its transparent do that otherwise strip the alpha because we dont have alpha blended colors
            {                           //maybe change this to some kind of clamping function later? 
                return "color-transparent";
            }
            else
            {
                hexColor = $"#{hexColor[3..]}";
            }
        }
        
        Color targetColor = ColorTranslator.FromHtml(hexColor);
        string closestColorClass = "default-color-class";
        double minDistance = double.MaxValue;
        
        foreach (var colorEntry in _colorToCssClass.Keys)
        {
            double distance = ColorDistance(targetColor, colorEntry);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestColorClass = _colorToCssClass[colorEntry];
            }
        }

        return closestColorClass;
    }

    private static double ColorDistance(Color c1, Color c2)
    {
        int c1I = c1.ToArgb();
        int c2I = c2.ToArgb();

        //i know it seems pretty silly to pull in an entire library just for this color stuff but trust me its worth it.
        //it helps **significantly** with selecting css colors that better match what the user wants
        Hct hct1 = Hct.FromInt((uint)c1I);
        Hct hct2 = Hct.FromInt((uint)c2I);

        return Math.Sqrt(Math.Pow(hct1.Hue - hct2.Hue, 2) + Math.Pow(hct1.Chroma - hct2.Chroma, 2) + Math.Pow(hct1.Tone - hct2.Tone, 2));
    }
}