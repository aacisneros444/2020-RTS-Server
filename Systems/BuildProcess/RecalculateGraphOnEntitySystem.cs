using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Pathfinding;

public class RecalculateGraphOnEntitySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref RecalculateGraphOnEntity recalculateGraphOnEntity) =>
        {
            Bounds bounds0 = EntityManager.GetComponentObject<BoxCollider>(entity).bounds;
            Bounds bounds1 = new Bounds(recalculateGraphOnEntity.colliderCenter, recalculateGraphOnEntity.colliderSize * 1f);
            AstarPath.active.UpdateGraphs(bounds1);

            EntityManager.RemoveComponent<RecalculateGraphOnEntity>(entity);
        });
    }

}
