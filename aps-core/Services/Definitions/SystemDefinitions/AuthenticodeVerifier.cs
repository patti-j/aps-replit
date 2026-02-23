using System.Runtime.InteropServices;

namespace PT.SystemDefinitions;

public static class AuthenticodeVerifier
{
    // GUID for WinTrust generic verify action
    static readonly Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 =
        new("{00AAC56B-CD44-11D0-8CC2-00C04FC295EE}");

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = true)]
    static extern uint WinVerifyTrust(IntPtr hwnd, [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID, ref WINTRUST_DATA pWVTData);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetDefaultDllDirectories(uint DirectoryFlags);
    
    /// <summary>
    /// Returns true if the file at <paramref name="filePath"/> has a valid
    /// Authenticode signature that chains to a trusted root.
    /// </summary>
    private static bool IsTrusted(string filePath)
    {
        var fileInfo = new WINTRUST_FILE_INFO(filePath);
        var wvtData = new WINTRUST_DATA(fileInfo);
        uint result = WinVerifyTrust(IntPtr.Zero, WINTRUST_ACTION_GENERIC_VERIFY_V2, ref wvtData);
        return result == 0;  // ERROR_SUCCESS == 0 means signature is valid
    }

    private const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;

    public static bool SetDefaultDllDirectories()
    {
        return SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_SYSTEM32);
    }

    public static void InitSecureLoad()
    {
        AppDomain.CurrentDomain.AssemblyLoad += (a_o, a_args) =>
        {
            if (a_args.LoadedAssembly.IsDynamic)
            {
                //The CLR needs to load methods that aren't from a dll
                return;
            }

            //if (string.IsNullOrEmpty(a_args.LoadedAssembly.Location))
            //{
            //    //don't know what these are
            //    if (a_args.LoadedAssembly.Location.Contains("test"))
            //    {
            //        bool b = false;
            //    }
            //    return;
            //}


            //string loadedPath = Path.GetDirectoryName(a_args.LoadedAssembly.Location);
            //if (loadedPath == a_currentPath)
            //{
            //    //This is loaded from the root directory, so allow.
            //    return;
            //}

            bool isTrusted = IsTrusted(a_args.LoadedAssembly.Location);
            if (!isTrusted)
            {
                //File.AppendAllText("AssemblyLoadErrors.log", $"{DateTime.UtcNow:O} ERROR: unsigned managed assembly {a_args.LoadedAssembly.FullName} @ {a_args.LoadedAssembly.Location}{Environment.NewLine}");
                Environment.FailFast($"Untrusted assembly {a_args.LoadedAssembly.FullName} cannot be loaded");
            }
        };

        //AppDomain.CurrentDomain.AssemblyResolve += (s, args) =>
        //{
        //    if (args.Name.Contains("Sentry"))
        //    {
        //        bool b = false;
        //        //System.Diagnostics.Debugger.Break();  // breaks into VS on *any* resolve attempt
        //    }
        //    return null;
        //};
    }

    #region PInvoke Structures & Enums

    // Which UI to display (we don’t want any UI in a service/console app)
    private enum WinTrustDataUIChoice : uint
    {
        All = 1,  // WTD_UI_ALL
        None = 2,  // WTD_UI_NONE
        NoBad = 3,  // WTD_UI_NOBAD
        NoGood = 4,  // WTD_UI_NOGOOD
    }

    // Revocation checking flags
    [Flags]
    private enum WinTrustDataRevocationChecks : uint
    {
        None = 0x00000000, // WTD_REVOKE_NONE
        WholeChain = 0x00000001, // WTD_REVOKE_WHOLECHAIN
    }

    // Which structure is in the union
    private enum WinTrustDataChoice : uint
    {
        File = 1, // WTD_CHOICE_FILE
        Catalog = 2,
        Blob = 3,
        Signer = 4,
        Certificate = 5
    }

    // How we process revocation in chain policy
    [Flags]
    private enum WinTrustDataStateAction : uint
    {
        Ignore = 0x00000000, // WTD_STATEACTION_IGNORE
        Verify = 0x00000001, // WTD_STATEACTION_VERIFY
        Close = 0x00000002, // WTD_STATEACTION_CLOSE
        AutoCache = 0x00000003, // WTD_STATEACTION_AUTO_CACHE
        AutoCacheFlush = 0x00000004, // WTD_STATEACTION_AUTO_CACHE_FLUSH
    }

    // WINTRUST_FILE_INFO — tells WinVerifyTrust which file to check
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WINTRUST_FILE_INFO
    {
        public uint cbStruct;       // size of this struct
        public string pcwszFilePath;  // file path to check
        public IntPtr hFile;          // optional handle (unused)
        public IntPtr pgKnownSubject; // optional: pointer to subject GUID (unused)

        public WINTRUST_FILE_INFO(string filePath)
        {
            cbStruct = (uint)Marshal.SizeOf<WINTRUST_FILE_INFO>();
            pcwszFilePath = filePath;
            hFile = IntPtr.Zero;
            pgKnownSubject = IntPtr.Zero;
        }
    }

    // WINTRUST_DATA — main struct describing how we want the check done
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WINTRUST_DATA
    {
        public uint cbStruct;             // size of this struct
        public IntPtr pPolicyCallbackData;  // optional policy callback (unused)
        public IntPtr pSIPClientData;       // optional SIP client data (unused)
        public WinTrustDataUIChoice dwUIChoice;         // UI option
        public WinTrustDataRevocationChecks fdwRevocationChecks; // revocation flags
        public WinTrustDataChoice dwUnionChoice;        // which union member to use
        public IntPtr pFile;                // pointer to WINTRUST_FILE_INFO
        public WinTrustDataStateAction dwStateAction;   // state action (ignored here)
        public IntPtr hWVTStateData;        // state data (unused)
        public string pwszURLReference;     // URL reference (unused)
        public uint dwProvFlags;          // provider flags (none)
        public uint dwUIContext;          // UI context (unused)

        public WINTRUST_DATA(WINTRUST_FILE_INFO fileInfo)
        {
            cbStruct = (uint)Marshal.SizeOf<WINTRUST_DATA>();
            pPolicyCallbackData = IntPtr.Zero;
            pSIPClientData = IntPtr.Zero;
            dwUIChoice = WinTrustDataUIChoice.None;        // no UI
            fdwRevocationChecks = WinTrustDataRevocationChecks.None; // no CRL/OCSP by default
            dwUnionChoice = WinTrustDataChoice.File;          // verify a file
            pFile = Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_FILE_INFO>());
            Marshal.StructureToPtr(fileInfo, pFile, false);
            dwStateAction = WinTrustDataStateAction.Ignore;
            hWVTStateData = IntPtr.Zero;
            pwszURLReference = null;
            dwProvFlags = 0x00000010; // WTD_SAFER_FLAG: skip safer checks
            dwUIContext = 0;
        }
    }

    #endregion
}