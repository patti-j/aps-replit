namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class ActionInstanceResponse
    {
        public bool success { get; set; }
        public string error { get; set; }
        public object instance { get; set; }
    }
}