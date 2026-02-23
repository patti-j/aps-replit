using System.Diagnostics;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler.PackageDefs;
using PT.Transmissions;

namespace PT.Scheduler.CoPilot.Pruning.Validators;

/// <summary>
/// This class contains the list of validation modules to use for pruning.
/// When pruning, all of the modules in this list will be checked, all must pass to be successful.
/// When creating a new validation module, add the module to the list in the constructor
/// </summary>
internal class ValidatorManager
{
    private readonly List<IPruneScenarioValidationModule> m_validatorsList;
    private readonly ScenarioBaseT m_transmission;

    internal enum EValidationResult { ErrorValidated, NewError, NoError, Undefined }

    internal ValidatorManager(byte[] a_sdBytes, byte[] a_ssBytes, ScenarioBaseT a_transmission, IPackageManager a_packageManager)
    {
        List<IPruneScenarioValidationModule> newModules = new ();
        CoPilotSimulationUtilities simUtilities = new (a_packageManager);

        m_validatorsList = new List<IPruneScenarioValidationModule>();
        m_transmission = a_transmission;

        //Add new modules here:
        //NOTE: The order in the list matters. Serialization tests should be first, followed by Timeout modules.
        //Serialization
        newModules.Add(new ValidateSuccessfulSerializationModule(a_packageManager));
        //Timeout
        newModules.Add(new ValidateTimeoutModule(a_transmission));
        //Transmission
        newModules.Add(new ValidateTransmissionModule(a_transmission));
        //Transmission
        newModules.Add(new ValidateKpiModule(a_transmission));
        //FailedToSchedule
        //newModules.Add(new ValidateFailedToScheduleModule());

        for (int i = 0; i < newModules.Count; i++)
        {
            Scenario testScenario = simUtilities.CopySimScenario(a_sdBytes, a_ssBytes);
            if (newModules[i].Initialize(testScenario))
            {
                m_validatorsList.Add(newModules[i]);
            }

            testScenario.Dispose();
        }

        //Note: The SuccessfulSerializationModule does not provide a result for an existing error so at least 2 modules are needed.
        if (m_validatorsList.Count <= 1)
        {
            throw new PTValidationException("No valid validator modules will work with this scenario".Localize());
            Debugger.Break();
        }
    }

    internal EValidationResult Validate(Scenario a_scenario)
    {
        for (int i = 0; i < m_validatorsList.Count; i++)
        {
            EValidationResult result = m_validatorsList[i].Validate(a_scenario);
            if (result != EValidationResult.ErrorValidated)
            {
                return result;
            }
        }

        return EValidationResult.ErrorValidated;
    }

    internal ScenarioBaseT GetValidationTransmission()
    {
        return m_transmission;
    }
}