using System.Windows.Forms;

namespace PT.ComponentLibrary.Controls;

public partial class SignedSpinEditControl : UserControl
{
    public event Action SettingsModified;

    public SignedSpinEditControl()
    {
        InitializeComponent();
    }

    public decimal GetValue()
    {
        return spinEdit1.Value;
    }

    public void SetValue(decimal a_value)
    {
        spinEdit1.EditValue = a_value;
    }

    private void simpleButton_ChangeSign_Click(object sender, EventArgs e)
    {
        spinEdit1.Value *= -1;
    }

    private void spinEdit1_ValueChanged(object sender, EventArgs e)
    {
        SettingsModified?.Invoke();
    }
}