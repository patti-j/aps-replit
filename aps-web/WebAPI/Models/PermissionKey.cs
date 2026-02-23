namespace WebAPI.Models
{
    public class PermissionKey
    {
        public string Key { get; set; }

        public PermissionKey()
        {
        }

        public PermissionKey(string key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return Key;
        }
    }
}
