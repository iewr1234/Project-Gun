using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireModeType
{
    SingleFire,
    AutoFire,
}

[System.Serializable]
public struct HitInfo
{
    public string indexName;
    public List<int> hitAccuracys;
    public bool isHit;
    public int hitNum;
}

public class Weapon : MonoBehaviour
{
    private enum AnumationLayers_CharacterA
    {
        Base,
        Pistol_A_Base,
        Pistol_A_Upper,
        Revolver_A_Upper,
        Rifle_A_Base,
        Rifle_A_Upper,
        Shotgun_A_Base,
        Shotgun_A_Upper,
    }

    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    [SerializeField] private CharacterController charCtr;

    [Header("---Access Component---")]
    [SerializeField] private List<GameObject> partsObjects = new List<GameObject>();
    private Transform bulletTf;

    [Header("--- Assignment Variable---")]
    public EquipType equipType;
    public WeaponDataInfo weaponData;
    [Space(5f)]

    [Tooltip("사격타입")] public FireModeType fireMode;
    [Tooltip("자동사격 발사 수")] public int autoFireNum;
    [Space(5f)]

    public int meshType;
    public int pelletNum;
    public int spread;
    [Tooltip("탄창용량")] public int magMax;
    [Tooltip("장전된 탄환 수")] public int loadedNum;
    //[Tooltip("사용탄환")] public BulletDataInfo useBullet;

    //[SerializeField] private List<bool> hitList = new List<bool>();
    public List<HitInfo> hitInfos = new List<HitInfo>();
    [HideInInspector] public int hitValue;

    private Vector3 holsterPos;
    private Vector3 holsterRot;
    private Vector3 defaultPos;
    private Vector3 defaultRot;

    private readonly Vector3 weaponPos_Pistol = new Vector3(0.082f, 0.034f, -0.037f);
    private readonly Vector3 weaponRot_Pistol = new Vector3(-8.375f, 89f, -90.246f);

    private readonly Vector3 weaponPos_Rifle_RightHolster = new Vector3(-0.19f, -0.21f, -0.2f);
    private readonly Vector3 weaponPos_Rifle_LeftHolster = new Vector3(-0.19f, -0.21f, 0.2f);
    private readonly Vector3 weaponRot_Rifle_Holster = new Vector3(0f, 90f, 0f);
    private readonly Vector3 weaponPos_Rifle = new Vector3(0.1f, 0.05f, 0.015f);
    private readonly Vector3 weaponRot_Rifle = new Vector3(-5f, 95.5f, -95f);

    private readonly Vector3 weaponPos_Shotgun = new Vector3(0.048f, 0.052f, -0.035f);
    private readonly Vector3 weaponRot_Shotgun = new Vector3(-5f, 95.5f, -95f);

    private readonly float shootDisparity_bullet = 0.15f;
    private readonly float shootDisparity_pellet = 0.3f;

    public void SetComponets(CharacterController _charCtr, EquipType _equipType, WeaponDataInfo _weaponData)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        equipType = _equipType;
        weaponData = _weaponData;
        //charCtr.SetWeaponAbility(true, weaponData);
        //if (weaponData.isMag && weaponData.equipMag.loadedBullets.Count > 0)
        //{
        //    var chamberBullet = weaponData.equipMag.loadedBullets[0];
        //    charCtr.SetBulletAbility(true, chamberBullet);
        //}
        charCtr.weapons.Add(this);

        bulletTf = transform.Find("BulletTransform");
        AddWeaponPartsObjects();
        SetWeaponPositionAndRotation();

        if (weaponData.isMag)
        {
            SetParts(weaponData.equipMag.magName, true);
        }
        for (int i = 0; i < weaponData.equipPartsList.Count; i++)
        {
            var partsData = weaponData.equipPartsList[i];
            SetParts(partsData.partsName, true);
        }
        gameObject.SetActive(true);
    }

    public void SetComponets(CharacterController _charCtr, EnemyWeapon weapon)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        charCtr.weapons.Add(this);

        weaponData.type = weapon.type;

        bulletTf = transform.Find("BulletTransform");
        AddWeaponPartsObjects();
        SetWeaponPositionAndRotation();

        meshType = weapon.meshType;
        pelletNum = weapon.pelletNum;
        spread = weapon.spread;
        magMax = weapon.magMax;
        loadedNum = magMax;
        gameObject.SetActive(true);
    }

    private void AddWeaponPartsObjects()
    {
        var parts = new List<GameObject>();
        var partsTf = transform.Find("PartsTransform");

        var magTf = partsTf.Find("Magazine");
        AddParts(magTf);

        var sightTf = partsTf.Find("Sight");
        AddParts(sightTf);

        var underRailTf = partsTf.Find("UnderRail");
        AddParts(underRailTf);

        partsObjects = parts;

        void AddParts(Transform partsTf)
        {
            if (partsTf == null) return;

            for (int i = 0; i < partsTf.childCount; i++)
            {
                var sample = partsTf.GetChild(i).gameObject;
                sample.SetActive(false);
                parts.Add(sample);
            }
        }
    }

    private void SetWeaponPositionAndRotation()
    {
        switch (weaponData.type)
        {
            case WeaponType.Pistol:
                holsterPos = Vector3.zero;
                holsterRot = Vector3.zero;
                defaultPos = weaponPos_Pistol;
                defaultRot = weaponRot_Pistol;
                break;
            case WeaponType.Rifle:
                if (charCtr.weapons.Count > 1)
                {
                    holsterPos = weaponPos_Rifle_RightHolster;
                    holsterRot = weaponRot_Rifle_Holster;
                }
                else
                {
                    holsterPos = weaponPos_Rifle_LeftHolster;
                    holsterRot = weaponRot_Rifle_Holster;
                }
                defaultPos = weaponPos_Rifle;
                defaultRot = weaponRot_Rifle;
                break;
            case WeaponType.Shotgun:
                if (charCtr.weapons.Count > 1)
                {
                    holsterPos = weaponPos_Rifle_RightHolster;
                    holsterRot = weaponRot_Rifle_Holster;
                }
                else
                {
                    holsterPos = weaponPos_Rifle_LeftHolster;
                    holsterRot = weaponRot_Rifle_Holster;
                }
                defaultPos = weaponPos_Shotgun;
                defaultRot = weaponRot_Shotgun;
                break;
            case WeaponType.Revolver:
                holsterPos = Vector3.zero;
                holsterRot = Vector3.zero;
                defaultPos = weaponPos_Pistol;
                defaultRot = weaponRot_Pistol;
                break;
            default:
                break;
        }
    }

    public void Initialize()
    {
        if (charCtr.currentWeapon == this)
        {
            UnequipWeapon();
        }
        //charCtr.weapons.Remove(this);
        charCtr = null;
        equipType = EquipType.None;
        weaponData = null;

        var activeParts = partsObjects.FindAll(x => x.gameObject.activeSelf);
        for (int i = 0; i < activeParts.Count; i++)
        {
            var parts = activeParts[i];
            parts.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    public void EquipWeapon()
    {
        //var isCover = charCtr.animator.GetBool("isCover");
        //var fullCover = charCtr.animator.GetBool("fullCover");
        //var isRight = charCtr.animator.GetBool("isRight");
        if (charCtr.baseIndex > 0 && charCtr.upperIndex > 0)
        {
            charCtr.animator.SetLayerWeight(charCtr.baseIndex, 0f);
            charCtr.animator.SetLayerWeight(charCtr.upperIndex, 0f);
        }

        charCtr.SetWeaponAbility(true, weaponData);
        if (weaponData.isMag && weaponData.equipMag.loadedBullets.Count > 0)
        {
            var chamberBullet = weaponData.equipMag.loadedBullets[0];
            charCtr.SetBulletAbility(true, chamberBullet);
        }

        switch (weaponData.type)
        {
            case WeaponType.Pistol:
                charCtr.baseIndex = (int)AnumationLayers_CharacterA.Pistol_A_Base;
                charCtr.upperIndex = (int)AnumationLayers_CharacterA.Pistol_A_Upper;
                break;
            case WeaponType.Rifle:
                charCtr.baseIndex = (int)AnumationLayers_CharacterA.Rifle_A_Base;
                charCtr.upperIndex = (int)AnumationLayers_CharacterA.Rifle_A_Upper;
                break;
            case WeaponType.Shotgun:
                charCtr.baseIndex = (int)AnumationLayers_CharacterA.Shotgun_A_Base;
                charCtr.upperIndex = (int)AnumationLayers_CharacterA.Shotgun_A_Upper;
                break;
            case WeaponType.Revolver:
                charCtr.baseIndex = (int)AnumationLayers_CharacterA.Pistol_A_Base;
                charCtr.upperIndex = (int)AnumationLayers_CharacterA.Revolver_A_Upper;
                break;
            default:
                break;
        }
        charCtr.animator.SetLayerWeight(charCtr.baseIndex, 1f);
        charCtr.animator.SetLayerWeight(charCtr.upperIndex, 1f);
        charCtr.SetRig(weaponData.type);

        //charCtr.animator.SetBool("isCover", isCover);
        //charCtr.animator.SetBool("fullCover", fullCover);
        //charCtr.animator.SetBool("isRight", isRight);
        //if (isCover)
        //{
        //    if (fullCover)
        //    {
        //        if (isRight)
        //        {
        //            charCtr.animator.Play("Base Layer.Cover.FullCover.CoverRight");
        //        }
        //        else
        //        {
        //            charCtr.animator.Play("Base Layer.Cover.FullCover.CoverLeft");
        //        }
        //    }
        //    else
        //    {
        //        charCtr.animator.Play("Base Layer.Cover.HalfCover.CoverIdle");
        //    }
        //}
    }

    public void UnequipWeapon()
    {
        charCtr.SetWeaponAbility(false, weaponData);
        if (weaponData.isMag && weaponData.equipMag.loadedBullets.Count > 0)
        {
            var chamberBullet = weaponData.equipMag.loadedBullets[0];
            charCtr.SetBulletAbility(false, chamberBullet);
        }
    }

    public void SetParts(string partsName, bool value)
    {
        var partsList = partsObjects.FindAll(x => x.name == partsName);
        for (int i = 0; i < partsList.Count; i++)
        {
            var parts = partsList[i];
            parts.gameObject.SetActive(value);
        }
    }

    public void SetParts(string parentsName, string partsName, bool value)
    {
        var partsList = partsObjects.FindAll(x => x.transform.parent.name == parentsName && x.name == partsName);
        for (int i = 0; i < partsList.Count; i++)
        {
            var parts = partsList[i];
            parts.gameObject.SetActive(value);
        }
    }

    public void WeaponSwitching(string switchPos)
    {
        switch (switchPos)
        {
            case "Holster":
                if (weaponData.type == WeaponType.Pistol)
                {
                    transform.SetParent(charCtr.subHolsterTf, false);
                }
                else
                {
                    transform.SetParent(charCtr.mainHolsterTf, false);
                }
                transform.localPosition = holsterPos;
                transform.localRotation = Quaternion.Euler(holsterRot);
                break;
            case "Right":
                transform.SetParent(charCtr.rightHandTf);
                transform.localPosition = defaultPos;
                transform.localRotation = Quaternion.Euler(defaultRot);
                break;
            case "Left":
                transform.SetParent(charCtr.leftHandTf);
                break;
            default:
                break;
        }
    }

    public void Reload()
    {
        //var ammoNum = magMax;
        //if (!chamberBullet)
        //{
        //    ammoNum--;
        //    chamberBullet = true;
        //}
        //loadedAmmo = ammoNum;
    }

    public bool CheckHitBullet(TargetInfo targetInfo, int shootNum)
    {
        hitInfos.Clear();
        hitValue = Random.Range(1, 101);
        var hitAccuracy = 100 - DataUtility.GetHitAccuracy(targetInfo);
        var isHit = hitAccuracy <= hitValue;
        var allMiss = !isHit;

        var hitAccuracys = new List<int>();
        int hitNum;
        int propellant;
        int pelletNum;
        int spread;
        var dist = DataUtility.GetDistance(targetInfo.shooterNode.transform.position, targetInfo.targetNode.transform.position);
        for (int i = 0; i < shootNum; i++)
        {
            hitAccuracys.Clear();
            hitNum = 0;
            propellant = 0;
            pelletNum = 0;
            spread = 0;
            if (i == 0)
            {
                SetUseValue(i, ref propellant, ref pelletNum, ref spread);
                ResultHitNum();
            }
            else
            {
                SetUseValue(i, ref propellant, ref pelletNum, ref spread);
                var rebound = Mathf.FloorToInt(charCtr.Rebound * 0.01f * propellant * 0.1f);
                if (rebound < 1) rebound = 1;

                hitAccuracy += rebound;
                ResultHitNum();
            }

            var hitText = isHit ? $"{hitNum}발 명중" : "빗나감";
            var hitAccuracyText = hitAccuracys.Count > 1 ? $"{hitAccuracys[0]}~{hitAccuracys[^1]}" : $"{hitAccuracys[0]}";
            var hitInfo = new HitInfo()
            {
                indexName = $"명중값(무작위값) = {hitValue}, 명중수치 = {hitAccuracyText}, {hitText}",
                hitAccuracys = new List<int>(hitAccuracys),
                isHit = isHit,
                hitNum = hitNum,
            };
            hitInfos.Add(hitInfo);
            Debug.Log($"{charCtr.name}: {i + 1}번째 탄: {hitText}");
        }

        return allMiss;

        void SetUseValue(int index, ref int propellant, ref int pelletNum, ref int spread)
        {
            switch (charCtr.ownerType)
            {
                case CharacterOwner.Player:
                    if (weaponData.isMag && weaponData.equipMag.loadedBullets.Count > 0)
                    {
                        var bulletIndex = weaponData.equipMag.loadedBullets.Count - (1 + index);
                        var bullet = weaponData.equipMag.loadedBullets[bulletIndex];
                        propellant = charCtr.ability.propellant + bullet.propellant;
                        pelletNum = bullet.pelletNum;
                        spread = bullet.spread;
                    }
                    break;
                default:
                    propellant = charCtr.Propellant;
                    pelletNum = this.pelletNum;
                    spread = this.spread;
                    break;
            }
        }

        void ResultHitNum()
        {
            isHit = hitAccuracy <= hitValue;
            if (pelletNum == 0)
            {
                hitNum = 1;
                hitAccuracys.Add(hitAccuracy);
            }
            else
            {
                hitNum++;
                hitAccuracys.Add(hitAccuracy);
                for (int i = 1; i < pelletNum; i++)
                {
                    var pelletSpread = Mathf.FloorToInt(dist * spread * 0.01f);
                    if (pelletSpread < 1) pelletSpread = 1;

                    hitAccuracy += pelletSpread;
                    if (hitAccuracy <= hitValue)
                    {
                        hitNum++;
                    }
                    hitAccuracys.Add(hitAccuracy);
                }
            }
        }
    }

    public void FireBullet(CharacterController target)
    {
        //if (!weaponData.isChamber)
        //{
        //    if (weaponData.equipMag.loadedBullets.Count == 0)
        //    {
        //        Debug.Log($"{charCtr.name}: Magazine empty");
        //        return;
        //    }

        //    var bulletData = weaponData.equipMag.loadedBullets[0];
        //    weaponData.chamberBullet = bulletData;
        //    weaponData.isChamber = true;
        //    weaponData.equipMag.loadedBullets.RemoveAt(0);
        //}

        //BulletDataInfo loadedBullet = null;
        //switch (charCtr.ownerType)
        //{
        //    case CharacterOwner.Player:
        //        loadedBullet = weaponData.equipMag.loadedBullets[^1];
        //        break;
        //    case CharacterOwner.Enemy:
        //        //loadedBullet = useBullet;
        //        break;
        //    default:
        //        break;
        //}

        var hitInfo = hitInfos[0];
        int count;
        switch (charCtr.ownerType)
        {
            case CharacterOwner.Player:
                var loadedBullet = weaponData.equipMag.loadedBullets[^1];
                count = loadedBullet.pelletNum == 0 ? 1 : loadedBullet.pelletNum;
                for (int i = 0; i < count; i++)
                {
                    var bullet = gameMgr.bulletPool.Find(x => !x.gameObject.activeSelf);
                    if (bullet == null)
                    {
                        Debug.LogError("There are no bullet in the bulletPool");
                        return;
                    }

                    SetBulletDirection(bullet, i == 0 && hitInfo.isHit);
                    bullet.SetBullet(charCtr, target, loadedBullet.meshType, i == 0 && hitInfo.isHit, i == 0 && !hitInfo.isHit, hitInfo.hitNum);
                }
                hitInfos.RemoveAt(0);
                charCtr.SetBulletAbility(false, loadedBullet);
                weaponData.equipMag.loadedBullets.Remove(loadedBullet);
                if (weaponData.equipMag.loadedBullets.Count == 0) return;

                var chamberBullet = weaponData.equipMag.loadedBullets[^1];
                charCtr.SetBulletAbility(true, chamberBullet);
                break;
            case CharacterOwner.Enemy:
                count = pelletNum == 0 ? 1 : pelletNum;
                for (int i = 0; i < count; i++)
                {
                    var bullet = gameMgr.bulletPool.Find(x => !x.gameObject.activeSelf);
                    if (bullet == null)
                    {
                        Debug.LogError("There are no bullet in the bulletPool");
                        return;
                    }

                    SetBulletDirection(bullet, i == 0 && hitInfo.isHit);
                    bullet.SetBullet(charCtr, target, meshType, i == 0 && hitInfo.isHit, i == 0 && !hitInfo.isHit, hitInfo.hitNum);
                }
                hitInfos.RemoveAt(0);
                loadedNum--;
                break;
            default:
                break;
        }

        void SetBulletDirection(Bullet bullet, bool noDisparity)
        {
            bullet.gameObject.SetActive(true);
            bullet.transform.position = bulletTf.position;
            var aimPos = charCtr.aimPoint.position;
            if (!noDisparity)
            {
                var disparity = count == 1 ? shootDisparity_bullet : shootDisparity_pellet;
                aimPos += (charCtr.transform.right * Random.Range(-disparity, disparity)) + (charCtr.transform.up * Random.Range(-disparity, disparity));
            }
            bullet.transform.LookAt(aimPos);
        }

        //if (weaponData.equipMag.loadedBullets.Count > 0)
        //{
        //    var bulletData = weaponData.equipMag.loadedBullets[0];
        //    weaponData.chamberBullet = bulletData;
        //    weaponData.equipMag.loadedBullets.RemoveAt(0);
        //}
        //else
        //{
        //    weaponData.chamberBullet = null;
        //    weaponData.isChamber = false;
        //}
    }

    public CharacterController CharCtr
    {
        private set { charCtr = value; }
        get { return charCtr; }
    }
}
