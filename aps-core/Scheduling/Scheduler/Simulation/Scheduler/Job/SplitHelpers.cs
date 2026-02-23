namespace PT.Scheduler.Simulation.Scheduler.Job;

internal class SplitHelpers
{
    internal static void MOSplittableValidation(ManufacturingOrder a_mO)
    {
        if (a_mO.Finished)
        {
            throw new ScenarioDetail.SimulationValidationException("2283");
        }
    }

    public const string SPLIT_MO_SEPERATOR_CHARS = "\\#";
    internal const int SEPERATOR_CHARS_LEN = 2;

    public static bool GetSourceMOExternalId(ManufacturingOrder a_mo, out string a_sourceMOExternalId, out int a_splitNbr)
    {
        a_sourceMOExternalId = "";
        a_splitNbr = -1;

        string externalId = a_mo.ExternalId;
        int sepIdx = externalId.IndexOf(SPLIT_MO_SEPERATOR_CHARS);
        if (sepIdx <= 0)
        {
            return false;
        }

        int firstIdx = sepIdx + SEPERATOR_CHARS_LEN;
        string postFix = externalId.Substring(firstIdx);

        try
        {
            a_splitNbr = int.Parse(postFix);
            a_sourceMOExternalId = externalId.Substring(0, sepIdx);
        }
        catch
        {
            return false;
        }

        return true;
    }
}