using System.Reflection;

namespace PT.Scheduler;

/// <summary>
/// Summary description for ScenarioRef.
/// </summary>
internal interface IScenarioRef
{
    void SetReferences(Scenario scenario, ScenarioDetail scenarioDetail);
}

internal class ScenarioRef
{
    public static void SetRef(IScenarioRef iRef, Scenario scenario, ScenarioDetail scenarioDetail)
    {
        iRef.SetReferences(scenario, scenarioDetail);

        Type t = iRef.GetType();
        FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (FieldInfo field in fields)
        {
            Type fieldType = field.FieldType;

            Type[] fieldInterfaceTypes = fieldType.GetInterfaces();

            foreach (Type fieldInterfaceType in fieldInterfaceTypes)
            {
                if (fieldInterfaceType.FullName == typeof(IScenarioRef).FullName)
                {
                    IScenarioRef fieldRef = (IScenarioRef)field.GetValue(iRef);
                    fieldRef.SetReferences(scenario, scenarioDetail);
                }
            }
        }
    }
}