using System.Collections;
using System.Reflection;

using PT.APSCommon;
using PT.Common.Extensions;

///--------------------------------------------------------------------------------------------------------------------
///1. Objects are deserialized.
///2. RestoreReferences is called to reconnect the system.
///3. AfterRestoreReferences is called to regenerate BaseIds.
///     There are two types of objects:
///     1. Manager. Will regenerate ids in all the objects it contains.
///        Will call AfterRestoreReferences on all BaseObjects.
///     2. Will call AfterRestoreReferences on any manager objects it contains.
///4. AfterRestoreReferences2 is called to reinsert references that use BaseIds.
///     1. Will reinsert all objects all objects it contains using the new BaseId.
///     2. Will call AfterRestoreReferences2 on any manager objects it contains.
///     
///--------------------------------------------------------------------------------------------------------------------
///
///Are there any classes that create data structures that store the resource id?
///
///Base Manager has a full sortedList by id. This affects plant, department and resource managers.
///
/// Get rid of TripleLongKey from the scheduler. See InternalResourceList.Add() for a usage.
/// EligibleResourceSet.public int Compare(object x, object y);thisis interesting because it uses get has code.
/// It's references in places like PlantEligibilityInfo.CreateTLK.
/// You can also look into getting rid of ResourceKey and BaseResourceKey.
/// FromRangeSetsManager uses ResourceKey.
/// 
/// ResourceManager has a set of ids.
/// 
/// EligibleResourceSet.Contains()
/// 
///----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
/// USAGE:
///    inherit from BaseIdObject
///    setup base serialization
///    setup base deserialization
///    accept a base id and pass it to the base constructor
/// Take a reference to the parent object in the constructor and serialization code
/// create ids for next objects.
/// setup calls to IAfterRestoreReferences
/// 
/// Recent examples of customized objects for this include the forecast objects and sales order objects.
/// other examples are users of class Helpers contained in this file.
///----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

namespace PT.Scheduler.AfterRestoreReferences;

internal interface IAfterRestoreReferences
{
    // ScenarioDetail implements this IAfterRestoreReferences.
    // Also see InternalActivityManager, BaseOperationManager, and  InventoryManager 
    // for examples of collections that implement this interface without inheriting from ScenarioDetailManager.
    // Implement
    // IAfterRestoreReferences
    // In both functions check the correct HashSet to determine whether the object has already been processed.
    // If not
    // 	add it to the set
    // 	use a helper PT.Scheduler.AfterRestoreReferences.Helpers.CallObjMembers_AfterRestoreReferences_1 or _2; other helpers already do this ...
    // store a reference to _idGen; all constructors; use it to generate ids when you create objects

    void AfterRestoreReferences_1(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2);
    void AfterRestoreReferences_2(int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2);
}

internal class MasterCopyManagerAttribute : Attribute { }

internal class Helpers
{
    private static readonly Type s_MasterCopyManagerAttribute_TypeOf = typeof(MasterCopyManagerAttribute);
    private static readonly Type s_IAfterRestoreReferences_TypeOf = typeof(IAfterRestoreReferences);

    internal static void CallObjMembers_AfterRestoreReferences_1(int serializationVersionNbr, object obj, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        CallObjMembers_AfterRestoreReferences(serializationVersionNbr, obj, ARRVersion.One, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    internal static void CallObjMembers_AfterRestoreReferences_2(int serializationVersionNbr, object obj, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        CallObjMembers_AfterRestoreReferences(serializationVersionNbr, obj, ARRVersion.Two, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    private enum ARRVersion { One, Two }

    private static void CallObjMembers_AfterRestoreReferences(int serializationVersionNbr, object obj, ARRVersion version, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        Type objType = obj.GetType();

        while (objType != null)
        {
            FieldInfo[] fiArray = objType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (FieldInfo field in fiArray)
            {
                Type fieldType = field.FieldType;
                Type interfaceRef = fieldType.GetInterface(s_IAfterRestoreReferences_TypeOf.Name);

                if (interfaceRef != null)
                {
                    IAfterRestoreReferences tmp = (IAfterRestoreReferences)field.GetValue(obj);

                    if (tmp != null)
                    {
                        switch (version)
                        {
                            case ARRVersion.One:
                                if (HasMasterCopyManagerAttribute(field))
                                {
                                    tmp.AfterRestoreReferences_1(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
                                }

                                break;

                            case ARRVersion.Two:
                                tmp.AfterRestoreReferences_2(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
                                break;
                        }
                    }
                }
            }

            objType = objType.BaseType;
        }
    }

    private static bool HasMasterCopyManagerAttribute(FieldInfo field)
    {
        object[] attribute = field.GetCustomAttributes(s_MasterCopyManagerAttribute_TypeOf, false);
        return attribute.Length > 0;
    }

    private static void OnEnumerablesCallAfterRestoreReferences_1(IEnumerable enumerable, int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        OnEnumerablesCallAfterRestoreReferences(enumerable, serializationVersionNbr, ARRVersion.One, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    private static void OnEnumerablesCallAfterRestoreReferences_2(IEnumerable enumerable, int serializationVersionNbr, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        OnEnumerablesCallAfterRestoreReferences(enumerable, serializationVersionNbr, ARRVersion.Two, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    private static void OnEnumerablesCallAfterRestoreReferences(IEnumerable enumerable, int serializationVersionNbr, ARRVersion version, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        IEnumerator enumerator = enumerable.GetEnumerator();

        while (enumerator.MoveNext())
        {
            IAfterRestoreReferences arr;

            if (enumerator is IDictionaryEnumerator)
            {
                IDictionaryEnumerator de = (IDictionaryEnumerator)enumerator;
                arr = (IAfterRestoreReferences)de.Value;
            }
            else
            {
                arr = enumerator.Current as IAfterRestoreReferences;

                if (arr == null)
                {
                    DictionaryEntry de = (DictionaryEntry)enumerator.Current;
                    arr = (IAfterRestoreReferences)de.Value;
                }
            }

            switch (version)
            {
                case ARRVersion.One:
                    arr.AfterRestoreReferences_1(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
                    break;

                case ARRVersion.Two:
                    arr.AfterRestoreReferences_2(serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
                    break;
            }
        }
    }

    private static void ReinsertObjects(bool resetIds, SortedList sl, BaseIdGenerator idGen)
    {
        object[] baseIdObjects = sl.GetArray();
        sl.Clear();

        for (int i = 0; i < baseIdObjects.Length; ++i)
        {
            BaseIdObject idObj = (BaseIdObject)baseIdObjects[i];

            if (resetIds)
            {
                idObj.Id = idGen.NextID();
            }

            sl.Add(idObj.Id, idObj);
        }
    }

    private static void ResetIds(Hashtable ht, BaseIdGenerator idGen, bool reinsertWithNewIds)
    {
        List<BaseIdObject> idList = new ();
        IDictionaryEnumerator de = ht.GetEnumerator();

        while (de.MoveNext())
        {
            BaseIdObject idObj = (BaseIdObject)de.Value;
            idObj.Id = idGen.NextID();
            idList.Add(idObj);
        }

        if (reinsertWithNewIds)
        {
            ht.Clear();

            for (int i = 0; i < idList.Count; ++i)
            {
                BaseIdObject idObj = idList[i];
                ht.Add(idObj.Id.Value, idObj);
            }
        }
    }

    //static void ResetIds(IEnumerable enumerableSet, BaseIdGenerator idGen, bool reinsertWithNewIds)
    //{
    //    //List<BaseIdObject> idList = new List<BaseIdObject>();
    //    //IDictionaryEnumerator de = enumerableSet.GetEnumerator();

    //    //while (de.MoveNext())
    //    //{
    //    //    BaseIdObject idObj = (BaseIdObject)de.Value;
    //    //    idObj.Id = idGen.NextID();
    //    //    idList.Add(idObj);
    //    //}

    //    //if (reinsertWithNewIds)
    //    //{
    //    //    enumerableSet.Clear();

    //    //    for (int i = 0; i < idList.Count; ++i)
    //    //    {
    //    //        BaseIdObject idObj = idList[i];
    //    //        enumerableSet.Add(idObj.Id.Value, idObj);
    //    //    }
    //    //}
    //}

    /// <summary>
    /// If you're enumerating a Dictionary or Hashtable, then it is assumed the Value contains the BaseIdObject whose Id will be reset.
    /// </summary>
    /// <param name="etor"></param>
    /// <param name="idGen"></param>
    private static void ResetListOfIds(IEnumerator etor, IIdGenerator idGen)
    {
        while (etor.MoveNext())
        {
            BaseIdObject idObj;

            if (etor is IDictionaryEnumerator)
            {
                IDictionaryEnumerator de = (IDictionaryEnumerator)etor;
                idObj = (BaseIdObject)de.Value;
            }
            else
            {
                idObj = (BaseIdObject)etor.Current;
            }

            idObj.Id = idGen.NextID();
        }
    }

    internal static void SortedIdListHelperFor_AfterRestoreReferences_1(int serializationVersionNbr, BaseIdGenerator idGen, SortedList sl, object manager, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        if (processedAfterRestoreReferences_1.Add(manager))
        {
            ReinsertObjects(true, sl, idGen);
            OnEnumerablesCallAfterRestoreReferences_1(sl, serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
            CallObjMembers_AfterRestoreReferences_1(serializationVersionNbr, manager, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    internal static void SortedIdListHelperFor_AfterRestoreReferences_2(int serializationVersionNbr, SortedList sl, object manager, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        if (processedAfterRestoreReferences_2.Add(manager))
        {
            ReinsertObjects(false, sl, null);
            OnEnumerablesCallAfterRestoreReferences_2(sl, serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
            CallObjMembers_AfterRestoreReferences_2(serializationVersionNbr, manager, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    internal static void NonIdHashtableHelperFor_AfterRestoreReferences_1(int serializationVersionNbr, BaseIdGenerator idGen, Hashtable ht, object manager, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2, bool reinsertWithNewIds)
    {
        if (processedAfterRestoreReferences_1.Add(manager))
        {
            ResetIds(ht, idGen, reinsertWithNewIds);
            OnEnumerablesCallAfterRestoreReferences_1(ht, serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
            CallObjMembers_AfterRestoreReferences_1(serializationVersionNbr, manager, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    //internal static void NonIdHashtableHelperFor_AfterRestoreReferences_1(int serializationVersionNbr, BaseIdGenerator idGen, IEnumerable enumerableSet, object manager, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2, bool reinsertWithNewIds)
    //{
    //    if (!processedAfterRestoreReferences_1.Contains(manager))
    //    {
    //        processedAfterRestoreReferences_1.Add(manager);

    //        ResetListOfIds(enumerableSet.GetEnumerator(), idGen);
    //        OnEnumerablesCallAfterRestoreReferences_1(enumerableSet, serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    //        CallObjMembers_AfterRestoreReferences_1(serializationVersionNbr, manager, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    //    }
    //}

    internal static void NonIdHashtableHelperFor_AfterRestoreReferences_2_without_reinserts(int serializationVersionNbr, Hashtable ht, object manager, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        if (processedAfterRestoreReferences_2.Add(manager))
        {
            OnEnumerablesCallAfterRestoreReferences_2(ht, serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
            CallObjMembers_AfterRestoreReferences_2(serializationVersionNbr, manager, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    /// <summary>
    /// If you're enumerating a Dictionary or Hashtable, then it is assumed the Value contains the BaseIdObject whose Id will be reset.
    /// </summary>
    /// <param name="serializationVersionNbr"></param>
    /// <param name="idGen"></param>
    /// <param name="enumerable"></param>
    /// <param name="manager"></param>
    /// <param name="processedAfterRestoreReferences_1"></param>
    /// <param name="processedAfterRestoreReferences_2"></param>
    internal static void IEnumerableHelperFor_AfterRestoreReferences_1(int serializationVersionNbr, IIdGenerator idGen, IEnumerable enumerable, object manager, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        if (processedAfterRestoreReferences_1.Add(manager))
        {
            ResetListOfIds(enumerable.GetEnumerator(), idGen);
            OnEnumerablesCallAfterRestoreReferences_1(enumerable, serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
            CallObjMembers_AfterRestoreReferences_1(serializationVersionNbr, manager, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    internal static void IEnumerableHelperFor_AfterRestoreReferences_2(int serializationVersionNbr, IEnumerable enumerable, object manager, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        if (processedAfterRestoreReferences_2.Add(manager))
        {
            OnEnumerablesCallAfterRestoreReferences_2(enumerable, serializationVersionNbr, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
            CallObjMembers_AfterRestoreReferences_2(serializationVersionNbr, manager, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    internal static void ResetContainedIds(int serializationVersionNbr, object o)
    {
        HashSet<object> processedAfterRestoreReferences_1 = new ();
        HashSet<object> processedAfterRestoreReferences_2 = new ();

        AfterRestoreReferences_1(serializationVersionNbr, o, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        AfterRestoreReferences_2(serializationVersionNbr, o, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
    }

    internal static void AfterRestoreReferences_1(int serializationVersionNbr, object o, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        if (processedAfterRestoreReferences_1.Add(o))
        {
            CallObjMembers_AfterRestoreReferences_1(serializationVersionNbr, o, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    internal static void AfterRestoreReferences_2(int serializationVersionNbr, object o, HashSet<object> processedAfterRestoreReferences_1, HashSet<object> processedAfterRestoreReferences_2)
    {
        if (processedAfterRestoreReferences_2.Add(o))
        {
            CallObjMembers_AfterRestoreReferences_2(serializationVersionNbr, o, processedAfterRestoreReferences_1, processedAfterRestoreReferences_2);
        }
    }

    //class HashSetHelper : IEqualityComparer<object>
    //{
    //    #region IEqualityComparer<object> Members

    //    bool IEqualityComparer<object>.Equals(object x, object y)
    //    {
    //        return object.ReferenceEquals(x, y);
    //    }

    //    int IEqualityComparer<object>.GetHashCode(object obj)
    //    {
    //        return obj.GetHashCode();
    //    }

    //    #endregion
    //}
}