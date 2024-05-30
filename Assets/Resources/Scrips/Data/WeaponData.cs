using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    None,
    Pistol,
    Rifle,
}

[System.Serializable]
public class WeaponDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string prefabName;
    public string weaponName;
    [Space(5f)]

    [Tooltip("����з�")] public WeaponType type;
    [Tooltip("���ط�")] public int damage;
    [Tooltip("����")] public int penetrate;
    [Tooltip("�� �ջ�")] public int armorBreak;
    [Tooltip("����ȭ")] public int critical;
    [Tooltip("�߻�ӵ�")] public int RPM;
    [Tooltip("��Ÿ�")] public float range;
    [Tooltip("��谢")] public int watchAngle;
    [Tooltip("��Ȯ��")] public float MOA;
    [Tooltip("������")] public int stability;
    [Tooltip("�ݵ�")] public int rebound;
    [Tooltip("�ൿ�Ҹ�")] public int actionCost;
    [Space(5f)]

    [Tooltip("�ѱ� ���")] public List<WeaponPartsSize> useMuzzle;
    [Tooltip("���ذ� ���")] public List<WeaponPartsSize> useScope;
    [Tooltip("źâ ���")] public List<WeaponPartsSize> useMagazine;
    [Tooltip("������ ���")] public List<WeaponPartsSize> useAttachment;
    [Tooltip("����跲 ���")] public List<WeaponPartsSize> useUnderBarrel;
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Object/WeaponData")]
public class WeaponData : ScriptableObject
{
    public List<WeaponDataInfo> weaponInfos = new List<WeaponDataInfo>();
}
