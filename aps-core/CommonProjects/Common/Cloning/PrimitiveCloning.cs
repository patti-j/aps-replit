using System.Reflection;

namespace PT.Common.Cloning;

public static class PrimitiveCloning
{
    private static readonly Type s_stringType = typeof(string);
    private static readonly Type s_decimalType = typeof(decimal);

    public enum OtherIncludedTypes
    {
        None = 0,
        String = 1,
        DecimalValueType = 2,
        AllValueTypes = 4,
        All = AllValueTypes * 2 - 1
    }

    public enum Depth { Shallow = 1, Deep = 2 }

    public static void PrimitiveClone(object a_obj, object a_newObj, Type a_objType, Depth a_depth, OtherIncludedTypes a_otherTypes)
    {
        if (a_obj.GetType() != a_newObj.GetType())
        {
            throw new InvalidCastException();
        }

        while (a_objType != null)
        {
            FieldInfo[] fiArray =
                a_objType.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo fi in fiArray)
            {
                Type fieldType = fi.FieldType;
                bool b = fieldType == s_stringType;

                if (fieldType.IsPrimitive ||
                    (fieldType == s_stringType && ((int)a_otherTypes & (int)OtherIncludedTypes.String) > 0) ||
                    (fieldType == s_decimalType && ((int)a_otherTypes & (int)OtherIncludedTypes.DecimalValueType) > 0))
                {
                    fi.SetValue(a_newObj, fi.GetValue(a_obj));
                }
                else if (fieldType.IsValueType && ((int)a_otherTypes & (int)OtherIncludedTypes.AllValueTypes) > 0)
                {
                    object newObjFieldValue = fi.GetValue(a_newObj);
                    PrimitiveClone(fi.GetValue(a_obj), newObjFieldValue, fieldType, a_depth, a_otherTypes);
                    fi.SetValue(a_newObj, newObjFieldValue);
                }
            }

            a_objType = a_depth == Depth.Deep ? a_objType.BaseType : null;
        }
    }
}