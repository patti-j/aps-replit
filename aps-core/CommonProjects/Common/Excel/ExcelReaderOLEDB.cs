//using System;
//using System.Data;
//using System.Data.OleDb;

//namespace PT.Common.Excel
//{
//    /// <summary>
//    /// For reading from and writing to Excel files.
//    /// Downloaded from: http://www.codeproject.com/csharp/Excel_using_OLEDB.asp
//    /// </summary>
//    public class ExcelReaderOLEDB : IDisposable
//    {
//        #region Variables
//        private int[] m_pkCol;
//        private string m_strExcelFilename;
//        private bool m_blnMixedData = true;
//        private bool m_blnHeaders;
//        private string m_strSheetName;
//        private string m_strSheetRange;
//        private bool m_blnKeepConnectionOpen;
//        private OleDbConnection m_oleConn;
//        private OleDbCommand m_oleCmdSelect;
//        private OleDbCommand m_oleCmdUpdate;
//        #endregion

//        #region properties
//        public int[] PKCols
//        {
//            get { return m_pkCol; }
//            set { m_pkCol = value; }
//        }

//        public string ColName(int a_intCol)
//        {
//            string sColName;
//            if (a_intCol < 26)
//            {
//                sColName = Convert.ToString(Convert.ToChar((Convert.ToByte((char)'A') + a_intCol)));
//            }
//            else
//            {
//                int intFirst = (a_intCol / 26);
//                int intSecond = (a_intCol % 26);
//                sColName = Convert.ToString(Convert.ToByte('A') + intFirst);
//                sColName += Convert.ToString(Convert.ToByte('A') + intSecond);
//            }
//            return sColName;
//        }

//        public int ColNumber(string a_strCol)
//        {
//            a_strCol = a_strCol.ToUpper();
//            int intColNumber;
//            if (a_strCol.Length > 1)
//            {
//                intColNumber = Convert.ToInt16(Convert.ToByte(a_strCol[1]) - 65);
//                intColNumber += Convert.ToInt16(Convert.ToByte(a_strCol[1]) - 64) * 26;
//            }
//            else
//            {
//                intColNumber = Convert.ToInt16(Convert.ToByte(a_strCol[0]) - 65);
//            }
//            return intColNumber;
//        }

//        public String[] GetExcelSheetNames()
//        {
//            DataTable dt = null;

//            try
//            {
//                if (m_oleConn == null)
//                {
//                    Open();
//                }

//                // Get the data table containing the schema
//                dt = m_oleConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

//                if (dt == null)
//                {
//                    return null;
//                }

//                String[] excelSheets = new String[dt.Rows.Count];
//                int i = 0;

//                // Add the m_sheet name to the string array.
//                foreach (DataRow row in dt.Rows)
//                {
//                    string strSheetTableName = row["TABLE_NAME"].ToString();
//                    excelSheets[i] = strSheetTableName.Substring(0, strSheetTableName.Length - 1);
//                    i++;
//                }

//                return excelSheets;
//            }
//            catch (Exception)
//            {
//                return null;
//            }
//            finally
//            {
//                // Clean up.
//                if (KeepConnectionOpen == false)
//                {
//                    Close();
//                }
//                if (dt != null)
//                {
//                    dt.Dispose();
//                    dt = null;
//                }
//            }
//        }

//        public string ExcelFilename
//        {
//            get { return m_strExcelFilename; }
//            set { m_strExcelFilename = value; }
//        }

//        public string SheetName
//        {
//            get { return m_strSheetName; }
//            set { m_strSheetName = value; }
//        }

//        /// <summary>
//        /// Range such as A1:C5.  Must contain a colon.
//        /// </summary>
//        public string SheetRange
//        {
//            get { return m_strSheetRange; }
//            set
//            {
//                if (value.IndexOf(":") == -1)
//                {
//                    throw new Exception("Invalid range length");
//                }
//                m_strSheetRange = value;
//            }
//        }

//        public bool KeepConnectionOpen
//        {
//            get { return m_blnKeepConnectionOpen; }
//            set { m_blnKeepConnectionOpen = value; }
//        }

//        public bool Headers
//        {
//            get { return m_blnHeaders; }
//            set { m_blnHeaders = value; }
//        }

//        /// <summary>
//        /// If true then mixed data columns load the correct data type for each value.
//        /// If false then mixed data columns are read as text for all rows.
//        /// </summary>
//        public bool MixedData
//        {
//            get { return m_blnMixedData; }
//            set { m_blnMixedData = value; }
//        }
//        #endregion

//        #region Methods

//        #region Excel Connection
//        private string ExcelConnectionOptions()
//        {
//            string strOpts = "";
//            if (MixedData)
//            {
//                strOpts += "Imex=2;";
//            }
//            else
//            {
//                strOpts += "Imex=1;"; //read mixed data as text
//            }
//            if (Headers)
//            {
//                strOpts += "HDR=Yes;";
//            }
//            else
//            {
//                strOpts += "HDR=No;";
//            }
//            return strOpts;
//        }

//        private string ExcelConnection()
//        {
//            return
//                @"Provider=Microsoft.Jet.OLEDB.4.0;" +
//                @"Data Source=" + m_strExcelFilename + ";" +
//                @"Extended Properties=" + Convert.ToChar(34) +
//                @"Excel 8.0;" + ExcelConnectionOptions() + Convert.ToChar(34);
//        }
//        #endregion

//        #region Open / Close
//        public void Open()
//        {
//            if (m_oleConn != null)
//            {
//                if (m_oleConn.State == ConnectionState.Open)
//                {
//                    m_oleConn.Close();
//                }
//                m_oleConn = null;
//            }

//            if (System.IO.File.Exists(m_strExcelFilename) == false)
//            {
//                throw new Exception("Excel file " + m_strExcelFilename + "could not be found.");
//            }
//            m_oleConn = new OleDbConnection(ExcelConnection());

//            m_oleConn.Open();
//        }

//        public void Close()
//        {
//            if (m_oleConn != null)
//            {
//                if (m_oleConn.State != ConnectionState.Closed)
//                {
//                    m_oleConn.Close();
//                }
//                m_oleConn.Dispose();
//                m_oleConn = null;
//            }
//        }
//        #endregion

//        #region Command Select
//        private bool SetSheetQuerySelect()
//        {
//            if (m_oleConn == null)
//            {
//                throw new Exception("Connection is unassigned or closed.");
//            }

//            if (m_strSheetName.Length == 0)
//            {
//                throw new Exception("Sheetname was not assigned.");
//            }

//            m_oleCmdSelect = new OleDbCommand(
//                @"SELECT * FROM ["
//                + m_strSheetName
//                + "$" + m_strSheetRange
//                + "]", m_oleConn);

//            return true;
//        }
//        #endregion

//        #region simple utilities
//        private string AddWithComma(string a_strSource, string a_strAdd)
//        {
//            if (a_strSource != "")
//            {
//                a_strSource += ", ";
//            }
//            return a_strSource + a_strAdd;
//        }

//        private string AddWithAnd(string a_strSource, string a_strAdd)
//        {
//            if (a_strSource != "")
//            {
//                a_strSource += " and ";
//            }
//            return a_strSource + a_strAdd;
//        }
//        #endregion

//        private OleDbDataAdapter SetSheetQueryAdapter(DataTable a_dt)
//        {
//            // Deleting in Excel workbook is not possible
//            //So this command is not defined
//            if (m_oleConn == null)
//            {
//                throw new Exception("Connection is unassigned or closed.");
//            }

//            if (m_strSheetName.Length == 0)
//            {
//                throw new Exception("Sheetname was not assigned.");
//            }

//            if (PKCols == null)
//            {
//                throw new Exception("Cannot update excel m_sheet with no primarykey set.");
//            }
//            if (PKCols.Length < 1)
//            {
//                throw new Exception("Cannot update excel m_sheet with no primarykey set.");
//            }

//            OleDbDataAdapter oleda = new OleDbDataAdapter(m_oleCmdSelect);
//            string strUpdate = "";
//            string strInsertPar = "";
//            string strInsert = "";
//            string strWhere = "";

//            for (int iPK = 0; iPK < PKCols.Length; iPK++)
//            {
//                strWhere = AddWithAnd(strWhere, a_dt.Columns[iPK].ColumnName + "=?");
//            }
//            strWhere = " Where " + strWhere;

//            for (int iCol = 0; iCol < a_dt.Columns.Count; iCol++)
//            {
//                strInsert = AddWithComma(strInsert, a_dt.Columns[iCol].ColumnName);
//                strInsertPar = AddWithComma(strInsertPar, "?");
//                strUpdate = AddWithComma(strUpdate, a_dt.Columns[iCol].ColumnName) + "=?";
//            }

//            string strTable = "[" + SheetName + "$" + this.SheetRange + "]";
//            strInsert = "INSERT INTO " + strTable + "(" + strInsert + ") Values (" + strInsertPar + ")";
//            strUpdate = "Update " + strTable + " Set " + strUpdate + strWhere;

//            oleda.InsertCommand = new OleDbCommand(strInsert, m_oleConn);
//            oleda.UpdateCommand = new OleDbCommand(strUpdate, m_oleConn);

//            for (int iCol = 0; iCol < a_dt.Columns.Count; iCol++)
//            {
//                OleDbParameter oleParIns = new OleDbParameter("?", a_dt.Columns[iCol].DataType.ToString());
//                OleDbParameter oleParUpd = new OleDbParameter("?", a_dt.Columns[iCol].DataType.ToString());
//                oleParIns.SourceColumn = a_dt.Columns[iCol].ColumnName;
//                oleParUpd.SourceColumn = a_dt.Columns[iCol].ColumnName;
//                oleda.InsertCommand.Parameters.Add(oleParIns);
//                oleda.UpdateCommand.Parameters.Add(oleParUpd);
//                oleParIns = null;
//                oleParUpd = null;
//            }

//            for (int iPK = 0; iPK < PKCols.Length; iPK++)
//            {
//                OleDbParameter oleParUpd = new OleDbParameter("?", a_dt.Columns[iPK].DataType.ToString());
//                oleParUpd.SourceColumn = a_dt.Columns[iPK].ColumnName;
//                oleParUpd.SourceVersion = DataRowVersion.Original;
//                oleda.UpdateCommand.Parameters.Add(oleParUpd);
//            }
//            return oleda;
//        }

//        #region command Singe Value Update
//        private bool SetSheetQuerySingelValUpdate(string a_strVal)
//        {
//            if (m_oleConn == null)
//            {
//                throw new Exception("Connection is unassigned or closed.");
//            }

//            if (m_strSheetName.Length == 0)
//            {
//                throw new Exception("Sheetname was not assigned.");
//            }

//            m_oleCmdUpdate = new OleDbCommand(
//                @" Update ["
//                + m_strSheetName
//                + "$" + m_strSheetRange
//                + "] set F1=" + a_strVal, m_oleConn);
//            return true;
//        }
//        #endregion

//        public void SetPrimaryKey(int a_intCol)
//        {
//            m_pkCol = new int[] {a_intCol};
//        }

//        public DataTable GetTable()
//        {
//            return GetTable("ExcelTable");
//        }

//        private void SetPrimaryKey(DataTable a_dt)
//        {
//            if (PKCols != null)
//            {
//                //set the primary key
//                if (PKCols.Length > 0)
//                {
//                    DataColumn[] dc;
//                    dc = new DataColumn[PKCols.Length];
//                    for (int i = 0; i < PKCols.Length; i++)
//                    {
//                        dc[i] = a_dt.Columns[PKCols[i]];
//                    }

//                    a_dt.PrimaryKey = dc;
//                }
//            }
//        }

//        public DataTable GetTable(string a_strTableName)
//        {
//            //Open and query
//            if (m_oleConn == null)
//            {
//                Open();
//            }
//            if (m_oleConn.State != ConnectionState.Open)
//            {
//                throw new Exception("Connection cannot open error.");
//            }
//            if (SetSheetQuerySelect() == false)
//            {
//                return null;
//            }

//            //Fill table
//            OleDbDataAdapter oleAdapter = new OleDbDataAdapter();
//            oleAdapter.SelectCommand = m_oleCmdSelect;
//            DataTable dt = new DataTable(a_strTableName);
//            oleAdapter.FillSchema(dt, SchemaType.Source);
//            oleAdapter.Fill(dt);
//            if (Headers == false)
//            {
//                if (m_strSheetRange.IndexOf(":") > 0)
//                {
//                    string FirstCol = m_strSheetRange.Substring(0, m_strSheetRange.IndexOf(":") - 1);
//                    int intCol = ColNumber(FirstCol);
//                    for (int intI = 0; intI < dt.Columns.Count; intI++)
//                    {
//                        dt.Columns[intI].Caption = ColName(intCol + intI);
//                    }
//                }
//            }
//            SetPrimaryKey(dt);
//            //Cannot delete rows in Excel workbook
//            dt.DefaultView.AllowDelete = false;

//            //Clean up
//            m_oleCmdSelect.Dispose();
//            m_oleCmdSelect = null;
//            oleAdapter.Dispose();
//            oleAdapter = null;
//            if (KeepConnectionOpen == false)
//            {
//                Close();
//            }
//            return dt;
//        }

//        private void CheckPKExists(DataTable a_dt)
//        {
//            if (a_dt.PrimaryKey.Length == 0)
//            {
//                if (PKCols != null)
//                {
//                    SetPrimaryKey(a_dt);
//                }
//                else
//                {
//                    throw new Exception("Provide an primary key to the datatable");
//                }
//            }
//        }

//        public DataTable SetTable(DataTable a_dt)
//        {
//            DataTable dtChanges = a_dt.GetChanges();
//            if (dtChanges == null)
//            {
//                throw new Exception("There are no changes to be saved!");
//            }
//            CheckPKExists(a_dt);
//            //Open and query
//            if (m_oleConn == null)
//            {
//                Open();
//            }
//            if (m_oleConn.State != ConnectionState.Open)
//            {
//                throw new Exception("Connection cannot open error.");
//            }
//            if (SetSheetQuerySelect() == false)
//            {
//                return null;
//            }

//            //Fill table
//            OleDbDataAdapter oleAdapter = SetSheetQueryAdapter(dtChanges);

//            oleAdapter.Update(dtChanges);
//            //Clean up
//            m_oleCmdSelect.Dispose();
//            m_oleCmdSelect = null;
//            oleAdapter.Dispose();
//            oleAdapter = null;
//            if (KeepConnectionOpen == false)
//            {
//                Close();
//            }
//            return a_dt;
//        }

//        #region Get/Set Single Value
//        public void SetSingleCellRange(string a_strCell)
//        {
//            m_strSheetRange = a_strCell + ":" + a_strCell;
//        }

//        public object GetValue(string a_strCell)
//        {
//            SetSingleCellRange(a_strCell);
//            object objValue = null;
//            //Open and query
//            if (m_oleConn == null)
//            {
//                Open();
//            }
//            if (m_oleConn.State != ConnectionState.Open)
//            {
//                throw new Exception("Connection is not open error.");
//            }

//            if (SetSheetQuerySelect() == false)
//            {
//                return null;
//            }
//            objValue = m_oleCmdSelect.ExecuteScalar();

//            m_oleCmdSelect.Dispose();
//            m_oleCmdSelect = null;
//            if (KeepConnectionOpen == false)
//            {
//                Close();
//            }
//            return objValue;
//        }

//        public void SetValue(string a_strCell, object a_objValue)
//        {
//            try
//            {
//                SetSingleCellRange(a_strCell);
//                //Open and query
//                if (m_oleConn == null)
//                {
//                    Open();
//                }
//                if (m_oleConn.State != ConnectionState.Open)
//                {
//                    throw new Exception("Connection is not open error.");
//                }

//                if (SetSheetQuerySingelValUpdate(a_objValue.ToString()) == false)
//                {
//                    return;
//                }
//                m_oleCmdUpdate.ExecuteNonQuery();

//                m_oleCmdUpdate.Dispose();
//                m_oleCmdUpdate = null;
//                if (KeepConnectionOpen == false)
//                {
//                    Close();
//                }
//            }
//            finally
//            {
//                if (m_oleCmdUpdate != null)
//                {
//                    m_oleCmdUpdate.Dispose();
//                    m_oleCmdUpdate = null;
//                }
//            }
//        }
//        #endregion

//        #endregion

//        public

//            #region Dispose / Destructor
//            void Dispose()
//        {
//            if (m_oleConn != null)
//            {
//                m_oleConn.Dispose();
//                m_oleConn = null;
//            }
//            if (m_oleCmdSelect != null)
//            {
//                m_oleCmdSelect.Dispose();
//                m_oleCmdSelect = null;
//            }
//            // Dispose of remaining objects.
//        }
//        #endregion
//    }
//}

