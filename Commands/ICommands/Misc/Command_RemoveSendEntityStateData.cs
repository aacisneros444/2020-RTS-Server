using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Command_RemoveSendEntityStateData : ICommand
{
    public ushort entityNetworkID;

    public Command_RemoveSendEntityStateData(ushort entityNetworkID)
    {
        this.entityNetworkID = entityNetworkID;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!NetworkEntityManager.networkEntities.ContainsKey(entityNetworkID))
            return;

        Entity entity = NetworkEntityManager.networkEntities[entityNetworkID];

        entityManager.RemoveComponent<SendEntityStateData>(entity);
    }
}
