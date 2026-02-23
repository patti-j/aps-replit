namespace PT.ComponentLibrary.Controls;

public partial class CheckBoxTopLabel : DevExpress.XtraEditors.XtraUserControl
{
    public CheckBoxTopLabel()
    {
        InitializeComponent();
        checkEdit1.CheckedChanged += new EventHandler(CheckChangedHandler);
    }

    private void CheckChangedHandler(object a_sender, EventArgs a_e)
    {
        CheckValueChanged?.Invoke(a_sender, a_e);
    }

    public string LabelText
    {
        get => labelControl.Text;
        set => labelControl.Text = value;
    }

    public bool Checked
    {
        get => checkEdit1.Checked;
        set => checkEdit1.Checked = value;
    }

    public event EventHandler CheckValueChanged;
}