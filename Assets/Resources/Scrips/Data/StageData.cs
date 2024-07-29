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
public struct DropItemInfo
{
    [Header("[Item Level]")]
    public int itemMinLevel;
    public int itemMaxLevel;

    [Header("[Item Num]")]
    public int itemMinNum;
    public int itemMaxNum;

    [Header("[Item Drop Percentage]")]
    [Tooltip("�ϱ� Ȯ��")] public int dropPercentage_lowGrade;
    [Tooltip("�Ϲ� Ȯ��")] public int dropPercentage_nomal;
    [Tooltip("�߱� Ȯ��")] public int dropPercentage_middleGrade;
    [Tooltip("��� Ȯ��")] public int dropPercentage_highGrade;
    [Tooltip("��� Ȯ��")] public int dropPercentage_advanced;
    [Tooltip("��Ʈ Ȯ��")] public int dropPercentage_set;
    [Tooltip("Ư�� Ȯ��")] public int dropPercentage_special;
    public readonly int TotalPercentage => dropPercentage_lowGrade
                                         + dropPercentage_nomal
                                         + dropPercentage_middleGrade
                                         + dropPercentage_highGrade
                                         + dropPercentage_advanced
                                         + dropPercentage_set
                                         + dropPercentage_special;
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

    [Header("[Item Drop Table]")]
    public DropItemInfo dropInfo_equipment;
    public DropItemInfo dropInfo_expendable;
    public DropItemInfo dropInfo_ingredient;

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
            dropInfo_equipment = dropInfo_equipment,
            dropInfo_expendable = dropInfo_expendable,
            dropInfo_ingredient = dropInfo_ingredient,
        };

        return stageData;
    }
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Object/StageData")]
public class StageData : ScriptableObject
{
    public List<StageDataInfo> stageInfos = new List<StageDataInfo>();
}
