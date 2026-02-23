using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// A class that can create other object from a serialization stream. Currently it only supports createion of BaseIds.
/// The class will be built up on an as needed basis.
/// </summary>
public class BaseIdClassFactory : IReaderClassFactory
{
    public object Read(IReader a_reader)
    {
        return new BaseId(a_reader);
    }

    public void ReadThis(IReader a_reader) { }
}