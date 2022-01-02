using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class DestroyEntitiesSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;
    public static List<Entity> entitiesToDestroy;
    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        entitiesToDestroy = new List<Entity>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer commandBuffer = commandBufferSystem.CreateCommandBuffer();
        
        for(int i = 0; i < entitiesToDestroy.Count; i++)
        {
            commandBuffer.DestroyEntity(entitiesToDestroy[i]);
            entitiesToDestroy.RemoveAt(i);
        }
    }
}
