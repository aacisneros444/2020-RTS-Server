using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendRotateTowardsDirection : ICommand
{
    public ushort unitNetworkID;
    public float directionX;
    public float directionZ;
    public byte buffer;

    public Command_SendRotateTowardsDirection(ushort unitNetworkID, float directionX, float directionZ, byte buffer)
    {
        this.unitNetworkID = unitNetworkID;
        this.directionX = directionX;
        this.directionZ = directionZ;
        this.buffer = buffer;
    }

    public void Execute()
    {
        SendRotateTowardsDirectionOrder(unitNetworkID, directionX, directionZ, buffer);
    }

    private void SendRotateTowardsDirectionOrder(ushort unitNetworkID, float directionX, float directionZ, byte buffer)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(unitNetworkID);
                writer.Write(directionX);
                writer.Write(directionZ);
                writer.Write(buffer);

                using (Message message = Message.Create(Tags.SendRotateTowardsDirection, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
