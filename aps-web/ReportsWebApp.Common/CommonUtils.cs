using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Microsoft.VisualBasic.CompilerServices;

namespace ReportsWebApp.Common
{
    /// <summary>
    /// Utility class containing common methods and constants.
    /// </summary>
    public static class CommonUtils
    {
        public const string DxGridCommandColumnWidth = "240px";
        public const string DxGridCommandToolbarStyleColumnWidth = "90px";
        public const string DateTimeOfficialFormat = "MMM dd yyyy - h:mm tt";
        public const string DateOfficialFormat = "MMM dd yyyy";
        private const int SERIALCODE_LENGTH = 10;
        public static string FormatSerialCodeAddSeparators(string a_serialCode)
        {
            if (a_serialCode != null && !a_serialCode.Contains("-") && a_serialCode.Length > SERIALCODE_LENGTH)
            {
                a_serialCode = a_serialCode.Insert(4, "-");
                a_serialCode = a_serialCode.Insert(9, "-");

                return a_serialCode;
            }
            else
            {
                return a_serialCode;
            }
        }
        
        public static string RemoveSerialCodeSeparators(string a_serialCode)
        {
            if (a_serialCode != null)
            {
                return a_serialCode.Replace("-", "").Replace(" ", "");
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a list of strings to a formatted string with a maximum length.
        /// </summary>
        /// <param name="values">The list of strings to convert.</param>
        /// <param name="maxLength">The maximum length of the resulting string.</param>
        /// <returns>The formatted string.</returns>
        public static string ListToString(List<string> values, int maxLength = 25)
        {
            string concatenatedValues = string.Join(", ", values);
            if (!(concatenatedValues == null || concatenatedValues.Length < maxLength || concatenatedValues.IndexOf(" ", maxLength) == -1))
            {
                return string.Format("{0}…", concatenatedValues.Substring(0, concatenatedValues.IndexOf(" ", maxLength) + 1));
            }
            else return concatenatedValues;
        }
        public static string GeneratePaApiKey()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(16);
            byte xorByte = 0;
            byte addByte = 0;
            
            for (int i = 0; i < 12; i++)
            {
                xorByte ^= bytes[i];
                addByte += bytes[i];
            }

            byte op = (byte)(xorByte ^ addByte);
            UInt16 final = 0;
            
            for (int i = 0; i < 12; i++)
            {
                final ^= (ushort)((((ushort)bytes[i] * op) + op) ^ ushort.MaxValue);
            }

            bytes[12] = xorByte;
            bytes[13] = addByte;
            bytes[14] = (byte)(final >> 8);
            bytes[15] = (byte)(final & 0xff);
            
            return Convert.ToBase64String(bytes);
        }

        public static bool ValidatePaApiKey(string a_apiKey)
        {
            byte[] bytes = Convert.FromBase64String(a_apiKey);
            byte xorByte = 0;
            byte addByte = 0;
            
            for (int i = 0; i < 12; i++)
            {
                xorByte ^= bytes[i];
                addByte += bytes[i];
            }

            byte op = (byte)(xorByte ^ addByte);
            UInt16 final = 0;
            
            for (int i = 0; i < 12; i++)
            {
                final ^= (ushort)((((ushort)bytes[i] * op) + op) ^ ushort.MaxValue);
            }

            if (bytes[12] == xorByte &&
                bytes[13] == addByte &&
                bytes[14] == (byte)(final >> 8) &&
                bytes[15] == (byte)(final & 0xff))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Formats a TimeSpan into a human-readable string.
        /// </summary>
        /// <param name="timeSpan">The TimeSpan to format.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.Days >= 365)
            {
                int years = timeSpan.Days / 365;
                return years + " year" + (years > 1 ? "s" : "");
            }
            else if (timeSpan.Days >= 30)
            {
                int months = timeSpan.Days / 30;
                return months + " month" + (months > 1 ? "s" : "");
            }
            else if (timeSpan.Days >= 1)
            {
                return timeSpan.Days + " day" + (timeSpan.Days > 1 ? "s" : "");
            }
            else if (timeSpan.Hours >= 1)
            {
                return timeSpan.Hours + " hour" + (timeSpan.Hours > 1 ? "s" : "");
            }
            else
            {
                return timeSpan.Minutes + " minute" + (timeSpan.Minutes > 1 ? "s" : "");
            }
        }

        /// <summary>
        /// Compares two objects for equality, handling IEnumerable objects as well.
        /// </summary>
        /// <param name="obj1">The first object to compare.</param>
        /// <param name="obj2">The second object to compare.</param>
        /// <returns>True if the objects are equal, false otherwise.</returns>
        public static bool IsEqualGeneric(object obj1, object obj2)
        {
            // Check if both objects are null or the same reference
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            // If either object is null, they can't be equal
            if (obj1 == null || obj2 == null)
            {
                return false;
            }

            // Check if both objects are IEnumerable
            if (obj1 is IEnumerable enumerable1 && obj2 is IEnumerable enumerable2)
            {
                // Compare the contents using SequenceEqual
                return Enumerable.Cast<object>(enumerable1).SequenceEqual(Enumerable.Cast<object>(enumerable2));
            }

            // If the objects are not IEnumerable, use the default equality comparison
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Checks if a property is a non-string enumerable.
        /// </summary>
        /// <param name="propInfo">The property information to check.</param>
        /// <returns>True if the property is a non-string enumerable, false otherwise.</returns>
        public static bool IsNonStringEnumerable(PropertyInfo propInfo)
        {
            return typeof(IEnumerable).IsAssignableFrom(propInfo.PropertyType)
                && propInfo.PropertyType != typeof(string); // Excluding individual strings as otherwise the string.Join tries to handle them too.
        }
        public static T DeepClone<T>(T obj)
        {
            return obj.Copy();
        }
        public static string NormalizeString(string input)
        {
            // Remove any characters that are not letters or digits and replace spaces with empty.
            return new string(input.Where(char.IsLetterOrDigit).ToArray()).Replace(" ", "");
        }
        public static string GetContrastTextColor(string hexColor)
        {
            // Assuming hexColor is in the format "#RRGGBB"
            if (hexColor.StartsWith("#"))
                hexColor = hexColor.Substring(1);

            int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);

            // Using the luminance formula to find the best contrast color
            double luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

            // Brightness threshold can be adjusted if necessary
            return luminance > 0.5 ? "#000000" : "#FFFFFF";
        }
        public static string DateToTimeAgo(DateTime date)
        {
            var localDate = date.ToLocalTime();
            var span = DateTime.Now - localDate;

            if (span.Days / 365d > 100)
            {
                return "Never.";
            }
            else if (span.Days / 365d > 1.2)
            {
                return $"{Math.Floor(span.Days / 365d)} year{(Math.Floor(span.Days / 365d) == 1 ? "" : "s")} ago";
            }
            else if (span.Days > 0)
            {
                return $"{span.Days} day{(span.Days == 1 ? "" : "s")} ago";
            }
            else if (span.Hours > 0)
            {
                return $"{span.Hours} hour{(span.Hours == 1 ? "" : "s")} ago";
            }
            else if (span.Minutes > 0)
            {
                return $"{span.Minutes} minute{(span.Minutes == 1 ? "" : "s")} ago";
            }
            else
            {
                return $"{span.Seconds} second{(span.Seconds == 1 ? "" : "s")} ago";
            }

        }

        public class ForeignType : Attribute { }

        public static void CopyPublicFieldsTo(this object source, object target)
        {
            if (source.GetType() != target.GetType())
            {
                throw new InvalidOperationException("Cannot copy fields of different types.");
            }

            FieldInfo[] fields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                field.SetValue(target, field.GetValue(source));
            }
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite && property.CanRead && !property.CustomAttributes.Select(x => x.AttributeType).Contains(typeof(ForeignType)))
                {
                    property.SetValue(target, property.GetValue(source));
                }
            }
        }
    }
    public static class ObjectExtensions
    {
        private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }

        public static Object Copy(this Object originalObject)
        {
            return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
        }
        private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
        {
            if (originalObject == null) return null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(typeToReflect)) return originalObject;
            if (visited.ContainsKey(originalObject)) return visited[originalObject];
            if (typeof(Delegate).IsAssignableFrom(typeToReflect)) return null;
            var cloneObject = CloneMethod.Invoke(originalObject, null);
            if (typeToReflect.IsArray)
            {
                var arrayType = typeToReflect.GetElementType();
                if (IsPrimitive(arrayType) == false)
                {
                    Array clonedArray = (Array)cloneObject;
                    clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                }

            }
            visited.Add(originalObject, cloneObject);
            CopyFields(originalObject, visited, cloneObject, typeToReflect);
            RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
            return cloneObject;
        }

        private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                if (filter != null && filter(fieldInfo) == false) continue;
                if (IsPrimitive(fieldInfo.FieldType)) continue;
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
            }
        }
        public static T Copy<T>(this T original)
        {
            return (T)Copy((Object)original);
        }
    }

    public class ReferenceEqualityComparer : EqualityComparer<Object>
    {
        public override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }
        public override int GetHashCode(object obj)
        {
            if (obj == null) return 0;
            return obj.GetHashCode();
        }
    }

    
    public static class ArrayExtensions
    {
        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0) return;
            ArrayTraverse walker = new ArrayTraverse(array);
            do action(array, walker.Position);
            while (walker.Step());
        }
    }

    internal class ArrayTraverse
    {
        public int[] Position;
        private int[] maxLengths;

        public ArrayTraverse(Array array)
        {
            maxLengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                maxLengths[i] = array.GetLength(i) - 1;
            }
            Position = new int[array.Rank];
        }

        public bool Step()
        {
            for (int i = 0; i < Position.Length; ++i)
            {
                if (Position[i] < maxLengths[i])
                {
                    Position[i]++;
                    for (int j = 0; j < i; j++)
                    {
                        Position[j] = 0;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
