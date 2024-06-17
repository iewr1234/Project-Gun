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

    [Tooltip("모델")] public int model;
    [Tooltip("구경")] public float caliber;
    [Tooltip("무기분류")] public WeaponType type;
    [Tooltip("발사속도")] public int RPM;
    [Tooltip("사거리")] public float range;
    [Tooltip("경계각")] public int watchAngle;
    [Tooltip("정확도")] public float MOA;
    [Tooltip("안정성")] public int stability;
    [Tooltip("반동")] public int rebound;
    [Tooltip("행동소모")] public int actionCost;
    [Space(5f)]

    [Tooltip("탄창 사용")] public List<WeaponPartsSize> useMagazine;
    [Tooltip("총구 사용")] public List<WeaponPartsSize> useMuzzle;
    [Tooltip("조준경 사용")] public List<WeaponPartsSize> useSight;
    [Tooltip("하부 사용")] public List<WeaponPartsSize> useUnderRail;
    [Tooltip("레일 사용")] public List<WeaponPartsSize> useRail;
    [Space(5f)]

    [Tooltip("약실 탄환")] public BulletDataInfo chamberBullet = null;
    [HideInInspector] public bool isChamber;
    [Tooltip("장착 탄창")] public MagazineDataInfo equipMag = null;
    [HideInInspector] public bool isMag;
    [Tooltip("장착부품 리스트")] public List<WeaponPartsDataInfo> equipPartsList = new List<WeaponPartsDataInfo>();

    public WeaponDataInfo CopyData()
    {
        var weaponData = new WeaponDataInfo()
        {
            indexName = indexName,

            ID = ID,
            prefabName = prefabName,
            weaponName = weaponName,

            model = model,
            caliber = caliber,
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

            chamberBullet = chamberBullet,
            isChamber = isChamber,
            equipMag = equipMag,
            isMag = isMag,
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
