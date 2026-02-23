using DevExpress.LookAndFeel;
using DevExpress.Skins;

using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.UserSettings;
using PT.UIDefinitions;

namespace PT.ScenarioControls.PackageHelpers;

public class EnterReadonlyHelper
{
    #region Declarations
    private readonly IMainForm m_mainForm;
    private readonly IScenarioInfo m_scenarioInfo;
    private readonly IPackageManagerUI m_pm;

    private PTCorePreferences m_preferences = new ();

    private static readonly string s_readOnlyPaletteString = "Readonly";
    #endregion

    /// <summary>
    /// Handles showing the License info dialog and switching to and from the Read-only theme
    /// </summary>
    /// <param name="a_mainForm"></param>
    /// <param name="a_scenarioInfo"></param>
    /// <param name="a_pm"></param>
    public EnterReadonlyHelper(IMainForm a_mainForm, IScenarioInfo a_scenarioInfo, IPackageManagerUI a_pm)
    {
        m_mainForm = a_mainForm;
        m_scenarioInfo = a_scenarioInfo;
        m_pm = a_pm;

        LoadPreferences();
    }

    private void LoadPreferences()
    {
        m_preferences = m_mainForm.UserPreferenceInfo.LoadSetting(m_preferences);
    }

    /// <summary>
    /// Handle Read-only state change
    /// </summary>
    /// <param name="a_readOnly"></param>
    public void ProcessReadonlyStateChange(bool a_readOnly)
    {
        if (a_readOnly)
        {
            m_scenarioInfo.InvokeControl.BeginInvoke(new Action(() =>
            {
                ChangeSkin(s_readOnlyPaletteString);

                m_mainForm.FireNavigationEvent(new UINavigationEvent("OpenLicenseDialog"));
            }));
        }
        else
        {
            string skinToLoad = string.IsNullOrEmpty(m_preferences.LastThemeLoaded) ? m_mainForm.CurrentBrand.AllowedThemes[0] : m_preferences.LastThemeLoaded;
            m_scenarioInfo.InvokeControl.BeginInvoke(new Action(() =>
            {
                if (UserLookAndFeel.Default.ActiveSvgPaletteName == s_readOnlyPaletteString)
                {
                    ChangeSkin(skinToLoad);
                }
            }));
        }
    }

    /// <summary>
    /// Set appropriate theme
    /// </summary>
    /// <param name="a_paletteName"></param>
    private void ChangeSkin(string a_paletteName)
    {
        //MultiLevelHourglass.TurnOn(10); This was never turned off. I'm guessing that we don't really know when the theme has fully switched and loaded in the UI
        using (new MultiLevelHourglass(TimeSpan.Zero))
        {
            Skin skin = CommonSkins.GetSkin(UserLookAndFeel.Default.ActiveLookAndFeel);

            UserLookAndFeel.Default.SetSkinStyle(skin.Name, a_paletteName);
            LookAndFeelHelper.ForceDefaultLookAndFeelChanged();

            m_mainForm.CurrentBrand.ActiveTheme.FireThemeChangedEvent();
        }
    }
}