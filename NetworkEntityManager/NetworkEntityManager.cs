using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

public class NetworkEntityManager : MonoBehaviour
{
    public static NativeHashMap<ushort, Entity> networkEntities;
    private static ushort uniqueNetworkIDCounter;
    private static EntityManager entityManager;

    private void Awake()
    {
        networkEntities = new NativeHashMap<ushort, Entity>(1000, Allocator.Persistent);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }


    private void OnDestroy()
    {
        networkEntities.Dispose();
    }

    public static ushort GetNetworkID()
    {
        ushort ID = uniqueNetworkIDCounter;
        uniqueNetworkIDCounter++;
        return ID;
    }

    public static ushort RegisterNetworkEntity(Entity networkEntity)
    {
        ushort networkID = GetNetworkID();

        entityManager.AddComponent(networkEntity, typeof(NetworkID));
        entityManager.SetComponentData(networkEntity, new NetworkID { value = networkID });

        networkEntities.Add(networkID, networkEntity);

        return networkID;
    }
}
