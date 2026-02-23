namespace PT.Scheduler.CoPilot.Pruning.Validators;

internal interface IPruneScenarioValidationModule
{
    /// <summary>
    /// This validation returns whether the specified problem still exists.
    /// </summary>
    ValidatorManager.EValidationResult Validate(Scenario a_scenario);

    /// <summary>
    /// Initialize the validator. Return whether this validator is relavent
    /// </summary>
    /// <param name="a_scenario"></param>
    /// <returns></returns>
    bool Initialize(Scenario a_scenario);
}