using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendEntityStateDataUpdateHealth : ICommand
{
    public float health;
    public float maxHealth;
    public ushort clientID;

    public Command_SendEntityStateDataUpdateHealth(float health, float maxHealth, ushort clientID)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.clientID = clientID;
    }

    public void Execute()
    {
        SendEntityStateDataUpdateHealth(health, maxHealth, clientID);
    }

    private void SendEntityStateDataUpdateHealth(float health, float maxHealth, ushort clientID)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            if(Clients.clients[i].ID == clientID)
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(health);
                    writer.Write(maxHealth);

                    using (Message message = Message.Create(Tags.SendEntityStateDataUpdateHealth, writer))
                    {
                        Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                    }
                }
            }
        }
    }
}
