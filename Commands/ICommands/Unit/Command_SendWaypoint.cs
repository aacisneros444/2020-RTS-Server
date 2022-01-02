using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendWaypoint : ICommand
{
    public ushort unitNetworkID;
    public uint arrivalTick;
    public float posX;
    public float posY;
    public float posZ;

    public Command_SendWaypoint(ushort unitNetworkID, uint arrivalTick, float posX, float posY, float posZ)
    {
        this.unitNetworkID = unitNetworkID;
        this.arrivalTick = arrivalTick;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
    }

    public void Execute()
    {
        SendWaypoint(unitNetworkID, arrivalTick, posX, posY, posZ);      
    }

    private void SendWaypoint(ushort unitNetworkID, uint arrivalTick, float posX, float posY, float posZ)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(unitNetworkID);
                writer.Write(arrivalTick);
                writer.Write(posX);
                writer.Write(posY);
                writer.Write(posZ);

                using (Message message = Message.Create(Tags.SendWaypoint, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
