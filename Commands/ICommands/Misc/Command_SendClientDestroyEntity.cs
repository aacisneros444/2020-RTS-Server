using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DarkRift;

public class Command_SendClientDestroyEntity : ICommand
{
    public ushort entityNetworkID;
    public uint tick;

    public Command_SendClientDestroyEntity(ushort entityNetworkID, uint tick)
    {
        this.entityNetworkID = entityNetworkID;
        this.tick = tick;
    }

    public void Execute()
    {
        SendClientDeletion(entityNetworkID, tick);
    }

    private void SendClientDeletion(ushort entityNetworkID, uint tick)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(entityNetworkID);
                writer.Write(tick);

                using (Message message = Message.Create(Tags.SendClientDeleteEntity, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
