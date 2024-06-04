using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponPartsType
{
    None,
    Muzzle,
    Sight,
    FrontHandle,
    Magazine,
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
    public WeaponPartsType type;
    public WeaponPartsSize size;
    public float weight;
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
}

[CreateAssetMenu(fileName = "WeaponPartsData", menuName = "Scriptable Object/WeaponPartsData")]
public class WeaponPartsData : ScriptableObject
{
    public List<WeaponPartsDataInfo> partsInfos = new List<WeaponPartsDataInfo>();
}
