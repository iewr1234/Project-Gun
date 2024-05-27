using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum ItemType
{
    None,
    Head,
    Body,
    Rig,
    Backpack,
    MainWeapon = 10,
    SubWeapon,
    Magazine = 30,
    Grenade,
}

public enum ItemRarity
{
    Nomal,
    Rare,
    Unique,
}

[System.Serializable]
public class ItemDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string itemName;
    public ItemType type;
    public ItemRarity rarity;
    public float weight;
    public int maxNesting;
    public int price;
    public Vector2Int size;
    [HideInInspector] public int index;
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Object/ItemData")]
public class ItemData : ScriptableObject
{
    public List<ItemDataInfo> itemInfos = new List<ItemDataInfo>();
}