using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class Command_ProcessResourceTransferRequest : ICommand
{
    public ushort unitNetworkID;
    public int amount;
    public List<ushort> unitNetworkIDsToTransferTo;

    public Command_ProcessResourceTransferRequest(ushort unitNetworkID, int amount, List<ushort> unitNetworkIDsToTransferTo)
    {
        this.unitNetworkID = unitNetworkID;
        this.amount = amount;
        this.unitNetworkIDsToTransferTo = unitNetworkIDsToTransferTo;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (!NetworkEntityManager.networkEntities.ContainsKey(unitNetworkID))
            return;

        Entity entity = NetworkEntityManager.networkEntities[unitNetworkID];
        float3 entityPosition = entityManager.GetComponentData<Translation>(entity).Value;
        float maxTransferRange = entityManager.GetComponentData<ResourceTransferRange>(entity).value;
        Inventory eInventory = entityManager.GetComponentData<Inventory>(entity);

        //Initial existence and range check of each entity to transfer to.
        for (int i = 0; i < unitNetworkIDsToTransferTo.Count; i++)
        {
            if (!NetworkEntityManager.networkEntities.ContainsKey(unitNetworkIDsToTransferTo[i]))
            {
                unitNetworkIDsToTransferTo.RemoveAt(i);
            }
            else
            {
                Entity unitToTransferTo = NetworkEntityManager.networkEntities[unitNetworkIDsToTransferTo[i]];

                if (math.distance(entityManager.GetComponentData<Translation>(unitToTransferTo).Value, entityPosition) > maxTransferRange)
                {
                    unitNetworkIDsToTransferTo.RemoveAt(i);
                }
            }
        }

        //Return if no units are are in existence or are in range.
        if (unitNetworkIDsToTransferTo.Count == 0)
            return;

        //Sort by capacity remaining (from least to most).
        unitNetworkIDsToTransferTo.Sort(SortByCapacityRemaining);

        //Does the amount requested to be transferred exceed currently held resource?
        if(amount > eInventory.resource)
        {
            //If so, transfer all we can.
            amount = eInventory.resource;
        }

        int amountDivided = amount / unitNetworkIDsToTransferTo.Count;
        int remainder = amount % unitNetworkIDsToTransferTo.Count;

        bool recalculateDistribution = false;
        int unitsMaxedCapacity = 0;
        int amountTransferred = 0;
        for(int i = 0; i < unitNetworkIDsToTransferTo.Count; i++)
        {
            Entity unitToTransferTo = NetworkEntityManager.networkEntities[unitNetworkIDsToTransferTo[i]];

            if (recalculateDistribution)
            {
                amountDivided = (amount - amountTransferred) / (unitNetworkIDsToTransferTo.Count - unitsMaxedCapacity);
                remainder = (amount - amountTransferred) % (unitNetworkIDsToTransferTo.Count - unitsMaxedCapacity);

                recalculateDistribution = false;
            }

            Inventory inventory = entityManager.GetComponentData<Inventory>(unitToTransferTo);

            if (i == unitNetworkIDsToTransferTo.Count - 1)
            {
                if (remainder > 0)
                {
                    int amountPlusRemainder = remainder + amountDivided;
                    if (inventory.resource + amountPlusRemainder > inventory.maxResource)
                    {
                        int difference = inventory.maxResource - inventory.resource;
                        inventory.resource += amountPlusRemainder;
                        amountTransferred += amountPlusRemainder;

                        recalculateDistribution = true;
                        unitsMaxedCapacity++;
                    }
                    else
                    {
                        inventory.resource += amountPlusRemainder;
                        amountTransferred += amountPlusRemainder;
                    }
                }
                else
                {
                    if (inventory.resource + amountDivided > inventory.maxResource)
                    {
                        int difference = inventory.maxResource - inventory.resource;
                        inventory.resource = inventory.maxResource;
                        amountTransferred += difference;

                        recalculateDistribution = true;
                        unitsMaxedCapacity++;
                    }
                    else
                    {
                        inventory.resource += amountDivided;
                        amountTransferred += amountDivided;
                    }
                }
            }
            else
            {
                if (inventory.resource + amountDivided > inventory.maxResource)
                {
                    int difference = inventory.maxResource - inventory.resource;
                    inventory.resource = inventory.maxResource;
                    amountTransferred += difference;

                    recalculateDistribution = true;
                    unitsMaxedCapacity++;
                }
                else
                {
                    inventory.resource += amountDivided;
                    amountTransferred += amountDivided;
                }
            }
            entityManager.SetComponentData(unitToTransferTo, inventory);
        }

        if(amountTransferred > 0)
        {
            eInventory.resource -= amountTransferred;
            entityManager.SetComponentData(entity, eInventory);
        }
    }

    private int SortByCapacityRemaining(ushort networkID0, ushort networkID1)
    {
        Entity entity0 = NetworkEntityManager.networkEntities[networkID0];
        Entity entity1 = NetworkEntityManager.networkEntities[networkID1];

        Inventory inventory0 = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Inventory>(entity0);
        Inventory inventory1 = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Inventory>(entity1);

        int delta0 = inventory0.maxResource - inventory0.resource;
        int delta1 = inventory1.maxResource - inventory1.resource;

        return delta0.CompareTo(delta1);
    }
}
