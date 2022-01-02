using Unity.Entities;
using Unity.Mathematics;

public struct SendWaypointData
{
    public ushort networkID;
    public uint arrivalTick;
    public float3 point;
    public byte send;
}
