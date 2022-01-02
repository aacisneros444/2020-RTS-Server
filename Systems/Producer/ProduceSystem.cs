using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateAfter(typeof(ProducerQueueSystem))]
public class ProduceSystem : ComponentSystem
{

    public List<Entity> entitiesToRemoveFirstFromQueue;

    protected override void OnCreate()
    {
        entitiesToRemoveFirstFromQueue = new List<Entity>();
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, DynamicBuffer<ProducerQueueElement> producerQueue, ref ProduceFirstInQueue produceFirstInQueue,
            ref TeamID teamID, ref Translation translation, ref ProducerCreationLocationEntity locationOffsetEntity) =>
        {
            float3 producePosition = EntityManager.GetComponentData<LocalToWorld>(locationOffsetEntity.entity).Position;

            ICommand command = new Command_CreateUnitEntityWithPositionFromRaycastPoint(producerQueue[0].prefabID, teamID.value,
                producePosition.x, producePosition.z, 500, 1000);

            CommandProcessor.AddCommand(command, 0);

            entitiesToRemoveFirstFromQueue.Add(entity);

            EntityManager.RemoveComponent<ProduceFirstInQueue>(entity);
        });

        if(entitiesToRemoveFirstFromQueue.Count > 0)
        {
            for(int i = 0; i < entitiesToRemoveFirstFromQueue.Count; i++)
            {
                DynamicBuffer<ProducerQueueElement> producerQueue = EntityManager.GetBuffer<ProducerQueueElement>(entitiesToRemoveFirstFromQueue[i]);

                producerQueue.RemoveAt(0);

                entitiesToRemoveFirstFromQueue.RemoveAt(i);
            }
        }
    }
}
