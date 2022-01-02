using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using DarkRift;

public class Command_CreateUnitEntityWithPositionFromRaycastPoint : ICommand
{
    public ushort prefabID;
    public ushort teamID;
    public float posX;
    public float posZ;
    public float raycastOriginHeight;
    public float raycastDistance;

    public Command_CreateUnitEntityWithPositionFromRaycastPoint(ushort prefabID, ushort teamID, float posX, float posZ, float raycastOriginHeight, float raycastDistance)
    {
        this.prefabID = prefabID;
        this.teamID = teamID;
        this.posX = posX;
        this.posZ = posZ;
        this.raycastOriginHeight = raycastOriginHeight;
        this.raycastDistance = raycastDistance;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity entityPrefab = EntityPrefabLookup.GetEntityPrefab(prefabID);
        Entity entity = entityManager.Instantiate(entityPrefab);
        entityManager.RemoveComponent<CostToProduce>(entity);

        float3 entityPosition = float3.zero;
        quaternion entityRotation = quaternion.identity;

        //Try to hit ground plane to set position.
        UnityEngine.RaycastHit raycastHit0;
        if (Physics.Raycast(new Vector3(posX, raycastOriginHeight, posZ), Vector3.down, out raycastHit0, raycastDistance))
        {
            entityPosition = new float3(raycastHit0.point.x, raycastHit0.point.y, raycastHit0.point.z);
            entityManager.SetComponentData(entity, new Translation
            {
                Value = raycastHit0.point
            });
        }

        //DOTS Raycast
        //Unity.Physics.RaycastHit raycastHit;
        //if (PhysicsUtils.Raycast(new float3(posX, raycastOriginHeight, posZ), new float3(0, -1, 0), raycastDistance, out raycastHit))
        //{
        //    entityPosition = new float3(raycastHit.Position.x, raycastHit.Position.y, raycastHit.Position.z);
        //    entityManager.SetComponentData(entity, new Translation
        //    {
        //        Value = entityPosition
        //    });
        //}

        //Register with networkID.
        ushort networkID = NetworkEntityManager.RegisterNetworkEntity(entity);

        //Set teamID for entity and all relying children (weapons).
        TeamID newTeamID = new TeamID { value = teamID };
        entityManager.SetComponentData(entity, new TeamID { value = teamID });
        if (entityManager.HasComponent<LocalWeapons>(entity))
        {
            LocalWeapons localWeapons = entityManager.GetComponentData<LocalWeapons>(entity);

            if (localWeapons.weapon0 != Entity.Null)
            {
                entityManager.AddComponentData(localWeapons.weapon0, newTeamID);
            }

            if (localWeapons.weapon1 != Entity.Null)
            {
                entityManager.AddComponentData(localWeapons.weapon1, newTeamID);
            }


            if (localWeapons.weapon2 != Entity.Null)
            {
                entityManager.AddComponentData(localWeapons.weapon2, newTeamID);
            }

            if (localWeapons.weapon3 != Entity.Null)
            {
                entityManager.AddComponentData(localWeapons.weapon3, newTeamID);
            }
        }


        SendEntityToClients(prefabID, networkID, teamID, entityPosition.x, entityPosition.y, entityPosition.z);

    }

    private void SendEntityToClients(ushort entityPrefabID, ushort networkID, ushort teamID, float posX, float posY, float posZ)
    {
        for (int i = 0; i < Clients.clients.Count; i++)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(entityPrefabID);
                writer.Write(networkID);
                writer.Write(teamID);
                writer.Write(posX);
                writer.Write(posY);
                writer.Write(posZ);

                using (Message message = Message.Create(Tags.SendUnitEntityWithPosition, writer))
                {
                    Clients.clients[i].SendMessage(message, SendMode.Unreliable);
                }
            }
        }
    }
}
