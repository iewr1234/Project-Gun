using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ErrorCodeDataInfo
{
    public string errorID;
    public string errorText;
}

[CreateAssetMenu(fileName = "ErrorCodeData", menuName = "Scriptable Object/ErrorCodeData")]
public class ErrorCodeData : ScriptableObject
{
    public List<ErrorCodeDataInfo> errorCodeInfos = new List<ErrorCodeDataInfo>();
}