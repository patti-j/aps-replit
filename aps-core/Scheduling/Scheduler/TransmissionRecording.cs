using PT.Transmissions;

namespace PT.Scheduler;

public class TransmissionRecording : IPTSerializable
{
    #region IPTSerializable Members
    public TransmissionRecording(IReader reader, IClassFactory classFactory)
    {
        reader.Read(out recordingTime);
        recordedTransmission = (Transmission)classFactory.Deserialize(reader);
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(recordingTime);

        writer.Write(recordedTransmission.UniqueId);
        recordedTransmission.Serialize(writer);
    }

    public const int UNIQUE_ID = 425;

    public int UniqueId => UNIQUE_ID;
    #endregion

    private readonly DateTime recordingTime;
    private readonly Transmission recordedTransmission;

    public TransmissionRecording(Transmission t)
    {
        recordingTime = t.TimeStamp.ToDateTime();
        recordedTransmission = t;
    }

    public DateTime Date => recordingTime;

    public Transmission Transmission => recordedTransmission;
}