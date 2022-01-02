using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class Command_BuildRequest : ICommand
{
    public ushort prefabID;
    public ushort teamID;
    public float posX;
    public float posY;
    public float posZ;
    public byte highestComponentIndex;
    public short a;
    public short b;
    public short c;

    public Command_BuildRequest(ushort prefabID, ushort teamID, float posX, float posY, float posZ,  byte highestComponentIndex, short a, short b, short c)
    {
        this.prefabID = prefabID;
        this.teamID = teamID;
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.highestComponentIndex = highestComponentIndex;
        this.a = a;
        this.b = b;
        this.c = c;
    }

    public void Execute()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Entity entityPrefab = EntityPrefabLookup.GetEntityPrefab(prefabID);

        Entity entity = entityManager.Instantiate(entityPrefab);

        entityManager.SetComponentData(entity, new Translation { Value = new float3(posX, posY, posZ) });
        entityManager.SetComponentData(entity, new Rotation { Value = DataDecompressionUtils.DecompressRotation(highestComponentIndex, a, b, c) });
        entityManager.SetComponentData(entity, new TeamID { value = teamID });
    }
}
