using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Turret : IComponentData
{
    public float traverseSpeed;

    //Clamp angles
    public float3 clamp;

    //Keep track of gun pivot
    public Entity gunPivot;
    public byte passedTargetToGunPivot;
}
