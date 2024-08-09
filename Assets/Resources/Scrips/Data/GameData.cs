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
public class BaseStorage
{
    public string indexName;
    public Vector2Int nodePos;
    public List<ItemData> itemList = new List<ItemData>();
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

    [Header("[BaseCamp]")]
    public List<BaseStorage> baseStorages = new List<BaseStorage>();
}