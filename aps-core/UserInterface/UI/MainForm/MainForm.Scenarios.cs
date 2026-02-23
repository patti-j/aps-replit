using PT.APSCommon;
using PT.APSCommon.Windows;
using PT.Common.Http;
using PT.GanttDotNet;
using PT.PackageDefinitions;
using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.Controls;
using PT.Scheduler;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.Transmissions;
using PT.UI.Managers;
using PT.UI.Scenarios;
using PT.UI.ScenarioViewer;
using PT.UIDefinitions;
using System.Windows.Forms;

namespace PT.UI;

partial class MainForm
{
    public void UpdateViewerPermissions(UserPermissionSet a_permissions, PlantPermissionSet a_plantPermissions, ScenarioDetail a_sd)
    {
        m_scenarioController.UpdateUserPermissions(a_permissions, a_plantPermissions, a_sd);
    }

    private ScenarioViewerContainer m_scenarioViewerContainer;
    private IScenarioInfo m_scenarioInfo;
    private IScenarioController m_scenarioController;
    private ImpactManager m_impactManager;
    private static readonly string s_desync = "Please wait. Your scenario has desynced, and it is being reloaded.".Localize();
    private static readonly string s_promoteWhatIfScenario = "Please wait. A new scenario is being promoted to production scenario".Localize();
    // ReSharper disable once CollectionNeverQueried.Local
    //This keeps the elements in memory in case they don't keep any references.
    private List<IScenarioBackgroundTaskElement> m_backgroundTaskElements;
    private bool m_productionOverlayShown;

    private void LoadScenarios()
    {
        //Add Scenarios
        m_impactManager = new ImpactManager(m_packageManager);

        //TODO: License for Multiple Scenarios
        if (true)
        {
            m_scenarioController = new MainScenarioControl(this, m_packageManager, m_impactManager);
        }
        else
        {
            m_scenarioController = new SingleScenarioController(this, m_packageManager, m_impactManager);
        }

        m_scenarioInfo = m_scenarioController.GetScenarioInfo();
        // All IScenarioInfo are currently IMultiScenarioInfo too.
        // This structure could probably be changed in the future
        ((IMultiScenarioInfo)m_scenarioInfo).ScenarioReloaded += HideOverlayOnceDesyncedScenarioIsCreated;

        SplashManager.UpdateSplashDescription("Loading Scenario Controls...".Localize());
        m_scenarioViewerContainer = new ScenarioViewerContainer(this, m_scenarioInfo, m_packageManager);
        m_scenarioViewerContainer.Dock = DockStyle.Fill;
        m_scenariosPanel.Controls.Add(m_scenarioViewerContainer);

        SplashManager.UpdateSplashDescription("Loading Scenario Controls...".Localize());

        m_scenarioController.NoAccessibleScenarios += ShutdownProgram;
        ((MainScenarioControl)m_scenarioController).ScenarioPromptDelete += MainForm_ScenarioPromptDelete;
        m_scenarioController.LoadScenarioData();

        //Load background task elements. These will run on their own in the background.
        //Store in a list to keep them in memory in case they don't store a reference to any passed objects
        m_backgroundTaskElements = new List<IScenarioBackgroundTaskElement>();
        List<IScenarioBackgroundTaskModule> modules = m_packageManager.GetScenarioBackgroundTaskModules();
        foreach (IScenarioBackgroundTaskModule taskModule in modules)
        {
            List<IScenarioBackgroundTaskElement> elements = taskModule.GetScenarioBackgroundTaskElements(m_scenarioInfo);
            m_backgroundTaskElements.AddRange(elements);
        }

        m_scenarioInfo.ScenarioActivated += ActivateScenario;
        m_scenarioInfo.ScenarioDesynced += st_ScenarioDesynchronized;
        m_scenarioInfo.BeginScenarioConversion += ConvertingScenario;
        m_scenarioInfo.ScenarioConversionComplete += ConvertToProductionComplete;

        GanttUtilities.InitializeMainForm(this, WorkspaceInfo);
    }

    private void MainForm_ScenarioPromptDelete(BaseId a_sId)
    {
        if (m_scenarioInfo.ActiveClientScenarioData.ScenarioId == a_sId)
        {
           //// This dialog needs to be created on the main thread to have access to a DevExpress skin
           BaseId selectedScenario = BaseId.NULL_ID;
           Invoke(new Action(() =>
            {
                List<GenericLabeledEditor.ComboBoxItem> comboBoxItemsMoExternalIds = new List<GenericLabeledEditor.ComboBoxItem>();
                List<UnloadedScenarioData> allUnloadedScenarioData = GetAllUnloadedScenarioData().ToList();
                foreach (UnloadedScenarioData scenarioData in allUnloadedScenarioData)
                {
                    GenericLabeledEditor.ComboBoxItem comboBoxItem = new(scenarioData.ScenarioName, scenarioData.ScenarioId);
                    comboBoxItemsMoExternalIds.Add(comboBoxItem);
                }

                if (allUnloadedScenarioData.Count == 0)
                {
                    return;
                }

                GenericLabeledEditor scenarioCollection = new GenericLabeledEditor("Your current scenario was deleted, please select a scenario to activate".Localize(), comboBoxItemsMoExternalIds, false, GenericLabeledEditor.EControlType.ComboBox);
                using (MultiValueDialog dlg = new("Select a scenario to activate", [scenarioCollection]))
                {
                    DialogResult dialogResult = dlg.ShowDialog();
                    if (dialogResult == DialogResult.OK)
                    {
                        selectedScenario = (BaseId)dlg[0];
                    }
                   
                }
            }));

            if (selectedScenario != BaseId.NULL_ID)
            {
                ScenarioLoadT loadT = new ScenarioLoadT(selectedScenario, SystemController.CurrentUserId);
                ClientSession.SendClientAction(loadT);
            }
            else
            {
                ((MainScenarioControl)m_scenarioController).FireNoAccessibleScenarios();
            }
        }
    }

    private IEnumerable<UnloadedScenarioData> GetAllUnloadedScenarioData()
    {
        ByteResponse response = ClientSession.MakeGetRequest<ByteResponse>("GetAllUnloadedScenarioData", "api/SystemService");

        if (response != null && response.Content.Length > 0)
        {
            using (BinaryMemoryReader reader = new(response.Content))
            {
                reader.Read(out int count);

                for (int i = 0; i < count; i++)
                {
                    UnloadedScenarioData data = new(reader);
                    yield return data;
                }
            }
        }
    }
    private void ConvertToProductionComplete(BaseId a_scenarioId, BaseId a_instigator)
    {
        //only using the bool flag because there is an issue to be worked on
        //(since we currently don't allow clients to be signed in without a production scenario) which
        //causes a_scenarioId to be a mismatch even when the active scenario is the production scenario (false production scenario)
        if (a_scenarioId == m_scenarioInfo.ScenarioId || m_productionOverlayShown)
        {
            m_scenarioViewerContainer.HideScenarioOverlay();
            m_productionOverlayShown = false;
        }
    }

    private void ConvertingScenario(BaseId a_scenarioId, BaseId a_promotedWhatIfScenarioId)
    {
        if (a_scenarioId == m_scenarioInfo.ScenarioId)
        {
            m_scenarioViewerContainer.ShowScenarioOverlay(s_promoteWhatIfScenario);
            m_productionOverlayShown = true;
        }
    }

    private void ShutdownProgram()
    {
        BeginInvoke(() =>
        {
            // This dialog needs to be created on the main thread to have access to a DevExpress skin
            UIClosingDialogEvent closeDialog = new ("You are being signed out due to no scenarios being accessible to you".Localize(), 5, false, false);
            FireNavigationEvent(closeDialog);
        });
        m_scenarioController.NoAccessibleScenarios -= ShutdownProgram;
    }

    private void ActivateScenario(Scenario a_arg1, ScenarioDetail a_arg2, ScenarioEvents a_arg3)
    {
        //The UI is ready to handle processing actions
        //TODO: This could probably be moved to a better spot so it's not based on an event.
        m_scenarioInfo.ScenarioActivated -= ActivateScenario;
        m_clientSession.StartReceiving();
    }

    private readonly object m_scenarioDesyncLock = new();
    private BaseId m_desyncedScenarioBeingReloaded;
    //TODO lite client: We need to handle multiple scenarios being desynced at the same time.
    // The structure that exists around the lock will only handle one scenario right now.
    private void st_ScenarioDesynchronized(Guid a_transmissionId, string a_description, BaseId a_scenarioId)
    {
        PTMessage desyncMessage = new PTMessage(string.Format("The client and server schedules are mismatched for Scenario with Id: '{0}'. The program will reload the Scenario to resolve the issue.".Localize(), a_scenarioId),
            "Client Schedule Mismatched".Localize())
        {
            Classification = PTMessage.EMessageClassification.Information, 
            HelpUrl = string.Empty,
            Action = PTMessage.EMessageActions.Ok
        };

        // 
        if (m_messageProvider.ShowMessageBox(desyncMessage, true, false, this) == DialogResult.OK)
        {
            lock (m_scenarioDesyncLock)
            {
                m_desyncedScenarioBeingReloaded = a_scenarioId;
            }
            if (a_scenarioId == m_scenarioInfo.ScenarioId)
            {
                m_scenarioViewerContainer.ShowScenarioOverlay(s_desync);
            }
            ScenarioReloadT reloadT = new(a_scenarioId, SystemController.CurrentUserId, a_transmissionId);
            ClientSession.SendClientAction(reloadT);
        }
        // This code below should be re-used if the scenario repeatedly desyncs, in which case, we'd just restart the program.
        // This is just a stopgap for now to account for the cancel status, but the goal is to have the 
        // message box just close automatically without waiting for user input. 
        else 
        {
            //Start a new instance using the previous login settings and then close this instance.
            if (LastLoginSetttings != null)
            {
                m_restartOnClose = true;
            }
            else
            {
                m_restartOnClose = false;
            }

            m_scenarioInfo.ScenarioDesynced -= st_ScenarioDesynchronized;

            #if DEBUG
            Environment.Exit(10);
            #else
            CloseForm();
            #endif
        }

        // This code below causes a circular reference since ClosingDialog is in CorePackage. 
        // Need to refactor this or make a modified copy of ClosingDialog in the UI project to deal with this issue.
        // Maybe change the MessageBox utility to have an option to close the message box after a period of time. 
        //BeginInvoke(() =>
        //{
        //    using (ClosingDialog closingDialog = new (this, "The active Scenario on the client is mismatched. It will now be reloaded from the server.".Localize(), false, false, TimeSpan.FromSeconds(5)))
        //    {
        //        closingDialog.SetTitle("Client Scenario Mismatch");
        //        closingDialog.Owner = this;
        //        closingDialog.ShowDialog();
        //        lock (m_scenarioDesyncLock)
        //        {
        //            m_desyncedScenarioBeingReloaded = a_scenarioId;
        //        }

        //        m_scenarioViewerContainer.ShowScenarioOverlay();
        //        ScenarioReloadT reloadT = new (a_scenarioId, SystemController.CurrentUserId, a_transmissionId);
        //        ClientSession.SendClientAction(reloadT);
        //    }
        //});


    }

    private void HideOverlayOnceDesyncedScenarioIsCreated(BaseId a_scenarioCreatedId, ScenarioBaseT a_transmission)
    {
        if (a_transmission is ScenarioReloadT)
        {
            //    ScenarioReloadT reloadT = new(a_scenarioId, SystemController.CurrentUserId, a_transmissionId);
            //    ClientSession.SendClientAction(reloadT);
            lock (m_scenarioDesyncLock)
            {
                if (a_scenarioCreatedId == m_desyncedScenarioBeingReloaded)
                {
                    m_desyncedScenarioBeingReloaded = BaseId.NULL_ID;
                    m_scenarioViewerContainer.HideScenarioOverlay();
                }
            }
        }
    }
}