using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using DarkRift;

public class Command_CreateEntityWithPositionRotation : ICommand
{
    public ushort prefabID;
    public float posX;
    public float posY;
    public float posZ;
    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    public Command_CreateEntityWithPositionRotation(ushort prefabID, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW)
    {
        this.prefabID = prefabID;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.rotX = rotX;
        this.rotY = rotY;
        this.rotZ = rotZ;
        this.rotW = rotW;
    }

    public void Execute()
    {
        Entity entityPrefab = EntityPrefabLookup.GetEntityPrefab(prefabID);

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity entity = entityManager.Instantiate(entityPrefab);

        entityManager.SetComponentData(entity, new Translation { Value = new float3(posX, posY, posZ) });
        entityManager.SetComponentData(entity, new Rotation { Value = new quaternion(rotX, rotY, rotZ, rotW) });

        NetworkEntityManager.RegisterNetworkEntity(entity);

        ushort networkID = entityManager.GetComponentData<NetworkID>(entity).value;

        SendEntityToClients(prefabID, networkID, posX, posY, posZ, rotX, rotY, rotZ, rotW);
    }

    private void SendEntityToClients(ushort entityPrefabID, ushort networkID, float posX, float posY, float posZ,
        float rotX, float rotY, float rotZ, float rotW)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(entityPrefabID);
                writer.Write(networkID);
                writer.Write(posX);
                writer.Write(posY);
                writer.Write(posZ);
                DataCompressionUtils.CompressAndWriteRotation(writer, new Quaternion(rotX, rotY, rotZ, rotW));

                using (Message message = Message.Create(Tags.SendEntityWithPositionRotation, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
