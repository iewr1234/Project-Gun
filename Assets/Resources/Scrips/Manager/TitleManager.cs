using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
        dataMgr.gameData.playerID = "P0001";
        sceneHlr.StartLoadScene("StageScene");
    }

    public void Button_MapEditor()
    {
        sceneHlr.StartLoadScene("SampleScene");
    }
}
