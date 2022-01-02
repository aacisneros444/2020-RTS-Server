using Unity.Entities;

public struct DeliverResourceToInventory : IComponentData
{
    public int resourceType;
    public int amount;
    public Entity entityToDeliverTo;
}
