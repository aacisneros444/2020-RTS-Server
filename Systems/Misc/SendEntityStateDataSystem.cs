using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SendEntityStateDataSystem : SystemBase
{
    protected override void OnUpdate()
    {
        //Send inventory data
        Entities.ForEach((Entity entity, ref SendEntityStateData sendEntityStateData, ref Inventory inventory) =>
        {
            if(sendEntityStateData.firstSendInventory == 0)
            {
                ICommand command = new Command_SendEntityStateDataUpdateResource(inventory.resource, inventory.maxResource,
                    sendEntityStateData.clientID);
                CommandProcessor.AddCommand(command, 0);

                inventory.lastSendValue = inventory.resource;
                sendEntityStateData.firstSendInventory = 1;
            }
            else
            {
                if (inventory.lastSendValue != inventory.resource)
                {
                    ICommand command = new Command_SendEntityStateDataUpdateResource(inventory.resource, inventory.maxResource,
                        sendEntityStateData.clientID);
                    CommandProcessor.AddCommand(command, 0);

                    inventory.lastSendValue = inventory.resource;
                }
            }
        }).WithoutBurst().Run();

        //Send health data
        Entities.ForEach((Entity entity, ref SendEntityStateData sendEntityStateData, ref Health health) =>
        {
            if(sendEntityStateData.firstSendHealth == 0)
            {
                ICommand command = new Command_SendEntityStateDataUpdateHealth(health.value, health.maxValue, sendEntityStateData.clientID);
                CommandProcessor.AddCommand(command, 0);

                health.lastSendValue = health.value;
                sendEntityStateData.firstSendHealth = 1;
            }
            else
            {
                if (health.lastSendValue != health.value)
                {
                    ICommand command = new Command_SendEntityStateDataUpdateHealth(health.value, health.maxValue, sendEntityStateData.clientID);
                    CommandProcessor.AddCommand(command, 0);

                    health.lastSendValue = health.value;
                }
            }
        }).WithoutBurst().Run();
    }
}
