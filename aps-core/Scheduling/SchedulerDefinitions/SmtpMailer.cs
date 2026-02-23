using System.Diagnostics;

namespace PT.SchedulerDefinitions;

//TODO: Get working the CORE way
/// <summary>
/// Summary description for SmtpMailer.
/// </summary>
public class SmtpMailer
{
    /// <summary>
    /// Used for sending mail via SMTP.
    /// </summary>
    /// <param name="a_smtpServer">Outgoing mail server such as mail.planettogether.com</param>
    public SmtpMailer(string a_smtpServer)
    {
        //SmtpMail.SmtpServer = a_smtpServer;
        //server = a_smtpServer;
    }

    private readonly string server;

    public string Server => server;

    [Conditional("DEBUG")] //Note this could still send emails when running customer data in release mode
    public void SendMail(Email email)
    {
        //try
        //{
        //    SmtpMail.Send(email);
        //}
        //catch (Exception e)
        //{
        //    throw new APSCommon.PTHandleableException("2600", e, new object[] { SmtpMail.SmtpServer, e.Message } );
        //}
    }

    public class Email //: MailMessage
    {
        public Email(string fromAddress, string toAddress, string subject, string body, bool htmlFormat)
        {
            //From = fromAddress;
            //To = toAddress;
            //Subject = subject;
            //Body = body;
            //if (htmlFormat)
            //    BodyFormat = MailFormat.Html;
            //else
            //    BodyFormat = MailFormat.Text;
        }

        public void AddAttachment(string a_attachmentFile)
        {
            if (!string.IsNullOrEmpty(a_attachmentFile))
            {
                //MailAttachment myAttach = new MailAttachment(a_attachmentFile, MailEncoding.Base64);
                //Attachments.Add(myAttach);
            }
        }
    }

    public void SendMail(string fromAddress, string toAddress, string subject)
    {
        //MailMessage newMessage = new MailMessage();
        //newMessage.From = fromAddress;
        //newMessage.To = toAddress;
        //newMessage.Subject = subject;

        //string attachmentFile = "C:\\test.txt";
        //string currentServer = "mail.planettogether.com";

        //if (attachmentFile != String.Empty && attachmentFile != null)
        //{
        //    MailAttachment myAttach =
        //        new MailAttachment(attachmentFile, MailEncoding.Base64);
        //    newMessage.Attachments.Add(myAttach);
        //}

        //newMessage.Body = richTextBoxMail.Text;
        //SmtpMail.SmtpServer = currentServer;

        //if (newMessage.Body != String.Empty && newMessage.Body != null && newMessage.To != String.Empty && newMessage.From != String.Empty)
        //{
        //    newMessage.BodyFormat = MailFormat.Html;

        //    try
        //    {
        //        SmtpMail.Send(newMessage);
        //    }
        //    catch (System.Web.HttpException)
        //    {
        //        MessageBox.Show("Application can't send your EMail - See The Help Content", "EMailSender - Sending Error", MessageBoxButtons.OK,
        //            MessageBoxIcon.Error);
        //    }
        //}
        //else
        //    MessageBox.Show("Empty Message body and/or empty 'To', 'From' fields", "EMailSender - Information",
        //        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }
}