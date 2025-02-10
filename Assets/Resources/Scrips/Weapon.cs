using System.Collections.Generic;
using System.Linq;
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
    public List<BodyPartsType> hitParts;
    public int impact;
    public int rebound;
    public int propellant;
}

public class Weapon : MonoBehaviour
{
    private enum AnimationLayers_A
    {
        Base,
        Sub_HG = 1,
        Main_AR = 3,
        Main_SMG = 5,
        Main_Pump = 7,
        Main_SR = 9,
    }

    private struct EjectBullet
    {
        public Mesh mesh;
        public Material mat;
    }

    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    [SerializeField] private CharacterController charCtr;

    [Header("---Access Component---")]
    public Animator animator;
    public List<Renderer> weaponRenderers;

    [Space(5f)] public Transform firePoint;
    public ParticleSystem fx_gunShot;
    public ParticleSystem[] fx_sEjects;
    public ParticleSystemRenderer[] fx_sEject_Rdrs;

    [Space(5f)] public GameObject baseMuzzle;
    public GameObject baseSight;
    public List<Renderer> partsRdrs = new List<Renderer>();

    [Header("--- Assignment Variable---")]
    public EquipSlot equipSlot;
    public WeaponDataInfo weaponData;
    [SerializeField] private Vector3 weaponPivot;
    public WeaponGripInfo gripInfo;
    [Space(5f)]

    [Space(5f)][Tooltip("탄창용량")] public int magMax;
    [Tooltip("장전된 탄환 수")] public int loadedNum;
    private List<EjectBullet> ejectBullets = new List<EjectBullet>();

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

        animator = GetComponent<Animator>();
        if (firePoint == null) firePoint = transform.Find("FirePoint");
        GetWeaponPartsObjects();
        SetHolsterPositionAndRotation();

        if (weaponData.isMag) SetParts(weaponData.equipMag.prefabName, true);
        for (int i = 0; i < weaponData.equipPartsList.Count; i++)
        {
            var partsData = weaponData.equipPartsList[i];
            SetParts(partsData.prefabName, true);
        }
        hitAccuracy = new HitAccuracy(this);
        gameObject.SetActive(true);
    }

    public void SetComponets(CharacterController _charCtr, EnemyGearDataInfo _gearInfo)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        charCtr.weapons.Add(this);
        weaponData.isMain = _gearInfo.isMain;
        weaponData.weaponType = _gearInfo.weaponType;
        weaponData.magType = _gearInfo.magType;
        weaponData.gripType = _gearInfo.gripType;
        weaponData.actionCost_shot = _gearInfo.actionCost_shot;
        weaponData.actionCost_reload = _gearInfo.actionCost_reload;
        gripInfo = DataUtility.GetWeaponGripInfo(weaponData.gripType);

        animator = GetComponent<Animator>();
        if (firePoint == null) firePoint = transform.Find("FirePoint");
        GetWeaponPartsObjects();
        SetHolsterPositionAndRotation();

        magMax = _gearInfo.magMax;
        loadedNum = magMax;
        hitAccuracy = new HitAccuracy(this, _gearInfo);
        gameObject.SetActive(true);
    }

    public List<Renderer> GetWeaponPartsObjects()
    {
        var parts = new List<Renderer>();
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

        partsRdrs = parts;
        return partsRdrs;

        void AddParts(Transform partsTf)
        {
            if (partsTf == null) return;

            for (int i = 0; i < partsTf.childCount; i++)
            {
                var sample = partsTf.GetChild(i).GetComponent<Renderer>();
                if (sample == null) continue;

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

    public void EquipWeapon()
    {
        if (charCtr.baseIndex > 0 && charCtr.upperIndex > 0)
        {
            charCtr.animator.SetLayerWeight(charCtr.baseIndex, 0f);
            charCtr.animator.SetLayerWeight(charCtr.upperIndex, 0f);
        }

        switch (weaponData.weaponType)
        {
            case WeaponType.Pistol:
                charCtr.baseIndex = (int)AnimationLayers_A.Sub_HG;
                break;
            case WeaponType.Revolver:
                charCtr.baseIndex = (int)AnimationLayers_A.Sub_HG;
                break;
            case WeaponType.SubMachineGun:
                charCtr.baseIndex = (int)AnimationLayers_A.Main_SMG;
                break;
            case WeaponType.AssaultRifle:
                charCtr.baseIndex = (int)AnimationLayers_A.Main_AR;
                break;
            case WeaponType.Rifle:
                charCtr.baseIndex = (int)AnimationLayers_A.Main_SR;
                break;
            case WeaponType.SniperRifle:
                charCtr.baseIndex = (int)AnimationLayers_A.Main_SR;
                break;
            case WeaponType.Shotgun:
                switch (weaponData.gripType)
                {
                    case WeaponGripType.Shotgun_PumpAction:
                        charCtr.baseIndex = (int)AnimationLayers_A.Main_Pump;
                        charCtr.animator.SetBool("isPump", true);
                        break;
                    case WeaponGripType.Shotgun_SemiAuto:
                        charCtr.baseIndex = (int)AnimationLayers_A.Main_Pump;
                        charCtr.animator.SetBool("isPump", false);
                        break;
                    case WeaponGripType.Shotgun_FullAuto:
                        charCtr.baseIndex = (int)AnimationLayers_A.Main_AR;
                        charCtr.animator.SetBool("isPump", false);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        charCtr.upperIndex = charCtr.baseIndex + 1;
        charCtr.animator.SetLayerWeight(charCtr.baseIndex, 1f);
        charCtr.animator.SetLayerWeight(charCtr.upperIndex, 1f);
        charCtr.animator.SetBool("isMain", weaponData.isMain);
        charCtr.animator.SetInteger("weaponType", (int)weaponData.weaponType);
        charCtr.animator.SetInteger("magType", (int)weaponData.magType);
        charCtr.animator.SetInteger("gripType", (int)weaponData.gripType);
        charCtr.SetRig(this);
        charCtr.SetWeaponPivot(gripInfo);
        charCtr.SetAbility();
    }

    public void SetParts()
    {
        var activeParts = partsRdrs.FindAll(x => x.gameObject.activeSelf);
        for (int i = 0; i < activeParts.Count; i++)
        {
            var activePart = activeParts[i];
            activePart.gameObject.SetActive(false);
        }

        if (weaponData.isMag)
        {
            var magParts = partsRdrs.Find(x => x.name == weaponData.equipMag.prefabName);
            if (magParts != null) magParts.gameObject.SetActive(true);
        }
        if (baseMuzzle != null) baseMuzzle.SetActive(weaponData.equipPartsList.Find(x => x.type == WeaponPartsType.Muzzle) == null);
        if (baseSight != null) baseSight.SetActive(weaponData.equipPartsList.Find(x => x.type == WeaponPartsType.Sight) == null);
        for (int i = 0; i < weaponData.equipPartsList.Count; i++)
        {
            var equipParts = partsRdrs.Find(x => x.name == weaponData.equipPartsList[i].prefabName);
            if (equipParts != null) equipParts.gameObject.SetActive(true);
        }
    }

    public void SetParts(string partsName, bool value)
    {
        var partsList = partsRdrs.FindAll(x => x.name == partsName);
        for (int i = 0; i < partsList.Count; i++)
        {
            var parts = partsList[i];
            parts.gameObject.SetActive(value);
        }
    }

    public void SetAllParts(string parentsName, bool value)
    {
        var partsList = partsRdrs.FindAll(x => x.transform.parent.name == parentsName);
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
                transform.SetParent(weaponData.isMain ? charCtr.mainHolsterPivot : charCtr.subHolsterPivot, false);
                transform.localPosition = holsterPos;
                transform.localRotation = Quaternion.Euler(holsterRot);
                break;
            case "Right":
                if (transform.parent == charCtr.rightHandPivot) return;

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
                if (transform.parent == charCtr.leftHandPivot) return;

                charCtr.leftHandPivot.transform.position = charCtr.rightHandPivot.transform.position;
                charCtr.leftHandPivot.transform.rotation = charCtr.rightHandPivot.transform.rotation;
                transform.SetParent(charCtr.leftHandPivot, true);
                //charCtr.gripPivot.transform.SetParent(charCtr.leftHandPivot);
                //charCtr.SetChainWeight(true, 0f);
                //charCtr.rigBdr.Build();
                charCtr.moveGripPivot = false;
                if (weaponData.weaponType == WeaponType.Revolver) animator.Play("OpenCylinder", 0, 0f);
                break;
            default:
                break;
        }
    }

    public void FireBullet(CharacterController target)
    {
        var hitInfo = hitInfos[0];
        int bulletNum;
        switch (charCtr.ownerType)
        {
            case CharacterOwner.Player:
                LoadingChamber();
                SetBulletShell();
                var chamberBullet = weaponData.chamberBullet;
                DecreaseDurability(chamberBullet);
                bulletNum = chamberBullet.pelletNum == 0 ? 1 : chamberBullet.pelletNum;
                for (int i = 0; i < hitInfo.hitParts.Count; i++)
                {
                    var bullet = gameMgr.bulletPool.Find(x => !x.gameObject.activeSelf);
                    if (bullet == null)
                    {
                        Debug.LogError("There are no bullet in the bulletPool");
                        return;
                    }

                    SetBulletDirection(bullet);
                    bullet.SetBullet(charCtr, target, hitInfo.hitParts[i], bulletNum > 1);
                }
                hitInfos.RemoveAt(0);
                weaponData.chamberBullet = null;
                weaponData.isChamber = false;
                if (weaponData.magType != MagazineType.Cylinder) LoadingChamber();
                if (equipSlot != null) equipSlot.SetItemCount();
                break;
            case CharacterOwner.Enemy:
                bulletNum = hitAccuracy.pelletNum == 0 ? 1 : hitAccuracy.pelletNum;
                for (int i = 0; i < hitInfo.hitParts.Count; i++)
                {
                    var bullet = gameMgr.bulletPool.Find(x => !x.gameObject.activeSelf);
                    if (bullet == null)
                    {
                        Debug.LogError("There are no bullet in the bulletPool");
                        return;
                    }

                    SetBulletDirection(bullet);
                    bullet.SetBullet(charCtr, target, hitInfo.hitParts[i], bulletNum > 1);
                }
                hitInfos.RemoveAt(0);
                loadedNum--;
                break;
            default:
                break;
        }

        if (fx_gunShot != null) fx_gunShot.Play();
        switch (weaponData.weaponType)
        {
            case WeaponType.Pistol:
                EjectionBulletShell();
                break;
            case WeaponType.SubMachineGun:
                EjectionBulletShell();
                break;
            case WeaponType.AssaultRifle:
                EjectionBulletShell();
                break;
            case WeaponType.Shotgun:
                if (weaponData.magType == MagazineType.Magazine) EjectionBulletShell();
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
            SetBulletShell_Revolver();
            charCtr.SetAbility();
        }

        void DecreaseDurability(BulletDataInfo bulletData)
        {
            int decreaseValue = Mathf.FloorToInt(weaponData.failureRate * 0.1f * ((float)bulletData.level / weaponData.level));
            weaponData.durability -= decreaseValue;
            if (weaponData.durability < 0) weaponData.durability = 0;
        }

        void SetBulletDirection(Bullet bullet)
        {
            bullet.gameObject.SetActive(true);
            bullet.transform.position = firePoint.position;
            float disparity;
            if (bulletNum == 1)
            {
                disparity = shootDisparity_bullet;
                if (hitInfo.hitParts.FindAll(x => x != BodyPartsType.Miss || x != BodyPartsType.Block).Count == 0) disparity += shootDisparity_spread;
            }
            else
            {
                disparity = shootDisparity_pellet;
            }
            var aimPos = charCtr.aimPoint.position;
            aimPos += (charCtr.transform.right * Random.Range(-disparity, disparity)) + (charCtr.transform.up * Random.Range(-disparity, disparity));
            bullet.transform.LookAt(aimPos);
        }
    }

    public bool CheckHitBullet(TargetInfo targetInfo, int shootNum, bool resultShoot)
    {
        if (!resultShoot)
        {
            hitAccuracy.ResultHitBullet(targetInfo, shootNum);
            return false;
        }
        else
        {
            int totalNum = 0;
            int hitNum = 0;
            for (int i = 0; i < hitInfos.Count; i++)
            {
                HitInfo hitInfo = hitInfos[i];
                totalNum += hitInfo.hitParts.Count;
                hitNum += hitInfo.hitParts.FindAll(x => x != BodyPartsType.Miss).Count;
                charCtr.SetStamina(-hitInfo.impact);
            }
            Debug.Log($"{charCtr.name}: 발사수 = {totalNum}, 명중수 = {hitNum}");

            return hitNum == 0;
        }
    }

    public void SetBulletShell()
    {
        if (weaponData.weaponType == WeaponType.Revolver) return;
        if (weaponData.chamberBullet == null) return;

        if (ejectBullets.Count > 0) ejectBullets.Clear();
        EjectBullet ejectBullet = new EjectBullet
        {
            mesh = weaponData.chamberBullet.bulletMesh,
            mat = weaponData.chamberBullet.bulletMat,
        };
        ejectBullets.Add(ejectBullet);
    }

    public void SetBulletShell_Revolver()
    {
        if (weaponData.weaponType != WeaponType.Revolver) return;
        if (ejectBullets.Count == weaponData.equipMag.magSize) return;

        EjectBullet ejectBullet = new EjectBullet
        {
            mesh = weaponData.chamberBullet.bulletMesh,
            mat = weaponData.chamberBullet.bulletMat,
        };
        ejectBullets.Add(ejectBullet);
    }

    public void EjectionBulletShell()
    {
        if (fx_sEjects == null) return;
        if (fx_sEject_Rdrs == null) return;

        for (int i = 0; i < ejectBullets.Count; i++)
        {
            EjectBullet ejectBullet = ejectBullets[i];
            ParticleSystem fx_sEject = fx_sEjects[i];
            ParticleSystemRenderer fx_sEject_Rdr = fx_sEject_Rdrs[i];

            if (weaponData.weaponType != WeaponType.Revolver) fx_sEject.transform.localRotation = Quaternion.Euler(0f, 90f + Random.Range(-5f, 5f), 0f);
            fx_sEject_Rdr.mesh = ejectBullet.mesh;
            fx_sEject_Rdr.material = ejectBullet.mat;
            fx_sEject.Emit(1);
        }
        ejectBullets.Clear();
    }

    public Vector3 GetWeaponCenter()
    {
        if (weaponRenderers.Count == 0) return weaponPivot;

        List<Renderer> renderers = new List<Renderer>(weaponRenderers);
        for (int i = 0; i < partsRdrs.Count; i++)
        {
            if (!partsRdrs[i].gameObject.activeSelf) continue;

            renderers.Add(partsRdrs[i]);
        }

        // 무기 전체를 감싸는 Bounds를 저장할 변수
        Bounds combinedBounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Count; i++)
        {
            // 기존 Bounds에 병합
            combinedBounds.Encapsulate(renderers[i].bounds);
        }


        // 중심 위치 반환 (로컬 좌표 기준)
        var _weaponPivot = -transform.InverseTransformPoint(combinedBounds.center);
        Vector3 itemScale = transform.GetComponent<ItemPivot>().itemPivot.scale;
        if (_weaponPivot.z < -0.25f)
        {
            weaponPivot = _weaponPivot * 0.4f;
            transform.localScale = itemScale - new Vector3(0.1f, 0.1f, 0.1f);
        }
        else
        {
            weaponPivot = _weaponPivot * 0.5f;
            transform.localScale = itemScale;
        }

        return weaponPivot;
    }

    public void Event_EjectionBulletShell()
    {
        EjectionBulletShell();
    }

    private class HitAccuracy
    {
        private Weapon weapon;

        [Tooltip("명중값")] public int hitAccuracy;
        [Tooltip("명중부위")] public List<BodyPartsType> hitParts;
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
        }

        public HitAccuracy(Weapon _weapon, EnemyGearDataInfo _gearInfo)
        {
            weapon = _weapon;
            pelletNum = _gearInfo.pelletNum;
            spread = _gearInfo.spread;
        }

        public void ResultHitBullet(TargetInfo targetInfo, int shootNum)
        {
            weapon.hitInfos.Clear();

            // 사격자 명중률 계산
            distance = DataUtility.GetDistance(targetInfo.shooterNode.transform.position, targetInfo.targetNode.transform.position);
            hitAccuracy = DataUtility.GetHitAccuracy(targetInfo, distance, shootNum);
            curStamina = weapon.charCtr.stamina;

            int loadedBulletNum = 0;
            if (weapon.weaponData.isChamber) loadedBulletNum++;
            if (weapon.weaponData.isMag) loadedBulletNum += weapon.weaponData.equipMag.loadedBullets.Count;
            for (int i = 0; i < shootNum; i++)
            {
                if (weapon.charCtr.ownerType == CharacterOwner.Player && i == loadedBulletNum) break;
                if (weapon.charCtr.ownerType == CharacterOwner.Enemy && i > weapon.loadedNum) break;

                ResultHitAccuracys(targetInfo, shootNum, i);
            }
        }

        private void ResultHitAccuracys(TargetInfo targetInfo, int maxShotNum, int shotNum)
        {
            int _hitAccuracy = hitAccuracy;

            // 사격자 스텟을 가져옴
            SetUseValue(shotNum);

            // 스테미너 체크
            CheckStamina();

            // 단발 or 연사 체크
            CheckSingleOrBurst();

            // 확산 체크
            CheckSpread();

            // 명중 여부 체크
            CheckHit();

            // 사격결과정보 추가
            AddHitInfo();

            void SetUseValue(int index)
            {
                stability = 0;
                rebound = 0;
                propellant = 0;
                pelletNum = 0;
                spread = 0;
                switch (weapon.charCtr.ownerType)
                {
                    case CharacterOwner.Player:
                        if (index == 0 && weapon.weaponData.isChamber)
                        {
                            stability = weapon.charCtr.stability;
                            rebound = weapon.charCtr.rebound;
                            propellant = weapon.charCtr.propellant;
                            pelletNum = weapon.weaponData.chamberBullet.pelletNum;
                            spread = weapon.weaponData.chamberBullet.spread;
                        }
                        else if (weapon.weaponData.isMag && weapon.weaponData.equipMag.loadedBullets.Count > 0)
                        {
                            var chamber = weapon.weaponData.isChamber ? index : 1 + index;
                            var bulletIndex = weapon.weaponData.equipMag.loadedBullets.Count - chamber;
                            var bullet = weapon.weaponData.equipMag.loadedBullets[bulletIndex];
                            stability = weapon.charCtr.ability.stability + weapon.weaponData.stability + bullet.stability;
                            rebound = weapon.charCtr.ability.rebound + weapon.weaponData.rebound + bullet.rebound;
                            propellant = weapon.charCtr.ability.propellant + weapon.weaponData.propellant + bullet.propellant;
                            pelletNum = bullet.pelletNum;
                            spread = bullet.spread;
                        }
                        break;
                    default:
                        stability = weapon.charCtr.stability;
                        rebound = weapon.charCtr.rebound;
                        propellant = weapon.charCtr.propellant;
                        break;
                }
            }

            void CheckStamina()
            {
                // (계산식)반동 충격량
                int recoilImpact = Mathf.CeilToInt((propellant * 0.1f) * 3 / (1 + stability * 0.05f));

                curStamina -= recoilImpact;
            }

            void CheckSingleOrBurst()
            {
                if (maxShotNum == 1) return;

                // (계산식)연사 명중 감소치
                int burstAccuracyDecay = Mathf.CeilToInt((propellant * 0.1f) * (rebound * 0.01f) * (distance * 0.1f));

                _hitAccuracy -= burstAccuracyDecay;
                if (curStamina < 0)
                {
                    _hitAccuracy = Mathf.FloorToInt(_hitAccuracy * 0.5f);
                    curStamina = 0;
                }

                if (_hitAccuracy < DataUtility.minHitAccuracy) _hitAccuracy = DataUtility.minHitAccuracy;
            }

            void CheckSpread()
            {
                if (pelletNum == 0) return;

                // (계산식)확산 명중 감소치
                int spreadAccuracyDecay = Mathf.CeilToInt(distance * 0.1f * spread * 0.1f);

                _hitAccuracy -= spreadAccuracyDecay;
            }

            void CheckHit()
            {
                hitParts = new List<BodyPartsType>();
                int bulletNum = pelletNum == 0 ? 1 : pelletNum;
                for (int i = 0; i < bulletNum; i++)
                {
                    int hitValue = Random.Range(0, 100);
                    bool isHit = _hitAccuracy > hitValue;
                    if (!isHit)
                    {
                        hitParts.Add(BodyPartsType.Miss);
                        continue;
                    }

                    BodyPartsType parts = BodyPartsType.Block;
                    int headAccuracy = 15;
                    int armAccuracy = headAccuracy + 20;
                    int legAccuracy = armAccuracy + 15;
                    int value = Random.Range(0, 100);
                    if (value < headAccuracy)
                    {
                        // 머리에 명중
                        if (CheckPartsCover(BodyPartsType.Body)) parts = BodyPartsType.Head;
                    }
                    else if (value < armAccuracy)
                    {
                        // 팔에 명중
                        BodyPartsType armType = Random.value > 0.5f ? BodyPartsType.RightArm : BodyPartsType.LeftArm;
                        if (CheckPartsCover(armType)) parts = armType;
                    }
                    else if (value < legAccuracy)
                    {
                        // 다리에 명중
                        BodyPartsType legType = Random.value > 0.5f ? BodyPartsType.RightLeg : BodyPartsType.LeftLeg;
                        if (CheckPartsCover(legType)) parts = legType;
                    }
                    else
                    {
                        // 몸통에 명중
                        if (CheckPartsCover(BodyPartsType.Body)) parts = BodyPartsType.Body;
                    }

                    switch (parts)
                    {
                        case BodyPartsType.Miss:
                            hitParts.Add(BodyPartsType.Miss);
                            break;
                        case BodyPartsType.Block:
                            hitParts.Add(BodyPartsType.Block);
                            break;
                        default:
                            hitParts.Add(parts);
                            break;
                    }
                }

                // 부위 엄폐 판정
                bool CheckPartsCover(BodyPartsType partsType)
                {
                    if (targetInfo.targetCover == null) return true;

                    List<BodyPartsType> blockingParts;
                    switch (targetInfo.targetCover.coverType)
                    {
                        case CoverType.Half:
                            blockingParts = new List<BodyPartsType>() { BodyPartsType.Body, BodyPartsType.RightLeg, BodyPartsType.LeftLeg };
                            break;
                        case CoverType.Full:
                            blockingParts = targetInfo.isRight ? new List<BodyPartsType>() { BodyPartsType.Body, BodyPartsType.LeftArm, BodyPartsType.LeftLeg }
                                                               : new List<BodyPartsType>() { BodyPartsType.Body, BodyPartsType.RightArm, BodyPartsType.RightLeg };
                            break;
                        default:
                            return true;
                    }

                    if (blockingParts.Contains(partsType))
                    {
                        return CheckBlockCover(targetInfo);
                    }
                    else
                    {
                        return true;
                    }
                }

                bool CheckBlockCover(TargetInfo targetInfo)
                {
                    int coverPercent = Mathf.FloorToInt(50 - (35 - targetInfo.angle) * 0.7f);
                    int value = Random.Range(0, 100);

                    Debug.Log($"{targetInfo.target.name}: {coverPercent}%");

                    return coverPercent > value;
                }
            }

            void AddHitInfo()
            {
                int missNum = 0;
                int blockNum = 0;
                int headHitNum = 0;
                int bodyHitNum = 0;
                int armHitNum = 0;
                int legHitNum = 0;
                for (int i = 0; i < hitParts.Count; i++)
                {
                    switch (hitParts[i])
                    {
                        case BodyPartsType.Miss:
                            missNum++;
                            break;
                        case BodyPartsType.Block:
                            blockNum++;
                            break;
                        case BodyPartsType.Head:
                            headHitNum++;
                            break;
                        case BodyPartsType.Body:
                            bodyHitNum++;
                            break;
                        case BodyPartsType.RightArm:
                            armHitNum++;
                            break;
                        case BodyPartsType.LeftArm:
                            armHitNum++;
                            break;
                        case BodyPartsType.RightLeg:
                            legHitNum++;
                            break;
                        case BodyPartsType.LeftLeg:
                            legHitNum++;
                            break;
                        default:
                            break;
                    }
                }

                string hitText = null;
                if (headHitNum == 0 && bodyHitNum == 0 && armHitNum == 0 && legHitNum == 0)
                {
                    if (blockNum > missNum)
                    {
                        hitText = "막힘";
                    }
                    else
                    {
                        hitText = "빗나감";
                    }
                }
                else
                {
                    if (headHitNum > 0) hitText += $"머리 {headHitNum}발 ";
                    if (bodyHitNum > 0) hitText += $"몸 {bodyHitNum}발 ";
                    if (armHitNum > 0) hitText += $"팔 {armHitNum}발 ";
                    if (legHitNum > 0) hitText += $"다리 {legHitNum}발 ";
                    hitText += "명중";
                }

                var hitInfo = new HitInfo()
                {
                    indexName = $"{hitText}: 거리 = {distance}, 명중값 = {_hitAccuracy}",
                    hitAccuracy = _hitAccuracy,
                    hitParts = hitParts,
                    impact = impact,
                    rebound = rebound,
                    propellant = propellant,
                };
                weapon.hitInfos.Add(hitInfo);
            }
        }
    }
}
