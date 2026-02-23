using System.Windows.Forms;

namespace MassRecordingsUI;

public partial class WarningsPopup : Form
{
    public WarningsPopup()
    {
        InitializeComponent();
    }

    public void DisplayWarnings(string a_warningsDetails)
    {
        memoEdit_WarningsDetails.Text = a_warningsDetails;
    }
}