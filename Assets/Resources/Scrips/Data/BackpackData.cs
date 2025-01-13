using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BackpackDataInfo
{
    public string indexName;
    public string ID;
    public string backpackName;
    public int weight;
    public Vector2Int storageSize;

    public BackpackDataInfo CopyData()
    {
        var backpackData = new BackpackDataInfo()
        {
            indexName = indexName,
            ID = ID,
            backpackName = backpackName,
            weight = weight,
            storageSize = storageSize,
        };

        return backpackData;
    }
}

[CreateAssetMenu(fileName = "BackpackData", menuName = "Scriptable Object/BackpackData")]
public class BackpackData : ScriptableObject
{
    public List<BackpackDataInfo> backpackInfos = new List<BackpackDataInfo>();
}
