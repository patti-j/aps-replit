using System.Reflection;

using PT.APSCommon;
using PT.Common.Exceptions;

namespace PT.PackageDefinitions.Extensions;

public static class Extensions
{
    /// <summary>
    /// Get all override elements and remove any element on the list that matches the override element's
    /// PackageObjectId
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_elementList"></param>
    public static void ProcessOverrides<T>(this List<T> a_elementList) where T : IPackageElement
    {
        Dictionary<string, int> coreElements = new ();
        List<string> nonCoreElements = new ();

        for (int i = a_elementList.Count - 1; i >= 0; i--)
        {
            if (a_elementList[i] is ICoreElement coreElement && !coreElement.Singleton && coreElement.AllowOverride)
            {
                coreElements.Add(a_elementList[i].PackageObjectId, i);
            }
            else
            {
                nonCoreElements.Add(a_elementList[i].PackageObjectId);
            }
        }

        foreach (KeyValuePair<string, int> coreElement in coreElements)
        {
            if (nonCoreElements.Contains(coreElement.Key))
            {
                a_elementList.RemoveAt(coreElement.Value);
            }
        }
    }

    /// <summary>
    /// Deserializes a stored ISettingData object into a known type. Use this to retrieve settings saved to a SettingData object
    /// </summary>
    /// <typeparam name="T">The known type of the setting</typeparam>
    /// <param name="a_settingData">The serializable SettingData information</param>
    /// <returns></returns>
    /// <exception cref="PTException">Will be thrown if a public IReader constructor is not found</exception>
    public static T DeserializeSettings<T>(this SettingData a_settingData) where T : ISettingData
    {
        ConstructorInfo constructorInfo = typeof(T).GetConstructor(new[] { typeof(IReader) });
        if (constructorInfo != null)
        {
            using (BinaryMemoryReader reader = new (a_settingData.Data))
            {
                return (T)constructorInfo.Invoke(new object[] { reader });
            }
        }

        throw new PTException($"Cannot deserialize setting data for type {typeof(T)}");
    }
}