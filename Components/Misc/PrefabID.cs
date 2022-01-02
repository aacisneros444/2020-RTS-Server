using Unity.Entities;

[GenerateAuthoringComponent]
public struct PrefabID : IComponentData
{
    public ushort value;
}
