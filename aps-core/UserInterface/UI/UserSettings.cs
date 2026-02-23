using System.Configuration;

namespace PT.UI;

public class LoginSettings : ApplicationSettingsBase
{
    [UserScopedSetting]
    [DefaultSettingValue("localhost")]
    public string ServerComputerOrIp
    {
        get => (string)this["ServerComputerOrIp"];
        set => this["ServerComputerOrIp"] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string InstanceName
    {
        get => (string)this["InstanceName"];
        set => this["InstanceName"] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("7990")]
    public int ServerManagerPort
    {
        get => (int)this["ServerManagerPort"];
        set => this["ServerManagerPort"] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("admin")]
    public string UserName
    {
        get => (string)this["UserName"];
        set => this["UserName"] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string Password
    {
        get => (string)this["Password"];
        set => this["Password"] = value;
    }

    [ApplicationScopedSetting]
    [DefaultSettingValue("true")]
    public bool AllowPasswordSaving
    {
        get => (bool)this["AllowPasswordSaving"];
        set => this["AllowPasswordSaving"] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("false")]
    public bool RememberPassword
    {
        get => (bool)this["RememberPassword"];
        set => this["RememberPassword"] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("false")]
    public bool UseActiveDirectory
    {
        get => (bool)this["UseActiveDirectory"];
        set => this["UseActiveDirectory"] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("true")]
    public bool ActiveDirectoryCurrentCredentials
    {
        get => (bool)this["ActiveDirectoryCurrentCredentials"];
        set => this["ActiveDirectoryCurrentCredentials"] = value;
    }

    private const string WIN_LOC_X_NAME = "WindowLocationX";
    private const string WIN_LOC_Y_NAME = "WindowLocationY";
    private const string WIN_LOC_DEFAULT = "0";

    [UserScopedSetting]
    [DefaultSettingValue(WIN_LOC_DEFAULT)]
    public int WindowLocationX
    {
        get => (int)this[WIN_LOC_X_NAME];
        set => this[WIN_LOC_X_NAME] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue(WIN_LOC_DEFAULT)]
    public int WindowLocationY
    {
        get => (int)this[WIN_LOC_Y_NAME];
        set => this[WIN_LOC_Y_NAME] = value;
    }
}