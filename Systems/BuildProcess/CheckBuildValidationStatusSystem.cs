using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

public class CheckBuildValidationStatusSystem : ComponentSystem
{
    private List<NewBuildSiteData> newBuildSiteData;

    protected override void OnCreate()
    {
        newBuildSiteData = new List<NewBuildSiteData>();
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref ValidateBuild validateBuild, ref BuildSitePrefabID buildSitePrefabID , ref Translation translation,
            ref Rotation rotation, ref TeamID teamID, ref LocalToWorld localToWorld) =>
        {
            validateBuild.framesUnprocessed++;

            if (validateBuild.processed == 1)
            {
                if (validateBuild.canBuild == 0)
                {
                    //Debug.Log("Collision detected. Invalid");
                }

                if (validateBuild.canBuild == 1)
                {
                    //Debug.Log("No collision detected. Valid");

                    CollisionFilter collisionFilter = new CollisionFilter
                    {
                        BelongsTo = ~(1u << 1),
                        CollidesWith = ~(1u << 1),
                        GroupIndex = 0
                    };

                    Unity.Physics.RaycastHit raycastHit;
                    if (PhysicsUtils.Raycast(new float3(translation.Value.x, translation.Value.y + 0.5f, translation.Value.z), -localToWorld.Up, 5f, collisionFilter, out raycastHit))
                    {
                        if (EntityManager.HasComponent<BuildableSurface>(raycastHit.Entity))
                        {
                            newBuildSiteData.Add(new NewBuildSiteData
                            {
                                prefabID = buildSitePrefabID.value,
                                teamID = teamID.value,
                                position = translation.Value,
                                rotation = rotation.Value
                            });
                        }
                        //else
                        //{
                        //    Debug.Log("No surface buildable underneath. Invalid.");
                        //    Debug.Log(raycastHit.Entity);
                        //    Debug.Log(EntityManager.GetComponentData<PrefabID>(entity).value);
                        //}
                    }
                }

                EntityManager.DestroyEntity(entity);
            }
            else
            {
                if(validateBuild.framesUnprocessed >= 7)
                {
                    //Debug.Log("No processing. Valid.");

                    CollisionFilter collisionFilter = new CollisionFilter
                    {
                        BelongsTo = ~(1u << 1),
                        CollidesWith = ~(1u << 1),
                        GroupIndex = 0
                    };

                    Unity.Physics.RaycastHit raycastHit;
                    if (PhysicsUtils.Raycast(new float3(translation.Value.x, translation.Value.y + 0.5f, translation.Value.z), -localToWorld.Up, 5f, collisionFilter, out raycastHit))
                    {
                        if (EntityManager.HasComponent<BuildableSurface>(raycastHit.Entity))
                        {
                            newBuildSiteData.Add(new NewBuildSiteData
                            {
                                prefabID = buildSitePrefabID.value,
                                teamID = teamID.value,
                                position = translation.Value,
                                rotation = rotation.Value
                            });
                        }
                    }
                    //else
                    //{
                    //    Debug.Log("No surface buildable underneath. Invalid.");
                    //}

                    EntityManager.DestroyEntity(entity);
                }
            }
        });

        if(newBuildSiteData.Count > 0)
        {
            for(int i = 0; i < newBuildSiteData.Count; i++)
            {
                ICommand command = new Command_CreateUnitEntityWithPositionRotation(newBuildSiteData[i].prefabID, newBuildSiteData[i].teamID,
                    newBuildSiteData[i].position.x, newBuildSiteData[i].position.y, newBuildSiteData[i].position.z,
                    newBuildSiteData[i].rotation.value.x, newBuildSiteData[i].rotation.value.y, newBuildSiteData[i].rotation.value.z,
                    newBuildSiteData[i].rotation.value.w);

                CommandProcessor.AddCommand(command, 0);
            }

            newBuildSiteData.Clear();
        }
    }
}

public struct NewBuildSiteData
{
    public ushort prefabID;
    public ushort teamID;
    public float3 position;
    public quaternion rotation;

    public NewBuildSiteData(ushort prefabID, ushort teamID, float3 position, quaternion rotation)
    {
        this.prefabID = prefabID;
        this.teamID = teamID;
        this.position = position;
        this.rotation = rotation;
    }
}
