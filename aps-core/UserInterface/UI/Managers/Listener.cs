using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using PT.Transmissions.User;

namespace PT.UI.Managers;

/// <summary>
/// Listens to evenst that effect the MainForm.
/// </summary>
public class MainFormListener
{
    public delegate void UserUpdateDelegate(User u, UserManager um, UserIdBaseT t);

    public delegate void OfflineStatusChangedDelegate(PTTransmission t);

    public delegate void PopupMessageDelegate(string message, bool showAllUsers, BaseId showForThisUser);

    public delegate void SystemStateSwitchDelegate(SystemStateSwitchT a_t);

    public delegate void RuleSeekSimulationStatusUpdateDelegate(ScenarioManager sm, CoPilotStatusUpdateT a_t);

    public delegate void RuleSeekSimulationStopDelegate(ScenarioManager sm, RuleSeekEndReasons a_reason);

    public delegate void SystemProcessingDelegate(PTTransmissionBase a_t);

    internal MainFormListener(MainForm aMainForm, ISystemLogger a_errorReporter)
    {
        m_mainForm = aMainForm;

        AddRemoveListeners(true, a_errorReporter);

        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            //This sets the default message when starting the client. 
            //Currently there is no way to determine if the Simulation is running or not.
            //It can be determeined if it is enabled.
            if (sm.RuleSeekSettings.Enabled)
            {
                sm_RuleSeekStatusUpdate(null, new CoPilotStatusUpdateT(CoPilotStatusUpdateT.CoPilotStatusUpdateValues.Enabled, CoPilotStatusUpdateT.CoPilotErrorValues.None));
            }
            else
            {
                sm_RuleSeekStopped(null, RuleSeekEndReasons.Dissabled);
            }
        }
    }

    internal void RemoveAllListeners(ISystemLogger a_errorReporter)
    {
        AddRemoveListeners(false, a_errorReporter);
    }

    private void AddRemoveListeners(bool a_add, ISystemLogger a_errorReporter)
    {
        AddRemoveUserManagerEventListeners(a_add);
        AddRemoveSystemEventListeners(a_add, a_errorReporter);

        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            AddRemoveScenarioManagerEventListeners(sm, a_add);
            for (int i = 0; i < sm.LoadedScenarioCount; i++)
            {
                Scenario s = sm.GetByIndex(i);
                using (s.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
                {
                    AddRemoveScenarioEventListeners(se, a_add);
                }
            }
        }
    }

    private readonly MainForm m_mainForm;

    #region User Events
    /// <summary>
    /// Add events from the User Manager.
    /// </summary>
    internal void AddRemoveUserManagerEventListeners(bool a_add)
    {
        //Users
        using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents umEvents))
        {
            if (a_add)
            {
                umEvents.UserDefaultEvent += new UserManagerEvents.UserDefaultDelegate(ReceiveUserDefaultTransmission);
                umEvents.UserCopyEvent += new UserManagerEvents.UserCopyDelegate(ReceiveUserCopyTransmission);
                umEvents.UserChatEvent += new UserManagerEvents.UserChatDelegate(ReceiveUserChatTransmission);
                umEvents.UserAdminLogOffEvent += new UserManagerEvents.UserAdminLogoffDelegate(um_UserAdminLogoffTransmission);
                umEvents.UserPermissionsUpdatedEvent += um_UserPermissionsUpdatedEvent;
                umEvents.ScenarioDataChangesEvent += new Action<IScenarioDataChanges>(um_ScenarioDataChangesEvent);
            }
            else //Remove
            {
                umEvents.UserDefaultEvent -= new UserManagerEvents.UserDefaultDelegate(ReceiveUserDefaultTransmission);
                umEvents.UserCopyEvent -= new UserManagerEvents.UserCopyDelegate(ReceiveUserCopyTransmission);
                umEvents.UserChatEvent -= new UserManagerEvents.UserChatDelegate(ReceiveUserChatTransmission);
                umEvents.UserAdminLogOffEvent -= new UserManagerEvents.UserAdminLogoffDelegate(um_UserAdminLogoffTransmission);
                umEvents.UserPermissionsUpdatedEvent -= um_UserPermissionsUpdatedEvent;
                umEvents.ScenarioDataChangesEvent -= new Action<IScenarioDataChanges>(um_ScenarioDataChangesEvent);
            }
        }
    }

    internal void AddRemoveScenarioManagerEventListeners(ScenarioManager a_sm, bool a_add)
    {
        if (a_add)
        {
            a_sm.RuleSeekStatusEvent += new ScenarioManager.RuleSeekStatusDelegate(sm_RuleSeekStatusUpdate);
            a_sm.RuleSeekStoppedEvent += new ScenarioManager.RuleSeekStoppedDelegate(sm_RuleSeekStopped);
        }
        else //Remove
        {
            a_sm.RuleSeekStatusEvent -= new ScenarioManager.RuleSeekStatusDelegate(sm_RuleSeekStatusUpdate);
            a_sm.RuleSeekStoppedEvent -= new ScenarioManager.RuleSeekStoppedDelegate(sm_RuleSeekStopped);
        }
    }

    private void sm_RuleSeekStatusUpdate(ScenarioManager a_sm, CoPilotStatusUpdateT a_t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.Invoke(new RuleSeekSimulationStatusUpdateDelegate(m_mainForm.RuleSeekStatusUpdateHandling), a_sm, a_t);
    }

    private void sm_RuleSeekStopped(ScenarioManager a_sm, RuleSeekEndReasons a_reason)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.Invoke(new RuleSeekSimulationStopDelegate(m_mainForm.RuleSeekSimulationStopHandling), a_sm, a_reason);
    }

    private void ReceiveUserChatTransmission(User u, UserChatT t)
    {
        if (IgnoreEvents) { }
        //TODO: Chat
        //m_mainForm.Invoke(new UserManagerEvents.UserChatDelegate(m_mainForm.ChatUser), new object[] {u, (UserChatT)t});
    }

    private void ReceiveUserDefaultTransmission(User u, UserManager um, UserDefaultT t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.Invoke(new UserManagerEvents.UserDefaultDelegate(m_mainForm.DefaultUser), u, um, t);
    }

    private void ReceiveUserCopyTransmission(User u, UserManager um, UserCopyT t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.Invoke(new UserManagerEvents.UserCopyDelegate(m_mainForm.CopyUser), u, um, t);
    }

    private void um_ScenarioDataChangesEvent(IScenarioDataChanges a_userChanges)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.BeginInvoke(new Action<IScenarioDataChanges>(m_mainForm.UpdateUsers), a_userChanges);
    }

    private void um_UserAdminLogoffTransmission(User a_u, PTTransmission a_t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.BeginInvoke(new UserManagerEvents.UserAdminLogoffDelegate(m_mainForm.UserAdminLogOff), a_u, a_t);
    }

    private void um_UserPermissionsUpdatedEvent()
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.BeginInvoke(new Action(m_mainForm.UserPermissionsUpdated));
    }
    #endregion User Events

    #region Scenario Events
    /// <summary>
    /// Adds event listeners for a specified Scenario.  This is done each time a Scenario is opened so it is kept up to date in the UI.
    /// </summary>
    internal void AddRemoveScenarioEventListeners(ScenarioEvents se, bool add)
    {
        if (add)
        {
            se.TransmissionFailureEvent += new PT.Scheduler.ScenarioEvents.TransmissionFailureDelegate(ReceiveTransmissionFailure);
            se.OfflineStatusChangedEvent += new ScenarioEvents.OfflineStatusChangedDelegate(se_OfflineStatusChangedEvent);
            se.PopupMessageEvent += new ScenarioEvents.PopupMessageDelegate(se_PopupMessageEvent);
        }
        else //Remove
        {
            se.TransmissionFailureEvent -= new PT.Scheduler.ScenarioEvents.TransmissionFailureDelegate(ReceiveTransmissionFailure);
            se.OfflineStatusChangedEvent -= new PT.Scheduler.ScenarioEvents.OfflineStatusChangedDelegate(se_OfflineStatusChangedEvent);
            se.PopupMessageEvent -= new ScenarioEvents.PopupMessageDelegate(se_PopupMessageEvent);
        }
    }
    #endregion Scenario Events

    #region System Events
    /// <summary>
    /// Adds event listeners at the system level.
    /// </summary>
    internal void AddRemoveSystemEventListeners(bool a_add, ISystemLogger a_errorLogger)
    {
        if (a_add)
        {
            a_errorLogger.TransmissionFailureEvent += new TransmissionFailureDelegate(ReceiveTransmissionFailure);
            a_errorLogger.ExceptionOccurredEvent += new ExceptionOccurredDelegate(ReceiveExceptionOcurred);

            //SystemController.Receiver.DeadConnectionEvent += Receiver_DeadConnectionEvent;
            //SystemController.Receiver.SystemServiceUnavailableEvent += Receiver_SystemServiceUnavailableEvent;
            //SystemController.Receiver.SystemServiceAvailableEvent += Receiver_SystemServiceAvailableEvent;
        }
        else //Remove
        {
            a_errorLogger.TransmissionFailureEvent -= new TransmissionFailureDelegate(ReceiveTransmissionFailure);
            a_errorLogger.ExceptionOccurredEvent -= new ExceptionOccurredDelegate(ReceiveExceptionOcurred);

            //SystemController.Receiver.DeadConnectionEvent -= Receiver_DeadConnectionEvent;
            //SystemController.Receiver.SystemServiceUnavailableEvent -= Receiver_SystemServiceUnavailableEvent;
            //SystemController.Receiver.SystemServiceAvailableEvent -= Receiver_SystemServiceAvailableEvent;
        }

        if (a_add)
        {
            //TODO: Do we need these?
            SystemController.Sys.SystemStateSwitchEvent += new PTSystem.SystemStateSwitchDelegate(Sys_SystemStateChangedEvent);
            SystemController.Sys.SystemMessageEvent += new PTSystem.SystemMessageDelegate(Sys_SystemMsgEvent);
            SystemController.Sys.RestartRequiredEvent += new PTSystem.RestartRequiredDelegate(Sys_RestartRequiredHandler);
            SystemController.Sys.RestartServiceRequiredEvent += new PTSystem.RestartServiceRequiredDelegate(Sys_RestartServiceRequiredHandler);
            SystemController.Sys.InstanceMessageReceivedEvent += new PTSystem.InstanceMessageReceivedDelegate(Sys_InstanceMessageReceivedHandler);
        }
        else //Remove
        {
            SystemController.Sys.SystemStateSwitchEvent -= new PTSystem.SystemStateSwitchDelegate(Sys_SystemStateChangedEvent);
            SystemController.Sys.SystemMessageEvent -= new PTSystem.SystemMessageDelegate(Sys_SystemMsgEvent);
            SystemController.Sys.RestartRequiredEvent -= new PTSystem.RestartRequiredDelegate(Sys_RestartRequiredHandler);
            SystemController.Sys.RestartServiceRequiredEvent -= new PTSystem.RestartServiceRequiredDelegate(Sys_RestartServiceRequiredHandler);
            SystemController.Sys.InstanceMessageReceivedEvent -= new PTSystem.InstanceMessageReceivedDelegate(Sys_InstanceMessageReceivedHandler);
        }
    }

    private void ReceiveTransmissionFailure(Exception e, PTTransmission t, SystemMessage message)
    {
        if (IgnoreEvents 
            || t is ScenarioBaseT scenarioT && scenarioT.ReplayForUndoRedo 
            || (t.Instigator != SystemController.CurrentUserId && t is not ERPTransmissions.ERPTransmission))
        {
            return;
        }

        m_mainForm.BeginInvoke(new TransmissionFailureDelegate(m_mainForm.NewTransmissionFailure), t, message, e);
    }

    public delegate void ExceptionOccurredListenerDelegate(Exception e, bool a_fatal, bool a_redo);
    private void ReceiveExceptionOcurred(Exception a_e, bool a_fatal, bool a_redo)
    {
        if (IgnoreEvents || a_redo)
        {
            return;
        }

        m_mainForm.BeginInvoke(new ExceptionOccurredListenerDelegate(m_mainForm.NewException), a_e, a_fatal, false);
    }

    private void se_OfflineStatusChangedEvent(PTTransmission t)
    {
        if (IgnoreEvents) { }
        //m_mainForm.BeginInvoke(new OfflineStatusChangedDelegate(m_mainForm.OfflineStatusChanged), new object[] { t });
    }

    private void Sys_SystemStateChangedEvent(SystemStateSwitchT a_t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.BeginInvoke(new SystemStateSwitchDelegate(m_mainForm.SystemStateSwitched), a_t);
    }

    private void Sys_SystemMsgEvent(SystemMessageT a_t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.BeginInvoke(new PopupMessageDelegate(m_mainForm.PopupMessage), a_t.MessageText, true, new BaseId(-1));
    }

    private void Sys_RestartRequiredHandler(ClientUserRestartT a_t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.BeginInvoke(new PTSystem.RestartRequiredDelegate(m_mainForm.RestartRequiredHandler), a_t);
    }

    private void Sys_RestartServiceRequiredHandler(PackageUpdateT a_t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.BeginInvoke(new PTSystem.RestartServiceRequiredDelegate(m_mainForm.RestartServiceRequiredHandler), a_t);
    }

    private void Sys_InstanceMessageReceivedHandler(InstanceMessageT a_t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.BeginInvoke(new PTSystem.InstanceMessageReceivedDelegate(m_mainForm.InstanceMessageReceivedHandler), a_t);
    }

    private void se_PopupMessageEvent(string message, MessageSeverity severity, bool showAllUsers, BaseId showForThisUser) //todo a_add , string msgCodeForHyperlink and pass in below
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_mainForm.BeginInvoke(new PopupMessageDelegate(m_mainForm.PopupMessage), message, showAllUsers, showForThisUser);
    }

    private void Receiver_DeadConnectionEvent(Exception e)
    {
        if (IgnoreEvents)
        {
            return;
        }

        try
        {
            //TODO: Sessions
            //m_mainForm.BeginInvoke(new ClientSession.DeadConnectionDelegate(m_mainForm.ConnectionChecker.DeadConnectionHandler), e);
        }
        catch (Exception)
        {
            //Sometimes get this if lost connection and shutting down.  Can't seem to disable this from firing so suppress since it's harmless.
        }
    }

    private void Receiver_SystemServiceUnavailableEvent(Exception a_e)
    {
        if (IgnoreEvents)
        {
            return;
        }

        try
        {
            //TODO: Sessions
            //m_mainForm.BeginInvoke(new Action<Exception>(m_mainForm.ConnectionChecker.SystemServiceUnavailable), a_e);
        }
        catch (Exception)
        {
            //Sometimes get this if lost connection and shutting down.  Can't seem to disable this from firing so suppress since it's harmless.
        }
    }

    private void Receiver_SystemServiceAvailableEvent()
    {
        if (IgnoreEvents)
        {
            return;
        }

        try
        {
            //TODO: Sessions
            //m_mainForm.BeginInvoke(new Action(m_mainForm.ConnectionChecker.SystemServiceAvailable));
        }
        catch (Exception)
        {
            //Sometimes get this if lost connection and shutting down.  Can't seem to disable this from firing so suppress since it's harmless.
        }
    }
    #endregion System Events

    #region Ignore events code.
    private long ignoreEventsCount;

    private bool IgnoreEvents
    {
        get
        {
            if (ignoreEventsCount < 0)
            {
                throw new PTException("Ignore events variable out of sync.");
            }

            return ignoreEventsCount > 0 || m_mainForm.ClosingSoIgnoreAllEvents;
        }
    }

    public class IgnoreEventsDisposer : IDisposable
    {
        public IgnoreEventsDisposer(MainFormListener listener)
        {
            this.listener = listener;
            ++listener.ignoreEventsCount;
        }

        private readonly MainFormListener listener;

        #region IDisposable Members
        public void Dispose()
        {
            --listener.ignoreEventsCount;
        }
        #endregion
    }

    public IgnoreEventsDisposer GetIgnoreEventsDisposer()
    {
        return new IgnoreEventsDisposer(this);
    }
    #endregion
}