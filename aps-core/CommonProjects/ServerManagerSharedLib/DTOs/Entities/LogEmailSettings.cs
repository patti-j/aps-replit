namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public class LogEmailSettings
    {
        public LogEmailSettings(string a_instanceName, string a_instanceVersion, string a_smtpServerAddress, int a_smtpServerPortNbr, string a_smtpFromEmailAddress,
                                bool a_smtpUseSsl, string a_smtpUsername, string a_smtpPasswordEncrypted, string a_logEmailToAddresses, string a_logEmailSubjectPrefix,
                                string a_supportEmailToAddresses, string a_supportEmailSubjectPrefix)
        {
            m_smtpServerAddress = a_smtpServerAddress;
            m_smtpServerPortNbr = a_smtpServerPortNbr;
            m_smtpUseSsl = a_smtpUseSsl;
            m_smtpUsername = a_smtpUsername;
            m_smtpPasswordEncrypted = a_smtpPasswordEncrypted;
            m_logEmailToAddresses = a_logEmailToAddresses;
            m_logEmailSubjectPrefix = a_logEmailSubjectPrefix;
            m_supportEmailToAddresses = a_supportEmailToAddresses;
            m_supportEmailSubjectPrefix = a_supportEmailSubjectPrefix;
        }

        public bool SendLogEmails
        {
            get { return !string.IsNullOrEmpty(LogEmailToAddresses) && !string.IsNullOrEmpty(LogEmailToAddresses); }
        }

        string m_smtpServerAddress;

        /// <summary>
        /// Network address to the SMTP server to use to send emails.
        /// </summary>
        public string SmtpServerAddress
        {
            get { return m_smtpServerAddress; }
            set { m_smtpServerAddress = value; }
        }

        int m_smtpServerPortNbr = 25;
        public int SmtpServerPortNbr
        {
            get { return m_smtpServerPortNbr; }
            set { m_smtpServerPortNbr = value; }
        }

        string m_smtpFromEmailAddress;

        /// <summary>
        /// Network address to the SMTP server to use to send emails.
        /// </summary>
        public string SmtpFromEmailAddress
        {
            get { return m_smtpFromEmailAddress; }
            set { m_smtpFromEmailAddress = value; }
        }

        bool m_smtpUseSsl;
        public bool SmtpUseSsl
        {
            get { return m_smtpUseSsl; }
            set { m_smtpUseSsl = value; }
        }

        string m_smtpUsername;
        public string SmtpUsername
        {
            get { return m_smtpUsername; }
            set { m_smtpUsername = value; }
        }

        string m_smtpPasswordEncrypted;
        public string SmtpEncryptedPassword
        {
            get { return m_smtpPasswordEncrypted; }
            set { m_smtpPasswordEncrypted = value; }
        }

        /// <summary>
        /// The email to use when sending log emails.
        /// </summary>
        string DefaultLogEmailFromAddress => "public@planettogether.com";

        string m_logEmailToAddresses;

        /// <summary>
        /// Specifies to whom emails should be sent when logs are automatically e-mailed.
        /// Separate multiple e-mails addresses with semi-colon.
        /// </summary>
        public string LogEmailToAddresses
        {
            get { return m_logEmailToAddresses; }
            set { m_logEmailToAddresses = value; }
        }

        string m_logEmailSubjectPrefix;
        public string LogEmailSubjectPrefix
        {
            get { return m_logEmailSubjectPrefix; }
            set { m_logEmailSubjectPrefix = value; }
        }


        string m_supportEmailToAddresses;
        public string SupportEmailToAddresses
        {
            get { return m_supportEmailToAddresses; }
            set { m_supportEmailToAddresses = value; }
        }

        string m_supportEmailSubjectPrefix;
        public string SupportEmailSubjectPrefix
        {
            get { return m_supportEmailSubjectPrefix; }
            set { m_supportEmailSubjectPrefix = value; }
        }
    }
}
