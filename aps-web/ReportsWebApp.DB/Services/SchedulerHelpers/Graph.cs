using ReportsWebApp.DB.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ReportsWebApp.DB.Services
{
    public class Graph
    {
        private readonly ConcurrentDictionary<long, ConcurrentBag<long>> _adjacencyList;
        private readonly ConcurrentDictionary<(long, long), Dependency> _dependencyMap;
        private readonly ConcurrentBag<Dependency> _cycledDependencies;

        public Graph()
        {
            _adjacencyList = new ConcurrentDictionary<long, ConcurrentBag<long>>();
            _dependencyMap = new ConcurrentDictionary<(long, long), Dependency>();
            _cycledDependencies = new ConcurrentBag<Dependency>();
        }

        public void AddEdge(long from, long to, Dependency dependency)
        {
            _adjacencyList.GetOrAdd(from, new ConcurrentBag<long>()).Add(to);
            _dependencyMap[(from, to)] = dependency;
        }

        public void DetectAndDeactivateCycles()
        {
            bool hasCycle;
            do
            {
                var cycledDependencies = GetCycledDependencies();
                hasCycle = cycledDependencies.Any();

                if (hasCycle)
                {
                    foreach (var dep in cycledDependencies)
                    {
                        dep.IsActive = false;  // Deactivate the dependency
                    }

                    // Remove deactivated dependencies from the graph
                    var activeDependencies = _dependencyMap.Values.Where(dep => dep.IsActive).ToList();

                    // Clear the graph and re-add active dependencies
                    Clear();
                    foreach (var dep in activeDependencies)
                    {
                        AddEdge(dep.From, dep.To, dep);
                    }
                }

                // Clear cycled dependencies for the next iteration
                ClearCycledDependencies();

            } while (hasCycle);
        }

        public HashSet<Dependency> GetCycledDependencies()
        {
            var visited = new ConcurrentDictionary<long, bool>();
            var recStack = new ConcurrentDictionary<long, bool>();

            foreach (var node in _adjacencyList.Keys)
            {
                DetectCyclesInStack(node, visited, recStack);
            }

            return new HashSet<Dependency>(_cycledDependencies);
        }

        private bool DetectCyclesInStack(long node, ConcurrentDictionary<long, bool> visited, ConcurrentDictionary<long, bool> recStack)
        {
            if (recStack.ContainsKey(node))
            {
                return true;
            }

            if (visited.ContainsKey(node))
            {
                return false;
            }

            visited[node] = true;
            recStack[node] = true;

            if (_adjacencyList.ContainsKey(node))
            {
                foreach (var neighbor in _adjacencyList[node])
                {
                    if (DetectCyclesInStack(neighbor, visited, recStack))
                    {
                        _cycledDependencies.Add(_dependencyMap[(node, neighbor)]);
                        return true;
                    }
                }
            }

            recStack.TryRemove(node, out _);
            return false;
        }

        public void ClearCycledDependencies()
        {
            _cycledDependencies.Clear();
        }

        public void Clear()
        {
            _adjacencyList.Clear();
            _dependencyMap.Clear();
        }
    }
}
