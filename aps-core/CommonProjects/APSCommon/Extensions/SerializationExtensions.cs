using System.Reflection;

namespace PT.APSCommon.Extensions;

public static class SerializationExtensions
{
    /// <summary>
    /// Creates a copy of an IPTSerializable object in memory
    /// Returns null if the IPTSerializable class does not have a Constructor with a single IReader parameter
    /// </summary>
    public static T CopyInMemory<T>(this T a_original, BaseIdGenerator a_idGen) where T : class, IPTDeserializable
    {
        ConstructorInfo constructor = typeof(T).GetConstructor(new[] { typeof(IReader), typeof(BaseIdGenerator) });
        if (constructor == null)
        {
            return null;
        }

        byte[] objBytes;
        using (BinaryMemoryWriter writer = new ())
        {
            a_original.Serialize(writer);
            objBytes = writer.GetBuffer();
        }

        using (BinaryMemoryReader reader = new (objBytes))
        {
            return (T)constructor.Invoke(new object[] { reader, a_idGen });
        }
    }

    /// <summary>
    /// Creates a copy of an IPTSerializable object in memory
    /// Invokes the provided delegate function
    /// </summary>
    public static T CopyInMemory<T>(this T a_original, BaseIdGenerator a_idGen, Func<IReader, BaseIdGenerator, T> a_constructor) where T : class, IPTDeserializableIdGen
    {
        byte[] objBytes;
        using (BinaryMemoryWriter writer = new())
        {
            a_original.Serialize(writer);
            objBytes = writer.GetBuffer();
        }

        using (BinaryMemoryReader reader = new(objBytes))
        {
            return a_constructor.Invoke(reader, a_idGen);
        }
    }
}