using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageLevel
{
    None,
    Easy,
    Nomal,
    Hard,
}

[System.Serializable]
public struct SpawnEnemyInfo
{
    public string ID;
    public int level;
}

[System.Serializable]
public class StageDataInfo
{
    public string indexName;
    public string ID;
    public string stageName;
    public StageLevel level;

    [Header("[Map List]")]
    public int waveNum;
    public List<string> mapList = new List<string>();
    public string bossMap;

    [Header("[Enemy List]")]
    public List<SpawnEnemyInfo> shortRangeEnemys = new List<SpawnEnemyInfo>();
    public List<SpawnEnemyInfo> middleRangeEnemys = new List<SpawnEnemyInfo>();
    public List<SpawnEnemyInfo> longRangeEnemys = new List<SpawnEnemyInfo>();
    public List<SpawnEnemyInfo> eliteEnemys = new List<SpawnEnemyInfo>();
    public SpawnEnemyInfo bossEnemy;

    public StageDataInfo CopyData()
    {
        var stageData = new StageDataInfo()
        {
            indexName = indexName,
            ID = ID,
            stageName = stageName,
            level = level,
            waveNum = waveNum,
            mapList = new List<string>(mapList),
            bossMap = bossMap,
            shortRangeEnemys = new List<SpawnEnemyInfo>(shortRangeEnemys),
            middleRangeEnemys = new List<SpawnEnemyInfo>(middleRangeEnemys),
            longRangeEnemys = new List<SpawnEnemyInfo>(longRangeEnemys),
            eliteEnemys = new List<SpawnEnemyInfo>(eliteEnemys),
            bossEnemy = bossEnemy,
        };

        return stageData;
    }
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Object/StageData")]
public class StageData : ScriptableObject
{
    public List<StageDataInfo> stageInfos = new List<StageDataInfo>();
}
