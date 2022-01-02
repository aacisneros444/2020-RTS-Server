using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class SendRotateTowardsDirectionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<SendRotateTowardsDirection>().ForEach((Entity entity, ref NetworkID networkID) =>
        {
            if (EntityManager.HasComponent<RotateTowardsDirection>(entity))
            {
                RotateTowardsDirection rotateTowardsDirection = EntityManager.GetComponentData<RotateTowardsDirection>(entity);

                ICommand command = new Command_SendRotateTowardsDirection(networkID.value, rotateTowardsDirection.direction.x,
                    rotateTowardsDirection.direction.z, rotateTowardsDirection.buffer);

                CommandProcessor.AddCommand(command, 0);
            }

            EntityManager.RemoveComponent<SendRotateTowardsDirection>(entity);
        });
    }
}
