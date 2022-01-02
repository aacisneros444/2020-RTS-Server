using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;


/// <summary>
/// This system places all entities with the QuadrantEntity tag in quadrants based on X and Z world coordinates. This data is stored in the
/// entityQuadrantMultiHashmap which is a static multiHashmap from the QuadrantMultiHashmaps class. By doing this, other systems can search for
/// entities in nearby quadrants only, which is very helpful for performance.
/// </summary>
public class PlaceEntitiesInQuadrantsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityQuery entityQuery = GetEntityQuery(typeof(QuadrantEntity));

        QuadrantMultiHashmaps.entityQuadrantMultiHashmap.Clear();
        if (entityQuery.CalculateEntityCount() > QuadrantMultiHashmaps.entityQuadrantMultiHashmap.Capacity)
        {
            QuadrantMultiHashmaps.entityQuadrantMultiHashmap.Capacity = entityQuery.CalculateEntityCount();
        }

        NativeMultiHashMap<int, BucketedEntityData>.ParallelWriter entityQuadrantMultiHashmap = QuadrantMultiHashmaps.entityQuadrantMultiHashmap.AsParallelWriter();

        Entities.WithAll<QuadrantEntity>().ForEach((Entity entity, int entityInQueryIndex, in Translation translation,
            in TeamID teamID, in UnitType unitType) =>
        {
            int hashmapKey = QuadrantMultiHashmaps.GetPositionHashMapKey(translation.Value);
            entityQuadrantMultiHashmap.Add(hashmapKey, new BucketedEntityData
            {
                entity = entity,
                position = translation.Value,
                teamID = teamID.value,
                unitType = unitType.value
            });

        }).ScheduleParallel(Dependency).Complete();

        //If there was no update due to an empty query with 0 entities, dead entities would not be cleared.
        KeepComponentSystemRunningInCaseOfEmptyQuery();
    }

    private void KeepComponentSystemRunningInCaseOfEmptyQuery()
    {
        Entities.ForEach((Entity entity, ref KeepComponentSystemRun keepComponentSystemRun) =>
        {

        }).WithoutBurst().Run();
    }
}
