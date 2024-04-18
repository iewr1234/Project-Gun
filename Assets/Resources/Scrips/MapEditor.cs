using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Sockets;

public enum MapEditorState
{
    None,
    Node,
    Player,
    Enemy,
}

public enum FindNodeType
{
    None,
    Create,
    Delete,
}

public class MapEditor : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---")]
    [SerializeField] private MapEditorState state;
    private GameObject activeUI;

    private FindNodeType findType;
    private FieldNode selectNode;

    [Header("[Set Node]")]
    private Transform fieldNodeTf;
    private GameObject nodeUI;
    private TMP_InputField xSize;
    private TMP_InputField ySize;

    [Header("[Create Player]")]
    private Transform characterTf;
    private GameObject playerUI;
    private List<FieldNode> pMarkerNodes = new List<FieldNode>();

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;

        fieldNodeTf = GameObject.FindGameObjectWithTag("FieldNodes").transform;
        nodeUI = transform.Find("UI/Node").gameObject;
        xSize = transform.Find("UI/Node/InputFields/Size_X/InputField_Value").GetComponent<TMP_InputField>();
        ySize = transform.Find("UI/Node/InputFields/Size_Y/InputField_Value").GetComponent<TMP_InputField>();
        nodeUI.SetActive(false);

        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;
        playerUI = transform.Find("UI/Player").gameObject;
        playerUI.SetActive(false);
    }

    private void Update()
    {
        if (findType == FindNodeType.None) return;

        switch (state)
        {
            case MapEditorState.Player:
                InputEvent();
                break;
            default:
                break;
        }

        void InputEvent()
        {
            KeyboardInput();
            MouseInput();
            PointerUpEvent();
        }
    }

    private void KeyboardInput()
    {
        switch (state)
        {
            case MapEditorState.Player:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (selectNode != null)
                    {
                        selectNode.SetNodeOutLine(false);
                        selectNode = null;
                    }
                    findType = FindNodeType.None;
                }
                break;
            default:
                break;
        }
    }

    private void MouseInput()
    {
        if (findType == FindNodeType.None) return;

        if (Input.GetMouseButtonDown(0) && selectNode != null)
        {
            selectNode.SetNodeOutLine(false);
            switch (state)
            {
                case MapEditorState.Player:
                    SetMarker();
                    break;
                default:
                    break;
            }
            selectNode = null;
            findType = FindNodeType.None;
        }

        void SetMarker()
        {
            var charType = state == MapEditorState.Player ? CharacterOwner.Player : CharacterOwner.Enemy;
            var markerNodes = state == MapEditorState.Player ? pMarkerNodes : null;
            switch (findType)
            {
                case FindNodeType.Create:
                    selectNode.SetMarker(charType, pMarkerNodes.Count);
                    markerNodes.Add(selectNode);
                    break;
                case FindNodeType.Delete:
                    selectNode.SetMarker();
                    markerNodes.Remove(selectNode);
                    for (int i = 0; i < markerNodes.Count; i++)
                    {
                        var markerNode = markerNodes[i];
                        markerNode.SetMarker(charType, i);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void PointerUpEvent()
    {
        if (findType == FindNodeType.None) return;

        var ray = gameMgr.camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gameMgr.nodeLayer))
        {
            var node = hit.collider.GetComponentInParent<FieldNode>();
            if (node == null) return;

            if (selectNode != null && node != selectNode)
            {
                selectNode.SetNodeOutLine(false);
            }
            switch (findType)
            {
                case FindNodeType.Create:
                    CreateMarker(node);
                    break;
                case FindNodeType.Delete:
                    DeleteMarker(node);
                    break;
                default:
                    break;
            }
        }
        else if (selectNode != null)
        {
            selectNode.SetNodeOutLine(false);
            selectNode = null;
        }

        void CreateMarker(FieldNode node)
        {
            if (node.canMove)
            {
                switch (state)
                {
                    case MapEditorState.Player:
                        node.SetNodeOutLine(true);
                        break;
                    default:
                        break;
                }
                selectNode = node;
            }
            else
            {
                selectNode = null;
            }
        }

        void DeleteMarker(FieldNode node)
        {
            var markerNodes = state == MapEditorState.Player ? pMarkerNodes : null;
            var find = markerNodes.Find(x => x == node);
            if (find != null)
            {
                node.SetNodeOutLine(true);
                selectNode = node;
            }
            else if (selectNode != null)
            {
                selectNode.SetNodeOutLine(false);
                selectNode = null;
            }
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
    public void Button_Node()
    {
        OnInterface(MapEditorState.Node, nodeUI);
    }

    public void Button_Node_Create()
    {
        if (xSize.text.Length == 0 || ySize.text.Length == 0) return;

        CreateField(int.Parse(xSize.text), int.Parse(ySize.text));
        activeUI.gameObject.SetActive(false);
        activeUI = null;
    }

    public void Button_Player()
    {
        OnInterface(MapEditorState.Player, playerUI);
    }

    public void Button_Player_CreateMarker()
    {
        findType = FindNodeType.Create;
    }

    public void Button_Player_DeleteMarker()
    {
        findType = FindNodeType.Delete;
    }
    #endregion
}