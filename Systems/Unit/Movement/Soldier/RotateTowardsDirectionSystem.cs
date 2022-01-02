using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class RotateTowardsDirectionSystem : JobComponentSystem
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

        JobHandle jobHandle = Entities.WithNone<Moving>().ForEach((Entity entity, int entityInQueryIndex,
            ref Rotation rotation, ref Translation translation, ref LocalToWorld localToWorld,
            in AngularSpeed angularSpeed, in RotateTowardsDirection rotateTowards) =>
        {
            if (rotateTowards.buffer == 1)
                return;

            quaternion targetRot = quaternion.LookRotationSafe(rotateTowards.direction, new float3(0, 1, 0));
            rotation.Value = math.slerp(rotation.Value, targetRot, angularSpeed.value * deltaTime);

            float dotProduct = math.dot(localToWorld.Forward, math.normalize(rotateTowards.direction));
            
            if(dotProduct > 0.98f)
            {
                commandBuffer.RemoveComponent<RotateTowardsDirection>(entityInQueryIndex, entity);
            }

        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
