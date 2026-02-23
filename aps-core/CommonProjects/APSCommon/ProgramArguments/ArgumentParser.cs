using System.IO;

using PT.Common.Extensions;

namespace PT.APSCommon.ProgramArguments;

public class ArgumentParser
{
    public static readonly char StartOfArgumentChar = '\\';
    public static readonly char ValueSeparator = ':';

    /// <summary>
    /// Set the program Arguments that were passed to the program as arguments.
    /// All arguments must be specified using the following format: \{ArgumentName}:{RequiredOrOptionalValue}
    /// Duplicate ArgumentNames aren't allowed.
    /// An PTArgumentException will be thrown if there's a problem with the program arguments.
    /// </summary>
    /// <param name="a_args">The arguments passed to the program.</param>
    /// <param name="a_argsStartsWithProgramPath">Whether the program arguments start with the program path.</param>
    /// <param name="a_possibleArgs">An array of the possible arguments. If the Argument was passed in, its flag is set and it's value is set if it was passed in to.</param>
    public static void Parse(string[] a_args, bool a_argsStartsWithProgramPath, params Argument[] a_possibleArgs)
    {
        for (int argI = a_argsStartsWithProgramPath ? 1 : 0; argI < a_args.Length; ++argI)
        {
            bool found = false;

            for (int possibleArgI = 0; possibleArgI < a_possibleArgs.Length; ++possibleArgI)
            {
                if (ParseArgument(a_args[argI], argI, a_possibleArgs[possibleArgI]))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                string msg = string.Format("Program argument index #{0} is an unrecognized argument. The argument was: {1}", argI, a_args[argI]);
                throw new ArgumentUnknownException(msg);
            }
        }
    }

    /// <summary>
    /// Set the program Arguments that were passed to the program as arguments.
    /// All arguments must be specified using the following format: \{ArgumentName}:{RequiredOrOptionalValue}
    /// Duplicate ArgumentNames aren't allowed.
    /// An PTArgumentException will be thrown if there's a problem with the program arguments.
    /// </summary>
    /// <param name="a_args">The arguments passed to the program.</param>
    /// <param name="a_argsStartsWithProgramPath">Whether the program arguments start with the program path.</param>
    /// <param name="a_possibleArgs">An array of the possible arguments. If the Argument was passed in, its flag is set and it's value is set if it was passed in to.</param>
    public List<PTArgumentException> ParseAndGatherErrors(string[] a_args, bool a_argsStartsWithProgramPath, params Argument[] a_possibleArgs)
    {
        List<PTArgumentException> errors = new ();
        for (int argI = a_argsStartsWithProgramPath ? 1 : 0; argI < a_args.Length; ++argI)
        {
            bool found = false;
            try
            {
                for (int possibleArgI = 0; possibleArgI < a_possibleArgs.Length; ++possibleArgI)
                {
                    if (ParseArgument(a_args[argI], argI, a_possibleArgs[possibleArgI]))
                    {
                        found = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                string msg = string.Format("There was an error parsing program argument index #{0}. The argument was: {1}", argI, a_args[argI]);
                errors.Add(new ArgumentUnknownException(msg));
                continue;
            }


            if (!found)
            {
                string msg = string.Format("Program argument index #{0} is an unrecognized argument. The argument was: {1}", argI, a_args[argI]);
                errors.Add(new ArgumentUnknownException(msg));
            }
        }

        return errors;
    }

    /// <summary>
    /// Whether the program argument matches the Argument it's being compared to and it's value, if one's specified.
    /// </summary>
    /// <param name="a_arg">A program argument value.</param>
    /// <param name="a_argIdx">The index of the program argument. This is used to indicate the location when an exception is thrown.</param>
    /// <param name="a_possibleArgument">The Argument the program argument is being compared to.</param>
    /// <returns>true if the program argument's name matches the Argument passed in.</returns>
    private static bool ParseArgument(string a_arg, int a_argIdx, Argument a_possibleArgument)
    {
        if (string.IsNullOrEmpty(a_arg))
        {
            string msg = string.Format("Program argument index #{0} was empty.", a_argIdx);
            throw new PTArgumentException(msg);
        }

        if (a_arg[0] != StartOfArgumentChar)
        {
            string msg = string.Format("Program argument index #{0} didn't start with a single backslash character. The argument was: {1}",
                a_argIdx,
                a_arg);
            throw new PTArgumentException(msg);
        }

        if (!a_arg.Contains(ValueSeparator))
        {
            string msg = string.Format("Program argument index #{0}'s name didn't end with a colon. The argument was: {1}", a_argIdx, a_arg);
            throw new PTArgumentException(msg);
        }

        char[] splitChars = { StartOfArgumentChar, ValueSeparator };
        string[] splitArg = a_arg.Split(splitChars);

        if (splitArg.Length < 3 || splitArg[1].Length == 0)
        {
            string msg =
                string.Format(
                    "Program argument index #{0} wasn't formatted correctly. The format should be: {1}\"ProgramArgumentName\"{2}\"AnyNecessaryValue\". The argument was: {3}",
                    a_argIdx,
                    StartOfArgumentChar,
                    ValueSeparator,
                    a_arg);
            throw new PTArgumentException(msg);
        }

        // Index 0 should be blank since it's a member of the split characters; a backslash (\).
        // Index 1 should have the argument name in it.
        // Index 2 should be blank since it's a member of the split characters; a colon (").
        // Index 3 might be set to something if a value can be supplied with the program argument.
        const int indexOfName = 1;
        string argName = splitArg[indexOfName];

        int indexOfValueSeparator = a_arg.IndexOf(ValueSeparator) + 1;
        string value = a_arg.Substring(indexOfValueSeparator);

        if (a_possibleArgument.ArgumentMatch(argName))
        {
            if (a_possibleArgument.ArgumentFound)
            {
                string msg = string.Format("Program argument index #{0} was specified more than once. The argument was: {1}", a_argIdx, a_arg);
                throw new PTArgumentException(msg);
            }

            a_possibleArgument.ArgumentFound = true;

            if (a_possibleArgument.ValueRequirement == EValueAfterNameRequirement.NoValue && value.Length > 0)
            {
                string msg = string.Format("Program argument index #{0} doesn't accept a value. The argument was: {1}", a_argIdx, a_arg);
                throw new PTArgumentException(msg);
            }

            if (a_possibleArgument.ValueRequirement == EValueAfterNameRequirement.Required && value.Length == 0)
            {
                string msg = string.Format("Program argument index #{0} requires a value. The argument was: {1}", a_argIdx, a_arg);

                if (a_possibleArgument.Name == "LoadDevPackages")
                {
                    msg = string.Format("Program argument index #{0} requires a value with a non-empty path for Dev Packages. The argument was: {1}", a_argIdx, a_arg);
                }

                throw new PTArgumentException(msg);
            }

            if (value.Length > 0)
            {
                a_possibleArgument.Value = value;

                if (a_possibleArgument.Name == "ServerURI")
                {
                    RemoveServerUriServerManagerPath(a_possibleArgument);

                    //test parsing
                    try
                    {
                        Uri testUri = new (a_possibleArgument.Value);
                        // ReSharper disable UnusedVariable
                        //These variables are set to test access to testUri values
                        string test1 = testUri.Host;
                        int test2 = testUri.Port;
                        // ReSharper restore UnusedVariable
                    }
                    catch
                    {
                        throw new PTArgumentException("ServerURI has an incorrect format. Should be like: http://localhost:7990");
                    }
                }

                if (a_possibleArgument.Name == "LoadDevPackages")
                {
                    if (!ValidDevPackagesPath(value))
                    {
                        string msg = string.Format("Program argument index #{0} requires a value with a valid path for Dev Packages - No package assemblies found. The argument was: {1}", a_argIdx, a_arg);
                        throw new PTArgumentException(msg);
                    }
                }
            }

            return true;
        }

        return false;
    }

    private static bool ValidDevPackagesPath(string a_devPackagesPath)
    {
        if (Directory.Exists(a_devPackagesPath))
        {
            string[] dllFilesThatStartWithPT = Directory.GetFiles(a_devPackagesPath, "PT.*Package.dll", SearchOption.AllDirectories);
            string[] dllFilesThatStartWithPlanetTogether = Directory.GetFiles(a_devPackagesPath, "PlanetTogether.*Package.dll", SearchOption.AllDirectories)
                                          .Where(fileName => !fileName.Contains("\\obj\\"))
                                          .ToArray();
            if (dllFilesThatStartWithPlanetTogether.Length > 0)
            {
                dllFilesThatStartWithPT = dllFilesThatStartWithPT.Concat(dllFilesThatStartWithPlanetTogether).ToArray();
            }

            if (dllFilesThatStartWithPT.Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    // v11 client manager used to append '/ServerManager' at the end. Remove this
    private static void RemoveServerUriServerManagerPath(Argument a_possibleArgument)
    {
        const string serverManagerPath = "/ServerManager";
        if (a_possibleArgument.Value.EndsWith(serverManagerPath))
        {
            int lengthBeforeServerManagerPathStarts = a_possibleArgument.Value.LastIndexOf(serverManagerPath);
            a_possibleArgument.Value = a_possibleArgument.Value.Substring(0, lengthBeforeServerManagerPathStarts);
        }
    }

    public static string GetArgumentString(params Argument[] a_args)
    {
        System.Text.StringBuilder builder = new ();
        for (int i = 0; i < a_args.Length; i++)
        {
            Argument arg = a_args[i];
            if (arg.ArgumentFound)
            {
                if (builder.Length > 0)
                {
                    builder.Append(" "); //space separated
                }

                builder.Append(string.Format("{0}{1}{2}", StartOfArgumentChar, arg.Name, ValueSeparator));
                if (arg.ValueRequirement == EValueAfterNameRequirement.Required ||
                    (arg.ValueRequirement == EValueAfterNameRequirement.Optional && !string.IsNullOrEmpty(arg.Value)))
                {
                    builder.Append(arg.Value.Quotation());
                }
            }
        }

        return builder.ToString();
    }
}

//// Test
//class Program
//{
//    static void Main(string[] args)
//    {
//        Argument instanceName = new Argument("InstanceName", ValueAfterNameRequirementEnum.Required);
//        Argument softwareVersion = new Argument("SoftwareVersion", ValueAfterNameRequirementEnum.Required);
//        Argument userName = new Argument("UserName", ValueAfterNameRequirementEnum.Required);
//        Argument password = new Argument("Password", ValueAfterNameRequirementEnum.Optional);
//        Argument skipLoginScreen = new Argument("SkipLoginScreen", ValueAfterNameRequirementEnum.NoValue);
//        Argument createInternalInstanceOfServer = new Argument("CreateInternalInstanceOfServer", ValueAfterNameRequirementEnum.NoValue);
//        Argument skipClientUpdater = new Argument("skipClientUpdater", ValueAfterNameRequirementEnum.NoValue);

//        ArgumentParser.Parse(args, instanceName, softwareVersion, userName, password, skipLoginScreen, createInternalInstanceOfServer, skipClientUpdater);
//}
//}