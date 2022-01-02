using Unity.Entities;

public struct ProducerQueueElement : IBufferElementData
{
    public ushort prefabID;
    public float timeToProduce;
    public float timeElapsed;
    public int resourceCost;
    public byte inProduction;
}
