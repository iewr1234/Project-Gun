using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RigDataInfo
{
    public string indexName;
    public string ID;
    public string rigName;
    public int weight;
    public Vector2Int storageSize;

    public RigDataInfo CopyData()
    {
        var rigData = new RigDataInfo()
        {
            indexName = indexName,
            ID = ID,
            rigName = rigName,
            weight = weight,
            storageSize = storageSize,
        };

        return rigData;
    }
}

[CreateAssetMenu(fileName = "RigData", menuName = "Scriptable Object/RigData")]
public class RigData : ScriptableObject
{
    public List<RigDataInfo> rigInfos = new List<RigDataInfo>();
}
