//using System;
//using System.Runtime.InteropServices;

//using Microsoft.Office.Interop.Excel;

//namespace PT.Common.Excel
//{
//    public class ExcelReaderInterop : IDisposable
//    {
//        public ExcelReaderInterop()
//        {
//            m_application = new Application();
//        }

//        /// <summary>
//        /// Excel application object
//        /// </summary>
//        private Application m_application;

//        private Microsoft.Office.Interop.Excel.Range m_excelRange;
//        private Workbooks m_workBooks;
//        private Workbook m_workBook;
//        private Sheets m_sheets;
//        private Worksheet m_sheet;

//        /// <summary>
//        /// Open the file path received in Excel. Then, open the workbook
//        /// within the file. Send the workbook to the next function, the internal scan
//        /// function. Will throw an exception if a file cannot be found or opened.
//        /// </summary>
//        public object[,] ExcelOpenSpreadsheets(string a_fileName, string a_sheetName = "")
//        {
//            try
//            {
//                m_application.Visible = false;
//                //
//                // This mess of code opens an Excel workbook. I don't know what all
//                // those arguments do, but they can be changed to influence behavior.
//                //
//                m_workBooks = m_application.Workbooks;
//                m_workBook = m_workBooks.Open(a_fileName,
//                    1, true, Type.Missing, Type.Missing,
//                    Type.Missing, false, Type.Missing, Type.Missing,
//                    false, Type.Missing, Type.Missing, Type.Missing,
//                    Type.Missing, Type.Missing);

//                //
//                // Pass the workbook to a separate function. This new function
//                // will iterate through the worksheets in the workbook.
//                //
//                Object[,] table = ExcelScanIntenal(m_workBook, a_sheetName);

//                return table;
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Could not read excel.", ex);
//            }
//        }

//        /// <summary>
//        /// Scan the selected Excel workbook and store the information in the cells
//        /// for this workbook in an object[,] array. Then, call another method
//        /// to process the data.
//        /// </summary>
//        private object[,] ExcelScanIntenal(Workbook a_workbook, string a_sheetName = "")
//        {
//            object[,] valueArray = null;
//            m_sheets = a_workbook.Sheets;
//            int count = m_sheets.Count;
//            for (int i = 1; i < count; i++)
//            {
//                m_sheet = (Worksheet)m_sheets[i];
//                if (m_sheet.Name == a_sheetName)
//                {
//                    m_excelRange = m_sheet.UsedRange;
//                    valueArray = (object[,])m_excelRange.get_Value(XlRangeValueDataType.xlRangeValueDefault);
//                    break;
//                }
//            }

//            return valueArray;
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//        }

//        protected virtual void Dispose(bool a_dispose)
//        {
//            if (a_dispose)
//            {
//                m_workBooks.Close();
//                Marshal.ReleaseComObject(m_workBooks);
//                m_workBooks = null;
//                m_excelRange = null;
//                if (m_application != null)
//                {
//                    m_application.Quit();
//                }
//                Marshal.ReleaseComObject(m_application);
//                m_application = null;
//            }
//        }
//    }
//}

