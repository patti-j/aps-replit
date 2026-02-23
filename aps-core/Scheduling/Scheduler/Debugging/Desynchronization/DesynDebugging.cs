namespace PT.Scheduler.Debugging;
#if TEST
    /// <summary>
    /// This was written for Product Backlog 1633.
    /// This class can be used to debug desynchronization issues between clients and server. It calculates and serializes 
    /// sum of numberic variables of objects as well as a desciption field containing string and object fields.
    /// These values can be used to check whether any variables are different between server and clients.
    /// 
    /// Code related to debugging using this class has been wrapped around DESYNC_DEBUG preprocessor keyword. Use this
    /// build configuration to debug desynchronization issues.
    ///
    /// 06/29/2012: GUA Desynchronization issue
    /// GUA was experiencing an issue where the data displayed on the client was different than what was published.
    /// It was discovered that the InventoryManager.m_inventories was a Hashtable. MRP logic would enumerate this 
    /// hashtable to generate jobs, but the order of Inventory objects in enumerations were different between the 
    /// Client and Server, therefore jobs had different Ids. 
    /// The solution was to change the data structure from Hashtable to CustomSortedListOptimized. This is a Sorted list
    /// that maintains a hashtable in the background for fast lookups. Enumerations from the list, always have the same order though.
    /// The data on the client and servers must match and be laid out in an identically.
    ///
    /// One way to determine at what point in deserialization the client and server start to differ is by looking at 
    /// ((PT.Common.BinaryFileReader)(reader)).fs.Position. You could open two instances of Visual Studio and put break points in
    /// different steps of deserialization and compare the Server and Client version.
    ///
    /// </summary>
    class DesyncDebuggingResult : PT.Common.IPTSerializable
    {

        internal DesyncDebuggingResult(decimal a_numericSum, string a_fields)
        {
            m_numericSum = a_numericSum;
            m_fields = a_fields;
        }

        decimal m_numericSum; // The sum of all fields that are convertable to a number (including enumerations).
        string m_fields; // A string description of all fields convertable to a number (including enumerations)

        internal DesyncDebuggingResult(Common.IReader reader)
        {
            reader.Read(out m_numericSum);
            reader.Read(out m_fields);
        }

        public void Serialize(Common.IWriter writer)
        {
            writer.Write(m_numericSum);
            writer.Write(m_fields);
        }

        const int UNIQUE_ID = 713;
        public int UniqueId
        {
            get { return UNIQUE_ID; }
        }

        internal static bool Different(object a_object, DesyncDebuggingResult a_originalResult, out DesyncDebuggingResult a_newDesyncResult, out List<string> o_differentFields)
        {
            a_newDesyncResult = DesyncDebugging.CalcPrimitiveNumericSum(a_object);
            try
            {
                o_differentFields = DifferentFields(a_originalResult, a_newDesyncResult);
            }
            catch (Exception e)
            {
                o_differentFields = new List<string>();
            }
            return a_newDesyncResult.m_numericSum != a_originalResult.m_numericSum;
        }

        internal static List<string> DifferentFields(DesyncDebuggingResult a_originalResult, DesyncDebuggingResult a_newResult)
        {
            HashSet<string> ignoreFields = new HashSet<string>();
            ignoreFields.Add("m_endOfPlanningHorizon");
            ignoreFields.Add("m_systemWideActivityIndex");
            ignoreFields.Add("mt_simulationType");
            ignoreFields.Add("m_simQty");
            ignoreFields.Add("m_availableQtyProfile");
            ignoreFields.Add("m_qtyProfile");
            ignoreFields.Add("_restoreInfo");
            ignoreFields.Add("item");
            ignoreFields.Add("m_templateMO");
            ignoreFields.Add("m_templateMoIdForRestoreReferencesOnly");



            List<string> result = new List<string>();
            Char[] pairDelimeter = new Char[] { ';' };
            Char[] fieldValueDelimeter = new Char[] { ':' };
            string[] origPairs = a_originalResult.m_fields.Split(pairDelimeter);
            string[] newPairs = a_newResult.m_fields.Split(pairDelimeter);
            if (a_originalResult.m_fields != "" && a_newResult.m_fields != "")
            {
                if (origPairs.Length == newPairs.Length)
                {
                    string[] origPair;
                    string[] newPair;

                    for (int i = 0; i < origPairs.Length; i++)
                    {
                        origPair = origPairs[i].Split(fieldValueDelimeter, 2);
                        newPair = newPairs[i].Split(fieldValueDelimeter, 2);

                        string val = origPair[0].TrimStart(' '); // Remove space from start of string.

                        if (!ignoreFields.Contains(val))
                        {
                            if (origPair[0].Equals(newPair[0]))
                            {
                                if (!origPair[1].Equals(newPair[1]))
                                {
                                    string fieldWithDifferentValues = string.Format("{0}:{1}:{2}", origPair[0], origPair[1], newPair[1]);
                                    result.Add(fieldWithDifferentValues);
                                }
                            }
                            else
                            {
                                throw new Exception("Field names don't match. They must be out of order since there're equal number of fields.");
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("The number of fields are different. They're expceted to be the same");
                }
            }
            return result;
        }
    }

    class DesyncDebugging
    {
        internal static Type s_lastType;
        internal static HashSet<Type> s_reviewdTypes = new HashSet<Type>();

        internal static DesyncDebuggingResult CalcPrimitiveNumericSum(object m_o)
        {
            Decimal numericSum = 0;
            System.Text.StringBuilder descriptionSB = new System.Text.StringBuilder();

            try
            {
                Type objectType = typeof(object);
                Type type = m_o.GetType();

                while (type != objectType)
                {
                    FieldInfo[] fia = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    for (int i = 0; i < fia.Length; i++)
                    {
                        FieldInfo fi = fia[i];
                        // Display name and type of the concerned member.
                        object fiVal = fi.GetValue(m_o);
                        Type fiType = Type.GetType(fi.FieldType.FullName);
                        //Console.WriteLine("'{0}' is a {1} {2}", fi.Name, fi.MemberType, fiVal);

                        if (fiType == null)
                            continue;

                        bool enumType = false;
                        if (fiType != objectType && fiType.BaseType != objectType && fiType.BaseType == typeof(System.Enum))
                        {
                            enumType = true;
                        }

                        if (fiType == typeof(bool)
                            || fiType == typeof(byte)
                            || fiType == typeof(DateTime)
                            || fiType == typeof(decimal)
                            || fiType == typeof(decimal)
                            || fiType == typeof(float)
                            || fiType == typeof(int)
                            || fiType == typeof(long)
                            || fiType == typeof(sbyte)
                            || fiType == typeof(short)
                            || fiType == typeof(uint)
                            || fiType == typeof(ulong)
                            || fiType == typeof(ushort)
                            || enumType
                            || fiType == typeof(PT.Common.BoolVector32))
                        {
                            decimal converted;
                            try
                            {
                                if (fiType == typeof(DateTime))
                                {
                                    DateTime td = (DateTime)fiVal;
                                    numericSum += td.Ticks;
                                }
                                else
                                {
                                    try
                                    {
                                        if (fiType == typeof(PT.Common.BoolVector32))
                                        {
                                            PT.Common.BoolVector32 bv = (PT.Common.BoolVector32)fiVal;
                                            converted = bv.ToInt();
                                        }
                                        else
                                        {
                                            converted = Convert.ToDecimal(fiVal);
                                        }
                                    }
                                    catch (OverflowException)
                                    {
                                        converted = -1;
                                    }

                                    decimal tmp = numericSum;
                                    try
                                    {
                                        numericSum += converted;
                                    }
                                    catch (OverflowException)
                                    {
                                        converted = -2;
                                        numericSum += converted;
                                    }
                                }

                                AppendValue(descriptionSB, fi, fiVal);
                            }
                            catch (FormatException)
                            {
                                throw;
                            }
                            catch (InvalidCastException)
                            {
                                throw;
                            }
                        }
                        else if (fiType == typeof(string))
                        {
                            object val = fiVal;
                            if (fiVal == null)
                            {
                                val = "nullString";
                            }
                            AppendValue(descriptionSB, fi, val);
                        }
                        else if (fiType.IsClass)
                        {
                            string text;
                            if (fiVal == null)
                            {
                                text = "nullObject";
                            }
                            else
                            {
                                text = "object";
                            }
                            AppendValue(descriptionSB, fi, text);
                        }
                        else
                        {
                        }
                    }
                    type = type.BaseType;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Exception : " + e.Message);
                throw;
            }

            string description = descriptionSB.ToString();
            return new DesyncDebuggingResult(numericSum, description);
        }

        private static void AppendValue(System.Text.StringBuilder descriptionSB, FieldInfo fi, object fiVal)
        {
            if (descriptionSB.Length == 0)
            {
                descriptionSB.AppendFormat("{0}:{1}", fi.Name, fiVal.ToString());
            }
            else
            {
                descriptionSB.AppendFormat("; {0}:{1}", fi.Name, fiVal.ToString());
            }
        }
    }

    //enum enumTest
    //{
    //    one,
    //    two
    //}

    //class Parent
    //{
    //    int p = 1;
    //    decimal dbl = 1;//8
    //    float flt = 1; //9
    //    enumTest et = enumTest.two;
    //}

    //class Child : Parent
    //{
    //    long c = 2;
    //}

    //class Child2 : Child
    //{
    //    public Decimal c2 = 4;
    //}

    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        Child2 c = new Child2();
    //        decimal d = DesyncDebugging.CalcPrimitiveNumericSum(c);
    //        Console.WriteLine("CalcPrimitiveNumericSum={0}", d);
    //    }
    //}
#endif