using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    None,
    Pistol,
    Rifle,
    Shotgun,
    Revolver,
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
    [Tooltip("����")] public float caliber;
    [Tooltip("����")] public float weight;
    [Tooltip("����")] public bool isMain;
    [Tooltip("����з�")] public WeaponType type;
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

    [HideInInspector][Tooltip("����źâID")] public string equipMagID;
    [HideInInspector][Tooltip("������ǰIDs")] public List<string> equipPartsIDs;

    //[Tooltip("��� źȯ")] public BulletDataInfo chamberBullet = null;
    //[HideInInspector] public bool isChamber;

    [Space(5f)]
    [Tooltip("����źâ")] public MagazineDataInfo equipMag;
    [HideInInspector] public bool isMag;
    [Tooltip("������ǰ ����Ʈ")] public List<WeaponPartsDataInfo> equipPartsList = new List<WeaponPartsDataInfo>();

    public WeaponDataInfo CopyData(DataManager dataMgr)
    {
        var weaponData = new WeaponDataInfo()
        {
            indexName = indexName,

            ID = ID,
            prefabName = prefabName,
            weaponName = weaponName,

            model = model,
            caliber = caliber,
            weight = weight,
            isMain = isMain,
            type = type,
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
            equipMagID = equipMagID,
            equipPartsIDs = new List<string>(equipPartsIDs),

            //chamberBullet = chamberBullet,
            //isChamber = isChamber,
        };

        if (isMag)
        {
            weaponData.equipMag = equipMag.CopyData();
            weaponData.isMag = true;
        }
        else if (weaponData.equipMagID != "None")
        {
            var magData = dataMgr.magData.magInfos.Find(x => x.ID == weaponData.equipMagID);
            weaponData.equipMag = magData.CopyData();
            var loadedBullet = dataMgr.bulletData.bulletInfos.Find(x => x.ID == weaponData.equipMag.loadedBulletID);
            if (loadedBullet != null)
            {
                for (int i = 0; i < weaponData.equipMag.magSize; i++)
                {
                    weaponData.equipMag.loadedBullets.Add(loadedBullet);
                }
            }
            weaponData.isMag = true;
        }

        for (int i = 0; i < weaponData.equipPartsIDs.Count; i++)
        {
            var partsData = dataMgr.partsData.partsInfos.Find(x => x.ID == weaponData.equipPartsIDs[i]);
            weaponData.equipPartsList.Add(partsData);
        }

        return weaponData;
    }

    public float GetWeaponWeight()
    {
        var totalWegiht = weight;
        if (isMag)
        {
            totalWegiht += equipMag.weight;
            for (int i = 0; i < equipMag.loadedBullets.Count; i++)
            {
                var bullet = equipMag.loadedBullets[i];
                totalWegiht += bullet.weight;
            }
        }
        for (int i = 0; i < equipPartsList.Count; i++)
        {
            var parts = equipPartsList[i];
            totalWegiht += parts.weight;
        }

        return DataUtility.GetFloorValue(totalWegiht, 1);
    }

    public int GetWeaponRebound(int propellant)
    {
        var totalRebound = rebound;
        totalRebound += propellant;
        for (int i = 0; i < equipPartsList.Count; i++)
        {
            var parts = equipPartsList[i];
            totalRebound += parts.rebound;
        }

        return totalRebound;
    }
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Object/WeaponData")]
public class WeaponData : ScriptableObject
{
    public List<WeaponDataInfo> weaponInfos = new List<WeaponDataInfo>();
}
