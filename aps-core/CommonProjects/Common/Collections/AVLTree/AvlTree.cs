using System.Diagnostics;

using PT.Common.Testing;

namespace PT.Common.Collections;

public partial class AVLTree<KeyType, ValueType> : IEnumerable<KeyValuePair<KeyType, ValueType>>
{
    public AVLTree(IComparer<KeyType> a_comparer)
    {
        m_comparer = a_comparer;
        m_adjustmentPath[0] = m_rootPointer;
        m_adjustmentNodeFromParentsLeft[0] = true;
        m_adjustmentNodeFromParentsLeft[1] = true;
        m_keys = new HashSet<KeyType>();
    }

    private readonly IComparer<KeyType> m_comparer;
    private HashSet<KeyType> m_keys;
#if DEBUG
    private int m_version; //This tracks changes so enumerators can validate if the tree has changed during enumerating
#endif

    [Conditional("DEBUG")]
    private void UpdateVersion()
    {
#if DEBUG
        m_version++;
#endif
    }

    public bool ContainsKey(KeyType a_key)
    {
        return m_keys.Contains(a_key);
    }

    private int Compare(TreeNode a_x, TreeNode a_y)
    {
        return m_comparer.Compare(a_x.m_key, a_y.m_key);
    }

    private TreeNode m_root;
    private int m_count;

    public int Count => m_count;

    public TreeNode Root => m_root;

    public void Clear()
    {
        m_root = null;
        m_rootPointer.m_left = null;
        m_count = 0;
        ResetAdjustmentPath();
        m_keys.Clear();
        UpdateVersion();
    }

#if DEBUG

    private long m_leftRotations;

    public long LeftRotations => m_leftRotations;

    private long m_leftDoubleRotations;

    public long LeftDoubleRotations => m_leftDoubleRotations;

    private long m_rightRotations;

    public long RightRotations => m_rightRotations;

    private long m_rightDoubleRotations;

    public long RightDoubleRotations => m_rightDoubleRotations;

    public long GetTotalRotations()
    {
        return m_leftRotations + m_leftDoubleRotations + m_rightRotations + m_rightDoubleRotations;
    }

    public long CalculateSize()
    {
        return TestInOrderTraversal(false).Count;
    }

#endif

    /// <summary>
    /// The root pointer never has any data in it. It's left node contains the actual root of the tree.
    /// </summary>
    private readonly TreeNode m_rootPointer = new ();

    private static readonly int MAX_HEIGHT = 64; // Minimum AVL elements: 44,945,570,212,852
    private readonly TreeNode[] m_adjustmentPath = new TreeNode[MAX_HEIGHT];
    private readonly bool[] m_adjustmentNodeFromParentsLeft = new bool[MAX_HEIGHT];

    private void ResetAdjustmentPath()
    {
        for (int i = 1; i < MAX_HEIGHT; ++i)
        {
            m_adjustmentPath[i] = null;
        }
    }

    public void Add(KeyType a_key, ValueType a_data)
    {
        TreeNode n = new (a_key, a_data);
        Add(n);
    }

    public void Add(TreeNode a_insertionNode)
    {
        UpdateVersion();
        m_keys.Add(a_insertionNode.Key);

        if (m_root == null)
        {
            m_root = a_insertionNode;
            ++m_count;
        }
        else
        {
            TreeNode currentNode = m_root;

            m_rootPointer.m_left = m_root;
            int insertionIdx = 1;
            m_adjustmentPath[1] = m_root;

            while (true)
            {
                int comparison = Compare(currentNode, a_insertionNode);

                if (comparison == 0)
                {
                    throw new Exception("The key already exists in the tree.");
                }

                if (comparison < 0)
                {
                    if (currentNode.m_right == null)
                    {
                        currentNode.m_right = a_insertionNode;
                        ++m_count;

                        if (currentNode.m_height == 1)
                        {
                            return;
                        }

                        currentNode.m_height = 1;
                        break;
                    }

                    currentNode = currentNode.m_right;
                    ++insertionIdx;
                    m_adjustmentNodeFromParentsLeft[insertionIdx] = false;
                }
                else
                {
                    if (currentNode.m_left == null)
                    {
                        currentNode.m_left = a_insertionNode;
                        ++m_count;

                        if (currentNode.m_height == 1)
                        {
                            return;
                        }

                        currentNode.m_height = 1;
                        break;
                    }

                    currentNode = currentNode.m_left;
                    ++insertionIdx;
                    m_adjustmentNodeFromParentsLeft[insertionIdx] = true;
                }

                m_adjustmentPath[insertionIdx] = currentNode;
            }

            for (int i = insertionIdx - 1; i >= 1; --i)
            {
                currentNode = m_adjustmentPath[i];

                int leftHeight = currentNode.m_left?.m_height ?? -1;
                int rightHeight = currentNode.m_right?.m_height ?? -1;

                int diffHeight = leftHeight - rightHeight;

                //if (diffHeight == 0) *************************************
                //    break;

                TreeNode childNode;
                TreeNode doubleNode;
                if (diffHeight < -1)
                {
                    childNode = currentNode.m_right;
                    if (Compare(childNode, a_insertionNode) == -1)
                    {
                        currentNode.m_right = childNode.m_left;
                        childNode.m_left = currentNode;
                        currentNode.UpdateHeight();
                        childNode.UpdateHeight();
                        if (m_adjustmentNodeFromParentsLeft[i])
                        {
                            m_adjustmentPath[i - 1].m_left = childNode;
                        }
                        else
                        {
                            m_adjustmentPath[i - 1].m_right = childNode;
                        }

#if DEBUG
                        ++m_rightRotations;
#endif
                        // Update heights after *************************************
                    }
                    else
                    {
                        doubleNode = childNode.m_left;
                        currentNode.m_right = doubleNode.m_left;
                        currentNode.UpdateHeight();
                        childNode.m_left = doubleNode.m_right;
                        childNode.UpdateHeight();
                        doubleNode.m_left = currentNode;
                        doubleNode.m_right = childNode;
                        doubleNode.UpdateHeight();
                        if (m_adjustmentNodeFromParentsLeft[i])
                        {
                            m_adjustmentPath[i - 1].m_left = doubleNode;
                        }
                        else
                        {
                            m_adjustmentPath[i - 1].m_right = doubleNode;
                        }

#if DEBUG
                        ++m_rightDoubleRotations;
#endif
                        // Update heights after *************************************
                    }
                }
                else if (diffHeight > 1)
                {
                    childNode = currentNode.m_left;
                    if (Compare(childNode, a_insertionNode) == 1)
                        // I think the comparison can be eliminated *************************************
                    {
                        currentNode.m_left = childNode.m_right;
                        childNode.m_right = currentNode;
                        currentNode.UpdateHeight();
                        childNode.UpdateHeight();
                        if (m_adjustmentNodeFromParentsLeft[i])
                        {
                            m_adjustmentPath[i - 1].m_left = childNode;
                        }
                        else
                        {
                            m_adjustmentPath[i - 1].m_right = childNode;
                        }

#if DEBUG
                        ++m_leftRotations;
#endif
                        // Update heights after *************************************
                    }
                    else
                    {
                        doubleNode = childNode.m_right;
                        currentNode.m_left = doubleNode.m_right;
                        currentNode.UpdateHeight();
                        childNode.m_right = doubleNode.m_left;
                        childNode.UpdateHeight();
                        doubleNode.m_left = childNode;
                        doubleNode.m_right = currentNode;
                        doubleNode.UpdateHeight();
                        if (m_adjustmentNodeFromParentsLeft[i])
                        {
                            m_adjustmentPath[i - 1].m_left = doubleNode;
                        }
                        else
                        {
                            m_adjustmentPath[i - 1].m_right = doubleNode;
                        }
#if DEBUG
                        ++m_leftDoubleRotations;
#endif
                        // Update heights after *************************************
                    }
                }
                else
                {
                    currentNode.m_height = leftHeight > rightHeight ? leftHeight : rightHeight;
                    ++currentNode.m_height;
                }
            }

            m_root = m_rootPointer.m_left;
        }
    }

    private readonly TreeNode m_keyNode = new ();

    public bool Remove(KeyType a_key)
    {
        UpdateVersion();
        m_keys.Remove(a_key);

        m_keyNode.SetKey(a_key);
        TreeNode currentNode = m_root;

        m_rootPointer.m_left = m_root;
        int insertionIdx = 1;
        m_adjustmentPath[1] = m_root;

        while (true)
        {
            if (currentNode == null)
            {
                return false;
            }

            int compare = Compare(m_keyNode, currentNode);

            if (compare == -1)
            {
                currentNode = currentNode.Left;
                ++insertionIdx;
                m_adjustmentNodeFromParentsLeft[insertionIdx] = true;
                m_adjustmentPath[insertionIdx] = currentNode;
            }
            else if (compare == 1)
            {
                currentNode = currentNode.m_right;
                ++insertionIdx;
                m_adjustmentNodeFromParentsLeft[insertionIdx] = false;
                m_adjustmentPath[insertionIdx] = currentNode;
            }
            else
            {
                --m_count;

                if (currentNode.m_left != null && currentNode.m_right != null)
                {
                    // replace it with the largest value from the left subtree and rebalance your way from the value all the way to the top.
                    TreeNode deleteNode = currentNode;

                    currentNode = currentNode.Left;
                    ++insertionIdx;
                    m_adjustmentNodeFromParentsLeft[insertionIdx] = true;
                    m_adjustmentPath[insertionIdx] = currentNode;

                    while (currentNode.m_right != null)
                    {
                        currentNode = currentNode.m_right;
                        ++insertionIdx;
                        m_adjustmentNodeFromParentsLeft[insertionIdx] = false;
                        m_adjustmentPath[insertionIdx] = currentNode;
                    }

                    deleteNode.SetKeyAndValue(currentNode);
                }

                int parentInsertionIdx = insertionIdx - 1;
                TreeNode parentNode = m_adjustmentPath[parentInsertionIdx];

                if (currentNode.m_left == null && currentNode.Right == null)
                {
                    if (m_adjustmentNodeFromParentsLeft[insertionIdx])
                    {
                        parentNode.m_left = null;
                    }
                    else
                    {
                        parentNode.m_right = null;
                    }
                }
                else if (currentNode.m_left == null)
                {
                    if (m_adjustmentNodeFromParentsLeft[insertionIdx])
                    {
                        parentNode.m_left = currentNode.m_right;
                    }
                    else
                    {
                        parentNode.m_right = currentNode.m_right;
                    }
                }
                else if (currentNode.m_right == null)
                {
                    if (m_adjustmentNodeFromParentsLeft[insertionIdx])
                    {
                        parentNode.m_left = currentNode.m_left;
                    }
                    else
                    {
                        parentNode.m_right = currentNode.m_left;
                    }
                }

                int leftChildHeight;
                int rightChildHeight;

                for (int i = parentInsertionIdx; i >= 1; --i)
                {
                    currentNode = m_adjustmentPath[i];

                    int leftHeight = currentNode.m_left?.m_height ?? -1;
                    int rightHeight = currentNode.m_right?.m_height ?? -1;

                    int diffHeight = leftHeight - rightHeight;

                    TreeNode childNode;
                    TreeNode doubleNode;
                    if (diffHeight > 1)
                    {
                        childNode = currentNode.m_left;

                        leftChildHeight = childNode.LeftHeight();
                        rightChildHeight = childNode.RightHeight();

                        if (leftChildHeight > rightChildHeight || leftChildHeight == rightChildHeight)
                        {
                            currentNode.m_left = childNode.m_right;
                            childNode.m_right = currentNode;
                            currentNode.UpdateHeight();
                            childNode.UpdateHeight();
                            if (m_adjustmentNodeFromParentsLeft[i])
                            {
                                m_adjustmentPath[i - 1].m_left = childNode;
                            }
                            else
                            {
                                m_adjustmentPath[i - 1].m_right = childNode;
                            }

#if DEBUG
                            ++m_leftRotations;
#endif
                        }
                        else
                        {
                            doubleNode = childNode.m_right;
                            currentNode.m_left = doubleNode.m_right;
                            currentNode.UpdateHeight();
                            childNode.m_right = doubleNode.m_left;
                            childNode.UpdateHeight();
                            doubleNode.m_left = childNode;
                            doubleNode.m_right = currentNode;
                            doubleNode.UpdateHeight();
                            if (m_adjustmentNodeFromParentsLeft[i])
                            {
                                m_adjustmentPath[i - 1].m_left = doubleNode;
                            }
                            else
                            {
                                m_adjustmentPath[i - 1].m_right = doubleNode;
                            }
#if DEBUG
                            ++m_leftDoubleRotations;
#endif
                        }
                    }
                    else if (diffHeight < -1)
                    {
                        childNode = currentNode.m_right;

                        leftChildHeight = childNode.LeftHeight();
                        rightChildHeight = childNode.RightHeight();

                        if (leftChildHeight < rightChildHeight || leftChildHeight == rightChildHeight)
                        {
                            currentNode.m_right = childNode.m_left;
                            childNode.m_left = currentNode;
                            currentNode.UpdateHeight();
                            childNode.UpdateHeight();
                            if (m_adjustmentNodeFromParentsLeft[i])
                            {
                                m_adjustmentPath[i - 1].m_left = childNode;
                            }
                            else
                            {
                                m_adjustmentPath[i - 1].m_right = childNode;
                            }

#if DEBUG
                            ++m_rightRotations;
#endif
                        }
                        else
                        {
                            doubleNode = childNode.m_left;
                            currentNode.m_right = doubleNode.m_left;
                            currentNode.UpdateHeight();
                            childNode.m_left = doubleNode.m_right;
                            childNode.UpdateHeight();
                            doubleNode.m_left = currentNode;
                            doubleNode.m_right = childNode;
                            doubleNode.UpdateHeight();
                            if (m_adjustmentNodeFromParentsLeft[i])
                            {
                                m_adjustmentPath[i - 1].m_left = doubleNode;
                            }
                            else
                            {
                                m_adjustmentPath[i - 1].m_right = doubleNode;
                            }

#if DEBUG
                            ++m_rightDoubleRotations;
#endif
                        }
                    }
                    else
                    {
                        currentNode.m_height = leftHeight > rightHeight ? leftHeight : rightHeight;
                        ++currentNode.m_height;
                    }
                }

                m_root = m_rootPointer.m_left;
                return true;
            }
        }
    }

    public override string ToString()
    {
        return "Count=" + Count;
    }

    internal List<TreeNode> TestInOrderTraversal(bool a_showNodesWithConsoleWriteLine)
    {
        List<TreeNode> nodes = new ();
        Traverse(m_root, nodes);

        if (a_showNodesWithConsoleWriteLine)
        {
            for (int i = 0; i < nodes.Count; ++i)
            {
                TreeNode n = nodes[i];
                Console.WriteLine(n.GetKeyString());
            }
        }

        for (int i = 1; i < nodes.Count; ++i)
        {
            int compare = Compare(nodes[i - 1], nodes[i]);
            if (compare != -1)
            {
                throw new Exception("Node " + i + " in node list isn't correct.");
            }
        }

        return nodes;
    }

    internal void Traverse(TreeNode a_n, List<TreeNode> a_nodes)
    {
        if (a_n != null)
        {
            Traverse(a_n.m_left, a_nodes);
            a_nodes.Add(a_n);
            Traverse(a_n.m_right, a_nodes);
        }
    }

    internal int TestHeightAndBalance()
    {
        return TestHeightAndBalance(m_root);
    }

    internal int TestHeightAndBalance(TreeNode a_n)
    {
        int height = -1;

        if (a_n != null)
        {
            int leftHeight = TestHeightAndBalance(a_n.m_left);
            int rightHeight = TestHeightAndBalance(a_n.m_right);

            if (Math.Abs(leftHeight - rightHeight) > 1)
            {
                throw new Exception("Tree out of balance.");
            }

            height = leftHeight > rightHeight ? leftHeight : rightHeight;
            ++height;

            if (height != a_n.m_height)
            {
                throw new Exception("The height of the node is wrong.");
            }

            if (a_n.m_left == null && a_n.m_right == null && a_n.m_height != 0)
            {
                throw new Exception("Height of leaf not 0");
            }
        }

        return height;
    }

    internal int MinLeafDepth()
    {
        int minDepth = int.MaxValue;
        MinLeafDepth(m_root, 0, ref minDepth);

        if (minDepth == int.MaxValue)
        {
            return 0;
        }

        return minDepth;
    }

    private static void MinLeafDepth(TreeNode a_n, int a_prevDepth, ref int r_minDepth)
    {
        if (a_n != null)
        {
            int newDepth = a_prevDepth + 1;

            if (newDepth < r_minDepth)
            {
                if (a_n.m_left == null && a_n.m_right == null)
                {
                    if (newDepth < r_minDepth)
                    {
                        r_minDepth = newDepth;
                    }
                }
                else
                {
                    MinLeafDepth(a_n.m_left, newDepth, ref r_minDepth);
                    MinLeafDepth(a_n.m_right, newDepth, ref r_minDepth);
                }
            }
        }
    }

    public static long MinAVLElements(long a_height)
    {
        if (a_height == -1)
        {
            return 0;
        }

        long[] n = new long[a_height + 2];
        n[0] = 1;
        n[1] = 2;

        for (int h = 2; h <= a_height; ++h)
        {
            n[h] = n[h - 1] + n[h - 2] + 1;
        }

        return n[a_height];
    }

    public TreeNode Find(KeyType a_key)
    {
        m_keyNode.SetKey(a_key);
        TreeNode currentNode = m_root;

        while (currentNode != null)
        {
            int compare = Compare(m_keyNode, currentNode);

            if (compare == 0)
            {
                return currentNode;
            }

            if (compare == -1)
            {
                currentNode = currentNode.m_left;
            }
            else
            {
                currentNode = currentNode.m_right;
            }
        }

        return null;
    }

    #region IEnumerable<AVLTree<KeyType, DataType>.Node>
    /// <summary>
    /// Returns an ordered enumerator
    /// </summary>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <summary>
    /// Returns an ordered enumerator.
    /// </summary>
    /// <returns>A sorted enumerator.</returns>
    IEnumerator<KeyValuePair<KeyType, ValueType>>
        IEnumerable<KeyValuePair<KeyType, ValueType>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Returns an ordered enumerator
    /// </summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    /// <summary>
    /// Returns an ordered enumerator starting at the specified key. This provides a
    /// fast way to start enumerating from a specified key.
    /// If the key isn't in the tree, the enumerator will be set to start at the first
    /// element less than the search key. If no elements in the set are <= the
    /// search key, MoveNext() will return false.
    /// </summary>
    /// <param name="a_key">The key you want the enumerator at or before.</param>
    /// <returns>A sorted enumerator.</returns>
    public Enumerator GetEnumeratorAtOrBefore(KeyType a_key)
    {
        return new Enumerator(this, a_key);
    }
}

#region Test Tree
public class AVLTreeTester
{
    public class TestData
    {
        public TestData(int a_key, int a_data)
        {
            m_key = a_key;
            m_data = a_data;
        }

        public TestData(TestData a_td)
        {
            m_key = a_td.m_key;
            m_data = a_td.m_data;
        }

        public int m_key;
        public int m_data;
    }

    public class TestDataComparer : IComparer<TestData>
    {
        #region IComparer<TestData> Members
        public int Compare(TestData a_x, TestData a_y)
        {
            if (a_x.m_key < a_y.m_key)
            {
                return -1;
            }

            if (a_x.m_key > a_y.m_key)
            {
                return 1;
            }

            return 0;

            //                return Comparer<int>.Default.Compare(a_x.m_key, a_y.m_key);
        }
        #endregion
    }

    internal void RandomTreeInsertDeleteTestsWithAVLAdherenceTests(int seed, int a_maxTests, int a_maxNbrOfRandomNbrsToInsertPerTest)
    {
        //Console.WriteLine("********************************************************************************");
        //LinkedList<ActivityKeyTree<TestData, TestData>.Node> randNbrs = new LinkedList<ActivityKeyTree<TestData, TestData>.Node>();
        //TestDataComparer tdc = new TestDataComparer();
        //ActivityKeyTree<TestData, TestData> tree = new ActivityKeyTree<TestData, TestData>(tdc);

        for (int ri = 1; ri <= a_maxTests; ++ri)
        {
            LinkedList<AVLTree<TestData, TestData>.TreeNode> randNbrs = new ();
            TestDataComparer tdc = new ();
            AVLTree<TestData, TestData> tree = new (tdc);

            {
                Random r = new (seed);
                HashSet<int> hs = new ();

                for (int rng = 0; rng < a_maxNbrOfRandomNbrsToInsertPerTest; ++rng)
                {
                    int rand = r.Next();
                    while (hs.Contains(rand))
                    {
                        rand = r.Next();
                    }

                    TestData td = new (rand, rng);
                    randNbrs.AddLast(new AVLTree<TestData, TestData>.TreeNode(td, td));
                    hs.Add(rand);
                }
            }

            //tree.Clear();

            foreach (AVLTree<TestData, TestData>.TreeNode node in randNbrs)
            {
                tree.Add(node);
            }

            {
                Console.WriteLine("Test " + ri + "/" + a_maxTests);

                List<AVLTree<TestData, TestData>.TreeNode> list = tree.TestInOrderTraversal(false);
                Console.WriteLine("Node Count: " + list.Count);
                if (list.Count != a_maxNbrOfRandomNbrsToInsertPerTest)
                {
                    throw new Exception("Bad node count");
                }

                int height = tree.TestHeightAndBalance();
                Console.WriteLine("Height: " + height);
                long minHeight = AVLTree<TestData, TestData>.MinAVLElements(height);
                Console.WriteLine("Min AVL Nodes for height: " + minHeight);
                if (list.Count < minHeight)
                {
                    throw new Exception("Min number of elements for height violated.");
                }

                double maxCapacity = Math.Pow(2, height + 1) - 1;
                double percentFull = a_maxNbrOfRandomNbrsToInsertPerTest / maxCapacity * 100;
                Console.WriteLine("% Full: " + percentFull);
                Console.WriteLine("Min Leaf Depth: " + tree.MinLeafDepth());

                DisplayRotations(tree);
            }

            {
                LinkedListNode<AVLTree<TestData, TestData>.TreeNode> randNode = randNbrs.First;
                Random r = new ();

                while (randNode != null)
                {
                    LinkedListNode<AVLTree<TestData, TestData>.TreeNode> randNodeTemp = randNode;
                    randNode = randNode.Next;

                    if (r.Next() % 2 == 0)
                    {
                        TestData td = new (randNodeTemp.Value.Key.m_key, randNodeTemp.Value.Key.m_data);
                        tree.Remove(td);
                        randNbrs.Remove(randNodeTemp);
                    }
                }

                tree.TestHeightAndBalance();
                List<AVLTree<TestData, TestData>.TreeNode> nodes = tree.TestInOrderTraversal(false);

                foreach (AVLTree<TestData, TestData>.TreeNode node in randNbrs)
                {
                    TestData td = new (node.Key);
                }

                {
                    Console.WriteLine("Test " + ri + "/" + a_maxTests);

                    List<AVLTree<TestData, TestData>.TreeNode> list = tree.TestInOrderTraversal(false);
                    Console.WriteLine("Node Count: " + list.Count);

                    int height = tree.TestHeightAndBalance();
                    Console.WriteLine("Height: " + height);
                    long minHeight = AVLTree<TestData, TestData>.MinAVLElements(height);
                    Console.WriteLine("Min AVL Nodes for height: " + minHeight);
                    if (list.Count < minHeight)
                    {
                        throw new Exception("Min number of elements for height violated.");
                    }

                    double maxCapacity = Math.Pow(2, height + 1) - 1;
                    double percentFull = a_maxNbrOfRandomNbrsToInsertPerTest / maxCapacity * 100;
                    Console.WriteLine("% Full: " + percentFull);
                    Console.WriteLine("Min Leaf Depth: " + tree.MinLeafDepth());

                    DisplayRotations(tree);
                }

                //////////{
                //////////    ActivityKeyTree<TestData, TestData>.Node n = tree.Find(randNbrs[i], i);
                //////////    if (n == null)
                //////////    {
                //////////        throw new Exception("Node not found");
                //////////    }

                //////////    tree.Delete(randNbrs[i], i);

                //////////    List<ActivityKeyTree<TestData, TestData>.Node> list = tree.TestInOrderTraversal(false);
                //////////    int remaining = MAX_RAND_NBRS - i - 1;
                //////////    if (list.Count != remaining)
                //////////    {
                //////////        throw new Exception("Bad node count");
                //////////    }

                //////////    int height = tree.TestHeightAndBalance();
                //////////    long minHeight = ActivityKeyTree<TestData, TestData>.MinAVLElements(height);
                //////////    if (list.Count < minHeight)
                //////////    {
                //////////        throw new Exception("Min number of elements for height violated.");
                //////////    }

                //////////    if (remaining % 100 == 0)
                //////////    {
                //////////        Console.Write(remaining + ";");
                //////////        if (remaining == 0)
                //////////        {
                //////////            Console.WriteLine();
                //////////        }
                //////////    }
                //////////}

                //Console.WriteLine("Delete check complete");
            }

            DisplayRotations(tree);

            Console.WriteLine("********************************************************************************");
        }
    }

    internal void RandomTreeOrderedInsertWithAVLAdherenceTests(int a_maxTests, int a_maxNbrOfNodesToInsertPerTest)
    {
        Console.WriteLine("********************************************************************************");

        TimingSet ts = new (true);

        TestDataComparer tdc = new ();
        AVLTree<TestData, TestData> tree = new (tdc);
        // about 20 seconds at 20,000,000 insertions. About 4 times faster than the microsoft tree.
        //SortedDictionary<TestData, TestData> tree; // about 80 seconds at 20,000,000 insertions.

        for (int ri = 1; ri <= a_maxTests; ++ri)
        {
            tree = new AVLTree<TestData, TestData>(tdc);
            //tree = new SortedDictionary<TestData, TestData>(new TestDataComparer());

            ts.Start();

            for (int i = 0; i < a_maxNbrOfNodesToInsertPerTest; ++i)
            {
                TestData td = new (i, i);
                tree.Add(new AVLTree<TestData, TestData>.TreeNode(td, td));
                //tree.Add(td, td);
            }

            ts.Stop();

            {
                Console.WriteLine("Test " + ri + "/" + a_maxTests);

                Console.WriteLine(ts.ToString());
            }

            Console.WriteLine("********************************************************************************");
        }
    }

    private static void DisplayRotations(AVLTree<TestData, TestData> a_tree)
    {
#if DEBUG
        Console.WriteLine("Left Rotations: " + a_tree.LeftRotations);
        Console.WriteLine("Left Double Rotations: " + a_tree.LeftDoubleRotations);
        Console.WriteLine("Right Rotations: " + a_tree.RightRotations);
        Console.WriteLine("Right Double Rotations: " + a_tree.RightDoubleRotations);
        Console.WriteLine("Total Rotations: " + a_tree.GetTotalRotations());
#endif
    }

    internal void TestInOrderedInsertions(int a_nbrElements)
    {
        TimingSet ts = new (false);

        Console.WriteLine("Test Ordered" + a_nbrElements);
        TestDataComparer tdc = new ();
        AVLTree<TestData, TestData> tree = new (tdc);

        int[] ds = new int[a_nbrElements];

        for (int i = 0; i < a_nbrElements; ++i)
        {
            ds[i] = i;
        }

        ts.Start();
        int mid = a_nbrElements / 2;
        bool even = a_nbrElements % 2 == 0;
        int i1, i2, j1, j2;

        if (even)
        {
            i1 = mid + 1;
            i2 = mid;
        }
        else
        {
            InsertHelper(tree, ds[mid]);

            i1 = mid + 2;
            i2 = mid + 1;
        }

        j1 = mid - 2;
        j2 = mid - 1;

        while (j1 >= 0)
        {
            InsertHelper(tree, ds[i1]);
            InsertHelper(tree, ds[j1]);
            InsertHelper(tree, ds[i2]);
            InsertHelper(tree, ds[j2]);

            i1 += 2;
            i2 += 2;
            j1 -= 2;
            j2 -= 2;
        }

        if (j1 == -1)
        {
            if (even)
            {
                InsertHelper(tree, ds[i2]);
                InsertHelper(tree, ds[j2]);
            }
            else
            {
                InsertHelper(tree, ds[i2]);
                InsertHelper(tree, ds[j2]);
            }
        }

        ts.Stop();
        Console.WriteLine("Total Time=" + ts);

        List<AVLTree<TestData, TestData>.TreeNode> list = tree.TestInOrderTraversal(false);
        if (list.Count != a_nbrElements)
        {
            throw new Exception("Wrong number of elements " + list.Count);
        }

        for (int i = 0; i < a_nbrElements; ++i)
        {
            TestData td = new (ds[i], ds[i]);
            AVLTree<TestData, TestData>.TreeNode n = tree.Find(td);
            if (n == null)
            {
                throw new Exception("Not Found");
            }

            if (n.Key.m_key != ds[i])
            {
                throw new Exception("Incorrect element");
            }
        }

        DisplayRotations(tree);
    }

    private void InsertHelper(AVLTree<TestData, TestData> a_tree, int a_d)
    {
        TestData td = new (a_d, a_d);
        if (a_tree.Find(td) != null)
        {
            throw new Exception("Already in tree");
        }

        a_tree.Add(new AVLTree<TestData, TestData>.TreeNode(td, td));
    }

    internal void TestInsertions(int a_nbrElements)
    {
        Console.WriteLine("Test insertions" + a_nbrElements);
        AVLTree<TestData, TestData> tree = new (m_tdc);

        TimingSet ts = new (false);
        ts.Start();
        for (int i = 0; i < a_nbrElements; ++i)
        {
            InsertHelper(tree, i);
        }

        ts.Stop();
        Console.WriteLine("Total Time=" + ts);

        DisplayRotations(tree);
    }

    #region IComparer<Node> Members
    private readonly TestDataComparer m_tdc = new ();

    public int Compare(AVLTree<TestData, TestData>.TreeNode a_x, AVLTree<TestData, TestData>.TreeNode a_y)
    {
        return m_tdc.Compare(a_x.Key, a_y.Key);
    }
    #endregion
}
#endregion