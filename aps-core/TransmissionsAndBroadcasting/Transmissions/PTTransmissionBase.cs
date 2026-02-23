using System.Xml.Serialization;

namespace PT.Transmissions;

/// <summary>
/// Summary description for PTTransmissionBase.
/// </summary>
public abstract class PTTransmissionBase : Transmission
{
    #region IPTSerializable Members
    public PTTransmissionBase(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 22)
        {
            reader.Read(out m_logErrorToUsualLogFile);

            int transmissionSenderTemp;
            reader.Read(out transmissionSenderTemp);
            m_transmissionSender = (TransmissionSenderType)transmissionSenderTemp;
            reader.Read(out m_reportAsEvent);
        }

        #region 21
        else if (reader.VersionNumber >= 21)
        {
            reader.Read(out m_logErrorToUsualLogFile);

            int transmissionSenderTemp;
            reader.Read(out transmissionSenderTemp);
            m_transmissionSender = (TransmissionSenderType)transmissionSenderTemp;
        }
        #endregion

        #region 19
        else if (reader.VersionNumber >= 19)
        {
            reader.Read(out m_logErrorToUsualLogFile);
        }
        #endregion
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_logErrorToUsualLogFile);
        writer.Write((int)m_transmissionSender);
        writer.Write(m_reportAsEvent);
    }

    //public new const int UNIQUE_ID = 97;
    #endregion

    #region Construction - Just a no argument constructor at the moment.
    public PTTransmissionBase() { }

    public PTTransmissionBase(PTTransmissionBase a_t)
    {
        m_logErrorToUsualLogFile = a_t.m_logErrorToUsualLogFile;
        m_transmissionSender = a_t.m_transmissionSender;
        m_reportAsEvent = a_t.m_reportAsEvent;
    }
    #endregion

    #region LogError - Whether any warnings that occur while this transmission is being processed should be logged to one of the log files.
    private bool m_logErrorToUsualLogFile;

    /// <summary>
    /// Whether any errors that result from this transmission should be logged to the usual log file appropriate for the error.
    /// These will be logged to some log file. At the time of this note was put in they logged as warnings in the Misc.log file, which
    /// generally should contain entries that are of no interest unless an error was incorrectly assigned that status of being insignificant.
    /// Examples of errors that you might not want to log are simple errors the user will encouter in the user interface. For instance
    /// attempting an invalid drag and drop or an invalid clock advance. These errors are displayed to the user in an error box already.
    /// These types of errors are just simple validation errors.
    /// Non-validation errors should be reported. So for instance if something serious is detected in the simulation it would need to be logged
    /// for support purposes.
    /// If a third party system ever attempts to make use of some of the typically PT user initiated transmissions, these transmissions
    /// should have their LogError setting set to true.
    /// Examples of errors you would always want to log are ERP Transmissions, since otherwise you would have no idea what has happened.
    /// </summary>
    [XmlIgnore]
    public bool LogErrorToUsualLogFile
    {
        get => m_logErrorToUsualLogFile;

        set => m_logErrorToUsualLogFile = value;
    }
    #endregion

    #region ReportAsEvent - Only relevant when TransmissionSender is PTUser.
    private bool m_reportAsEvent = true;

    /// <summary>
    /// Only relevant when TransmissionSender is PTUser.
    /// </summary>
    [XmlIgnore]
    public bool ReportAsEvent
    {
        get => m_reportAsEvent;

        set => m_reportAsEvent = value;
    }
    #endregion

    #region TransmissionSender - Identifies the type of transmission sender. ExternalInterface, PTInterface, PTSystem, PTUser.
    /// <summary>
    /// Indicates the type of process that sent this transmission.
    /// </summary>
    public enum TransmissionSenderType
    {
        /// <summary>
        /// The transmission was created by some other process, such as a custom interface created for an ERP system.
        /// </summary>
        ExternalInterface,

        /// <summary>
        /// Sent based on actions of the PT Interface.
        /// </summary>
        PTInterface,

        /// <summary>
        /// Don't use. Sent from the PTSystem to itself.
        /// </summary>
        PTSystem,

        /// <summary>
        /// Sent directly from the PT User interface.
        /// </summary>
        PTUser
    }

    private TransmissionSenderType m_transmissionSender = TransmissionSenderType.PTInterface;

    /// <summary>
    /// Indicated who sent this transmission. The default is PTInterface. This may affect things such as logging.
    /// </summary>
    [XmlIgnore]
    public TransmissionSenderType TransmissionSender
    {
        get => m_transmissionSender;
        set => m_transmissionSender = value;
    }
    #endregion
}