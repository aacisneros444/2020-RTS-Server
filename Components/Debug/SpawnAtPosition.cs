using Unity.Entities;

[GenerateAuthoringComponent]
public struct SpawnAtPosition : IComponentData
{
    public ushort prefabID;
}
