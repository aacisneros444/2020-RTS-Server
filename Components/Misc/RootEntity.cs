using Unity.Entities;

[GenerateAuthoringComponent]
public struct RootEntity : IComponentData
{
    public Entity entity;
}
