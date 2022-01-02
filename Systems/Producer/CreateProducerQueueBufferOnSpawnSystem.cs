using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class CreateProducerQueueBufferOnSpawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<CreateProducerQueueBufferOnSpawn>().ForEach((Entity entity) =>
        {
            EntityManager.AddBuffer<ProducerQueueElement>(entity);
            EntityManager.RemoveComponent<CreateProducerQueueBufferOnSpawn>(entity);
        });
    }
}
