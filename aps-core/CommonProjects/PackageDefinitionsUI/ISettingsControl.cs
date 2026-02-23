using System.Windows.Forms;

using DevExpress.Utils;

using PT.PackageDefinitions;
using PT.Scheduler;
using PT.Transmissions;

namespace PT.PackageDefinitionsUI;

/// <summary>
/// A user control that will be displayed in the client.
/// This can have any functionality but does not interact with other controls automatically except through stored settings
/// </summary>
public interface ISettingsControl : IPackageElement
{
    /// <summary>
    /// Signal that the control has changes that could be saved or reverted
    /// </summary>
    event Action<ISettingsControl> SettingsModified;

    /// <summary>
    /// Reload the control after an optimize or compress transmission is processed
    /// </summary>
    /// <param name="a_t"></param>
    void ReloadControl(PTTransmission a_t);

    /// <summary>
    /// User has indicated that the settings should be saved
    /// </summary>
    PTTransmission Save();

    /// <summary>
    /// Used to validate whether or not Save needs to be called
    /// </summary>
    /// <returns></returns>
    bool Validate()
    {
        return true;
    }

    /// <summary>
    /// User has indicated that the settings should be reverted
    /// </summary>
    void Revert();

    /// <summary>
    /// User has indicated that the settings should be reset to its default values
    /// </summary>
    void Reset();

    /// <summary>
    /// Reference to the control
    /// </summary>
    Control ControlInstance { get; }

    /// <summary>
    /// Help string to place on help button tool tip
    /// </summary>
    SuperToolTip HelpToolTip { get; }

    /// <summary>
    /// Text will be localized
    /// </summary>
    string DisplayText { get; }

    /// <summary>
    /// This is the group code the UI will use to determine where to place this control
    /// </summary>
    string SettingsGroup { get; }

    /// <summary>
    /// This is the subgroup under the SettingsGroup group that the UI will use to determine where to place this control
    /// </summary>
    string SettingsGroupCategory { get; }
}

public interface IInitializeWithScenarioData
{
    /// <summary>
    /// Initialize the control
    /// </summary>
    void Initialize(ScenarioManager a_sm, ScenarioDetail a_sd);
}

/// <summary>
/// Inherited by Settings Controls that don't require locking Scenario Detail
/// </summary>
public interface IUserOrSystemSettingControl : ISettingsControl
{
    /// <summary>
    /// Initialize the control
    /// </summary>
    void Initialize();
}

/// <summary>
/// A user control that will be displayed in the client.
/// This can have any functionality but does not interact with other controls automatically except through stored settings
/// </summary>
public interface ISequenceFactorSettingsControl : ISettingsControl, IInitializeWithScenarioData
{
    string OptimizeFactorElementPackageId { get; }
    SettingData GetCurrentSettings();
    void LoadSettings(SettingData a_currentSettings);
}