using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class ValidateBuildSystem : JobComponentSystem
{
    private struct TriggerJob : ITriggerEventsJob
    {
        public ComponentDataFromEntity<ValidateBuild> validateBuild;
        [ReadOnly] public ComponentDataFromEntity<BuildableSurface> buildableSurface;

        public void Execute(TriggerEvent triggerEvent)
        {
            byte onBuildableSurface = 0;

            if (buildableSurface.HasComponent(triggerEvent.Entities.EntityA))
            {
                onBuildableSurface = 1;
            }

            if (buildableSurface.HasComponent(triggerEvent.Entities.EntityB))
            {
                onBuildableSurface = 1;
            }

            if (validateBuild.HasComponent(triggerEvent.Entities.EntityA))
            {
                ValidateBuild validateBuild0 = validateBuild[triggerEvent.Entities.EntityA];
                if (onBuildableSurface == 0)
                {
                    validateBuild0.canBuild = 0;
                }
                validateBuild0.processed = 1;
                validateBuild[triggerEvent.Entities.EntityA] = validateBuild0;
            }

            if (validateBuild.HasComponent(triggerEvent.Entities.EntityB))
            {
                ValidateBuild validateBuild0 = validateBuild[triggerEvent.Entities.EntityB];
                if (onBuildableSurface == 0)
                {
                    validateBuild0.canBuild = 0;
                }
                validateBuild0.processed = 1;
                validateBuild[triggerEvent.Entities.EntityB] = validateBuild0;
            }
        }
    }

    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        TriggerJob triggerJob = new TriggerJob
        {
            validateBuild = GetComponentDataFromEntity<ValidateBuild>(),
            buildableSurface = GetComponentDataFromEntity<BuildableSurface>()
        };
        return triggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);
    }
}
