using Unity.Entities;

[GenerateAuthoringComponent]
public struct VerticalCorrectionRaycastData : IComponentData
{
    public float raycastYOrigin;
    public float raycastDistance;
}
