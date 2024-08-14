using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CreatePos
{
    None,
    Equip,
    Rig,
    Backpack,
}

[System.Serializable]
public struct InitialItem
{
    public string ID;
    public int num;
    public CreatePos createPos;
}

[System.Serializable]
public class StorageItemInfo
{
    public string indexName;
    public Vector2Int slotIndex;
    public Vector2Int itemSize;
    public int totalCount;
    public ItemDataInfo itemData;
}

[System.Serializable]
public class StorageInfo
{
    public string storageName;
    public Vector2Int nodePos;
    public Vector2Int slotSize;
    public List<StorageItemInfo> itemList = new List<StorageItemInfo>();
}

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Object/GameData")]
public class GameData : ScriptableObject
{
    [HideInInspector] public DataManager dataMgr;
    [HideInInspector] public InventoryManager invenMgr;

    [Header("[Stage]")]
    public StageDataInfo stageData;
    public string mapName;
    public bool mapLoad;

    [Header("[Player]")]
    public string playerID;
    public int health;
    [Space(5f)]

    public List<InitialItem> initialItemIDList = new List<InitialItem>();

    [Header("[BaseCamp]")]
    public List<StorageInfo> baseStorages = new List<StorageInfo>();

    public void RandomMapSelection()
    {
        if (stageData.mapList.Count == 0) return;

        var index = Random.Range(0, stageData.mapList.Count);
        stageData.waveNum--;
        if (stageData.waveNum > 0)
        {
            mapName = stageData.mapList[index];
            stageData.mapList.RemoveAt(index);
            mapLoad = true;
        }
        else if (stageData.waveNum == 0)
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