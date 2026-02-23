using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class PermissionKey
    {
        public string Key { get; set; }

        public PermissionKey() {
        }

        public PermissionKey(string key)
        {
            Key = key;
        }

        public override string ToString()
        {
            return Key;
        }
        
        public static implicit operator string(PermissionKey key) => key.Key;
        public static implicit operator PermissionKey(string key) => new() { Key = key };
    }
}
