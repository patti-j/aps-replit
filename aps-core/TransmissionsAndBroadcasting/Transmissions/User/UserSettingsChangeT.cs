using PT.APSCommon;
using PT.APSCommon.Serialization;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for changing various User Settings.
/// The Transmission keeps track of which Properties have been set so just those can be stored.
/// </summary>
public class UserSettingsChangeT : UserIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 436;

    #region IPTSerializable Members
    public UserSettingsChangeT(IReader reader)
        : base(reader)
    {
        int obsoleteInt;
        bool obsoleteBool;
        TimeSpan obsoleteTimeSpan;

        #region 12000
        if (reader.VersionNumber >= 12000)
        {
            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            reader.Read(out m_currentWorkspaceName);
            m_bools = new BoolVector32(reader);
            m_userWorkspaces = new Dictionary<string, byte[]>();
            int dictCount;
            reader.Read(out dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                reader.Read(out key);
                reader.Read(out value);
                m_userWorkspaces.Add(key, value);
            }

            reader.Read(out m_skinName);
            reader.Read(out m_userPreferences);
        }
        #endregion 12000

        #region 674
        else if (reader.VersionNumber >= 674)
        {
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            reader.Read(out m_currentWorkspaceName);
            m_bools = new BoolVector32(reader);
            m_userWorkspaces = new Dictionary<string, byte[]>();
            int dictCount;
            reader.Read(out dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                reader.Read(out key);
                reader.Read(out value);
                m_userWorkspaces.Add(key, value);
            }

            new FormLayout(reader);
            reader.Read(out m_skinName);
        }
        #endregion 671

        #region 671
        else if (reader.VersionNumber >= 671)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            reader.Read(out m_currentWorkspaceName);
            m_bools = new BoolVector32(reader);
            m_userWorkspaces = new Dictionary<string, byte[]>();
            int dictCount;
            reader.Read(out dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                reader.Read(out key);
                reader.Read(out value);
                m_userWorkspaces.Add(key, value);
            }

            new FormLayout(reader);
            reader.Read(out m_skinName);
        }
        #endregion 671

        #region 656
        else if (reader.VersionNumber >= 656)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            reader.Read(out m_currentWorkspaceName);
            m_bools = new BoolVector32(reader);
            m_userWorkspaces = new Dictionary<string, byte[]>();
            int dictCount;
            reader.Read(out dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                reader.Read(out key);
                reader.Read(out value);
                m_userWorkspaces.Add(key, value);
            }

            new FormLayout(reader);
            reader.Read(out m_skinName);
        }
        #endregion 656

        #region 647
        else if (reader.VersionNumber >= 647)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            reader.Read(out m_currentWorkspaceName);
            m_bools = new BoolVector32(reader);
            m_userWorkspaces = new Dictionary<string, byte[]>();
            int dictCount;
            reader.Read(out dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                reader.Read(out key);
                reader.Read(out value);
                m_userWorkspaces.Add(key, value);
            }

            new FormLayout(reader);
        }
        #endregion 647

        #region 635
        else if (reader.VersionNumber >= 635)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            ;
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            reader.Read(out m_currentWorkspaceName);
            m_bools = new BoolVector32(reader);
            m_userWorkspaces = new Dictionary<string, byte[]>();
            int dictCount;
            reader.Read(out dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                reader.Read(out key);
                reader.Read(out value);
                m_userWorkspaces.Add(key, value);
            }

            new FormLayout(reader);
        }
        #endregion

        #region 633
        else if (reader.VersionNumber >= 633)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            ;
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            reader.Read(out m_currentWorkspaceName);
            m_bools = new BoolVector32(reader);
            m_userWorkspaces = new Dictionary<string, byte[]>();
            int dictCount;
            reader.Read(out dictCount);
            for (int i = 0; i < dictCount; i++)
            {
                string key;
                byte[] value;
                reader.Read(out key);
                reader.Read(out value);
                m_userWorkspaces.Add(key, value);
            }
        }
        #endregion

        #region 626
        else if (reader.VersionNumber >= 626)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            ;
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            reader.Read(out m_currentWorkspaceName);
            m_bools = new BoolVector32(reader);
            new GridLayoutList(reader);
        }
        #endregion

        #region 620
        else if (reader.VersionNumber >= 620)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            ;
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            reader.Read(out m_currentWorkspaceName);
            m_bools = new BoolVector32(reader);
        }
        #endregion

        #region 607
        else if (reader.VersionNumber >= 607)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            ;
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            m_bools = new BoolVector32(reader);
        }
        #endregion

        #region 500
        else if (reader.VersionNumber >= 500)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            ReadUnusedLayoutProperties(reader);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            ;
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            m_bools = new BoolVector32(reader);
        }
        #endregion

        #region 498
        else if (reader.VersionNumber >= 498)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            ReadUnusedLayoutProperties(reader);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            GridLayoutList unusedList = new (reader);
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            m_bools = new BoolVector32(reader);
            string unusedActivityGridString;
            reader.Read(out unusedActivityGridString);
        }
        #endregion

        #region 212
        else if (reader.VersionNumber >= 212)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            ReadUnusedLayoutProperties(reader);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            GridLayoutList unusedList = new (reader);
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
            m_bools = new BoolVector32(reader);
        }
        #endregion

        #region version 175
        else if (reader.VersionNumber >= 175)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            ReadUnusedLayoutProperties(reader);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            GridLayoutList unusedList = new (reader);
            reader.Read(out m_clockAutoFinish);
            reader.Read(out m_clockAutoFinishSet);
            reader.Read(out m_clockAutoProgress);
            reader.Read(out m_clockAutoProgressSet);
        }
        #endregion

        #region Version 147
        else if (reader.VersionNumber >= 147)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            ReadUnusedLayoutProperties(reader);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
            GridLayoutList unusedList = new (reader);
        }
        #endregion

        #region Version 109
        else if (reader.VersionNumber >= 109)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            ReadUnusedLayoutProperties(reader);

            m_kpiPlotVisibleFlags = new BoolVector32(reader);
            reader.Read(out m_dockGanttRowHeight);
        }
        #endregion

        #region 58
        else if (reader.VersionNumber >= 58)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);

            ReadUnusedLayoutProperties(reader);
        }
        #endregion

        #region Version 32
        else if (reader.VersionNumber >= 32)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);
            bool unsedLayoutBool; //new in 32
            reader.Read(out unsedLayoutBool);
            if (unsedLayoutBool)
            {
                new FormLayout(reader);
            }

            reader.Read(out unsedLayoutBool);
        }
        #endregion

        #region Version 31
        else if (reader.VersionNumber >= 31)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out TimeSpan closeDialogWaitDuration); //new in 31
            reader.Read(out bool closeDialogWaitDurationSet);
        }
        #endregion

        #region Version 28
        else if (reader.VersionNumber >= 28)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
            reader.Read(out obsoleteInt);
            reader.Read(out obsoleteBool);
        }
        #endregion

        #region Version 1
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out obsoleteTimeSpan);
            reader.Read(out obsoleteBool);
        }
        #endregion
    }

    private void ReadUnusedLayoutProperties(IReader a_reader)
    {
        bool unsedLayoutBool; //new in 32
        a_reader.Read(out unsedLayoutBool);
        if (unsedLayoutBool)
        {
            new FormLayout(a_reader);
        }

        a_reader.Read(out unsedLayoutBool);

        a_reader.Read(out unsedLayoutBool);
        if (unsedLayoutBool)
        {
            new FormLayout(a_reader);
        }

        a_reader.Read(out unsedLayoutBool);

        a_reader.Read(out unsedLayoutBool);
        if (unsedLayoutBool)
        {
            new FormLayout(a_reader);
        }

        a_reader.Read(out unsedLayoutBool);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_kpiPlotVisibleFlags.Serialize(writer);

        writer.Write(m_clockAutoFinish);
        writer.Write(m_clockAutoFinishSet);
        writer.Write(m_clockAutoProgress);
        writer.Write(m_clockAutoProgressSet);
        writer.Write(m_currentWorkspaceName);
        m_bools.Serialize(writer);
        writer.Write(m_userWorkspaces.Count);
        foreach (KeyValuePair<string, byte[]> keyValuePair in m_userWorkspaces)
        {
            writer.Write(keyValuePair.Key);
            writer.Write(keyValuePair.Value);
        }

        writer.Write(m_skinName);
        writer.Write(m_userPreferences);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public UserSettingsChangeT() { }

    public UserSettingsChangeT(BaseId userId)
        : base(userId)
    {
    }

    private string m_currentWorkspaceName;

    public string CurrentWorkspaceName
    {
        get => m_currentWorkspaceName;
        set
        {
            m_currentWorkspaceName = value;
            m_bools[m_activityGridLayoutsSetIdx] = true;
        }
    }

    public bool CurrentWorkspaceNameSet => m_bools[m_activityGridLayoutsSetIdx];

    #region bools
    private BoolVector32 m_bools;
    private const int KpiFlagsSetIdx = 0;
    private const int unused2 = 1;
    private const int unused1 = 2;
    private const int UserChangedLanguageIdx = 3;
    private const int UserChangedLanguageSetIdx = 4;
    private const int m_activityGridLayoutsSetIdx = 5;
    private const int c_userWorkspacesSetIdx = 6;
    private const int unused3 = 7;
    private const int c_userPreferencesSetIdx = 8;
    #endregion bools

    /// <summary>
    /// This is used to signal that the user changed her language preferences last time. This is used to reset some necessary settings to allow for
    /// for localization to properly take affect.
    /// </summary>
    public bool UserChangedLanguage
    {
        get => m_bools[UserChangedLanguageIdx];
        set
        {
            m_bools[UserChangedLanguageIdx] = value;
            UserChangedLanguageSet = true;
        }
    }

    public bool UserChangedLanguageSet
    {
        get => m_bools[UserChangedLanguageSetIdx];
        private set => m_bools[UserChangedLanguageSetIdx] = value;
    }

    #region KPI Plot Settings
    private BoolVector32 m_kpiPlotVisibleFlags;

    /// <summary>
    /// Indicate which KPI values to plot.
    /// </summary>
    public BoolVector32 KpiPlotVisibleFlags => m_kpiPlotVisibleFlags;

    public void SetKpiFlagsBoolVector(bool[] flags)
    {
        for (int i = 0; i < flags.Length; i++)
        {
            m_kpiPlotVisibleFlags[i] = (bool)flags.GetValue(i);
        }

        m_bools[KpiFlagsSetIdx] = true;
    }

    public bool KpiFlagsSet => m_bools[KpiFlagsSetIdx];
    #endregion

    private readonly int m_dockGanttRowHeight; //obsolete

    #region Clock Advance Options
    private bool m_clockAutoFinish;

    public bool ClockAutoFinish
    {
        get => m_clockAutoFinish;
        set
        {
            m_clockAutoFinish = value;
            m_clockAutoFinishSet = true;
        }
    }

    private bool m_clockAutoFinishSet;

    public bool ClockAutoFinishSet => m_clockAutoFinishSet;

    private bool m_clockAutoProgress;

    public bool ClockAutoProgress
    {
        get => m_clockAutoProgress;
        set
        {
            m_clockAutoProgress = value;
            m_clockAutoProgressSet = true;
        }
    }

    private bool m_clockAutoProgressSet;

    public bool ClockAutoProgressSet => m_clockAutoProgressSet;
    #endregion

    public bool ActivityGridLayoutsSet
    {
        get => m_bools[m_activityGridLayoutsSetIdx];
        private set => m_bools[m_activityGridLayoutsSetIdx] = value;
    }

    public bool UserWorkspacesSet => m_bools[c_userWorkspacesSetIdx];

    private Dictionary<string, byte[]> m_userWorkspaces = new ();

    public Dictionary<string, byte[]> UserWorkspaces
    {
        get => m_userWorkspaces;
        set
        {
            m_userWorkspaces = value;
            m_bools[c_userWorkspacesSetIdx] = true;
        }
    }

    private byte[] m_userPreferences;

    public byte[] UserPreferences
    {
        get => m_userPreferences;
        set
        {
            m_userPreferences = value;
            m_bools[c_userPreferencesSetIdx] = true;
        }
    }

    public bool UserPreferencesSet => m_bools[c_userPreferencesSetIdx];

    private string m_skinName = "";

    public string SkinName
    {
        get => m_skinName;
        set => m_skinName = value;
    }

    public override string Description => "User settings changed";
}