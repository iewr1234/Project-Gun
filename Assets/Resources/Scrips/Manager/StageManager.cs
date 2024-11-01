using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private DataManager dataMgr;
    private SceneHandler sceneHlr;

    [Header("--- Assignment Variable---")]
    private List<StageIcon> stageIcons = new List<StageIcon>();
    private bool selcetStage;

    private void Start()
    {
        dataMgr = FindAnyObjectByType<DataManager>();
        sceneHlr = FindAnyObjectByType<SceneHandler>();
        sceneHlr.EndLoadScene();

        stageIcons = GameObject.FindGameObjectWithTag("StageUI").transform.Find("StageIcons").GetComponentsInChildren<StageIcon>().ToList();
        for (int i = 0; i < stageIcons.Count; i++)
        {
            var stageIcon = stageIcons[i];
            var stageData = dataMgr.stageData.stageInfos.Find(x => x.ID == "S0001");
            //stageIcon.SetComponents(this, stageData);
        }
    }

    public void EnterTheStage(StageDataInfo stageData)
    {
        if (selcetStage) return;

        selcetStage = true;
        dataMgr.gameData.stageData = stageData.CopyData();
        dataMgr.gameData.RandomMapSelection();
        sceneHlr.StartLoadScene("GameScene");
    }
}
