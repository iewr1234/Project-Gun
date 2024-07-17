using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ItemLevelInfo
{
    public int minLevel;
    public int maxLevel;
}

[System.Serializable]
public struct ItemRankOptionInfo
{
    public string indexName;
    public int option1_rank;
    public int option2_rank;
    public int option3_rank;
    public int option4_rank;
}

[System.Serializable]
public class OptionSheetDataInfo
{
    public string indexName;
    public ItemLevelInfo levelInfo;
    public List<ItemRankOptionInfo> rankOptions = new List<ItemRankOptionInfo>();
}

[CreateAssetMenu(fileName = "OptionSheetData", menuName = "Scriptable Object/OptionSheetData")]
public class OptionSheetData : ScriptableObject
{
    public List<OptionSheetDataInfo> optionSheetInfos = new List<OptionSheetDataInfo>();
}
