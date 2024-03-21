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
    [SerializeField] private CharacterController charCtr;

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
        charCtr = _charCtr;
        charCtr.weapon = this;

        var handTf = charCtr.transform.Find("Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_R/Shoulder_R/Elbow_R/Hand_R");
        transform.SetParent(handTf, false);
        transform.localPosition = weaponPos_Rifle;
        transform.localRotation = Quaternion.Euler(weaponRot_Rifle);

        magAmmo = magMax;
    }

    public void FireBullet(CharacterController target)
    {
        magAmmo--;
    }
}
