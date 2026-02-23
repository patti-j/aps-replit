using System.Reflection;

namespace PT.Common.Extensions;

public static class SerializationExtensions
{
    /// <summary>
    /// Creates a copy of an IPTSerializable object in memory
    /// Returns null if the IPTSerializable class does not have a Constructor with a single IReader parameter
    /// </summary>
    public static T CopyInMemory<T>(this T a_original) where T : class, IPTDeserializable
    {
        ConstructorInfo constructor = typeof(T).GetConstructor(new[] { typeof(IReader) });
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
            return (T)constructor.Invoke(new object[] { reader });
        }
    }

    /// <summary>
    /// Creates a copy of an IPTSerializable object in memory
    /// Invokes the provided delegate function
    /// </summary>
    public static T CopyInMemory<T>(this T a_original, Func<IReader, T> a_constructor) where T : class, IPTDeserializable
    {
        byte[] objBytes;
        using (BinaryMemoryWriter writer = new ())
        {
            a_original.Serialize(writer);
            objBytes = writer.GetBuffer();
        }

        using (BinaryMemoryReader reader = new (objBytes))
        {
            return a_constructor.Invoke(reader);
        }
    }
}