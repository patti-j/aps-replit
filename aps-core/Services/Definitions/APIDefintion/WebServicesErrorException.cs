using PT.APIDefinitions.RequestsAndResponses;

namespace PT.APIDefinitions;

public class WebServicesErrorException : Exception
{
    public WebServicesErrorException(EApsWebServicesResponseCodes a_code)
    {
        Code = a_code;
    }

    public WebServicesErrorException(EApsWebServicesResponseCodes a_code, string a_message)
    {
        Code = a_code;
        WebExceptionMessage = a_message;
    }

    public EApsWebServicesResponseCodes Code;
    public string WebExceptionMessage;
}