using System.Collections;
using System.Reflection;
using System.Text;

// This was originally developed to compare the same ScenarioDetail to itself at the start of each simulation. The schedule
// was changing after each optimize. This helped determine that the JIT start ticks of activities were changing after each optimize.
// To compare scenario details, the following temporary changes were made to use this class:
// 1. At the start of each Serialize function under the scheduler folder in the scheduler project this call was added: LogDifferencesBetweenObjects.CompareStateHelper(this);
// 2. The following 3 lines were added to ScenarioDetai.Simulation() to trigger the Serialize at the start of each Simulate after line JobManager.ResetSimulationStateVariables2();
//                BinaryMemoryWriter bmw = new BinaryMemoryWriter(10);
//                Serialize(bmw);
//                LogDifferencesBetweenObjects.SaveState();
// 3. Comment out the body of Scenario.CreateUndoSet(), becuase it will probably call Serialize and overwrite the temporary files used to compare the ScenarioDetails.
//    In the future, this can be changed to a non static class, so instances of objects can be used and this step doesn't need to be performed. 
// 
// After performing 2 Optimizes the result will be the 3 files defined in the code below. DifferencesInStatesLog.txt shows the differences between
// the ScenarioDetails in the first 2 files. You'll see "/\/\" used as a separator in the log.

namespace PT.Common;

/// <summary>
/// Class that manages gathering and comparing object field and property values.
/// </summary>
public static class LogDifferencesBetweenObjects
{
    private const string c_defaultCurrentSimulationStatePath = "C:\\CurrentSimulationState.txt";
    private static string s_currentSimulationState = c_defaultCurrentSimulationStatePath;

    private const string c_previousSimulationStatePath = "C:\\PreviousSimulationState.txt";
    private static string s_previousSimulationState = c_previousSimulationStatePath;

    private const string c_DifferencesInStatesLogPath = "C:\\DifferencesInStatesLog.txt";
    private static string s_DifferencesInStatesLog = c_DifferencesInStatesLogPath;

    private static bool s_skipNextFlag;

    private static readonly StringBuilder s_currentSimulationString = new ();
    private static readonly StringBuilder s_previousSimulationString = new ();

    /// <summary>
    /// Uses reflection to get all of th evariable values and appends them to a string
    /// </summary>
    /// <param name="baseObject">Base Object</param>
    /// <param name="a_simulationState1FilePath">Current simulation state path</param>
    /// <param name="a_simulationState2FilePath">Previous simulation state path</param>
    /// <param name="a_comparisonResultsLogPath">Log file path</param>
    public static void CompareStateHelper(object baseObject,
                                          string a_simulationState1FilePath = c_defaultCurrentSimulationStatePath,
                                          string a_simulationState2FilePath = c_previousSimulationStatePath,
                                          string a_comparisonResultsLogPath = c_DifferencesInStatesLogPath)
    {
        s_currentSimulationState = a_simulationState1FilePath;
        s_previousSimulationState = a_simulationState2FilePath;
        s_DifferencesInStatesLog = a_comparisonResultsLogPath;

        GetState(baseObject);
    }

    private static void GetState(object o)
    {
        //Skip this object if it is null
        if (o == null)
        {
            return;
        }

        //Set what level of encapsulation to retrive
        BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        // | BindingFlags.Static | BindingFlags.GetField | BindingFlags.GetProperty;
        Type t;
        try
        {
            t = o.GetType();
        }
        catch
        {
            return;
        }

        FieldInfo[] f = t.GetFields(bf);
        //PropertyInfo[] p = t.GetProperties(bf);
        //MemberInfo[] m = t.GetMembers(bf);

        //Check if each member is an array and find their values recursively. Log the parent class, member name, and member value.
        foreach (FieldInfo fi in f)
        {
            object fieldValue = fi.GetValue(o);

            if (fieldValue != null && fieldValue.GetType() != typeof(string))
            {
                if (fi.FieldType.IsArray)
                {
                    IList fieldArray = (IList)fi.GetValue(o);
                    foreach (object oValue in fieldArray)
                    {
                        GetState(oValue);
                    }
                }
            }

            string name = fi.Name;
            string parentClass = t.FullName;

            string value;
            if (name == null || name == "")
            {
                name = "null";
            }

            if (fieldValue == null)
            {
                value = "null";
            }
            else
            {
                try
                {
                    value = fieldValue.ToString();
                }
                catch
                {
                    value = "Error: Unable to convert object value of type: " + fi.GetType() + " to string.";
                }
            }

            s_currentSimulationString.Append(parentClass + "::" + name + " /\\/\\ " + value + Environment.NewLine);
        }

        //Not used

        #region PropertyInfo members
        //foreach (PropertyInfo pi in p)
        //{
        //    string name = "";
        //    string value = "";
        //    object objectValue = null;

        //    if (!(pi.GetIndexParameters().Length > 0) && pi.GetType() != typeof(System.String))
        //    {
        //        try
        //        {
        //            objectValue = pi.GetValue(o, null);
        //        }
        //        catch
        //        {
        //            break;
        //        }
        //        name = pi.Name;
        //        if (name == null || name == "")
        //        {
        //            name = "null";
        //        }

        //        if (objectValue == null)
        //        {
        //            value = "null";
        //        }
        //        else
        //        {
        //            value = objectValue.ToString();
        //        }

        //        string parentClass = o.GetType().BaseType.FullName;

        //        sb1.Append(parentClass + "::" + name + " /\\/\\ " + value + Environment.NewLine);

        //    }
        //}
        #endregion
    }

    //Reads stringbuilders line by line and compares name/value pairs.
    private static void CompareStates()
    {
        StringBuilder CompareString = new (Environment.NewLine + "Comparing Scenarios " + DateTime.Now + Environment.NewLine);

        string sb1Line;
        string sb2Line;

        //Create stream readers to read the strings line by line
        StringReader sr1 = new (s_currentSimulationString.ToString());
        StringReader sr2 = new (s_previousSimulationString.ToString());

        while (true)
        {
            sb1Line = sr1.ReadLine();
            sb2Line = sr2.ReadLine();

            if ((sb1Line == null) | (sb2Line == null))
            {
                break;
            }

            if (sb1Line != sb2Line)
            {
                sb1Line.Replace(Environment.NewLine, "");
                sb2Line.Replace(Environment.NewLine, "");
                CompareString.Append(Environment.NewLine + sb1Line + Environment.NewLine + sb2Line + Environment.NewLine);
            }
        }

        System.IO.File.AppendAllText(s_DifferencesInStatesLog, CompareString.ToString());
    }

    //Save string builder string to file
    public static void SaveState()
    {
        if (!s_skipNextFlag)
        {
            //Clear files
            System.IO.File.Delete(s_currentSimulationState);
            System.IO.File.Delete(s_previousSimulationState);
            System.IO.File.WriteAllText(s_DifferencesInStatesLog, "");

            System.IO.File.AppendAllText(s_currentSimulationState, s_currentSimulationString.ToString());
            System.IO.File.AppendAllText(s_previousSimulationState, s_previousSimulationString.ToString());

            CompareStates();

            //Move current state to previous state
            s_previousSimulationString.Clear();
            s_previousSimulationString.Append(s_currentSimulationString.ToString());
            s_currentSimulationString.Clear();
            s_skipNextFlag = false;
        }
        //else
        //{
        //    sb1.Clear();
        //    skipNextFlag = false;
        //}
    }
}