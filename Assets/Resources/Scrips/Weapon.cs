using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using UnityEngine;

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
    public Transform bulletTf;
    public Transform gripTf;

    [Space(5f)][SerializeField] private GameObject baseSight;
    [SerializeField] private List<GameObject> partsObjects = new List<GameObject>();

    [Header("--- Assignment Variable---")]
    public EquipSlot equipSlot;
    public WeaponDataInfo weaponData;
    public WeaponGripInfo gripInfo;
    [Space(5f)]

    [HideInInspector] public int meshType;
    [Space(5f)][Tooltip("탄창용량")] public int magMax;
    [Tooltip("장전된 탄환 수")] public int loadedNum;

    private HitAccuracy hitAccuracy;
    public List<HitInfo> hitInfos = new List<HitInfo>();

    private Vector3 holsterPos;
    private Vector3 holsterRot;

    private readonly Vector3 weaponPos_main_rightHolster = new Vector3(-0.19f, -0.21f, -0.2f);
    private readonly Vector3 weaponPos_main_leftHolster = new Vector3(-0.19f, -0.21f, 0.2f);
    private readonly Vector3 weaponRot_main_holster = new Vector3(0f, 90f, 0f);

    private readonly float shootDisparity_bullet = 0.05f;
    private readonly float shootDisparity_pellet = 0.3f;
    private readonly float shootDisparity_spread = 0.15f;

    public void SetComponets(CharacterController _charCtr, EquipSlot _equipSlot, WeaponDataInfo _weaponData)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        equipSlot = _equipSlot;
        weaponData = _weaponData;
        gripInfo = DataUtility.GetWeaponGripInfo(weaponData.gripType);
        charCtr.weapons.Add(this);
        if (charCtr.weapons.Count > 1) charCtr.weapons = charCtr.weapons.OrderBy(x => x.equipSlot.type).ToList();

        if (bulletTf == null) bulletTf = transform.Find("BulletTransform");
        if (gripTf == null) gripTf = transform.Find("GripTransform");
        AddWeaponPartsObjects();
        SetHolsterPositionAndRotation();

        if (weaponData.isMag) SetParts(weaponData.equipMag.magName, true);
        for (int i = 0; i < weaponData.equipPartsList.Count; i++)
        {
            var partsData = weaponData.equipPartsList[i];
            SetParts(partsData.partsName, true);
        }
        hitAccuracy = new HitAccuracy(this);
        gameObject.SetActive(true);
    }

    public void SetComponets(CharacterController _charCtr, EnemyGearDataInfo.WeaponInfo _weaponInfo)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        charCtr.weapons.Add(this);
        weaponData.isMain = _weaponInfo.isMain;
        weaponData.weaponType = _weaponInfo.weaponType;
        weaponData.magType = _weaponInfo.magType;
        weaponData.gripType = _weaponInfo.gripType;
        gripInfo = DataUtility.GetWeaponGripInfo(weaponData.gripType);

        if (bulletTf == null) bulletTf = transform.Find("BulletTransform");
        if (gripTf == null) gripTf = transform.Find("GripTransform");
        AddWeaponPartsObjects();
        SetHolsterPositionAndRotation();

        meshType = _weaponInfo.meshType;
        magMax = _weaponInfo.magMax;
        loadedNum = magMax;
        hitAccuracy = new HitAccuracy(this, _weaponInfo);
        gameObject.SetActive(true);
    }

    private void AddWeaponPartsObjects()
    {
        if (partsObjects.Count > 0) return;

        var parts = new List<GameObject>();
        var partsTf = transform.Find("PartsTransform");

        var magTf = partsTf.Find("Magazine");
        AddParts(magTf);

        var muzzleTf = partsTf.Find("Muzzle");
        AddParts(muzzleTf);

        var sightTf = partsTf.Find("Sight");
        AddParts(sightTf);

        var attachmentTf = partsTf.Find("Attachment");
        if (attachmentTf != null)
        {
            AddParts(attachmentTf.Find("Left"));
            AddParts(attachmentTf.Find("Right"));
        }

        var underBarrelTf = partsTf.Find("UnderBarrel");
        AddParts(underBarrelTf);

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

    private void SetHolsterPositionAndRotation()
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
        }
        else
        {
            holsterPos = Vector3.zero;
            holsterRot = Vector3.zero;
        }
    }

    public void Initialize()
    {
        Destroy(gameObject);

        //charCtr = null;
        //equipSlot = null;
        //weaponData = null;

        //var activeParts = partsObjects.FindAll(x => x.activeSelf);
        //for (int i = 0; i < activeParts.Count; i++)
        //{
        //    var parts = activeParts[i];
        //    parts.SetActive(false);
        //}
        //gameObject.SetActive(false);
    }

    public void EquipWeapon()
    {
        if (charCtr.baseIndex > 0 && charCtr.upperIndex > 0)
        {
            charCtr.animator.SetLayerWeight(charCtr.baseIndex, 0f);
            charCtr.animator.SetLayerWeight(charCtr.upperIndex, 0f);
        }

        charCtr.baseIndex = weaponData.isMain ? (int)AnimationLayers_A.Main_A_Base : (int)AnimationLayers_A.Sub_A_Base;
        charCtr.upperIndex = charCtr.baseIndex + 1;
        charCtr.animator.SetLayerWeight(charCtr.baseIndex, 1f);
        charCtr.animator.SetLayerWeight(charCtr.upperIndex, 1f);
        charCtr.animator.SetBool("isMain", weaponData.isMain);
        charCtr.animator.SetInteger("weaponType", (int)weaponData.weaponType);
        charCtr.animator.SetInteger("magType", (int)weaponData.magType);
        charCtr.SetRig(this);
        charCtr.SetWeaponPivot(gripInfo);
        charCtr.SetAbility();
    }

    public void SetParts()
    {
        var activeParts = partsObjects.FindAll(x => x.activeSelf);
        for (int i = 0; i < activeParts.Count; i++)
        {
            var activePart = activeParts[i];
            activePart.SetActive(false);
        }

        if (weaponData.isMag)
        {
            var magParts = partsObjects.Find(x => x.name == weaponData.equipMag.magName);
            if (magParts != null) magParts.SetActive(true);
        }
        for (int i = 0; i < weaponData.equipPartsList.Count; i++)
        {
            var equipParts = partsObjects.Find(x => x.name == weaponData.equipPartsList[i].prefabName);
            if (equipParts != null) equipParts.SetActive(true);
        }
    }

    public void SetParts(string partsName, bool value)
    {
        var partsList = partsObjects.FindAll(x => x.name == partsName);
        for (int i = 0; i < partsList.Count; i++)
        {
            var parts = partsList[i];
            parts.SetActive(value);
        }
    }

    public void SetAllParts(string parentsName, bool value)
    {
        var partsList = partsObjects.FindAll(x => x.transform.parent.name == parentsName);
        for (int i = 0; i < partsList.Count; i++)
        {
            var parts = partsList[i];
            parts.SetActive(value);
        }
    }

    public void WeaponSwitching(string switchPos)
    {
        switch (switchPos)
        {
            case "Holster":
                transform.SetParent(weaponData.isMain ? charCtr.mainHolsterPivot : charCtr.subHolsterPivot, false);
                transform.localPosition = holsterPos;
                transform.localRotation = Quaternion.Euler(holsterRot);
                break;
            case "Right":
                transform.SetParent(charCtr.rightHandPivot);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                //if (weaponData.isMain)
                //{
                //    charCtr.gripPivot.transform.SetParent(charCtr.rightHandPivot);
                //    charCtr.gripPivot.SetLocalPositionAndRotation(gripInfo.gripPos, gripInfo.gripRot);
                //    charCtr.rigBdr.Build();
                //    charCtr.SetChainWeight(true, 1f);
                //}
                charCtr.moveGripPivot = true;
                break;
            case "Left":
                transform.SetParent(charCtr.leftHandPivot, true);
                //charCtr.gripPivot.transform.SetParent(charCtr.leftHandPivot);
                //charCtr.SetChainWeight(true, 0f);
                //charCtr.rigBdr.Build();
                charCtr.moveGripPivot = false;
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

                    SetBulletDirection(bullet);
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
                    SetBulletDirection(bullet);
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

        void SetBulletDirection(Bullet bullet)
        {
            bullet.gameObject.SetActive(true);
            bullet.transform.position = bulletTf.position;
            float disparity;
            if (count == 1)
            {
                disparity = shootDisparity_bullet;
                if (!hitInfo.isHit) disparity += shootDisparity_spread;
            }
            else
            {
                disparity = shootDisparity_pellet;
            }
            var aimPos = charCtr.aimPoint.position;
            aimPos.y += 0.3f;
            aimPos += (charCtr.transform.right * Random.Range(-disparity, disparity)) + (charCtr.transform.up * Random.Range(-disparity, disparity));
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

    private class HitAccuracy
    {
        private Weapon weapon;

        [Tooltip("명중값")] public int hitAccuracy;
        [Tooltip("확산명중치")] private int spreadAccuracy;
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
            pelletAccuracys = new List<int>();
        }

        public HitAccuracy(Weapon _weapon, EnemyGearDataInfo.WeaponInfo _weaponInfo)
        {
            weapon = _weapon;
            pelletNum = _weaponInfo.pelletNum;
            spread = _weaponInfo.spread;
            pelletAccuracys = new List<int>();
        }

        public void CheckHitBullet_Aim(TargetInfo targetInfo, int shootNum)
        {
            weapon.hitInfos.Clear();

            // 사격자 명중률 계산
            distance = DataUtility.GetDistance(targetInfo.shooterNode.transform.position, targetInfo.targetNode.transform.position);
            hitAccuracy = 100 - DataUtility.GetHitAccuracy(targetInfo, distance, shootNum);
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
                    hitAccuracy = hitAccuracy - spreadAccuracy,
                    pelletAccuracys = new List<int>(pelletAccuracys),
                    impact = impact,
                };
                weapon.hitInfos.Add(hitInfo);
            }
        }

        public bool CheckHitBullet_Shoot(TargetInfo targetInfo, int shootNum)
        {
            weapon.hitInfos.Clear();

            // 랜덤값
            hitValue = Random.Range(1, 101);

            // 사격자 명중률 계산
            distance = DataUtility.GetDistance(targetInfo.shooterNode.transform.position, targetInfo.targetNode.transform.position);
            hitAccuracy = 100 - DataUtility.GetHitAccuracy(targetInfo, distance, shootNum);
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
                    hitAccuracy = hitAccuracy - spreadAccuracy,
                    pelletAccuracys = new List<int>(pelletAccuracys),
                    isHit = isHit,
                    hitNum = hitNum,
                    impact = impact,
                };
                weapon.hitInfos.Add(hitInfo);
            }
            var hitCount = weapon.hitInfos.FindAll(x => x.isHit).Count;
            Debug.Log($"{weapon.charCtr.name}: 발사수 = {weapon.hitInfos.Count}, 명중수 = {hitCount}");
            var useStamina = weapon.charCtr.stamina - curStamina;
            weapon.charCtr.SetStamina(useStamina);

            return hitCount == 0;
        }

        private void ResultHitAccuracys(int index)
        {
            pelletAccuracys.Clear();

            // 사격자 스텟을 가져옴
            SetUseValue(index);

            // 스테미너 체크
            CheckStabilityStamina(index);

            // 추가 확산 명중치
            GetSpreadAccuracy();

            // 명중하는 총알수 체크
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
                // 충격량
                impact = Mathf.CeilToInt(propellant * 0.1f * 3 / (1 + stability * 0.02f));

                // 현재 스테미너 - 충격량
                curStamina -= impact;

                // 스테미너 체크
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

                // 추가 명중치
                var addAccuracy = Mathf.CeilToInt(distance * 0.01f * (rebound * propellant * 0.01f));
                if (addAccuracy < 1) addAccuracy = 1;

                if (index == 0)
                {
                    hitAccuracy += addAccuracy * 2;
                }
                else
                {
                    hitAccuracy = !burnout ? hitAccuracy + addAccuracy : hitAccuracy + (addAccuracy * 2);
                }
            }

            void GetSpreadAccuracy()
            {
                if (pelletNum == 0)
                {
                    spreadAccuracy = 0;
                }
                else
                {
                    // 확산 명중 보정
                    spreadAccuracy = Mathf.FloorToInt(distance * 0.01f * spread) * 2;
                    if (spreadAccuracy > pelletNum * 2) spreadAccuracy = pelletNum * 2;
                }
            }

            void ResultHitNum()
            {
                isHit = hitAccuracy - spreadAccuracy <= hitValue;
                hitNum = isHit ? 1 : 0;
                if (pelletNum == 0) return;

                var tempAccuracy = hitAccuracy - spreadAccuracy;
                var pelletSpread = Mathf.FloorToInt(distance * 0.01f * spread);
                if (pelletSpread < 1) pelletSpread = 1;

                if (isHit)
                {
                    for (int i = 1; i < pelletNum; i++)
                    {
                        tempAccuracy += pelletSpread;
                        if (tempAccuracy <= hitValue) hitNum++;

                        pelletAccuracys.Add(tempAccuracy);
                    }
                }
                else
                {
                    for (int i = 1; i < pelletNum; i++)
                    {
                        tempAccuracy += pelletSpread;
                        pelletAccuracys.Add(tempAccuracy);
                    }
                }
            }
        }
    }
}
