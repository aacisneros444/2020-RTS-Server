using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendTarget : ICommand
{
    public ushort unitNetworkID;
    public ushort targetNetworkID;
    public byte localWeaponID;

    public Command_SendTarget(ushort unitNetworkID, ushort targetNetworkID, byte localWeaponID)
    {
        this.unitNetworkID = unitNetworkID;
        this.targetNetworkID = targetNetworkID;
        this.localWeaponID = localWeaponID;
    }

    public void Execute()
    {
        //Debug.Log("Sent target for unit " + unitNetworkID + " against " + targetNetworkID + " for local weapon " + localWeaponID);
        SendTarget(unitNetworkID, targetNetworkID, localWeaponID);
    }

    private void SendTarget(ushort unitNetworkID, ushort targetNetworkID, byte localWeaponID)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(unitNetworkID);
                writer.Write(targetNetworkID);
                writer.Write(localWeaponID);

                using (Message message = Message.Create(Tags.SendTarget, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
