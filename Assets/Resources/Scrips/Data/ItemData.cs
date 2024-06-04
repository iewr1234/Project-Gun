using System.Collections;
using System.Collections.Generic;
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
    Bullet = 20,
    Magazine,
    Muzzle,
    Sight,
    FrontHandle,
    Attachment,
    UnderBarrel,
}

public enum ItemRarity
{
    None,
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
    public string dataID;
    public string itemName;
    public ItemType type;
    public ItemRarity rarity;
    public float weight;
    public int maxNesting;
    public int price;
    public Vector2Int size;

    public ItemDataInfo()
    {
        indexName = string.Empty;
        ID = string.Empty;
        dataID = string.Empty;
        itemName = string.Empty;
        type = ItemType.None;
        rarity = ItemRarity.Nomal;
        weight = 0f;
        maxNesting = 0;
        price = 0;
        size = Vector2Int.zero;
    }

    public ItemDataInfo(ItemDataInfo itemData)
    {
        indexName = itemData.indexName;
        ID = itemData.ID;
        dataID = itemData.dataID;
        itemName = itemData.itemName;
        type = itemData.type;
        rarity = itemData.rarity;
        weight = itemData.weight;
        maxNesting = itemData.maxNesting;
        price = itemData.price;
        size = itemData.size;
    }
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Object/ItemData")]
public class ItemData : ScriptableObject
{
    public List<ItemDataInfo> itemInfos = new List<ItemDataInfo>();
}