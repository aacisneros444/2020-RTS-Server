using Unity.Entities;

public struct SendEntityStateData : IComponentData
{
    public ushort clientID;
    public byte firstSendInventory;
    public byte firstSendHealth;
}
