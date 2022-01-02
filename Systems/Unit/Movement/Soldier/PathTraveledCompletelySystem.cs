using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PathTraveledCompletelySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<PathTraveledCompletely>().ForEach((Entity entity) =>
        {
            if (EntityManager.HasComponent<RotateTowardsDirection>(entity))
            {
                RotateTowardsDirection rotateTowardsDirection = EntityManager.GetComponentData<RotateTowardsDirection>(entity);
                EntityManager.SetComponentData(entity, new RotateTowardsDirection
                { direction = rotateTowardsDirection.direction,

                buffer = 0});
            }

            EntityManager.RemoveComponent<PathTraveledCompletely>(entity);
        });
    }
}
