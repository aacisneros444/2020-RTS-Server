using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

/// <summary>
/// This system handles most functions related to combat including determining when to send client data
/// for combat, validating targets, rotating to the target, and shooting. It also sets the dependencies between these systems.
/// </summary>
[DisableAutoCreation]
public class TurretSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;
    BuildPhysicsWorld buildPhysicsWorld;
    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        #region Get references

        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();
        ComponentDataFromEntity<LocalToWorld> localToWorlds = GetComponentDataFromEntity<LocalToWorld>(true);
        ComponentDataFromEntity<Health> healths = GetComponentDataFromEntity<Health>(true);
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
        uint globalSimulationTick = GlobalSimulationTick.value;

        #endregion


        #region ShouldSendFiringDataJob

        /// <summary>
        /// This job checks to how many ticks targetSeekers have been without a target.
        /// If a certain amount of ticks have passed, the gun.clientShooting variable is set
        /// to 0 to indicate to the ShootSystem that clients will need to be sent combat data in
        /// order to accurately predict the server simulation.
        /// </summary>
        JobHandle shouldSendFiringDataJob = Entities.WithNone<HasTarget>().ForEach((Entity entity, ref TargetSeeker targetSeeker,
            ref Turret turret, ref Gun gun) =>
        {
            if (gun.clientShooting == 0)
                return;

            targetSeeker.ticksWithoutTarget++;
            if (targetSeeker.ticksWithoutTarget > 5)
            {
                gun.clientShooting = 0;
            }

        }).ScheduleParallel(Dependency);

        #endregion


        #region Validate Target Job

        /// <summary>
        /// This job checks to make sure the current target is not dead (exists), not out of range, and did not go behind an obstacle.
        /// </summary>
        JobHandle validateTargetJob = Entities.ForEach((Entity entity, int entityInQueryIndex, ref Turret turret, ref Gun gun,
            ref TargetSeeker targetSeeker, ref HasTarget hasTarget, in LocalToWorld localToWorld) =>
        {
            if (!healths.Exists(hasTarget.entity))
            {
                //Turret.PassedTargetToGunPivot will be set to 0 for the turretJob to pass a new target reference for the gunPivot job.
                turret.passedTargetToGunPivot = 0;

                //Gun.aimed will be set to 0 in order for gun jobs to not 
                //attempt to fire since it takes the command buffer one frame to get rid of the HasTarget component.
                gun.aimed = 0;

                //TargetSeeker.ticksWithoutTarget will be set to 0 for the shouldSendFiringData job.
                targetSeeker.ticksWithoutTarget = 0;

                commandBuffer.RemoveComponent<HasTarget>(entityInQueryIndex, entity);

                if (turret.gunPivot == Entity.Null)
                    return;

                commandBuffer.RemoveComponent<HasTarget>(entityInQueryIndex, turret.gunPivot);
            }
            else
            {
                //Range check.
                if(math.distancesq(localToWorlds[hasTarget.entity].Position, localToWorlds[entity].Position) > (targetSeeker.detectionRadius * targetSeeker.detectionRadius))
                {
                    //Turret.PassedTargetToGunPivot will be set to 0 for the turretJob to pass a new target reference for the gunPivot job.
                    turret.passedTargetToGunPivot = 0;

                    //Gun.aimed will be set to 0 in order for gun jobs to not 
                    //attempt to fire since it takes the command buffer one frame to get rid of the HasTarget component.
                    gun.aimed = 0;

                    //TargetSeeker.ticksWithoutTarget will be set to 0 for the shouldSendFiringData job.
                    targetSeeker.ticksWithoutTarget = 0;

                    commandBuffer.RemoveComponent<HasTarget>(entityInQueryIndex, entity);

                    if (turret.gunPivot == Entity.Null)
                        return;

                    commandBuffer.RemoveComponent<HasTarget>(entityInQueryIndex, turret.gunPivot);
                }

                //Periodic obstacle check.
                if (hasTarget.ticksWithoutObstacleCheck > 30)
                {
                    float3 checkerPosition = localToWorld.Position;
                    checkerPosition.y += targetSeeker.seekOffsetY;
                    float3 targetPosition = localToWorlds[hasTarget.entity].Position;
                    float3 dir = targetPosition - localToWorld.Position;

                    RaycastInput raycastInput = PhysicsUtils.CreateRaycastInputIgnoreOneLayer(checkerPosition,
                        dir, math.distance(targetPosition, checkerPosition), 2);

                    if (collisionWorld.CastRay(raycastInput))
                    {
                        //Turret.PassedTargetToGunPivot will be set to 0 for the turretJob to pass a new target reference for the gunPivot job.
                        turret.passedTargetToGunPivot = 0;

                        //Gun.aimed will be set to 0 in order for gun jobs to not 
                        //attempt to fire since it takes the command buffer one frame to get rid of the HasTarget component.
                        gun.aimed = 0;

                        //TargetSeeker.ticksWithoutTarget will be set to 0 for the shouldSendFiringData job.
                        targetSeeker.ticksWithoutTarget = 0;

                        commandBuffer.RemoveComponent<HasTarget>(entityInQueryIndex, entity);

                        if (turret.gunPivot == Entity.Null)
                            return;

                        commandBuffer.RemoveComponent<HasTarget>(entityInQueryIndex, turret.gunPivot);
                    }

                    hasTarget.ticksWithoutObstacleCheck = 0;
                }
                else
                {
                    hasTarget.ticksWithoutObstacleCheck++;
                }
            }

        }).WithReadOnly(localToWorlds).WithReadOnly(healths).WithReadOnly(collisionWorld).ScheduleParallel(shouldSendFiringDataJob);

        #endregion


        #region RotateTurretJobs

        /// <summary>
        /// This job rotates the turret entity to the target, passes the reference to the target entity to the gunPivot entity for rotating,
        /// and determines if gun is aimed for shoot jobs.
        /// </summary>
        JobHandle rotateTurretJobHasParent = Entities.ForEach((Entity entity, int entityInQueryIndex, ref Turret turret, ref Rotation rotation,
            ref Gun gun, in Parent parent, in HasTarget hasTarget) =>
        {
            //Check to see if target still exists.
            if (!healths.Exists(hasTarget.entity))
                return;


            //If gunPivot is not null, this is supposed to act like a tank turret (y rotation on main turret and x on a gunPivot).
            //If gunPivot is null, this is supposed to act like a ball turret (looks in all directions).
            if(turret.gunPivot != Entity.Null)
            {
                //Pass target reference to gunPivot entity for rotating (limits usage of commandbuffer set rotation spam).
                if (turret.passedTargetToGunPivot == 0)
                {
                    commandBuffer.AddComponent(entityInQueryIndex, turret.gunPivot, hasTarget);

                    turret.passedTargetToGunPivot = 1;
                }

                //Rotate turret towards target.
                float3 targetPosition = localToWorlds[hasTarget.entity].Position;
                rotation.Value = MathUtils.RotateToTargetHasParent(localToWorlds[parent.Value].Value, localToWorlds[entity].Position, rotation.Value,
                    turret.traverseSpeed, localToWorlds[hasTarget.entity].Position, new float3(0, 1, 0), turret.clamp);


                //Check if aimed.
                float3 dir = targetPosition - localToWorlds[turret.gunPivot].Position;
                if (math.dot(math.forward(localToWorlds[turret.gunPivot].Rotation), math.normalize(dir)) > 0.995f)
                {
                    gun.aimed = 1;
                }
                else
                {
                    gun.aimed = 0;
                }
            }
            else
            {
                //Rotate turret towards target.
                float3 targetPosition = localToWorlds[hasTarget.entity].Position;
                rotation.Value = MathUtils.RotateToTargetHasParent(localToWorlds[parent.Value].Value, localToWorlds[entity].Position, rotation.Value,
                    turret.traverseSpeed, localToWorlds[hasTarget.entity].Position, new float3(0, 0, 0), turret.clamp);


                //Check if aimed.
                float3 dir = targetPosition - localToWorlds[entity].Position;
                if (math.dot(math.forward(localToWorlds[entity].Rotation), math.normalize(dir)) > 0.995f)
                {
                    gun.aimed = 1;
                }
                else
                {
                    gun.aimed = 0;
                }
            }
            
        }).WithReadOnly(healths).WithReadOnly(localToWorlds).ScheduleParallel(validateTargetJob);

        JobHandle rotateTurretJobNoParent = Entities.WithNone<Parent>().ForEach((Entity entity, int entityInQueryIndex, ref Turret turret,
            ref Rotation rotation, ref Gun gun, in HasTarget hasTarget) =>
        {
            //Check to see if target still exists.
            if (!healths.Exists(hasTarget.entity))
                return;


            //If gunPivot is not null, this is supposed to act like a tank turret (y rotation on main turret and x on a gunPivot).
            //If gunPivot is null, this is supposed to act like a ball turret (looks in all directions).
            if (turret.gunPivot != Entity.Null)
            {
                //Pass target reference to gunPivot entity for rotating (limits usage of commandbuffer set rotation spam).
                if (turret.passedTargetToGunPivot == 0)
                {
                    commandBuffer.AddComponent(entityInQueryIndex, turret.gunPivot, hasTarget);

                    turret.passedTargetToGunPivot = 1;
                }

                //Rotate turret towards target.
                float3 targetPosition = localToWorlds[hasTarget.entity].Position;
                rotation.Value = MathUtils.RotateToTarget(localToWorlds[entity].Position, rotation.Value,
                    turret.traverseSpeed, localToWorlds[hasTarget.entity].Position, new float3(0, 1, 0), turret.clamp);


                //Check if aimed.
                float3 dir = targetPosition - localToWorlds[turret.gunPivot].Position;
                if (math.dot(math.forward(localToWorlds[turret.gunPivot].Rotation), math.normalize(dir)) > 0.995f)
                {
                    gun.aimed = 1;
                }
                else
                {
                    gun.aimed = 0;
                }
            }
            else
            {
                //Rotate turret towards target.
                float3 targetPosition = localToWorlds[hasTarget.entity].Position;
                rotation.Value = MathUtils.RotateToTarget(localToWorlds[entity].Position, rotation.Value,
                    turret.traverseSpeed, localToWorlds[hasTarget.entity].Position, new float3(0, 0, 0), turret.clamp);


                //Check if aimed.
                float3 dir = targetPosition - localToWorlds[entity].Position;
                if (math.dot(math.forward(localToWorlds[entity].Rotation), math.normalize(dir)) > 0.995f)
                {
                    gun.aimed = 1;
                }
                else
                {
                    gun.aimed = 0;
                }
            }

        }).WithReadOnly(healths).WithReadOnly(localToWorlds).ScheduleParallel(rotateTurretJobHasParent);

        #endregion


        #region GunPivotJob

        /// <summary>
        /// This job rotates the gunPivot entity to the target.
        /// </summary>
        JobHandle gunPivotJob = Entities.ForEach((Entity entity, ref TurretGunPivot turretGunPivot, ref Rotation rotation,
            in Parent parent, in HasTarget hasTarget) =>
        {
            //Check to see if target still exists.
            if (!healths.Exists(hasTarget.entity))
                return;


            rotation.Value = MathUtils.RotateToTargetAHasParent(localToWorlds[parent.Value].Value, localToWorlds[entity].Position, rotation.Value,
                turretGunPivot.elevationSpeed, localToWorlds[hasTarget.entity].Position, new float3(1, 0, 0), turretGunPivot.clamp);


        }).WithReadOnly(healths).WithReadOnly(localToWorlds).ScheduleParallel(rotateTurretJobNoParent);

        #endregion


        #region ShootJobs


        /// <summary>
        /// This job handles firing for all entities with the Gun and Humanoid component attached to them.
        /// Does not run on moving humanoids.
        /// </summary>
        JobHandle shootJobHumanoid = Entities.WithNone<Moving>().WithAll<Humanoid>().ForEach((Entity entity, int entityInQueryIndex,
            ref Gun gun, in HasTarget hasTarget) =>
        {
            //Is gun aimed?
            if (gun.aimed == 0)
                return;

            //Is main gun loaded?
            if (gun.roundsInMagazine == 0)
                return;

            //Has there been enough time between shots taken?
            if (gun.ticksBetweenShotsTaken < gun.ticksBetweenShots)
            {
                gun.ticksBetweenShotsTaken++;
                return;
            }

            //All set. Fire!


            //Creates a bullet at proper position in proper direction
            Entity round = commandBuffer.Instantiate(entityInQueryIndex, gun.round);
            commandBuffer.SetComponent(entityInQueryIndex, round, new Translation { Value = localToWorlds[gun.muzzle].Position });

            float3 targetPosition = localToWorlds[hasTarget.entity].Position;
            float3 dir = new float3(targetPosition.x, targetPosition.y + 1, targetPosition.z) - localToWorlds[gun.muzzle].Position;
            quaternion rotationWithDispersion = math.mul(quaternion.LookRotationSafe(dir, math.up()),
                  quaternion.Euler(math.radians(gun.dispersionValue * math.sin(globalSimulationTick)),
                  math.radians(gun.dispersionValue * math.sin(globalSimulationTick - 200)), 0));
            commandBuffer.SetComponent(entityInQueryIndex, round, new Rotation { Value = rotationWithDispersion });


            //Client Update/ For Client---
            if (gun.clientShooting == 0)
            {
                gun.clientShooting = 1;
                commandBuffer.AddComponent(entityInQueryIndex, entity, new SendStartFiring
                {
                    startFiringTick = globalSimulationTick,
                    roundsInTheMagazine = gun.roundsInMagazine
                });
            }

            gun.roundsInMagazine--;
            gun.ticksBetweenShotsTaken = 1;

        }).WithReadOnly(localToWorlds).ScheduleParallel(gunPivotJob);




        /// <summary>
        /// This job handles firing for all entities with the Gun component attached to them.
        /// </summary>
        JobHandle shootJob = Entities.WithNone<GunC, Humanoid>().ForEach((Entity entity, int entityInQueryIndex,
            ref Gun gun, in HasTarget hasTarget) =>
        {
            //Is gun aimed?
            if (gun.aimed == 0)
                return;

            //Is main gun loaded?
            if (gun.roundsInMagazine == 0)
                return;

            //Has there been enough time between shots taken?
            if (gun.ticksBetweenShotsTaken < gun.ticksBetweenShots)
            {
                gun.ticksBetweenShotsTaken++;
                return;
            }

            //All set. Fire!


            //Creates a bullet at proper position in proper direction
            Entity round = commandBuffer.Instantiate(entityInQueryIndex, gun.round);
            commandBuffer.SetComponent(entityInQueryIndex, round, new Translation { Value = localToWorlds[gun.muzzle].Position });

            float3 targetPosition = localToWorlds[hasTarget.entity].Position;
            float3 dir = new float3(targetPosition.x, targetPosition.y + 1, targetPosition.z) - localToWorlds[gun.muzzle].Position;
            quaternion rotationWithDispersion = math.mul(quaternion.LookRotationSafe(dir, math.up()),
                  quaternion.Euler(math.radians(gun.dispersionValue * math.sin(globalSimulationTick)),
                  math.radians(gun.dispersionValue * math.sin(globalSimulationTick - 200)), 0));
            commandBuffer.SetComponent(entityInQueryIndex, round, new Rotation { Value = rotationWithDispersion });


            //Client Update/ For Client---
            if (gun.clientShooting == 0)
            {
                gun.clientShooting = 1;
                commandBuffer.AddComponent(entityInQueryIndex, entity, new SendStartFiring
                {
                    startFiringTick = globalSimulationTick,
                    roundsInTheMagazine = gun.roundsInMagazine
                });
            }

            gun.roundsInMagazine--;
            gun.ticksBetweenShotsTaken = 1;

        }).WithReadOnly(localToWorlds).ScheduleParallel(shootJobHumanoid);




        /// <summary>
        /// This job handles firing for all entities with both the Gun and GunC component attached to them.
        /// This involves the firing of both a main gun and secondary gun for the same turret (used for tanks with coaxial MG).
        /// </summary>
        JobHandle shootJobWithCoaxial = Entities.ForEach((Entity entity, int entityInQueryIndex, ref Turret turret, ref Gun gun,
            ref GunC gunC, in HasTarget hasTarget) =>
        {
            //Is gun aimed?
            if (gun.aimed == 0)
                return;

            //Is main gun loaded?
            if(gun.roundsInMagazine > 0)
            {
                //Has there been enough time between shots taken?
                if (gun.ticksBetweenShotsTaken < gun.ticksBetweenShots)
                {
                    gun.ticksBetweenShotsTaken++;
                    return;
                }

                //All set. Fire!


                //Creates a bullet at proper position in proper direction with dispersion
                Entity round = commandBuffer.Instantiate(entityInQueryIndex, gun.round);
                commandBuffer.SetComponent(entityInQueryIndex, round, new Translation
                {
                    Value = localToWorlds[gun.muzzle].Position
                });

                //Create dispersion 
                float3 targetPosition = localToWorlds[hasTarget.entity].Position;
                float3 dir = new float3(targetPosition.x, targetPosition.y + 1, targetPosition.z) - localToWorlds[gun.muzzle].Position;
                quaternion rotationWithDispersion = math.mul(quaternion.LookRotationSafe(dir, math.up()),
                    quaternion.Euler(math.radians(gun.dispersionValue * math.sin(globalSimulationTick)),
                    math.radians(gun.dispersionValue * math.sin(globalSimulationTick - 200)), 0));

                commandBuffer.SetComponent(entityInQueryIndex, round, new Rotation
                {
                    Value = rotationWithDispersion
                });

                //Client Update/ For Client---
                if (gun.clientShooting == 0)
                {
                    gun.clientShooting = 1;
                    commandBuffer.AddComponent(entityInQueryIndex, entity, new SendStartFiring
                    {
                        startFiringTick = globalSimulationTick,
                        roundsInTheMagazine = gun.roundsInMagazine,
                        roundsInTheMagazineC = gunC.roundsInMagazine
                    });
                }

                gun.roundsInMagazine--;
                gun.ticksBetweenShotsTaken = 1;

            }
            else
            {
                //No rounds in primary gun. Use secondary coaxial gun.

                //Is target infantry?
                if (hasTarget.targetType > 0)
                    return;

                //Is gun loaded?
                if (gunC.roundsInMagazine == 0)
                    return;


                //Has there been enough time between shots taken?
                if (gunC.ticksBetweenShotsTaken < gunC.ticksBetweenShots)
                {
                    gunC.ticksBetweenShotsTaken++;
                    return;
                }

                //All set. Fire!


                //Creates a bullet at proper position in proper direction with dispersion
                Entity round = commandBuffer.Instantiate(entityInQueryIndex, gunC.round);
                commandBuffer.SetComponent(entityInQueryIndex, round, new Translation
                {
                    Value = localToWorlds[gunC.muzzle].Position
                });

                //Create dispersion 
                float3 targetPosition = localToWorlds[hasTarget.entity].Position;
                float3 dir = new float3(targetPosition.x, targetPosition.y + 1, targetPosition.z) - localToWorlds[gunC.muzzle].Position;
                quaternion rotationWithDispersion = math.mul(quaternion.LookRotationSafe(dir, math.up()),
                    quaternion.Euler(math.radians(gunC.dispersionValue * math.sin(globalSimulationTick)),
                    math.radians(gunC.dispersionValue * math.sin(globalSimulationTick - 200)), 0));

                commandBuffer.SetComponent(entityInQueryIndex, round, new Rotation
                {
                    Value = rotationWithDispersion
                });

                //Client Update/ For Client---
                if (gun.clientShooting == 0)
                {
                    gun.clientShooting = 1;
                    commandBuffer.AddComponent(entityInQueryIndex, entity, new SendStartFiring
                    {
                        startFiringTick = globalSimulationTick,
                        roundsInTheMagazine = gun.roundsInMagazine,
                        roundsInTheMagazineC = gunC.roundsInMagazine
                    });
                }

                gunC.roundsInMagazine--;
                gunC.ticksBetweenShotsTaken = 1;

            }                            

        }).WithReadOnly(localToWorlds).ScheduleParallel(shootJob);


        #endregion

        Dependency = shootJobWithCoaxial;

        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
