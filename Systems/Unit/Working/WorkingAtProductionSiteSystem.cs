using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class WorkingAtProductionSiteSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<Moving, PathQueued>().ForEach((Entity entity, ref WorkTasked workTasked,
            ref Translation translation, ref NetworkID networkID) =>
        {
            float3 workSitePosition = EntityManager.GetComponentData<Translation>(workTasked.assignedWorkSite).Value;

            if(math.distancesq(translation.Value, workSitePosition) < 36f)
            {
                ResourcePlant resourcePlant0 = EntityManager.GetComponentData<ResourcePlant>(workTasked.assignedWorkSite);
                resourcePlant0.laborForce = (ushort)(resourcePlant0.laborForce + 1);
                EntityManager.SetComponentData(workTasked.assignedWorkSite, resourcePlant0);

                EntityManager.RemoveComponent<WorkTasked>(entity);

                EntityManager.AddComponent<Working>(entity);
                EntityManager.SetComponentData(entity, new Working { workSite = workTasked.assignedWorkSite });
                return;
            }
            else
            {
                float3 dir = math.normalize(workSitePosition - translation.Value);
                float3 endPos = workSitePosition - dir;
                List<ushort> entityID = new List<ushort>();
                entityID.Add(networkID.value);

                ICommand command = new Command_MoveOrder(endPos.x, endPos.y, endPos.z, entityID);
                CommandProcessor.AddCommand(command, 0);
            }
        });
    }
}
