using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataUtility
{
    public static readonly float nodeSize = 1.2f;
    public static readonly float nodeInterval = 0.1f;

    public static float GetDistance(Vector3 posA, Vector3 posB)
    {
        var distance = Mathf.Sqrt((posA - posB).sqrMagnitude);
        return Mathf.Round(distance * 100) / 100;
    }
}