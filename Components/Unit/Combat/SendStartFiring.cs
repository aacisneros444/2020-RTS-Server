using Unity.Entities;

public struct SendStartFiring : IComponentData
{
    public uint startFiringTick;
    public ushort roundsInTheMagazine;
    public ushort roundsInTheMagazineC;
}
