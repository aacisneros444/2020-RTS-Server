//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Transforms;
//using Unity.Mathematics;

//[UpdateAfter(typeof(BuildersTaskDecisionSystem))]
//public class DeliverResourceToInventorySystem : ComponentSystem
//{
//    protected override void OnUpdate()
//    {
//        Entities.WithNone<PathQueued, Moving>().ForEach((Entity entity, DynamicBuffer<Resource> inventory, ref NetworkID networkID,
//            ref DeliverResourceToInventory deliverResourceToInventory, ref Translation translation, ref BuildTasked buildTasked) =>
//        {
//            int resourceIndex = deliverResourceToInventory.resourceType;

//            if(inventory[resourceIndex].amount >= deliverResourceToInventory.amount)
//            {
//                float3 inventoryEntityPosition = EntityManager.GetComponentData<Translation>(deliverResourceToInventory.entityToDeliverTo).Value;

//                if (math.distance(EntityManager.GetComponentObject<BoxCollider>(buildTasked.assignedBuildSite).ClosestPoint(translation.Value), translation.Value) < 10f)
//                {
//                    DynamicBuffer<Resource> deliveryInventory = EntityManager.GetBuffer<Resource>(deliverResourceToInventory.entityToDeliverTo);

//                    Resource resource0 = deliveryInventory[resourceIndex];
//                    if (resource0.amount < resource0.maxAmount)
//                    {
//                        resource0.amount += deliverResourceToInventory.amount;
//                        deliveryInventory[resourceIndex] = resource0;

//                        Resource resource1 = inventory[resourceIndex];
//                        resource1.amount -= deliverResourceToInventory.amount;
//                        inventory[resourceIndex] = resource1;
//                    }
//                    EntityManager.RemoveComponent<DeliverResourceToInventory>(entity);
//                    return;
//                }
//                else
//                {
//                    //float3 dir = math.normalize(inventoryEntityPosition - translation.Value) * 0.5f;
//                    //float3 endPos = inventoryEntityPosition - dir;
//                    float3 endPos = EntityManager.GetComponentObject<BoxCollider>(buildTasked.assignedBuildSite).ClosestPoint(translation.Value);
//                    List<ushort> entityID = new List<ushort>();
//                    entityID.Add(networkID.value);

//                    ICommand command = new Command_MoveOrder(endPos.x, endPos.y, endPos.z, entityID);
//                    CommandProcessor.AddCommand(command, 0);
//                }
//            }

//        });
//    }
//}
