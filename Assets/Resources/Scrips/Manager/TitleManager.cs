using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private DataManager dataMgr;
    private InventoryManager invenMgr;
    private SceneHandler sceneHlr;

    private void Start()
    {
        dataMgr = FindAnyObjectByType<DataManager>();
        invenMgr = FindAnyObjectByType<InventoryManager>();
        sceneHlr = FindAnyObjectByType<SceneHandler>();
    }

    public void Button_Start()
    {
        dataMgr.gameData.playerID = "P0001";
        sceneHlr.StartLoadScene("StageScene");
    }

    public void Button_MapEditor()
    {
        sceneHlr.StartLoadScene("SampleScene");
    }
}
