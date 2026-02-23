using PT.Transmissions;

namespace PT.Scheduler.CoPilot.Pruning.Validators;

/// <summary>
/// This module tests whether a simulation will complete.
/// When validating it will send the specified transmission and wait a specific amount of time.
/// If the transmission runs within the alotted time, the validation has failed because the issue is fixed.
/// </summary>
internal class ValidateTimeoutModule : IPruneScenarioValidationModule
{
    private readonly ScenarioBaseT m_transmission;
    private static readonly TimeSpan s_timeoutSpan = TimeSpan.FromMinutes(3);
    private CancellationTokenSource m_tokenSource;
    private readonly CancellationTokenSource m_neverCancelledTokenSource = new ();

    internal ValidateTimeoutModule(ScenarioBaseT a_transmission)
    {
        m_transmission = a_transmission;
    }

    private void TestTimeout(Scenario a_scenario)
    {
        a_scenario.Receive(m_transmission);
    }

    public bool Initialize(Scenario a_scenario)
    {
        try
        {
            m_tokenSource = new CancellationTokenSource();
            PTSystem.CancelToken = m_tokenSource.Token;
            Task task = Task.Run(() => TestTimeout(a_scenario), m_tokenSource.Token);

            if (!task.Wait(s_timeoutSpan))
            {
                try
                {
                    m_tokenSource.Cancel();
                    task.Wait();
                }
                catch (Exception) { }

                return true;
            }
        }
        catch { }

        return false;
    }

    public ValidatorManager.EValidationResult Validate(Scenario a_scenario)
    {
        try
        {
            m_tokenSource = new CancellationTokenSource();
            PTSystem.CancelToken = m_tokenSource.Token;
            Task task = Task.Run(() => TestTimeout(a_scenario), m_tokenSource.Token);

            if (!task.Wait(s_timeoutSpan))
            {
                try
                {
                    m_tokenSource.Cancel();
                    task.Wait();
                }
                catch (Exception) { }

                PTSystem.CancelToken = m_neverCancelledTokenSource.Token;
                return ValidatorManager.EValidationResult.ErrorValidated;
            }

            PTSystem.CancelToken = m_neverCancelledTokenSource.Token;
            return ValidatorManager.EValidationResult.NoError;
        }
        catch
        {
            PTSystem.CancelToken = m_neverCancelledTokenSource.Token;
            return ValidatorManager.EValidationResult.NewError;
        }
    }
}