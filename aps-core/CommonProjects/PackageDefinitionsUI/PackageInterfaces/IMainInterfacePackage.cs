using PT.PackageDefinitions;
using PT.UIDefinitions;

namespace PT.PackageDefinitionsUI.PackageInterfaces;

public interface IMainInterfacePackage : IUIPackage
{
    List<IMainInterfaceModule> GetMainInterfaceModules();
    List<IMainWorkspaceManagerModule> GetWorkspaceManagerModules();
}

public interface IMainInterfaceModule
{
    List<ISystemClosingElement> GetSystemClosingElements();
    List<INavigationElement> GetNavigationElements();
}

public interface IMainWorkspaceManagerModule
{
    IWorkspaceController GetWorkspaceController();
}

public enum EExitResult
{
    ExitImmediately,
    CancelExit,
    ContinueExit,
    FirstPrompt,
    ContinueNoPrompt
}

public interface ISystemClosingElement : IPackageElement
{
    int Priority { get; }

    EExitResult ShowExitPrompt(EExitResult a_exitResult);

    /*
     * We had to split off the actual program exit and showing the exit prompt when we did a re-design due to some
     * confusing interactions amongst the different closing dialogs, specifically, the ClosingDialog only being
     * shown when it's the first prompt. This was a result of trying not to show more than one closing dialog,
     * but the structure and logic gets a bit confusing.
     */
    void Exiting();
}

public interface INavigationElement : IPackageElement
{
    void InitializeNavigation(IMainForm a_mainForm, IScenarioInfo a_scenarioInfo, IUINavigationEventManager a_eventManager);
}