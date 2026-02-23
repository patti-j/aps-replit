using PT.PackageDefinitionsUI;
using PT.Transmissions;
using PT.Transmissions.Interfaces;
using PT.UIDefinitions;

namespace PT.UI.Dialogs.CommandWindow;

internal partial class APSUICommandWindow : APSCommandWindow
{
    private readonly IClientSession m_session;
    private static IScenarioInfo s_scenarioInfo;

    internal static void Initialize(IScenarioInfo a_scenarioInfo)
    {
        s_scenarioInfo = a_scenarioInfo;
    }

    internal APSUICommandWindow(string a_command, IClientSession a_session) : base(a_command, true, a_session)
    {
        m_session = a_session;
        InitializeComponent();
        SendEvent += APSUICommandWindow_SendEvent;
    }

    private void APSUICommandWindow_SendEvent(PTTransmission a_t)
    {
        using (new MultiLevelHourglass())
        {
            m_session.SendClientAction(a_t);
        }
    }

    /// <summary>
    /// Override AddOnCommands in LightWeightUI's APSCommandWindow to add support for new UI related commands.
    /// </summary>
    public override bool AddOnCommands(string a_command)
    {
        if (string.Compare(a_command, "JitCompress", true) == 0)
        {
            ScenarioDetailJitCompressT t = new (s_scenarioInfo.ScenarioId);
            Send(t);
            return true;
        }

        return false;
    }

    // help for commands added through AddOnCommands method.
    public override string Help
    {
        get
        {
            string help = @"
***OPTIMIZE***
JitCompress:     Slide activities closer to their due date without 
                    making adjacent activities late.";
            return base.Help + help;
        }
    }
}