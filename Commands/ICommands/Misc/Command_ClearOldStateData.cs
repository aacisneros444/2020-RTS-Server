using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Command_ClearOldStateData : ICommand
{
    public List<ushort> unitNetworkIDs;

    public Command_ClearOldStateData(List<ushort> unitNetworkIDs)
    {
        this.unitNetworkIDs = unitNetworkIDs;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (int i = 0; i < unitNetworkIDs.Count; i++)
        {
            ushort unitNetworkID = unitNetworkIDs[i];

            //Safety check. Make sure entity exists.
            if (!NetworkEntityManager.networkEntities.ContainsKey(unitNetworkID))
                return;

            Entity entity = NetworkEntityManager.networkEntities[unitNetworkID];

            if (entityManager.HasComponent<Moving>(entity))
            {
                entityManager.RemoveComponent<Moving>(entity);
                entityManager.RemoveComponent<PathIndex>(entity);
            }

            if (entityManager.HasComponent<RotateTowardsDirection>(entity))
            {
                entityManager.RemoveComponent<RotateTowardsDirection>(entity);
            }

            if (entityManager.HasComponent<BuildTasked>(entity))
            {
                entityManager.RemoveComponent<BuildTasked>(entity);
            }

            if (entityManager.HasComponent<DeliverResourceToInventory>(entity))
            {
                entityManager.RemoveComponent<DeliverResourceToInventory>(entity);
            }

            if (entityManager.HasComponent<WorkingOnBuild>(entity))
            {
                entityManager.RemoveComponent<WorkingOnBuild>(entity);
            }

            if (entityManager.HasComponent<WorkTasked>(entity))
            {
                entityManager.RemoveComponent<WorkTasked>(entity);
            }

            if (entityManager.HasComponent<Working>(entity))
            {
                Entity resourcePlant = entityManager.GetComponentData<Working>(entity).workSite;

                ResourcePlant resourcePlant0 = entityManager.GetComponentData<ResourcePlant>(resourcePlant);
                resourcePlant0.laborForce--;

                entityManager.SetComponentData(resourcePlant, resourcePlant0);
                entityManager.RemoveComponent<Working>(entity);
            }

            if (entityManager.HasComponent<TankMoving>(entity))
            {
                entityManager.RemoveComponent<TankMoving>(entity);
            }
        }
    }
}
