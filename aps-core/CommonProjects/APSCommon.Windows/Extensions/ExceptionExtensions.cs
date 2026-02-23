namespace PT.APSCommon.Windows.Extensions;

public static class ExceptionExtensions
{
    public static PTMessage CreatePTMessage(this Exception a_e)
    {
        PTMessage message = new ();
        message.LoadFromException(a_e);
        return message;
    }

    public static PTMessage CreatePTErrorMessage(this Exception a_e)
    {
        PTMessage message = new ();
        message.LoadFromException(a_e, PTMessage.EMessageClassification.Error);
        return message;
    }
}