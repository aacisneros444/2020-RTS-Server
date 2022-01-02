using Unity.Entities;
using Unity.Mathematics;

public struct HasTarget : IComponentData
{
    public Entity entity;
    public byte targetType;
    public byte ticksWithoutObstacleCheck;
}
