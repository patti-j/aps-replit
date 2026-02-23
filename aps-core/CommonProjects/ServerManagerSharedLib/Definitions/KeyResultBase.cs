namespace PT.ServerManagerSharedLib.Definitions
{
    public class KeyResultBase : Result
    {
        public bool InvalidSerialCode { get; set; }
        public bool InvalidSystemIdFormat { get; set; }
        public bool LicenseIsSystemIdType { get; set; }
        public bool LicenseIsWindowsProductIdType { get; set; }
    }
}
