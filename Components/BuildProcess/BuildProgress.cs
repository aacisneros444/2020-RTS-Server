using Unity.Entities;

[GenerateAuthoringComponent]
public struct BuildProgress : IComponentData
{
    public float value;
    public float requiredValue;
}
