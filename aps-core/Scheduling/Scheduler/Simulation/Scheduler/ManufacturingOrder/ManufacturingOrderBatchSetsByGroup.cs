//namespace PT.Scheduler.Simulation;

///// <summary>
///// Each set corresponds to a unique ManufacturingOrderBatchSet where all elements in the set are in the same group.
///// You can access sets by group name. O(1).
///// </summary>
//public class ManufacturingOrderBatchSetsByDefAndGroup
//{
//    private readonly SortedDictionary<Key, ManufacturingOrderBatchSet> m_mobsDictionary = new ();

//    public class Key : IComparable
//    {
//        internal string m_batchDefName;
//        internal string m_groupName;

//        internal Key(string a_batchDefName, string a_groupName)
//        {
//            m_batchDefName = a_batchDefName;
//            m_groupName = a_groupName;
//        }

//        #region IComparable Members
//        public int CompareTo(object obj)
//        {
//            Key k = (Key)obj;
//            int c;
//            if ((c = string.Compare(m_batchDefName, k.m_batchDefName)) != 0)
//            {
//                return c;
//            }

//            return string.Compare(m_groupName, k.m_groupName);
//        }
//        #endregion
//    }

//    internal void Add(string a_batchDefName, string a_group, ManufacturingOrderBatchSet a_mobs)
//    {
//        m_mobsDictionary.Add(new Key(a_batchDefName, a_group), a_mobs);
//    }

//    internal ManufacturingOrderBatchSet GetMOSetByDefAndGroup(string a_batchDefName, string a_group)
//    {
//        ManufacturingOrderBatchSet mobs;

//        if (m_mobsDictionary.TryGetValue(new Key(a_batchDefName, a_group), out mobs))
//        {
//            return mobs;
//        }

//        return null;
//    }

//    public SortedDictionary<Key, ManufacturingOrderBatchSet>.Enumerator GetEnumerator()
//    {
//        return m_mobsDictionary.GetEnumerator();
//    }
//}