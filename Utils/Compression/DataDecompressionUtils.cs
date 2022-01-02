using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataDecompressionUtils
{
    public static Quaternion DecompressRotation(byte highestComponentIndex, short a, short b, short c)
    {
        float aNew = a / 32767f;
        float bNew = b / 32767f;
        float cNew = c / 32767f;

        float d = Mathf.Sqrt(1f - (aNew * aNew + bNew * bNew + cNew * cNew));

        if (highestComponentIndex == 0)
        {
            return new Quaternion(d, aNew, bNew, cNew);
        }

        if (highestComponentIndex == 1)
        {
            return new Quaternion(aNew, d, bNew, cNew);
        }

        if (highestComponentIndex == 2)
        {
            return new Quaternion(aNew, bNew, d, cNew);
        }

        return new Quaternion(aNew, bNew, cNew, d);
    }
}
