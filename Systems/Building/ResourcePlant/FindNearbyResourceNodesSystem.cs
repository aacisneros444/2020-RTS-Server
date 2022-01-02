using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

public class FindNearbyResourceNodesSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();

        NativeMultiHashMap<int, BucketedResourceNodeData> resourceNodeQuadrantMultiHashmap = QuadrantMultiHashmaps.resourceNodeQuadrantMultiHashmap;

        JobHandle jobHandle = Entities.WithAll<FindNearbyResourceNodes>().ForEach((Entity entity, int entityInQueryIndex, ref Translation translation,
                ref ResourcePlant resourcePlant) =>
        {
            int hashmapKey = QuadrantMultiHashmaps.GetPositionHashMapKey(translation.Value);
            int nearbyNodes = 0;

            FindNearbyResourceNodes(hashmapKey, translation.Value, resourcePlant.resourceType, ref nearbyNodes);
            FindNearbyResourceNodes(hashmapKey + 1, translation.Value, resourcePlant.resourceType, ref nearbyNodes);
            FindNearbyResourceNodes(hashmapKey - 1, translation.Value, resourcePlant.resourceType, ref nearbyNodes);
            FindNearbyResourceNodes(hashmapKey + 1000, translation.Value, resourcePlant.resourceType, ref nearbyNodes);
            FindNearbyResourceNodes(hashmapKey - 1000, translation.Value, resourcePlant.resourceType, ref nearbyNodes);
            FindNearbyResourceNodes(hashmapKey + 1 + 1000, translation.Value, resourcePlant.resourceType, ref nearbyNodes);
            FindNearbyResourceNodes(hashmapKey - 1 + 1000, translation.Value, resourcePlant.resourceType, ref nearbyNodes);
            FindNearbyResourceNodes(hashmapKey + 1 - 1000, translation.Value, resourcePlant.resourceType, ref nearbyNodes);
            FindNearbyResourceNodes(hashmapKey - 1 - 1000, translation.Value, resourcePlant.resourceType, ref nearbyNodes);

            resourcePlant.linkedNodes = (ushort)nearbyNodes;

            commandBuffer.RemoveComponent<FindNearbyResourceNodes>(entityInQueryIndex, entity);

        }).WithReadOnly(resourceNodeQuadrantMultiHashmap).Schedule(inputDeps);



        void FindNearbyResourceNodes(int hashmapKey, float3 finderPosition, byte resourceType, ref int nearbyNodes)
        {
            BucketedResourceNodeData bucketedResourceNodeData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if (resourceNodeQuadrantMultiHashmap.TryGetFirstValue(hashmapKey, out bucketedResourceNodeData, out nativeMultiHashMapIterator))
            {
                do
                {
                    if(bucketedResourceNodeData.resourceType == resourceType)
                    {
                        if (math.distancesq(bucketedResourceNodeData.position, finderPosition) < 625f) //25 meters
                        {
                            nearbyNodes++;
                            //Debug.Log("Node in quadrant " + hashmapKey + " successful link with a distance of " + math.distancesq(bucketedEntityData.position, finderPosition));
                        }
                        //else
                        //{
                        //    Debug.Log("Node in quadrant " + hashmapKey + " too far with a distance of " + math.distancesq(bucketedEntityData.position, finderPosition));
                        //}
                    }
                } while (resourceNodeQuadrantMultiHashmap.TryGetNextValue(out bucketedResourceNodeData, ref nativeMultiHashMapIterator));
            }
        }



        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
