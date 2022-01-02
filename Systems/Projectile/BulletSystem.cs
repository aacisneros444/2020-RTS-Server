using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Collections;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;

public class BulletSystem : SystemBase
{
    BuildPhysicsWorld buildPhysicsWorld;
    EndFramePhysicsSystem endFramePhysics;
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        endFramePhysics = World.GetOrCreateSystem<EndFramePhysicsSystem>();
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();

        float deltaTime = Time.DeltaTime;
        PhysicsWorld physicsWorld = buildPhysicsWorld.PhysicsWorld;
        ComponentDataFromEntity<Health> healths = GetComponentDataFromEntity<Health>(true);

        JobHandle bulletJob = Entities.WithNone<Penetration>().ForEach((Entity entity, int entityInQueryIndex, ref Bullet bullet,
            ref Translation translation, ref Rotation rotation) =>
        {
            float3 moveVector = math.forward(rotation.Value) * bullet.speed * deltaTime;

            RaycastInput raycastInput = new RaycastInput
            {
                Start = translation.Value,
                End = translation.Value + moveVector,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                }
            };

            Unity.Physics.RaycastHit raycastHit = new Unity.Physics.RaycastHit();
            bool haveHit = physicsWorld.CastRay(raycastInput, out raycastHit);

            if (haveHit)
            {
                translation.Value = raycastHit.Position;

                commandBuffer.DestroyEntity(entityInQueryIndex, entity);

                if (HasComponent<Armor>(raycastHit.Entity))
                    return;

                if (!HasComponent<RootEntity>(raycastHit.Entity))
                    return;

                Entity parentEntity = GetComponent<RootEntity>(raycastHit.Entity).entity;

                //Set health data.
                if (healths.HasComponent(parentEntity))
                {
                    Health health = healths[parentEntity];

                    commandBuffer.SetComponent(entityInQueryIndex, parentEntity, new Health
                    {
                        value = health.value -= bullet.damage,
                        maxValue = health.maxValue,
                        lastSendValue = health.lastSendValue
                    });
                }
            }
            else
            {
                translation.Value += moveVector;
            }

            //Destroy bullet after expiration time
            bullet.activeTime += deltaTime;
            if (bullet.activeTime > bullet.lifetime)
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }

        }).WithReadOnly(physicsWorld).WithReadOnly(healths).ScheduleParallel(Dependency);

        JobHandle bulletWithPenetrationJob = Entities.ForEach((Entity entity, int entityInQueryIndex, ref Bullet bullet,
            ref Translation translation, ref Rotation rotation, in Penetration penetration) =>
        {
            float3 moveVector = math.forward(rotation.Value) * bullet.speed * deltaTime;

            RaycastInput raycastInput = new RaycastInput
            {
                Start = translation.Value,
                End = translation.Value + moveVector,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                }
            };

            Unity.Physics.RaycastHit raycastHit = new Unity.Physics.RaycastHit();
            bool haveHit = physicsWorld.CastRay(raycastInput, out raycastHit);

            if (haveHit)
            {
                translation.Value = raycastHit.Position;

                commandBuffer.DestroyEntity(entityInQueryIndex, entity);

                //Does the hit entity have the armor component?
                if (HasComponent<Armor>(raycastHit.Entity))
                {
                    //Calculate effective armor thickness (with angling).
                    float armor = GetComponent<Armor>(raycastHit.Entity).value;
                    float angle = MathUtils.Float3Angle(-math.forward(rotation.Value), raycastHit.SurfaceNormal);
                    float effectiveArmor = armor / (math.cos(math.radians(angle)));

                    //Does the bullet have more penetration than the hit entity's armor?
                    if (penetration.value < effectiveArmor)
                        return;

                    //Check to see if hit entity has the root entity component.
                    if (!HasComponent<RootEntity>(raycastHit.Entity))
                        return;

                    Entity rootEntity = GetComponent<RootEntity>(raycastHit.Entity).entity;

                    //Set health data.
                    if (healths.HasComponent(rootEntity))
                    {
                        Health health = healths[rootEntity];

                        commandBuffer.SetComponent(entityInQueryIndex, rootEntity, new Health
                        {
                            value = health.value -= bullet.damage,
                            maxValue = health.maxValue,
                            lastSendValue = health.lastSendValue
                        });
                    }
                }
                else
                {
                    //Check to see if hit entity has the root entity component.
                    if (!HasComponent<RootEntity>(raycastHit.Entity))
                        return;

                    Entity rootEntity = GetComponent<RootEntity>(raycastHit.Entity).entity;

                    //Set health data.
                    if (healths.HasComponent(rootEntity))
                    {
                        Health health = healths[rootEntity];

                        commandBuffer.SetComponent(entityInQueryIndex, rootEntity, new Health
                        {
                            value = health.value -= bullet.damage,
                            maxValue = health.maxValue,
                            lastSendValue = health.lastSendValue
                        });
                    }
                }
            }
            else
            {
                translation.Value += moveVector;
            }

            //Destroy bullet after expiration time
            bullet.activeTime += deltaTime;
            if (bullet.activeTime > bullet.lifetime)
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }

        }).WithReadOnly(physicsWorld).WithReadOnly(healths).ScheduleParallel(bulletJob);

        endFramePhysics.HandlesToWaitFor.Add(bulletWithPenetrationJob);

        Dependency = bulletWithPenetrationJob;
    }
}
