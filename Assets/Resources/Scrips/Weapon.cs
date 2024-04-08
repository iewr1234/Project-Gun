using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using UnityEngine;

public enum WeaponType
{
    None,
    Pistol,
    Rifle,
}

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
    [Tooltip("명중률")] public int hitAccuracy;
    [Space(5f)]

    [Tooltip("사격타입")] public FireModeType fireMode;
    [Tooltip("자동사격 발사 수")] public int autoFireNum;
    [Space(5f)]

    [Tooltip("탄창용량")] public int magMax;
    [Tooltip("장전된 탄환 수")] public int loadedAmmo;
    [Tooltip("약실 내 탄환 존재 여부")] public bool chamberBullet;

    [SerializeField] private List<bool> hitList = new List<bool>();

    private readonly Vector3 weaponPos_Rifle = new Vector3(0.1f, 0.05f, 0.015f);
    private readonly Vector3 weaponRot_Rifle = new Vector3(-5f, 95.5f, -95f);

    private readonly float shootDisparity = 0.15f;

    public void SetComponets(CharacterController _charCtr)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        charCtr.weapon = this;
        muzzleTf = transform.Find("Muzzle");

        meshs = transform.GetComponentsInChildren<MeshRenderer>().ToList();
        DataUtility.SetMeshsMaterial(charCtr.ownerType, meshs);

        WeaponSwitching("Right");
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

    public bool CheckHitBullet(int shootNum)
    {
        hitList.Clear();
        var allMiss = true;
        for (int i = 0; i < shootNum; i++)
        {
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
                transform.localPosition = weaponPos_Rifle;
                transform.localRotation = Quaternion.Euler(weaponRot_Rifle);
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
}
