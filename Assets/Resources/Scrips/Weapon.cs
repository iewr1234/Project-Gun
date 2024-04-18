using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FireModeType
{
    SingleFire,
    AutoFire,
}

public class Weapon : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    [SerializeField] private CharacterController charCtr;

    [Header("---Access Component---")]
    [SerializeField] private Transform muzzleTf;

    [HideInInspector] public List<MeshRenderer> meshs = new List<MeshRenderer>();

    [Header("--- Assignment Variable---")]
    [Tooltip("무기분류")] public WeaponType type;
    [Tooltip("피해량")] public int damage;
    [Tooltip("관통")] public int penetrate;
    [Tooltip("방어구 손상")] public int armorBreak;
    [Tooltip("파편화")] public int critical;
    [Tooltip("사거리")] public float range;
    [Tooltip("경계각")] public int watchAngle;
    [Tooltip("정확도")] public float MOA;
    [Tooltip("안정성")] public int stability;
    [Tooltip("반동")] public int rebound;
    [Space(5f)]

    [Tooltip("사격타입")] public FireModeType fireMode;
    [Tooltip("자동사격 발사 수")] public int autoFireNum;
    [Space(5f)]

    [Tooltip("탄창용량")] public int magMax;
    [Tooltip("장전된 탄환 수")] public int loadedAmmo;
    [Tooltip("약실 내 탄환 존재 여부")] public bool chamberBullet;

    [SerializeField] private List<bool> hitList = new List<bool>();

    private Vector3 defaultPos;
    private Vector3 defaultRot;
    private readonly Vector3 weaponPos_Pistol = new Vector3(0.082f, 0.034f, -0.037f);
    private readonly Vector3 weaponRot_Pistol = new Vector3(-8.375f, 89f, -90.246f);
    private readonly Vector3 weaponPos_Rifle = new Vector3(0.1f, 0.05f, 0.015f);
    private readonly Vector3 weaponRot_Rifle = new Vector3(-5f, 95.5f, -95f);

    private readonly float shootDisparity = 0.15f;

    public void SetComponets(CharacterController _charCtr, WeaponDataInfo weaponData)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        charCtr.weapon = this;
        muzzleTf = transform.Find("Muzzle");

        meshs = transform.GetComponentsInChildren<MeshRenderer>().ToList();
        DataUtility.SetMeshsMaterial(charCtr.ownerType, meshs);
        type = weaponData.type;
        switch (type)
        {
            case WeaponType.Pistol:
                charCtr.animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/Character/Pistol/Pistol");
                defaultPos = weaponPos_Pistol;
                defaultRot = weaponRot_Pistol;
                break;
            case WeaponType.Rifle:
                charCtr.animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/Character/Rifle/Rifle");
                defaultPos = weaponPos_Rifle;
                defaultRot = weaponRot_Rifle;
                break;
            default:
                break;
        }
        charCtr.SetRig(type);
        WeaponSwitching("Right");

        damage = weaponData.damage;
        penetrate = weaponData.penetrate;
        armorBreak = weaponData.armorBreak;
        critical = weaponData.critical;
        range = weaponData.range;
        watchAngle = weaponData.watchAngle;
        MOA = weaponData.MOA;
        stability = weaponData.stability;
        rebound = weaponData.rebound;

        magMax = weaponData.magMax;
        Reload();
    }

    public int GetShootBulletNumber()
    {
        var shootNum = 0;
        switch (fireMode)
        {
            case FireModeType.SingleFire:
                shootNum = 1;
                break;
            case FireModeType.AutoFire:
                shootNum = autoFireNum > loadedAmmo + 1 ? loadedAmmo + 1 : autoFireNum;
                break;
            default:
                break;
        }

        return shootNum;
    }

    public bool CheckHitBullet(TargetInfo targetInfo, int shootNum)
    {
        hitList.Clear();
        var pos = targetInfo.shooterNode.transform.position;
        var targetPos = targetInfo.targetNode.transform.position;
        var dist = DataUtility.GetDistance(pos, targetPos);
        var allMiss = true;
        var reboundCheck = 0;
        for (int i = 0; i < shootNum; i++)
        {
            charCtr.stamina -= stability;
            if (charCtr.stamina < 0)
            {
                charCtr.stamina = 0;
                reboundCheck++;
            }
            var value = Random.Range(0, 100);
            var shooterHit = charCtr.aiming - (MOA * dist) + (15 / (dist / 3)) - (rebound * reboundCheck);
            if (shooterHit < 0f)
            {
                shooterHit = 0f;
            }
            var coverBonus = GetCoverBonus();
            var reactionBonus = GetReactionBonus();
            var targetEvasion = coverBonus + (targetInfo.target.reaction * reactionBonus);
            var hitAccuracy = Mathf.Floor((shooterHit - targetEvasion) * 100f) / 100f;
            var isHit = value < hitAccuracy;
            //Debug.Log($"{charCtr.name}: {value} < {hitAccuracy} = {isHit}");
            if (isHit && allMiss)
            {
                allMiss = false;
            }
            hitList.Add(isHit);
        }

        var hit = hitList.FindAll(x => x == true);
        var miss = hitList.FindAll(x => x == false);
        Debug.Log($"{charCtr.name}: ShootNum = {shootNum}, Hit = {hit.Count}, Miss = {miss.Count}");

        return allMiss;

        int GetCoverBonus()
        {
            if (targetInfo.targetCover == null)
            {
                return 0;
            }
            else
            {
                return targetInfo.targetCover.cover.type == CoverType.Full ? 40 : 20;
            }
        }

        float GetReactionBonus()
        {
            if (targetInfo.targetCover == null)
            {
                return 0.1f;
            }
            else
            {
                return targetInfo.targetCover.cover.type == CoverType.Full ? 0.4f : 0.2f;
            }
        }
    }

    public void FireBullet()
    {
        var bullet = gameMgr.bulletPool.Find(x => !x.gameObject.activeSelf);
        if (bullet == null)
        {
            Debug.LogError("There are no bullet in the bulletPool");
            return;
        }

        bullet.gameObject.SetActive(true);
        bullet.transform.position = muzzleTf.position;
        var aimPos = charCtr.aimPoint.position;
        var random = Random.Range(-shootDisparity, shootDisparity);
        aimPos += charCtr.transform.right * random;
        random = Random.Range(-shootDisparity, shootDisparity);
        aimPos += charCtr.transform.up * random;
        bullet.transform.LookAt(aimPos);

        var isHit = hitList[0];
        bullet.SetComponents(this, isHit);
        hitList.RemoveAt(0);
        if (loadedAmmo > 0)
        {
            loadedAmmo--;
        }
        else
        {
            chamberBullet = false;
        }
    }

    public void WeaponSwitching(string switchPos)
    {
        switch (switchPos)
        {
            case "Right":
                transform.SetParent(charCtr.rightHandTf, false);
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
        var ammoNum = magMax;
        if (!chamberBullet)
        {
            ammoNum--;
            chamberBullet = true;
        }
        loadedAmmo = ammoNum;
    }

    public CharacterController CharCtr
    {
        private set { charCtr = value; }
        get { return charCtr; }
    }
}
