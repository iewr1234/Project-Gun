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
    [Space(5f)][Tooltip("구경")] public float caliber;
    [Tooltip("펠릿 수")] public int pelletNum;
    [Tooltip("확산")] public int spread;
    [Tooltip("무게")] public int weight;
    [Space(5f)]

    [Tooltip("정확도")] public float MOA;
    [Tooltip("안정성")] public int stability;
    [Tooltip("반동")] public int rebound;
    [Tooltip("장약")] public int propellant;
    [Space(5f)]

    [Tooltip("피해량")] public int damage;
    [Tooltip("관통")] public int penetrate;
    [Tooltip("방어구 손상")] public int armorBreak;
    [Tooltip("파편화")] public int critical;

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
