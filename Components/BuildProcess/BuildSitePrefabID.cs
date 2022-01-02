using Unity.Entities;

[GenerateAuthoringComponent]
public struct BuildSitePrefabID : IComponentData
{
    public ushort value;
}
