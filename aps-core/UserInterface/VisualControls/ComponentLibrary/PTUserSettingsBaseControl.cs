using PT.Common.Debugging;
using PT.UIDefinitions.Interfaces;

namespace PT.ComponentLibrary;

public partial class PTUserSettingsBaseControl : PTBaseControl, IUserSettingsControl
{
    private IUserSettingsManager m_settingsManager;

    protected IUserSettingsManager p_settingsManager => m_settingsManager;

    protected PTUserSettingsBaseControl()
    {
        InitializeComponent();
    }

    protected PTUserSettingsBaseControl(IUserSettingsManager a_manager)
    {
        m_settingsManager = a_manager;
        InitializeComponent();
    }

    /// <summary>
    /// Alternate method for setting user manager if the reference is not provided during construction
    /// </summary>
    protected void InitializeUserManager(IUserSettingsManager a_userSettingsManager)
    {
        m_settingsManager = a_userSettingsManager;
    }

    public virtual void SaveUserSettings()
    {
        DebugException.ThrowInDebug("Control did not override SaveUserSettings()");
    }

    public virtual void LoadUserSettings()
    {
        DebugException.ThrowInDebug("Control did not override LoadserSettings()");
    }
}