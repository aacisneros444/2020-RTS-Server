using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class MoveSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        uint globalSimulationTick = GlobalSimulationTick.value;
     
        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();

        EntityQuery entityQuery = GetEntityQuery(typeof(Moving));

        JobHandle jobHandle = Entities.WithAll<Moving>().ForEach((Entity entity, int entityInQueryIndex,
            DynamicBuffer<Waypoint> path, ref Translation translation, ref PathIndex pathIndex, ref NetworkID networkID) =>
        {
            float tickElapsed = globalSimulationTick - path[pathIndex.value].arrivalTick;
            float deltaTick = path[pathIndex.value + 1].arrivalTick - path[pathIndex.value].arrivalTick;
            float percentTraveled = math.clamp((tickElapsed / deltaTick), 0f, 1f);

            float3 currentWaypointPoint = path[pathIndex.value].point;
            float3 nextWaypointPoint = path[pathIndex.value + 1].point;

            //We must keep the Y value unchanged, so that the VerticalCorrectionSystem has full control over the Y value
            float originalYValue = translation.Value.y;
            //Lerp between waypoints
            translation.Value = math.lerp(currentWaypointPoint, nextWaypointPoint, percentTraveled);
            //Apply original Y value
            translation.Value = new float3(translation.Value.x, originalYValue, translation.Value.z);

            if (tickElapsed >= deltaTick)
            {
                if (pathIndex.value + 2 == path.Length)
                {
                    commandBuffer.RemoveComponent<Moving>(entityInQueryIndex, entity);
                    commandBuffer.RemoveComponent<PathIndex>(entityInQueryIndex, entity);
                    commandBuffer.AddComponent<PathTraveledCompletely>(entityInQueryIndex, entity);
                }
                else
                {
                    pathIndex.value++;
                }
            }


            if (pathIndex.lastSend != pathIndex.value)
            {
                if (pathIndex.value + 1 < path.Length)
                {
                    commandBuffer.AddComponent<SendNextWaypoint>(entityInQueryIndex, entity);

                    pathIndex.lastSend = pathIndex.value;
                }
            }


        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
