using System.Windows.Forms;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.ComponentLibrary.Controls;
using PT.SchedulerData;

namespace PT.ScenarioControls;

public class AsyncLock : BackgroundLock, IDisposable
{
    private const string c_lockingText = "Waiting for updated data";

    private readonly ESplashVisibility m_splashVisibility;
    private DataLoadingSplash m_splashScreen;
    private DataLoadingSplashLite m_splashScreenLite;
    private Action m_postSplashShownAction;
    private bool m_splashShown;
    private readonly string m_loadingText;
    protected Control m_parentControl;
    private readonly bool m_liteMode;

    /// <summary>
    /// Handles background processing of the PT locks. The UI will not be blocked while waiting for lock
    /// </summary>
    /// <param name="a_parentControl">The control on which to invoke any function calls. Optional if running lock code does not require invoking a UI controls thread.</param>
    /// <param name="a_scenarioId">Scenario Id in which to lock</param>
    /// <param name="a_splashVisibility">Determines if and when the splash screen should be shown</param>
    /// <param name="a_loadingText">Text to display while locking</param>
    public AsyncLock(Control a_parentControl, BaseId a_scenarioId, ESplashVisibility a_splashVisibility, string a_loadingText = c_lockingText)
        : base(a_scenarioId)
    {
        m_parentControl = a_parentControl;
        m_loadingText = a_loadingText.Localize();
        m_splashVisibility = a_splashVisibility;
        if (m_splashVisibility == ESplashVisibility.Always || m_splashVisibility == ESplashVisibility.AlwaysWithCancel)
        {
            m_parentControl.Invoke(new Common.Delegates.VoidDelegate(ShowSplash));
        }
    }

    /// <summary>
    /// Handles background processing of the PT locks. The UI will not be blocked while waiting for lock
    /// </summary>
    /// <param name="a_parentControl">The control on which to invoke any function calls. Optional if running lock code does not require invoking a UI controls thread.</param>
    /// <param name="a_scenarioId">Scenario Id in which to lock</param>
    public AsyncLock(Control a_parentControl, BaseId a_scenarioId) : base(a_scenarioId)
    {
        m_parentControl = a_parentControl;
        m_splashVisibility = ESplashVisibility.AsNeeded;
        m_liteMode = true;
    }

    private static void SplashHelper(AsyncLock a_lock)
    {
        if (a_lock != null && a_lock.m_parentControl != null)
        {
            if (a_lock.m_liteMode)
            {
                a_lock.ShowLiteSplash();
            }
            else
            {
                a_lock.ShowSplash();
            }
        }
    }
    
    public enum ESplashVisibility { Always, AlwaysWithCancel, AsNeeded, AsNeededWithCancel }

    /// <summary>
    /// Sets the action to invoke after the splash screen has been shown.
    /// </summary>
    public void SetPostSplashShownHandler(Action a_action)
    {
        m_postSplashShownAction = a_action;
    }

    protected override void InvokeSplashScreen()
    {
        //This used to check for !m_parentControl.Visible, but popup controls that call this are not visible.
        if (m_splashShown || (m_splashVisibility != ESplashVisibility.AsNeeded && m_splashVisibility != ESplashVisibility.AsNeededWithCancel))
        {
            return;
        }
        m_parentControl.BeginInvoke(() =>
        {
            SplashHelper(this);
        });
    }

    protected override void InvokeDisableCancel()
    {
        if (m_splashShown && (m_splashVisibility == ESplashVisibility.AlwaysWithCancel || m_splashVisibility == ESplashVisibility.AsNeededWithCancel))
        {
            m_parentControl.Invoke(new Common.Delegates.VoidDelegate(SplashDisableCancel));
        }
    }

    private void InvokePostSplashScreen()
    {
        m_postSplashShownAction?.Invoke();
    }

    private void ShowSplash()
    {
        m_splashScreen = new DataLoadingSplash();
        m_splashScreen.Dock = DockStyle.Fill;
        if (m_splashVisibility == ESplashVisibility.AlwaysWithCancel || m_splashVisibility == ESplashVisibility.AsNeededWithCancel)
        {
            m_splashScreen.AllowCancel = true;
            m_splashScreen.Canceled += new DataLoadingSplash.CanceledDelegate(LoadCanceled);
        }

        //m_parentControl.BeginInvoke(new Action(() => AddSplashToParent(a_message)));
        m_parentControl.Controls.Add(m_splashScreen);
        m_splashScreen.SetLoadingText(m_loadingText);
        m_splashScreen.BringToFront();
        m_splashScreen.Refresh();
        m_splashShown = true;
        InvokePostSplashScreen();
    }

    private void ShowLiteSplash()
    {
        m_splashScreenLite = new DataLoadingSplashLite();
        m_splashScreenLite.Dock = DockStyle.Fill;

        //m_parentControl.BeginInvoke(new Action(() => AddSplashToParent(a_message)));
        m_parentControl.Controls.Add(m_splashScreenLite);
        m_splashScreenLite.BringToFront();
        m_splashScreenLite.Refresh();
        m_splashShown = true;
        InvokePostSplashScreen();
    }

    private void SplashDisableCancel()
    {
        m_splashScreen.AllowCancel = false;
    }

    private void LoadCanceled()
    {
        m_canceled = true;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();

        //Invoke dispose.
        m_parentControl.Invoke(new Action(() =>
        {
            if (m_liteMode)
            {
                m_splashScreenLite?.Dispose();
            }
            else
            {
                //m_parentControl.Controls.Remove(m_splashScreen);
                m_splashScreen?.Dispose();
            }
        }));

        m_parentControl = null;
    }
}