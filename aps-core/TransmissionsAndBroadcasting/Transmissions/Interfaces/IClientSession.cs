using PT.APSCommon;
using PT.Common.Http;

namespace PT.Transmissions.Interfaces;

public interface IClientSession
{
    string SessionToken { get; }

    //Send a transmission to the server
    bool SendClientAction(PTTransmission a_t, bool a_registerAwait = false);
    bool SendClientAction(PTTransmission a_t, BaseId a_instigatorOverride, bool a_registerAwait = false);

    /// <summary>
    /// Sends all provided transmissions as a single packet. They will be processed in sequence.
    /// </summary>
    /// <param name="a_transmissions"></param>
    /// <returns>The TransmissionId of the packetT in case it needs to be awaited</returns>
    Guid SendClientActionsPacket(IEnumerable<PTTransmission> a_transmissions);

    Guid SendClientActionsPacket(params PTTransmission[] a_transmissions);

    /// <summary>
    /// Awaits the processing of a transmission with the provided transmission number.
    /// If the transmission was not registered for await, this will return immediately.
    /// </summary>
    /// <param name="a_senderLastTransmissionNbr"></param>
    /// <returns></returns>
    public Task WaitForTransmissionReceive(Guid a_senderLastTransmissionNbr);

    T MakePostRequest<T>(string a_endpoint, object a_content, string a_route) where T : class;
    T MakeGetRequest<T>(string a_endpointName, string a_route = null, params GetParam[] a_paramList) where T : class;
}