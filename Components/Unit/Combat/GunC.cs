using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct GunC : IComponentData
{
    public byte aimed;
    public float dispersionValue;
    public ushort roundsInMagazine;
    public ushort magazineSize;
    public ushort ticksBetweenShotsTaken;
    public ushort ticksBetweenShots;
    public ushort reloadTicksTaken;
    public ushort reloadTicks;
    public Entity round;
    public Entity muzzle;
}