using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string prefabName;
    public string charName;
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
    public int health;
    public int maxStamina;
    public float sight;
    public int aiming;
    public int reaction;
    [Space(5f)]

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
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Object/PlayerData")]
public class PlayerData : ScriptableObject
{
    public List<PlayerDataInfo> playerInfos = new List<PlayerDataInfo>();
}
