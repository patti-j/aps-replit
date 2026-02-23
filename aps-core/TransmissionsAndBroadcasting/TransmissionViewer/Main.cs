using System.Windows.Forms;

namespace TransmissionViewer;

/// <summary>
/// Summary description for Main.
/// </summary>
public class StartupClass
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length == 1 || args.Length == 2)
        {
            Application.Run(new Form1(args));
        }
        else
        {
            Application.Run(new OpenStartupForm());
        }
    }
}