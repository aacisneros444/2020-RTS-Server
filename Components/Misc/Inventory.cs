using Unity.Entities;

[GenerateAuthoringComponent]
public struct Inventory : IComponentData
{
    public int resource;
    public int maxResource;
    public int lastSendValue;
}
