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
}

[CreateAssetMenu(fileName = "MagazineData", menuName = "Scriptable Object/MagazineData")]
public class MagazineData : ScriptableObject
{
    public List<MagazineDataInfo> magInfos = new List<MagazineDataInfo>();
}
