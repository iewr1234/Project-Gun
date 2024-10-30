using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageIcon : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---")]
    private Image image;
    private TextMeshProUGUI nameText;

    [Header("--- Assignment Variable---")]
    [HideInInspector] public StageDataInfo stageData;

    public void SetComponents(GameManager _gameMgr, StageDataInfo _stageData)
    {
        gameMgr = _gameMgr;

        image = transform.Find("Image").GetComponent<Image>();
        nameText = transform.Find("Name").GetComponent<TextMeshProUGUI>();

        stageData = _stageData;
        nameText.text = $"{stageData.stageName}";
    }

    public void Button_StageIcon()
    {
        if (gameMgr.uiMgr.selcetStage != null) return;

        gameMgr.uiMgr.selcetStage = this;
        if (gameMgr.dataMgr.gameData.floorStorages.Count > 0)
        {
            gameMgr.gameMenuMgr.popUp_warning.SetWarning(WarningState.DeleteDropItems);
        }
        else
        {
            gameMgr.EnterTheStage();
        }
    }
}
