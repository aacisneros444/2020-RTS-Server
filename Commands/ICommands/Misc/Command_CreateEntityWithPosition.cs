using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using DarkRift;

public class Command_CreateEntityWithPosition : ICommand
{
    public ushort prefabID;
    public float posX;
    public float posY;
    public float posZ;

    public Command_CreateEntityWithPosition(ushort prefabID, float posX, float posY, float posZ)
    {
        this.prefabID = prefabID;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
    }

    public void Execute()
    {
        Entity entityPrefab = EntityPrefabLookup.GetEntityPrefab(prefabID);

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity entity = entityManager.Instantiate(entityPrefab);

        entityManager.SetComponentData(entity, new Translation { Value = new float3(posX, posY, posZ) });

        NetworkEntityManager.RegisterNetworkEntity(entity);

        ushort networkID = entityManager.GetComponentData<NetworkID>(entity).value;

        SendEntityToClients(prefabID, networkID, posX, posY, posZ);
    }

    private void SendEntityToClients(ushort entityPrefabID, ushort networkID, float posX, float posY, float posZ)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(entityPrefabID);
                writer.Write(networkID);
                writer.Write(posX);
                writer.Write(posY);
                writer.Write(posZ);

                using (Message message = Message.Create(Tags.SendEntityWithPosition, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
