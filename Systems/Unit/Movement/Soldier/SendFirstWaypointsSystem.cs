using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[UpdateAfter(typeof(MoveSystem))]
public class SendFirstWaypointsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<SendFirstWaypoints>().ForEach((Entity entity, ref NetworkID networkID, DynamicBuffer<Waypoint> waypoints) =>
        {
            if (waypoints.Length == 0)
                return;

            ICommand command = new Command_SendFirstWaypoints(networkID.value, waypoints[0].point.x, waypoints[0].point.y,
                waypoints[0].point.z, waypoints[0].arrivalTick, waypoints[1].point.x, waypoints[1].point.y, waypoints[1].point.z,
                waypoints[1].arrivalTick, (ushort)(waypoints.Length - 1));

            CommandProcessor.AddCommand(command, 0);

            EntityManager.RemoveComponent<SendFirstWaypoints>(entity);
        });
    }
}
