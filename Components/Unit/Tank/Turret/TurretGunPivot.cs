using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct TurretGunPivot : IComponentData
{
    public float elevationSpeed;
    public float3 clamp;
}
