using Unity.Entities;

//Used for identifying child weapons of entities.
[GenerateAuthoringComponent]
public struct LocalWeapon : IComponentData
{
    public byte localID;
}
