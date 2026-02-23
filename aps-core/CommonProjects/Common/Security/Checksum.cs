namespace PT.Common.Security;

public class Checksum
{
    public static decimal StringChecksum(string a_s, out decimal o_checksum, out int o_lineCount, out int o_charCount)
    {
        string tempFile = Path.GetTempFileName();
        decimal d;
        try
        {
            System.IO.File.AppendAllText(tempFile, a_s);
            d = FileChecksum(tempFile, out o_checksum, out o_lineCount, out o_charCount);
        }
        finally
        {
            System.IO.File.Delete(tempFile);
        }

        return d;
    }

    public static decimal FileChecksum(string path, out decimal o_checksum, out int o_lineCount, out int o_charCount)
    {
        o_lineCount = 0;
        o_charCount = 0;
        o_checksum = -111270;

        StreamReader sr = System.IO.File.OpenText(path);

        try
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                ++o_lineCount;
                for (int i = 0; i < line.Length; ++i)
                {
                    ++o_charCount;
                    o_checksum += CharacterChecksum(line[i], o_lineCount, i);
                }
            }
        }
        finally
        {
            sr.Close();
        }

        return o_checksum;
    }

    private static decimal CharacterChecksum(char a_c, int a_line, int a_index)
    {
        decimal checksum = a_c;
        checksum += a_line * a_c;
        checksum += a_index * a_c;
        return checksum;
    }
}