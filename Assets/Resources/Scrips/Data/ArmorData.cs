using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArmorDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string armorName;
    [Space(5f)]

    public float maxBulletproof;
    public int maxDurability;
}

[CreateAssetMenu(fileName = "ArmorData", menuName = "Scriptable Object/ArmorData")]
public class ArmorData : ScriptableObject
{
    public List<ArmorDataInfo> armorInfos = new List<ArmorDataInfo>();
}