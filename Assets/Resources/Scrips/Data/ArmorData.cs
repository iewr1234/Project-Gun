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
    public string prefabName;

    [Space(5f)][Tooltip("무게")] public int weight;
    [Tooltip("최대 방탄력")] public float maxBulletProof;
    [Tooltip("방탄력")] public float bulletProof;
    [Tooltip("최대 내구도")] public int maxDurability;
    [Tooltip("내구도")] public int durability;

    public ArmorDataInfo CopyData()
    {
        ArmorDataInfo armorData = new ArmorDataInfo()
        {
            indexName = indexName,

            ID = ID,
            armorName = armorName,
            prefabName = prefabName,

            weight = weight,
            maxBulletProof = maxBulletProof,
            bulletProof = maxBulletProof,
            maxDurability = maxDurability,
            durability = maxDurability
        };

        return armorData;
    }
}

[CreateAssetMenu(fileName = "ArmorData", menuName = "Scriptable Object/ArmorData")]
public class ArmorData : ScriptableObject
{
    public List<ArmorDataInfo> armorInfos = new List<ArmorDataInfo>();
}