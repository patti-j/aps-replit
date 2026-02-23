using System.Collections;

using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores a collection of AlternatePaths.
/// </summary>
public partial class AlternatePathsCollection : ICopyTable, IEnumerable<AlternatePath>
{
    #region IPTSerializable
    internal AlternatePathsCollection(IReader reader, ManufacturingOrder mo)
    {
        reader.Read(out int count);

        for (int i = 0; i < count; ++i)
        {
            AlternatePath path = new (reader, mo);
            Add(path);
        }
    }

    internal void Serialize(IWriter writer)
    {
        writer.Write(Count);
        for (int i = 0; i < Count; ++i)
        {
            this[i].Serialize(writer);
        }
    }
    #endregion

    #region Construction
    internal AlternatePathsCollection() { }
    #endregion

    #region Properties and Methods
    /// <summary>
    /// A set of alternate paths.
    /// </summary>
    private readonly List<AlternatePath> m_pathsList = new ();

    /// <summary>
    /// Add an alternate path.
    /// </summary>
    /// <param name="a_alternatePath">The path to add.</param>
    /// <returns></returns>
    public AlternatePath Add(AlternatePath a_alternatePath)
    {
        m_pathsList.Add(a_alternatePath);
        return a_alternatePath;
    }

    /// <summary>
    /// Remove an alternate path.
    /// </summary>
    /// <param name="a_index">Index of a path.</param>
    public void Remove(int a_index)
    {
        m_pathsList.RemoveAt(a_index);
    }

    internal void Remove(AlternatePath a_path)
    {
        m_pathsList.Remove(a_path);
    }

    public Type ElementType => typeof(AlternatePath);

    /// <summary>
    /// Return the alternate path at the specified index.
    /// </summary>
    /// <param name="a_index">Index of an alternate path.</param>
    /// <returns></returns>
    [Obsolete("Don't use this, what is this?")]
    public object GetRow(int a_index)
    {
        return m_pathsList[a_index];
    }

    /// <summary>
    /// The number of alternate paths.
    /// </summary>
    public int Count => m_pathsList.Count;

    /// <summary>
    /// Return the specified alternate path.
    /// </summary>
    public AlternatePath this[int a_index] => m_pathsList[a_index];

    /// <summary>
    /// O(n).
    /// Return the AlternatePath with the specified external id. null is returned if the id isn't found.
    /// </summary>
    /// <param name="a_externalId">The external id of the alternate path that you're looking for.</param>
    /// <returns>The AlternatePath or null if the AlternatePath isn't in the collection.</returns>
    internal AlternatePath FindByExternalId(string a_externalId)
    {
        for (int alternatePathI = 0; alternatePathI < Count; ++alternatePathI)
        {
            AlternatePath ap = this[alternatePathI];

            if (ap.ExternalId == a_externalId)
            {
                return ap;
            }
        }

        return null;
    }

    /// <summary>
    /// O(n).
    /// Return the AlternatePath with the specified name. null is returned if the id isn't found.
    /// </summary>
    /// <param name="a_name">The name of the alternate path that you're looking for.</param>
    /// <returns>The AlternatePath or null if the AlternatePath isn't in the collection.</returns>
    internal AlternatePath FindByName(string a_name)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (this[i].Name == a_name)
            {
                return this[i];
            }
        }

        return null;
    }

    /// <summary>
    /// O(n).
    /// Return the AlternatePath with the specified id. null is returned if the id isn't found.
    /// </summary>
    /// <param name="a_id">The ID of the alternate path that you're looking for.</param>
    /// <returns>The AlternatePath or null if the AlternatePath isn't in the collection.</returns>
    internal AlternatePath FindById(BaseId a_id)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (this[i].Id == a_id)
            {
                return this[i];
            }
        }

        return null;
    }
    #endregion

    #region Update
    /// <summary>
    /// Tell whether two AlternatePathCollections are identicle.
    /// They are equal if they have the same number of AlternatePath.
    /// And each path's structure matches.
    /// </summary>
    /// <param name="apc"></param>
    /// <returns></returns>
    internal RoutingChanges IdenticlePathStructures(AlternatePathsCollection apc)
    {
        RoutingChanges analyzer = new ();

        if (apc.Count != Count)
        {
            analyzer.AlternatePathChanged = true;
            if (apc.Count > Count)
            {
                analyzer.AddDescription("Alternate Paths Added.");
            }
            else
            {
                analyzer.AddDescription("Alternate Paths Removed.");
            }
        }

        for (int pathI = 0; pathI < Count; ++pathI)
        {
            AlternatePath ap = this[pathI];
            AlternatePath ap2 = apc.FindByExternalId(ap.ExternalId);

            if (ap2 == null)
            {
                //The path was removed. Check if it is scheduled.
                for (int i = 0; i < ap.NodeCount; i++)
                {
                    if (ap[i].Operation.Scheduled)
                    {
                        analyzer.ScheduledRoutingChanged = true;
                        analyzer.AddDescription("Scheduled Path " + ap.ExternalId + " was removed");
                        analyzer.RoutingChangeCause = RoutingChanges.RoutingChangeCauses.ScheduledOperationRemoved;
                        return analyzer;
                    }
                }

                analyzer.AlternatePathChanged = true;
                continue;
            }

            ap.IdenticalPathStructure(ap2, analyzer);
            if (analyzer.ScheduledRoutingChanged)
            {
                analyzer.RoutingChangeCause = RoutingChanges.RoutingChangeCauses.ScheduledPathChanged;
                return analyzer;
            }
        }

        return analyzer;
    }

    /// <summary>
    /// Update the alternate paths within this collection.
    /// At the moment the only thing that happens is the values of each node are updated.
    /// To handle more complicated sturcture changes use the other Update function
    /// </summary>
    /// <param name="a_paths"></param>
    /// <param name="a_dataChanges"></param>
    /// <returns>Whether any significant updates were made.</returns>
    internal bool Update(AlternatePathsCollection a_paths, IScenarioDataChanges a_dataChanges)
    {
        bool updates = false;

        for (int alternatePathI = 0; alternatePathI < a_paths.Count; ++alternatePathI)
        {
            AlternatePath apUpdated = a_paths[alternatePathI];
            AlternatePath apOriginal = FindByExternalId(apUpdated.ExternalId);

            if (apOriginal != null)
            {
                if (apOriginal.Update(apUpdated, a_dataChanges))
                {
                    updates = true;
                }
            }
        }

        return updates;
    }

    /// <summary>
    /// Update the alternate paths within this collection.
    /// New paths are added, including all nodes. Deleted paths are removed.
    /// </summary>
    /// <param name="a_paths"></param>
    /// <param name="a_mo"></param>
    /// <param name="a_dataChanges"></param>
    /// <returns>Whether any significant updates were made.</returns>
    internal bool Update(AlternatePathsCollection a_paths, ManufacturingOrder a_mo, IScenarioDataChanges a_dataChanges)
    {
        //Note: This could be simplified by just deleting and recreating all paths.
        //The only thing lost would be the updates bool.
        bool updates = false;
        List<AlternatePath> updatedList = new ();

        for (int alternatePathI = 0; alternatePathI < a_paths.Count; ++alternatePathI)
        {
            AlternatePath apUpdated = a_paths[alternatePathI];
            AlternatePath apOriginal = FindByExternalId(apUpdated.ExternalId);

            if (apOriginal != null)
            {
                updatedList.Add(apOriginal);
                if (apUpdated.NodeCount != apOriginal.NodeCount)
                {
                    Remove(apOriginal);
                    AlternatePath ap = new (apUpdated.Id, apUpdated, a_mo); //Use the updated path's id, it has already been generated and is unique
                    Add(ap);
                    updatedList.Add(ap);
                    updates = true;
                }
                else
                {
                    if (apOriginal.ValidateUpdate(apUpdated))
                    {
                        if (apOriginal.Update(apUpdated, a_dataChanges))
                        {
                            updates = true;
                        }
                    }
                    else
                    {
                        //Can't be updated, recreate the path
                        Remove(apOriginal);
                        AlternatePath ap = new (apUpdated.Id, apUpdated, a_mo); //Use the updated path's id, it has already been generated and is unique
                        Add(ap);
                        updatedList.Add(ap);
                        updates = true;
                    }
                }
            }
            else
            {
                AlternatePath ap = new (apUpdated.Id, apUpdated, a_mo); //Use the updated path's id, it has already been generated and is unique
                //add path
                Add(ap);
                updatedList.Add(ap);
                updates = true;
            }
        }

        //Delete non updated paths
        for (int i = Count - 1; i >= 0; i--)
        {
            if (!updatedList.Contains(this[i]))
            {
                Remove(this[i]);
                updates = true;
            }
        }

        // update MO's DefaultPath
        AlternatePath tmp = null;
        if (a_mo.DefaultPath != null)
        {
            tmp = FindByExternalId(a_mo.DefaultPath.ExternalId);
            if (tmp != null)
            {
                a_mo.DefaultPath = tmp;
            }
            else
            {
                a_mo.DefaultPath = this[0];
            }
        }

        //  update MO's CurrentPath references.
        if (a_mo.CurrentPath != null)
        {
            tmp = FindByExternalId(a_mo.CurrentPath.ExternalId);
            if (tmp != null)
            {
                a_mo.CurrentPath_setter = tmp;
            }
            else
            {
                a_mo.CurrentPath_setter = a_mo.DefaultPath;
            }
        }

        return updates;
    }
    #endregion

    #region Path eligibility stuff for the UI
    /// When an operation is Alt-clicked use this functionality to determine eligibilty.
    /// Alt-Clicking on a block means you want to reschedule the job as a different path.
    /// The drop indicates the new start time of the job plus alternate path to use.
    /// You'll get:
    /// 1. The set of resources eligible as that same path.
    /// 2. The set of resources eligible as a different path.
    /// 3. The set of resource eligible as the same path or different path.
    /// <summary>
    /// This is to allow you to determine which resources are eligible which paths correspond to the resources.
    /// </summary>
    /// <param name="io"></param>
    /// <returns></returns>
    public PlantEligibilityInfo GetPathEligibility(InternalOperation io)
    {
        ManufacturingOrder mo = io.ManufacturingOrder;
        Dictionary<InternalResource, List<AlternatePath>> resAndEligPaths = new ();

        for (int pathI = 0; pathI < Count; ++pathI)
        {
            AlternatePath ap = this[pathI];
            AlternatePath.NodeCollection leaves = ap.EffectiveLeaves;

            for (int leaveI = 0; leaveI < leaves.Count; ++leaveI)
            {
                AlternatePath.Node node = leaves[leaveI];
                InternalOperation pathLeadOperation = (InternalOperation)node.Operation;

                //The path may not have any eligable resources.
                if (node.ResReqsEligibilityNarrowedDuringSimulation.Count > 0)
                {
                    PlantResourceEligibilitySet pres = node.ResReqsEligibilityNarrowedDuringSimulation.PrimaryEligibilitySet;

                    SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
                    while (ersEtr.MoveNext())
                    {
                        EligibleResourceSet ers = ersEtr.Current.Value;

                        for (int eligibleResourceI = 0; eligibleResourceI < ers.Count; ++eligibleResourceI)
                        {
                            InternalResource ir = ers[eligibleResourceI];
                            List<AlternatePath> altPaths;
                            if (resAndEligPaths.TryGetValue(ir, out altPaths))
                            {
                                if (!altPaths.Contains(ap))
                                {
                                    altPaths.Add(ap);
                                }
                            }
                            else
                            {
                                altPaths = new List<AlternatePath>();
                                altPaths.Add(ap);
                                resAndEligPaths.Add(ir, altPaths);
                            }
                        }
                    }
                }
            }
        }

        PlantEligibilityInfo ei = new ();
        Dictionary<InternalResource, List<AlternatePath>>.Enumerator enumerator = resAndEligPaths.GetEnumerator();

        while (enumerator.MoveNext())
        {
            List<AlternatePath> paths = enumerator.Current.Value;
            paths.Sort(SortComparer);
            bool currentPath = false;

            for (int pathI = 0; pathI < paths.Count; ++pathI)
            {
                AlternatePath path = paths[pathI];

                if (path == mo.CurrentPath)
                {
                    currentPath = true;
                }
            }

            InternalResource ir = enumerator.Current.Key;

            if (currentPath)
            {
                if (paths.Count == 1)
                {
                    ei.EligibleAsSame.Add(new Resource_PathEligibilityInfo(ir, paths));
                }
                else
                {
                    ei.EligibleAsSameAndDifferent.Add(new Resource_PathEligibilityInfo(ir, paths));
                }
            }
            else if (!io.Split) //Split operations are not eligible on other paths.
            {
                ei.EligibleAsDifferent.Add(new Resource_PathEligibilityInfo(ir, paths));
            }
        }

        ei.ConstructionComplete();

        return ei;
    }

    /// <summary>
    /// Associate each operation with its path node.
    /// <param name="a_automaticallyResolveErrors">Whether to alter transfer quantity as needed to avoid validation errors</param>
    /// </summary>
    internal void AssociateOpsWithPathNodes(bool a_automaticallyResolveErrors)
    {
        Dictionary<BaseId, InternalOperation> opHashSet = new ();
        foreach (AlternatePath path in m_pathsList)
        {
            path.AssociateOpsWithPathNodes();
            path.ValidateOverlapTransferQuantity(a_automaticallyResolveErrors);

            IEnumerator<InternalOperation> opEtr = ((IEnumerable<InternalOperation>)path).GetEnumerator();
            while (opEtr.MoveNext())
            {
                if (opHashSet.ContainsKey(opEtr.Current.Id))
                {
                    throw new PTValidationException("2140", new object[] { opEtr.Current.ToString() });
                }

                opHashSet.Add(opEtr.Current.Id, opEtr.Current);
            }
        }
    }

    /// <summary>
    /// Don't use this function outside of this class. It was written to sort AlternatePaths by preference.
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public int SortComparer(AlternatePath p1, AlternatePath p2)
    {
        int preference = p1.Preference;

        if (preference < p2.Preference)
        {
            return -1;
        }

        if (preference > p2.Preference)
        {
            return 1;
        }

        return 0;
    }
    #endregion

    public IEnumerator<AlternatePath> GetEnumerator()
    {
        return m_pathsList.GetEnumerator();
    }

    public override string ToString()
    {
        if (Count == 1)
        {
            return "1 path";
        }

        return string.Format("{0} paths", Count);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerable<AlternatePath> GetPathsSortedByPreference()
    {
        List<AlternatePath> list = new (Count);
        for (int i = 0; i < Count; ++i)
        {
            AlternatePath path = this[i];
            list.Add(path);
        }

        return list.OrderBy(p => p.Preference);
    }
}