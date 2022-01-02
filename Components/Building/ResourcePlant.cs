using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ResourcePlant : IComponentData
{
    public byte resourceType;
    public ushort linkedNodes;
    public ushort laborForce;
    public float percentageUntilIncrease;
}
