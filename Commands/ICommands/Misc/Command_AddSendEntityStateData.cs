using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Server;
using Unity.Entities;

public class Command_AddSendEntityStateData : ICommand
{
    public ushort entityNetworkID;
    public ushort clientID;

    public Command_AddSendEntityStateData(ushort entityNetworkID, ushort clientID)
    {
        this.entityNetworkID = entityNetworkID;
        this.clientID = clientID;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!NetworkEntityManager.networkEntities.ContainsKey(entityNetworkID))
            return;

        Entity entity = NetworkEntityManager.networkEntities[entityNetworkID];

        //In case an entity still has the SendEntityStateData component, we remove it,
        //so it does not comflict with the sent data of the currently selected entity
        ClearOldSendEntityStateDataComponentsSystem.Instance.Run(clientID);

        entityManager.AddComponent<SendEntityStateData>(entity);
        entityManager.SetComponentData(entity, new SendEntityStateData { clientID = clientID });
    }
}
