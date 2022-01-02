using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class Command_MoveOrderWithEndRotation : ICommand
{
    public float clickPointX;
    public float clickPointY;
    public float clickPointZ;
    public float rotateDirectionX;
    public float rotateDirectionZ;
    public List<ushort> unitNetworkIDs;

    public Command_MoveOrderWithEndRotation(float clickPointX, float clickPointY, float clickPointZ,
        float rotateDirectionX, float rotateDirectionZ, List<ushort> unitNetworkIDs)
    {
        this.clickPointX = clickPointX;
        this.clickPointY = clickPointY;
        this.clickPointZ = clickPointZ;
        this.rotateDirectionX = rotateDirectionX;
        this.rotateDirectionZ = rotateDirectionZ;
        this.unitNetworkIDs = unitNetworkIDs;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Vector3 clickPoint = new Vector3(clickPointX, clickPointY, clickPointZ);

        //Multiple Units
        if (unitNetworkIDs.Count > 1)
        {
            float radius = math.sqrt(unitNetworkIDs.Count * 5f);

            for (int i = 0; i < unitNetworkIDs.Count; i++)
            {
                ushort unitNetworkID = unitNetworkIDs[i];

                if (!NetworkEntityManager.networkEntities.ContainsKey(unitNetworkID))
                    return;

                Entity entity = NetworkEntityManager.networkEntities[unitNetworkID];
                Translation entityPos = entityManager.GetComponentData<Translation>(entity);

                PathfindingManager.Instance.StartPath(entityPos.Value, clickPoint + UnityEngine.Random.insideUnitSphere * radius, entity);

                //Not implemented for tanks yet, so return.
                if (entityManager.GetComponentData<UnitType>(entity).value > 0)
                    return;

                entityManager.AddComponent<RotateTowardsDirection>(entity);
                entityManager.SetComponentData(entity, new RotateTowardsDirection { direction = new float3(rotateDirectionX, 0, rotateDirectionZ), buffer = 1 });
            }
        }
        else
        {
            //1 Unit

            ushort unitNetworkID = unitNetworkIDs[0];

            if (!NetworkEntityManager.networkEntities.ContainsKey(unitNetworkID))
                return;

            Entity entity = NetworkEntityManager.networkEntities[unitNetworkID];
            Translation entityPos = entityManager.GetComponentData<Translation>(entity);

            PathfindingManager.Instance.StartPath(entityPos.Value, clickPoint, entity);

            //Not implemented for tanks yet, so return.
            if (entityManager.GetComponentData<UnitType>(entity).value > 0)
                return;

            entityManager.AddComponent<RotateTowardsDirection>(entity);
            entityManager.SetComponentData(entity, new RotateTowardsDirection { direction = new float3(rotateDirectionX, 0, rotateDirectionZ), buffer = 1 });
        }
    }
}
