using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;
using System.Reflection;

namespace ReportsWebApp.DB.Services
{
    public enum TreeNodeType
    {
        Default,
        CapacityLabel,
        MaterialLink,
        ActivityLink
    }
    public class GanttLabelTemplateTreeNodeUtils
    {
        private static TreeNode CreateTreeNode(Type type, PropertyInfo prop, Dictionary<string, string> exampleData, bool isTo)
        {
            string key = $"{type.Name}{(isTo?"To":"")}.{prop.Name}";
            return new TreeNode
            {
                Text = key,
                Title = prop.Name,
                Value = exampleData.TryGetValue(key, out var value) ? value : string.Empty
            };
        }

        private static List<TreeNode> GetFieldsWithClassPrefix(Type type, Dictionary<string, string> exampleData, bool isTo=false)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                       .Select(prop => CreateTreeNode(type, prop, exampleData, isTo))
                       .ToList();
        }

        private static TreeNode CreateTabNode(string tabName, List<TreeNode> fields)
        {
            return new TreeNode
            {
                Text = tabName,
                Children = fields
            };
        }

        public static List<TreeNode> GenerateTreeNodes(Dictionary<string, string> exampleData, TreeNodeType treeType)
        {
            var tabFields = GetTabFields(exampleData);

            return treeType switch
            {
                TreeNodeType.CapacityLabel => new List<TreeNode>
                {
                    CreateTabNode("Capacity", tabFields["Capacity"])
                },
                TreeNodeType.MaterialLink or TreeNodeType.ActivityLink => new List<TreeNode>
                {
                    CreateTabNode("From", tabFields["Planning"]),
                    CreateTabNode("To", tabFields["PlanningTo"]),
                    //CreateTabNode("Resources", tabFields["Resources"]),
                    CreateTabNode("Materials", tabFields["Materials"])
                },
                _ => new List<TreeNode>
                {
                    CreateTabNode("Planning", tabFields["Planning"]),
                    //CreateTabNode("Materials", tabFields["Materials"])
                }
            };
        }

        private static Dictionary<string, List<TreeNode>> GetTabFields(Dictionary<string, string> exampleData)
        {
            var capacityTabFields = GetFieldsWithClassPrefix(typeof(DashtCapacityPlanningShiftsCombined), exampleData);
            var planningTabFields = GetFieldsWithClassPrefix(typeof(DashtPlanning), exampleData);
            var planningTabFieldsTo = GetFieldsWithClassPrefix(typeof(DashtPlanning), exampleData, true);
            var resourceTabFields = GetFieldsWithClassPrefix(typeof(DashtResource), exampleData);
            var marerialsTabFields = GetFieldsWithClassPrefix(typeof(Dasht_Materials), exampleData);

            return new Dictionary<string, List<TreeNode>>
            {
                { "Planning", planningTabFields },
                { "PlanningTo", planningTabFieldsTo },
                { "Resources", resourceTabFields },
                { "Capacity", capacityTabFields },
                { "Materials", marerialsTabFields }
            };
        }
    }
}
