using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendFirstWaypoints : ICommand
{
    public ushort unitNetworkID;
    public float posX0;
    public float posY0;
    public float posZ0;
    public uint arrivalTick0;
    public float posX1;
    public float posY1;
    public float posZ1;
    public uint arrivalTick1;
    public ushort finalPathIndex;

    public Command_SendFirstWaypoints(ushort unitNetworkID, float posX0, float posY0, float posZ0,
        uint arrivalTick0, float posX1, float posY1, float posZ1, uint arrivalTick1, ushort finalPathIndex)
    {
        this.unitNetworkID = unitNetworkID;
        this.posX0 = posX0;
        this.posY0 = posY0;
        this.posZ0 = posZ0;
        this.arrivalTick0 = arrivalTick0;
        this.posX1 = posX1;
        this.posY1 = posY1;
        this.posZ1 = posZ1;
        this.arrivalTick1 = arrivalTick1;
        this.finalPathIndex = finalPathIndex;
    }

    public void Execute()
    {
        SendWaypoint(unitNetworkID, posX0, posY0, posZ0, arrivalTick0, posX1, posY1, posZ1, arrivalTick1, finalPathIndex);
    }

    private void SendWaypoint(ushort unitNetworkID, float posX0, float posY0, float posZ0,
        uint arrivalTick0, float posX1, float posY1, float posZ1, uint arrivalTick1, ushort finalPathIndex)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(unitNetworkID);
                writer.Write(posX0);
                writer.Write(posY0);
                writer.Write(posZ0);
                writer.Write(arrivalTick0);
                writer.Write(posX1);
                writer.Write(posY1);
                writer.Write(posZ1);
                writer.Write(arrivalTick1);
                writer.Write(finalPathIndex);

                using (Message message = Message.Create(Tags.SendFirstWaypoints, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
