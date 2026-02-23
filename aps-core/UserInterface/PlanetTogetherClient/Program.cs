using DevExpress.XtraEditors;
using PT.APSCommon.Extensions;
using PT.APSCommon.ProgramArguments;
using PT.APSCommon.Windows.Extensions;
using PT.Common.Exceptions;
using PT.PackageDefinitionsUI.Controls.MessageControl;
using PT.UI;
using System.ComponentModel;
using System.Configuration; //Needed for ConfigurationManager
using System.IO;            //Needed for ConfigurationManager
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PT.PlanetTogetherClient;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        //Only load dlls from base windows folder and the current directory.
        if (!SystemDefinitions.AuthenticodeVerifier.SetDefaultDllDirectories())
            throw new Win32Exception(Marshal.GetLastWin32Error());

#if RELEASE
        if (bool.TryParse(ConfigurationManager.AppSettings["SecureLoad"], out bool secureParse) && secureParse)
        {
            SystemDefinitions.AuthenticodeVerifier.InitSecureLoad();
        }
#endif

        DevExpress.Skins.SkinManager.EnableFormSkins();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        //TODO: Sets to system scaling, not per monitor
        WindowsFormsSettings.SetDPIAware();

        DevExpress.LookAndFeel.UserLookAndFeel.Default.StyleChanged += new EventHandler(MainForm.Default_StyleChanged);

        // Information on custom skins registered in the main thread is not available in the splash screen thread  
        // until you call the SplashScreenManager.RegisterUserSkins method.  
        // To provide information on custom skins to the splash screen thread, uncomment the following line. 
        Assembly asm = typeof(DevExpress.UserSkins.Modern).Assembly;
        DevExpress.Skins.SkinManager.Default.RegisterAssembly(asm);
        MainForm.RegisterImages();
        MainForm.SetDefaultSkinInfo();
        MainForm.SetDefaultToolTipValues();

        //Flag to UI controls that we are executing in runtime, not design time
        RuntimeStatus.InitializeRuntime();

        // Subscribe to thread (unhandled) exception events
        // Set up the listeners before any work is done in case an exception is thrown
        Application.ThreadException += new ThreadExceptionEventHandler(MainForm.Application_ThreadException);
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MainForm.CurrentDomain_UnhandledException);

        try
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
#if SDK
                //Adding some custom command line arguments. These can be added here or in the Project Properties -> Debug section
                //SDK: Replace the CertificateThumbprint parameter with your certificate thumbprint (found in Instance Manager -> Server Manager screen). 
                commandLineArgs = commandLineArgs.Append(@"\skipclientupdater:")
                                                 .Append(@"\SystemServiceUri:https://localhost:7982")
                                                 .Append(@"\CreateInstanceOfServer:Internal") //Automatically starts and runs the server as part of the UI process. The Instance services don't need to be started.
                                                 .Append(@"\InstanceName: Debug")
                                                 .Append(@"\SoftwareVersion: 0.0")
                                                 .Append(@"\UserName: Admin")
                                                 .Append(@"\CertificateThumbprint: 1f98b094ec8c4eccb4e2eed72f734c2c9a87c66e")
                                                 .Append(@"\skiploginscreen:").ToArray();
#endif

            //Initialize the CommandLineArguments
            MainForm.CommandLineArguments = new UI.Utilities.CommandLineArgumentsHelper(commandLineArgs);
            if (MainForm.CommandLineArguments.Errors.Count > 0)
            {
                foreach (PTArgumentException argumentException in MainForm.CommandLineArguments.Errors)
                {
                    if (argumentException is ArgumentUnknownException e)
                    {
                        MainForm.LogException(e);
                    }
                }
            }
        }
        catch (Exception e)
        {
            using (PTMessageForm messageForm = new (true))
            {
                messageForm.Show(new PTException("An error was encountered while processing the command line arguments passed to the system so they won't be used.", e).CreatePTErrorMessage());
            }

            MainForm.CommandLineArguments = new UI.Utilities.CommandLineArgumentsHelper(new string[0]);
        }

        try
        {
            //The entry point for PlanetTogether that this shell implements
            Application.Run(new MainForm());
        }
        catch (Exception err)
        {
            MainForm.LogException(err);
            MainForm.ExitImmediately();
        }
    }

    // Recommended code for design-time skin initialization.  
    // In Visual Studio 2012 and higher, add the following code to your project  
    // to ensure that your custom skin assembly is loaded and that the custom skin is registered at design time.  
    public class SkinRegistration : Component
    {
        public SkinRegistration()
        {
            DevExpress.Skins.SkinManager.Default.RegisterAssembly(typeof(DevExpress.UserSkins.Modern).Assembly);
        }
    }
}