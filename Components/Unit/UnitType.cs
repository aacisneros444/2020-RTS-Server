using Unity.Entities;

[GenerateAuthoringComponent]
public struct UnitType : IComponentData
{
    public byte value;
    //0 = soldier
    //1 = emplacement
    //2 = tank
}
