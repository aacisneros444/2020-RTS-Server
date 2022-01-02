using Unity.Entities;
using Unity.Mathematics;

public struct RotateTowardsDirection : IComponentData
{
    public float3 direction;
    public byte buffer;
}
