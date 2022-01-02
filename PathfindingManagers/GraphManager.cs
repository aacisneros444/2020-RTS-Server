using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GraphManager : MonoBehaviour
{
    public static GraphManager Instance { get; private set; }

    public Dictionary<int, GameObject> graphObstacles = new Dictionary<int, GameObject>();
    private int uniqueGraphObstacleIDCounter;
    public Transform graphObstacleParent;
    public GameObject obstaclePrefab;

    private void Awake()
    {
        Instance = this;
    }

    public int GetUniqueGraphObstacleID()
    {
        int ID = uniqueGraphObstacleIDCounter;
        uniqueGraphObstacleIDCounter++;
        return ID;
    }

    public int RegisterGraphObstacle(NavmeshCut navmeshCut0, Vector3 position, Quaternion rotation)
    {
        int ID = GetUniqueGraphObstacleID();
        GameObject graphObstacle =  Instantiate(obstaclePrefab, position, rotation);
        graphObstacle.transform.parent = graphObstacleParent;
        NavmeshCut navmeshCut1 = graphObstacle.AddComponent<NavmeshCut>();
        navmeshCut1.type = navmeshCut0.type;
        navmeshCut1.center = navmeshCut0.center;
        navmeshCut1.rectangleSize = navmeshCut0.rectangleSize;
        navmeshCut1.height = navmeshCut0.height;
        navmeshCut1.useRotationAndScale = true;

        graphObstacles.Add(ID, graphObstacle);

        AstarPath.active.navmeshUpdates.ForceUpdate();

        return ID;
    }

    public void DestroyGraphObstacle(int ID)
    {
        if (graphObstacles.ContainsKey(ID))
        {
            Destroy(graphObstacles[ID].gameObject);
            graphObstacles.Remove(ID);
        }
    }
}
