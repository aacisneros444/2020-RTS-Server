using Unity.Entities;

[GenerateAuthoringComponent]
public struct BuildSite : IComponentData
{
    public byte hasAllResources;
    public ushort finalBuildPrefabID;
}
