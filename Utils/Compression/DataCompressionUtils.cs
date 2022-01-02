using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;

public static class DataCompressionUtils
{

    //Compresses rotation by using "smallest three" compression and writes to given writer.
    public static void CompressAndWriteRotation(DarkRiftWriter writer, Quaternion rotation)
    {
        byte highestComponentIndex = 0; // x = 0, y = 1, z = 2, w = 3
        float highestValue = 0f;
        float sign = 1f;

        for (int i = 0; i < 4; i++)
        {
            float abs = Mathf.Abs(rotation[i]);

            if (abs > highestValue)
            {
                highestValue = abs;

                highestComponentIndex = (byte)i;

                if (rotation[i] < 0)
                {
                    sign = -1f;
                }
                else
                {
                    sign = 1f;
                }
            }
        }


        short a = 0;
        short b = 0;
        short c = 0;

        if (highestComponentIndex == 0)
        {
            a = (short)(rotation.y * sign * 32767f);
            b = (short)(rotation.z * sign * 32767f);
            c = (short)(rotation.w * sign * 32767f);
        }

        if (highestComponentIndex == 1)
        {
            a = (short)(rotation.x * sign * 32767f);
            b = (short)(rotation.z * sign * 32767f);
            c = (short)(rotation.w * sign * 32767f);
        }

        if (highestComponentIndex == 2)
        {
            a = (short)(rotation.x * sign * 32767f);
            b = (short)(rotation.y * sign * 32767f);
            c = (short)(rotation.w * sign * 32767f);
        }

        if (highestComponentIndex == 3)
        {
            a = (short)(rotation.x * sign * 32767f);
            b = (short)(rotation.y * sign * 32767f);
            c = (short)(rotation.z * sign * 32767f);
        }

        writer.Write(highestComponentIndex);
        writer.Write(a);
        writer.Write(b);
        writer.Write(c);

    }


}
