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

    [Tooltip("�ѱ� ���")] public List<WeaponPartsSize> useMuzzle;
    [Tooltip("���ذ� ���")] public List<WeaponPartsSize> useSight;
    [Tooltip("źâ ���")] public List<WeaponPartsSize> useMagazine;
    [Tooltip("�Ϻ� ���")] public List<WeaponPartsSize> useUnderRail;
    [Tooltip("���� ���")] public List<WeaponPartsSize> useRail;
    [Space(5f)]

    [Tooltip("������ǰ ����Ʈ")] public List<WeaponPartsDataInfo> equipPartsList;

    public WeaponDataInfo()
    {
        indexName = string.Empty;

        ID = string.Empty;
        prefabName = string.Empty;
        weaponName = string.Empty;

        model = 0;
        type = WeaponType.None;
        damage = 0;
        penetrate = 0;
        armorBreak = 0;
        critical = 0;
        RPM = 0;
        range = 0;
        watchAngle = 0;
        MOA = 0f;
        stability = 0;
        rebound = 0;
        actionCost = 0;

        useMuzzle = new List<WeaponPartsSize>();
        useSight = new List<WeaponPartsSize>();
        useMagazine = new List<WeaponPartsSize>();
        useUnderRail = new List<WeaponPartsSize>();
        useRail = new List<WeaponPartsSize>();

        equipPartsList = new List<WeaponPartsDataInfo>();
    }

    public WeaponDataInfo Copy()
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

            useMuzzle = new List<WeaponPartsSize>(useMuzzle),
            useSight = new List<WeaponPartsSize>(useSight),
            useMagazine = new List<WeaponPartsSize>(useMagazine),
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
