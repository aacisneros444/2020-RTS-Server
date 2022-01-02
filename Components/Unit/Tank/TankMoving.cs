using Unity.Entities;

public struct TankMoving : IComponentData
{
    public uint startTick;
    public int pathIndex;
    public float currentMovementSpeed;
    public byte facingWaypoint;
    public byte checkAngleSize;
    public byte rotateInPlace;
    public byte reverse;
    public byte stuckInRotateInPlace;
    public float dotProductLastFrame;
    public byte sentNextWaypoint;
}
