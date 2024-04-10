using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Armor
{
    [Tooltip("방어구 이름")] public string armorName;
    [Space(5f)]

    [Tooltip("방탄력")] public float bulletproof;
    [Tooltip("최대 내구도")] public int maxDurability;
    [Tooltip("내구도")] public int durability;

    public Armor(ArmorDataInfo armorData)
    {
        armorName = armorData.armorName;

        bulletproof = armorData.bulletproof;
        maxDurability = armorData.maxDurability;
        durability = maxDurability;
    }
}
