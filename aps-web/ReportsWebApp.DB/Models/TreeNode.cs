using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class TreeNode
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Text { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public List<TreeNode> Children { get; set; } = new();
    }

    public static class TreeNodeExtensions
    {
        public static TreeNode? RecursiveFind(this TreeNode node, string id)
        {
            TreeNode? treeNode = node?.Children.Find(n => n.Id == id);
            if (treeNode != null)
            {
                return treeNode;
            }
            
            foreach (TreeNode nodeChild in node.Children)
            {
                TreeNode? item = nodeChild.RecursiveFind(id);
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
