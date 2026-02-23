using System.Diagnostics;

namespace PT.Common.Collections;

partial class AVLTree<KeyType, ValueType>
{
    /// <summary>
    /// A sorted enumerator.
    /// </summary>
    public class Enumerator : IEnumerator<KeyValuePair<KeyType, ValueType>>
    {
#if DEBUG
        private int m_treeVersion;
#endif 
        
        /// <summary>
        /// This was originally a class, but was changed to a struct after profiling revealed a significant
        /// amount of time was being consumed resetting the nodes.
        /// It's possible to change the struct back to a class and not reset the nodes when Reset() is called,
        /// but it makes it a lot more difficult to see what's happening when debugging.
        /// The primary problem with struct is things like this don't work:
        /// TraversalNode tn = m_traversalStack[m_traversalIdx];
        /// tn.Left=true;
        /// Instead you have to reference the element directly like so:
        /// m_traversalStack[m_traversalIdx].Left=true;
        /// </summary>
        private struct TraversalNode
        {
            internal TreeNode Node;

            private bool m_left;

            internal bool Left
            {
                get => m_left;
                set => m_left = value;
            }

            private bool m_center;

            internal bool Center
            {
                get => m_center;
                set => m_center = value;
            }

            private bool m_right;

            internal bool Right
            {
                get => m_right;
                set => m_right = value;
            }

            /// <summary>
            /// Created to reduce some unnecessary work of the general Reset function.
            /// </summary>
            /// <param name="a_n"></param>
            internal void ResetCurrentTraversalNode(TreeNode a_n)
            {
                Node = a_n;
            }

            internal void Reset(TreeNode a_n)
            {
                ResetCurrentTraversalNode(a_n);
                Left = Center = Right = false;
            }

            public override string ToString()
            {
                return string.Format("Node: {0}; Left={1}; Center={2}; Right={3}", Node, Left, Center, Right);
            }
        }

        private readonly AVLTree<KeyType, ValueType> m_tree;

        private readonly TraversalNode[] m_traversalStack = new TraversalNode[MAX_HEIGHT];
        private int m_traversalIdx;

        private TreeNode m_current;

        public void Reset()
        {
#if DEBUG
            m_treeVersion = m_tree.m_version;
#endif
            Array.Clear(m_traversalStack, 0, MAX_HEIGHT);

            m_traversalIdx = m_tree.m_root != null ? 0 : -1;
            m_traversalStack[0].ResetCurrentTraversalNode(m_tree.m_root); // Reset() version isn't necessary since the array's memory was cleared.
        }

        public Enumerator(AVLTree<KeyType, ValueType> a_tree)
        {
            m_tree = a_tree;
            Reset();
        }

        internal Enumerator(AVLTree<KeyType, ValueType> a_tree, KeyType a_key)
            : this(a_tree)
        {
            ResetAtOrBefore(a_key);
        }

        /// <summary>
        /// Create a copy of an enumerator.
        /// </summary>
        /// <param name="a_etr"></param>
        public Enumerator(Enumerator a_etr)
        {
            m_tree = a_etr.m_tree;
#if DEBUG
            m_treeVersion = m_tree.m_version;
#endif
            CopyPosition(a_etr);
        }

        /// <summary>
        /// Copy the internals of an enumerator into this enumerator.
        /// After the copy, this enumerator will have the same position
        /// in the collection as the enumerator it's copying from.
        /// Both enumerator must be for the same tree.
        /// </summary>
        /// <param name="a_etr">The node whose position you want to copy into this node.</param>
        public void CopyPosition(Enumerator a_etr)
        {
            TestCopyPosition(a_etr);

            m_current = a_etr.m_current;
            m_traversalIdx = a_etr.m_traversalIdx;
            for (int i = 0; i < a_etr.m_traversalStack.Length; ++i)
            {
                m_traversalStack[i] = a_etr.m_traversalStack[i];
            }

            TestCopyPosition(a_etr);
        }

        [Conditional("DEBUG")]
        private void TestCopyPosition(Enumerator a_etr)
        {
            if (a_etr.m_tree != m_tree)
            {
                throw new Exception("AVLTree<>.Enumerator.CopyPosition. a_etr.m_tree!=m_tree. The enumerators must be for the same tree.");
            }
        }
        
        [Conditional("DEBUG")]
        private void ValidateTreeVersion()
        {
#if DEBUG
            if (m_treeVersion != m_tree.m_version)
            {
                throw new InvalidOperationException("Collection was modified");
            }
#endif
        }
        
        /// <summary>
        /// Once MoveNext returns false, this function will return null.
        /// </summary>
        public KeyValuePair<KeyType, ValueType> Current => new (m_current.Key, m_current.Value);

        public void Dispose() { }

        object System.Collections.IEnumerator.Current => Current;

        /// <summary>
        /// Move to the next element in the sorted enumerator.
        /// Once this function returns false, Current will return null.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            ValidateTreeVersion();
            
            while (m_traversalIdx >= 0)
            {
                if (!m_traversalStack[m_traversalIdx].Left)
                {
                    m_traversalStack[m_traversalIdx].Left = true;

                    if (m_traversalStack[m_traversalIdx].Node.m_left != null)
                    {
                        int origTraversalIdx = m_traversalIdx;
                        m_traversalStack[++m_traversalIdx].Reset(m_traversalStack[origTraversalIdx].Node.m_left);
                    }
                    else
                    {
                        m_traversalStack[m_traversalIdx].Center = true;
                        m_current = m_traversalStack[m_traversalIdx].Node;
                        return true;
                    }
                }
                else if (!m_traversalStack[m_traversalIdx].Center)
                {
                    m_traversalStack[m_traversalIdx].Center = true;
                    m_current = m_traversalStack[m_traversalIdx].Node;
                    return true;
                }
                else if (!m_traversalStack[m_traversalIdx].Right)
                {
                    m_traversalStack[m_traversalIdx].Right = true;

                    if (m_traversalStack[m_traversalIdx].Node.m_right != null)
                    {
                        int origTraveralIdx = m_traversalIdx;
                        m_traversalStack[++m_traversalIdx].Reset(m_traversalStack[origTraveralIdx].Node.m_right);
                    }
                    else
                    {
                        --m_traversalIdx;
                    }
                }
                else
                {
                    --m_traversalIdx;
                }
            }

            m_current = null;
            return false;
        }

        /// <summary>
        /// Resets the sorted enumerator to the first element at the specified search key.
        /// This provides a fast way to start enumerating from a specified key.
        /// If the specified search key isn't in the set, the enumerator will be set to start at
        /// the first element less than the search key. If no elements in the set are <= the
        /// search key, MoveNext() will return false.
        /// </summary>
        /// <param name="a_key">The key you want the enumerator at or before.</param>
        public void ResetAtOrBefore(KeyType a_onOrBefore)
        {
            Reset();

            int traversalIdxCur;

            while ((traversalIdxCur = m_traversalIdx) >= 0)
            {
                // Test for equivalence.
                int comp = m_tree.m_comparer.Compare(a_onOrBefore, m_traversalStack[traversalIdxCur].Node.Key);

                if (comp == 0)
                {
                    m_traversalStack[traversalIdxCur].Left = true;
                    return;
                }

                if (comp < 0) // If the key is smaller, go left.
                {
                    m_traversalStack[traversalIdxCur].Left = true;
                    if (m_traversalStack[traversalIdxCur].Node.Left != null)
                    {
                        m_traversalStack[++m_traversalIdx].Reset(m_traversalStack[traversalIdxCur].Node.Left);
                    }
                    else
                    {
                        // Can't go further to the left.
                        // The current key is the first key greater than the key being searched for.
                        return;
                    }
                }
                else // The key is larger, go right.
                {
                    // The enumerator will never go to the left since it's too small. 
                    m_traversalStack[traversalIdxCur].Left = true;
                    m_traversalStack[traversalIdxCur].Center = true;
                    m_traversalStack[traversalIdxCur].Right = true;
                    if (m_traversalStack[traversalIdxCur].Node.Right != null)
                    {
                        m_traversalStack[++m_traversalIdx].Reset(m_traversalStack[traversalIdxCur].Node.Right);
                    }
                    else
                    {
                        // Can't go any further to the right.
                        // Head back up the call stack until you 
                        // reach a node that is greater than the key or until you run out of nodes.
                        // Everything is smaller than the key being sought.
                        if (m_traversalIdx == 0)
                        {
                            m_traversalIdx = -1;
                            return;
                        }

                        do
                        {
                            --m_traversalIdx;
                            if (m_traversalIdx == -1)
                            {
                                return;
                            }

                            int compReverse = m_tree.m_comparer.Compare(a_onOrBefore, m_traversalStack[m_traversalIdx].Node.Key);
                            if (compReverse < 0)
                            {
                                return;
                            }
                        } while (m_traversalIdx >= 0);

                        return;
                    }
                }
            }
        }
        /*
         * **************************************************************************************************************
         * ResetAtOrBefore test code
         * **************************************************************************************************************
         *
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

using PT.Common.Collections;

class ConsoleApplication1
{
static void Main()
{
    //Console.WriteLine((int)JobDefs.ExcludedReasons.NotExcluded);
    //Console.WriteLine((int)JobDefs.ExcludedReasons.DoNotScheduleSelected);
    //Console.WriteLine((int)JobDefs.ExcludedReasons.ExcludedPlanned);
    //Console.WriteLine((int)JobDefs.ExcludedReasons.ExcludedEstimate);
    //Console.WriteLine((int)JobDefs.ExcludedReasons.ExcludedOnHold);
    //Console.WriteLine((int)JobDefs.ExcludedReasons.ExcludedNew);
    //Console.WriteLine((int)JobDefs.ExcludedReasons.ExcludedMaxTrialDemoLimit);
    //Console.WriteLine((int)JobDefs.ExcludedReasons.ExcludedUnscheduled);

    //JobDefs.ExcludedReasons r = JobDefs.ExcludedReasons.NotSet;
    //r |= JobDefs.ExcludedReasons.ExcludedEstimate;
    //if((r.HasFlag(JobDefs.ExcludedReasons.ExcludedEstimate)))
    //{
    //    int xx = 0;
    //}

    //int x=(int)JobDefs.ExcludedReasons.NotExcluded;

    //Test test = new Test();
    //JobDefs.ExcludedReasons[] reasons = test.GetExcludeReasons();

    //test.ExcludeReasons_AddReason(JobDefs.ExcludedReasons.DoNotSchedule);
    //test.ExcludeReasons_AddReason(JobDefs.ExcludedReasons.DoNotScheduleSelected);

    //reasons = test.GetExcludeReasons();

    int minTest = int.MaxValue;
    int maxTest = int.MinValue;

    const int nbrOfTests = 10000;
    DateTime start = DateTime.Now;

    for (int testI = 0; testI < nbrOfTests; ++testI)
    {
        Console.WriteLine(string.Format("Test {0} of {1}", testI + 1, nbrOfTests));
        List<int> tmpRandNbrs = new List<int>();
        const int nbrOfRandNbrs = 1000;
        for (int i = 0; i < nbrOfRandNbrs; ++i)
        {
            tmpRandNbrs.Add(i);
        }

        List<int> randNbrs = new List<int>();
        System.Threading.Thread.Sleep(10);
        Random r = new Random();
        for (int i = 0; i < nbrOfRandNbrs; ++i)
        {
            int idx = r.Next(tmpRandNbrs.Count);
            randNbrs.Add(tmpRandNbrs[idx]);
            tmpRandNbrs.RemoveAt(idx);
        }


        const int maxElements = nbrOfRandNbrs;
        int testSize = r.Next(maxElements+1);

        if(testSize<minTest)
        {
            minTest = testSize;
        }
        if(testSize>maxTest)
        {
            maxTest = testSize;
        }

        int[] nbrs = new int[testSize];
        for (int i = 0; i < testSize; ++i)
        {
            nbrs[i] = randNbrs[i];
        }

        int searchNbr = r.Next(nbrOfRandNbrs);
        TestEnumerator(searchNbr, nbrs);
    }

    DateTime end = DateTime.Now;

    Console.WriteLine();
    Console.WriteLine(string.Format("MinTest={0}", minTest));
    Console.WriteLine(string.Format("MaxTest={0}", maxTest));

    TimeSpan span = end - start;

    Console.WriteLine("Start={0}", start);
    Console.WriteLine("End={0}", end);
    Console.WriteLine("TimeSpan={0}", span.ToString());

    ////Nothing. Find Nothing
    //TestEnumerator(0);

    //// Sinlge. Find something ==.
    //TestEnumerator(10, 10);

    //// Sinlge <. Find something <.
    //TestEnumerator(9, 10);

    //// Sinlge>. Find nothing.
    //TestEnumerator(10, 1);

    //// doulbe to left. Find 10 ==
    //TestEnumerator(10, 10, 1);

    //// doulbe to left. Find 10 <
    //TestEnumerator(9, 10, 1);

    //// doulbe to left. Find 10 >
    //TestEnumerator(11, 10, 1);

    //// Five. Find 3,4,5,
    //TestEnumerator(3, 1,2,3,4,5);

}

static void TestEnumerator(int a_search, params int[] a_adds)
{
    int find = int.MaxValue;
    int minDiff = int.MaxValue;
    AVLTree<int, int> tree = new AVLTree<int, int>(Comparer<int>.Default);
    for (int i = 0; i < a_adds.Length; ++i)
    {
        int v = a_adds[i];
        tree.Add(v, v);

        if (v >= a_search)
        {
            int diff = v - a_search;
            if (diff < minDiff)
            {
                find = v;
                minDiff = diff;
            }
        }
    }

    StringBuilder sb = new StringBuilder();
    sb.AppendFormat("TestEnumerator({0},", a_search);
    sb.AppendLine();

    for (int i = 0; i < a_adds.Length; ++i)
    {
        int v = a_adds[i];

        if (i > 0)
        {
            sb.Append(", ");
        }

        sb.Append(v);
    }

    sb.AppendLine(");");

    string findMsg = find.ToString();
    if (find == int.MaxValue)
    {
        findMsg = "Nothing";
    }
    sb.AppendFormat("Search for {0}. Should find {1}", a_search, findMsg);
    sb.AppendLine();

    AVLTree<int, int>.Enumerator etr = tree.GetEnumeratorAtOrBefore(a_search);
    WriteNodes(etr, sb.ToString());

    List<int> atOrBeforeNodes = new List<int>();
    etr = tree.GetEnumeratorAtOrBefore(a_search);
    while(etr.MoveNext())
    {
        int v = etr.Current.Value;
        if(v<a_search)
        {
            Error("GetEnumeratorAtOrBefore problem");
        }
        atOrBeforeNodes.Add(v);
    }

    // Verify the full traversal is in order.
    etr = tree.GetEnumerator();
    int prev = int.MinValue;

    List<int> etrNodes = new List<int>();
    int etrTotalNodeCount = 0;
    while(etr.MoveNext())
    {
        ++etrTotalNodeCount;
        int v = etr.Current.Value;
        if(v>prev)
        {
            prev = v;
        }
        else
        {
            Error("Full traversal not in order.");
        }
        if(v>=a_search)
        {
            etrNodes.Add(v);
        }
    }

    if(etrTotalNodeCount!=a_adds.Length)
    {
        Error("The number of enumerated nodes isn't equal to the number of nodes added.");
    }

    // Verify lists match
    if(etrNodes.Count!=atOrBeforeNodes.Count)
    {
        Error("List counts don't match.");
    }

    if(etrNodes.Count>0)
    {
        int v = etrNodes[0];
        if(v!=find)
        {
            Error("Find and first node don't match");
        }
    }

    for(int i=0;i<etrNodes.Count; ++i)
    {
        if(etrNodes[i]!=atOrBeforeNodes[i])
        {
            Error("Nodes don't match");
        }
    }
}

private static void Error(string a_msg)
{
    Console.WriteLine(a_msg);
    System.Threading.Thread.Sleep(10 * 60 * 1000);
}

private static void WriteNodes(AVLTree<int, int>.Enumerator etr, string msg)
{
    WriteSeparator();
    WriteSeparator();
    Console.WriteLine(msg);

    int i = 1;
    while (etr.MoveNext())
    {
        System.Collections.Generic.KeyValuePair<int, int> kv = etr.Current;
        Console.WriteLine(string.Format("{0}. {1}", i, kv.Value));
        ++i;
    }
    WriteSeparator();
    Console.WriteLine();
}

private static void WriteSeparator()
{
    Console.WriteLine("++++++++++++++++++++++++++++++++");
}
}
*/
    }
}