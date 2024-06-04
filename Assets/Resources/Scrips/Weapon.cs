using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
    [SerializeField] private Transform bulletTf;

    [HideInInspector] public List<GameObject> partsObjects = new List<GameObject>();

    [Header("--- Assignment Variable---")]
    public WeaponDataInfo weaponData;
    public float weight;
    [Space(5f)]

    [Tooltip("사격타입")] public FireModeType fireMode;
    [Tooltip("자동사격 발사 수")] public int autoFireNum;
    [Space(5f)]

    [Tooltip("탄창용량")] public int magMax;
    [Tooltip("장전된 탄환 수")] public int loadedAmmo;
    [Tooltip("약실 내 탄환 존재 여부")] public bool chamberBullet;

    private List<WeaponPartsDataInfo> partsList = new List<WeaponPartsDataInfo>();
    [SerializeField] private List<bool> hitList = new List<bool>();

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

    private readonly float shootDisparity = 0.15f;

    public void SetComponets(CharacterController _charCtr, WeaponDataInfo _weaponData)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        charCtr.weapons.Add(this);

        bulletTf = transform.Find("BulletTransform");
        AddWeaponPartsObjects();

        weaponData = _weaponData;
        SetWeaponPositionAndRotation();

        magMax = 30;
        Reload();

        void AddWeaponPartsObjects()
        {
            var parts = new List<GameObject>();
            var partsTf = transform.Find("PartsTransform");
            var scopeTf = partsTf.transform.Find("Scope");
            for (int i = 0; i < scopeTf.childCount; i++)
            {
                var sample = scopeTf.GetChild(i).gameObject;
                sample.SetActive(false);
                parts.Add(sample);
            }

            partsObjects = parts;
        }

        void SetWeaponPositionAndRotation()
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
                default:
                    break;
            }
        }
    }

    public void SetComponets()
    {
        var parts = new List<GameObject>();
        var partsTf = transform.Find("PartsTransform");
        var scopeTf = partsTf.transform.Find("Scope");
        for (int i = 0; i < scopeTf.childCount; i++)
        {
            var sample = scopeTf.GetChild(i).gameObject;
            sample.SetActive(false);
            parts.Add(sample);
        }

        partsObjects = parts;
    }

    public void EquipWeapon()
    {
        switch (weaponData.type)
        {
            case WeaponType.Pistol:
                charCtr.animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/Character/Pistol/Pistol");
                break;
            case WeaponType.Rifle:
                charCtr.animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/Character/Rifle/Rifle");
                break;
            default:
                break;
        }
        charCtr.SetRig(weaponData.type);
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

    public int GetShootBulletNumber()
    {
        var shootNum = 0;
        if (chamberBullet)
        {
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
        for (int i = 0; i < shootNum; i++)
        {
            charCtr.SetStamina(-weaponData.stability);
            var hitAccuracy = DataUtility.GetHitAccuracy(charCtr, targetInfo);
            var value = Random.Range(0, 100);
            var isHit = value < hitAccuracy;
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
    }

    public void FireBullet(CharacterController target)
    {
        var bullet = gameMgr.bulletPool.Find(x => !x.gameObject.activeSelf);
        if (bullet == null)
        {
            Debug.LogError("There are no bullet in the bulletPool");
            return;
        }

        bullet.gameObject.SetActive(true);
        bullet.transform.position = bulletTf.position;
        var aimPos = charCtr.aimPoint.position;
        var random = Random.Range(-shootDisparity, shootDisparity);
        aimPos += charCtr.transform.right * random;
        random = Random.Range(-shootDisparity, shootDisparity);
        aimPos += charCtr.transform.up * random;
        bullet.transform.LookAt(aimPos);

        var isHit = hitList[0];
        bullet.SetComponents(this, target, isHit);
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

    public CharacterController CharCtr
    {
        private set { charCtr = value; }
        get { return charCtr; }
    }
}
