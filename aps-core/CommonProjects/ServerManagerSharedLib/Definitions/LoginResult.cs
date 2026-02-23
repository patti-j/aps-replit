namespace PT.ServerManagerSharedLib.Definitions
{
    public class LoginResult
    {
        public SoftwareVersion SoftwareVersion { get; set; }
        public string UserLanguage { get; set; }
        public int v { get; set; }
        public byte[] SymmetricKey { get; set; }
    }
}
