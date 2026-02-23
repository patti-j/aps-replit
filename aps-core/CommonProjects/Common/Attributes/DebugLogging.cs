namespace PT.Common.Attributes;

public enum EDebugLoggingType
{
    /// <summary>
    /// Value not set
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// Skip all logging of this item
    /// </summary>
    None = 1,

    /// <summary>
    /// Log only sub objects. Applies to arrays and lists
    /// </summary>
    IterateOnly = 2,

    /// <summary>
    /// Log using default settings
    /// </summary>
    Default = 3,

    /// <summary>
    /// Log using default settings but also include properties
    /// </summary>
    Properties = 4,

    /// <summary>
    /// Log as much information as possible about this object. Includes sub objects and properties
    /// </summary>
    Verbose = 5
}

/// <summary>
/// Can be used to indicate how to log this field or property for debugging purposes
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class DebugLogging : Attribute
{
    public readonly EDebugLoggingType LoggingType;

    public DebugLogging(EDebugLoggingType a_logType)
    {
        LoggingType = a_logType;
    }
}