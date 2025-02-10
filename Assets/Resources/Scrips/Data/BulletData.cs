using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string prefabName;
    public string bulletName;
    public Mesh bulletMesh;
    public Material bulletMat;

    [HideInInspector] public int level;
    [Space(5f)][Tooltip("����")] public float caliber;
    [Tooltip("�縴 ��")] public int pelletNum;
    [Tooltip("Ȯ��")] public int spread;
    [Tooltip("����")] public int weight;
    [Space(5f)]

    [Tooltip("��Ȯ��")] public float MOA;
    [Tooltip("������")] public int stability;
    [Tooltip("�ݵ�")] public int rebound;
    [Tooltip("���")] public int propellant;
    [Space(5f)]

    [Tooltip("���ط�")] public int damage;
    [Tooltip("����")] public int penetrate;
    [Tooltip("�� �ջ�")] public int armorBreak;
    [Tooltip("����ȭ")] public int critical;

    public BulletDataInfo CopyData(int level)
    {
        var bulletData = new BulletDataInfo()
        {
            indexName = indexName,

            ID = ID,
            prefabName = prefabName,
            bulletName = bulletName,
            bulletMesh = bulletMesh,
            bulletMat = bulletMat,

            level = level,
            caliber = caliber,
            pelletNum = pelletNum,
            spread = spread,
            weight = weight,

            MOA = MOA,
            stability = stability,
            rebound = rebound,
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
