using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct VerticalCorrectionRaycastDataTank : IComponentData
{
    public Entity backLeft;
    public Entity backRight;
    public Entity frontLeft;
    public Entity frontRight;
}
