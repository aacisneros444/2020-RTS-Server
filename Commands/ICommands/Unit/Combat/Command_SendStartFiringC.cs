using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;

public class Command_SendStartFiringC : ICommand
{
    public ushort unitNetworkID;
    public byte localWeaponID;
    public uint startFiringTick;
    public ushort roundsInTheMagazine;
    public ushort roundsInTheMagazineC;

    public Command_SendStartFiringC(ushort unitNetworkID, byte localWeaponID, uint startFiringTick, ushort roundsInTheMagazine,
        ushort roundsInTheMagazineC)
    {
        this.unitNetworkID = unitNetworkID;
        this.localWeaponID = localWeaponID;
        this.startFiringTick = startFiringTick;
        this.roundsInTheMagazine = roundsInTheMagazine;
        this.roundsInTheMagazineC = roundsInTheMagazineC;
    }

    public void Execute()
    {
        SendStartFiring(unitNetworkID, localWeaponID, startFiringTick, roundsInTheMagazine, roundsInTheMagazineC);
    }

    private void SendStartFiring(ushort unitNetworkID, byte localWeaponID, uint startFiringTick, ushort roundsInTheMagazine,
        ushort roundsInTheMagazineC)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(unitNetworkID);
                writer.Write(localWeaponID);
                writer.Write(startFiringTick);
                writer.Write(roundsInTheMagazine);
                writer.Write(roundsInTheMagazineC);

                using (Message message = Message.Create(Tags.SendStartFiringC, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
