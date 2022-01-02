using Unity.Entities;

public struct BuildTasked : IComponentData
{
    public Entity assignedBuildSite;
}
