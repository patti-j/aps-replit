using System.Net.Mail;

namespace PT.Common;

public class Emailer : IDisposable
{
    private const string c_systemAccountAddress = "system@planettogether.com";
    private const string c_systemAccountPassword = "aylvgksdoxfegpyp";
    private const string c_emailServiceHost = "smtp.gmail.com";
    private const int c_emailServicePort = 587;

    /// <summary>
    /// Create emailer with PlanetTogether Outlook server account
    /// </summary>
    /// <returns></returns>
    public static Emailer CreateWithPTSmtpSettings()
    {
        return new Emailer(c_emailServiceHost, c_emailServicePort, true, c_systemAccountAddress, c_systemAccountPassword);
    }

    public Emailer(string a_smtpServerAddress, int a_smtpServerPortNbr, bool a_enableSsl, string a_smtpUserName, string a_smtpPassword)
    {
        m_client = new SmtpClient(a_smtpServerAddress, a_smtpServerPortNbr)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            EnableSsl = a_enableSsl
        };
        if (!string.IsNullOrEmpty(a_smtpUserName))
        {
            m_client.Credentials = new System.Net.NetworkCredential(a_smtpUserName, a_smtpPassword);
        }
    }

    private MailMessage m_outMessage;
    private readonly SmtpClient m_client;

    /// <summary>
    /// Creates a new email message. Will not be sent unless one of the Send functions is called.
    /// </summary>
    /// <param name="a_subject">Subject line</param>
    /// <param name="a_body">Email Body</param>
    /// <param name="a_emailTargets">Email addresses to send the message to</param>
    /// <param name="a_fromEmail">Email address to send the message from.</param>
    public void NewEmail(string a_subject, string a_body, List<string> a_emailTargets, string a_fromEmail)
    {
        m_outMessage = new MailMessage();
        m_outMessage.Body = a_body;
        m_outMessage.Subject = a_subject;
        m_outMessage.From = new MailAddress(a_fromEmail);

        foreach (string address in a_emailTargets)
        {
            m_outMessage.To.Add(new MailAddress(address));
        }
    }

    /// <summary>
    /// Send without waiting. No error will be given if the message fails to send
    /// </summary>
    public void SendAsync()
    {
        try
        {
            m_client.SendAsync(m_outMessage, null);
        }
        catch
        {
            //ignore
        }
    }

    /// <summary>
    /// Send message and wait for confirmation or error. This action can take several seconds
    /// </summary>
    public void Send()
    {
        m_client.Send(m_outMessage);
    }

    private bool m_disposed;

    public void Dispose()
    {
        if (!m_disposed)
        {
            m_disposed = true;
            if (m_client != null)
            {
                m_client.Dispose();
            }
        }
    }
}