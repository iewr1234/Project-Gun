using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum MapEditorState
{
    None,
    SetNode,
    CreatePlayer,
}

public class MapEditor : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---")]
    [SerializeField] private MapEditorState state;
    private GameObject activeUI;

    [Header("[Set Node]")]
    private Transform fieldNodeTf;
    private GameObject setNodeUI;
    private TMP_InputField xSize;
    private TMP_InputField ySize;

    [Header("[Create Player]")]
    private Transform characterTf;
    private GameObject createPlayerUI;

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;

        fieldNodeTf = GameObject.FindGameObjectWithTag("FieldNodes").transform;
        setNodeUI = transform.Find("UI/SetNode").gameObject;
        xSize = transform.Find("UI/SetNode/InputFields/Size_X/InputField_Value").GetComponent<TMP_InputField>();
        ySize = transform.Find("UI/SetNode/InputFields/Size_Y/InputField_Value").GetComponent<TMP_InputField>();
        setNodeUI.gameObject.SetActive(false);

        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;
    }

    private void Update()
    {
        if (state == MapEditorState.None) return;

        switch (state)
        {
            case MapEditorState.CreatePlayer:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 필드 생성
    /// </summary>
    /// <param name="size_X"></param>
    /// <param name="size_Y"></param>
    private void CreateField(int size_X, int size_Y)
    {
        if (gameMgr.fieldNodes.Count > 0)
        {
            for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
            {
                var node = gameMgr.fieldNodes[i];
                Destroy(node.gameObject);
            }
            gameMgr.fieldNodes.Clear();
        }

        var size = DataUtility.nodeSize;
        var interval = DataUtility.nodeInterval;
        for (int i = 0; i < size_Y; i++)
        {
            for (int j = 0; j < size_X; j++)
            {
                var fieldNode = Instantiate(Resources.Load<FieldNode>("Prefabs/FieldNode"));
                fieldNode.transform.SetParent(fieldNodeTf, false);
                var pos = new Vector3((j * size) + (j * interval), 0f, (i * size) + (i * interval));
                fieldNode.transform.position = pos;
                fieldNode.SetComponents(gameMgr, new Vector2(j, i));
                fieldNode.NodeColor = Color.gray;
                gameMgr.fieldNodes.Add(fieldNode);
            }
        }

        for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
        {
            var node = gameMgr.fieldNodes[i];
            node.AddAdjacentNodes();
        }
    }

    private void OnInterface(MapEditorState _state, GameObject _activeUI)
    {
        switch (_activeUI.activeSelf)
        {
            case true:
                _activeUI.SetActive(false);
                activeUI = null;
                state = MapEditorState.None;
                break;
            case false:
                if (activeUI != null)
                {
                    activeUI.SetActive(false);
                }
                activeUI = _activeUI;
                activeUI.SetActive(true);
                state = _state;
                break;
        }
    }

    #region Button Event
    public void Button_SetNode()
    {
        OnInterface(MapEditorState.SetNode, setNodeUI);
    }

    public void Button_SetNode_Create()
    {
        if (xSize.text.Length == 0 || ySize.text.Length == 0) return;

        CreateField(int.Parse(xSize.text), int.Parse(ySize.text));
        activeUI.gameObject.SetActive(false);
        activeUI = null;
    }

    public void Button_CreatePlayer()
    {
        OnInterface(MapEditorState.CreatePlayer, setNodeUI);
    }
    #endregion
}