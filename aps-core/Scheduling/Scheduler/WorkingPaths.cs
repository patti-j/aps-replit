using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// Contains static properties of paths to files such as the PT.xml file, etc...
/// </summary>
internal class KeyFilePaths
{
    private const string c_publicKeyFileName = "public.key";
    private const string c_keyFileName = "json.Key";
    private const string c_configFileName = "pt.json";

    internal static string PublicKeyPath
    {
        get
        {
            ValidateIsSetup(c_publicKeyFileName);
            return Path.Combine(PTSystem.WorkingDirectory.Key, c_publicKeyFileName);
        }
    }

    internal static string KeyPath
    {
        get
        {
            ValidateIsSetup(c_keyFileName);
            return Path.Combine(PTSystem.WorkingDirectory.Key, c_keyFileName);
        }
    }

    internal static string ConfigPath
    {
        get
        {
            #if DEBUG
            if (!string.IsNullOrEmpty(PTSystem.WorkingDirectory.KeyFile))
            {
                return PTSystem.WorkingDirectory.KeyFile;
            }
            #endif
            ValidateIsSetup(c_configFileName);
            return Path.Combine(PTSystem.WorkingDirectory.Key, c_configFileName);
        }
    }

    #region Validation functions and Exception definitions.
    public class KeyFilePathsException : PTException
    {
        internal KeyFilePathsException(string msg)
            : base(msg) { }

        public KeyFilePathsException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = true)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }
    }

    private static void ValidateIsSetup(string a_keyFileName)
    {
        if (PTSystem.WorkingDirectory == null)
        {
            throw new KeyFilePathsException("2965", new object[] { a_keyFileName });
        }
    }
    #endregion
}