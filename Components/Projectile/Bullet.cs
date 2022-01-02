using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Bullet : IComponentData
{
    public float speed;
    public float damage;
    public float activeTime;
    public float lifetime;
}
