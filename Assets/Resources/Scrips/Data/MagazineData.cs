using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MagazineDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string prefabName;
    public string magName;
    public List<int> compatModel;
    public int magSize;

    public MagazineDataInfo CopyData()
    {
        var magData = new MagazineDataInfo()
        {
            indexName = indexName,
            ID = ID,
            prefabName = prefabName,
            magName = magName,
            compatModel = new List<int>(compatModel),
            magSize = magSize,
        };

        return magData;
    }
}

[CreateAssetMenu(fileName = "MagazineData", menuName = "Scriptable Object/MagazineData")]
public class MagazineData : ScriptableObject
{
    public List<MagazineDataInfo> magInfos = new List<MagazineDataInfo>();
}
