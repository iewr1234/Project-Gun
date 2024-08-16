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
    public string aiID;
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

    public string headID;
    public string bodyID;
    public string mainWeapon1_ID;
    public string mainBullet1_ID;
    public string mainWeapon2_ID;
    public string mainBullet2_ID;
    public string subWeapon_ID;
    public string subBullet_ID;
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Object/EnemyData")]
public class EnemyData : ScriptableObject
{
    public List<EnemyDataInfo> enemyInfos = new List<EnemyDataInfo>();
}
