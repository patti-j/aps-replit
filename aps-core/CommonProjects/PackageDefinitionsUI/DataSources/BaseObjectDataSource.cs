using System.Collections.Concurrent;

using PT.APSCommon;
using PT.Common.Debugging;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.DataSources;

internal class BaseObjectDataSource<TKeyType, TPropertyType> : IBaseObjectDataSource<TKeyType>
{
    protected readonly BaseId ScenarioId;
    private readonly ScenarioDetailCacheLock m_sdCache;
    private readonly Dictionary<string, ConcurrentDictionary<TKeyType, object>> m_propToObjectValueDict = new ();
    private readonly object m_propToObjectValueLock = new ();
    protected readonly Dictionary<string, TPropertyType> m_propLookup = new ();
    private readonly Dictionary<string, IScenarioDetailProperty> m_scenarioDetailPropsToInitialize = new ();
    protected readonly Dictionary<string, IUserDetailProperty> m_userDetailPropsToInitialize = new ();

    internal BaseObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache)
    {
        ScenarioId = a_scenarioId;
        m_sdCache = a_sdCache;
    }

    protected void ClearData()
    {
        m_propToObjectValueDict.Clear();
        m_scenarioDetailPropsToInitialize.Clear();
        m_userDetailPropsToInitialize.Clear();
        foreach (TPropertyType property in m_propLookup.Values)
        {
            if (property is IScenarioDetailProperty sdProp)
            {
                m_scenarioDetailPropsToInitialize.Add(sdProp.PropertyName, sdProp);
            }

            if (property is IUserDetailProperty userProp)
            {
                m_userDetailPropsToInitialize.Add(userProp.PropertyName, userProp);
            }
        }
        //TODO: Start background cache
    }

    private object GetValueInternal(string a_property, TKeyType a_objectId, ScenarioDetail a_sd)
    {
        if (m_scenarioDetailPropsToInitialize.TryGetValue(a_property, out IScenarioDetailProperty sdProp))
        {
            sdProp.Reload(a_sd);
            m_scenarioDetailPropsToInitialize.Remove(a_property);
        }

        return GetValue(a_property, a_objectId, a_sd);
    }

    public virtual object GetValue(string a_property, TKeyType a_objectId, ScenarioDetail a_sd)
    {
        throw new DebugException("This must be overriden");
    }

    private bool m_dataUnavailable;

    public void SignalScenarioActivated()
    {
        ClearData();
    }

    public virtual void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        //Check for related data changes and clear the cache if needed
        DebugException.ThrowInDebug("Datachanges is not handled");
    }

    public void SignalSimulationCompleted()
    {
        m_dataUnavailable = false;
        ClearData();
    }

    public void SignalSimulationStarted()
    {
        m_dataUnavailable = true;
    }

    private int m_readLockAttemptDuration;

    public List<LookUpValueStruct> GetValueBlock(List<TKeyType> a_objectIds, string a_property)
    {
        List<LookUpValueStruct> returnValues = new (a_objectIds.Count);
        int index = 0;
        while (index < a_objectIds.Count)
        {
            if (m_propToObjectValueDict.TryGetValue(a_property, out ConcurrentDictionary<TKeyType, object> objectValues))
            {
                if (objectValues.TryGetValue(a_objectIds[index], out object value))
                {
                    LookUpValueStruct lookUpValue = new (value);
                    returnValues.Add(lookUpValue);
                }
                else
                {
                    break;
                }
            }
            else
            {
                m_propToObjectValueDict.Add(a_property, new ConcurrentDictionary<TKeyType, object>());
                break;
            }

            index++;
        }

        if (index != a_objectIds.Count)
        {
            //Need to continue with ScenarioDetail
            try
            {
                if (SystemController.Sys.ScenariosLock.IsWriteLockHeld)
                {
                    m_readLockAttemptDuration -= 10; //This data is changing, don't hold up the UI and the lock
                    if (m_readLockAttemptDuration <= 0)
                    {
                        return new List<LookUpValueStruct> { LookUpValueStruct.EmptyLookUpValue };
                    }
                }
                else
                {
                    m_readLockAttemptDuration = 100;
                }

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, m_readLockAttemptDuration))
                {
                    Scenario scenario = sm.Find(ScenarioId);
                    using (scenario.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, m_readLockAttemptDuration))
                    {
                        //Cache all properties for this object
                        while (index < a_objectIds.Count)
                        {
                            if (m_propToObjectValueDict.TryGetValue(a_property, out ConcurrentDictionary<TKeyType, object> objectValues))
                            {
                                if (objectValues.TryGetValue(a_objectIds[index], out object cachedValue))
                                {
                                    LookUpValueStruct lookUpValue = new (cachedValue);
                                    returnValues.Add(lookUpValue);
                                }
                                else
                                {
                                    object value = GetValueInternal(a_property, a_objectIds[index], sd);
                                    objectValues.TryAdd(a_objectIds[index], value);
                                    LookUpValueStruct lookUpValue = new (value);
                                    returnValues.Add(lookUpValue);
                                }
                            }

                            index++;
                        }
                    }
                }
            }
            catch (AutoTryEnterException e)
            {
                //TODO:
            }
        }

        return returnValues;
    }

    public LookUpValueStruct GetValue(TKeyType a_object, string a_property, ScenarioDetail a_sd = null)
    {
        if (m_propToObjectValueDict.TryGetValue(a_property, out ConcurrentDictionary<TKeyType, object> objectToValue))
        {
            if (objectToValue.TryGetValue(a_object, out object value))
            {
                LookUpValueStruct lookUpValue = new (value);
                return lookUpValue;
            }

            //If data not available, return null
            if (m_dataUnavailable && a_sd == null)
            {
                return LookUpValueStruct.EmptyLookUpValue;
            }

            //TODO: Maybe lock per column...
            //Go find the value
            //object lockObject = m_lockDict[columnName];
            //lock (lockObject)
            //{
            //    //Value could now be cached
            //    if (m_rowToDictMapping.TryGetValue(a_rowIdx, out Dictionary<string, object> cachedValues))
            //    {
            //        if (cachedValues.TryGetValue(columnName, out object updatedValue))
            //        {
            //            return updatedValue;
            //        }
            //    }

            try
            {
                if (a_sd != null)
                {
                    try
                    {
                        value = GetValueInternal(a_property, a_object, a_sd);
                        objectToValue.TryAdd(a_object, value);
                        LookUpValueStruct lookUpValue = new (value);
                        return lookUpValue;
                    }
                    catch (Exception e)
                    {
                        return LookUpValueStruct.EmptyLookUpValue;
                    }
                }

                //Check for cached reference
                lock (m_sdLock)
                {
                    if (m_sdList.Count > 0)
                    {
                        try
                        {
                            value = GetValueInternal(a_property, a_object, m_sdList[0].ScenarioDetail);
                            objectToValue.TryAdd(a_object, value);
                            LookUpValueStruct lookUpValue = new (value);
                            return lookUpValue;
                        }
                        catch (Exception e)
                        {
                            return LookUpValueStruct.EmptyLookUpValue;
                        }
                    }
                }


                //Check for cached reference
                ScenarioDetailCacheLock.ScenarioDetailCacheValue cacheValue = m_sdCache.GetScenarioDetailCache();
                if (cacheValue.CacheFound)
                {
                    try
                    {
                        value = GetValueInternal(a_property, a_object, cacheValue.ScenarioDetail);
                        objectToValue.TryAdd(a_object, value);
                        LookUpValueStruct lookUpValue = new (value);

                        return lookUpValue;
                    }
                    catch (Exception e)
                    {
                        return LookUpValueStruct.EmptyLookUpValue;
                    }
                    finally
                    {
                        m_sdCache.ReturnScenarioDetailCache();
                    }
                }

                m_sdCache.InitCache();

                using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, 10))
                {
                    Scenario scenario = sm.Find(ScenarioId);
                    AutoDisposer tryEnter = scenario.ScenarioDetailLock.TryEnterReadNoException(out ScenarioDetail sd, out bool writeLockPending, AutoExiter.UiQuickWait);
                    if (tryEnter != null)
                    {
                        try
                        {
                            value = GetValueInternal(a_property, a_object, sd);
                            objectToValue.TryAdd(a_object, value);
                            LookUpValueStruct lookUpValue = new (value);
                            return lookUpValue;
                        }
                        catch (Exception e)
                        {
                            return LookUpValueStruct.EmptyLookUpValue;
                        }
                        finally
                        {
                            tryEnter.Dispose();
                        }

                        //Cache all properties for this object
                        //CacheObject(a_object, sd);
                    }
                }
            }
            catch (AutoTryEnterException e)
            {
                //TODO
            }

            if (objectToValue.TryGetValue(a_object, out object nowCachedValue))
            {
                LookUpValueStruct lookUpValue = new (nowCachedValue);
                return lookUpValue;
            }
            //TODO: We missed something
        }
        else
        {
            lock (m_propToObjectValueLock) //Make sure multiple threads don't try to add the key
            {
                if (!m_propToObjectValueDict.ContainsKey(a_property))
                {
                    //Property was not added yet, now get the value
                    m_propToObjectValueDict.Add(a_property, new ConcurrentDictionary<TKeyType, object>());
                }
            }

            return GetValue(a_object, a_property, a_sd);
        }

        return LookUpValueStruct.EmptyLookUpValue;
    }

    private readonly object m_sdLock = new ();
    private readonly List<ScenarioDetailCache> m_sdList = new ();

    public void AddScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        lock (m_sdLock)
        {
            m_sdList.Add(a_cache);
        }
    }

    public void DeleteScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        lock (m_sdLock)
        {
            m_sdList.Remove(a_cache);
        }
    }

    public IObjectProperty GetPropertyFromColumnKey(string a_key)
    {
        throw new DebugException("This must be overriden");
    }
}