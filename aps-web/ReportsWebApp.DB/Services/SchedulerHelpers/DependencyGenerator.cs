using ReportsWebApp.DB.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ReportsWebApp.DB.Services
{
    public class DependencyGenerator
    {
        private readonly List<Event> _events;
        private readonly BryntumSettings _settings;
        public readonly TemplateSegmentContext _templateSegmentContext;
        private int _dependencyId = 1;

        public DependencyGenerator(List<Event> events, BryntumSettings settings, TemplateSegmentContext a_templateSegmentContext)
        {
            _events = events;
            _settings = settings;
            _templateSegmentContext = a_templateSegmentContext;
        }

        public List<Dependency> GenerateDependencies()
        {
            var dependencies = InitializeDependencies();
            var visibleEventIds = new HashSet<long>(_events.Select(e => e.Id));
            var graph = new Graph();
            _templateSegmentContext.SetSettings(_settings);

            ProcessEventDependencies(dependencies, visibleEventIds, graph);

            return FilterActiveDependencies(dependencies);
        }

        private ConcurrentDictionary<(long, long, int), Dependency> InitializeDependencies()
        {
            return new ConcurrentDictionary<(long, long, int), Dependency>();
        }

        private void ProcessEventDependencies(ConcurrentDictionary<(long, long, int), Dependency> dependencies, HashSet<long> visibleEventIds, Graph graph)
        {
            foreach (var eventItem in _events)
            {
                ProcessSingleEvent(dependencies, eventItem, visibleEventIds, graph);
            }
        }

        private void ProcessSingleEvent(ConcurrentDictionary<(long, long, int), Dependency> dependencies, Event eventItem, HashSet<long> visibleEventIds, Graph graph)
        {
            var eventSuccessorDict = _events.ToDictionary(e => e.Id, e => e.DependenciesAsSuccessor);
            var eventMaterialsSuccessorDict = _events.ToDictionary(e => e.Id, e => e.MaterialsAsSuccessor);
            var eventMaterialsPredecessorDict = _events.ToDictionary(e => e.Id, e => e.MaterialsAsPredecessor);

            var successors = eventSuccessorDict[eventItem.Id].Where(visibleEventIds.Contains);
            AddUniqueDependencies(dependencies, GenerateDependencies(eventItem, successors, true, _settings.LinkColor, graph, LinkType.ActivityLink));

            var materialSuccessors = eventMaterialsSuccessorDict[eventItem.Id].Where(visibleEventIds.Contains);
            var materialPredecessors = eventMaterialsPredecessorDict[eventItem.Id].Where(visibleEventIds.Contains);
            AddUniqueDependencies(dependencies, GenerateDependencies(eventItem, materialSuccessors, true, _settings.MaterialColor, graph, LinkType.MaterialsLink));
            AddUniqueDependencies(dependencies, GenerateDependencies(eventItem, materialPredecessors, false, _settings.MaterialColor, graph, LinkType.MaterialsLink));
        }

        private List<Dependency> FilterActiveDependencies(ConcurrentDictionary<(long, long, int), Dependency> dependencies)
        {
            return dependencies.Values.Where(dep => dep.IsActive).ToList();
        }

        private void AddUniqueDependencies(ConcurrentDictionary<(long, long, int), Dependency> dependencies, IEnumerable<Dependency> newDependencies)
        {
            foreach (var dependency in newDependencies)
            {
                (long From, long To, int Type) key; 
                if (dependency.DependencyType == DependencyType.StartToEnd)
                {
                    //if we are drawing a link from start to end then this is a backwards link, not bad on its own but since we may also have a 
                    //forward link on the same two blocks we need to be able to know if these blocks have already been linked with the same "real"
                    //link already. so we reverse the link direction and let the dictionary do its thing.
                    
                    //it might be worthwhile to learn graph theory and rewrite most of this code. I'd guess we could make the code much easier to read/understand. TODO: <-- do that
                    key = (dependency.To, dependency.From, DependencyType.EndToStart.Value);
                }
                else
                {
                    key = (dependency.From, dependency.To, dependency.Type);
                }
                
                dependencies.TryAdd(key, dependency);
            }
        }

        private IEnumerable<Dependency> GenerateDependencies(Event eventItem, IEnumerable<long> relatedEventIds, bool isSuccessor, string color, Graph graph, LinkType linkType)
        {
            foreach (var relatedEvent in relatedEventIds.Select(id => _events.FirstOrDefault(e => e.Id == id)))
            {
                if (relatedEvent == null) continue;

                (Event sourceEvent, Event targetEvent) = (eventItem, relatedEvent);
                
                
                if (linkType == LinkType.MaterialsLink)
                {
                    (sourceEvent, targetEvent) = (relatedEvent, eventItem);
                }
                else
                {
                    if (isSuccessor)
                    {
                        if (eventItem.LinkDirection == "Backward")
                        {
                            (sourceEvent, targetEvent) = (relatedEvent, eventItem);
                        }
                        else
                        {
                            (sourceEvent, targetEvent) = (eventItem, relatedEvent);
                        }
                    }
                    else
                    {
                        if (eventItem.LinkDirection == "Backward")
                        {
                            (sourceEvent, targetEvent) = (eventItem, relatedEvent);
                        }
                        else
                        {
                            (sourceEvent, targetEvent) = (relatedEvent, eventItem);
                        }
                    }
                }
                
                
                DependencyType dependencyType;

                if (linkType == LinkType.MaterialsLink)
                {
                    dependencyType = isSuccessor ? DependencyType.EndToStart :  DependencyType.StartToEnd;
                }
                else
                {
                    dependencyType =  DependencyType.FromString(sourceEvent.LinkRelationType);
                }
                
                var (fromElement, toElement) = eventItem.LinkDirection == "Backward"
                    ? (targetEvent, sourceEvent)
                    : (sourceEvent, targetEvent);

                var dependency = new Dependency
                {
                    Id = Interlocked.Increment(ref _dependencyId),
                    From = fromElement.Id,
                    To = toElement.Id,
                    Color = color,
                    LinkType = linkType,
                    DependencyType = dependencyType,
                    Type = dependencyType.Value,
                    IsActive = true
                };

                dependency.TooltipLabel = _templateSegmentContext.GetDependencyTemplateValued(dependency, fromElement.DashtPlanning, toElement.DashtPlanning, toElement.DashtMaterials);

                graph.AddEdge(fromElement.Id, toElement.Id, dependency);

                yield return dependency;
            }
        }
    }
}