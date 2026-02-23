using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;

namespace PT.PlanetTogetherAPI.APIs;

internal static class Extensions
{
    internal static void ValidateScenarioId(this ApsWebServiceScenarioRequest a_request)
    {
        if (a_request.ScenarioId < 0 || a_request.ScenarioId == BaseId.NULL_ID.Value)
        {
            throw new WebServicesErrorException(EApsWebServicesResponseCodes.InvalidScenarioId);
        }
    }
}