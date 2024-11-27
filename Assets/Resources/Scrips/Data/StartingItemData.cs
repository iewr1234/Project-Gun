using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CreateSpace
{
    None,
    Equip,
    Pocket,
    Rig,
    Backpack,
    Storage,
}

[System.Serializable]
public class StartingItemDataInfo
{
    public string indexName;
    public string createLocation;
    public string itemID;
    public int createNum;
    public CreateSpace createSpace;
}

[CreateAssetMenu(fileName = "StartingItemData", menuName = "Scriptable Object/StartingItemData")]
public class StartingItemData : ScriptableObject
{
    public List<StartingItemDataInfo> startingItemInfos = new List<StartingItemDataInfo>();
}
