using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string prefabName;
    public string charName;
    public string gearID;
    public string AI_ID;
    public string dropTableID;
    public string uniqueItemID;
    [Space(5f)]

    public int strength;
    public int vitality;
    public int intellect;
    public int wisdom;
    public int agility;
    public int dexterity;
    [Space(5f)]

    public int maxAction;
    public int maxHealth;
    public int maxStamina;
    public float sight;
    public int aiming;
    public int reaction;
    [Space(5f)]

    public List<ShootingModeInfo> sModeInfos;
    [Tooltip("발사속도")] public int RPM;
    [Tooltip("사거리")] public float range;
    [Tooltip("경계각")] public int watchAngle;
    [Tooltip("정확도")] public float MOA;
    [Tooltip("안정성")] public int stability;
    [Tooltip("반동")] public int rebound;
    [Tooltip("장약")] public int propellant;
    [Tooltip("피해량")] public int damage;
    [Tooltip("관통")] public int penetrate;
    [Tooltip("방어구 손상")] public int armorBreak;
    [Tooltip("파편화")] public int critical;
    [Space(5f)]

    [Tooltip("최대 머리 방탄력")] public float maxBP_head;
    [Tooltip("최대 머리 내구도")] public int maxDura_head;
    [Tooltip("최대 방탄력")] public float maxBP_body;
    [Tooltip("최대 내구도")] public int maxDura_body;
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Object/EnemyData")]
public class EnemyData : ScriptableObject
{
    public List<EnemyDataInfo> enemyInfos = new List<EnemyDataInfo>();
}
