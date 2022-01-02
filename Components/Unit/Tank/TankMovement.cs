using Unity.Entities;

[GenerateAuthoringComponent]
public struct TankMovement : IComponentData
{
    public float movementSpeed;
    public float acceleration;
    public float traverseSpeed;
    public float maxMoveAndTurnAngle;
}
