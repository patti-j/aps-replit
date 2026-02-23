namespace PT.Common.Http;

public class ApiException : Exception
{
    public int StatusCode { get; set; }
    public string Content { get; set; }

    public override string Message => $"Status {StatusCode}: {Content}";
}