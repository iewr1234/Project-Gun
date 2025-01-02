using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Armor
{
    [Tooltip("방어구 이름")] public string armorName;
    [Space(5f)]

    [Tooltip("최대 방탄력")] public float maxBulletProof;
    [Tooltip("방탄력")] public float bulletProof;
    [Tooltip("최대 내구도")] public int maxDurability;
    [Tooltip("내구도")] public int durability;

    public Armor(ArmorDataInfo armorData)
    {
        armorName = armorData.armorName;

        maxBulletProof = armorData.maxBulletProof;
        bulletProof = maxBulletProof;
        maxDurability = armorData.maxDurability;
        durability = maxDurability;
    }
}