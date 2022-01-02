using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using DarkRift;

public class Command_CreateFirstUnits : ICommand
{
    public ushort teamID;

    public Command_CreateFirstUnits(ushort teamID)
    {
        this.teamID = teamID;
    }

    public void Execute()
    {
        float spawnPosX = UnityEngine.Random.Range(10, 60);
        float spawnPosZ = UnityEngine.Random.Range(10, 60);

        //ICommand createDropPodCommand = new Command_CreateUnitEntityWithPositionRotationFromRaycastNormal(0, teamID, dropPodPosX,
        //    dropPodPosZ, 100, 105);

        ICommand createSoldierCommand0 = new Command_CreateUnitEntityWithPositionFromRaycastPoint(1, teamID, spawnPosX + 5,
            spawnPosZ, 500, 1000);

        ICommand createSoldierCommand1 = new Command_CreateUnitEntityWithPositionFromRaycastPoint(1, teamID, spawnPosX - 5,
            spawnPosZ, 500, 1000);

        //CommandProcessor.AddCommand(createDropPodCommand, 0);
        CommandProcessor.AddCommand(createSoldierCommand0, 0);
        CommandProcessor.AddCommand(createSoldierCommand1, 0);
    }
}
