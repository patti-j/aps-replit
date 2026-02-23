using System.Diagnostics;
using System.Windows.Forms;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Windows;
using PT.PackageDefinitionsUI.Controls;
using PT.UIDefinitions;

namespace PT.UI;

partial class MainForm
{
    private Utilities.CommandLineArgumentsHelper m_lastLoginSettings;
    private static readonly string s_connectionLostString = "Please wait. Attempting to re-establish connection. Click button to close the client.".Localize();

    /// <summary>
    /// If successully logged in then this stores the settings that were used.
    /// </summary>
    private Utilities.CommandLineArgumentsHelper LastLoginSetttings
    {
        get => m_lastLoginSettings;
        set => m_lastLoginSettings = value;
    }

    #region Logout
    private async Task Logout()
    {
        try
        {
            await m_clientSession.LogOff();
        }
        catch (Exception e)
        {
            LogException(e);
        }
    }
    #endregion Logout

    private bool m_deadConnectionMessageShown;

    private void connectionChecker_DeadConnectionEvent(Exception a_serverReturnedException)
    {
        if (m_deadConnectionMessageShown) //this can keep showing if the user isn't there to ok the message.  Can get thousands of windows showing!
        {
            return;
        }

        m_deadConnectionMessageShown = true;
        if (InvokeRequired)
        {
            Invoke(new Action(DeadConnectionShowError));
        }
        else
        {
            DeadConnectionShowError();
        }

        //TODO: Maybe log in a better spot, with a new exception
        m_errorLogger.LogException(a_serverReturnedException, null, false);

        if (LastLoginSetttings != null)
        {
            Process.Start(Application.ExecutablePath, LastLoginSetttings.CreateArgumentString());
        }

        s_userDeleted = true;
        s_closeImmediately = true;
        if (InvokeRequired)
        {
            Invoke(new Action(Close));
        }
        else
        {
            Close();
        }
    }

    private void DeadConnectionShowError()
    {
        //Only show the message if the user is active, otherwise just restart
        if (UserIsActive)
        {
            string message = "The client is disconnected from the server and will now restart to reestablish the connection." + Environment.NewLine + "Would you like to browse the client logs?";
            PTMessage ptMessage = new (message, "Disconnected from the server");
            ptMessage.Classification = PTMessage.EMessageClassification.Question;
            DialogResult result = m_messageProvider.ShowMessageBox(ptMessage, true, false, this);
            if (result == DialogResult.Yes)
            {
                try
                {
                    using (new MultiLevelHourglass())
                    {
                        Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", ClientWorkingFolder.ClientDataFolder);
                    }
                }
                catch (Exception err)
                {
                    m_messageProvider.ShowMessageBoxError(err, true, null, this);
                }
            }
        }
    }

    private OverlayForm m_overlayForm;

    /// <summary>
    /// Called when client can't communicate with server.
    /// </summary>
    /// <param name="a_exception">exception provided when communication fails (e.g. TimeoutException)</param>
    /// <param name="a_lastReceiveDate">Last time communication time in UTC</param>
    /// <param name="a_timeoutMilliseconds">if connection isn't restored by a_lastReceiveDate + a_timeoutMilliseconds the client needs to restart</param>
    private void ConnectionChecker_SystemServiceUnavailableEvent(Exception a_exception, long a_lastReceiveDate, double a_timeoutMilliseconds)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(ShowLoginOverlay));
        }
        else
        {
            ShowLoginOverlay();
        }
    }

    private void ConnectionChecker_SystemServiceAvailableEvent()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(HideLoginOverlay));
        }
        else
        {
            HideLoginOverlay();
        }
    }

    private void ShowLoginOverlay()
    {
        m_overlayForm = new OverlayForm(this, m_brand.ActiveTheme);
        m_overlayForm.Cancelled += OverlayFormOnCancelled;
        m_overlayForm.SetLoadingText(s_connectionLostString);

        m_overlayForm.ShowOverlay();
    }

    private void OverlayFormOnCancelled()
    {
        BeginInvoke(new Action(ExitImmediately));
    }

    private void HideLoginOverlay()
    {
        m_overlayForm?.HideOverlay();
    }
}