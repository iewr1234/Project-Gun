using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DropTable
{
    public string indexName;

    [Header("[Item Num]")]
    public int itemMinNum;
    public int itemMaxNum;

    [Header("[Item Drop Percentage]")]
    [Tooltip("ÇÏ±Þ È®·ü")] public int dropPercentage_lowGrade;
    [Tooltip("ÀÏ¹Ý È®·ü")] public int dropPercentage_nomal;
    [Tooltip("Áß±Þ È®·ü")] public int dropPercentage_middleGrade;
    [Tooltip("»ó±Þ È®·ü")] public int dropPercentage_highGrade;
    [Tooltip("°í±Þ È®·ü")] public int dropPercentage_advanced;
    [Tooltip("¼¼Æ® È®·ü")] public int dropPercentage_set;

    public readonly int LowGrade => dropPercentage_lowGrade;
    public readonly int Nomal => dropPercentage_lowGrade 
                               + dropPercentage_nomal;
    public readonly int MiddleGrade => dropPercentage_lowGrade 
                                     + dropPercentage_nomal 
                                     + dropPercentage_middleGrade;
    public readonly int HighGrade => dropPercentage_lowGrade 
                                   + dropPercentage_nomal 
                                   + dropPercentage_middleGrade
                                   + dropPercentage_highGrade;
    public readonly int Advanced => dropPercentage_lowGrade 
                                  + dropPercentage_nomal
                                  + dropPercentage_middleGrade
                                  + dropPercentage_highGrade
                                  + dropPercentage_advanced;
    //public readonly int Set => dropPercentage_lowGrade
    //                         + dropPercentage_nomal
    //                         + dropPercentage_middleGrade
    //                         + dropPercentage_highGrade
    //                         + dropPercentage_advanced
    //                         + dropPercentage_set;
    public readonly int TotalPercentage => dropPercentage_lowGrade
                                         + dropPercentage_nomal
                                         + dropPercentage_middleGrade
                                         + dropPercentage_highGrade
                                         + dropPercentage_advanced
                                         + dropPercentage_set;
}

[System.Serializable]
public class UniqueTable
{
    public int itemMinNum;
    public int itemMaxNum;
    public int dropPercentage;
}

[System.Serializable]
public class DropTableDataInfo
{
    public string ID;
    public StageLevel stageLevel;
    public int minItemLevel;
    public int maxItemLevel;
    [Space(5f)]

    public DropTable dropInfo_equipment;
    public DropTable dropInfo_expendable;
    public DropTable dropInfo_ingredient;
    [Space(5f)]

    public UniqueTable uniqueTable;
}

[CreateAssetMenu(fileName = "DropTableData", menuName = "Scriptable Object/DropTableData")]
public class DropTableData : ScriptableObject
{
    public List<DropTableDataInfo> dropTableInfo = new List<DropTableDataInfo>();
}
