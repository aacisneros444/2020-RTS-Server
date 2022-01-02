using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateAfter(typeof(BuildersTaskDecisionSystem))]
public class WorkingOnBuildSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<PathQueued, Moving>().WithAll<WorkingOnBuild>().ForEach((Entity entity, ref BuildTasked buildTasked,
            ref Translation translation, ref NetworkID networkID) =>
        {
            float3 buildSitePosition = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            if (EntityManager.HasComponent<Translation>(buildTasked.assignedBuildSite))
            {
                buildSitePosition = EntityManager.GetComponentData<Translation>(buildTasked.assignedBuildSite).Value;
            }

            if (buildSitePosition.x == float.MaxValue)
                return;

            if (math.distancesq(buildSitePosition, translation.Value) < 100f)
            {
                BuildProgress buildProgress = EntityManager.GetComponentData<BuildProgress>(buildTasked.assignedBuildSite);
                buildProgress.value += Time.DeltaTime;
                EntityManager.SetComponentData(buildTasked.assignedBuildSite, buildProgress);
            }
            else
            {
                //float3 dir = math.normalize(buildSitePosition - translation.Value);
                //float3 endPos = buildSitePosition - dir;
                float3 endPos = buildSitePosition;
                List<ushort> entityID = new List<ushort>();
                entityID.Add(networkID.value);

                ICommand command = new Command_MoveOrder(endPos.x, endPos.y, endPos.z, entityID);
                CommandProcessor.AddCommand(command, 0);
            }
        });
    }
}
