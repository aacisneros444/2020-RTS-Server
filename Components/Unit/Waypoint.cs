using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct Waypoint : IBufferElementData
{
    public float3 point;
    public uint arrivalTick;
}
