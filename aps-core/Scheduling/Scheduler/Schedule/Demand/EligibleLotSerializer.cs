namespace PT.Scheduler.Schedule.Demand;

internal class EligibleLotSerializer : IReaderDictionaryKeyValueSerializer
{
    public object ReadKey(IReader a_reader)
    {
        string s;
        a_reader.Read(out s);
        return s;
    }

    public object ReadValue(IReader a_reader)
    {
        EligibleLot el = new (a_reader);

        return el;
    }

    public void WriteKey(IWriter a_writer, object a_key)
    {
        string s = (string)a_key;
        a_writer.Write(s);
    }

    public void WriteValue(IWriter a_writer, object a_value)
    {
        EligibleLot el = (EligibleLot)a_value;
        el.Serialize(a_writer);
    }
}