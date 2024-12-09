using System.Reflection;
using Arch.System;

namespace Lythrum.Core;

public class SystemRegistry
{
    private readonly SystemBuilder _builder = new();
    private readonly Dictionary<string, GroupConfig> _groupConfig = new();

    public SystemRegistry(IEnumerable<GroupConfig> groups)
    {
        foreach (var group in groups)
        {
            if (_groupConfig.ContainsKey(group.Name))
            {
                throw new ArgumentException($"Duplicate group name: {group.Name}");
            }
            _groupConfig[group.Name] = group;
        }
    }

    public SystemRegistry Register<T>(T system) where T : ISystem<float>
    {
        var attr = typeof(T).GetCustomAttribute<GroupAttribute>();
        var groupName = attr?.GroupName ?? "Default";

        if (!_groupConfig.ContainsKey(groupName))
        {
            throw new InvalidOperationException($"Group '{groupName}' is not defined");
        }

        var systemOrder = attr?.SystemOrder ?? 0;
        _builder.AddToGroup(groupName, system, systemOrder);
        return this;
    }

    public SystemRegistry RegisterMany(params ISystem<float>[] systems)
    {
        foreach (var system in systems)
        {
            Register(system);
        }
        return this;
    }

    public Groups Build()
    {
        var groups = _builder.Build();
        return new Groups(groups, _groupConfig);
    }
}