using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendNextWaypointVehicle : ICommand
{
    public ushort unitNetworkID;
    public float posX;
    public float posY;
    public float posZ;

    public Command_SendNextWaypointVehicle(ushort unitNetworkID, float posX, float posY, float posZ)
    {
        this.unitNetworkID = unitNetworkID;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
    }

    public void Execute()
    {
        SendWaypoint(unitNetworkID, posX, posY, posZ);
    }

    private void SendWaypoint(ushort unitNetworkID, float posX, float posY, float posZ)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(unitNetworkID);
                writer.Write(posX);
                writer.Write(posY);
                writer.Write(posZ);

                using (Message message = Message.Create(Tags.SendNextWaypointVehicle, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
