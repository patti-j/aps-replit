using System.Windows.Forms;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Windows;
using PT.SchedulerDefinitions;

namespace PT.ComponentLibrary.Forms;

public partial class AddUserField : BaseResizableForm
{
    public AddUserField() : base("AddUserField")
    {
        InitializeComponent();

        fieldDataTypeCombo.Properties.Items.AddEnum(typeof(UserField.UDFTypes));
        fieldDataTypeCombo.EditValue = UserField.UDFTypes.String;
    }

    private Dictionary<string, string> m_existingColumnNamesInCaps;

    public void SetExistingFieldNames(Dictionary<string, string> a_existingColumnNamesInCaps)
    {
        m_existingColumnNamesInCaps = a_existingColumnNamesInCaps;
    }

    public override void Localize()
    {
        UILocalizationHelper.LocalizeFormCaption(this);
        UILocalizationHelper.LocalizeControlsRecursively(Controls, fieldDataTypeCombo);
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        bool valid = true;
        if (fieldNameTB.Text.Trim().Length == 0)
        {
            m_messageProvider.ShowMessageBox(new PTMessage("Please enter a Field Name.".Localize(), "Blank Field Name".Localize()) { Classification = PTMessage.EMessageClassification.Error }, true);
            valid = false;
        }

        if (m_existingColumnNamesInCaps.ContainsKey(FieldName.ToUpper()))
        {
            m_messageProvider.ShowMessageBox(new PTMessage("A field with that name already exists.".Localize(), "Duplicate Field Name".Localize()) { Classification = PTMessage.EMessageClassification.Error }, true);
            valid = false;
        }

        if (valid)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    public string FieldName => fieldNameTB.Text;

    public UserField.UDFTypes FieldDataType => (UserField.UDFTypes)fieldDataTypeCombo.EditValue;
}