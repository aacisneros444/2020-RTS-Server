using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct RecalculateGraphOnEntity : IComponentData
{
    public float3 colliderCenter;
    public float3 colliderSize;
}
