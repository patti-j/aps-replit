using System.Text;
using System.Windows.Forms;

using PT.APSCommon.Extensions;
using PT.Common.Extensions;
using PT.Common.Localization;

namespace PT.APSCommon.Windows;

/// <summary>
/// This class contains all information needed to display a message to the user. It can be used by message forms to display the information in a user friendly layout.
/// </summary>
public class PTMessage : ILocalizable
{
    public PTMessage() { }

    public PTMessage(string a_message, string a_title)
    {
        Title = a_title;
        Message = a_message;
    }

    public string Title;

    private string m_message;

    public string Message
    {
        get => m_message;
        set
        {
            m_message = value;
            ParseHelpURL(m_message);
            if (!string.IsNullOrEmpty(HelpUrl))
            {
                m_message = m_message.Replace(HelpUrl, "");
                m_message = m_message.TrimEnd('n');
                m_message = m_message.TrimEnd('r');
                m_message = m_message.TrimEnd('n');
                m_message = m_message.TrimEnd('r');
                m_message = m_message.Trim();
            }
        }
    }

    public string ErrorCode;
    public string Details;
    public string Tips;
    public string HelpUrl;

    private EMessageClassification m_classification;

    public EMessageClassification Classification
    {
        get => m_classification;
        set
        {
            m_classification = value;
            if (value == EMessageClassification.Question)
            {
                Action = EMessageActions.YesNo;
            }
        }
    }

    public EMessageActions Action;

    public enum EMessageClassification
    {
        Unknown,
        Information,
        Notification,
        Question,
        Warning,
        Error
    }

    public enum EMessageActions { Ok, YesNo }

    public bool ContainsHelpLink()
    {
        return !string.IsNullOrEmpty(HelpUrl);
    }

    private void ParseHelpURL(string a_string)
    {
        string helpUrl = Localizer.GetHelpUrl();
        if (!string.IsNullOrEmpty(helpUrl) && a_string.Contains(helpUrl))
        {
            try
            {
                int pos = a_string.IndexOf(helpUrl);
                //TODO: make this more generic
                const int errorCodeLength = 4;
                HelpUrl = a_string.Substring(pos, helpUrl.Length + errorCodeLength);
                ErrorCode = HelpUrl.Substring(HelpUrl.Length - errorCodeLength, errorCodeLength);
            }
            catch (Exception)
            {
                HelpUrl = null;
                ErrorCode = null;
            }
        }
    }

    public string GetFullMessageText()
    {
        StringBuilder sb = new ();
        if (!string.IsNullOrEmpty(ErrorCode))
        {
            sb.Append(ErrorCode);
            sb.Append(Environment.NewLine);
        }

        if (!string.IsNullOrEmpty(Message))
        {
            sb.Append(Environment.NewLine);
            sb.Append(Message);
            sb.Append(Environment.NewLine);
        }

        if (!string.IsNullOrEmpty(Tips))
        {
            sb.Append(Environment.NewLine);
            sb.Append(Tips);
            sb.Append(Environment.NewLine);
        }

        if (!string.IsNullOrEmpty(HelpUrl))
        {
            sb.Append(Environment.NewLine);
            sb.Append(HelpUrl);
            sb.Append(Environment.NewLine);
        }

        if (!string.IsNullOrEmpty(Details))
        {
            sb.Append(Environment.NewLine);
            sb.Append(Details);
        }

        return sb.ToString();
    }

    public async Task<bool> CopyFullMessageToClipboard()
    {
        string message = GetFullMessageText();
        //Run in a new thread since this is a popup. It may not have STA ApartmentState
        return await Task.Factory.StartNew(() =>
            {
                try
                {
                    Clipboard.SetText(message);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.FromCurrentSynchronizationContext());
        //t.SetApartmentState(ApartmentState.STA);
        //t.Start();
    }

    public void Localize()
    {
        Title = Title.Localize();
        Message = Message.Localize();
        Tips = Tips.Localize();
    }

    public void LoadFromException(Exception a_exception, EMessageClassification a_classification = EMessageClassification.Warning)
    {
        Title = "System Message";
        Message = a_exception.Message;
        Details = a_exception.GetExceptionFullMessage() + a_exception.GetExceptionFullStackTrace();
        Classification = a_classification;
    }
}