using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SendNextWaypointVehicleSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = commandBufferSystem.CreateCommandBuffer();

        Entities.ForEach((Entity entity, in NetworkID networkID, in SendNextWaypointVehicle sendNextWaypointVehicle) =>
        {
            ICommand command = new Command_SendNextWaypointVehicle(networkID.value, sendNextWaypointVehicle.value.x,
                sendNextWaypointVehicle.value.y, sendNextWaypointVehicle.value.z);

            CommandProcessor.AddCommand(command, 0f);

            commandBuffer.RemoveComponent<SendNextWaypointVehicle>(entity);

        }).WithoutBurst().Run();
    }
}
