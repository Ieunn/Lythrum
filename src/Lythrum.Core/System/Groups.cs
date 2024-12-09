using Arch.System;

namespace Lythrum.Core;

public class GroupConfig
{
    public string Name { get; }
    public int Order { get; }

    public GroupConfig(string name, int order)
    {
        Name = name;
        Order = order;
    }
}

public class Groups
{
    private readonly SortedDictionary<int, Group<float>> _orderedGroups = new();

    public Groups(
        Dictionary<string, Group<float>> groups,
        Dictionary<string, GroupConfig> configs)
    {
        foreach (var (groupName, group) in groups)
        {
            var config = configs[groupName];
            _orderedGroups[config.Order] = group;
        }
    }
    
    public void AddGroup(Group<float> group, int order)
    {
        _orderedGroups[order] = group;
    }
    
    public void Initialize()
    {
        foreach (var group in _orderedGroups.Values)
        {
            group.Initialize();
        }
    }
    
    public void BeforeUpdate(float deltaTime)
    {
        foreach (var group in _orderedGroups.Values)
        {
            group.BeforeUpdate(deltaTime);
        }
    }
    
    public void Update(float deltaTime)
    {
        foreach (var group in _orderedGroups.Values)
        {
            group.Update(deltaTime);
        }
    }
    
    public void AfterUpdate(float deltaTime)
    {
        foreach (var group in _orderedGroups.Values)
        {
            group.AfterUpdate(deltaTime);
        }
    }
    
    public void Dispose()
    {
        foreach (var group in _orderedGroups.Values)
        {
            group.Dispose();
        }
    }
}