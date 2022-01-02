using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using DarkRift;

public class Command_CreateEntityWithPositionRotationFromRaycastNormal : ICommand
{
    public ushort prefabID;
    public float posX;
    public float posZ;
    public float raycastOriginHeight;
    public float raycastDistance;

    public Command_CreateEntityWithPositionRotationFromRaycastNormal(ushort prefabID, float posX, float posZ, float raycastOriginHeight, float raycastDistance)
    {
        this.prefabID = prefabID;
        this.posX = posX;
        this.posZ = posZ;
        this.raycastOriginHeight = raycastOriginHeight;
        this.raycastDistance = raycastDistance;
    }

    public void Execute()
    {
        Entity entityPrefab = EntityPrefabLookup.GetEntityPrefab(prefabID);

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity entity = entityManager.Instantiate(entityPrefab);

        float3 entityPosition = float3.zero;
        quaternion entityRotation = quaternion.identity;

        Unity.Physics.RaycastHit raycastHit;
        if (PhysicsUtils.Raycast(new float3(posX, raycastOriginHeight, posZ), new float3(0, -1, 0), raycastDistance, out raycastHit))
        {
            entityPosition = new float3(raycastHit.Position.x, raycastHit.Position.y, raycastHit.Position.z);
            entityManager.SetComponentData(entity, new Translation
            {
                Value = entityPosition
            });

            entityRotation = Quaternion.FromToRotation(Vector3.up, raycastHit.SurfaceNormal);
            entityManager.SetComponentData(entity, new Rotation { Value = entityRotation });

        }

        NetworkEntityManager.RegisterNetworkEntity(entity);

        ushort networkID = entityManager.GetComponentData<NetworkID>(entity).value;

        SendEntityToClients(prefabID, networkID, entityPosition.x, entityPosition.y, entityPosition.z,
            entityRotation.value.x, entityRotation.value.y, entityRotation.value.z, entityRotation.value.w);
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
