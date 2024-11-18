using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CreatePos
{
    None,
    Equip,
    Pocket,
    Rig,
    Backpack,
}

[System.Serializable]
public struct StartingItem_Inventory
{
    public string ID;
    public int num;
    public CreatePos createPos;
}

[System.Serializable]
public struct StartingItem_Storage
{
    public string storageName;
    public string ID;
    public int num;
}

[System.Serializable]
public class StorageItemInfo
{
    public string indexName;
    public Vector2Int slotIndex;
    public Vector2Int itemSize;
    public int totalCount;
    public bool rotation;
    [Space(5f)]

    public ItemDataInfo itemData;
    [HideInInspector] public RigDataInfo rigData;
    [HideInInspector] public BackpackDataInfo backpackData;
    [HideInInspector] public WeaponDataInfo weaponData;
    [HideInInspector] public BulletDataInfo bulletData;
    [HideInInspector] public MagazineDataInfo magData;
    [HideInInspector] public WeaponPartsDataInfo partsData;
    [HideInInspector] public GrenadeDataInfo grenadeData;
}

[System.Serializable]
public class StorageInfo
{
    public enum StorageType
    {
        Floor,
        Storage,
        Reward,
    }

    public string storageName;
    public StorageType type;
    public Vector2Int nodePos;
    public Vector2Int slotSize;
    public List<StorageItemInfo> itemList = new List<StorageItemInfo>();
}

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Object/GameData")]
public class GameData : ScriptableObject
{
    [HideInInspector] public DataManager dataMgr;
    [HideInInspector] public GameMenuManager gameMenuMgr;

    [Header("[Stage]")]
    public StageDataInfo stageData;
    public string mapName;
    public bool mapLoad;

    [Header("[Player]")]
    public string playerID;
    public int health;

    [Header("[BaseCamp]")]
    public List<StorageInfo> baseStorages = new List<StorageInfo>();
    public List<StorageInfo> floorStorages = new List<StorageInfo>();
    [Space(10f)]

    public List<StartingItem_Inventory> startingItemID_List = new List<StartingItem_Inventory>();

    public void RandomMapSelection()
    {
        stageData.waveNum--;
        if (stageData.waveNum > 0 && stageData.mapList.Count > 0)
        {
            var index = Random.Range(0, stageData.mapList.Count);
            mapName = stageData.mapList[index];
            stageData.mapList.RemoveAt(index);
            mapLoad = true;
        }
        else if (stageData.mapList.Count == 0 || stageData.waveNum == 0)
        {
            mapName = stageData.bossMap;
            mapLoad = true;
        }
        else
        {
            mapLoad = false;
        }
    }
}