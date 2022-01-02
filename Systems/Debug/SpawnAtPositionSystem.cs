using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class SpawnAtPositionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Translation translation, ref Rotation rotation, ref SpawnAtPosition spawnAtPosition) =>
        {
            ICommand command = new Command_CreateEntityWithPositionRotation(spawnAtPosition.prefabID, translation.Value.x,
                translation.Value.y, translation.Value.z, rotation.Value.value.x, rotation.Value.value.y,
                rotation.Value.value.z, rotation.Value.value.w);
            CommandProcessor.AddCommand(command, 100);

            EntityManager.DestroyEntity(entity);

        });
    }
}
