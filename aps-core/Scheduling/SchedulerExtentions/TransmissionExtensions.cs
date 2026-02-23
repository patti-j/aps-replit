using PT.Scheduler;

namespace PT.Transmissions.Extensions;

public static class TransmissionExtensions
{
    public static bool SentByActiveUser(this PTTransmission a_transmission)
    {
        return a_transmission.Instigator == SystemController.CurrentUserId;
    }
}