using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class Command_SendFirstWaypointsVehicle : ICommand
{
    public Entity entity; 
    public float posX1;
    public float posY1;
    public float posZ1;
    public uint startingTick;

    public Command_SendFirstWaypointsVehicle(Entity entity, float posX1, float posY1, float posZ1, uint startingTick)
    {
        this.entity = entity;
        this.posX1 = posX1;
        this.posY1 = posY1;
        this.posZ1 = posZ1;
        this.startingTick = startingTick;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        quaternion currentRotation = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Rotation>(entity).Value;

        SendWaypoints(entityManager.GetComponentData<NetworkID>(entity).value, currentRotation,
            entityManager.GetComponentData<Translation>(entity).Value, posX1, posY1, posZ1, startingTick);
    }

    private void SendWaypoints(ushort unitNetworkID, quaternion currentRotation, float3 position,
    float posX1, float posY1, float posZ1, uint startingTick)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(unitNetworkID);
                DataCompressionUtils.CompressAndWriteRotation(writer, currentRotation);
                writer.Write(position.x);
                writer.Write(position.y);
                writer.Write(position.z);
                writer.Write(posX1);
                writer.Write(posY1);
                writer.Write(posZ1);
                writer.Write(startingTick);

                using (Message message = Message.Create(Tags.SendFirstWaypointsVehicle, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
