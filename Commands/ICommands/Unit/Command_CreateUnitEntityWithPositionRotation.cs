using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using DarkRift;

public class Command_CreateUnitEntityWithPositionRotation : ICommand
{
    public ushort prefabID;
    public ushort teamID;
    public float posX;
    public float posY;
    public float posZ;
    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    public Command_CreateUnitEntityWithPositionRotation(ushort prefabID, ushort teamID, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, float rotW)
    {
        this.prefabID = prefabID;
        this.teamID = teamID;
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

        entityManager.SetComponentData(entity, new TeamID { value = teamID });

        NetworkEntityManager.RegisterNetworkEntity(entity);

        ushort networkID = entityManager.GetComponentData<NetworkID>(entity).value;

        SendUnitEntity(prefabID, networkID, teamID, posX, posY, posZ, rotX, rotY, rotZ, rotW);
    }

    public void SendUnitEntity(ushort prefabID, ushort networkID, ushort teamID, float posX, float posY, float posZ,
        float rotX, float rotY, float rotZ, float rotW)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(prefabID);
                writer.Write(networkID);
                writer.Write(teamID);
                writer.Write(posX);
                writer.Write(posY);
                writer.Write(posZ);
                DataCompressionUtils.CompressAndWriteRotation(writer, new Quaternion(rotX, rotY, rotZ, rotW));

                using (Message message = Message.Create(Tags.SendUnitEntityWithPositionRotation, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }


}
