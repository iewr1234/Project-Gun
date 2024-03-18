using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataUtility
{
    public static float GetDistance(Vector3 posA, Vector3 posB)
    {
        var distance = Mathf.Sqrt((posA - posB).sqrMagnitude);
        return Mathf.Round(distance * 100) / 100;
    }
}