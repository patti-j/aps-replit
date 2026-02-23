using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler.Simulation;

namespace PT.Scheduler;

// [BATCH_CODE]
public partial class BatchManager : IEnumerable<Batch>
{
    #region IPTSerializable Members
    internal BatchManager(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12521)
        {
            a_reader.Read(out int nbrOfBatches);
            for (int batchI = 0; batchI < nbrOfBatches; ++batchI)
            {
                Batch batch = new (a_reader);
                Add(batch);
            }
        }
        else if (a_reader.VersionNumber >= 12008)
        {
            a_reader.Read(out int nbrOfBatches);
            for (int batchI = 0; batchI < nbrOfBatches; ++batchI)
            {
                a_reader.Read(out int batchId);

                Batch batch = new (a_reader);
                Add(batch);
                
                if (batchId == 740) //TankBatch.UniqueId
                {
                    //Old tank batch
                    if (a_reader.VersionNumber >= 12308)
                    {
                        a_reader.Read(out long endOfStorageTicks);
                        a_reader.Read(out long endOfStoragePostProcessingTicks);
                        new RequiredSpan(a_reader);
                        a_reader.Read(out long endOfStorageCleanTicks);
                        new RequiredSpanPlusClean(a_reader);

                        batch.EndOfStorageTicks = endOfStorageTicks;
                        batch.CleanEndTicks = endOfStorageCleanTicks;
                    }
                    else if (a_reader.VersionNumber >= 1)
                    {
                        a_reader.Read(out long endOfStorageTicks);
                        batch.EndOfStorageTicks = endOfStorageTicks;
                        batch.CleanEndTicks = endOfStorageTicks;
                    }
                }
                else
                {
                    //not tank batch, but we need to still set the new batch field.
                    batch.EndOfStorageTicks = batch.PostProcessingEndTicks;
                }
            }
        }
        else
        {
            a_reader.Read(out int nbrOfBatches);
            for (int batchI = 0; batchI < nbrOfBatches; ++batchI)
            {
                Batch batch = new (a_reader);
                Add(batch);
            }
        }
    }

    public const int UNIQUE_ID = 638;

    public int UniqueId => UNIQUE_ID;

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Count);

        using IEnumerator<Batch> etr = GetEnumerator();
        while (etr.MoveNext())
        {
            etr.Current.Serialize(a_writer);
        }
    }
    #endregion

    internal BatchManager() { }

    /// <summary>
    /// Link batches to activities and blocks.
    /// </summary>
    /// <param name="a_jobs">ScenarioDetail jobs.</param>
    /// <param name="a_plants">ScenarioDetail plants.</param>
    internal void RestoreReferences(JobManager a_jobs, PlantManager a_plants)
    {
        List<Resource> resources = a_plants.GetResourceArrayList();

        Dictionary<BaseId, Batch> batchDictionary = new ();

        // Link batches and activities.
        for (int batchI = 0; batchI < Count; ++batchI)
        {
            Batch batch = this[batchI];
            batch.RestoreReferences(a_jobs);
            batchDictionary.Add(batch.Id, batch);
        }

        // Link batches and blocks.
        for (int resI = 0; resI < resources.Count; ++resI)
        {
            Resource res = resources[resI];
            res.RestoreReferences_2_batches(batchDictionary);
        }
    }

    private readonly List<Batch> m_batches = new ();

    private void Add(Batch a_batch)
    {
        m_batches.Add(a_batch);
    }

    /// <summary>
    /// This isn't serialized and is reset at the start of each simulation. So batch ids aren't unique across all system objects.
    /// </summary>
    [Common.Attributes.DebugLogging(Common.Attributes.EDebugLoggingType.None)]
    private readonly BaseIdGenerator m_batchIdManager = new (); // [BATCH]

    /// <summary>
    /// Remove all batches that no longer have any activities scheduled in them. It might be necessary to call this function when
    /// an activity is unscheduled.
    /// </summary>
    internal void RemoveDeadBatches()
    {
        for (int batchI = Count - 1; batchI >= 0; --batchI)
        {
            if (m_batches[batchI].Empty())
            {
                m_batches.RemoveAt(batchI);
            }
        }
    }

    internal int Count => m_batches.Count;

    internal Batch this[int a_idx] => m_batches[a_idx];

    #region DEBUG
    #if DEBUG

    internal void TestBatches()
    {
        foreach (Batch batch in m_batches)
        {
            batch.EmptyTest();
        }
    }

    #endif
    #endregion

    /// <summary>
    /// For debugging purposes. Get an array that describes the individual batches.
    /// </summary>
    /// <returns></returns>
    internal string[] GetBatchToStringArray()
    {
        string[] batchToStrings = new string[Count];
        int i = 0;
        foreach (Batch batch in m_batches)
        {
            batchToStrings[i++] = batch.ToString();
        }

        return batchToStrings;
    }

    public IEnumerator<Batch> GetEnumerator()
    {
        return m_batches.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

//        bool m_lastSimBatches;
    /// <summary>
    /// These are now batches from the last simulation and are being used to help keep batches together.
    /// Link activities to the batches they were originally scheduled in.
    /// </summary>
    internal void SetLastSimBatchesOfActivities()
    {
//            m_lastSimBatches = true;
        for (int batchI = 0; batchI < Count; ++batchI)
        {
            Batch batch = this[batchI];
            foreach (InternalActivity act in batch)
            {
                act.SimData.LastSimBatchByteIndex = batchI;
            }
        }
    }

    internal Batch GetLastSimBatch(InternalActivity a_act)
    {
        return this[a_act.SimData.LastSimBatchByteIndex];
    }

    public override string ToString()
    {
        return string.Format("{0} batches".Localize(), Count);
    }
}