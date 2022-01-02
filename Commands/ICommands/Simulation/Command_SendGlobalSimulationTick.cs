using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendGlobalSimulationTick : ICommand
{
    public void Execute()
    {
        SendTick(GlobalSimulationTick.value);
    }

    public void SendTick(uint tick)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(tick);

                using (Message message = Message.Create(Tags.SendGlobalTick, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
