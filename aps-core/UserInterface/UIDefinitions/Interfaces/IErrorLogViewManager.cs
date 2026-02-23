namespace PT.UIDefinitions.Interfaces;

public interface IErrorLogViewManager
{
    /// <summary>
    /// Opens the log view, optionally selecting a specific log by Index
    /// userTabIndex = 0
    /// interfaceTabIndex = 1
    /// systemTabIndex = 2
    /// externalTabIndex = 3
    /// fatalTabIndex = 4
    /// miscTabIndex = 5
    /// schedulingWarningsTabIndex = 6
    /// notificationsTabIndex = 7
    /// interfaceFrmServerTabIndex = 8
    /// </summary>
    void ShowErrorLog(short a_focusTabIdx = 0);
}