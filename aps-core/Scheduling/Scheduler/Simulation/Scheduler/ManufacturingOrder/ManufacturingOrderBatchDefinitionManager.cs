using PT.APSCommon;
using PT.APSCommon.Collections;

namespace PT.Scheduler.Simulation;

[Obsolete("Class is obsolete do not use, kept around for backward compatibility")]
public class ManufacturingOrderBatchDefinitionManager : CustomSortedList<ManufacturingOrderBatchDefinition>, /*AfterRestoreReferences.IAfterRestoreReferences,*/ IPTSerializable
{
    private readonly BaseIdGenerator m_idGen;

    #region list maintenance
    protected override ManufacturingOrderBatchDefinition CreateInstance(IReader a_reader)
    {
        return new ManufacturingOrderBatchDefinition(a_reader);
    }

    internal bool Contains(string a_batchName)
    {
        return GetValue(a_batchName) != null;
    }
    #endregion

    #region IPTSerializable Members
    public ManufacturingOrderBatchDefinitionManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader, new MOBatchDefComparer())
    {
        m_idGen = a_idGen;

        if (a_reader.VersionNumber >= 410)
        {
            // Nothin to do. Base class will read out the list
        }
    }

//    internal ManufacturingOrderBatchDefinitionManager(BaseIdGenerator a_idGen)
//        : base(new MOBatchDefComparer())
//    {
//        m_idGen = a_idGen;
//    }

//#if DEBUG
//    internal ManufacturingOrderBatchDefinitionManager(BaseIdGenerator a_idGen, params ManufacturingOrderBatchDefinition[] a_mobDefs)
//        : base(new MOBatchDefComparer())
//    {
//        m_idGen = a_idGen;

//        for (int i = 0; i < a_mobDefs.Length; ++i)
//        {
//            Add(a_mobDefs[i]);
//        }
//    }
//#endif

    public const int UNIQUE_ID = 671;

    int IPTSerializable.UniqueId => UNIQUE_ID;
    #endregion

    //#region IAfterRestoreReferences
    //public void AfterRestoreReferences_1(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences_1, HashSet<object> a_processedAfterRestoreReferences_2)
    //{
    //    AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_1(a_serializationVersionNbr, m_idGen, ReadOnlyList, this, a_processedAfterRestoreReferences_1, a_processedAfterRestoreReferences_2);
    //}

    //public void AfterRestoreReferences_2(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences_1, HashSet<object> a_processedAfterRestoreReferences_2)
    //{
    //    AfterRestoreReferences.Helpers.IEnumerableHelperFor_AfterRestoreReferences_2(a_serializationVersionNbr, ReadOnlyList, this, a_processedAfterRestoreReferences_1, a_processedAfterRestoreReferences_2);
    //}
    //#endregion

    //internal ManufacturingOrderBatchDefinition GetMOBatchSetDefinition(string a_mobName)
    //{
    //    return GetValue(a_mobName);
    //}

    //#region Transmission handling
    //internal void Receive(ScenarioDetail a_sd, Transmissions.ManufacturingOrderBatchDefinitionSetT a_batchDefSetT)
    //{
    //    Dictionary<BaseId, ManufacturingOrderBatchDefinition> mobdIdDictionary = CreateMobdBaseIdDictionary();
    //    Validate(a_batchDefSetT, mobdIdDictionary);

    //    List<ManufacturingOrderBatchDefinition> newMobDefSet = new();
    //    IEnumerator<Transmissions.ManufacturingOrderBatchDefinitionT> moBatchDefEnum = a_batchDefSetT.GetEnumerator();

    //    while (moBatchDefEnum.MoveNext())
    //    {
    //        Transmissions.ManufacturingOrderBatchDefinitionT mobdT = moBatchDefEnum.Current;

    //        if (mobdT.m_id == BaseId.NULL_ID)
    //        {
    //            // new; the validation function verified the name didn't exist
    //            ManufacturingOrderBatchDefinition mobd = new(a_sd.IdGen.NextID(), mobdT);
    //            newMobDefSet.Add(mobd);
    //        }
    //        else
    //        {
    //            // update; the validation function verified it exists.
    //            ManufacturingOrderBatchDefinition mobd = mobdIdDictionary[mobdT.m_id];
    //            mobd.Update(mobdT);
    //            newMobDefSet.Add(mobd);
    //        }
    //    }

    //    SetCollectionListTo(newMobDefSet);
    //}

    //private Dictionary<BaseId, ManufacturingOrderBatchDefinition> CreateMobdBaseIdDictionary()
    //{
    //    Dictionary<BaseId, ManufacturingOrderBatchDefinition> idMobdDictionary = new();
    //    IEnumerator<ManufacturingOrderBatchDefinition> nameMobdEnum = GetEnumerator();

    //    while (nameMobdEnum.MoveNext())
    //    {
    //        ManufacturingOrderBatchDefinition mobdTemp = nameMobdEnum.Current;
    //        idMobdDictionary.Add(mobdTemp.Id, mobdTemp);
    //    }

    //    return idMobdDictionary;
    //}

    //private void Validate(Transmissions.ManufacturingOrderBatchDefinitionSetT a_batchDefSetT, Dictionary<BaseId, ManufacturingOrderBatchDefinition> a_mobdIdDictionary)
    //{
    //    a_batchDefSetT.Validate();

    //    // Verify the new entries don't have the names of existing entries.
    //    IEnumerator<Transmissions.ManufacturingOrderBatchDefinitionT> moBatchDefEnum = a_batchDefSetT.GetEnumerator();

    //    while (moBatchDefEnum.MoveNext())
    //    {
    //        Transmissions.ManufacturingOrderBatchDefinitionT mobdT = moBatchDefEnum.Current;

    //        if (mobdT.m_id != BaseId.NULL_ID && !a_mobdIdDictionary.ContainsKey(mobdT.m_id))
    //        {
    //            throw new Transmissions.ValidationException("2767", new object[] { mobdT.m_name });
    //        }
    //    }
    //}
    //#endregion

    public class MOBatchDefComparer : IKeyObjectComparer<ManufacturingOrderBatchDefinition>
    {
        public int Compare(ManufacturingOrderBatchDefinition x, ManufacturingOrderBatchDefinition y)
        {
            return CompareMOBatchDefs(x, y);
        }

        internal static int CompareMOBatchDefs(ManufacturingOrderBatchDefinition a_moBatchDef, ManufacturingOrderBatchDefinition a_anotherMoBatchDef)
        {
            return Comparison.Compare(a_moBatchDef.Name, a_anotherMoBatchDef.Name);
        }

        public object GetKey(ManufacturingOrderBatchDefinition a_mobDef)
        {
            return a_mobDef.Name;
        }
    }
}