using System.Text.RegularExpressions;

using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.Utils
{
    public static class Utils
    {
        public const string c_ServerAgentServiceName = "PlanetTogether Server Agent";

        private const int SERIALCODE_LENGTH = 10;
        public static string FormatSerialCodeAddSeparators(string a_serialCode)
        {
            if (a_serialCode != null && !a_serialCode.Contains("-") && a_serialCode.Length > SERIALCODE_LENGTH)
            {
                a_serialCode = a_serialCode.Insert(4, "-");
                a_serialCode = a_serialCode.Insert(9, "-");

                return a_serialCode;
            }
            else
            {
                return a_serialCode;
            }
        }

        public static string FormatSerialCodeRemoveSeparators(string a_serialCode)
        {
            if (a_serialCode != null)
            {
                return a_serialCode.Replace("-", "").Replace(" ", "");
            }
            else
            {
                return null;
            }
        }
        public static string[] GetIntegrationCodes()
        {
            string integrationFilesFolder = Paths.GetIntegrationFilesFolder();
            if (Directory.Exists(integrationFilesFolder))
            {
                return Directory.GetDirectories(integrationFilesFolder).Select(Path.GetFileName).ToList().ToArray();
            }

            return new List<string>().ToArray();
        }
        public static bool DirExists(string sDirName)
        {
            try
            {
                return (System.IO.Directory.Exists(sDirName));
            }
            catch (Exception)
            {
                return (false);
            }
        }
        public static string GetSystemServiceURL(string a_computerNameOrIp, int a_port)
        {
                return $"https://{a_computerNameOrIp}:{a_port}/";
        }

        public static bool VerifyDotnetVersion(string version)
        {
            string o = ServiceInstaller.RunCommand("dotnet", "--list-runtimes").Replace(Environment.NewLine,"");

            string[] rows = o.Split("]");
            List<PackageDotNet> packages = new List<PackageDotNet>();
            foreach (string row in rows)
            {
                if(row == string.Empty)
                {
                    continue;
                }
                string packageFullName = row.Split('[')[0];
                string packageName = packageFullName.Split(' ')[0];
                string packageVersion = packageFullName.Split(' ')[1];
                packages.Add(new PackageDotNet(packageName, packageVersion));
            }
            Version minimumAspNetCore = null;
            Version minimumNETCore = null;
            foreach (PackageDotNet package in packages)
            {
                if(package.Name == "Microsoft.AspNetCore.App")
                {
                    if(minimumAspNetCore == null || minimumAspNetCore < package.Version)
                        minimumAspNetCore = package.Version;
                }
                if (package.Name == "Microsoft.NETCore.App")
                {
                    if (minimumNETCore == null || minimumNETCore < package.Version)
                        minimumNETCore = package.Version;
                }
            }
            return minimumAspNetCore > new Version(version) && minimumNETCore > new Version(version);
        }

        private const string c_serverUriFormat = "https://{0}:7982";
        private const string c_urlRegexPattern = "https?:\\/\\/(?:w{1,3}\\.)?[^\\s.]+(?:\\.[a-z]+)*(?::\\d+)(?![^<]*(?:<\\/\\w+>|\\/?>))";

        public static bool FormatConnectionString(string a_server, out string o_formattedServer)
        {
            o_formattedServer = a_server;
            if (string.IsNullOrWhiteSpace(a_server))
            {
                return false;
            }

            //Check if the input string is not already in uri format, if so, format it.
            if (!Regex.IsMatch(a_server, c_urlRegexPattern))
            {
                o_formattedServer = string.Format(c_serverUriFormat, a_server);
            }

            return true;
        }

        public static string GetServerNameFromFormattedServerUri(string a_server)
        {
            return Regex.Replace(a_server, c_urlRegexPattern, "");
        }

        class PackageDotNet
        {
            public PackageDotNet(string v1, string v2)
            {
                this.Name = v1;
                this.Version = new Version(v2);
            }

            public string Name { get; set; }
            public Version Version { get; set; }
        }
    }
}