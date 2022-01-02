using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class ProduceResource : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((Entity entity, ref Inventory inventory, ref ResourcePlant resourcePlant) =>
        {
            if(resourcePlant.laborForce > 0 && resourcePlant.linkedNodes > 0)
            {
                if(inventory.resource < inventory.maxResource)
                {
                    resourcePlant.percentageUntilIncrease += math.pow(resourcePlant.laborForce, 0.88f) * math.pow(resourcePlant.linkedNodes / 10f, 0.88f) * deltaTime;
                    if (resourcePlant.percentageUntilIncrease >= 1f)
                    {
                        inventory.resource += 1;
                        resourcePlant.percentageUntilIncrease = 0;
                    }
                }
            }

        }).ScheduleParallel();
    }
}
