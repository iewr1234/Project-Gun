using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    public DataManager dataMgr;
    public SceneHandler sceneHlr;
    public CameraManager camMgr;
    public MapEditor mapEdt;
    public InventoryManager invenMgr;

    [HideInInspector] public List<FieldNode> fieldNodes = new List<FieldNode>();

    public void Start()
    {
        dataMgr = FindAnyObjectByType<DataManager>();
        if (dataMgr.gameData.invenMgr == null)
        {
            invenMgr = FindAnyObjectByType<InventoryManager>();
            //invenMgr.SetComponents(this);
            invenMgr.baseMgr = this;
            invenMgr.dataMgr = dataMgr;
            dataMgr.gameData.invenMgr = invenMgr;
        }
        else
        {
            invenMgr = dataMgr.gameData.invenMgr;
            invenMgr.baseMgr = this;
        }
        if (invenMgr.invenCam.enabled)
        {
            invenMgr.ShowInventory();
        }
        camMgr = FindAnyObjectByType<CameraManager>();
        camMgr.SetComponents(this);
        mapEdt = FindAnyObjectByType<MapEditor>();
        mapEdt.SetComponents(this);

        var mapData = dataMgr.LoadMapData("BASECAMP");
        //StartCoroutine(mapEdt.Coroutine_MapLoad(this, mapData, false));
        dataMgr.gameData.mapLoad = false;
        mapEdt.SetActive(false);
    }
}
