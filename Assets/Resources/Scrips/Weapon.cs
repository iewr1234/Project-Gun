using System.Collections;
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
        int count;
        switch (charCtr.ownerType)
        {
            case CharacterOwner.Player:
                LoadingChamber();
                SetBulletShell();
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
                    bullet.SetBullet(charCtr, target, hitInfo.isHit, count > 1);
                }
                hitInfos.RemoveAt(0);
                weaponData.chamberBullet = null;
                weaponData.isChamber = false;
                if (weaponData.magType != MagazineType.Cylinder) LoadingChamber();
                if (equipSlot != null) equipSlot.SetItemCount();
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
                    bullet.SetBullet(charCtr, target, hitInfo.isHit, count > 1);
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

        void SetBulletDirection(Bullet bullet)
        {
            bullet.gameObject.SetActive(true);
            bullet.transform.position = firePoint.position;
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

        public HitAccuracy(Weapon _weapon, EnemyGearDataInfo _gearInfo)
        {
            weapon = _weapon;
            pelletNum = _gearInfo.pelletNum;
            spread = _gearInfo.spread;
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

                ResultHitAccuracys(shootNum, i);
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
                ResultHitAccuracys(shootNum, i);
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

        private void ResultHitAccuracys(int maxShotNum, int shotNum)
        {
            pelletAccuracys.Clear();

            // 사격자 스텟을 가져옴
            SetUseValue(shotNum);

            // 스테미너 체크
            CheckStabilityStamina(shotNum);

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
                impact = weapon.charCtr.GetUseStamina(Mathf.CeilToInt(propellant * 0.1f * 3 / (1 + stability * 0.02f)));

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
                int addAccuracy = Mathf.CeilToInt((propellant * 0.1f) * (distance * 0.1f) * (rebound * 0.01f) * (1 - ((maxShotNum - 1) / (maxShotNum - 1 + 10))));
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
