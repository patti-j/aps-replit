using PT.Common.Exceptions;

namespace PT.Scheduler.Simulation.Customizations;

internal class CustomizationExceptionHelpers
{
    internal static void THROW_RequiredCapacityCutomization_EXCEPTION(Exception a_e, InternalOperation a_operation)
    {
        const string errMsgBase = "An exception occurred while attempting to run RequiredCapacityCustomization '{0}'";

        string errMsgBase2 = string.Format(errMsgBase, a_operation.ExternalId);
        string errMsg = string.Format(errMsgBase2 + " {0}", a_e.Message);
        throw new PTException(errMsg);
    }
}