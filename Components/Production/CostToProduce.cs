using Unity.Entities;

[GenerateAuthoringComponent]
public struct CostToProduce : IComponentData
{
    public int resourceCost;
    public float time;
}
