using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private DataManager dataMgr;
    private SceneHandler sceneHlr;

    private void Start()
    {
        dataMgr = FindAnyObjectByType<DataManager>();
        sceneHlr = FindAnyObjectByType<SceneHandler>();
    }

    public void Button_Start()
    {
        dataMgr.gameData.mapName = "M0001";
        dataMgr.gameData.playerID = "P0001";
        dataMgr.gameData.mapLoad = true;
        sceneHlr.StartLoadScene("SampleScene");
    }
}
