using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// A record of a Transmission for the Scenario.
/// </summary>
public class ScenarioHistory : ICloneable, IPTSerializable
{
    public const int UNIQUE_ID = 340;

    #region IPTSerializable Members
    public ScenarioHistory(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out key);
            reader.Read(out objectTypeName);
            reader.Read(out description);
            int val;
            reader.Read(out val);
            historyType = (historyTypes)val;

            reader.Read(out timestamp);

            user = new BaseId(reader);
            plant = new BaseId(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        #if DEBUG
        writer.DuplicateErrorCheck(this);
        #endif
        writer.Write(key);
        writer.Write(objectTypeName);
        writer.Write(description);
        writer.Write((int)historyType);

        writer.Write(timestamp);

        user.Serialize(writer);
        plant.Serialize(writer);
    }

    [Browsable(false)]
    public int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    //Properties
    public const string DESCRIPTION = "Description";
    public const string TIME_STAMP = "TimeStamp";
    public const string OBJECT_KEY = "Key"; //Must match object's Property name
    public const string OBJECT_TYPE_NAME = "ObjectType"; //Must match object's Property name
    public const string USER = "User";
    public const string HISTORY_TYPE = "HistoryType";

    public enum historyTypes
    {
        JobAdded,
        JobNeedDateEarlier,
        JobNeedDateLater,
        JobQtyIncreased,
        JobQtyDecreased,
        JobPriorityIncreased,
        JobPriorityDecreased,
        JobHot,
        JobNotHot,
        JobCancelled,
        JobCommitmentChanged,
        JobRoutingChanged,
        JobMaterialsChanged,
        FinishedActivity,
        FinishedJob,
        ExcessiveScrap,
        SlowSetup,
        SlowRun,
        MaterialDelay,
        Miscellaneous,
        ErpMaintenance,
        JobChanged,
        JobMoved,
        JobExpedited
    } //Used by Execution and Demand Monitors.  TODO JMC JobRoutingChanged,JobMaterialsChanged, MaterialDelay

    public class ScenarioHistoryException : PTException
    {
        public ScenarioHistoryException(string e)
            : base(e) { }
    }
    #endregion

    public ScenarioHistory(string key, string objectTypeName, DateTime timeStamp, BaseId user, historyTypes historyType, string description)
    {
        plant = BaseId.NULL_ID;
        this.key = key;
        this.objectTypeName = objectTypeName;
        timestamp = timeStamp;
        this.user = user;
        this.historyType = historyType;
        this.description = description;
    }

    public ScenarioHistory(BaseId plant, string key, string objectTypeName, DateTime timeStamp, BaseId user, historyTypes historyType, string description)
    {
        this.plant = plant;
        this.key = key;
        this.objectTypeName = objectTypeName;
        timestamp = timeStamp;
        this.user = user;
        this.historyType = historyType;
        this.description = description;
    }

    #region Properties
    private string key;

    /// <summary>
    /// Unique identifier (made up of Ids) for the object affected by the transmission (if any).
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public string Key
    {
        get => key;
        set => key = value;
    }

    private string objectTypeName;

    /// <summary>
    /// Name of the Type of BaseObject affected by the transmission (if any).
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public string ObjectType
    {
        get => objectTypeName;
        set => objectTypeName = value;
    }

    private string description;

    /// <summary>
    /// Describes the action that caused this history.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public string Description
    {
        get => description;
        set => description = value;
    }

    private DateTime timestamp;

    /// <summary>
    /// When the action ocurred.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime Timestamp
    {
        get => timestamp;
        set => timestamp = value;
    }

    private BaseId user;

    /// <summary>
    /// The User who instigated the action.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public BaseId User
    {
        get => user;
        set => user = value;
    }

    private readonly historyTypes historyType;

    /// <summary>
    /// Type type of action that created the history.
    /// </summary>
    public historyTypes HistoryType => historyType;

    private BaseId plant;

    /// <summary>
    /// The Plant Id (if any).
    /// </summary>
    public BaseId Plant
    {
        get => plant;
        set => plant = value;
    }
    #endregion

    #region Cloning
    public ScenarioHistory Clone()
    {
        return (ScenarioHistory)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion
}