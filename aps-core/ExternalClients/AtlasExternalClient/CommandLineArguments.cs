using PT.APSCommon.ProgramArguments;

namespace AtlasExternalClient
{
    public class CommandLineArguments
    {
        internal CommandLineArguments(string[] args)
        {
            InitArguments();
            ArgumentParser.Parse(args, false, ServerAddress, ServerPort, Instance, SoftwareVersion, UserName, Password, Trace, Log, TimeoutMinutes);
        }

        /// <summary>
        /// ServerAddress (SA) | Required: Computer name or IP to server
        /// ServerPort (SP) | Optional: Server Manager Port. Defaults to 7990
        /// Instance | Required: Instance Name
        /// Version |  Required: Software Version
        /// UserName | (UN) Required: APS User Name
        /// Password | (PW) Optional: APS User password. Defaults to no password
        /// Trace | Optional: If present will write to console output.
        /// Log | Optional: Full file path to write Trace output. Defaults to working directory
        /// </summary>
        private void InitArguments()
        {
            ServerAddress = new Argument("ServerAddress", EValueAfterNameRequirement.Required, "SA");
            ServerPort = new Argument("ServerPort", EValueAfterNameRequirement.Optional, "SP");
            Instance = new Argument("Instance", EValueAfterNameRequirement.Required);
            SoftwareVersion = new Argument("Version", EValueAfterNameRequirement.Required);
            UserName = new Argument("UserName", EValueAfterNameRequirement.Required, "UN");
            Password = new Argument("Password", EValueAfterNameRequirement.Optional, "PW");
            Trace = new Argument("Trace", EValueAfterNameRequirement.NoValue);
            Log = new Argument("Log", EValueAfterNameRequirement.Optional);
            TimeoutMinutes = new Argument("Timeout", EValueAfterNameRequirement.Optional);
        }

        internal Argument ServerAddress { get; private set; }
        internal Argument ServerPort { get; private set; }
        internal Argument Instance { get; private set; }
        internal Argument SoftwareVersion { get; private set; }
        internal Argument UserName { get; private set; }
        internal Argument Password { get; private set; }
        internal Argument Trace { get; private set; }
        internal Argument Log { get; private set; }
        internal Argument TimeoutMinutes { get; private set; }

        internal string CreateArgumentString()
        {
            return ArgumentParser.GetArgumentString(ServerAddress, ServerPort, Instance, SoftwareVersion, UserName, Password, Trace, Log, TimeoutMinutes);
        }
    }
}
