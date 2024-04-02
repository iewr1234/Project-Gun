using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum WeaponType
{
    None,
    Pistol,
    Rifle,
}

public class Weapon : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    [SerializeField] private CharacterController charCtr;

    [Header("---Access Component---")]
    [SerializeField] private Transform muzzleTf;

    [Header("--- Assignment Variable---")]
    public WeaponType type;
    public float range;
    public int hitAccuracy;
    public int bulletsPerShot;
    public int damage;
    [Space(5f)]

    public int magMax;
    public int magAmmo;

    [HideInInspector] public bool firstShot;
    [HideInInspector] public bool isHit;

    private readonly Vector3 weaponPos_Rifle = new Vector3(0.1f, 0.05f, 0.015f);
    private readonly Vector3 weaponRot_Rifle = new Vector3(-5f, 95.5f, -95f);

    private readonly float shootDisparity = 0.15f;

    public void SetComponets(CharacterController _charCtr)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        charCtr.weapon = this;
        muzzleTf = transform.Find("Muzzle");

        var mashs = transform.GetComponentsInChildren<MeshRenderer>().ToList();
        DataUtility.SetMeshsMaterial(charCtr.ownerType, mashs);

        WeaponSwitching("Right");
        magAmmo = magMax;
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
        if (isHit && firstShot)
        {
            bullet.transform.LookAt(charCtr.aimPoint.position);
        }
        else
        {
            var aimPos = charCtr.aimPoint.position;
            var random = Random.Range(-shootDisparity, shootDisparity);
            aimPos += charCtr.transform.right * random;
            random = Random.Range(-shootDisparity, shootDisparity);
            aimPos += charCtr.transform.up * random;
            bullet.transform.LookAt(aimPos);
        }
        bullet.SetComponents(this);
        bullet.bulletRb.velocity = bullet.transform.forward * bullet.speed;

        if (firstShot)
        {
            Debug.Log($"{charCtr.name}: isHit = {isHit}");
        }
        firstShot = false;
        magAmmo--;
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
        magAmmo = magMax;
    }
}
