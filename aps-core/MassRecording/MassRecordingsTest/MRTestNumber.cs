using System.Reflection;

namespace MassRecordingsTest;

public class MRTestNumber
{
    /// <summary>
    /// Sets the number of test recordings in unit test.
    /// </summary>
    public int MRTestCount()
    {
        Type typeOfMrTest = typeof(MrTest);
        // Get the test methods.
        MethodInfo[] methodInfos = typeOfMrTest.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        return methodInfos.Length;
    }
}