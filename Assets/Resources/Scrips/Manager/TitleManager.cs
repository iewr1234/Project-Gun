using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private DataManager dataMgr;
    private GameMenuManager gameMenuMgr;
    private SceneHandler sceneHlr;

    private void Start()
    {
        dataMgr = FindAnyObjectByType<DataManager>();
        gameMenuMgr = FindAnyObjectByType<GameMenuManager>();
        sceneHlr = FindAnyObjectByType<SceneHandler>();
    }

    public void Button_Start()
    {
        dataMgr.gameData.playerID = "P0001";
        //sceneHlr.StartLoadScene("StageScene");
        dataMgr.gameData.mapName = "BASECAMP";
        dataMgr.gameData.mapLoad = true;
        dataMgr.gameData.baseStorages.Clear();
        dataMgr.gameData.floorStorages.Clear();
        sceneHlr.StartLoadScene("GameScene");
    }

    public void Button_MapEditor()
    {
        sceneHlr.StartLoadScene("GameScene");
    }
}
