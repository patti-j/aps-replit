namespace PT.SchedulerDefinitions.MassRecordings;

public class MassRecordingsTableDefinitions
{
    /// <summary>
    /// ScenarioDetailReceive database table column titles for checksum data.
    /// </summary>
    public struct ActionChecksums
    {
        public string TableName => "ActionChecksums";
        public string StartAndEndSums => "StartAndEndSums";
        public string ResourceJobOperationCombos => "ResourceJobOperationCombos";
        public string BlockCount => "BlockCount";
        public string ScheduleDescription => "ScheduleDescription";
        public string NbrOfSimulations => "NbrOfSimulations";
        public string ScenarioId => "ScenarioId";
        public string SessionId => "SessionId";
        public string PlayerPath => "PlayerPath";
        public string TransmissionType => "TransmissionType";
        public string TransmissionNbr => "TransmissionNbr";
    }

    /// <summary>
    /// ScenarioDetailReceive database table column titles for checksum data.
    /// </summary>
    public struct ScheduleIssues
    {
        public string TableName => "ScheduleIssues";
        public string ScenarioId => "ScenarioId";
        public string SessionId => "SessionId";
        public string PlayerId => "PlayerId";
        public string TransmissionType => "TransmissionType";
        public string TransmissionNbr => "TransmissionNbr";
        public string LateObjectType => "LateObjectType";
        public string LateObjectName => "LateObjectName";
        public string LateObjectId => "LateObjectId";
    }

    /// <summary>
    /// ErrorReporter database table column titles for error data.
    /// </summary>
    public struct InstanceLogs
    {
        public string TableName => "InstanceLogs";
        public string InstanceName => "InstanceName";
        public string SoftwareVersion => "SoftwareVersion";
        public string TypeName => "TypeName";
        public string Message => "Message";
        public string StackTrace => "StackTrace";
        public string Source => "Source";
        public string InnerExceptionMessage => "InnerExceptionMessage";
        public string InnerExceptionStackTrace => "InnerExceptionStackTrace";
        public string LogType => "LogType";
        public string HeaderMessage => "HeaderMessage";
    }
}