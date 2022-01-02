using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Gun : IComponentData
{
    public byte aimed;
    public float dispersionValue;
    public ushort roundsInMagazine;
    public ushort magazineSize;
    public ushort ticksBetweenShotsTaken;
    public ushort ticksBetweenShots;
    public ushort reloadTicksTaken;
    public ushort reloadTicks;
    public byte roundType;
    public Entity round;
    public Entity secondaryRound;
    public Entity muzzle;

    //Client
    public byte clientShooting;
}