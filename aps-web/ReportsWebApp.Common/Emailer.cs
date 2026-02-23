using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Identity.Client;
using MimeKit;

namespace PT.Common;

public class Emailer
{
    private const string c_systemAccountAddress = "system@planettogether.com";
    private const string c_systemOAuth2Secret = "NTN8Q~AWSHzLOJ6LZa5kGkyR-bjXxJrgb5XaCdzP";
    private const string c_emailServiceHost = "smtp-mail.outlook.com";
    private const string c_tenantId = "c41e2af3-6d94-41b8-9fa5-94c0d8dfcd99";
    private const string c_clientId = "c2331aba-8899-47a3-9d87-18ad7307994d";
    private const int c_emailServicePort = 587;

    /// <summary>
    /// Create emailer with PlanetTogether Outlook server account
    /// </summary>
    /// <returns></returns>
    public static Emailer CreateWithPTSmtpSettings()
    {
        return new Emailer(c_emailServiceHost, c_emailServicePort, c_systemAccountAddress, c_systemOAuth2Secret);
    }

    private string m_smtpServerAddress;
    private int m_smtpServerPort;
    private string m_smtpUserName;
    private string m_oAuth2Secret;

    public Emailer(string a_smtpServerAddress, int a_smtpServerPortNbr, string a_smtpUserName, string a_oAuth2Secret)
    {
        m_smtpServerAddress = a_smtpServerAddress;
        m_smtpServerPort = a_smtpServerPortNbr;
        m_smtpUserName = a_smtpUserName;
        m_oAuth2Secret = a_oAuth2Secret;
    }

    private (DateTimeOffset ExpiresOn, string AccessToken) m_AuthToken = default;
    
    /// <summary>
    /// Creates a new email message. Will not be sent unless one of the Send functions is called.
    /// </summary>
    /// <param name="a_subject">Subject line</param>
    /// <param name="a_body">Email Body</param>
    /// <param name="a_emailTargets">Email addresses to send the message to</param>
    /// <param name="a_fromEmail">Email address to send the message from.</param>
    public async Task SendNewEmail(string a_subject, MimeEntity a_body, List<MailboxAddress> a_emailTargets, MailboxAddress a_fromEmail)
    {
        var message = new MimeMessage();
        message.Body = a_body;
        message.Subject = a_subject;
        message.From.Add(a_fromEmail);
        message.To.AddRange(a_emailTargets);

        if (m_AuthToken == default || m_AuthToken.ExpiresOn < DateTimeOffset.UtcNow)
        {
            m_AuthToken = await GetAuthToken();
        }
        
        using var client = new SmtpClient();
        await client.ConnectAsync(m_smtpServerAddress, m_smtpServerPort);
        await client.AuthenticateAsync(new SaslMechanismOAuth2(m_smtpUserName, m_AuthToken.AccessToken));
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task<(DateTimeOffset ExpiresOn, string AccessToken)> GetAuthToken()
    {
        var authority = $"https://login.microsoftonline.com/{c_tenantId}";

        var confidentialClient = ConfidentialClientApplicationBuilder.Create(c_clientId)
                                                                     .WithClientSecret(m_oAuth2Secret)
                                                                     .WithAuthority(authority)
                                                                     .Build();
        
        var scopes = new string[] { "https://outlook.office365.com/.default" };

        var result = await confidentialClient.AcquireTokenForClient(scopes).ExecuteAsync();
        
        return (result.ExpiresOn, result.AccessToken);
    }
    
    public void SendVerificationEmailAsync(string email, string pageUri)
    {
        //intentionally run as background task
        SendNewEmail("Complete Your PlanetTogether Account",
            new TextPart("html", $"Visit this link to finish setting up your PlanetTogether account.<br><br> <a href='{pageUri}'>Click here to activate</a><br><br>This link will expire in 24 hours."),
            new List<MailboxAddress>() { new MailboxAddress(email, email) }, new MailboxAddress("PT System", "system@planettogether.com"));
    }
}