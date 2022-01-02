using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command_ProcessLogin : ICommand
{
    public ushort teamID;

    public Command_ProcessLogin(ushort teamID)
    {
        this.teamID = teamID;
    }

    public void Execute()
    {
        if (!TeamIDs.teamIDs.Contains(teamID))
        {
            ICommand command = new Command_CreateFirstUnits(teamID);

            CommandProcessor.AddCommand(command, 0);
        }
    }
}
