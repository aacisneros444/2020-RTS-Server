using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Command_BuildOrder : ICommand
{

    public ushort buildingNetworkID;
    public List<ushort> builderUnitNetworkIDs;

    public Command_BuildOrder(ushort buildingNetworkID, List<ushort> builderUnitNetworkIDs)
    {
        this.buildingNetworkID = buildingNetworkID;
        this.builderUnitNetworkIDs = builderUnitNetworkIDs;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!NetworkEntityManager.networkEntities.ContainsKey(buildingNetworkID))
            return;

        Entity building = NetworkEntityManager.networkEntities[buildingNetworkID];

        if (!entityManager.Exists(building))
            return;
        
        for (int i = 0; i < builderUnitNetworkIDs.Count; i++)
        {
            ushort unitNetworkID = builderUnitNetworkIDs[i];

            if (!NetworkEntityManager.networkEntities.ContainsKey(unitNetworkID))
                return;

            Entity entity = NetworkEntityManager.networkEntities[unitNetworkID];

            entityManager.AddComponent<BuildTasked>(entity);
            entityManager.SetComponentData(entity, new BuildTasked { assignedBuildSite = building });
        }
    }
}
