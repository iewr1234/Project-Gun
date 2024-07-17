using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Object/GameData")]
public class GameData : ScriptableObject
{
    [Header("[Map]")]
    public string mapName;
    public bool mapLoad;

    [Header("[Player]")]
    public string playerID;
    public int health;
}