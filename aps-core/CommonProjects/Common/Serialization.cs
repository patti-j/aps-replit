using System.Reflection;

using PT.Common.Compression;

namespace PT.Common;

/*
 *  Written: Nov. 2023, Peter Chen
 *  Serialization Overview:
 *    Serialization is a process that converts data into another form and stores it
 *    in a specific order. It can be input (reading data into memory) or output
 *    (saving the scenario on disk). It is used in the PlanetTogether program whenever
 *    there is data that needs to persist longer than the application's runtime (ex: user settings),
 *    which is also often data that needs to be synchronized across the system instance and
 *    users (most scenario data).
 *
 *  PlanetTogether Serialization Structure:
 *    1. The PTBaseObject class implements IPTSerializable interface, which has the Serialize
 *    function that takes in a writer.
 *    2. They should all have a constructor that takes in an IReader too,
 *    but this is not enforced in the IPTSerializable interface. Within the
 *    constructor, there should be a group conditional statements that check
 *    what version was used when the scenario was being serialized/saved,
 *    then it should de-serialize the scenario accordingly. This is done
 *    for backwards compatibility.
 *       This constructor is often accessed through the use of reflection
 *    which is why many of them will show up as having 0 references.
 *    3. In some cases, there will also be a RestoreReferences(...) function
 *    where the input parameters are chosen by the developer. This function can be
 *    added when PTObjects store references to other objects or if an object's property
 *    is serialized in another object. The input parameters will usually consist of
 *    information necessary to re-established inter-object relationships, and will
 *    frequently be the scenario detail.
 *
 *  How to Make Changes to PT Serialization:
 *   1. Increment Serialization.VersionNumber (below) by 1
 *   2. Remove/Add properties to the Serialize function of the PTObject
 *      being modified as necessary
 *   3. Copy the if statement block corresponding to the most recent
 *      de-serialization code, paste it to the top, change the version
 *      to whatever the newest value is, and add an "else" to the if
 *      statement so it becomes an else-if. Now remove/add properties
 *      to each of the de-serialization code as needed.
 *      *** It is critical that order of the newest if block ***
 *      *** matchs the order of the new Serialize function   ***
 *   4. Implement and use RestoreReferences(...) if needed
 *
 *  Most Common Mistake:
 *   - Don't forget to add the "else" to the old if statement so it
 *     becomes and else-if!
 */
public class Serialization
{
    public static readonly int VersionNumber = 13014;

    //NOTE: ONLY VERSION 740 AND HIGHER NEED TO BE MAINTAINED. OLD SERIALIZATION CODE CAN BE REMOVED
    //IF There is no reader 740, keep the next highest value under 740 so serialization 740 will still work

    //If an object has a new boolean stored in a bool vector, we don't want to update serialization, we want to phrase the new
    //boolean in such a way that the default is false as to be backwards compatible with older serialization versions. For example: 
    //we added a bool on UserField for whether to publish the UDF or not. Since UDFs were previously always published, we want to name the new
    //field 'ExcludeFromPublish' so that when an older scenario is loaded into the new version, the 'ExcludeFromPublish' flag on the bool vector will be false and the UDF
    //can still publish

    public delegate object CopyCreatorDelegate(IReader a_reader);

    /// <summary>
    /// Creates a copy of an object in memory
    /// Also returns the size of the object as an out variable.
    /// </summary>
    public static object CopyInMemory(IPTSerializable a_original, CopyCreatorDelegate a_copyCreatorDelegate, out long o_sizeInBytesSerialized)
    {
        object o;

        byte[] objBytes;
        using (BinaryMemoryWriter writer = new ())
        {
            a_original.Serialize(writer);
            objBytes = writer.GetBuffer();
        }

        using (BinaryMemoryReader reader = new (objBytes))
        {
            o = a_copyCreatorDelegate(reader);
            o_sizeInBytesSerialized = reader.Length;
        }

        return o;
    }

    public static object CopyInMemory(IPTSerializable a_original, IClassFactory a_classFactory)
    {
        byte[] objBytes;
        // Save the object to disk.
        using (BinaryMemoryWriter writer = new ())
        {
            writer.Write(a_original.UniqueId);
            a_original.Serialize(writer);
            objBytes = writer.GetBuffer();
        }

        // Create a copy of the object.
        using (BinaryMemoryReader reader = new (objBytes))
        {
            return a_classFactory.Deserialize(reader);
        }
    }

    /// <summary>
    /// Copies an object in memory and serializes it in a BinaryMemoryWriter for later access.
    /// </summary>
    public static byte[] CopyAndStoreInByteArray(IPTSerializable a_original)
    {
        using (BinaryMemoryWriter writer = new ())
        {
            a_original.Serialize(writer);
            return writer.GetBuffer();
        }
    }
    /// <summary>
    /// Creates a copy of an obect that has been serialized in a BinaryMemoryWriter.
    /// </summary>
    public static object CopyFromSerializedByteArray(byte[] a_serializedBytes, CopyCreatorDelegate a_copyCreatorDelegate)
    {
        object o;

        using (BinaryMemoryReader reader = new (a_serializedBytes))
        {
            o = a_copyCreatorDelegate(reader);
        }

        return o;
    }

    /// <summary>
    /// Copies an object by writing it to a file and then reading it back.
    /// Use this as apposed to CopyInMemory to avoid using extra memory.
    /// </summary>
    public static object CopyWithFile(IPTSerializable a_original, CopyCreatorDelegate a_copyCreatorDelegate, out long o_sizeInBytesSerialized)
    {
        object o;
        string tempFileName = Path.GetTempFileName();
        o_sizeInBytesSerialized = 0;

        try
        {
            using (BinaryFileWriter writer = new (tempFileName))
            {
                a_original.Serialize(writer);
            }

            using (BinaryFileReader reader = new (tempFileName))
            {
                o = a_copyCreatorDelegate(reader);
                o_sizeInBytesSerialized = reader.Length;
            }
        }
        finally
        {
            System.IO.File.Delete(tempFileName);
        }

        return o;
    }

    public static object CopyWithFile(IPTSerializable a_original, IClassFactory a_classFactory)
    {
        object o;
        string tempFileName = Path.GetTempFileName();

        try
        {
            // Save the object to disk.
            using (BinaryFileWriter writer = new (tempFileName, ECompressionType.Fast))
            {
                writer.Write(a_original.UniqueId);
                a_original.Serialize(writer);
            }

            // Create a copy of the object.
            using (BinaryFileReader reader = new (tempFileName))
            {
                o = a_classFactory.Deserialize(reader);
            }
        }
        finally
        {
            System.IO.File.Delete(tempFileName);
        }

        return o;
    }

    /// <summary>
    /// Serializes a PTSerializable to a provided stream.
    /// </summary>
    /// <param name="ptSerializable"></param>
    /// <param name="outputStream"></param>
    public static void SerializeToStream(IPTSerializable a_ptSerializable, MemoryStream a_outputStream)
    {
        using (BinaryMemoryWriter writer = new ())
        {
            writer.Write(a_ptSerializable.UniqueId);
            a_ptSerializable.Serialize(writer);
            writer.CompressAndCopyStream(a_outputStream);
        }
    }

    /// <summary>
    /// Attempts to deserialize a byte array dynamically into an IPTSerializable object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_serializedBytes"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IPTSerializable TryDeserialize<T>(byte[] a_serializedBytes) where T : class
    {
        // Don't allow passing in a generic that doesn't implement IPTSerializable.
        // Note: We can't constrain the generic for this in the method signature. The caller may not have an instance of T,
        // and without one cannot determine if it implements the interface at compile time.
        if (!typeof(IPTSerializable).IsAssignableFrom(typeof(T)))
        {
            throw new ArgumentException("The generic T must implement IPTSerializable.");
        }

        // Allow empty arrays to return as null objects.
        if (a_serializedBytes.Length == 0)
        {
            return null;
        }

        // Begin Deserializing
        // Pull out IPTSerializable type identifier from serialized object
        int deserializedUniqueId = -1;
        using BinaryMemoryReader reader = new (a_serializedBytes);
        reader.Read(out deserializedUniqueId);

        // Attempt to construct a new T from the response array
        ConstructorInfo iReaderConstructor = GetDeserializeConstructor<T>();
        object deserializedObject = iReaderConstructor.Invoke(new[] { reader });

        // Ensure it deserialized to the correct type
        if (deserializedObject is T and IPTSerializable ptSerializable // T was constructable from the data
            &&
            ptSerializable.UniqueId == deserializedUniqueId) // The correct UniqueId was encoded with the data
        {
            return ptSerializable;
        }

        throw new ArgumentException($"Expected PTSerializable {nameof(T)} but received {nameof(deserializedObject)}");
    }

    /// <summary>
    /// Uses reflection to get a constructor that takes an IReader and constructs an object based on what's deserialized there.
    /// By convention, all IPTSerializable objects have this constructor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static ConstructorInfo GetDeserializeConstructor<T>() where T : class
    {
        ConstructorInfo iReaderConstructor = typeof(T).GetConstructor(new[] { typeof(IReader) });

        if (iReaderConstructor == null)
        {
            throw new InvalidOperationException($"No matching constructor found for {typeof(T)}");
        }

        return iReaderConstructor;
    }
}