namespace PT.ServerManagerSharedLib.Definitions
{
    public class DeadConnectionFault
    {
        public DeadConnectionFault(string a_message)
        {
            Message = a_message;
        }
        public string Message { get; set; }
    }
}
