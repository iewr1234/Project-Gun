using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageIcon : MonoBehaviour
{
    [Header("---Access Script---")]
    private StageManager stageMgr;

    [Header("---Access Component---")]
    private Image image;
    private TextMeshProUGUI nameText;

    [Header("--- Assignment Variable---")]
    private StageDataInfo stageData;

    public void SetComponents(StageManager _stageMgr, StageDataInfo _stageData)
    {
        stageMgr = _stageMgr;

        image = transform.Find("Image").GetComponent<Image>();
        nameText = transform.Find("Name").GetComponent<TextMeshProUGUI>();

        stageData = _stageData;
        nameText.text = $"{stageData.stageName}";
    }

    public void Button_StageIcon()
    {
        stageMgr.EnterTheStage(stageData);
    }
}
