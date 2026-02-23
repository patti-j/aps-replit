using PT.APSCommon;
using PT.APSCommon.Collections;

namespace PT.Scheduler;

public partial class AllowedHelperManager : CustomSortedList<AllowedHelperManager.AllowedHelperRelation>, IPTSerializable
{
    #region Serialization
    internal AllowedHelperManager(IReader reader)
        : base(reader, new AllowedHelperRelationComparer())
    {
        if (reader.VersionNumber >= 410) { } // Nothing to do. base class reads the list.

        #region < 410
        else
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                AllowedHelperRelation r = new (reader);
                Add(r);
            }
        }
        #endregion
    }

    internal void RestoreReferences(Plant a_plant)
    {
        IEnumerator<AllowedHelperRelation> etr = GetUnsortedEnumerator();
        while (etr.MoveNext())
        {
            etr.Current.RestoreReferences(a_plant);
        }
    }
    #endregion

    internal AllowedHelperManager()
        : base(new AllowedHelperRelationComparer()) { }

    protected override AllowedHelperRelation CreateInstance(IReader a_reader)
    {
        return new AllowedHelperRelation(a_reader);
    }

    internal void HandleDeletedResources(Dictionary<BaseId, Resource> a_resDictionary)
    {
        List<AllowedHelperRelation> tempResToHelperRelationList = new ();

        IEnumerator<AllowedHelperRelation> enumerator = GetEnumerator();
        while (enumerator.MoveNext())
        {
            AllowedHelperRelation allowedHelperRelation = enumerator.Current;
            Resource primaryRes = allowedHelperRelation.PrimaryResource;

            if (a_resDictionary.ContainsKey(primaryRes.Id))
            {
                List<Resource> allowedHelpers = new ();

                IEnumerator<Resource> resEnumerator = allowedHelperRelation.GetEnumerator();
                while (resEnumerator.MoveNext())
                {
                    Resource res = resEnumerator.Current;
                    if (a_resDictionary.ContainsKey(res.Id))
                    {
                        allowedHelpers.Add(res);
                    }
                }

                if (allowedHelpers.Count > 0)
                {
                    tempResToHelperRelationList.Add(new AllowedHelperRelation(primaryRes, allowedHelpers));
                }
            }
        }

        SetCollectionListTo(tempResToHelperRelationList);
    }

    public class AllowedHelperRelationComparer : IKeyObjectComparer<AllowedHelperRelation>
    {
        public int Compare(AllowedHelperRelation x, AllowedHelperRelation y)
        {
            return CompareAllowedHelperRelation(x, y);
        }

        internal static int CompareAllowedHelperRelation(AllowedHelperRelation a_relation, AllowedHelperRelation a_anotherRelation)
        {
            return Comparison.Compare(a_relation.PrimaryResource.Id.Value, a_anotherRelation.PrimaryResource.Id.Value);
        }

        public object GetKey(AllowedHelperRelation a_relation)
        {
            return a_relation.PrimaryResource == null ? a_relation.RestoreReferencesProp.m_primaryResId : a_relation.PrimaryResource.Id;
        }
    }

    public AllowedHelperRelation GetAllowedHelperRelationForPrimary(Resource a_primary)
    {
        IEnumerator<AllowedHelperRelation> enumerator = GetEnumerator();
        while (enumerator.MoveNext())
        {
            AllowedHelperRelation allowedHelperRelation = enumerator.Current;
            if (allowedHelperRelation.PrimaryResource.Id == a_primary.Id)
            {
                return allowedHelperRelation;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the number of Allowed Helpers defined.
    /// </summary>
    /// <returns></returns>
    internal int GetRelationshipCount()
    {
        return Count;
    }

    public class AllowedHelperRelation : CustomSortedList<Resource>, IPTSerializable
    {
        #region Serialization
        public override void Serialize(IWriter writer)
        {
            PrimaryResource.Id.Serialize(writer);
            writer.Write(Count);
            foreach (Resource res in ReadOnlyList)
            {
                res.Id.Serialize(writer);
            }
        }

        internal class ReferenceInfo
        {
            internal ReferenceInfo(IReader reader)
            {
                m_primaryResId = new BaseId(reader);

                int count;
                reader.Read(out count);
                m_helperResIdList = new List<BaseId>();
                for (int i = 0; i < count; ++i)
                {
                    BaseId helperId = new (reader);
                    m_helperResIdList.Add(helperId);
                }
            }

            internal BaseId m_primaryResId;
            internal List<BaseId> m_helperResIdList;
        }

        private ReferenceInfo m_restoreReferences;

        internal ReferenceInfo RestoreReferencesProp
        {
            get => m_restoreReferences;
            private set => m_restoreReferences = value;
        }

        internal AllowedHelperRelation(IReader reader)
            : base(new ResourceComparer())
        {
            RestoreReferencesProp = new ReferenceInfo(reader);
        }

        protected override Resource CreateInstance(IReader a_reader)
        {
            throw new NotImplementedException();
        }

        internal void RestoreReferences(Plant a_plant)
        {
            PrimaryResource = a_plant.FindResource(RestoreReferencesProp.m_primaryResId);

            for (int i = 0; i < RestoreReferencesProp.m_helperResIdList.Count; ++i)
            {
                Resource r = a_plant.FindResource(RestoreReferencesProp.m_helperResIdList[i]);
                Add(r);
            }
        }

        public new int UniqueId => throw new NotImplementedException();
        #endregion

        internal AllowedHelperRelation()
            : base(new ResourceComparer()) { }

        internal AllowedHelperRelation(Resource a_res)
            : base(new ResourceComparer())
        {
            PrimaryResource = a_res;
        }

        internal AllowedHelperRelation(Resource a_primaryResource, List<Resource> a_allowedHelpers)
            : base(new ResourceComparer())
        {
            PrimaryResource = a_primaryResource;
            SetCollectionListTo(a_allowedHelpers);
        }

        internal AllowedHelperRelation(ResourceManager a_rm, Transmissions.AllowedHelperResourcesT.HelperRelation a_hr)
            : base(new ResourceComparer())
        {
            PrimaryResource = a_rm.GetById(a_hr.PrimaryResourceId);
            foreach (BaseId helperId in a_hr.AllowedHelperResourceIds)
            {
                Resource r = a_rm.GetById(helperId);
                Add(r);
            }
        }

        private Resource m_primaryResource;

        public Resource PrimaryResource
        {
            get => m_primaryResource;

            private set => m_primaryResource = value;
        }

        public class ResourceComparer : IKeyObjectComparer<Resource>
        {
            public int Compare(Resource x, Resource y)
            {
                return CompareResource(x, y);
            }

            internal static int CompareResource(Resource a_res, Resource a_anotherRes)
            {
                return Comparison.Compare(a_res.Id.Value, a_anotherRes.Id.Value);
            }

            public object GetKey(Resource a_res)
            {
                return a_res.Id;
            }
        }
    }
}