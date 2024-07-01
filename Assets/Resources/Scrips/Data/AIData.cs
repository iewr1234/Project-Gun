using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UseActionType
{
    None,
    Shoot,
    Aim,
    Rest,
}

[System.Serializable]
public class AIDataInfo
{
    public string indexName;
    [Space(5f)]

    public string ID;
    public string typeName;
    [Space(5f)]

    public UseActionType actionType;
    public int score_move;
    public int score_noneCover;
    public int score_halfCover;
    public int score_fullCover;
    public int score_noneShoot;
    public int score_halfShoot;
    public int score_fullShoot;
}

[CreateAssetMenu(fileName = "AIData", menuName = "Scriptable Object/AIData")]
public class AIData : ScriptableObject
{
    public List<AIDataInfo> aiInfos = new List<AIDataInfo>();
}