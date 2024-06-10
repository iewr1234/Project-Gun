using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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

    [Tooltip("��")] public int model;
    [Tooltip("����з�")] public WeaponType type;
    [Tooltip("���ط�")] public int damage;
    [Tooltip("����")] public int penetrate;
    [Tooltip("�� �ջ�")] public int armorBreak;
    [Tooltip("����ȭ")] public int critical;
    [Tooltip("�߻�ӵ�")] public int RPM;
    [Tooltip("��Ÿ�")] public float range;
    [Tooltip("��谢")] public int watchAngle;
    [Tooltip("��Ȯ��")] public float MOA;
    [Tooltip("������")] public int stability;
    [Tooltip("�ݵ�")] public int rebound;
    [Tooltip("�ൿ�Ҹ�")] public int actionCost;
    [Space(5f)]

    [Tooltip("źâ ���")] public List<WeaponPartsSize> useMagazine;
    [Tooltip("�ѱ� ���")] public List<WeaponPartsSize> useMuzzle;
    [Tooltip("���ذ� ���")] public List<WeaponPartsSize> useSight;
    [Tooltip("�Ϻ� ���")] public List<WeaponPartsSize> useUnderRail;
    [Tooltip("���� ���")] public List<WeaponPartsSize> useRail;
    [Space(5f)]

    [Tooltip("��� źȯ")] public BulletDataInfo chamberBullet;
    [Tooltip("���� źâ")] public MagazineDataInfo equipMag;
    [Tooltip("������ǰ ����Ʈ")] public List<WeaponPartsDataInfo> equipPartsList;

    public WeaponDataInfo CopyData()
    {
        var weaponData = new WeaponDataInfo()
        {
            indexName = indexName,

            ID = ID,
            prefabName = prefabName,
            weaponName = weaponName,

            model = model,
            type = type,
            damage = damage,
            penetrate = penetrate,
            armorBreak = armorBreak,
            critical = critical,
            RPM = RPM,
            range = range,
            watchAngle = watchAngle,
            MOA = MOA,
            stability = stability,
            rebound = rebound,
            actionCost = actionCost,

            useMagazine = new List<WeaponPartsSize>(useMagazine),
            useMuzzle = new List<WeaponPartsSize>(useMuzzle),
            useSight = new List<WeaponPartsSize>(useSight),
            useUnderRail = new List<WeaponPartsSize>(useUnderRail),
            useRail = new List<WeaponPartsSize>(useRail),

            equipPartsList = new List<WeaponPartsDataInfo>(equipPartsList),
        };

        return weaponData;
    }
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Object/WeaponData")]
public class WeaponData : ScriptableObject
{
    public List<WeaponDataInfo> weaponInfos = new List<WeaponDataInfo>();
}
