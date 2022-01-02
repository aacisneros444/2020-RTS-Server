using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EntityPrefabLookup : MonoBehaviour
{
    private BlobAssetStore blobAssetStore;
    public GameObject entityPrefabHolderGameObject;
    public static Entity entityPrefabHolderEntity;
    public static EntityPrefabHolder entityPrefabHolder;

    public void Awake()
    {
        blobAssetStore = new BlobAssetStore();
        GameObjectConversionSettings gameObjectConversionSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        entityPrefabHolderEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(entityPrefabHolderGameObject, gameObjectConversionSettings);

        entityPrefabHolder = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<EntityPrefabHolder>(entityPrefabHolderEntity);
    }

    private void OnDestroy()
    {
        blobAssetStore.Dispose();
    }

    public static Entity GetEntityPrefab(int ID)
    {
        if (ID == 0)
            return entityPrefabHolder.DropPod;

        if (ID == 1)
            return entityPrefabHolder.SoldierStart;


        if (ID >= 2000)
        {
            if (ID == 2000)
                return entityPrefabHolder.Soldier;

            if (ID == 2001)
                return entityPrefabHolder.Soldier_Kar98k;

            if (ID == 2002)
                return entityPrefabHolder.Soldier_M1919Browning;
        }

        if(ID >= 3000)
        {
            if (ID == 3000)
                return entityPrefabHolder.Tiger;
        }

        if (ID >= 5000)
        {
            if (ID == 5000)
                return entityPrefabHolder.StorageCrateBuildPlan;

            if (ID == 5001)
                return entityPrefabHolder.StorageCrateBuildSite;

            if (ID == 5002)
                return entityPrefabHolder.StorageCrate;

            if (ID == 5003)
                return entityPrefabHolder.BarracksBuildPlan;

            if (ID == 5004)
                return entityPrefabHolder.BarracksBuildSite;

            if (ID == 5005)
                return entityPrefabHolder.Barracks;

            if (ID == 5006)
                return entityPrefabHolder.MaterialPlantBuildPlan;

            if (ID == 5007)
                return entityPrefabHolder.MaterialPlantBuildSite;

            if (ID == 5008)
                return entityPrefabHolder.MaterialPlant;
        }

        return Entity.Null;
    }
}
