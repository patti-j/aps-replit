namespace PT.ServerManagerSharedLib.Definitions
{
    public class Result
    {
        public bool Exception { get; set; }
        public bool Success { get; set; }
        public bool FailedToUpdateDatabase { get; set; }
    }
}
