namespace PT.Transmissions;

public class DataActivationWarningT : PTTransmission, IPTSerializable
{
    public const int UNIQUE_ID = 856;

    public override int UniqueId => UNIQUE_ID;

    public DataActivationWarningT()
    {
    }

    public DataActivationWarningT(IReader a_reader) : base(a_reader)
    {
    }
}