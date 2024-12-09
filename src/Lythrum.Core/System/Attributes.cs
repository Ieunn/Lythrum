namespace Lythrum.Core;

[AttributeUsage(AttributeTargets.Class)]
public class GroupAttribute : Attribute
{
    public string GroupName { get; }
    public int SystemOrder { get; }

    public GroupAttribute(string groupName = "Default", int systemOrder = 0)
    {
        GroupName = groupName;
        SystemOrder = systemOrder;
    }
}