using PT.Scheduler.PackageDefs;

namespace PT.Scheduler.CoPilot.Pruning.Validators;

/// <summary>
/// This validation module verifies if the scenario can be serialized. A good result is no error.
/// </summary>
internal class ValidateSuccessfulSerializationModule : IPruneScenarioValidationModule
{
    private readonly CoPilotSimulationUtilities m_simUtilities;

    internal ValidateSuccessfulSerializationModule(IPackageManager a_packageManager)
    {
        m_simUtilities = new CoPilotSimulationUtilities(a_packageManager);
    }

    public bool Initialize(Scenario a_scenario)
    {
        return true;
    }

    public ValidatorManager.EValidationResult Validate(Scenario a_scenario)
    {
        try
        {
            m_simUtilities.CopyAndStoreScenario(a_scenario, out byte[] sdByteArray, out byte[] ssByteArray);
            Scenario testScenario = m_simUtilities.CopySimScenario(sdByteArray, ssByteArray);
            testScenario.Dispose();
            return ValidatorManager.EValidationResult.ErrorValidated;
        }
        catch (Exception)
        {
            return ValidatorManager.EValidationResult.NewError;
        }
    }
}