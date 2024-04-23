using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum MapEditorType
{
    None,
    Node,
    Player,
    Enemy,
    Floor,
    HalfCover,
    FullCover,
    FloorObject,
}

public enum FindNodeType
{
    None,
    CreateMarker,
    DeleteMarker,
    SetFloor,
    SetObject,
}

public class MapEditor : MonoBehaviour
{
    private enum InterfaceType
    {
        None,
        Top,
        Side,
    }

    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---")]
    private Transform fieldNodeTf;
    private Transform characterTf;

    #region Top
    [Header("[Node]")]
    private GameObject nodeUI;
    private TMP_InputField xSize;
    private TMP_InputField ySize;

    [Header("[Player]")]
    private GameObject playerUI;
    private List<FieldNode> pMarkerNodes = new List<FieldNode>();

    [Header("[Enemy]")]
    private GameObject enemyUI;
    private List<FieldNode> eMarkerNodes = new List<FieldNode>();
    #endregion

    #region Side
    private GameObject sideButtons;
    private GameObject sideUI;
    private TextMeshProUGUI setDirText;
    private TextMeshProUGUI floorRandomText;
    private TextMeshProUGUI gridSwitchText;
    private bool onSideButton;

    [Header("[Floor]")]
    private GameObject floorUI;
    private bool floorDirRandom;
    private List<FieldNode> setFloorNodes = new List<FieldNode>();

    [Header("[HalfCover]")]
    private GameObject halfCoverUI;

    [Header("[FullCover]")]
    private GameObject fullCoverUI;

    [Header("[FloorObject]")]
    private GameObject floorObjectUI;
    #endregion

    [Header("--- Assignment Variable---")]
    [SerializeField] private MapEditorType editType;
    private InterfaceType activeType;
    private GameObject activeUI;

    private FindNodeType findType;
    private FieldNode selectNode;
    private List<FieldNode> selectNodes = new List<FieldNode>();

    [Space(5f)]
    public MapItem selectItem;
    [SerializeField] private TargetDirection setDirection;

    private Vector3 sidePos_On;
    private Vector3 sidePos_Off;

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;

        fieldNodeTf = GameObject.FindGameObjectWithTag("FieldNodes").transform;
        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;

        nodeUI = transform.Find("Top/UI/Node").gameObject;
        xSize = transform.Find("Top/UI/Node/InputFields/Size_X/InputField_Value").GetComponent<TMP_InputField>();
        ySize = transform.Find("Top/UI/Node/InputFields/Size_Y/InputField_Value").GetComponent<TMP_InputField>();
        nodeUI.SetActive(false);

        playerUI = transform.Find("Top/UI/Player").gameObject;
        playerUI.SetActive(false);

        enemyUI = transform.Find("Top/UI/Enemy").gameObject;
        enemyUI.SetActive(false);

        sideButtons = transform.Find("Side/Buttons").gameObject;
        sideUI = transform.Find("Side/UI").gameObject;
        setDirText = sideUI.transform.Find("SetDirectionText").GetComponent<TextMeshProUGUI>();
        floorRandomText = sideUI.transform.Find("FloorRandom/Text").GetComponent<TextMeshProUGUI>();
        gridSwitchText = sideUI.transform.Find("GridSwitch/Text").GetComponent<TextMeshProUGUI>();
        sidePos_On = sideButtons.transform.localPosition;
        var width = sideUI.GetComponent<RectTransform>().rect.width;
        sidePos_Off = sidePos_On + new Vector3(width, 0f, 0f);
        sideButtons.transform.localPosition = sidePos_Off;
        sideUI.transform.localPosition = sidePos_Off;

        floorUI = transform.Find("Side/UI/Floor").gameObject;
        floorUI.SetActive(false);

        halfCoverUI = transform.Find("Side/UI/HalfCover").gameObject;
        halfCoverUI.SetActive(false);

        fullCoverUI = transform.Find("Side/UI/FullCover").gameObject;
        fullCoverUI.SetActive(false);

        floorObjectUI = transform.Find("Side/UI/FloorObject").gameObject;
        floorObjectUI.SetActive(false);
    }

    private void Update()
    {
        if (findType == FindNodeType.None) return;

        switch (editType)
        {
            case MapEditorType.Player:
                InputEvent();
                break;
            case MapEditorType.Enemy:
                InputEvent();
                break;
            case MapEditorType.Floor:
                InputEvent();
                break;
            case MapEditorType.HalfCover:
                InputEvent();
                break;
            case MapEditorType.FullCover:
                InputEvent();
                break;
            case MapEditorType.FloorObject:
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
        switch (editType)
        {
            case MapEditorType.Player:
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
        var canInput = !onSideButton && findType != FindNodeType.None && selectNode != null;
        if (!canInput) return;

        if (Input.GetMouseButtonUp(0))
        {
            setFloorNodes.Clear();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            switch (editType)
            {
                case MapEditorType.Player:
                    SetMarker();
                    break;
                case MapEditorType.Enemy:
                    SetMarker();
                    break;
                default:
                    break;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            Debug.Log("Click");
            switch (editType)
            {
                case MapEditorType.Floor:
                    SetFloor(true);
                    break;
                case MapEditorType.HalfCover:
                    SetObject(true);
                    break;
                case MapEditorType.FullCover:
                    SetObject(true);
                    break;
                case MapEditorType.FloorObject:
                    SetObject(true);
                    break;
                default:
                    break;
            }
        }
        else if (Input.GetMouseButton(1))
        {
            switch (editType)
            {
                case MapEditorType.Floor:
                    SetFloor(false);
                    break;
                case MapEditorType.HalfCover:
                    SetObject(false);
                    break;
                case MapEditorType.FullCover:
                    SetObject(false);
                    break;
                case MapEditorType.FloorObject:
                    SetObject(false);
                    break;
                default:
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(2) && findType == FindNodeType.SetObject)
        {
            switch (setDirection)
            {
                case TargetDirection.Left:
                    setDirection = TargetDirection.Front;
                    break;
                case TargetDirection.Front:
                    setDirection = TargetDirection.Right;
                    break;
                case TargetDirection.Back:
                    setDirection = TargetDirection.Left;
                    break;
                case TargetDirection.Right:
                    setDirection = TargetDirection.Back;
                    break;
                default:
                    break;
            }
            setDirText.text = $"배치방향 : {setDirection}";

            if (selectNode != null)
            {
                if (selectItem.size.x == 1 && selectItem.size.y == 1)
                {
                    selectNode.SetNodeOutLine(setDirection);
                }
                else
                {
                    ClearSelectNodes();
                    HighlightNodes(selectNode);
                }
            }
        }

        void SetMarker()
        {
            selectNode.SetNodeOutLine(false);
            var charType = editType == MapEditorType.Player ? CharacterOwner.Player : CharacterOwner.Enemy;
            var markerNodes = editType == MapEditorType.Player ? pMarkerNodes : eMarkerNodes;
            switch (findType)
            {
                case FindNodeType.CreateMarker:
                    selectNode.SetMarker(charType, markerNodes.Count);
                    markerNodes.Add(selectNode);
                    break;
                case FindNodeType.DeleteMarker:
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
            selectNode = null;
            findType = FindNodeType.None;
        }

        void SetFloor(bool value)
        {
            switch (value)
            {
                case true:
                    if (selectItem != null && setFloorNodes.Find(x => x == selectNode) == null)
                    {
                        selectNode.SetOnNodeMesh(selectItem, floorDirRandom);
                        setFloorNodes.Add(selectNode);
                    }
                    break;
                case false:
                    selectNode.SetOffNodeMesh();
                    break;
            }
        }

        void SetObject(bool value)
        {
            switch (value)
            {
                case true:
                    if (selectItem != null)
                    {
                        if (selectItem.size.x == 1 && selectItem.size.y == 1)
                        {
                            selectNode.SetOnObject(selectItem, setDirection);
                        }
                        else
                        {
                            selectNode.SetOnObject(selectNodes, selectItem, setDirection);
                        }
                    }
                    break;
                case false:
                    selectNode.SetOffObject(editType);
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
            Debug.Log(hit.collider.name);
            var node = hit.collider.GetComponentInParent<FieldNode>();
            if (node == null) return;

            if (selectNode != null && node != selectNode)
            {
                selectNode.SetNodeOutLine(false);
                ClearSelectNodes();
            }

            switch (findType)
            {
                case FindNodeType.CreateMarker:
                    CreateMarker(node);
                    break;
                case FindNodeType.DeleteMarker:
                    DeleteMarker(node);
                    break;
                case FindNodeType.SetFloor:
                    HighlightNode(node);
                    break;
                case FindNodeType.SetObject:
                    HighlightNode(node);
                    break;
                default:
                    break;
            }
        }
        else if (selectNode != null)
        {
            selectNode.SetNodeOutLine(false);
            selectNode = null;
            ClearSelectNodes();
        }

        void CreateMarker(FieldNode node)
        {
            if (node.canMove)
            {
                switch (editType)
                {
                    case MapEditorType.Player:
                        node.SetNodeOutLine(true);
                        break;
                    case MapEditorType.Enemy:
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
            var markerNodes = editType == MapEditorType.Player ? pMarkerNodes : eMarkerNodes;
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

        void HighlightNode(FieldNode node)
        {
            if (selectItem == null) return;

            switch (findType)
            {
                case FindNodeType.SetFloor:
                    node.SetNodeOutLine(true);
                    selectNode = node;
                    break;
                case FindNodeType.SetObject:
                    if (node.Mesh.enabled)
                    {
                        if (selectItem.size.x == 1 && selectItem.size.y == 1)
                        {
                            node.SetNodeOutLine(setDirection);
                        }
                        else
                        {
                            HighlightNodes(node);
                        }
                        selectNode = node;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void HighlightNodes(FieldNode node)
    {
        var size = selectItem.size.x * selectItem.size.y;
        var xMin = 0f;
        var xMax = 0f;
        var yMin = 0f;
        var yMax = 0f;
        switch (setDirection)
        {
            case TargetDirection.Left:
                xMin = node.nodePos.x;
                xMax = node.nodePos.x + selectItem.size.y;
                yMin = node.nodePos.y - selectItem.size.x + 1;
                yMax = node.nodePos.y + 1;
                break;
            case TargetDirection.Front:
                xMin = node.nodePos.x;
                xMax = node.nodePos.x + selectItem.size.x;
                yMin = node.nodePos.y;
                yMax = node.nodePos.y + selectItem.size.y;
                break;
            case TargetDirection.Back:
                xMin = node.nodePos.x - selectItem.size.x + 1;
                xMax = node.nodePos.x + 1;
                yMin = node.nodePos.y - selectItem.size.y + 1;
                yMax = node.nodePos.y + 1;
                break;
            case TargetDirection.Right:
                xMin = node.nodePos.x - selectItem.size.y + 1;
                xMax = node.nodePos.x + 1;
                yMin = node.nodePos.y;
                yMax = node.nodePos.y + selectItem.size.x;
                break;
            default:
                break;
        }
        var setNodeList = gameMgr.fieldNodes.FindAll(x => x.nodePos.x >= xMin && x.nodePos.x < xMax && x.nodePos.y >= yMin && x.nodePos.y < yMax);
        if (setNodeList.Count == size && setNodeList.Find(x => x.Mesh.enabled == false) == null)
        {
            selectNodes = setNodeList;
            for (int i = 0; i < selectNodes.Count; i++)
            {
                var selectNode = selectNodes[i];
                selectNode.SetNodeOutLine(selectNodes, setDirection);
            }
        }
    }

    private void ClearSelectNodes()
    {
        if (selectNodes.Count > 0)
        {
            for (int i = 0; i < selectNodes.Count; i++)
            {
                var selectNode = selectNodes[i];
                selectNode.SetNodeOutLine(false);
            }
            selectNodes.Clear();
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
                //fieldNode.NodeColor = Color.gray;
                gameMgr.fieldNodes.Add(fieldNode);
            }
        }

        for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
        {
            var node = gameMgr.fieldNodes[i];
            node.AddAdjacentNodes();
        }
    }

    private void OnInterface(InterfaceType _type, MapEditorType _state, GameObject _activeUI)
    {
        Debug.Log("Button");
        switch (_activeUI.activeSelf)
        {
            case true:
                SetUI(false);
                editType = MapEditorType.None;
                activeType = InterfaceType.None;
                activeUI = null;
                break;
            case false:
                if (activeUI != null)
                {
                    SetUI(false);
                }
                editType = _state;
                activeType = _type;
                activeUI = _activeUI;
                SetUI(true);
                break;
        }

        void SetUI(bool value)
        {
            if (activeType == InterfaceType.Side)
            {
                switch (value)
                {
                    case true:
                        sideButtons.transform.localPosition = sidePos_On;
                        sideUI.transform.localPosition = sidePos_On;
                        break;
                    case false:
                        if (selectItem != null)
                        {
                            selectItem.outline.enabled = false;
                            selectItem = null;
                        }
                        sideButtons.transform.localPosition = sidePos_Off;
                        sideUI.transform.localPosition = sidePos_Off;
                        break;
                }
            }
            activeUI.SetActive(value);
        }
    }

    public void SelectMapItem(MapItem item)
    {
        if (selectItem == item)
        {
            selectItem.outline.enabled = false;
            selectItem = null;
            return;
        }
        else if (selectItem != null)
        {
            selectItem.outline.enabled = false;
        }

        selectItem = item;
        selectItem.outline.enabled = true;
        switch (item.type)
        {
            case MapEditorType.Floor:
                findType = FindNodeType.SetFloor;
                break;
            case MapEditorType.HalfCover:
                findType = FindNodeType.SetObject;
                break;
            case MapEditorType.FullCover:
                findType = FindNodeType.SetObject;
                break;
            case MapEditorType.FloorObject:
                findType = FindNodeType.SetObject;
                break;
            default:
                break;
        }
    }

    #region Button Event

    #region Top
    public void Button_Node()
    {
        OnInterface(InterfaceType.Top, MapEditorType.Node, nodeUI);
    }

    public void Button_Node_Create()
    {
        if (xSize.text.Length == 0 || ySize.text.Length == 0) return;

        CreateField(int.Parse(xSize.text), int.Parse(ySize.text));
        activeUI.gameObject.SetActive(false);
        activeType = InterfaceType.None;
        activeUI = null;
    }

    public void Button_Player()
    {
        OnInterface(InterfaceType.Top, MapEditorType.Player, playerUI);
    }

    public void Button_Enemy()
    {
        OnInterface(InterfaceType.Top, MapEditorType.Enemy, enemyUI);
    }

    public void Button_Character_CreateMarker()
    {
        findType = FindNodeType.CreateMarker;
    }

    public void Button_Character_DeleteMarker()
    {
        findType = FindNodeType.DeleteMarker;
    }
    #endregion

    #region Side
    public void PointerEnter_SideButton()
    {
        onSideButton = true;
    }

    public void PointerExit_SideButton()
    {
        onSideButton = false;
    }

    public void Button_Floor()
    {
        OnInterface(InterfaceType.Side, MapEditorType.Floor, floorUI);
    }

    public void Button_HalfCover()
    {
        OnInterface(InterfaceType.Side, MapEditorType.HalfCover, halfCoverUI);
    }

    public void Button_FullCover()
    {
        OnInterface(InterfaceType.Side, MapEditorType.FullCover, fullCoverUI);
    }

    public void Button_FloorObject()
    {
        OnInterface(InterfaceType.Side, MapEditorType.FloorObject, floorObjectUI);
    }
    #endregion


    public void Button_FloorRandom()
    {
        floorDirRandom = !floorDirRandom;
        switch (floorDirRandom)
        {
            case true:
                floorRandomText.text = "바닥방향 무작위 OFF";
                break;
            case false:
                floorRandomText.text = "바닥방향 무작위 ON";
                break;
        }
    }

    public void Button_GridSwitch()
    {
        var value = gridSwitchText.text == "그리드 ON" ? true : false;
        for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
        {
            var node = gameMgr.fieldNodes[i];
            node.SetActiveNodeFrame(value);
        }
        switch (value)
        {
            case true:
                gridSwitchText.text = "그리드 OFF";
                break;
            case false:
                gridSwitchText.text = "그리드 ON";
                break;
        }
    }
    #endregion
}