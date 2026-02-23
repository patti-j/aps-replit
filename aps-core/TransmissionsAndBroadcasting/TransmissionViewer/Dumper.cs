using System.Reflection;
using System.Text;

namespace TransmissionViewer;

/// <summary>
/// Summary description for Dumper.
/// </summary>
public class Dumper
{
    protected Dumper() { }

    public static string DumpObject(object a_obj)
    {
        return DumpObject(a_obj, -1);
    }

    public static string DumpObject(object a_obj, int a_maxLevel)
    {
        StringBuilder sb = new ();
        if (a_obj == null)
        {
            return "Nothing";
        }

        PrivDump(sb, a_obj, "[ObjectToDump]", 0, a_maxLevel);
        return sb.ToString();
    }

    protected static void PrivDump(StringBuilder a_sb, object a_obj, string a_objName, int a_level, int a_maxLevel)
    {
        if (a_obj == null)
        {
            return;
        }

        if (a_maxLevel >= 0 && a_level >= a_maxLevel)
        {
            return;
        }

        string padstr = "";
        for (int i = 0; i < a_level; i++)
        {
            if (i < a_level - 1)
            {
                padstr += "|";
            }
            else
            {
                padstr += "+";
            }
        }

        Type t = a_obj.GetType();
        string[] strarr = new string[7];
        strarr[0] = padstr;
        strarr[1] = a_objName;
        strarr[2] = " AS ";
        strarr[3] = t.FullName;
        strarr[4] = " = ";
        strarr[5] = $"[{a_obj}]";
        strarr[6] = "\r\n";

        if (a_obj is char c)
        {
            //Is it possible that the char is an end of string character. We must not write that value to stringbuilder as it will terminate the resulting ToString().
            if (strarr[5] == "[\0]")
            {
                strarr[5] = "[EOS]";
            }
        }

        a_sb.Append(string.Concat(strarr));
        if (a_obj.GetType().BaseType == typeof(ValueType))
        {
            return;
        }

        DumpType(padstr, a_sb, a_obj, a_level, t, a_maxLevel);
        Type bt = t.BaseType;
        if (bt != null)
        {
            while (!(bt == typeof(object)))
            {
                string str = bt.FullName;
                a_sb.Append(padstr + "(" + str + ")\r\n");
                DumpType(padstr, a_sb, a_obj, a_level, bt, a_maxLevel);
                bt = bt.BaseType;
                if (bt != null)
                {
                    continue;
                }

                break;
            }
        }
    }

    protected static void DumpType(string a_initialStr, StringBuilder a_sb, object a_obj, int a_level, Type a_t, int a_maxlevel)
    {
        FieldInfo[] fi = a_t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (a_t == typeof(Delegate))
        {
            return;
        }

        foreach (FieldInfo f in fi)
        {
            PrivDump(a_sb, f.GetValue(a_obj), f.Name, a_level + 1, a_maxlevel);
        }

        if (a_obj is Array)
        {
            try
            {
                object[] arl = (object[])a_obj;
                for (int i = 0; i < arl.GetLength(0); i++)
                {
                    PrivDump(a_sb, arl[i], "[" + i + "]", a_level + 1, a_maxlevel);
                }
            }
            catch (Exception) { }
        }
    }
}