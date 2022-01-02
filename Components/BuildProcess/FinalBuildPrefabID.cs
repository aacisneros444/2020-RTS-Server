using Unity.Entities;

[GenerateAuthoringComponent]
public struct FinalBuildPrefabID : IComponentData
{
    public ushort value;
}
