using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class SaveInstanceResponse
    {
        public bool created { get; set; }
        public bool saved { get; set; }
        public Instance instance { get; set; }
        public string error { get; set; }
        public bool restartRequired { get; set; }
    }
}