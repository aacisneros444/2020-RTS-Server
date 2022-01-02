using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DarkRift;
using Unity.Mathematics;

[UpdateAfter(typeof(MoveSystem))]
public class SendNextWaypointSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        Entities.WithAll<SendNextWaypoint>().ForEach((Entity entity, DynamicBuffer<Waypoint> waypoints, ref PathIndex pathIndex, ref NetworkID networkID) =>
        {
            if (waypoints.Length == 0)
                return;

            ushort nextPathIndex = (ushort)(pathIndex.value + 1);

            if (nextPathIndex > waypoints.Length - 1)
                return;


            ICommand command = new Command_SendWaypoint(networkID.value, waypoints[nextPathIndex].arrivalTick,
                waypoints[nextPathIndex].point.x, waypoints[nextPathIndex].point.y, waypoints[nextPathIndex].point.z);

            CommandProcessor.AddCommand(command, 0);

            EntityManager.RemoveComponent<SendNextWaypoint>(entity);
        });
    }
}
