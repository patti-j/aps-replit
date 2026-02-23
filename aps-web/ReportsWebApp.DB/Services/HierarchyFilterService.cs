using ReportsWebApp.DB.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Services
{
    public class HierarchyFilterService
    {
        private List<TreeNode> Nodes { get; set; } = new List<TreeNode>();

        public async Task LoadDataAsync(BryntumProject scenario)
        {
            List<Event> Events = scenario.Events;
            await InitializeTreeViewDataAsync(Events, scenario);
            scenario.HierarchyFilterNodes = Nodes;
        }

        public async Task InitializeTreeViewDataAsync(List<Event> Events, BryntumProject scenario)
        {
            var plants = new Dictionary<(string, int), TreeNode>();
            var departments = new Dictionary<(string, int, int), TreeNode>();
            var resources = new HashSet<string>();

            foreach (var evt in Events)
            {
                var scenarioId = $"Scenario-{evt.PAScenarioId}";
                var plantId = $"Plant-{evt.PlantId}";
                var departmentId = $"Department-{evt.DepartmentId}";
                var resourceId = $"Resource-{evt.ResourceId}";
                // Add plant node if it doesn't exist
                if (!plants.ContainsKey((evt.PAScenarioId, evt.PlantId)))
                {
                    var plantNode = new TreeNode
                    {
                        Id = plantId,
                        Text = evt.BlockPlant,
                        ParentId = scenarioId,
                        Children = new List<TreeNode>()
                    };
                    plants[(evt.PAScenarioId, evt.PlantId)] = plantNode;
                }

                // Add department node if it doesn't exist
                if (!departments.ContainsKey((evt.PAScenarioId, evt.PlantId, evt.DepartmentId)))
                {
                    var deptNode = new TreeNode
                    {
                        Id = departmentId,
                        Text = evt.BlockDepartment,
                        ParentId = plantId,
                        Children = new List<TreeNode>()
                    };
                    departments[(evt.PAScenarioId, evt.PlantId, evt.DepartmentId)] = deptNode;
                    plants[(evt.PAScenarioId, evt.PlantId)].Children.Add(deptNode);
                }

                // Add resource node if it doesn't exist
                if (resources.Add(resourceId))
                {
                    var resourceNode = new TreeNode
                    {
                        Id = resourceId,
                        Text = evt.ResourceName,
                        ParentId = departmentId
                    };
                    departments[(evt.PAScenarioId, evt.PlantId, evt.DepartmentId)].Children.Add(resourceNode);
                }
            }

            // Root nodes are scenarios
            Nodes = plants.Values.ToList();
            
            scenario.Resources.ForEach(r =>
            {
                if (r.Enabled == false)
                {
                    TreeNode? node = null;
                    _ = Nodes.Any(n => (node = n.RecursiveFind($"Resource-{r.Id}")) != null);
                    if (node != null)
                    {
                        node.IsEnabled = false;
                    }
                    
                }
            });

            await DisableNodesWithDisabledChildren(Nodes);
        }

        private async Task DisableNodesWithDisabledChildren(List<TreeNode> nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Children.Any(c => c.Children.Any())) //does any of this nodes children have children?
                {
                    await DisableNodesWithDisabledChildren(node.Children);
                }

                if (node.Children.All(c => !c.IsEnabled))
                {
                    node.IsEnabled = false;
                }
            }
        }

        // Get nodes for the selected scenario (plants, departments, resources only)
        public List<TreeNode> GetNodes()
        {
            return Nodes;
        }
    }

}
