namespace PT.APIDefinitions.RequestsAndResponses;

public class PostPublishWebHookResponse
{
    public List<string> Errors { get; set; }

    public string GetFullErrorMessage()
    {
        return string.Concat(Errors.Select(e => e + Environment.NewLine + Environment.NewLine));
    }
}