using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class BuildSiteCompletionMonitorSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Inventory inventory, ref BuildSite buildSite, ref BuildProgress buildProgress,
            ref TeamID teamID, ref Translation translation, ref Rotation rotation) => 
        {
            if(buildSite.hasAllResources == 0)
            {
                if(inventory.resource == inventory.maxResource)
                {
                    buildSite.hasAllResources = 1;
                }
            }


            if (buildProgress.value >= buildProgress.requiredValue)
            {
                ICommand command = new Command_DestroyEntity(entity);
                CommandProcessor.AddCommand(command, 0);

                ICommand command0 = new Command_CreateUnitEntityWithPositionRotation(buildSite.finalBuildPrefabID, teamID.value,
                    translation.Value.x, translation.Value.y, translation.Value.z, rotation.Value.value.x, rotation.Value.value.y,
                    rotation.Value.value.z, rotation.Value.value.w);
                CommandProcessor.AddCommand(command0, 0);
            }

        });
    }
}
