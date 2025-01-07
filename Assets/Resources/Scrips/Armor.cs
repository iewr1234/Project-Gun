using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Armor
{
    public enum Type
    {
        Head,
        Body,
    }

    [Header("---Access Script---")]
    [HideInInspector] public CharacterController charCtr;

    [Header("--- Assignment Variable---")]
    [HideInInspector] public EquipSlot equipSlot;
    public ArmorDataInfo armorData;
    public Type type;

    public void SetComponets(CharacterController _charCtr, ItemHandler _item)
    {
        charCtr = _charCtr;
        equipSlot = _item.equipSlot;
        armorData = equipSlot.item.armorData;
        type = equipSlot.item.itemData.type == ItemType.Head ? Type.Head : Type.Body;
    }

    public void SetComponets(CharacterController _charCtr, Type _type, float maxBulletProof, int maxDurability)
    {
        charCtr = _charCtr;
        armorData = new ArmorDataInfo()
        {
            maxBulletProof = maxBulletProof,
            bulletProof = maxBulletProof,
            maxDurability = maxDurability,
            durability = maxDurability,
        };
        type = _type;
    }
}