using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Unity.Entities;
using Unity.Mathematics;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { get; private set; }
    public Seeker globalSeeker;
    public EntityManager entityManager;
    public GameObject debug;
    public List<GameObject> debugs = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public void OnPathComplete(Path path, Entity entity)
    {
        //for(int i = 0; i < debugs.Count; i++)
        //{
        //    Destroy(debugs[i]);
        //}

        //debugs.Clear();

        globalSeeker.PostProcess(path);
        if(path.vectorPath.Count > 0)
        {
            DynamicBuffer<Waypoint> waypoints = entityManager.GetBuffer<Waypoint>(entity);

            waypoints.Clear();

            for (int i = 0; i < path.vectorPath.Count; i++)
            {
                if (i == 0)
                {
                    //GameObject objDebug = Instantiate(debug, path.vectorPath[i], Quaternion.identity);
                    //debugs.Add(objDebug);
                    waypoints.Add(new Waypoint { point = path.vectorPath[i], arrivalTick = GlobalSimulationTick.value });
                }
                else
                {
                    //GameObject objDebug = Instantiate(debug, path.vectorPath[i], Quaternion.identity);
                    //debugs.Add(objDebug);
                    waypoints.Add(new Waypoint { point = path.vectorPath[i], arrivalTick = 0 });
                }
            }

            byte unitType = entityManager.GetComponentData<UnitType>(entity).value;

            if(unitType == 0)
            {
                entityManager.AddComponent<CalculatePathArrivalTicks>(entity);
            }
            if(unitType == 2)
            {
                TankMoving tankMoving = new TankMoving();
                tankMoving.startTick = GlobalSimulationTick.value;
                entityManager.AddComponentData(entity, tankMoving);

                ICommand command = new Command_SendFirstWaypointsVehicle(entity, path.vectorPath[1].x, path.vectorPath[1].y,
                    path.vectorPath[1].z, GlobalSimulationTick.value);

                CommandProcessor.AddCommand(command, 0);
            }
        }
    }

    public void StartPath(float3 unitPosition, float3 clickPoint, Entity entity)
    {
        ABPath p = ABPath.Construct(unitPosition, clickPoint,
            (Path p0) => Instance.OnPathComplete(p0, entity));

        AstarPath.StartPath(p);

        entityManager.AddComponent<PathQueued>(entity);
    }

}
