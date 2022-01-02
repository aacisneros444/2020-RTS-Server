using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class CreateWaypointBufferOnSpawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<CreateWaypointBufferOnSpawn>().ForEach((Entity entity) =>
        {
            EntityManager.AddBuffer<Waypoint>(entity);
            EntityManager.RemoveComponent<CreateWaypointBufferOnSpawn>(entity);
        });
    }
}
