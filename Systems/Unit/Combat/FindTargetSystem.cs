using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;

/// <summary>
/// This system finds targets for entities with the TargetSeeker component attached. It also takes range, obstacles, target priority, and the angle to target (if necessary)
/// into consideration. For example, an entity 300m from a target seeker with a range of 200m will not be selected. Another example - if a targetSeeker is deciding between
/// two entities, one with a unitType.value of 0 and another of 2, and the TargetSeeker.priority target type is 2, the entity with the unitType value of 2
/// will be selected. Another example - if targetSeeker.maxAngleToTarget is 25 and the angle to the target from the target seeker is 60, the target will not be selected.
/// </summary>
[UpdateAfter(typeof(PlaceEntitiesInQuadrantsSystem))]
[UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
public class FindTargetSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;
    BuildPhysicsWorld buildPhysicsWorld;
    EndFramePhysicsSystem endFramePhysics;
    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        endFramePhysics = World.GetOrCreateSystem<EndFramePhysicsSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();
        NativeMultiHashMap<int, BucketedEntityData> entityQuadrantMultiHashmap = QuadrantMultiHashmaps.entityQuadrantMultiHashmap;
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
        Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.FinalJobHandle);

        Entities.WithNone<HasTarget>().ForEach((Entity entity, int entityInQueryIndex, ref TargetSeeker targetSeeker,
            in TeamID teamID, in LocalToWorld localToWorld) =>
        {
            int unitHashmapKey = QuadrantMultiHashmaps.GetPositionHashMapKey(localToWorld.Position);
            Entity targetEntity = Entity.Null;
            float3 targetPosition = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
            byte targetedUnitType = 0;

            //Search nearby quadrants.
            if(targetSeeker.maxAngleToTarget == 0)
            {
                //If clamped angle is 0, that means target can be at any angle to the target seeker.
                float3 seekerPosition = new float3(localToWorld.Position.x, localToWorld.Position.y + targetSeeker.seekOffsetY, localToWorld.Position.z);
                FindTarget(unitHashmapKey, seekerPosition, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTarget(unitHashmapKey + 1, seekerPosition, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTarget(unitHashmapKey - 1, seekerPosition, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTarget(unitHashmapKey + 1000, seekerPosition, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTarget(unitHashmapKey - 1000, seekerPosition, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTarget(unitHashmapKey + 1 + 1000, seekerPosition, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTarget(unitHashmapKey - 1 + 1000, seekerPosition, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTarget(unitHashmapKey + 1 - 1000, seekerPosition, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTarget(unitHashmapKey - 1 - 1000, seekerPosition, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
            }
            else
            {
                //Make sure target is clamped within field of fire.
                float3 seekerPosition = new float3(localToWorld.Position.x, localToWorld.Position.y + targetSeeker.seekOffsetY, localToWorld.Position.z);
                float3 targetSeekerForward = math.forward(GetComponent<LocalToWorld>(targetSeeker.defaultRotationEntity).Rotation);
                FindTargetWithAngleLimit(unitHashmapKey, seekerPosition, targetSeekerForward, targetSeeker.maxAngleToTarget, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTargetWithAngleLimit(unitHashmapKey + 1, seekerPosition, targetSeekerForward, targetSeeker.maxAngleToTarget, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTargetWithAngleLimit(unitHashmapKey - 1, seekerPosition, targetSeekerForward, targetSeeker.maxAngleToTarget, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTargetWithAngleLimit(unitHashmapKey + 1000, seekerPosition, targetSeekerForward, targetSeeker.maxAngleToTarget, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTargetWithAngleLimit(unitHashmapKey - 1000, seekerPosition, targetSeekerForward, targetSeeker.maxAngleToTarget, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTargetWithAngleLimit(unitHashmapKey + 1 + 1000, seekerPosition, targetSeekerForward, targetSeeker.maxAngleToTarget, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTargetWithAngleLimit(unitHashmapKey - 1 + 1000, seekerPosition, targetSeekerForward, targetSeeker.maxAngleToTarget, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTargetWithAngleLimit(unitHashmapKey + 1 - 1000, seekerPosition, targetSeekerForward, targetSeeker.maxAngleToTarget, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
                FindTargetWithAngleLimit(unitHashmapKey - 1 - 1000, seekerPosition, targetSeekerForward, targetSeeker.maxAngleToTarget, targetSeeker.detectionRadius, teamID.value, targetSeeker.priorityTargetType, ref targetEntity, ref targetPosition, ref targetedUnitType);
            }

            //Was a target was found?
            if (targetEntity != Entity.Null)
            {
                //Add hasTarget component, a target was found.
                HasTarget hasTarget = new HasTarget { entity = targetEntity };
                commandBuffer.AddComponent(entityInQueryIndex, entity, new HasTarget
                {
                    entity = targetEntity,
                    targetType = targetedUnitType,              
                });

                //Add a SendTarget component, so the SendCombatDataSystem can send the target to clients.
                commandBuffer.AddComponent<SendTarget>(entityInQueryIndex, entity);
            }

        }).WithReadOnly(collisionWorld).WithReadOnly(entityQuadrantMultiHashmap).ScheduleParallel();


        #region Find Target Methods

        void FindTarget(int hashmapKey, float3 seekerPosition, float detectionRadius, ushort seekerTeamID, byte priorityTargetType,
            ref Entity targetEntity, ref float3 targetPosition, ref byte targetedUnitType)
        {
            BucketedEntityData bucketedEntityData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if (entityQuadrantMultiHashmap.TryGetFirstValue(hashmapKey, out bucketedEntityData, out nativeMultiHashMapIterator))
            {
                do
                {
                    //Team check
                    if (bucketedEntityData.teamID != seekerTeamID)
                    {
                        //Targetable?
                        if (bucketedEntityData.unitType <= priorityTargetType)
                        {
                            //Do we have a target yet?
                            if (targetEntity == Entity.Null)
                            {
                                //Distance check for targeting range
                                if (math.distancesq(seekerPosition, bucketedEntityData.position) < (detectionRadius * detectionRadius))
                                {
                                    //Check for obstacles.
                                    bool obstacleInWay = CheckForObstaclesToTarget(bucketedEntityData, seekerPosition);

                                    if (!obstacleInWay)
                                    {
                                        //Set target data.
                                        targetEntity = bucketedEntityData.entity;
                                        targetPosition = bucketedEntityData.position;
                                        targetedUnitType = bucketedEntityData.unitType;
                                    }
                                }
                            }
                            else
                            {
                                //Is this entity a more preferable target?
                                if (bucketedEntityData.unitType > targetedUnitType)
                                {
                                    //Distance check for targeting range
                                    if (math.distancesq(seekerPosition, bucketedEntityData.position) < (detectionRadius * detectionRadius))
                                    {
                                        //Check for obstacles.
                                        bool obstacleInWay = CheckForObstaclesToTarget(bucketedEntityData, seekerPosition);

                                        if (!obstacleInWay)
                                        {
                                            //Set target data.
                                            targetEntity = bucketedEntityData.entity;
                                            targetPosition = bucketedEntityData.position;
                                            targetedUnitType = bucketedEntityData.unitType;
                                        }
                                    }
                                }
                                else
                                {
                                    //This entity is not more preferable, but is the unit closer?
                                    //Check if unit is not lesser (at least equal) in preferentiability than current target.
                                    if (bucketedEntityData.unitType == targetedUnitType)
                                    {
                                        //Is the target closer?
                                        if (math.distancesq(seekerPosition, bucketedEntityData.position) < math.distancesq(seekerPosition, targetPosition))
                                        {
                                            //Check for obstacles.
                                            bool obstacleInWay = CheckForObstaclesToTarget(bucketedEntityData, seekerPosition);

                                            if (!obstacleInWay)
                                            {
                                                //Set target data.
                                                targetEntity = bucketedEntityData.entity;
                                                targetPosition = bucketedEntityData.position;
                                                targetedUnitType = bucketedEntityData.unitType;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                } while (entityQuadrantMultiHashmap.TryGetNextValue(out bucketedEntityData, ref nativeMultiHashMapIterator));
            }
        }

        void FindTargetWithAngleLimit(int hashmapKey, float3 seekerPosition, float3 seekerForward, float maxAngle,
            float detectionRadius, ushort seekerTeamID, byte priorityTargetType, ref Entity targetEntity,
            ref float3 targetPosition, ref byte targetedUnitType)
        {
            BucketedEntityData bucketedEntityData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            if (entityQuadrantMultiHashmap.TryGetFirstValue(hashmapKey, out bucketedEntityData, out nativeMultiHashMapIterator))
            {
                do
                {
                    //Team check
                    if (bucketedEntityData.teamID != seekerTeamID)
                    {
                        //Targetable?
                        if (bucketedEntityData.unitType <= priorityTargetType)
                        {
                            //Do we have a target yet?
                            if (targetEntity == Entity.Null)
                            {
                                //Distance check for targeting range
                                if (math.distancesq(seekerPosition, bucketedEntityData.position) < (detectionRadius * detectionRadius))
                                {
                                    //Check if within field of fire.
                                    float3 dir = bucketedEntityData.position - seekerPosition;
                                    if (MathUtils.Float3Angle(seekerForward, dir) < maxAngle)
                                    {
                                        //Check for obstacles.
                                        bool obstacleInWay = CheckForObstaclesToTarget(bucketedEntityData, seekerPosition);

                                        if (!obstacleInWay)
                                        {
                                            //Set target data.
                                            targetEntity = bucketedEntityData.entity;
                                            targetPosition = bucketedEntityData.position;
                                            targetedUnitType = bucketedEntityData.unitType;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Is this entity a more preferable target?
                                if (bucketedEntityData.unitType > targetedUnitType)
                                {
                                    //Distance check for targeting range
                                    if (math.distancesq(seekerPosition, bucketedEntityData.position) < (detectionRadius * detectionRadius))
                                    {
                                        //Check if within field of fire.
                                        float3 dir = bucketedEntityData.position - seekerPosition;
                                        if (MathUtils.Float3Angle(seekerForward, dir) < maxAngle)
                                        {
                                            //Check for obstacles.
                                            bool obstacleInWay = CheckForObstaclesToTarget(bucketedEntityData, seekerPosition);

                                            if (!obstacleInWay)
                                            {
                                                //Set target data.
                                                targetEntity = bucketedEntityData.entity;
                                                targetPosition = bucketedEntityData.position;
                                                targetedUnitType = bucketedEntityData.unitType;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //This entity is not more preferable, but is the unit closer?
                                    //Check if unit is not lesser (at least equal) in preferentiability than current target.
                                    if (bucketedEntityData.unitType == targetedUnitType)
                                    {
                                        //Is the target closer?
                                        if (math.distancesq(seekerPosition, bucketedEntityData.position) < math.distancesq(seekerPosition, targetPosition))
                                        {
                                            //Check if within field of fire cone.
                                            float3 dir = bucketedEntityData.position - seekerPosition;
                                            if (MathUtils.Float3Angle(seekerForward, dir) < maxAngle)
                                            {
                                                //Check for obstacles.
                                                bool obstacleInWay = CheckForObstaclesToTarget(bucketedEntityData, seekerPosition);

                                                if (!obstacleInWay)
                                                {
                                                    //Set target data.
                                                    targetEntity = bucketedEntityData.entity;
                                                    targetPosition = bucketedEntityData.position;
                                                    targetedUnitType = bucketedEntityData.unitType;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                } while (entityQuadrantMultiHashmap.TryGetNextValue(out bucketedEntityData, ref nativeMultiHashMapIterator));
            }
        }

        #endregion

        #region FindTargetUtils

        bool CheckForObstaclesToTarget(BucketedEntityData bucketedEntityData, float3 seekerPosition)
        {
            float3 dir = bucketedEntityData.position - seekerPosition;
            dir.y += 1.5f; //arbitrary offset

            RaycastInput raycastInput = PhysicsUtils.CreateRaycastInputIgnoreOneLayer(seekerPosition,
                dir, math.distance(bucketedEntityData.position, seekerPosition), 2);

            return collisionWorld.CastRay(raycastInput);
        }

        #endregion

        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
