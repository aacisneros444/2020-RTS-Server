using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct TargetSeeker : IComponentData
{
    public byte priorityTargetType;
    public float detectionRadius;
    public uint ticksWithoutTarget;
    public float seekOffsetY;
    public float maxAngleToTarget;
    public Entity defaultRotationEntity;
}
