using Unity.Entities;

[GenerateAuthoringComponent]
public struct LocalWeapons : IComponentData
{
    //All weapons that are children of this entity.
    //Mainly used in for vehicles. Needed when setting teamIDs for target seekers.
    public Entity weapon0;
    public Entity weapon1;
    public Entity weapon2;
    public Entity weapon3;
}
