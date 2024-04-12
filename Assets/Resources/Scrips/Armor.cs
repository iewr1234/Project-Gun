using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Armor
{
    [Tooltip("�� �̸�")] public string armorName;
    [Space(5f)]

    [Tooltip("�ִ� ��ź��")] public float maxBulletproof;
    [Tooltip("��ź��")] public float bulletproof;
    [Tooltip("�ִ� ������")] public int maxDurability;
    [Tooltip("������")] public int durability;

    public Armor(ArmorDataInfo armorData)
    {
        armorName = armorData.armorName;

        maxBulletproof = armorData.maxBulletproof;
        bulletproof = maxBulletproof;
        maxDurability = armorData.maxDurability;
        durability = maxDurability;
    }
}