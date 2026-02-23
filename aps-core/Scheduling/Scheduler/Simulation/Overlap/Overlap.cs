namespace PT.Scheduler;

/// <summary>
/// This is for debugging purposes.
/// You can get rid of it once several customers are live on operation overlap.
/// </summary>
internal class Overlap
{
    internal static void Throw_OverlapDebugError(Exception a_e)
    {
        if (a_e is ScenarioDetail.SimulationValidationException)
        {
            throw a_e;
        }
        ////This is just a temporary class. Once overlap is fully proven at a customer site you can get rid of this test code and its usages.
        //string errMsg = string.Format("ERROR - Possible overlap cycle completion calculations problems. Exception Msg={0}", e.Message);
        //throw new Exception(errMsg);
    }
}