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
    [Space(5f)]

    public int level;
    [Tooltip("����")] public float caliber;
    [Tooltip("����")] public float weight;
    [Tooltip("���")] public int propellant;
    [Tooltip("���ط�")] public int damage;
    [Tooltip("����")] public int penetrate;
    [Tooltip("�� �ջ�")] public int armorBreak;
    [Tooltip("����ȭ")] public int critical;

    public BulletDataInfo CopyData()
    {
        var bulletData = new BulletDataInfo()
        {
            indexName = indexName,

            ID = ID,
            bulletName = bulletName,

            level = level,
            caliber = caliber,
            weight = weight,
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
