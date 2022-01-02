using Unity.Entities;

[GenerateAuthoringComponent]
public struct ResourceNode : IComponentData
{
    public byte resourceType;
}
