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

    [Tooltip("�߻�ӵ�")] public int RPM;
    [Tooltip("��Ÿ�")] public float range;
    [Tooltip("��谢")] public int watchAngle;
    [Tooltip("��Ȯ��")] public float MOA;
    [Tooltip("������")] public int stability;
    [Tooltip("�ݵ�")] public int rebound;
    [Tooltip("���")] public int propellant;
    [Tooltip("���ط�")] public int damage;
    [Tooltip("����")] public int penetrate;
    [Tooltip("�� �ջ�")] public int armorBreak;
    [Tooltip("����ȭ")] public int critical;
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Object/PlayerData")]
public class PlayerData : ScriptableObject
{
    public List<PlayerDataInfo> playerInfos = new List<PlayerDataInfo>();
}
