using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Command_AddProducerQueueElementToProducer : ICommand
{
    public ushort producerNetworkID;
    public ushort prefabID;

    public Command_AddProducerQueueElementToProducer(ushort producerNetworkID, ushort prefabID)
    {
        this.producerNetworkID = producerNetworkID;
        this.prefabID = prefabID;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!NetworkEntityManager.networkEntities.ContainsKey(producerNetworkID))
            return;

        Entity producerEntity = NetworkEntityManager.networkEntities[producerNetworkID];

        Entity entityPrefab = EntityPrefabLookup.GetEntityPrefab(prefabID);

        CostToProduce costToProduce = entityManager.GetComponentData<CostToProduce>(entityPrefab);

        ProducerQueueElement producerQueueElement = new ProducerQueueElement
        {
            prefabID = prefabID,
            timeElapsed = 0,
            timeToProduce = costToProduce.time,
            resourceCost = costToProduce.resourceCost,
            inProduction = 0
        };

        DynamicBuffer<ProducerQueueElement> producerQueue = entityManager.GetBuffer<ProducerQueueElement>(producerEntity);

        producerQueue.Add(producerQueueElement);
    }
}
