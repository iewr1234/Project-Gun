using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemOptionType
{
    None,
    Health,
    Defense,
    Damage,
    Critical,
}

[System.Serializable]
public class ItemOptionDataInfo
{
    public string indexName;
    public int rank;
    public int mainType;
    public int subType;
    public ItemOptionType optionType;
    public int minValue;
    public int maxValue;
    public string scriptText;
}

[CreateAssetMenu(fileName = "ItemOptionData", menuName = "Scriptable Object/ItemOptionData")]
public class ItemOptionData : ScriptableObject
{
    public List<ItemOptionDataInfo> itemOptionInfos = new List<ItemOptionDataInfo>();
}
