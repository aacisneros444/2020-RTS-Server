using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

public static class PhysicsUtils
{
    public static bool Raycast(float3 origin, float3 direction, float distance, out Unity.Physics.RaycastHit raycastHit)
    {
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        RaycastInput raycastInput = new RaycastInput
        {
            Start = origin,
            End = origin + direction * distance,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            }
        };

        return (collisionWorld.CastRay(raycastInput, out raycastHit));
    }

    public static bool Raycast(float3 origin, float3 direction, float distance, CollisionFilter collisionFilter, out Unity.Physics.RaycastHit raycastHit)
    {
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

        RaycastInput raycastInput = new RaycastInput
        {
            Start = origin,
            End = origin + direction * distance,
            Filter = collisionFilter
        };

        return (collisionWorld.CastRay(raycastInput, out raycastHit));
    }

    public static bool RaycastJob(float3 origin, float3 direction, float distance, CollisionFilter collisionFilter, out Unity.Physics.RaycastHit raycastHit,
        CollisionWorld collisionWorld, BuildPhysicsWorld buildPhysicsWorld)
    {
        RaycastInput raycastInput = new RaycastInput
        {
            Start = origin,
            End = origin + direction * distance,
            Filter = collisionFilter
        };

        return (collisionWorld.CastRay(raycastInput, out raycastHit));
    }

    public static RaycastInput CreateRaycastInput(float3 origin, float3 direction, float distance)
    {
        return new RaycastInput
        {
            Start = origin,
            End = origin + math.normalize(direction) * distance,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            }
        };
    }

    public static RaycastInput CreateRaycastInputIgnoreOneLayer(float3 origin, float3 direction, float distance, int ignoreLayer)
    {
        return new RaycastInput
        {
            Start = origin,
            End = origin + math.normalize(direction) * distance,
            Filter = new CollisionFilter
            {
                BelongsTo = ~1u << ignoreLayer,
                CollidesWith = ~1u << ignoreLayer,
                GroupIndex = 0
            }
        };
    }
}
