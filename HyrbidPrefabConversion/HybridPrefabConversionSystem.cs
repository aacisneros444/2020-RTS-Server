using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class HybridPrefabConversionSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((UnityEngine.BoxCollider collider) =>
        {
            AddHybridComponent(collider);
        });

        //Entities.ForEach((UnityEngine.MeshCollider collider) =>
        //{
        //    AddHybridComponent(collider);
        //});
    }
}
