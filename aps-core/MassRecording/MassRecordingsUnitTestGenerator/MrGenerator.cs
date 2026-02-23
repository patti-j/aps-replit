using System.Data;
using System.Text;

using MassRecordings;

using PT.Common.Extensions;
using PT.Common.SqlServer;

namespace MassRecordingUnitTestGenerator;

internal class UnitTestCodeGenerator
{
    private DataTable m_runTimesTable;
    private DataTable m_hostConfigTable;
    private string m_dbConnectionString;
    private string m_sharedFolderPath;
    private readonly CommandLineArgumentsHelper m_argsHelper;

    internal UnitTestCodeGenerator(CommandLineArgumentsHelper a_argsHelper)
    {
        m_argsHelper = a_argsHelper;
    }

    /// <summary>
    /// Generate a UnitTest for each recording in the shared PT folder
    /// </summary>
    internal void GenerateTestCode()
    {
        SimpleConfiguration config = new ();
        m_dbConnectionString = config.LoadValue("DBConnectionString");
        m_sharedFolderPath = config.LoadValue("MasterCopy");

        StringBuilder sb = new ();

        AppendFileHeader(sb);

        Console.WriteLine("Browsing MasterCopy directory: " + m_sharedFolderPath);
        List<string[]> files = new ();

        //Get all the subdirectories given the shared folder path
        List<string> subDir = new (Directory.EnumerateDirectories(m_sharedFolderPath));
        int fileCount = 0;

        foreach (string s in subDir)
        {
            //Only look inside the serialize number folders
            //The TryParse function checks all the subfolders in the shared folder path and returns true if the subfolder is an integer
            if (int.TryParse(s.Substring(s.LastIndexOf("\\") + 1), out int value))
            {
                //if the current serialization number is greater than the Number given by the serialization folder, extract all the recordings from the folder
                if (Serialization.VersionNumber >= value)
                {
                    string[] temp = Directory.GetFiles(s, "*.ZIP", SearchOption.AllDirectories);
                    fileCount += temp.Length;
                    files.Add(temp);
                }
            }
        }

        //string[] files = Directory.GetFiles(m_sharedFolderPath, "*.ZIP", SearchOption.AllDirectories);
        Console.WriteLine("Found Files: " + fileCount);

        try
        {
            Console.WriteLine("Gathering runtime information from database.");
            MassRecordings.SqlStrings.TableDefinitions.RunInstance runInstanceDt = new ();
            DatabaseConnections dbConnector = new (m_dbConnectionString);
            DataTable dt = dbConnector.SelectSQLTable(SqlStrings.GetConfigNames());
            List<string> hostNames = new ();
            bool validateInput = true;

            if (hostNames.Count > 0)
            {
                //If a command line argument was passed, don't ask user to select run location 
                if (!m_argsHelper.LocalHostName.ArgumentFound)
                {
                    if (hostNames.Count == 1)
                    {
                        m_runTimesTable = dbConnector.SelectSQLTable(SqlStrings.GetAverageRunTimeQuery(hostNames[0]));
                    }
                    else
                    {
                        Console.WriteLine("Please select one of the following Host Names to choose which run location to use to calculate average speed runs: ");
                        foreach (DataRow row in dt.Rows)
                        {
                            hostNames.Add(Convert.ToString(row[runInstanceDt.RunLocation]));
                            Console.WriteLine("\t" + Convert.ToString(row[runInstanceDt.RunLocation]));
                        }

                        Console.WriteLine();

                        while (validateInput)
                        {
                            string choice = Console.ReadLine();

                            foreach (string hostName in hostNames)
                            {
                                if (hostName == choice)
                                {
                                    validateInput = false;
                                    m_runTimesTable = dbConnector.SelectSQLTable(SqlStrings.GetAverageRunTimeQuery(choice));
                                    break;
                                }
                            }

                            if (validateInput)
                            {
                                Console.WriteLine("Please retype the HostName: ");
                            }
                        }
                    }
                }
                else
                {
                    m_runTimesTable = dbConnector.SelectSQLTable(SqlStrings.GetAverageRunTimeQuery(Environment.MachineName));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: ===============================");
            Console.WriteLine("Failed to retrieve runtime information from database.");
            Console.WriteLine();
        }

        Console.WriteLine("Generating unit tests.");

        //Generate a unit test per recording
        foreach (string[] sourcefile in files)
        {
            foreach (string s in sourcefile)
            {
                string sourcePath = "";
                string folderName = "";
                List<string> subDirectories = new ();

                try
                {
                    //Create a list of parent directories that can be used as categories
                    sourcePath = s.Replace(m_sharedFolderPath + "\\", ""); //remove the shared folder path from string to use as part of the function name
                    folderName = sourcePath.Substring(0, sourcePath.IndexOf("\\", sourcePath.IndexOf("\\") + 1)); //extract serialization folder + customer folder
                    folderName = folderName.Substring(folderName.IndexOf('\\') + 1); //remove serialization folder name since function names can't start with a number.
                    subDirectories.Add(folderName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("SourcePath: {0}", sourcePath);
                    Console.WriteLine("SourceFile: {0}", sourcefile);
                    Console.WriteLine("FolderName: {0}", folderName);

                    Console.WriteLine(e.GetExceptionFullMessage());
                }

                AppendFunction(sb, s, subDirectories);
            }
        }

        AppendFileFooter(sb);

        string mrRecordingsTest;

        //If command line argument was passed use argument value
        if (m_argsHelper.TestFileLocation.ArgumentFound)
        {
            mrRecordingsTest = m_argsHelper.TestFileLocation.Value;
        }
        else
        {
            string currentDirectory = Environment.CurrentDirectory;
            Console.WriteLine("Working Directory: " + currentDirectory);
            currentDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).ToString()).ToString()).ToString();
            Console.WriteLine("Solution Directory: " + currentDirectory);
            mrRecordingsTest = Path.Combine(currentDirectory, "MassRecordingsTest\\MrTestRecordings.cs");
        }

        Console.WriteLine("Verifying target file: " + mrRecordingsTest);

        //Clear the text file before running again.
        FileInfo fi = new (mrRecordingsTest);

        //Check if the file exists and is writable.
        if (fi.Exists)
        {
            Console.WriteLine("Found File. Deleting...");
            File.Delete(mrRecordingsTest);
            Console.WriteLine("File Deleted");
        }

        Console.WriteLine("Writing test file.");

        File.AppendAllText(mrRecordingsTest, sb.ToString());

        if (m_argsHelper.ClosePrompt.ArgumentFound)
        {
            Console.WriteLine("Done! ");
        }
        else
        {
            Console.WriteLine("Done! \nPress any key to close window.");
            Console.ReadLine();
        }
    }

    /// <summary>
    /// Appends code for a unit test method
    /// </summary>
    private void AppendFunction(StringBuilder a_sb, string a_sourcefile, List<string> a_subDirectories)
    {
        string filePath = Directory.GetParent(a_sourcefile).FullName; //get the file path 
        string customerPath = filePath.Replace(m_sharedFolderPath + "\\", "V"); //remove the shared folder path from string to use as part of the function name
        //customerPath = customerPath.Substring(customerPath.IndexOf("\\") + 1); //remove the serialization number folder
        string recording = Path.GetFileNameWithoutExtension(a_sourcefile); //get the scenario file name without the .zip extension
        string functionName = GetFunctionName(customerPath, recording); //make a unique function name

        a_sb.AppendFormat("\t\t[DataRow(@\"{0}\", \"{1}\")]", customerPath, recording);
        a_sb.AppendLine();
        a_sb.AppendLine("\t\t[DataTestMethod]");
        AppendCustomAttributes(a_sb, a_subDirectories, a_sourcefile);
        a_sb.AppendFormat("\t\tpublic void {0}(string a_path, string a_zip)", functionName);
        a_sb.AppendLine();
        a_sb.AppendLine("\t\t{");
        a_sb.AppendLine("\t\tTestProcessor processor = new TestProcessor(s_instanceConfig);");
        a_sb.AppendLine("\t\tprocessor.SetFullRecordingPath(a_path, a_zip);");
        a_sb.AppendLine("\t\tprocessor.Test();");
        a_sb.AppendLine("\t\tprocessor.CheckForErrors();");
        a_sb.AppendLine("\t\t}");
        a_sb.AppendLine();
    }

    /// <summary>
    /// Generates function attributes
    /// </summary>
    private void AppendCustomAttributes(StringBuilder a_sb, List<string> a_subDirectories, string a_sourceFile)
    {
        if (a_subDirectories.Count == 0)
        {
            return;
        }

        //The first path in the source folder
        a_sb.AppendLine($"\t\t[TestCategory(\"{a_subDirectories[a_subDirectories.Count - 1]}\")]");

        //if (a_subDirectories.Count > 1)
        //{
        //    a_sb.AppendLine($"\t\t[TestProperty(\"SubCategory\", \"{a_subDirectories[0]}\")]");
        //}

        string recordingPath = a_sourceFile.Replace(m_sharedFolderPath, "");
        recordingPath = recordingPath.Replace(".zip", "");
        recordingPath = recordingPath.TrimStart('\\');
        //Add speed traits
        if (m_runTimesTable != null)
        {
            foreach (DataRow row in m_runTimesTable.Rows)
            {
                if ((string)row["RecordingPath"] == recordingPath)
                {
                    string runSpeed = GetRunSpeedTrait(Convert.ToInt32(row["Seconds"]));
                    a_sb.AppendLine($"\t\t[TestCategory(\"{runSpeed}\")]");
                }
            }
        }
    }

    /// <summary>
    /// Returns a trait category based on number of seconds
    /// </summary>
    private string GetRunSpeedTrait(int a_seconds)
    {
        if (a_seconds < TimeSpan.FromSeconds(15).TotalSeconds)
        {
            return "Fast";
        }

        if (a_seconds < TimeSpan.FromMinutes(1).TotalSeconds)
        {
            return "Normal";
        }

        if (a_seconds < TimeSpan.FromMinutes(10).TotalSeconds)
        {
            return "Slow";
        }

        return "Very Slow";
    }

    /// <summary>
    /// Appends code for the unit test file header
    /// </summary>
    private void AppendFileHeader(StringBuilder a_sb)
    {
        a_sb.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
        a_sb.AppendLine();
        a_sb.AppendLine("namespace MassRecordingsTest");
        a_sb.AppendLine("{");
        a_sb.AppendLine("\t[TestClass]");
        a_sb.AppendLine("\tpublic partial class MrTest");
        a_sb.AppendLine("\t{");
        a_sb.AppendLine("\t\t/// <summary>");
        a_sb.AppendLine("\t\t/// Pick a customer and a recording to run and launch the recording helper");
        a_sb.AppendLine("\t\t/// </summary>");
        a_sb.AppendLine("\t\t/// ");
    }

    /// <summary>
    /// Appends code for the unit test file footer
    /// </summary>
    private void AppendFileFooter(StringBuilder a_sb)
    {
        a_sb.AppendLine("\t}");
        a_sb.AppendLine("}");
    }

    /// <summary>
    /// Generate a unique function name given the customer and recording names
    /// </summary>
    /// <param name="a_customerName"></param>
    /// <param name="a_recording"></param>
    /// <returns></returns>
    private static string GetFunctionName(string a_customerName, string a_recording)
    {
        a_customerName = GetCleanString(a_customerName); //clean string
        a_recording = GetCleanString(a_recording); //clean string

        string funcName = a_customerName + "_" + a_recording;

        return funcName;
    }

    /// <summary>
    /// Clean the string to remove any characters not allowed in function declarations
    /// </summary>
    private static string GetCleanString(string a_newString)
    {
        a_newString = a_newString.Replace("\\", "_");
        a_newString = a_newString.Replace("-", "");
        a_newString = a_newString.Replace(" ", "");
        a_newString = a_newString.Replace("'", "");
        a_newString = a_newString.Replace(".", "");
        a_newString = a_newString.Replace(",", "");
        a_newString = a_newString.Replace("(", "");
        a_newString = a_newString.Replace(")", "");
        a_newString = a_newString.Replace("&", "");
        a_newString = a_newString.Replace("$", "");
        a_newString = a_newString.Replace("*", "");

        return a_newString;
    }
}