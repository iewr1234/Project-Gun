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

public enum MagazineType
{
    None,
    Magazine,
    IntMagazine,
    Cylinder,
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

    [Tooltip("모델")] public int model;
    [Tooltip("구경")] public float caliber;
    [Tooltip("무게")] public float weight;
    [Tooltip("구분")] public bool isMain;
    [Tooltip("무기분류")] public WeaponType weaponType;
    [Tooltip("탄창분류")] public MagazineType magType;
    [Space(5f)]

    public List<ShootingModeInfo> sModeInfos = new List<ShootingModeInfo>();
    [Tooltip("발사속도")] public int RPM;
    [Tooltip("사거리")] public float range;
    [Tooltip("경계각")] public int watchAngle;
    [Tooltip("행동소모")] public int actionCost;
    [Space(5f)]

    [Tooltip("정확도")] public float MOA;
    [Tooltip("안정성")] public int stability;
    [Tooltip("반동")] public int rebound;
    [Tooltip("장약")] public int propellant;
    [Space(5f)]

    [Tooltip("약실 탄환")] public BulletDataInfo chamberBullet = null;
    [HideInInspector] public bool isChamber;

    [Tooltip("장착탄창")] public MagazineDataInfo equipMag;
    [HideInInspector] public bool isMag;

    [Tooltip("장착부품 리스트")] public List<WeaponPartsDataInfo> equipPartsList = new List<WeaponPartsDataInfo>();

    [HideInInspector][Tooltip("탄창 사용")] public List<WeaponPartsSize> useMagazine;
    [HideInInspector][Tooltip("총구 사용")] public List<WeaponPartsSize> useMuzzle;
    [HideInInspector][Tooltip("조준경 사용")] public List<WeaponPartsSize> useSight;
    [HideInInspector][Tooltip("하부 사용")] public List<WeaponPartsSize> useUnderRail;
    [HideInInspector][Tooltip("레일 사용")] public List<WeaponPartsSize> useRail;

    [HideInInspector][Tooltip("장착탄창ID")] public string equipMagID;
    [HideInInspector][Tooltip("장착부품IDs")] public List<string> equipPartsIDs;

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
            weaponType = weaponType,
            magType = magType,

            sModeInfos = new List<ShootingModeInfo>(sModeInfos),
            RPM = RPM,
            range = range,
            watchAngle = watchAngle,
            actionCost = actionCost,

            MOA = MOA,
            stability = stability,
            rebound = rebound,
            propellant = propellant,

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

    //public string GetWeaponName(EquipType equipType)
    //{
    //    switch (equipType)
    //    {
    //        case EquipType.MainWeapon1:
    //            return $"{weaponName}_A";
    //        case EquipType.MainWeapon2:
    //            return $"{weaponName}_B";
    //        default:
    //            return weaponName;
    //    }
    //}

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
