using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ClearOldSendEntityStateDataComponentsSystem : ComponentSystem
{
    //In case an entity still has the SendEntityStateData component, we remove it,
    //so it does not comflict with the sent data of the currently selected entity

    public static ClearOldSendEntityStateDataComponentsSystem Instance { get; private set; }

    protected override void OnCreate()
    {
        Instance = this;
    }

    protected override void OnUpdate()
    {

    }

    public void Run(ushort clientID)
    {
        Entities.ForEach((Entity entity, ref SendEntityStateData sendEntityStateData) =>
        {
            if (sendEntityStateData.clientID == clientID)
            {
                EntityManager.RemoveComponent<SendEntityStateData>(entity);
            }
        });
    }


}
