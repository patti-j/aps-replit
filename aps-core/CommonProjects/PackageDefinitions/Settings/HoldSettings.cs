using System.Collections;

using PT.APSCommon.Extensions;

namespace PT.PackageDefinitions.Settings;

/// <summary>
/// Stores the customized settings for holding Jobs.
/// </summary>
public class HoldSettings : IPTSerializable, ICloneable, ISettingData
{
    #region IPTSerializable Members
    public HoldSettings(IReader reader)
    {
        if (reader.VersionNumber >= 162)
        {
            reader.Read(out m_limitToList);
            reader.Read(out requireReason);
            int count;
            reader.Read(out count);
            string reason;
            for (int i = 0; i < count; i++)
            {
                reader.Read(out reason);
                AddReason(reason);
            }
        }
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_limitToList);
            int count;
            reader.Read(out count);
            string reason;
            for (int i = 0; i < count; i++)
            {
                reader.Read(out reason);
                AddReason(reason);
            }
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_limitToList);
        writer.Write(requireReason);
        writer.Write(m_reasons.Count);
        for (int i = 0; i < m_reasons.Count; i++)
        {
            writer.Write((string)m_reasons[i]);
        }
    }

    public const int UNIQUE_ID = 451;

    [System.ComponentModel.Browsable(false)]
    public int UniqueId => UNIQUE_ID;
    #endregion

    public HoldSettings()
    {
        AddDefaultReasons();
    }

    public HoldSettings(bool a_loadDefaults)
    {
        if (a_loadDefaults)
        {
            AddDefaultReasons();
        }
    }

    public void AddDefaultReasons()
    {
        AddReason("1. Material Hold".Localize());
        AddReason("2. Tooling Hold".Localize());
        AddReason("3. Production Hold".Localize());
        AddReason("4. Engineering Hold".Localize());
        AddReason("5. Customer Hold".Localize());
        AddReason("6. Quality Hold".Localize());
        AddReason("7. Lack of Material".Localize());
        AddReason("8. Design Change Pending".Localize());
    }

    private readonly ArrayList m_reasons = new ();

    private bool m_limitToList = true;

    /// <summary>
    /// If true then selections can when placing work on Hold, only pre-defined reasons can be used.  Otherwise, custom reasons can be entered.
    /// </summary>
    public bool LimitToList
    {
        get => m_limitToList;
        set => m_limitToList = value;
    }

    private bool requireReason;

    /// <summary>
    /// If true then to be placed on Hold a Reason must always be specified.
    /// </summary>
    public bool RequireReason
    {
        get => requireReason;
        set => requireReason = value;
    }

    /// <summary>
    /// Gets an array of all the hold reasons, and will add "None" if the passed in bool is true
    /// </summary>
    public string[] GetReasons(bool a_addNone)
    {
        List<string> reasonsList = new ();
        if (!requireReason && a_addNone)
            //Add a blank reason since one is not required
        {
            reasonsList.Add("");
        }

        for (int i = 0; i < m_reasons.Count; i++)
        {
            reasonsList.Add((string)m_reasons[i]);
        }

        return reasonsList.ToArray();
    }

    public void AddReason(string a_reason)
    {
        m_reasons.Add(a_reason);
    }

    /// <summary>
    /// Delete all Hold Reasons from the list of reasons.
    /// </summary>
    public void ClearReasons()
    {
        m_reasons.Clear();
    }

    #region ICloneable Members
    object ICloneable.Clone()
    {
        return Clone();
    }

    public HoldSettings Clone()
    {
        HoldSettings clone = new ();
        clone.m_limitToList = LimitToList;
        for (int i = 0; i < m_reasons.Count; i++)
        {
            clone.AddReason((string)m_reasons[i]);
        }

        return clone;
    }
    #endregion

    public string SettingKey => Key;
    public string SettingCaption => "Hold Settings";
    public string Description => "Manage hold settings and hold reasons";
    public string SettingsGroup => SettingGroupConstants.ScenariosGroup;
    public string SettingsGroupCategory => "Hold";
    public static string Key => "scenarioSetting_holdSettings";
}