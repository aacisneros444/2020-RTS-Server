using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Pathfinding;
using Unity.Transforms;

public class CutGraphOnEntitySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref GraphObstacle graphObstacle , ref Translation translation, ref Rotation rotation) =>
        {
            NavmeshCut navmeshCut = new NavmeshCut
            {
                type = NavmeshCut.MeshType.Rectangle,
                center = graphObstacle.center,
                rectangleSize = new Vector2(graphObstacle.size.x, graphObstacle.size.z),
                height = graphObstacle.size.y
            };

            int companionGameObjectID = GraphManager.Instance.RegisterGraphObstacle(navmeshCut, translation.Value, rotation.Value);

            EntityManager.AddComponent<CompanionGraphCutGameObjectID>(entity);
            EntityManager.SetComponentData(entity, new CompanionGraphCutGameObjectID { value = companionGameObjectID });

            EntityManager.RemoveComponent<GraphObstacle>(entity);
        });
    }
}
