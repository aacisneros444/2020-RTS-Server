using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Jobs;

[DisableAutoCreation]
public class TankMoveSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;
    BuildPhysicsWorld buildPhysicsWorld;
    ExportPhysicsWorld exportPhysicsWorld;
    EndFramePhysicsSystem endFramePhysics;

    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        exportPhysicsWorld = World.GetOrCreateSystem<ExportPhysicsWorld>();
        endFramePhysics = World.GetOrCreateSystem<EndFramePhysicsSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();
        PhysicsWorld physicsWorld = buildPhysicsWorld.PhysicsWorld;
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;
        ComponentDataFromEntity<LocalToWorld> localToWorlds = GetComponentDataFromEntity<LocalToWorld>(true);
        uint globalSimulationTick = GlobalSimulationTick.value;

        //Rotate to face waypoint job
        JobHandle rotationJob = Entities.ForEach((Entity entity, DynamicBuffer<Waypoint> path, ref Rotation rotation,
            ref TankMoving tankMoving, in TankMovement tankMovement, in Translation translation) =>
        {
            //Buffered
            if (globalSimulationTick < tankMoving.startTick)
                return;


            //Get direction to waypoint.
            float3 dir = path[tankMoving.pathIndex + 1].point - translation.Value;
            dir.y = 0;

            //Cancel out y component of forward vector and normalize to not take height into consideration when deciding if vehicle is facing in direction of waypoint.
            float3 forward = math.forward(rotation.Value);
            forward.y = 0;
            math.normalize(forward);
            if (tankMoving.rotateInPlace == 1)
            {
                //This initial check exists to see if a tank may be potentially stuck while rotating in place.
                //Getting stuck occurs due to slight changes in rotation when adjusting to the ground normal.
                //If the delta dot product is small enough, it's likely the vehicle is stuck but generally facing the right way.
                //With this knowledge, allowing the tank to move by setting rotateInPlace to 0, allows the vehicle to move forward and
                //correct itself on its path.

                float dotProductThisFrame = math.dot(forward, math.normalize(dir));
                if (dotProductThisFrame - tankMoving.dotProductLastFrame < 0.0005f)
                {
                    tankMoving.rotateInPlace = 0;
                }
            }

            //Rotate to face waypoint if not facing waypoint.
            if (tankMoving.facingWaypoint == 0)
            {
                //Get angle to waypoint and target rotation
                quaternion targetRotation = quaternion.LookRotation(dir, math.up());
                float angleToWaypoint = 0;
                if (tankMoving.reverse == 0)
                {
                    angleToWaypoint = MathUtils.GetQuaternionAngle(rotation.Value, targetRotation);
                }

                //Check if angle is too big or too close to rotate to on the move. (Do this once per waypoint to decide method of moving.)
                if (tankMoving.checkAngleSize == 0)
                {
                    float sqrMagnitude = math.distancesq(path[tankMoving.pathIndex + 1].point, translation.Value);

                    if (angleToWaypoint > tankMovement.maxMoveAndTurnAngle || sqrMagnitude < 56f)
                    {
                        //Rotate in place, angle too big!
                        if (angleToWaypoint < 140f)
                        {
                            tankMoving.currentMovementSpeed = 0;
                            tankMoving.rotateInPlace = 1;
                        }
                        else
                        {
                            //Rotate in place and reverse. Waypoint is not too far and is right behind the vehicle. 
                            if (sqrMagnitude < 225f)
                            {
                                tankMoving.currentMovementSpeed = 0;
                                tankMoving.reverse = 1;
                                tankMoving.rotateInPlace = 1;
                            }
                            else
                            {
                                //Rotate in place. Angle is behind us but too far away.
                                tankMoving.currentMovementSpeed = 0;
                                tankMoving.rotateInPlace = 1;
                            }
                        }
                    }

                    tankMoving.checkAngleSize = 1;
                }

                //If reversing, add 180 to target rotation since we want to rotate to face the opposite vector for reversal.
                if (tankMoving.reverse == 1)
                {
                    targetRotation = math.mul(targetRotation, quaternion.Euler(0f, math.radians(180f), 0f));
                    angleToWaypoint = MathUtils.GetQuaternionAngle(rotation.Value, targetRotation);
                }

                //Actually rotate
                float timeToComplete = angleToWaypoint / tankMovement.traverseSpeed;
                float rotationPercentage = math.min(1F, GlobalSimulationTickManager.simulationTickRate / timeToComplete);

                rotation.Value = math.slerp(rotation.Value, targetRotation, rotationPercentage);


                //Use dot products to determine if facing waypoint.
                float dotToTarget = math.dot(forward, math.normalize(dir));
                if (tankMoving.reverse == 0)
                {
                    if (dotToTarget > 0.9999f)
                    {
                        tankMoving.facingWaypoint = 1;
                        tankMoving.checkAngleSize = 0;
                        tankMoving.rotateInPlace = 0;
                    }
                }
                else
                {
                    if (dotToTarget < -0.9999f)
                    {
                        tankMoving.facingWaypoint = 1;
                        tankMoving.checkAngleSize = 0;
                        tankMoving.rotateInPlace = 0;
                    }
                }

                //Don't move if rotating in place.
                if (tankMoving.rotateInPlace == 1)
                {
                    tankMoving.dotProductLastFrame = dotToTarget;
                    return;
                }
            }
            else
            {
                //Have we strayed too far from our inteded path? (this happens because this rotate function and the normal adjustment 
                //function compete for Y rotation values).
                //If so, mark facingWaypoint as 0 in order to rotate to waypoing.

                float dotToTarget = math.dot(forward, math.normalize(dir));
                if (tankMoving.reverse == 0)
                {
                    if (dotToTarget < 0.9995f)
                    {
                        tankMoving.facingWaypoint = 0;
                        tankMoving.checkAngleSize = 1;
                        tankMoving.rotateInPlace = 0;
                    }
                }
                else
                {
                    if (dotToTarget > -0.9995f)
                    {
                        tankMoving.facingWaypoint = 0;
                        tankMoving.checkAngleSize = 1;
                        tankMoving.rotateInPlace = 0;
                    }
                }
            }

        }).ScheduleParallel(Dependency);

        //Adjust to surface normal and set Y translation value job
        JobHandle adjustYAndToSurfaceNormalJob = Entities.ForEach((Entity entity, ref Translation translation, ref Rotation rotation,
            in VerticalCorrectionRaycastDataTank raycastCorrection, in TankMoving tankMoving) =>
        {
            //Buffered
            if (globalSimulationTick < tankMoving.startTick)
                return;

            Unity.Physics.RaycastHit backleft = new Unity.Physics.RaycastHit();
            Unity.Physics.RaycastHit backRight = new Unity.Physics.RaycastHit();
            Unity.Physics.RaycastHit frontLeft = new Unity.Physics.RaycastHit();
            Unity.Physics.RaycastHit frontRight = new Unity.Physics.RaycastHit();

            RaycastInput raycastInputBL = CreateRaycastInput(localToWorlds[raycastCorrection.backLeft].Position, 20f);
            RaycastInput raycastInputBR = CreateRaycastInput(localToWorlds[raycastCorrection.backRight].Position, 20f);
            RaycastInput raycastInputFL = CreateRaycastInput(localToWorlds[raycastCorrection.frontLeft].Position, 20f);
            RaycastInput raycastInputFR = CreateRaycastInput(localToWorlds[raycastCorrection.frontRight].Position, 20f);

            bool hit0 = physicsWorld.CastRay(raycastInputBL, out backleft);
            bool hit1 = physicsWorld.CastRay(raycastInputBR, out backRight);
            bool hit2 = physicsWorld.CastRay(raycastInputFL, out frontLeft);
            bool hit3 = physicsWorld.CastRay(raycastInputFR, out frontRight);

            //For safety (in case a raycast misses)
            if (hit0 == false || hit1 == false || hit2 == false || hit3 == false)
            {
                return;
            }

            float3 a = backRight.Position - backleft.Position;
            float3 b = frontRight.Position - backRight.Position;
            float3 c = frontLeft.Position - frontRight.Position;
            float3 d = backRight.Position - frontLeft.Position;

            float3 crossBA = math.cross(b, a);
            float3 crossCB = math.cross(c, b);
            float3 crossDC = math.cross(d, c);
            float3 crossAD = math.cross(a, d);

            //Set rotation
            quaternion target = math.mul(MathUtils.FromToRotation(localToWorlds[entity].Up, math.normalize(crossBA + crossCB + crossDC + crossAD)), rotation.Value);
            rotation.Value = math.slerp(rotation.Value, target, 0.0166f * 20f);

            //Set Y translation value
            float y = (backleft.Position.y + backRight.Position.y + frontLeft.Position.y + frontRight.Position.y) / 4f;
            translation.Value = math.lerp(translation.Value, new float3(translation.Value.x, y, translation.Value.z), 0.0166f * 20f);

        }).WithReadOnly(physicsWorld).WithReadOnly(localToWorlds).ScheduleParallel(rotationJob);

        JobHandle intermediateDependencies = JobHandle.CombineDependencies(rotationJob, adjustYAndToSurfaceNormalJob);

        //Move job
        JobHandle moveJob = Entities.ForEach((Entity entity, int entityInQueryIndex, DynamicBuffer<Waypoint> path, ref Translation translation,
            ref TankMoving tankMoving, in TankMovement tankMovement, in Rotation rotation) =>
        {
            //Buffered
            if (globalSimulationTick < tankMoving.startTick)
                return;

            //Return if rotating in place.
            if (tankMoving.rotateInPlace == 1)
                return;

            //Accelerate if not at top speed.
            if (tankMoving.currentMovementSpeed < tankMovement.movementSpeed)
            {
                //Only accelerate if not decelerating already.
                if (tankMoving.currentMovementSpeed + tankMovement.acceleration > tankMovement.movementSpeed)
                {
                    tankMoving.currentMovementSpeed = tankMovement.movementSpeed;
                }
                else
                {
                    tankMoving.currentMovementSpeed += tankMovement.acceleration;
                }
            }


            //Move position;
            if (tankMoving.reverse == 0)
            {
                translation.Value += math.forward(rotation.Value) * tankMoving.currentMovementSpeed;
            }
            else
            {
                translation.Value += -math.forward(rotation.Value) * tankMoving.currentMovementSpeed;
            }

            //Check if waypoint reached.
            float3 waypointPoint = path[tankMoving.pathIndex + 1].point;
            waypointPoint.y = 0;
            float3 translationValue = translation.Value;
            translationValue.y = 0;

            float distanceToNextWaypointSq = math.distancesq(waypointPoint, translationValue);

            if (distanceToNextWaypointSq < 25f)
            {
                //Path length greater than two, meaning a new waypoint needs to be sent if it hasn't already.
                if(path.Length > 2)
                {
                    if(tankMoving.sentNextWaypoint == 0)
                    {
                        //Make sure this isn't the end of the path.
                        if(tankMoving.pathIndex < path.Length - 2)
                        {
                            SendNextWaypointVehicle sendNextWaypointVehicle = new SendNextWaypointVehicle { value = path[tankMoving.pathIndex + 2].point };
                            commandBuffer.AddComponent(entityInQueryIndex, entity, sendNextWaypointVehicle);
                            tankMoving.sentNextWaypoint = 1;
                        }
                    }
                }

                //End of path.
                if(distanceToNextWaypointSq < 1f)
                {
                    //Remove TankMoving component.
                    if (tankMoving.pathIndex == path.Length - 2)
                    {
                        commandBuffer.RemoveComponent<TankMoving>(entityInQueryIndex, entity);
                    }
                    else
                    {
                        //Moving to next waypoint, increase path index and change facingWaypoint to 0 in order to calculate needed rotation.
                        tankMoving.pathIndex++;
                        tankMoving.facingWaypoint = 0;
                        tankMoving.checkAngleSize = 0;
                        if (tankMoving.reverse == 1)
                        {
                            tankMoving.reverse = 0;
                            tankMoving.currentMovementSpeed = 0;
                        }
                        tankMoving.sentNextWaypoint = 0;
                    }
                }
            }
        }).ScheduleParallel(intermediateDependencies);

        Dependency = moveJob;

        //Unity.Physics Utils
        RaycastInput CreateRaycastInput(float3 origin, float castDistance)
        {
            return new RaycastInput
            {
                Start = origin,
                End = origin + new float3(0f, -castDistance, 0),
                Filter = new CollisionFilter
                {
                    BelongsTo = (1u << 3),
                    CollidesWith = (1u << 3),
                    GroupIndex = 0
                }
            };
        }

        commandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}