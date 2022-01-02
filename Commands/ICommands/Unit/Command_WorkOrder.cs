using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Command_WorkOrder : ICommand
{

    public ushort buildingNetworkID;
    public List<ushort> workerUnitNetworkIDs;

    public Command_WorkOrder(ushort buildingNetworkID, List<ushort> workerUnitNetworkIDs)
    {
        this.buildingNetworkID = buildingNetworkID;
        this.workerUnitNetworkIDs = workerUnitNetworkIDs;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!NetworkEntityManager.networkEntities.ContainsKey(buildingNetworkID))
            return;

        Entity building = NetworkEntityManager.networkEntities[buildingNetworkID];

        if (!entityManager.Exists(building))
            return;

        for (int i = 0; i < workerUnitNetworkIDs.Count; i++)
        {
            ushort unitNetworkID = workerUnitNetworkIDs[i];

            if (!NetworkEntityManager.networkEntities.ContainsKey(unitNetworkID))
                return;

            Entity entity = NetworkEntityManager.networkEntities[unitNetworkID];

            entityManager.AddComponent<WorkTasked>(entity);
            entityManager.SetComponentData(entity, new WorkTasked { assignedWorkSite = building });
        }
    }
}
