namespace PT.Common.Collections;

partial class AVLTree<KeyType, ValueType>
{
    public class TreeNode
    {
        public TreeNode(KeyType a_key, ValueType a_data)
        {
            m_key = a_key;
            m_value = a_data;
        }

        protected internal TreeNode() { }

        internal void SetKeyAndValue(TreeNode a_n)
        {
            m_key = a_n.m_key;
            m_value = a_n.m_value;
        }

        protected internal void SetKey(KeyType a_key)
        {
            m_key = a_key;
        }

        protected internal KeyType m_key;

        public KeyType Key => m_key;

        protected internal ValueType m_value;

        public ValueType Value => m_value;

        protected internal void UpdateHeight()
        {
            int leftHeight = m_left?.m_height ?? -1;
            int rightHeight = m_right?.m_height ?? -1;
            m_height = leftHeight > rightHeight ? leftHeight : rightHeight;
            ++m_height;
        }

        protected internal TreeNode m_left;

        public TreeNode Left => m_left;

        protected internal TreeNode m_right;

        public TreeNode Right => m_right;

        protected internal int m_height;

        public int Height => m_height;

        protected internal int LeftHeight()
        {
            return m_left?.m_height ?? -1;
        }

        protected internal int RightHeight()
        {
            return m_right?.m_height ?? -1;
        }

        public override string ToString()
        {
            return base.ToString() + ":" + GetKeyString();
        }

        protected internal string GetKeyString()
        {
            return "{" + m_key + ", " + "; " + m_height + ")";
        }
    }
}