using Unity.Entities;

/// <summary>
/// This is a tag component used for efficient querying. Needed for differences in behavior for firing.
/// Used in TurretSystem.
/// </summary>

[GenerateAuthoringComponent]
public struct Humanoid : IComponentData
{

}
