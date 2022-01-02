using Unity.Entities;

[GenerateAuthoringComponent]
public struct ValidateBuild : IComponentData
{
    public byte canBuild;
    public byte processed;
    //0 = false
    //1 = true
    public byte framesUnprocessed;
}
