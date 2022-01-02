using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ProducerCreationLocationEntity : IComponentData
{
    public Entity entity;
}
