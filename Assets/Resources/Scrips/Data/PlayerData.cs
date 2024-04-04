using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerDataInfo
{
    public string indexName;
    public string ID;
    public string prefabName;
    public string charName;
    public int mobility;
    public int maxHealth;
    public string mainWeaponID;
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Object/PlayerData")]
public class PlayerData : ScriptableObject
{
    public List<PlayerDataInfo> playerInfos = new List<PlayerDataInfo>();
}
