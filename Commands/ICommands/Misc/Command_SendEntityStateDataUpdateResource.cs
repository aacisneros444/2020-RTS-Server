using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendEntityStateDataUpdateResource : ICommand
{
    public int amount;
    public int maxAmount;
    public ushort clientID;

    public Command_SendEntityStateDataUpdateResource(int amount, int maxAmount, ushort clientID)
    {
        this.amount = amount;
        this.maxAmount = maxAmount;
        this.clientID = clientID;
    }

    public void Execute()
    {
        SendEntityStateDataUpdateResource(amount, maxAmount, clientID);
    }

    private void SendEntityStateDataUpdateResource(int amount, int maxAmount, ushort clientID)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            if (Clients.clients[i].ID == clientID)
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(amount);
                    writer.Write(maxAmount);

                    using (Message message = Message.Create(Tags.SendEntityStateDataUpdateResource, writer))
                    {
                        Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                    }
                }
            }
        }
    }
}
