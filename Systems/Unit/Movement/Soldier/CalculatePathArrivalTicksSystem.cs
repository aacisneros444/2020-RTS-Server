using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class CalculatePathArrivalTicksSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int sumlationTickRate = GlobalSimulationTickManager.simulationTickRate;
        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();

        JobHandle jobHandle = Entities.WithAll<CalculatePathArrivalTicks>().ForEach((Entity entity,
            int entityInQueryIndex, DynamicBuffer<Waypoint> path, in MovementSpeed movementSpeed) =>
        {
            for(int i = 0; i < path.Length - 1; i++)
            {
                float distanceToNextWaypoint = math.distance(path[i].point, path[i + 1].point);
                float ticksUntilNextWaypoint = math.ceil(distanceToNextWaypoint / (movementSpeed.value / sumlationTickRate));

                Waypoint waypoint0 = path[i];

                Waypoint waypoint1 = path[i + 1];
                waypoint1.arrivalTick = (uint)(waypoint0.arrivalTick + ticksUntilNextWaypoint);
                path[i + 1] = waypoint1;
            }

            commandBuffer.RemoveComponent<PathQueued>(entityInQueryIndex, entity);
            commandBuffer.RemoveComponent<CalculatePathArrivalTicks>(entityInQueryIndex, entity);
            commandBuffer.AddComponent<PathIndex>(entityInQueryIndex, entity);
            commandBuffer.AddComponent<Moving>(entityInQueryIndex, entity);
            commandBuffer.AddComponent<SendFirstWaypoints>(entityInQueryIndex, entity);
            commandBuffer.AddComponent<SendRotateTowardsDirection>(entityInQueryIndex, entity);

        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return (jobHandle);
    }
}
