using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
    public int bulletsPerShot;
    public int damage;
    [Space(5f)]

    public int magMax;
    public int magAmmo;

    private readonly Vector3 weaponPos_Rifle = new Vector3(0.1f, 0.05f, 0.015f);
    private readonly Vector3 weaponRot_Rifle = new Vector3(-5f, 95.5f, -95f);

    public void SetComponets(CharacterController _charCtr)
    {
        gameMgr = _charCtr.GameMgr;
        charCtr = _charCtr;
        charCtr.weapon = this;
        muzzleTf = transform.Find("Muzzle");

        var handTf = charCtr.transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_R/Shoulder_R/Elbow_R/Hand_R");
        transform.SetParent(handTf, false);
        transform.localPosition = weaponPos_Rifle;
        transform.localRotation = Quaternion.Euler(weaponRot_Rifle);

        magAmmo = magMax;
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
        bullet.SetComponents(this);
        bullet.transform.position = muzzleTf.position;
        bullet.transform.LookAt(target.aimingPoint);
        bullet.bulletRb.velocity = bullet.transform.forward * bullet.speed;
        magAmmo--;
    }
}
