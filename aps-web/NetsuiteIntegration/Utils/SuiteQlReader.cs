using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetsuiteIntegration.Utils
{
    public interface ISuiteQlReader
    {
        string Read(string logicalName);
    }

    
    public sealed class SuiteQlReader : ISuiteQlReader
    {
        private readonly string? _folder;
        private readonly Assembly _asm;
        private readonly string _embeddedPrefix;
        
        public SuiteQlReader(string? a_folder = null)
        {
            _folder = string.IsNullOrWhiteSpace(a_folder) ? null : a_folder;
            _asm = typeof(SuiteQlReader).Assembly;
            _embeddedPrefix = $"{_asm.GetName().Name}.SuiteQL."; 
        }

        public string Read(string logicalName)
        {            
            if (!string.IsNullOrWhiteSpace(_folder))
            {
                string path = Path.Combine(_folder!, logicalName);
                if (File.Exists(path))
                    return File.ReadAllText(path, Encoding.UTF8);
            }
            
            string resourceName = _embeddedPrefix + logicalName;
            using Stream? s = _asm.GetManifestResourceStream(resourceName);
            if (s == null)
            {
                string available = string.Join(", ",
                    _asm.GetManifestResourceNames().Where(n => n.StartsWith(_embeddedPrefix)));
                throw new FileNotFoundException(
                    $"SuiteQL not found: {logicalName}. Embedded prefix: {_embeddedPrefix}. Available: {available}");
            }

            using StreamReader reader = new StreamReader(s, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
