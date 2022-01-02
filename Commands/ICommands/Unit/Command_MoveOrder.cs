using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class Command_MoveOrder : ICommand
{
    public float clickPointX;
    public float clickPointY;
    public float clickPointZ;
    public List<ushort> unitNetworkIDs;

    public Command_MoveOrder(float clickPointX, float clickPointY, float clickPointZ, List<ushort> unitNetworkIDs)
    {
        this.clickPointX = clickPointX;
        this.clickPointY = clickPointY;
        this.clickPointZ = clickPointZ;
        this.unitNetworkIDs = unitNetworkIDs;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        Vector3 clickPoint = new Vector3(clickPointX, clickPointY, clickPointZ);

        //Multiple units
        if(unitNetworkIDs.Count > 1)
        {
            float radius = math.sqrt(unitNetworkIDs.Count * 12f);
            for (int i = 0; i < unitNetworkIDs.Count; i++)
            {
                ushort unitNetworkID = unitNetworkIDs[i];

                if (!NetworkEntityManager.networkEntities.ContainsKey(unitNetworkID))
                    return;

                Entity entity = NetworkEntityManager.networkEntities[unitNetworkID];
                Translation entityPos = entityManager.GetComponentData<Translation>(entity);

                PathfindingManager.Instance.StartPath(entityPos.Value, clickPoint + UnityEngine.Random.insideUnitSphere * radius, entity);
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
        }
    }
}
