using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageIcon : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameUIManager uiMgr;

    [Header("---Access Component---")]
    private Image image;
    private TextMeshProUGUI nameText;

    [Header("--- Assignment Variable---")]
    private StageDataInfo stageData;

    public void SetComponents(GameUIManager _uiMgr, StageDataInfo _stageData)
    {
        uiMgr = _uiMgr;

        image = transform.Find("Image").GetComponent<Image>();
        nameText = transform.Find("Name").GetComponent<TextMeshProUGUI>();

        stageData = _stageData;
        nameText.text = $"{stageData.stageName}";
    }

    public void Button_StageIcon()
    {
        uiMgr.EnterTheStage(stageData);
    }
}
