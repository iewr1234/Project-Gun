using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string bulletName;
    public int level;
    [Space(5f)]

    [Tooltip("구경")] public float caliber;
    [Tooltip("장약")] public int propellant;
    [Tooltip("피해량")] public int damage;
    [Tooltip("관통")] public int penetrate;
    [Tooltip("방어구 손상")] public int armorBreak;
    [Tooltip("파편화")] public int critical;

    public BulletDataInfo CopyData()
    {
        var bulletData = new BulletDataInfo()
        {
            indexName = indexName,

            ID = ID,
            bulletName = bulletName,
            level = level,

            caliber = caliber,
            propellant = propellant,
            damage = damage,
            penetrate = penetrate,
            armorBreak = armorBreak,
            critical = critical,
        };

        return bulletData;
    }
}

[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Object/BulletData")]
public class BulletData : ScriptableObject
{
    public List<BulletDataInfo> bulletInfos = new List<BulletDataInfo>();
}
