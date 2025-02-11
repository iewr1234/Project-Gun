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
    SubWeapon =10,
    MainWeapon,
    Bullet = 20,
    Magazine,
    Muzzle,
    Sight,
    Attachment,
    UnderBarrel,
    Grenade = 30,
}

public enum ItemRarity
{
    None,
    LowGrade,
    Nomal,
    MiddleGrade,
    HighGrade,
    Advanced,
    Set,
    Unique,
}

[System.Serializable]
public struct ItemOption
{
    public string indexName;
    public ItemOptionType type;
    public int value;
    public string scriptText;
}

[System.Serializable]
public class ItemDataInfo
{
    public string indexName;
    [Space(5f)]

    public string serialID;
    public string ID;
    public string dataID;
    public string itemName;
    public ItemType type;
    public int level;
    public ItemRarity rarity;
    public int maxNesting;
    public int price;
    public Vector2Int size;
    [Space(5f)]

    public bool addOption;
    public bool setDropTable;
    public List<ItemOption> itemOptions = new List<ItemOption>();

    public ItemDataInfo CopyData()
    {
        var itemData = new ItemDataInfo
        {
            indexName = indexName,
            serialID = System.Guid.NewGuid().ToString(),
            ID = ID,
            dataID = dataID,
            itemName = itemName,
            type = type,
            level = level,
            rarity = rarity,
            maxNesting = maxNesting,
            price = price,
            size = size,
            addOption = addOption,
            itemOptions = new List<ItemOption>(itemOptions),
        };

        return itemData;
    }
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Object/ItemData")]
public class ItemData : ScriptableObject
{
    public List<ItemDataInfo> itemInfos = new List<ItemDataInfo>();
}