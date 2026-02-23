using DevExpress.XtraEditors;

using PT.APSCommon.Extensions;

namespace PT.ComponentLibrary.Extensions;

public static class ComboBoxEditExtensions
{
    public static void AddEnumValues(this ComboBoxEdit a_comboBox, Type a_enumType)
    {
        Array enumValues = Enum.GetValues(a_enumType);
        foreach (object enumValue in enumValues)
        {
            a_comboBox.Properties.Items.Add(enumValue.ToString().Localize());
        }
    }
}