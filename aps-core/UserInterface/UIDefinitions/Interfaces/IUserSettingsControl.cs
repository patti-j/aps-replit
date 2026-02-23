using PT.Common.Localization;

namespace PT.UIDefinitions.Interfaces;

public interface IUserSettingsControl : ILocalizable
{
    void SaveUserSettings();
    void LoadUserSettings();
}