using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    None,
    Pistol,
    Rifle,
}

[System.Serializable]
public class WeaponDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string prefabName;
    public string weaponName;
    [Space(5f)]

    public WeaponType type;
    public int damage;
    public int penetrate;
    public int armorBreak;
    public int critical;
    public int rpm;
    public float range;
    public int watchAngle;
    public float MOA;
    public int stability;
    public int rebound;
    public int actionCost;
    public int magMax;
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Object/WeaponData")]
public class WeaponData : ScriptableObject
{
    public List<WeaponDataInfo> weaponInfos = new List<WeaponDataInfo>();
}
