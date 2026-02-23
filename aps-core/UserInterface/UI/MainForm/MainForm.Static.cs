using System.Reflection;
using System.Resources;
using System.Windows.Forms;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.File;
using PT.Common.Localization;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI.Packages.UserWorkspaces;
using PT.Scheduler;
using PT.SystemDefinitions;
using PT.SystemDefinitions.Interfaces;
using Timer = System.Windows.Forms.Timer;

namespace PT.UI;

partial class MainForm
{
    private IWorkspaceInfo m_workspaceInfo;
    public IWorkspaceInfo WorkspaceInfo => m_workspaceInfo;

    private IWorkspaceController m_workspaceController;
    private WorkspaceManagerHelper m_workspaceManagerHelper;

    internal static ClientWorkingFolder ClientWorkingFolderFlag;

    public static bool Initializing = true; //Flag that MainForm2 is setting up.

    private static Managers.ClientUpdaterFileRetriever s_clientUpdaterManager;

    /// <summary>
    /// Handles connections to client updater service
    /// </summary>
    internal static Managers.ClientUpdaterFileRetriever ClientUpdaterFileManager
    {
        get => s_clientUpdaterManager;
        set => s_clientUpdaterManager = value;
    }

    //TODO: User Collaboration Controls
    //private static CollaborationForm s_collaborationForm;

    /// <summary>
    /// Checks if the form should be accessed based on it's current state of disposal
    /// </summary>
    /// <param name="a_frm"></param>
    public static bool FormValid(Form a_frm)
    {
        return a_frm != null && !a_frm.IsDisposed && !a_frm.Disposing;
    }

    private static LoggedInInstanceInfo s_loggedInInstance;

    /// <summary>
    /// The Instance that was successfully logged into.  Null if not yet logged in.
    /// </summary>
    internal static LoggedInInstanceInfo LoggedInInstance
    {
        get => s_loggedInInstance;
        private set => s_loggedInInstance = value;
    }
    public static void ExitImmediately()
    {
        s_closeImmediately = true;

        try
        {
            foreach (Exception unreportedException in s_unreportedExceptions)
            {
                SimpleExceptionLogger.LogException(Path.Combine(Environment.CurrentDirectory, "startup exceptions.log"), unreportedException.GetExceptionFullMessage());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            Application.Exit();
        }
    }

    #region Exception Handling
    public static void Application_ThreadException(object sender, ThreadExceptionEventArgs a_e)
    {
        MainFormInstance.Invoke(new Action(() => HandleException(sender, a_e.Exception)));
    }

    public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs a_e)
    {
        if (MainFormInstance.IsDisposed)
        {
            //Nothing we can do at this point;
            //TODO: Add breakpoint in debug and attempt to figure out what this is.
            return;
        }

        MainFormInstance.Invoke(new Action(() => HandleException(sender, (Exception)a_e.ExceptionObject)));
    }

    public void UnhandledException(Exception a_e)
    {
        if (InvokeRequired)
        {
            MainFormInstance.Invoke(new Action(() => HandleException(null, a_e)));
        }
        else
        {
            HandleException(null, a_e);
        }
    }

    private static readonly List<Exception> s_unreportedExceptions = new ();

    internal static void HandleException(object sender, Exception a_e)
    {
        try
        {
            //Suppressing some know errors that I haven't been able to fix yet
            //The first is from the Infragistics Chart (KPI i think)
            //THe second is from the gantt in the JOb Dialog on close of the form
            //Argument Exception is from KPI chart when resizing after uncheckin all but %Late and Avg. Leadtime.
            if (!(a_e is MissingManifestResourceException) && !(a_e is TargetInvocationException) && !(a_e is SchedulerData.BackgroundLock.BackgroundLockException)
               )
            {
                LogExceptionInternal(a_e);
                //The exception is displayed in a dialog to the user in MainForm.NewException.
            }
            else
            {
                LogExceptionInternal(new PTHandleableException("2299", a_e));
            }

            //Not sure if we'll need this with the new logging
            //if (SystemController.Receiving && !MainFormInstance.m_clientSession.IsConnectionDown && !MainFormInstance.m_closingSoIgnoreAllEvents)
            //{
            //    UserErrorT errorT = new (SystemController.CurrentUserId, a_e, string.Format("An error occurred for User with UserId={0}.", SystemController.CurrentUserId));
            //    MainFormInstance.m_clientSession.SendClientAction(errorT);
            //}
        }
        catch (Exception ex)
        {
            #if RELEASE
            if (!MainFormInstance.IsDisposed && MainFormInstance.Visible) //These can occur during app shutdown, no point to show an error at this point
            #endif
            {
                MainFormInstance.m_messageProvider.ShowMessageBoxError(ex, true, "Problem logging exception".Localize());
            }
        }
    }
    private static ICommonLogger s_uiErrorLogger;

    public void LogNotification(string a_message, string a_info)
    {
        string fullMessage = string.Format("{0}{1}{2}{1}{3}", a_message, Environment.NewLine, SimpleExceptionLogger.NOTIFICATION_INFO_TEXT, a_info);
        SystemController.Sys.SystemLoggerInstance.Log(a_message, fullMessage);
    }

    public void ClearNotificationsLog()
    {
        SystemController.Sys.SystemLoggerInstance.ClearLog(ELogClassification.Notifications);
    }

    public static void LogException(Exception a_e)
    {
        try
        {
            // Attempt to log with full handling, send error transmission to server if possible, etc.
            if (MainFormInstance != null)
            {
                MainFormInstance.Invoke(new Action(() => HandleException(null, a_e)));
            }
            else
            {
                LogExceptionInternal(a_e);
            }
        }
        catch (Exception ex)
        {
            try
            {
                // Fallback standard logging in case something unexpected occurs.
                LogExceptionInternal(ex);
                LogExceptionInternal(a_e);
            }
            catch (Exception e)
            {
                // If we fail here as well, logging is grievously misbehaving. This will just write to the default error logger and avoid any other handling. 
                SimpleExceptionLogger.LogException(string.Empty, e);
            }
        }
    }

    private static void LogExceptionInternal<T>(T a_e)
    {
        if (s_uiErrorLogger == null)
        {
            s_unreportedExceptions.Add(a_e as Exception);
            return;
        }

        if (a_e is PTException ptException)
        {
            s_uiErrorLogger.LogException(ptException, null, ptException.LogToSentry);
        }
        else //Unhandled exception
        {
            s_uiErrorLogger.LogException(a_e as Exception, null, true);
        }
    }

    public void LogSimpleException(Exception a_e)
    {
        LogException(a_e);
    }

    public void LogException(Exception a_e, bool a_showMessage)
    {
        if (a_showMessage && MainFormInstance.m_messageProvider != null)
        {
            MainFormInstance.m_messageProvider.ShowMessage(a_e.Message);
        }

        LogException(a_e);
    }
    #endregion

    private bool m_closingSoIgnoreAllEvents;

    /// <summary>
    /// Set to true when UI is closing so no need to remove the event listeners.
    /// </summary>
    public bool ClosingSoIgnoreAllEvents => m_closingSoIgnoreAllEvents;

    #region Message Showing
    // A reference to the instance of mainForm.
    private static MainForm s_mainForm;
    internal static MainForm MainFormInstance => s_mainForm;

    public static void ShowBusyMessage()
    {
        s_busyMessageTimer.Interval = 10000; //10 seconds to give time to see then clear it so it's not sitting there all the time.
        s_busyMessageTimer.Start();
    }

    private static readonly Timer s_busyMessageTimer = new ();
    #endregion
}