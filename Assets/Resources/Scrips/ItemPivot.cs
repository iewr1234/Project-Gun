using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPivot : MonoBehaviour
{
    [System.Serializable]
    public struct PivotInfo
    {
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scale;
    }

    public PivotInfo itemPivot;
}
