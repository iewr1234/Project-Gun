using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditorInternal.ReorderableList;

[System.Serializable]
public class ShootingModeInfo
{
    public string indexName;
    public ShootingMode modeType;
    public int value;
}

[System.Serializable]
public struct HitInfo
{
    public string indexName;
    public int hitAccuracy;
    public List<int> pelletAccuracys;
    public bool isHit;
    public int hitNum;
    public int impact;
}

public class Weapon : MonoBehaviour
{
    public enum FireModeType
    {
        SingleFire,
        AutoFire,
    }

    private enum AnimationLayers_A
    {
        Base,
        Sub_A_Base,
        Sub_A_Upper,
        Main_A_Base,
        Main_A_Upper,
    }

    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    [SerializeField] private CharacterController charCtr;

    [Header("---Access Component---")]
    [SerializeField] private List<GameObject> partsObjects = new List<GameObject>();
    private Transform bulletTf;

    [Header("--- Assignment Variable---")]
    public EquipSlot equipSlot;
    public WeaponDataInfo weaponData;
    [Space(5f)]

    [Tooltip("사격타입")] public FireModeType fireMode;
    [Tooltip("자동사격 발사 수")] public int autoFireNum;
    [Space(5f)]

    public int meshType;
    [Tooltip("탄창용량")] public int magMax;
    [Tooltip("장전된 탄환 수")] public int loadedNum;

    private HitAccuracy hitAccuracy;
    public List<HitInfo> hitInfos = new List<HitInfo>();

    private Vector3 holsterPos;
    private Vector3 holsterRot;
    private Vector3 defaultPos;
    private Vector3 defaultRot;
    private bool isMove;

    private readonly Vector3 weaponPos_sub = new Vector3(0.082f, 0.034f, -0.037f);
    private readonly Vector3 weaponRot_sub = new Vector3(-8.375f, 89f, -90.246f);

    private readonly Vector3 weaponPos_main_rightHolster = new Vector3(-0.19f, -0.21f, -0.2f);
    private readonly Vector3 weaponPos_main_leftHolster = new Vector3(-0.19f, -0.21f, 0.2f);
    private readonly Vector3 weaponRot_main_holster = new Vector3(0f, 90f, 0f);
    private readonly Vector3 weaponPos_main = new Vector3(0.1f, 0.05f, 0.015f);
    private readonly Vector3 weaponRot_main = new Vector3(-5f, 95.5f, -95f);

    private readonly Vector3 weaponPos_main_sniperRifle = new Vector3(0.09f, 0.015f, -0.052f);
    private readonly Vector3 weaponRot_main_sniperRifle = new Vector3(-11.1f, 91.31f, -95f);
    private readonly Vector3 weaponPos_main_sniperRifle_aim = new Vector3(0.09f, 0.015f, -0.052f);
    private readonly Vector3 weaponRot_main_sniperRifle_aim = new Vector3(13.4f, 96.19f, -95f);


    private readonly float shootDisparity_bullet = 0.15f;
    private readonly float shootDisparity_pellet = 0.3f;

    public void SetComponets(CharacterController _charCtr, EquipSlot _equipSlot, WeaponDataInfo _weaponData)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        equipSlot = _equipSlot;
        weaponData = _weaponData;
        charCtr.weapons.Add(this);
        if (charCtr.weapons.Count > 1)
        {
            charCtr.weapons = charCtr.weapons.OrderBy(x => x.equipSlot.type).ToList();
        }

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
        hitAccuracy = new HitAccuracy(this);
        gameObject.SetActive(true);
    }

    public void SetComponets(CharacterController _charCtr, EnemyWeapon _eWeapon)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        charCtr.weapons.Add(this);
        weaponData.isMain = _eWeapon.isMain;
        weaponData.weaponType = _eWeapon.weaponType;
        weaponData.magType = _eWeapon.magType;

        bulletTf = transform.Find("BulletTransform");
        AddWeaponPartsObjects();
        SetWeaponPositionAndRotation();

        meshType = _eWeapon.meshType;
        magMax = _eWeapon.magMax;
        loadedNum = magMax;
        hitAccuracy = new HitAccuracy(this, _eWeapon);
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
        if (weaponData.isMain)
        {
            if (charCtr.weapons.Count > 1)
            {
                holsterPos = weaponPos_main_rightHolster;
                holsterRot = weaponRot_main_holster;
            }
            else
            {
                holsterPos = weaponPos_main_leftHolster;
                holsterRot = weaponRot_main_holster;
            }

            switch (weaponData.weaponType)
            {
                case WeaponType.SniperRifle:
                    defaultPos = weaponPos_main_sniperRifle;
                    defaultRot = weaponRot_main_sniperRifle;
                    break;
                default:
                    defaultPos = weaponPos_main;
                    defaultRot = weaponRot_main;
                    break;
            }
        }
        else
        {
            holsterPos = Vector3.zero;
            holsterRot = Vector3.zero;
            defaultPos = weaponPos_sub;
            defaultRot = weaponRot_sub;
        }
    }

    private void Update()
    {
        if (!isMove) return;
        if (!charCtr.animator.GetCurrentAnimatorStateInfo(charCtr.upperIndex).IsTag("None")) return;

        switch (weaponData.weaponType)
        {
            case WeaponType.SniperRifle:
                defaultPos = weaponPos_main_sniperRifle;
                defaultRot = weaponRot_main_sniperRifle;
                transform.localPosition = weaponPos_main_sniperRifle;
                transform.localRotation = Quaternion.Euler(weaponRot_main_sniperRifle);
                Debug.Log($"{charCtr.name}: Reset SniperRifle");
                break;
            default:
                break;
        }
        isMove = false;
    }

    public void Initialize()
    {
        charCtr = null;
        equipSlot = null;
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
        if (charCtr.baseIndex > 0 && charCtr.upperIndex > 0)
        {
            charCtr.animator.SetLayerWeight(charCtr.baseIndex, 0f);
            charCtr.animator.SetLayerWeight(charCtr.upperIndex, 0f);
        }
        charCtr.baseIndex = weaponData.isMain ? (int)AnimationLayers_A.Main_A_Base : (int)AnimationLayers_A.Sub_A_Base;
        charCtr.upperIndex = weaponData.isMain ? (int)AnimationLayers_A.Main_A_Upper : (int)AnimationLayers_A.Sub_A_Upper;
        charCtr.animator.SetLayerWeight(charCtr.baseIndex, 1f);
        charCtr.animator.SetLayerWeight(charCtr.upperIndex, 1f);
        charCtr.animator.SetBool("isMain", weaponData.isMain);
        charCtr.animator.SetInteger("weaponType", (int)weaponData.weaponType);
        charCtr.animator.SetInteger("magType", (int)weaponData.magType);
        charCtr.SetRig(weaponData.weaponType);
        charCtr.SetAbility();
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

    public void SetAllParts(string parentsName, bool value)
    {
        var partsList = partsObjects.FindAll(x => x.transform.parent.name == parentsName);
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
                if (weaponData.isMain)
                {
                    transform.SetParent(charCtr.mainHolsterTf, false);
                }
                else
                {
                    transform.SetParent(charCtr.subHolsterTf, false);
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

    public void FireBullet(CharacterController target)
    {
        var hitInfo = hitInfos[0];
        int count;
        switch (charCtr.ownerType)
        {
            case CharacterOwner.Player:
                LoadingChamber();
                var chamberBullet = weaponData.chamberBullet;
                count = chamberBullet.pelletNum == 0 ? 1 : chamberBullet.pelletNum;
                for (int i = 0; i < count; i++)
                {
                    var bullet = gameMgr.bulletPool.Find(x => !x.gameObject.activeSelf);
                    if (bullet == null)
                    {
                        Debug.LogError("There are no bullet in the bulletPool");
                        return;
                    }

                    SetBulletDirection(bullet, i == 0 && hitInfo.isHit);
                    bullet.SetBullet(charCtr, target, chamberBullet.meshType, i == 0 && hitInfo.isHit, i == 0 && !hitInfo.isHit, hitInfo.hitNum);
                }
                hitInfos.RemoveAt(0);
                weaponData.chamberBullet = null;
                weaponData.isChamber = false;
                if (weaponData.magType != MagazineType.Cylinder) LoadingChamber();
                if (equipSlot != null) equipSlot.SetLoadedBulletCount();
                break;
            case CharacterOwner.Enemy:
                count = hitAccuracy.pelletNum == 0 ? 1 : hitAccuracy.pelletNum;
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

        void LoadingChamber()
        {
            if (weaponData.isChamber) return;
            if (!weaponData.isMag) return;
            if (weaponData.equipMag.loadedBullets.Count == 0) return;

            var loadedBullet = weaponData.equipMag.loadedBullets[^1];
            weaponData.chamberBullet = loadedBullet;
            weaponData.isChamber = true;
            weaponData.equipMag.loadedBullets.Remove(loadedBullet);
            charCtr.SetAbility();
        }

        void SetBulletDirection(Bullet bullet, bool noDisparity)
        {
            bullet.gameObject.SetActive(true);
            bullet.transform.position = bulletTf.position;
            var aimPos = charCtr.aimPoint.position;
            aimPos.y = bulletTf.position.y;
            if (!noDisparity)
            {
                var disparity = count == 1 ? shootDisparity_bullet : shootDisparity_pellet;
                aimPos += (charCtr.transform.right * Random.Range(-disparity, disparity)) + (charCtr.transform.up * Random.Range(-disparity, disparity));
            }
            bullet.transform.LookAt(aimPos);
        }
    }

    public bool CheckHitBullet(TargetInfo targetInfo, int shootNum, bool resultShoot)
    {
        var allMiss = false;
        if (!resultShoot)
        {
            hitAccuracy.CheckHitBullet_Aim(targetInfo, shootNum);
        }
        else
        {
            allMiss = hitAccuracy.CheckHitBullet_Shoot(targetInfo, shootNum);
        }

        return allMiss;
    }

    public int GetHitValue()
    {
        return hitAccuracy.hitValue;
    }

    public void MoveLocalPosition(bool value)
    {
        switch (weaponData.weaponType)
        {
            case WeaponType.SniperRifle:
                if (value)
                {
                    defaultPos = weaponPos_main_sniperRifle_aim;
                    defaultRot = weaponRot_main_sniperRifle_aim;
                    transform.localPosition = weaponPos_main_sniperRifle_aim;
                    transform.localRotation = Quaternion.Euler(weaponRot_main_sniperRifle_aim);
                }
                else
                {
                    isMove = true;
                }
                break;
            default:
                break;
        }
    }

    private class HitAccuracy
    {
        private Weapon weapon;

        [Tooltip("명중값")] public int hitAccuracy;
        [Tooltip("팰릿명중값 리스트")] public List<int> pelletAccuracys;
        [Tooltip("무작위값")] public int hitValue;
        [Tooltip("명중여부")] public bool isHit;
        [Tooltip("명중수")] public int hitNum;
        [Tooltip("충격량")] public int impact;
        [Tooltip("안정성")] public int stability;
        [Tooltip("반동")] public int rebound;
        [Tooltip("장약")] public int propellant;
        [Tooltip("팰릿 수")] public int pelletNum;
        [Tooltip("확산")] public int spread;

        [Tooltip("거리")] public float distance;
        [Tooltip("현재 기력")] private int curStamina;

        public HitAccuracy(Weapon _weapon)
        {
            weapon = _weapon;
            //hitAccuracys = new List<int>();
            pelletAccuracys = new List<int>();
        }

        public HitAccuracy(Weapon _weapon, EnemyWeapon _eWeapon)
        {
            weapon = _weapon;
            pelletNum = _eWeapon.pelletNum;
            spread = _eWeapon.spread;
            //hitAccuracys = new List<int>();
            pelletAccuracys = new List<int>();
        }

        public void CheckHitBullet_Aim(TargetInfo targetInfo, int shootNum)
        {
            weapon.hitInfos.Clear();
            distance = DataUtility.GetDistance(targetInfo.shooterNode.transform.position, targetInfo.targetNode.transform.position);
            hitAccuracy = 100 - DataUtility.GetHitAccuracy(targetInfo, distance);
            curStamina = weapon.charCtr.stamina;

            int loadedBulletNum = 0;
            if (weapon.weaponData.isChamber) loadedBulletNum++;
            if (weapon.weaponData.isMag) loadedBulletNum += weapon.weaponData.equipMag.loadedBullets.Count;
            for (int i = 0; i < shootNum; i++)
            {
                if (weapon.charCtr.ownerType == CharacterOwner.Player && i == loadedBulletNum) break;
                if (weapon.charCtr.ownerType == CharacterOwner.Enemy && i > weapon.loadedNum) break;

                ResultHitAccuracys(i);
                var hitAccuracyText = pelletAccuracys.Count > 0 ? $"{hitAccuracy}~{pelletAccuracys[^1]}" : $"{hitAccuracy}";
                var hitInfo = new HitInfo()
                {
                    indexName = $"거리 = {distance}, 충격량 = {impact}, 반동 = {rebound}, 장약 = {propellant}, 명중값 = {hitAccuracyText}",
                    //hitAccuracys = new List<int>(hitAccuracys),
                    hitAccuracy = hitAccuracy,
                    pelletAccuracys = new List<int>(pelletAccuracys),
                    impact = impact,
                };
                weapon.hitInfos.Add(hitInfo);
            }
        }

        public bool CheckHitBullet_Shoot(TargetInfo targetInfo, int shootNum)
        {
            weapon.hitInfos.Clear();
            hitValue = Random.Range(1, 101);
            distance = DataUtility.GetDistance(targetInfo.shooterNode.transform.position, targetInfo.targetNode.transform.position);
            hitAccuracy = 100 - DataUtility.GetHitAccuracy(targetInfo, distance);
            isHit = hitAccuracy <= hitValue;
            curStamina = weapon.charCtr.stamina;

            int loadedBulletNum = 0;
            if (weapon.weaponData.isChamber) loadedBulletNum++;
            if (weapon.weaponData.isMag) loadedBulletNum += weapon.weaponData.equipMag.loadedBullets.Count;
            for (int i = 0; i < shootNum; i++)
            {
                if (weapon.charCtr.ownerType == CharacterOwner.Player && i == loadedBulletNum) break;
                if (weapon.charCtr.ownerType == CharacterOwner.Enemy && i > weapon.loadedNum) break;

                hitNum = 0;
                ResultHitAccuracys(i);
                weapon.charCtr.SetStamina(-impact);
                var hitText = isHit ? $"{hitNum}발 명중" : "빗나감";
                var hitAccuracyText = pelletAccuracys.Count > 0 ? $"{hitAccuracy}~{pelletAccuracys[^1]}" : $"{hitAccuracy}";
                var hitInfo = new HitInfo()
                {
                    indexName = $"무작위값 = {hitValue}, 명중값 = {hitAccuracyText}, {hitText}",
                    //hitAccuracys = new List<int>(hitAccuracys),
                    hitAccuracy = hitAccuracy,
                    pelletAccuracys = new List<int>(pelletAccuracys),
                    isHit = isHit,
                    hitNum = hitNum,
                    impact = impact,
                };
                weapon.hitInfos.Add(hitInfo);
                Debug.Log($"{weapon.charCtr.name}: {i + 1}번째 탄: {hitText}");
            }
            var useStamina = weapon.charCtr.stamina - curStamina;
            weapon.charCtr.SetStamina(useStamina);

            return weapon.hitInfos.FindAll(x => x.isHit) == null;
        }

        private void ResultHitAccuracys(int index)
        {
            //hitAccuracys.Clear();
            pelletAccuracys.Clear();
            SetUseValue(index);
            CheckStabilityStamina(index);
            ResultHitNum();

            void SetUseValue(int index)
            {
                switch (weapon.charCtr.ownerType)
                {
                    case CharacterOwner.Player:
                        if (index == 0)
                        {
                            if (weapon.weaponData.isChamber)
                            {
                                ApplyValue(weapon.charCtr.stability,
                                           weapon.charCtr.rebound,
                                           weapon.charCtr.propellant,
                                           weapon.weaponData.chamberBullet.pelletNum,
                                           weapon.weaponData.chamberBullet.spread);
                            }
                            else if (weapon.weaponData.isMag && weapon.weaponData.equipMag.loadedBullets.Count > 0)
                            {
                                var bulletIndex = weapon.weaponData.equipMag.loadedBullets.Count - 1;
                                var bullet = weapon.weaponData.equipMag.loadedBullets[bulletIndex];
                                ApplyValue(weapon.charCtr.ability.stability + weapon.weaponData.stability + bullet.stability,
                                           weapon.charCtr.ability.rebound + weapon.weaponData.rebound + bullet.rebound,
                                           weapon.charCtr.ability.propellant + weapon.weaponData.propellant + bullet.propellant,
                                           bullet.pelletNum,
                                           bullet.spread);
                            }
                        }
                        else if (weapon.weaponData.isMag && weapon.weaponData.equipMag.loadedBullets.Count > 0)
                        {
                            var chamber = weapon.weaponData.isChamber ? index : 1 + index;
                            var bulletIndex = weapon.weaponData.equipMag.loadedBullets.Count - chamber;
                            var bullet = weapon.weaponData.equipMag.loadedBullets[bulletIndex];
                            ApplyValue(weapon.charCtr.ability.stability + weapon.weaponData.stability + bullet.stability,
                                       weapon.charCtr.ability.rebound + weapon.weaponData.rebound + bullet.rebound,
                                       weapon.charCtr.ability.propellant + weapon.weaponData.propellant + bullet.propellant,
                                       bullet.pelletNum,
                                       bullet.spread);
                        }
                        break;
                    default:
                        ApplyValue(weapon.charCtr.stability, weapon.charCtr.rebound, weapon.charCtr.propellant, 0, 0);
                        break;
                }

                void ApplyValue(int _stability, int _rebound, int _propellant, int _pelletNum, int _spread)
                {
                    stability = _stability;
                    rebound = _rebound;
                    propellant = _propellant;
                    pelletNum = _pelletNum;
                    spread = _spread;
                }
            }

            void CheckStabilityStamina(int index)
            {
                impact = Mathf.CeilToInt(propellant * 0.1f * 3 / (1 + stability * 0.02f));
                curStamina -= impact;
                bool burnout;
                if (curStamina < 0)
                {
                    curStamina = 0;
                    burnout = true;
                }
                else
                {
                    burnout = false;
                }
                if (index == 0 && !burnout) return;

                if (index == 0)
                {
                    var addAccuracy = Mathf.CeilToInt(distance * 0.01f * (rebound * propellant * 0.01f));
                    if (addAccuracy < 1) addAccuracy = 1;

                    hitAccuracy += addAccuracy * 2;
                }
                else
                {
                    //추가 명중치
                    var addAccuracy = Mathf.CeilToInt(distance * 0.01f * (rebound * propellant * 0.01f));
                    if (addAccuracy < 1) addAccuracy = 1;

                    if (!burnout)
                    {
                        hitAccuracy += addAccuracy;
                    }
                    else
                    {
                        hitAccuracy += addAccuracy * 2;
                    }
                }
            }

            void ResultHitNum()
            {
                isHit = hitAccuracy <= hitValue;
                hitNum = isHit ? 1 : 0;
                //hitAccuracys.Add(hitAccuracy);
                if (pelletNum == 0) return;

                if (isHit)
                {
                    var tempAccuracy = hitAccuracy;
                    for (int i = 1; i < pelletNum; i++)
                    {
                        var pelletSpread = Mathf.FloorToInt(distance * 0.01f * spread);
                        if (pelletSpread < 1) pelletSpread = 1;

                        tempAccuracy += pelletSpread;
                        if (tempAccuracy <= hitValue) hitNum++;
                        pelletAccuracys.Add(tempAccuracy);
                    }
                }
                else
                {
                    var tempAccuracy = hitAccuracy;
                    for (int i = 1; i < pelletNum; i++)
                    {
                        var pelletSpread = Mathf.FloorToInt(distance * 0.01f * spread);
                        if (pelletSpread < 1) pelletSpread = 1;

                        tempAccuracy += pelletSpread;
                        pelletAccuracys.Add(tempAccuracy);
                    }
                }
            }
        }
    }
}
