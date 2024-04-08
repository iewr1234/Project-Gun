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

    public int maxHealth;
    public int maxStamina;
    public int sight;
    public int mobility;
    [Space(5f)]

    public string mainWeaponID;
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Object/PlayerData")]
public class PlayerData : ScriptableObject
{
    public List<PlayerDataInfo> playerInfos = new List<PlayerDataInfo>();
}
