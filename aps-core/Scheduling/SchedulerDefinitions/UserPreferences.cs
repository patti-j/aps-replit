namespace PT.SchedulerDefinitions;

public class UserPreferences : IPTSerializable, ICloneable
{
    #region IPTSerializable
    public UserPreferences(IReader a_reader)
    {
        #region 656
        if (a_reader.VersionNumber >= 656)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_skinName);
        }
        #endregion

        #region < 12000
        if (a_reader.VersionNumber < 12000)
        {
            //Hard coded for now until skin elements are compatible with buttons(high impact)
            m_skinName = "PlanetTogetherThemes";
        }
        #endregion
    }

    public void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_skinName);
    }

    public int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 824;
    #endregion

    public UserPreferences()
    {
        //Default
        HideInactiveAlerts = true;
        ConfirmDelete = true;
    }

    #region Bools 1
    private BoolVector32 m_bools;
    private const short c_confirmDelete = 0;
    private const short c_confirmDeleteSet = 1;
    private const short c_hideInactiveAlertsIdx = 2;
    private const short c_hideInactiveAlertsSetIdx = 3;
    private const short c_alwaysShowMoveCursorIdx = 4;
    private const short c_alwaysShowMoveCursorSetIdx = 5;
    private const short c_showOtherUserActionMessagesIdx = 6;
    private const short c_showOtherUserActionMessagesSetIdx = 7;
    private const short c_useScenarioOptimizeSettingsIdx = 8;
    private const short c_useScenarioOptimizeSettingsSetIdx = 9;
    private const short c_useScenarioCompressSettingsIdx = 10;
    private const short c_useScenarioCompressSettingsSetIdx = 11;
    private const short c_prependSetupOnMoveIdx = 12;
    private const short c_prependSetupOnMoveSetIdx = 13;
    private const short c_skinNameSetIdx = 14;

    public bool ConfirmDelete
    {
        get => m_bools[c_confirmDelete];
        set
        {
            m_bools[c_confirmDelete] = value;
            m_bools[c_confirmDeleteSet] = true;
        }
    }

    public bool ConfirmDeleteSet => m_bools[c_confirmDeleteSet];
    #endregion

    private string m_skinName;

    public string SkinName
    {
        get => m_skinName;
        set
        {
            m_skinName = value;
            m_bools[c_skinNameSetIdx] = true;
        }
    }

    public bool SkinNameSet => m_bools[c_skinNameSetIdx];

    public bool HideInactiveAlerts
    {
        get => m_bools[c_hideInactiveAlertsIdx];
        set
        {
            m_bools[c_hideInactiveAlertsIdx] = value;
            m_bools[c_hideInactiveAlertsSetIdx] = true;
        }
    }

    public bool HideInactiveAlertsSet => m_bools[c_hideInactiveAlertsSetIdx];

    /// <summary>
    /// Whether to show the multi move curor even when dragging only a single block
    /// </summary>
    public bool AlwaysShowMoveCursor
    {
        get => m_bools[c_alwaysShowMoveCursorIdx];
        set
        {
            m_bools[c_alwaysShowMoveCursorIdx] = value;
            m_bools[c_alwaysShowMoveCursorIdx] = true;
        }
    }

    public bool AlwaysShowMoveCursorSet => m_bools[c_alwaysShowMoveCursorSetIdx];

    public bool ShowOtherUserActionMessages
    {
        get => m_bools[c_showOtherUserActionMessagesIdx];
        set
        {
            m_bools[c_showOtherUserActionMessagesIdx] = value;
            m_bools[c_showOtherUserActionMessagesSetIdx] = true;
        }
    }

    public bool ShowOtherUserActionMessagesSet => m_bools[c_showOtherUserActionMessagesSetIdx];

    /// <summary>
    /// Whether the User is using the shared Scenario Optimize Settings as opposed to his own settings for performing Optimizations.
    /// </summary>
    public bool UseScenarioOptimizeSettings
    {
        get => m_bools[c_useScenarioOptimizeSettingsIdx];
        set
        {
            m_bools[c_useScenarioOptimizeSettingsIdx] = value;
            m_bools[c_useScenarioOptimizeSettingsSetIdx] = true;
        }
    }

    public bool UseScenarioOptimizeSettingsSet => m_bools[c_useScenarioOptimizeSettingsSetIdx];

    /// <summary>
    /// Whether the User is using the shared Scenario Compress Settings or his own settings for performing Optimizations.
    /// </summary>
    public bool UseScenarioCompressSettings
    {
        get => m_bools[c_useScenarioCompressSettingsIdx];
        set
        {
            m_bools[c_useScenarioCompressSettingsIdx] = value;
            m_bools[c_useScenarioCompressSettingsSetIdx] = true;
        }
    }

    public bool UseScenarioCompressSettingsSet => m_bools[c_useScenarioCompressSettingsSetIdx];

    /// <summary>
    /// Whether to Attempt to prevent a block from ending later by prepending any new
    /// setup added to it when the block to its left is moved.
    /// </summary>
    public bool PrependSetupOnMove
    {
        get => m_bools[c_prependSetupOnMoveIdx];
        set
        {
            m_bools[c_prependSetupOnMoveIdx] = value;
            m_bools[c_prependSetupOnMoveSetIdx] = true;
        }
    }

    public bool PrependSetupOnMoveSet => m_bools[c_prependSetupOnMoveSetIdx];

    public void Update(UserPreferences a_newPreferences)
    {
        if (a_newPreferences.ConfirmDeleteSet)
        {
            ConfirmDelete = a_newPreferences.ConfirmDelete;
        }

        if (a_newPreferences.HideInactiveAlertsSet)
        {
            HideInactiveAlerts = a_newPreferences.HideInactiveAlerts;
        }

        if (a_newPreferences.AlwaysShowMoveCursorSet)
        {
            AlwaysShowMoveCursor = a_newPreferences.AlwaysShowMoveCursor;
        }

        if (a_newPreferences.ShowOtherUserActionMessagesSet)
        {
            ShowOtherUserActionMessages = a_newPreferences.ShowOtherUserActionMessages;
        }

        if (a_newPreferences.UseScenarioOptimizeSettingsSet)
        {
            UseScenarioOptimizeSettings = a_newPreferences.UseScenarioOptimizeSettings;
        }

        if (a_newPreferences.UseScenarioCompressSettingsSet)
        {
            UseScenarioCompressSettings = a_newPreferences.UseScenarioCompressSettings;
        }

        if (a_newPreferences.PrependSetupOnMoveSet)
        {
            PrependSetupOnMove = a_newPreferences.PrependSetupOnMove;
        }

        if (a_newPreferences.SkinNameSet)
        {
            SkinName = a_newPreferences.SkinName;
        }
    }

    public object Clone()
    {
        UserPreferences clone = new ();
        clone.m_bools = new BoolVector32(m_bools);
        clone.SkinName = SkinName;
        return clone;
    }
}