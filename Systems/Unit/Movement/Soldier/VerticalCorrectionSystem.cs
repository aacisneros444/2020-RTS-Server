using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;


[UpdateAfter(typeof(MoveSystem))]
public class VerticalCorrectionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<Moving>().ForEach((Entity entity, ref Translation translation, ref VerticalCorrectionRaycastData verticalCorrectionRaycastData) =>
        {
            Vector3 castOrigin = new Vector3(translation.Value.x, translation.Value.y + verticalCorrectionRaycastData.raycastYOrigin, translation.Value.z);

            UnityEngine.RaycastHit raycastHit;
            if (Physics.Raycast(castOrigin, Vector3.down, out raycastHit, verticalCorrectionRaycastData.raycastDistance))
            {
                translation.Value = raycastHit.point;
            }
        });
    }
}
