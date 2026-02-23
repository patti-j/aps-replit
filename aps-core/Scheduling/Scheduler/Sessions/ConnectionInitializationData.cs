namespace PT.Scheduler.Sessions;

public class ConnectionInitializationData
{
    public long TimeWaitingForScenarioData { get; set; }

    public long TimeTransferingScenarioData { get; set; }

    public long TimeToDeserializeSystemTicks { get; set; }

    public long TimeToLoadDeserializeAndPerformOtherInitializationsTicks { get; set; }

    /// <summary>
    /// Has no affect on the broadcaster's timeout for this connection. The value is copied into this field and may be used for things like ToString. It's possible this value hasn't been set or is incorrect.
    /// </summary>
    public long TimeoutIntervalTicks { get; set; }

    /// <summary>
    /// Has no affect on the actual rate contacts are made. This value is copied into this field and may be used for things like ToString. It's possible this value hasn't been set or is incorrect.
    /// </summary>
    public long ContactIntervalTicks { get; set; }

    /// <summary>
    /// Has no affect on the actual rate contacts are made. This value is copied into this field and may be used for things like ToString. It's possible this value hasn't been set or is incorrect.
    /// </summary>
    public long ClientUpdaterFinished { get; set; }

    /// <summary>
    /// Has no affect on the actual rate contacts are made. This value is copied into this field and may be used for things like ToString. It's possible this value hasn't been set or is incorrect.
    /// </summary>
    public long ClientWorkspacesLoaded { get; set; }

    /// <summary>
    /// Has no affect on the actual rate contacts are made. This value is copied into this field and may be used for things like ToString. It's possible this value hasn't been set or is incorrect.
    /// </summary>
    public long ClientWorkspacesApplied { get; set; }

    /// <summary>
    /// Has no affect on the actual rate contacts are made. This value is copied into this field and may be used for things like ToString. It's possible this value hasn't been set or is incorrect.
    /// </summary>
    public long ClientScneariosLoaded { get; set; }

    /// <summary>
    /// Has no affect on the actual rate contacts are made. This value is copied into this field and may be used for things like ToString. It's possible this value hasn't been set or is incorrect.
    /// </summary>
    public long ClientFullyLoaded { get; set; }

    public long ScenarioBytesTransferred;
}