using System.Windows.Forms;

namespace MassRecordingsUI;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try
        {
            Application.Run(new Form1());
        }
        catch (Exception ex)
        {
            string caption = "Error found";
            MessageBoxButtons buttons = MessageBoxButtons.RetryCancel;
            DialogResult result;
            result = MessageBox.Show(ex.Message, caption, buttons);

            if (result == DialogResult.Cancel)
            {
                Application.Exit();
            }

            if (result == DialogResult.Retry)
            {
                Main();
            }
        }
    }
}