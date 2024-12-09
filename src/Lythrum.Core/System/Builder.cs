using Arch.System;

namespace Lythrum.Core;

public class SystemBuilder
{
    private readonly Dictionary<string, List<(ISystem<float> System, int Order)>> _groupedSystems = new();
    
    public SystemBuilder AddToGroup(string groupName, ISystem<float> system, int order = 0)
    {
        if (!_groupedSystems.ContainsKey(groupName))
        {
            _groupedSystems[groupName] = new List<(ISystem<float>, int)>();
        }
        
        _groupedSystems[groupName].Add((system, order));
        return this;
    }

    public Dictionary<string, Group<float>> Build()
    {
        var groups = new Dictionary<string, Group<float>>();
        
        foreach (var (groupName, systems) in _groupedSystems)
        {
            var orderedSystems = systems
                .OrderByDescending(x => x.Order)
                .Select(x => x.System)
                .ToArray();
                
            groups[groupName] = new Group<float>(groupName, orderedSystems);
        }
        
        return groups;
    }
}