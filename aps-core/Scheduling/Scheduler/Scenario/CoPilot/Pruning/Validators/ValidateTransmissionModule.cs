using PT.Transmissions;

namespace PT.Scheduler.CoPilot.Pruning.Validators;

internal class ValidateTransmissionModule : IPruneScenarioValidationModule
{
    private readonly ScenarioBaseT m_transmission;
    private Exception m_exception;

    internal ValidateTransmissionModule(ScenarioBaseT a_transmission)
    {
        m_transmission = a_transmission;
    }

    public bool Initialize(Scenario a_scenario)
    {
        try
        {
            a_scenario.Receive(m_transmission);
        }
        catch (Exception e)
        {
            m_exception = e;
            return true;
        }

        return false;
    }

    public ValidatorManager.EValidationResult Validate(Scenario a_scenario)
    {
        try
        {
            a_scenario.Receive(m_transmission);
        }
        catch (Exception e)
        {
            if (e.GetType().FullName == m_exception.GetType().FullName)
            {
                if (e.Message.StartsWith("ERROR ") && m_exception.Message.StartsWith("ERROR "))
                {
                    //They both have a PT error code
                    if (e.Message.Substring(0, 12) == m_exception.Message.Substring(0, 12))
                    {
                        //Same error
                        if (m_exception.InnerException != null)
                        {
                            if (e.InnerException == null)
                            {
                                return ValidatorManager.EValidationResult.NewError;
                            }

                            //Both have an inner exception
                            if (m_exception.InnerException.Message == e.InnerException.Message)
                            {
                                return ValidatorManager.EValidationResult.ErrorValidated;
                            }
                        }
                    }
                }
                else
                {
                    //This might be the best we can do. Possibly check stack trace, but that might not be consistent. Assume this is the same error if the messages match.
                    if (e.Message == m_exception.Message)
                    {
                        return ValidatorManager.EValidationResult.ErrorValidated;
                    }
                }
            }

            return ValidatorManager.EValidationResult.NewError;
        }

        return ValidatorManager.EValidationResult.NoError;
    }
}