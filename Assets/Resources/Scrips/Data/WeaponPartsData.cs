using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponPartsType
{
    None,
    Muzzle,
    Sight,
    Attachment,
    UnderBarrel,
}

public enum WeaponPartsSize
{
    None,
    Small,
    Medium,
    Large,
}

[System.Serializable]
public class WeaponPartsDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string prefabName;
    public string partsName;
    [Space(5f)]

    public List<int> compatModel;
    public int weight;
    public WeaponPartsType type;
    public WeaponPartsSize size;
    [Space(5f)]

    public int RPM;
    public float range;
    public int watchAngle;
    public float MOA;
    public int stability;
    public int rebound;
    public int ergonomy;
    public float headShot;
    public int actionCost;

    public WeaponPartsDataInfo CopyData()
    {
        var partsData = new WeaponPartsDataInfo()
        {
            indexName = indexName,

            ID = ID,
            prefabName = prefabName,
            partsName = partsName,

            compatModel = new List<int>(compatModel),
            weight = weight,
            type = type,
            size = size,

            RPM = RPM,
            range = range,
            watchAngle = watchAngle,
            MOA = MOA,
            stability = stability,
            rebound = rebound,
            ergonomy = ergonomy,
            headShot = headShot,
            actionCost = actionCost,
        };

        return partsData;
    }
}

[CreateAssetMenu(fileName = "WeaponPartsData", menuName = "Scriptable Object/WeaponPartsData")]
public class WeaponPartsData : ScriptableObject
{
    public List<WeaponPartsDataInfo> partsInfos = new List<WeaponPartsDataInfo>();
}
