using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DarkRift;

public class Command_DestroyEntity : ICommand
{
    public Entity entity;

    public Command_DestroyEntity(Entity entity)
    {
        this.entity = entity;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        ushort entityNetworkID = entityManager.GetComponentData<NetworkID>(entity).value;

        NetworkEntityManager.networkEntities.Remove(entityNetworkID);

        if (entityManager.HasComponent<CompanionGraphCutGameObjectID>(entity))
        {
            int companionGameObjectID = entityManager.GetComponentData<CompanionGraphCutGameObjectID>(entity).value;

            GraphManager.Instance.DestroyGraphObstacle(companionGameObjectID);
        }

        entityManager.DestroyEntity(entity);
        SendDeletion(entityNetworkID);
    }

    private void SendDeletion(ushort entityNetworkID)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(entityNetworkID);

                using (Message message = Message.Create(Tags.SendDeleteEntity, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
