using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public class PlaceResourceNodesInQuadrantsSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(ResourceNodeQuadrantEntity));

        QuadrantMultiHashmaps.resourceNodeQuadrantMultiHashmap.Clear();
        if (entityQuery.CalculateEntityCount() > QuadrantMultiHashmaps.resourceNodeQuadrantMultiHashmap.Capacity)
        {
            QuadrantMultiHashmaps.resourceNodeQuadrantMultiHashmap.Capacity = entityQuery.CalculateEntityCount();
        }

        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();

        NativeMultiHashMap<int, BucketedResourceNodeData>.ParallelWriter resourceNodeQuadrantMultiHashmap = QuadrantMultiHashmaps.resourceNodeQuadrantMultiHashmap.AsParallelWriter();

        JobHandle jobHandle = Entities.WithAll<ResourceNodeQuadrantEntity>().ForEach((Entity entity, int entityInQueryIndex, ref Translation translation,
            ref ResourceNode resourceNode) =>
        {
            int hashmapKey = QuadrantMultiHashmaps.GetPositionHashMapKey(translation.Value);
            resourceNodeQuadrantMultiHashmap.Add(hashmapKey, new BucketedResourceNodeData { entity = entity, position = translation.Value, resourceType = resourceNode.resourceType });
            commandBuffer.RemoveComponent<ResourceNodeQuadrantEntity>(entityInQueryIndex, entity);

        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
