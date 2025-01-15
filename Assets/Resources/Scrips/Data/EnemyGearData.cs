using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyGearDataInfo
{
    //[System.Serializable]
    //public struct WeaponInfo
    //{
    //    public string prefabName;
    //    public bool isMain;
    //    public WeaponType weaponType;
    //    public MagazineType magType;
    //    public WeaponGripType gripType;
    //    public int magMax;

    //    [Space(5f)] public Mesh bulletMesh;
    //    public Material bulletMat;
    //    public int pelletNum;
    //    public int spread;
    //}

    public string indexName;
    public string ID;
    public string gearName;
    [Space(5f)]

    public string prefabName;
    public bool isMain;
    public WeaponType weaponType;
    public MagazineType magType;
    public WeaponGripType gripType;
    public int magMax;
    public int actionCost_shot;
    public float actionCost_reload;

    [Space(5f)] public Mesh bulletMesh;
    public Material bulletMat;
    public int pelletNum;
    public int spread;

    //public WeaponInfo mainWeapon1;
    //public WeaponInfo mainWeapon2;
    //public WeaponInfo subWeapon;
}

[CreateAssetMenu(fileName = "EnemyGearData", menuName = "Scriptable Object/EnemyGearData")]
public class EnemyGearData : ScriptableObject
{
    public List<EnemyGearDataInfo> enemyGearInfo = new List<EnemyGearDataInfo>();
}
