using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics;

public class MonoDrawRay : MonoBehaviour
{
    public static List<RaycastInput> inputsToDraw = new List<RaycastInput>();


    private void Update()
    {
        if(inputsToDraw.Count > 0)
        {
            for(int i = 0; i < inputsToDraw.Count; i++)
            {
                Debug.DrawLine(inputsToDraw[i].Start, inputsToDraw[i].End, Color.red, 2f);
            }

            inputsToDraw.Clear();
        }
    }
}
