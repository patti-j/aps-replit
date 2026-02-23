using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace PT.Common
{
    /// <summary>
    /// PTColor is just a wrapper for the System.Drawing.Color struct, just so we can implement IComparable for Sorting in our grids
    /// since the Color struct doesn't implement IComparable which caused devExpress exceptions to be thrown when sorting Color columns on the grid.
    /// Also, we implemented, IEquatable for Searching and IConvertible to support conversion to and from Color when grid editors need to load and save color values
    /// </summary>
    [TypeConverter(typeof(PTColorConverter))]
    public readonly struct PTColor : IComparable, IConvertible, IEquatable<PTColor?>
    {
        public Color Color { get; }

        public PTColor(Color a_color)
        {
            Color = a_color;
        }

        public override string ToString()
        {
            return ColorUtils.ConvertColorToHexString(Color);
        }

        public bool Equals(PTColor? a_other)
        {
            if (a_other == null)
            {
                return false;
            }

            return Color.Equals(a_other.Value.Color);
        }
        public int CompareTo(object a_other)
        {
            if (a_other != null)
            {
                return ((PTColor)a_other).Color.ToArgb().CompareTo(Color.ToArgb());
            }

            return -1;
        }
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider a_provider)
        {
            return Color.ToArgb() != 0;
        }

        public byte ToByte(IFormatProvider a_provider)
        {
            return Convert.ToByte(Color.ToArgb(), a_provider);
        }

        public char ToChar(IFormatProvider a_provider)
        {
            return Convert.ToChar(Color.ToArgb(), a_provider);
        }

        public DateTime ToDateTime(IFormatProvider a_provider)
        {
            return Convert.ToDateTime(Color.ToArgb(), a_provider);
        }

        public decimal ToDecimal(IFormatProvider a_provider)
        {
            return Convert.ToDecimal(Color.ToArgb(), a_provider);
        }

        public double ToDouble(IFormatProvider a_provider)
        {
            return Convert.ToDouble(Color.ToArgb(), a_provider);
        }

        public short ToInt16(IFormatProvider a_provider)
        {
            return Convert.ToInt16(Color.ToArgb(), a_provider);
        }

        public int ToInt32(IFormatProvider a_provider)
        {
            return Color.ToArgb();
        }

        public long ToInt64(IFormatProvider a_provider)
        {
            return Convert.ToInt64(Color.ToArgb(), a_provider);
        }

        public sbyte ToSByte(IFormatProvider a_provider)
        {
            return Convert.ToSByte(Color.ToArgb(), a_provider);
        }

        public float ToSingle(IFormatProvider a_provider)
        {
            return Convert.ToSingle(Color.ToArgb(), a_provider);
        }

        public string ToString(IFormatProvider a_provider)
        {
            return Color.ToString();
        }

        public object ToType(Type a_conversionType, IFormatProvider a_provider)
        {
            if (a_conversionType == typeof(Color))
            {
                return Color;
            }

            if (a_conversionType == typeof(string))
            {
                return ToString(a_provider);
            }

            if (a_conversionType == typeof(int))
            {
                return ToInt32(a_provider);
            }

            if (a_conversionType == typeof(bool))
            {
                return ToBoolean(a_provider);
            }

            if (a_conversionType == typeof(byte))
            {
                return ToByte(a_provider);
            }

            if (a_conversionType == typeof(char))
            {
                return ToChar(a_provider);
            }

            if (a_conversionType == typeof(DateTime))
            {
                return ToDateTime(a_provider);
            }

            if (a_conversionType == typeof(decimal))
            {
                return ToDecimal(a_provider);
            }

            if (a_conversionType == typeof(double))
            {
                return ToDouble(a_provider);
            }

            if (a_conversionType == typeof(short))
            {
                return ToInt16(a_provider);
            }

            if (a_conversionType == typeof(long))
            {
                return ToInt64(a_provider);
            }

            if (a_conversionType == typeof(sbyte))
            {
                return ToSByte(a_provider);
            }

            if (a_conversionType == typeof(float))
            {
                return ToSingle(a_provider);
            }

            if (a_conversionType == typeof(ushort))
            {
                return ToUInt16(a_provider);
            }

            if (a_conversionType == typeof(uint))
            {
                return ToUInt32(a_provider);
            }

            if (a_conversionType == typeof(ulong))
            {
                return ToUInt64(a_provider);
            }

            return this;
        }

        public ushort ToUInt16(IFormatProvider a_provider)
        {
            return Convert.ToUInt16(Color.ToArgb(), a_provider);
        }

        public uint ToUInt32(IFormatProvider a_provider)
        {
            return Convert.ToUInt32(Color.ToArgb(), a_provider);
        }

        public ulong ToUInt64(IFormatProvider a_provider)
        {
            return Convert.ToUInt64(Color.ToArgb(), a_provider);
        }
    }

    public class PTColorConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? a_context, Type a_sourceType)
        {
            return a_sourceType == typeof(int) || a_sourceType == typeof(string) || a_sourceType == typeof(Color) || base.CanConvertFrom(a_context, a_sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? a_context, CultureInfo? a_culture, object a_value)
        {
            if (a_value is int argb)
            {
                return new PTColor(Color.FromArgb(argb));
            }

            if (a_value is string str)
            {
                if (int.TryParse(str, NumberStyles.HexNumber, a_culture ?? CultureInfo.InvariantCulture, out int parsedArgb))
                {
                    return new PTColor(Color.FromArgb(parsedArgb));
                }

                throw new FormatException($"Cannot convert '{str}' to PTColor — expected ARGB hex string.");
            }

            if (a_value is Color color)
            {
                return new PTColor(color);
            }

            return base.ConvertFrom(a_context, a_culture, a_value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? a_context, Type a_destinationType)
        {
            return a_destinationType == typeof(int) || a_destinationType == typeof(string) || a_destinationType == typeof(Color) || base.CanConvertTo(a_context, a_destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext? a_context, CultureInfo? a_culture, object a_value, Type a_destinationType)
        {
            if (a_value is PTColor pt)
            {
                if (a_destinationType == typeof(int))
                {
                    return pt.Color.ToArgb();
                }

                if (a_destinationType == typeof(string))
                {
                    return pt.Color.ToArgb().ToString("X8");
                }

                if (a_destinationType == typeof(Color))
                {
                    return pt.Color;
                }
            }

            return base.ConvertTo(a_context, a_culture, a_value, a_destinationType);
        }
    }
}
