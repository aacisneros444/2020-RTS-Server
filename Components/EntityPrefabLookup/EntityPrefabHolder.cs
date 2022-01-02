using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct EntityPrefabHolder : IComponentData
{
    public Entity DropPod;
    public Entity SoldierStart;
    public Entity StorageCrateBuildPlan;
    public Entity StorageCrateBuildSite;
    public Entity StorageCrate;
    public Entity BarracksBuildPlan;
    public Entity BarracksBuildSite;
    public Entity Barracks;
    public Entity MaterialPlantBuildPlan;
    public Entity MaterialPlantBuildSite;
    public Entity MaterialPlant;
    public Entity Soldier;
    public Entity Soldier_Kar98k;
    public Entity Soldier_M1919Browning;
    public Entity Tiger;
}
