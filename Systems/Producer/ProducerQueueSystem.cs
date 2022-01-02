using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class ProducerQueueSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, DynamicBuffer <ProducerQueueElement> producerQueue, ref Inventory inventory) =>
        {
            if(producerQueue.Length > 0)
            {
                if(producerQueue[0].inProduction == 0)
                {
                    //Check that we have enough resource to produce.
                    if(producerQueue[0].resourceCost > inventory.resource)
                    {
                        //If we don't actually have the amount needed, discard this queue element
                        producerQueue.RemoveAt(0);
                        return;
                    }

                    //If so, subtract cost from inventory.
                    inventory.resource -= producerQueue[0].resourceCost;

                    //Set this to produce.
                    ProducerQueueElement producerQueueElement = producerQueue[0];
                    producerQueueElement.inProduction = 1;
                    producerQueue[0] = producerQueueElement;
                }
                else
                {
                    //Produce.

                    ProducerQueueElement producerQueueElement = producerQueue[0];
                    producerQueueElement.timeElapsed += Time.DeltaTime;
                    producerQueue[0] = producerQueueElement;

                    if(producerQueue[0].timeElapsed > producerQueue[0].timeToProduce)
                    {
                        EntityManager.AddComponent<ProduceFirstInQueue>(entity);
                    }
                }

            }

        });
    }
}
