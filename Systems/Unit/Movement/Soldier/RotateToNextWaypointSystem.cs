using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class RotateToNextWaypointSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;

        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();

        JobHandle jobHandle = Entities.WithAll<Moving>().ForEach((Entity entity, int entityInQueryIndex,
            DynamicBuffer<Waypoint> path, ref Rotation rotation, ref Translation translation, in AngularSpeed angularSpeed,
            in PathIndex pathIndex) =>
        {
            if (pathIndex.value > path.Length - 1)
                return;

            float3 dir = path[pathIndex.value + 1].point - translation.Value;
            dir = new float3(dir.x, 0, dir.z);
            if (dir.x == 0 && dir.y == 0 && dir.z == 0)
            {
                return;
            }
            quaternion targetRot = quaternion.LookRotationSafe(dir, new float3(0, 1, 0));
            rotation.Value = math.slerp(rotation.Value, targetRot, angularSpeed.value * deltaTime);

        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
