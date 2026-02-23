//namespace PT.Scheduler.Simulation;

///// <summary>
///// A set of batches acessible by index.
///// After adding members to this batch, call the Sort function to Sort them by batch sort date.
///// If the batches were added in order, you don't need to call Sort.
///// </summary>
//public class ManufacturingOrderBatchSet
//{
//    private readonly List<ManufacturingOrderBatch> m_batches = new ();

//    internal List<ManufacturingOrderBatch> Batches => m_batches;

//    /// <summary>
//    /// The number of elements. You can access them with the index operator.
//    /// </summary>
//    internal int Count => m_batches.Count;

//    /// <summary>
//    /// After you're done adding, call sort to arrange the batches by batch start date.
//    /// </summary>
//    /// <param name="batch"></param>
//    internal void Add(ManufacturingOrderBatch a_batch)
//    {
//        m_batches.Add(a_batch);
//    }

//    /// <summary>
//    /// O(n)
//    /// </summary>
//    /// <param name="idx"></param>
//    /// <returns></returns>
//    internal ManufacturingOrderBatch this[int a_idx] => m_batches[a_idx];

//    ///// <summary>
//    ///// Sort the batches by start date.
//    ///// </summary>
//    //internal void Sort()
//    //{
//    //    m_batches.Sort(CompareBatchesByStartDate);
//    //}

//    ///// <summary>
//    ///// Used to compare 2 batches by start date.
//    ///// </summary>
//    ///// <param name="a"></param>
//    ///// <param name="b"></param>
//    ///// <returns></returns>
//    //static int CompareBatchesByStartDate(ManufacturingOrderBatch a_x, ManufacturingOrderBatch a_y)
//    //{
//    //    if (a_x.BatchDate > a_y.BatchDate)
//    //    {
//    //        return 1;
//    //    }
//    //    else if (a_x.BatchDate < a_y.BatchDate)
//    //    {
//    //        return -1;
//    //    }
//    //    else
//    //    {
//    //        return 0;
//    //    }
//    //}

//    internal void BatchingComplete()
//    {
//        for (int i = 0; i < Count; ++i)
//        {
//            this[i].BatchingComplete();
//        }
//    }

//    public List<ManufacturingOrderBatch>.Enumerator GetEnumerator()
//    {
//        return m_batches.GetEnumerator();
//    }
//}