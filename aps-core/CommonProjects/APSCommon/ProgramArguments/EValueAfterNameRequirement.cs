namespace PT.APSCommon.ProgramArguments;

public enum EValueAfterNameRequirement
{
    /// <summary>
    /// A value must be specified after the colon.
    /// </summary>
    Required,

    /// <summary>
    /// The value after the colon is optional.
    /// </summary>
    Optional,

    /// <summary>
    /// A value can't be specified after the colon.
    /// </summary>
    NoValue
}