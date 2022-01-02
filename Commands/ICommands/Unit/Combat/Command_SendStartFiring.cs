using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendStartFiring: ICommand
{
    public ushort unitNetworkID;
    public byte localWeaponID;
    public uint startFiringTick;
    public ushort roundsInTheMagazine;

    public Command_SendStartFiring(ushort unitNetworkID, byte localWeaponID, uint startFiringTick, ushort roundsInTheMagazine)
    {
        this.unitNetworkID = unitNetworkID;
        this.localWeaponID = localWeaponID;
        this.startFiringTick = startFiringTick;
        this.roundsInTheMagazine = roundsInTheMagazine;
    }

    public void Execute()
    {
        SendStartFiring(unitNetworkID, localWeaponID, startFiringTick, roundsInTheMagazine);
    }

    private void SendStartFiring(ushort unitNetworkID, byte localWeaponID, uint startFiringTick,
        ushort roundsInTheMagazine)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(unitNetworkID);
                writer.Write(localWeaponID);
                writer.Write(startFiringTick);
                writer.Write(roundsInTheMagazine);

                using (Message message = Message.Create(Tags.SendStartFiring, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
