using Unity.Entities;
using Unity.Mathematics;

public struct BucketedEntityData
{
    public Entity entity;
    public float3 position;
    public ushort teamID;
    public byte unitType;
}
