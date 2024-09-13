using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class V360_Utilities
{
    /*************************************************************************
    * LLAToXYZ
    * 
    * Accepts a latitude and longitude (in degrees) and returns a normalized
    * vector toward the correct direction of the location. Multiply the result
    * by the size of the globe's radius to get a position on the surface.
    * 
    ***********************************************************************/
    static public Vector3 LLAToXYZ(float latitude, float longitude)
    {
        float fInv = 298.257224f;
        float f = 1.0f / fInv;

        float cosLat = Mathf.Cos(latitude * Mathf.PI / 180.0f);
        float sinLat = Mathf.Sin(latitude * Mathf.PI / 180.0f);

        float cosLong = Mathf.Cos(longitude * Mathf.PI / 180.0f);
        float sinLong = Mathf.Sin(longitude * Mathf.PI / 180.0f);

        float c = 1 / Mathf.Sqrt(cosLat * cosLat + (1 - f) * (1 - f) * sinLat * sinLat);
        float s = (1 - f) * (1 - f) * c;

        float x = c * cosLat * cosLong;
        float z = c * cosLat * sinLong;
        float y = s * sinLat;

        return new Vector3(x, y, z);
    }
}