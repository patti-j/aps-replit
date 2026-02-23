using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.Scheduler;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.APIs;

internal class ClockAdvance
{
    private static DateTime GetCurrentAPSDateTime()
    {
        try
        {
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 5000))
            {
                using (sm.GetFirstProductionScenario().ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
                {
                    return sd.ClockDate;
                }
            }
        }
        catch
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.SuccessWithoutValidation);
        }
    }

    /// <summary>
    /// Validates DateTime is in the future but less than 5 years away. broadcasts a ScenarioClockAdvanceT
    /// </summary>
    internal static EApsWebServicesResponseCodes SendClockAdvanceT(DateTime a_dateTime, BaseId a_instigator, ApiLogger a_apiLogger, long a_scenarioId = long.MinValue)
    {
        try
        {
            DateTime advanceTime = a_dateTime.ToServerTime();
            DateTime currentAPSClock = GetCurrentAPSDateTime();
            if (advanceTime < currentAPSClock)
            {
                return EApsWebServicesResponseCodes.InvalidDatePast;
            }

            if (advanceTime > currentAPSClock + TimeSpan.FromDays(365 * 5))
            {
                return EApsWebServicesResponseCodes.InvalidDateFuture;
            }

            //Success, send transmission
            ScenarioClockAdvanceT successClockT = new (advanceTime);
            successClockT.Instigator = a_instigator;
            successClockT.SpecificScenarioId = a_scenarioId != long.MinValue ? new BaseId(a_scenarioId) : BaseId.NULL_ID;

            string broadcastArgs = $"AdvanceTime: {successClockT.time} | Instigator: {successClockT.Instigator} | ScenarioId: {successClockT.SpecificScenarioId}";
            a_apiLogger.LogBroadcast("SendClockAdvanceT", broadcastArgs);
            SystemController.ClientSession.SendClientAction(successClockT);

            return EApsWebServicesResponseCodes.Success;
        }
        catch (WebServicesErrorException e)
        {
            //Timed out when attempting to retrieve the clock value
            ScenarioClockAdvanceT clockT = new (a_dateTime);
            clockT.Instigator = a_instigator;
            clockT.SpecificScenarioId = a_scenarioId != long.MinValue ? new BaseId(a_scenarioId) : BaseId.NULL_ID;
            SystemController.ClientSession.SendClientAction(clockT);
            return e.Code;
        }
        catch
        {
            return EApsWebServicesResponseCodes.FailedToBroadcast;
        }
    }
}