using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

[UpdateAfter(typeof(BulletSystem))]
public class DestroyDeadUnitsSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();

        EntityQuery entityQuery = GetEntityQuery(typeof(Health));

        NativeArray<ushort> networkIDsForClientDeletion = new NativeArray<ushort>(entityQuery.CalculateEntityCount(), Allocator.TempJob);
        NativeArray<int> companionGraphCutID = new NativeArray<int>(entityQuery.CalculateEntityCount(), Allocator.TempJob);

        for(int i = 0; i < companionGraphCutID.Length; i++)
        {
            companionGraphCutID[i] = int.MaxValue;
        }

        Entities.ForEach((Entity entity, int entityInQueryIndex, in Health health, in NetworkID networkID, in PrefabID prefabID) =>
        {
            if(health.value <= 0)
            {
                networkIDsForClientDeletion[entityInQueryIndex] = networkID.value;
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);

                if (prefabID.value == 5004 || prefabID.value == 5005 || prefabID.value == 5008 || prefabID.value == 5007 || prefabID.value == 5001 || prefabID.value == 5002)
                {
                    companionGraphCutID[entityInQueryIndex] = GetComponent<CompanionGraphCutGameObjectID>(entity).value;
                }
            }
            else
            {
                networkIDsForClientDeletion[entityInQueryIndex] = ushort.MaxValue;
            }

        }).ScheduleParallel(Dependency).Complete();

        commandBufferSystem.AddJobHandleForProducer(Dependency);


        //Send deletions to client
        for(int i = 0; i < networkIDsForClientDeletion.Length; i++)
        {
            if(networkIDsForClientDeletion[i] != ushort.MaxValue)
            {
                ICommand command = new Command_SendClientDestroyEntity(networkIDsForClientDeletion[i], GlobalSimulationTick.value);
                CommandProcessor.AddCommand(command, 0f);
                NetworkEntityManager.networkEntities.Remove(networkIDsForClientDeletion[i]);
            }
        }

        for(int i = 0; i < companionGraphCutID.Length; i++)
        {
            if(companionGraphCutID[i] != int.MaxValue)
            {
                GraphManager.Instance.DestroyGraphObstacle(companionGraphCutID[i]);
            }
        }

        networkIDsForClientDeletion.Dispose();
        companionGraphCutID.Dispose();
    }
}
