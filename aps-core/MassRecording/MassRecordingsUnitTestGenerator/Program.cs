using PT.APSCommon.ProgramArguments;

namespace MassRecordingUnitTestGenerator;

internal class Program
{
    public static CommandLineArgumentsHelper CmdLineHelp;

    [STAThread]
    private static void Main(string[] args)
    {
        CmdLineHelp = new CommandLineArgumentsHelper(args);

        UnitTestCodeGenerator testGenerator = new (CmdLineHelp);
        try
        {
            testGenerator.GenerateTestCode();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.ReadLine();
        }
    }
}

/// <summary>
/// Takes command line argument to pick the local HostName. This allows the Automatic Build to use the Unit Test
/// generator to generate test methods using the local run instances to calculate average recording run speeds
/// </summary>
public class CommandLineArgumentsHelper
{
    internal CommandLineArgumentsHelper(string[] args)
    {
        InitArguments();
        ArgumentParser.Parse(args, false, LocalHostName, TestFileLocation, ClosePrompt);
    }

    private void InitArguments()
    {
        LocalHostName = new Argument("LocalHostName", EValueAfterNameRequirement.NoValue);
        TestFileLocation = new Argument("TestFileLocation", EValueAfterNameRequirement.Required);
        ClosePrompt = new Argument("ClosePrompt", EValueAfterNameRequirement.NoValue);
    }

    internal Argument LocalHostName { get; private set; }
    internal Argument TestFileLocation { get; private set; }
    internal Argument ClosePrompt { get; private set; }

    internal string CreateArgumentString()
    {
        return ArgumentParser.GetArgumentString(LocalHostName, TestFileLocation, ClosePrompt);
    }
}