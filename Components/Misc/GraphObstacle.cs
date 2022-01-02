using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct GraphObstacle : IComponentData
{
    public float3 center;
    public float3 size;
}
