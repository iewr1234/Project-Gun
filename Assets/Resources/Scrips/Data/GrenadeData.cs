using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GrenadeDataInfo
{
    public string indexName;
    public string ID;
    public string grenadeName;
    public string FX_name;
    public float throwRange;
    public float blastRange;
    public int weight;
    public int damage;

    public GrenadeDataInfo CopyData()
    {
        var grdData = new GrenadeDataInfo()
        {
            indexName = indexName,
            ID = ID,
            grenadeName = grenadeName,
            FX_name = FX_name,
            throwRange = throwRange,
            blastRange = blastRange,
            weight = weight,
            damage = damage,
        };

        return grdData;
    }
}

[CreateAssetMenu(fileName = "GrenadeData", menuName = "Scriptable Object/GrenadeData")]
public class GrenadeData : ScriptableObject
{
    public List<GrenadeDataInfo> grenadeInfos = new List<GrenadeDataInfo>();
}
