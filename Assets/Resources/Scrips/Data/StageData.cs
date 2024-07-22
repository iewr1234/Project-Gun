using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageDataInfo
{
    public string indexName;
    public string ID;
    public string stageName;
    public List<string> mapList = new List<string>();
    public int waveNum;
    public string bossID;

    public StageDataInfo CopyData()
    {
        var stageData = new StageDataInfo()
        {
            indexName = indexName,
            ID = ID,
            stageName = stageName,
            mapList = new List<string>(mapList),
            waveNum = waveNum,
            bossID = bossID,
        };

        return stageData;
    }
}

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Object/StageData")]
public class StageData : ScriptableObject
{
    public List<StageDataInfo> stageInfos = new List<StageDataInfo>();
}
