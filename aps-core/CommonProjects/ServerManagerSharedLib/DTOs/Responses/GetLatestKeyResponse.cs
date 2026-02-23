using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class GetLatestKeyResponse
    {
        public KeyResultBase ResultKey { get; set; }
        public object KeyStream { get; set; }
    }
}
