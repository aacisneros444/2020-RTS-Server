using Unity.Entities;

[GenerateAuthoringComponent]
public struct Health : IComponentData
{
    public float value;
    public float maxValue;
    public float lastSendValue;
}
