using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterDataInfo
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
    public int maxStamina;
    public float sight;
    public int aiming;
    public int reaction;
    [Space(5f)]

    public string mainWeapon1_ID;
    public string mainWeapon2_ID;
    public string subWeapon_ID;
    public string armorID;
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Object/CharacterData")]
public class CharacterData : ScriptableObject
{
    public List<CharacterDataInfo> charInfos = new List<CharacterDataInfo>();
}
