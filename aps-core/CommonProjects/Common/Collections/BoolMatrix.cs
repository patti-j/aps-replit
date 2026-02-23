namespace PT.Common;

/// <summary>
/// This is a memory efficient data structure for representing a matrix of bool values. Each bool in the matrix only takes 1 bit.
/// </summary>
public class BoolMaxtrix : IPTSerializable
{
    #region IPTSerializable Members
    public BoolMaxtrix(IReader a_reader)
    {
        int rows;
        int cols;
        a_reader.Read(out rows);
        a_reader.Read(out cols);
        Init(rows, cols);
        int length = m_bits.Length;
        for (int i = 0; i < length; ++i)
        {
            int v;
            a_reader.Read(out v);
            m_bits[i] = v;
        }
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private static void ValidateInitialization(int a_nbrOfRows, int a_nbrOfCols)
    {
        if (a_nbrOfRows <= 0 || a_nbrOfCols <= 0)
        {
            throw new Exception("The number of rows and columns in a bitVector must be greater than 0.");
        }
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_nbrOfRows);
        writer.Write(m_nbrOfCols);
        writer.Write(m_bits.Length);
        int length = m_bits.Length;
        for (int i = 0; i < length; ++i)
        {
            writer.Write(m_bits[i]);
        }
    }

    public int UniqueId =>
        // TODO:  Add BoolVector32.UniqueId getter implementation
        0;
    #endregion

    private int[] m_bits;
    private int m_nbrOfRows;
    private int m_nbrOfCols;

    private const int c_bits = 32;

    public int Rows => m_nbrOfRows;

    public int Columns => m_nbrOfCols;

    public BoolMaxtrix(int a_nbrOfRows, int a_nbrOfCols)
    {
        Init(a_nbrOfRows, a_nbrOfCols);
    }

    public BoolMaxtrix(int a_nbrOfRows, int a_nbrOfCols, bool a_defaultValue)
    {
        Init(a_nbrOfRows, a_nbrOfCols);
        if (a_defaultValue)
        {
            for (int i = 0; i < m_bits.Length; ++i)
            {
                m_bits[i] = byte.MaxValue;
            }
        }
    }

    private void Init(int a_nbrOfRows, int a_nbrOfCols)
    {
        ValidateInitialization(a_nbrOfRows, a_nbrOfCols);

        m_nbrOfRows = a_nbrOfRows;
        m_nbrOfCols = a_nbrOfCols;

        int nbrOfInts = CalcNbrOfInts();
        m_bits = new int[nbrOfInts];
    }

    private int CalcNbrOfInts()
    {
        double nbrOfFlags = m_nbrOfRows * m_nbrOfCols;
        int nbrOfInts = (int)Math.Ceiling(nbrOfFlags / c_bits);
        return nbrOfInts;
    }

    /// <summary>
    /// Set all the booleans to false.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < m_bits.Length; ++i)
        {
            m_bits[i] = 0;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="a_row"></param>
    /// <param name="a_col"></param>
    /// <returns></returns>
    public bool Get(int a_row, int a_col)
    {
        VerifyRowCol(a_row, a_col);
        int rowIdx = GetByteIdx(a_row, a_col);
        int bitIdx = GetBitIdx(a_row, a_col);

        return (m_bits[rowIdx] & (1 << bitIdx)) != 0;
    }

    /// <summary>
    /// Get the value as an integer.
    /// </summary>
    public int GetAsInt(int a_row, int a_col)
    {
        return Get(a_row, a_col) ? 1 : 0;
    }

    /// <summary>
    /// Set the specified entry to true or false.
    /// </summary>
    public void Set(int a_row, int a_col, bool a_value)
    {
        VerifyRowCol(a_row, a_col);

        if (a_value)
        {
            SetTrue(a_row, a_col);
        }
        else
        {
            SetFalse(a_row, a_col);
        }
    }

    /// <summary>
    /// Sets the specified boolean to true.
    /// </summary>
    public void SetTrue(int a_row, int a_col)
    {
        VerifyRowCol(a_row, a_col);

        int byteIdx = GetByteIdx(a_row, a_col);
        int bitIdx = GetBitIdx(a_row, a_col);
        m_bits[byteIdx] = m_bits[byteIdx] | (1 << bitIdx);
    }

    /// <summary>
    /// Sets the specified boolean to false.
    /// </summary>
    public void SetFalse(int a_row, int a_col)
    {
        VerifyRowCol(a_row, a_col);

        int byteIdx = GetByteIdx(a_row, a_col);
        int bitIdx = (byte)GetBitIdx(a_row, a_col);
        m_bits[byteIdx] = m_bits[byteIdx] & ~(1 << bitIdx);
    }

    #if DEBUG
    private static bool s_dePrintMatrix;
    #endif

    /// <summary>
    /// Below is an example of what would be returned
    /// if you had set every other boolean to true
    /// in a 2x8 matrix.
    /// 01010101
    /// 01010101
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        #if DEBUG
        if (s_dePrintMatrix)
        {
            System.Text.StringBuilder sb = new ();
            for (int row = 0; row < m_nbrOfRows; ++row)
            {
                for (int col = 0; col < m_nbrOfCols; ++col)
                {
                    char c;
                    if (Get(row, col))
                    {
                        c = '1';
                    }
                    else
                    {
                        c = '0';
                    }

                    sb.Append(c);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
        #endif
        {
            return string.Format("Rows={0}; Columns={1}", m_nbrOfRows, m_nbrOfCols);
        }
    }

    /// <summary>
    /// Get the byte the bit is stored in.
    /// </summary>
    private int GetByteIdx(int a_row, int a_col)
    {
        return (a_row * m_nbrOfCols + a_col) / c_bits;
    }

    /// <summary>
    /// Get the index of the bit within a byte.
    /// </summary>
    private int GetBitIdx(int a_row, int a_col)
    {
        return (a_row * m_nbrOfCols + a_col) % c_bits;
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private void VerifyRowCol(int a_row, int a_col)
    {
        #if RELEASE
            throw new Exception("Test whether conditional works.");
        #endif
        if (a_row >= m_nbrOfRows)
        {
            throw new Exception("Row index out of range.");
        }

        if (a_col >= m_nbrOfCols)
        {
            throw new Exception("Column index out of range.");
        }
    }

    #region Program Testing
    //static void Main(string[] args)
    //{
    //    int x = 1;
    //    while (x > 0)
    //    {
    //        RandomTest(x, 1350, false, 1000);
    //        ++x;
    //    }
    //}

    private static void RandomTest(int a_testNbr, int a_maxRowsCols, bool a_printTestAndBoolMatrix, int a_pauseBetweenTestsMilliseconds)
    {
        Thread.Sleep(a_pauseBetweenTestsMilliseconds);

        Random random = new ();

        int rows = random.Next(1, a_maxRowsCols);
        int cols = random.Next(1, a_maxRowsCols);
        #if DEBUG
        s_dePrintMatrix = a_printTestAndBoolMatrix;
        #endif

        Console.Write("Test {0}: Rows={1}; Columns={2}; ", a_testNbr, rows, cols);

        BoolMaxtrix bm = new (rows, cols);

        int[,] testSet = new int[rows, cols];

        int oddCount = 0;
        int evenCount = 0;

        for (int row = 0; row < rows; ++row)
        {
            System.Text.StringBuilder sb = new ();

            for (int col = 0; col < cols; ++col)
            {
                int rn = random.Next(1, int.MaxValue);
                bool b = rn % 2 == 1;

                if (b)
                {
                    ++oddCount;
                    testSet[row, col] = 1;
                    bm.SetTrue(row, col);
                    sb.Append("1");
                }
                else
                {
                    ++evenCount;
                    sb.Append("0");
                }
            }
        }

        ValidateTestSet(testSet, bm);
        Console.WriteLine("Odd={0}; Even={1}; Success!", oddCount, evenCount);

        if (a_printTestAndBoolMatrix)
        {
            Console.WriteLine("---------------------------------------------------------------------------");
            PrintTestSet(testSet);
            Console.WriteLine("---------------------------------------------------------------------------");
            Console.Write(bm.ToString());
            Console.WriteLine("---------------------------------------------------------------------------");
        }
    }

    private static void ValidateTestSet(int[,] a_testSet, BoolMaxtrix a_bm)
    {
        int rows = a_testSet.GetLength(0);
        int cols = a_testSet.GetLength(1);
        if (rows != a_bm.Rows || cols != a_bm.Columns)
        {
            throw new Exception("They're not the same size.");
        }

        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < cols; ++col)
            {
                if (a_testSet[row, col] != a_bm.GetAsInt(row, col))
                {
                    throw new Exception(string.Format("Row {0}, Col {1} is wrong. It should be {2}", row, col, a_testSet[row, col]));
                }
            }
        }
    }

    private static void PrintTestSet(int[,] a_testSet)
    {
        int rows = a_testSet.GetLength(0);
        int cols = a_testSet.GetLength(1);

        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < cols; ++col)
            {
                Console.Write(a_testSet[row, col]);
            }

            Console.WriteLine();
        }
    }
    #endregion
}