using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[UpdateAfter(typeof(BuildSiteCompletionMonitorSystem))]
public class BuildersTaskDecisionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref BuildTasked buildTasked) =>
        {
            if (!EntityManager.Exists(buildTasked.assignedBuildSite))
            {
                EntityManager.RemoveComponent<WorkingOnBuild>(entity);
                EntityManager.RemoveComponent<BuildTasked>(entity);
            }
        });

        Entities.WithNone<WorkingOnBuild>().ForEach((Entity entity, ref BuildTasked buildTasked) =>
        {
            Entity buildSite = buildTasked.assignedBuildSite;

            if(EntityManager.GetComponentData<BuildSite>(buildSite).hasAllResources == 1)
            {
                PostUpdateCommands.AddComponent<WorkingOnBuild>(entity);
            }
        });
    }
}
