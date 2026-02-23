using SevenZip;

namespace PT.Common.Compression.LZMA;

public static class SevenZipHelper
{
    // static Int32 posStateBits = 2;
    // static  Int32 litContextBits = 3; // for normal files
    // UInt32 litContextBits = 0; // for 32-bit data
    // static  Int32 litPosBits = 0;
    // UInt32 litPosBits = 2; // for 32-bit data
    // static   Int32 algorithm = 2;
    // static    Int32 numFastBytes = 128;

    private static readonly CoderPropID[] s_propIDs =
    {
        CoderPropID.DictionarySize,
        CoderPropID.PosStateBits,
        CoderPropID.LitContextBits,
        CoderPropID.LitPosBits,
        CoderPropID.NumFastBytes,
        CoderPropID.MatchFinder,
        CoderPropID.EndMarker
        //CoderPropID.Algorithm
    };

    // these are the default properties, keeping it simple for now:
    private static readonly object[] s_properties =
    {
        1 << 24, //26. Higher = more memory usage but faster compression. Reduced to 24 since some clients were getting OutOfMemory exceptions.
        4, // 0 - 4; 2
        3, //0 - 8; 3
        0, // 0 - 4; 0
        12, // 5 - 128. Higher = more compression but slower. Extreme diminishing returns for scenario files. Anything above 50 is a complete waste
        "BT4", //Only method supported
        false
        //2 not supported
    };

    private const int c_threadCount = 8;
    private const int c_minThreadSize = 1 * 1024 * 1024 / 10; // 1/10 MB

    /// <summary>
    /// Uses multiple threads to compress data into compressed chunks
    /// This is a slow but very powerful compression method
    /// </summary>
    public static List<byte[]> Compress(Stream a_stream)
    {
        List<byte[]> array = new ();

        a_stream.Seek(0, 0);
        int length = Convert.ToInt32(Math.Ceiling(a_stream.Length / (double)c_threadCount));

        if (length > c_minThreadSize)
        {
            byte[][] byteMap = new byte[c_threadCount][];
            for (int i = 0; i < byteMap.Length; i++)
            {
                byteMap[i] = new byte[length];
                a_stream.Read(byteMap[i], 0, length);
            }

            List<Task> taskList = new ();
            for (int i = 0; i < byteMap.Length; i++)
            {
                int i1 = i;
                Task t1 = Task.Run(new Action(() => byteMap[i1] = Compress(byteMap[i1])));
                taskList.Add(t1);
            }

            Task.WaitAll(taskList.ToArray());

            foreach (byte[] bytes in byteMap)
            {
                array.Add(bytes);
            }

            return array;
        }

        //The file is too small to break up. Just compress on a single thread
        byte[] s1 = new byte[a_stream.Length];
        a_stream.Read(s1, 0, s1.Length);
        s1 = Compress(s1);
        array.Add(s1);
        return array;
    }

    /// <summary>
    /// Decompresses a set of compressed chunks
    /// </summary>
    /// <returns>The original uncompressed bytes</returns>
    public static byte[] Decompress(List<byte[]> a_inputBytes)
    {
        byte[][] byteMap = new byte[a_inputBytes.Count][];
        List<Task> taskList = new ();

        for (int i = 0; i < a_inputBytes.Count; i++)
        {
            int i1 = i;
            Task t1 = Task.Run(new Action(() => byteMap[i1] = Decompress(a_inputBytes[i1])));
            taskList.Add(t1);
        }

        Task.WaitAll(taskList.ToArray());

        using (MemoryStream s = new ())
        {
            foreach (byte[] bytes in byteMap)
            {
                s.Write(bytes, 0, bytes.Length);
            }

            return s.ToArray();
        }
    }

    public static byte[] Compress(byte[] a_inputBytes)
    {
        using (MemoryStream inStream = new (a_inputBytes))
        {
            using (MemoryStream outStream = new ())
            {
                SevenZip.Compression.LZMA.Encoder encoder = new ();
                encoder.SetCoderProperties(s_propIDs, s_properties);
                encoder.WriteCoderProperties(outStream);
                long fileSize = inStream.Length;
                for (int i = 0; i < 8; i++)
                {
                    outStream.WriteByte((byte)(fileSize >> (8 * i)));
                }

                encoder.Code(inStream, outStream, -1, -1, null);
                return outStream.ToArray();
            }
        }
    }

    public static byte[] Decompress(byte[] a_inputBytes)
    {
        using (MemoryStream newInStream = new (a_inputBytes))
        {
            SevenZip.Compression.LZMA.Decoder decoder = new ();

            newInStream.Seek(0, 0);
            using (MemoryStream newOutStream = new ())
            {
                byte[] properties2 = new byte[5];
                if (newInStream.Read(properties2, 0, 5) != 5)
                {
                    throw new Exception("input .lzma is too short");
                }

                long outSize = 0;
                for (int i = 0; i < 8; i++)
                {
                    int v = newInStream.ReadByte();
                    if (v < 0)
                    {
                        throw new Exception("Can't Read 1");
                    }

                    outSize |= (long)(byte)v << (8 * i);
                }

                decoder.SetDecoderProperties(properties2);

                long compressedSize = newInStream.Length - newInStream.Position;
                decoder.Code(newInStream, newOutStream, compressedSize, outSize, null);

                byte[] b = newOutStream.ToArray();

                return b;
            }
        }
    }
}