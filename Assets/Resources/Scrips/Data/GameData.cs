using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void RandomMapSelection()
    {
        if (stageData.mapList.Count == 0) return;

        var index = Random.Range(0, stageData.mapList.Count);
        mapName = stageData.mapList[index];
        stageData.mapList.RemoveAt(index);
        stageData.waveNum--;
        mapLoad = stageData.waveNum >= 0;
    }
}