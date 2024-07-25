using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MapEditorType
{
    None,
    Data,
    CreateNode,
    SetArea,
    Player,
    Enemy,
    Floor,
    FloorObject,
    Hurdle,
    Box,
    HalfCover,
    FullCover,
    SideObject,
}

public enum FindNodeType
{
    None,
    SetUnableMove,
    SetHalfCover,
    SetFullCover,
    CreateMarker,
    DeleteMarker,
    SetFloor,
    SetObject,
}

public enum EnemyMarkerType
{
    ShortRange,
    MiddleRange,
    LongRange,
    Elite,
    Boss,
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
    private Transform components;

    private Transform fieldNodeTf;
    private Transform nodeOutlineTf;
    private Transform characterTf;

    #region Top
    [Header("[Data]")]
    private GameObject dataUI;
    private TMP_InputField saveInput;
    private TMP_Dropdown loadDropdown;

    [Header("[CreateNode]")]
    private GameObject createNodeUI;
    private TMP_InputField xSizeInput;
    private TMP_InputField ySizeInput;

    [Header("[SetArea]")]
    private GameObject setAreaUI;
    private TextMeshProUGUI coverFormText;
    private List<Image> setTypeOutlines = new List<Image>();
    private bool lineForm;

    [Header("[Player]")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private List<FieldNode> pMarkerNodes = new List<FieldNode>();

    [Header("[Enemy]")]
    [SerializeField] private GameObject enemyUI;
    [SerializeField] private List<FieldNode> eMarkerNodes = new List<FieldNode>();
    [SerializeField] private TMP_Dropdown markerDropdown;
    public EnemyMarkerType enemyType;
    #endregion

    #region Side
    private GameObject sideButtons;
    private GameObject sideUI;
    private TextMeshProUGUI setDirText;
    private TextMeshProUGUI allFloorText;
    private TextMeshProUGUI floorRandomText;
    private TextMeshProUGUI gridSwitchText;
    public bool onSideButton;

    [Header("[Floor]")]
    private GameObject floorUI;
    private bool allFloor;
    private bool floorDirRandom;

    [Header("[FloorObject]")]
    private GameObject floorObjectUI;

    [Header("[Hurdle]")]
    private GameObject hurdleUI;

    [Header("[HalfCover]")]
    private GameObject halfCoverUI;

    [Header("[FullCover]")]
    private GameObject fullCoverUI;

    [Header("[SideObject]")]
    private GameObject sideObjectUI;
    #endregion

    [Header("--- Assignment Variable---")]
    [SerializeField] private MapEditorType editType;
    private Vector2 mapSize;
    private InterfaceType activeType;
    private GameObject activeUI;

    private FindNodeType findType;
    private FieldNode selectNode;
    private List<FieldNode> selectNodes = new List<FieldNode>();

    [Space(5f)]
    public MapItem selectItem;
    private List<MapItem> mapItems = new List<MapItem>();
    [SerializeField] private TargetDirection setDirection;

    private Vector3 sidePos_On;
    private Vector3 sidePos_Off;

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;

        fieldNodeTf = GameObject.FindGameObjectWithTag("FieldNodes").transform;
        nodeOutlineTf = GameObject.FindGameObjectWithTag("NodeOutlines").transform;
        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;

        components = transform.Find("Components");
        dataUI = components.Find("Top/UI/Data").gameObject;
        saveInput = components.Find("Top/UI/Data/InputField_Save").GetComponent<TMP_InputField>();
        loadDropdown = components.Find("Top/UI/Data/Dropdown_Load").GetComponent<TMP_Dropdown>();
        gameMgr.dataMgr.ReadMapLoadIndex(loadDropdown);
        dataUI.SetActive(false);

        createNodeUI = components.Find("Top/UI/CreateNode").gameObject;
        xSizeInput = createNodeUI.transform.Find("InputFields/Size_X/InputField_Value").GetComponent<TMP_InputField>();
        ySizeInput = createNodeUI.transform.Find("InputFields/Size_Y/InputField_Value").GetComponent<TMP_InputField>();
        createNodeUI.SetActive(false);

        setAreaUI = components.Find("Top/UI/SetArea").gameObject;
        coverFormText = setAreaUI.transform.Find("CoverForm/Text").GetComponent<TextMeshProUGUI>();
        setTypeOutlines.Add(setAreaUI.transform.Find("SetTypes/SetUnableMove/Outline").GetComponent<Image>());
        setTypeOutlines.Add(setAreaUI.transform.Find("SetTypes/SetHalfCover/Outline").GetComponent<Image>());
        setTypeOutlines.Add(setAreaUI.transform.Find("SetTypes/SetFullCover/Outline").GetComponent<Image>());
        setAreaUI.SetActive(false);

        playerUI = components.Find("Top/UI/Player").gameObject;
        playerUI.SetActive(false);

        enemyUI = components.Find("Top/UI/Enemy").gameObject;
        markerDropdown = enemyUI.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
        enemyUI.SetActive(false);

        sideButtons = components.Find("Side/Buttons").gameObject;
        sideUI = components.Find("Side/UI").gameObject;
        setDirText = sideUI.transform.Find("SetDirection/Text").GetComponent<TextMeshProUGUI>();
        allFloorText = sideUI.transform.Find("SubButtons/AllFloor/Text").GetComponent<TextMeshProUGUI>();
        floorRandomText = sideUI.transform.Find("SubButtons/FloorRandom/Text").GetComponent<TextMeshProUGUI>();
        gridSwitchText = sideUI.transform.Find("SubButtons/GridSwitch/Text").GetComponent<TextMeshProUGUI>();

        sidePos_On = sideUI.transform.localPosition;
        var width = sideUI.GetComponent<RectTransform>().rect.width;
        sidePos_Off = sidePos_On + new Vector3(width, 0f, 0f);
        sideButtons.transform.localPosition = sidePos_Off;
        sideUI.transform.localPosition = sidePos_Off;

        floorUI = components.Find("Side/UI/Floor").gameObject;
        floorObjectUI = components.Find("Side/UI/FloorObject").gameObject;
        hurdleUI = components.Find("Side/UI/Hurdle").gameObject;
        halfCoverUI = components.Find("Side/UI/HalfCover").gameObject;
        fullCoverUI = components.Find("Side/UI/FullCover").gameObject;
        sideObjectUI = components.Find("Side/UI/SideObject").gameObject;
        SetActiveAllSideUI(true);
        mapItems = sideUI.GetComponentsInChildren<MapItem>().ToList();
        for (int i = 0; i < mapItems.Count; i++)
        {
            var mapItem = mapItems[i];
            mapItem.SetComponents(this);
        }
        SetActiveAllSideUI(false);

        void SetActiveAllSideUI(bool value)
        {
            floorUI.SetActive(value);
            floorObjectUI.SetActive(value);
            hurdleUI.SetActive(value);
            halfCoverUI.SetActive(value);
            fullCoverUI.SetActive(value);
            sideObjectUI.SetActive(value);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            components.gameObject.SetActive(!components.gameObject.activeSelf);
        }

        if (findType == FindNodeType.None) return;

        switch (editType)
        {
            case MapEditorType.None:
                break;
            case MapEditorType.Data:
                break;
            case MapEditorType.CreateNode:
                break;
            default:
                InputEvent();
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
                        selectNode.SetNodeOutline(false);
                        selectNode = null;
                    }
                    findType = FindNodeType.None;
                }
                break;
            case MapEditorType.Enemy:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (selectNode != null)
                    {
                        selectNode.SetNodeOutline(false);
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
            selectNodes.Clear();
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
                case MapEditorType.FloorObject:
                    SetObject(true);
                    break;
                case MapEditorType.Hurdle:
                    SetObject(true);
                    break;
                case MapEditorType.HalfCover:
                    SetObject(true);
                    break;
                case MapEditorType.FullCover:
                    SetObject(true);
                    break;
                case MapEditorType.SideObject:
                    SetObject(true);
                    break;
                default:
                    break;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            switch (editType)
            {
                case MapEditorType.SetArea:
                    SetArea(true);
                    break;
                case MapEditorType.Floor:
                    SetFloor(true);
                    break;
                default:
                    break;
            }
        }
        else if (Input.GetMouseButton(1))
        {
            switch (editType)
            {
                case MapEditorType.SetArea:
                    SetArea(false);
                    break;
                case MapEditorType.Floor:
                    SetFloor(false);
                    break;
                case MapEditorType.FloorObject:
                    SetObject(false);
                    break;
                case MapEditorType.Hurdle:
                    SetObject(false);
                    break;
                case MapEditorType.HalfCover:
                    SetObject(false);
                    break;
                case MapEditorType.FullCover:
                    SetObject(false);
                    break;
                case MapEditorType.SideObject:
                    SetObject(false);
                    break;
                default:
                    break;
            }
        }
        else if (Input.GetMouseButtonDown(2))
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
            setDirText.text = $"배치방향\n{setDirection}";

            if (selectNode != null)
            {
                if (selectItem == null)
                {
                    selectNode.SetNodeOutline(setDirection);
                }
                else if (selectItem.size.x == 1 && selectItem.size.y == 1)
                {
                    selectNode.SetNodeOutline(setDirection);
                }
                else
                {
                    ClearSelectNodes();
                    HighlightNodes(selectNode);
                }
            }
        }

        void SetArea(bool value)
        {
            switch (value)
            {
                case true:
                    if (selectNodes.Find(x => x == selectNode) == null)
                    {
                        if (lineForm)
                        {
                            selectNode.SetOnArea(setDirection, findType, true);
                        }
                        else
                        {
                            selectNode.SetOnArea(findType, true);
                        }
                        selectNodes.Add(selectNode);
                    }
                    break;
                case false:
                    selectNode.SetOffArea();
                    break;
            }
        }

        void SetMarker()
        {
            selectNode.SetNodeOutline(false);
            var charType = editType == MapEditorType.Player ? CharacterOwner.Player : CharacterOwner.Enemy;
            var markerNodes = editType == MapEditorType.Player ? pMarkerNodes : eMarkerNodes;
            switch (findType)
            {
                case FindNodeType.CreateMarker:
                    if (charType == CharacterOwner.Enemy)
                    {
                        selectNode.SetOnMarker(enemyType);
                    }
                    else
                    {
                        selectNode.SetOnMarker();
                    }
                    markerNodes.Add(selectNode);
                    break;
                case FindNodeType.DeleteMarker:
                    selectNode.SetOffMarker();
                    markerNodes.Remove(selectNode);
                    //for (int i = 0; i < markerNodes.Count; i++)
                    //{
                    //    var markerNode = markerNodes[i];
                    //    markerNode.SetOnMarker(charType);
                    //}
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
                    if (selectItem != null && selectNodes.Find(x => x == selectNode) == null)
                    {
                        if (allFloor)
                        {
                            for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
                            {
                                var node = gameMgr.fieldNodes[i];
                                node.SetOnFloor(selectItem, floorDirRandom);
                                selectNodes.Add(node);
                            }
                        }
                        else
                        {
                            selectNode.SetOnFloor(selectItem, floorDirRandom);
                            selectNodes.Add(selectNode);
                        }
                    }
                    break;
                case false:
                    selectNode.SetOffFloor();
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
            var node = hit.collider.GetComponentInParent<FieldNode>();
            if (node == null) return;

            if (selectNode != null && node != selectNode)
            {
                selectNode.SetNodeOutline(false);
                ClearSelectNodes();
            }

            switch (findType)
            {
                case FindNodeType.SetUnableMove:
                    HighlightNode(node);
                    break;
                case FindNodeType.SetHalfCover:
                    HighlightNode(node);
                    break;
                case FindNodeType.SetFullCover:
                    HighlightNode(node);
                    break;
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
            selectNode.SetNodeOutline(false);
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
                        node.SetNodeOutline(true);
                        break;
                    case MapEditorType.Enemy:
                        node.SetNodeOutline(true);
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
                node.SetNodeOutline(true);
                selectNode = node;
            }
            else if (selectNode != null)
            {
                selectNode.SetNodeOutline(false);
                selectNode = null;
            }
        }

        void HighlightNode(FieldNode node)
        {
            switch (findType)
            {
                case FindNodeType.SetUnableMove:
                    if (lineForm)
                    {
                        node.SetNodeOutline(setDirection);
                    }
                    else
                    {
                        node.SetNodeOutline(true);
                    }
                    selectNode = node;
                    break;
                case FindNodeType.SetHalfCover:
                    if (lineForm)
                    {
                        node.SetNodeOutline(setDirection);
                    }
                    else
                    {
                        node.SetNodeOutline(true);
                    }
                    selectNode = node;
                    break;
                case FindNodeType.SetFullCover:
                    if (lineForm)
                    {
                        node.SetNodeOutline(setDirection);
                    }
                    else
                    {
                        node.SetNodeOutline(true);
                    }
                    selectNode = node;
                    break;
                case FindNodeType.SetFloor:
                    node.SetNodeOutline(true);
                    selectNode = node;
                    break;
                case FindNodeType.SetObject:
                    if (node.Mesh.enabled)
                    {
                        if (selectItem == null)
                        {
                            node.SetNodeOutline(setDirection);
                        }
                        else if (selectItem.size.x == 1 && selectItem.size.y == 1)
                        {
                            node.SetNodeOutline(setDirection);
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
                selectNode.SetNodeOutline(selectNodes, setDirection);
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
                selectNode.SetNodeOutline(false);
            }
            selectNodes.Clear();
        }
    }

    public void SelectMapItem(MapItem item)
    {
        selectNodes.Clear();
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
    }

    public void SwitchingLayer(LayerMask layer, bool value)
    {
        var newLayer = value ? gameMgr.camMgr.mainCam.cullingMask | layer : gameMgr.camMgr.mainCam.cullingMask & ~layer;
        gameMgr.camMgr.mainCam.cullingMask = newLayer;
    }

    private IEnumerator Coroutine_CreateNodes(float xSize, float ySize)
    {
        Debug.Log("시작");

        if (gameMgr.fieldNodes.Count > 0)
        {
            for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
            {
                var node = gameMgr.fieldNodes[i];
                for (int j = 0; j < node.outlines.Count; j++)
                {
                    var outline = node.outlines[j];
                    if (outline == null) continue;

                    Destroy(outline.gameObject);
                }
                Destroy(node.gameObject);

                if (i % 50 == 0)
                {
                    float progress = (float)i / gameMgr.fieldNodes.Count * 100;
                    Debug.Log($"기존 노드 제거 진행률: {progress}%");
                    yield return null;
                }
            }
        }
        pMarkerNodes.Clear();
        eMarkerNodes.Clear();
        gameMgr.fieldNodes.Clear();

        mapSize = new Vector2(xSize, ySize);
        var size = DataUtility.nodeSize;
        var interval = DataUtility.nodeInterval;
        int totalNodes = (int)(mapSize.x * mapSize.y);
        int createdNodes = 0;
        for (int i = 0; i < mapSize.y; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                var fieldNode = Instantiate(Resources.Load<FieldNode>("Prefabs/FieldNode"));
                fieldNode.transform.SetParent(fieldNodeTf, false);
                var pos = new Vector3((j * size) + (j * interval), 0f, (i * size) + (i * interval));
                fieldNode.transform.position = pos;
                fieldNode.SetComponents(gameMgr, new Vector2Int(j, i));
                //fieldNode.NodeColor = Color.gray;
                gameMgr.fieldNodes.Add(fieldNode);
                createdNodes++;

                if ((i + j) % 50 == 0)
                {
                    float progress = (float)createdNodes / totalNodes * 100;
                    Debug.Log($"노드 생성 진행률: {progress}%");
                    yield return null;
                }
            }
        }

        for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
        {
            var node = gameMgr.fieldNodes[i];
            node.AddAdjacentNodes();
            node.AddNodeOutline(nodeOutlineTf);

            if (i % 50 == 0)
            {
                float progress = (float)i / gameMgr.fieldNodes.Count * 100;
                Debug.Log($"인접 노드 및 아웃라인 추가 진행률: {progress}%");
                yield return null;
            }
        }

        Debug.Log("완료");
    }

    public IEnumerator Coroutine_MapLoad(MapData mapData, bool allLoad)
    {
        if (gameMgr.fieldNodes.Count > 0)
        {
            for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
            {
                var node = gameMgr.fieldNodes[i];
                for (int j = 0; j < node.outlines.Count; j++)
                {
                    var outline = node.outlines[j];
                    if (outline == null) continue;

                    Destroy(outline.gameObject);
                }
                Destroy(node.gameObject);

                if (i % 50 == 0)
                {
                    float progress = (float)i / gameMgr.fieldNodes.Count * 100;
                    Debug.Log($"기존 노드 제거 진행률: {progress}%");
                    yield return null;
                }
            }
        }
        pMarkerNodes.Clear();
        eMarkerNodes.Clear();
        gameMgr.fieldNodes.Clear();

        mapSize = new Vector2(mapData.mapSize.x, mapData.mapSize.y);
        var size = DataUtility.nodeSize;
        var interval = DataUtility.nodeInterval;
        int totalNodes = (int)(mapSize.x * mapSize.y);
        int createdNodes = 0;
        for (int i = 0; i < mapSize.y; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                var fieldNode = Instantiate(Resources.Load<FieldNode>("Prefabs/FieldNode"));
                fieldNode.transform.SetParent(fieldNodeTf, false);
                var pos = new Vector3((j * size) + (j * interval), 0f, (i * size) + (i * interval));
                fieldNode.transform.position = pos;
                fieldNode.SetComponents(gameMgr, new Vector2Int(j, i));
                //fieldNode.NodeColor = Color.gray;
                gameMgr.fieldNodes.Add(fieldNode);
                createdNodes++;

                if ((i + j) % 50 == 0)
                {
                    float progress = (float)createdNodes / totalNodes * 100;
                    Debug.Log($"노드 생성 진행률: {progress}%");
                    yield return null;
                }
            }
        }

        for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
        {
            var node = gameMgr.fieldNodes[i];
            node.AddAdjacentNodes();
            node.AddNodeOutline(nodeOutlineTf);

            if (i % 50 == 0)
            {
                float progress = (float)i / gameMgr.fieldNodes.Count * 100;
                Debug.Log($"인접 노드 및 아웃라인 추가 진행률: {progress}%");
                yield return null;
            }
        }

        for (int i = 0; i < mapData.nodeDatas.Length; i++)
        {
            var nodeData = mapData.nodeDatas[i];
            var node = gameMgr.fieldNodes[i];

            // FloorData
            if (nodeData.isMesh)
            {
                var floorItem = mapItems.Find(x => x.name == $"{nodeData.floorItemName}");
                node.SetOnFloor(floorItem, nodeData.floorRot);
            }

            // NodeCover Data
            if (nodeData.isNodeCover)
            {
                node.SetOnArea(nodeData.nCoverType, allLoad);
            }

            // LineCover Data
            if (nodeData.isLineCover)
            {
                for (int j = 0; j < nodeData.lCoverTypes.Length; j++)
                {
                    var setDirection = nodeData.lCoverDirs[j];
                    var coverType = nodeData.lCoverTypes[j];
                    if (setDirection != TargetDirection.None && coverType != FindNodeType.None)
                    {
                        node.SetOnArea(setDirection, coverType, allLoad);
                    }
                }
            }

            // MarkerData
            if (nodeData.isMarker)
            {
                var markerNodes = nodeData.markerType == CharacterOwner.Player ? pMarkerNodes : eMarkerNodes;
                markerNodes.Add(node);
                if (allLoad)
                {
                    if (nodeData.markerType == CharacterOwner.Enemy)
                    {
                        selectNode.SetOnMarker(nodeData.enemyType);
                    }
                    else
                    {
                        selectNode.SetOnMarker();
                    }
                }
            }

            // ObjectData
            if (nodeData.isObject)
            {
                for (int j = 0; j < nodeData.objectDatas.Length; j++)
                {
                    var objectData = nodeData.objectDatas[j];
                    var objectUI = GetObjectUI(objectData.objectType);
                    var objectItem = mapItems.Find(x => x.name == $"{objectData.itemName}");
                    node.SetOnObject(objectItem, objectData.setDir);
                }
            }
        }

        if (!allLoad)
        {
            var playerNode = pMarkerNodes[0];
            gameMgr.CreateCharacter(CharacterOwner.Player, playerNode.nodePos, gameMgr.dataMgr.gameData.playerID);
            yield return null;

            for (int i = 0; i < eMarkerNodes.Count; i++)
            {
                var markerNode = eMarkerNodes[i];
                gameMgr.CreateCharacter(CharacterOwner.Enemy, markerNode.nodePos, "E0001");
                yield return null;
            }
        }

        Debug.Log("Map load complete");
        gameMgr.sceneHlr.EndLoadScene();

        GameObject GetObjectUI(MapEditorType type)
        {
            switch (type)
            {
                case MapEditorType.FloorObject:
                    return floorObjectUI;
                case MapEditorType.HalfCover:
                    return halfCoverUI;
                case MapEditorType.FullCover:
                    return fullCoverUI;
                case MapEditorType.SideObject:
                    return sideObjectUI;
                default:
                    return null;
            }
        }
    }

    public void SetActive(bool value)
    {
        components.gameObject.SetActive(value);
    }

    #region Button Event
    public void PointerEnter_MapEditorButton()
    {
        onSideButton = true;
    }

    public void PointerExit_MapEditorButton()
    {
        onSideButton = false;
    }

    #region Top
    public void Button_Data()
    {
        OnInterface(InterfaceType.Top, MapEditorType.Data, dataUI);
    }

    public void Button_Data_Save()
    {
        if (saveInput.text.Length == 0) return;

        gameMgr.dataMgr.SaveMapData(saveInput.text, mapSize, gameMgr.fieldNodes);
        gameMgr.dataMgr.ReadMapLoadIndex(loadDropdown);
        saveInput.text = "";
    }

    public void Button_Data_Load()
    {
        if (loadDropdown.options.Count == 0 || saveInput.text.Length > 0) return;

        var loadName = loadDropdown.options[loadDropdown.value].text;
        var mapData = gameMgr.dataMgr.LoadMapData(loadName);
        if (mapData != null)
        {
            StartCoroutine(Coroutine_MapLoad(mapData, true));
        }
    }

    public void Button_CreateNode()
    {
        OnInterface(InterfaceType.Top, MapEditorType.CreateNode, createNodeUI);
    }

    public void Button_CreateNode_Create()
    {
        if (xSizeInput.text.Length == 0 || ySizeInput.text.Length == 0) return;

        StartCoroutine(Coroutine_CreateNodes(int.Parse(xSizeInput.text), int.Parse(ySizeInput.text)));
        //CreateNodes(int.Parse(xSizeInput.text), int.Parse(ySizeInput.text));
        activeUI.gameObject.SetActive(false);
        editType = MapEditorType.None;
        activeType = InterfaceType.None;
        activeUI = null;
    }

    public void Button_SetArea()
    {
        OnInterface(InterfaceType.Top, MapEditorType.SetArea, setAreaUI);
    }

    public void Button_SetArea_CoverForm()
    {
        lineForm = !lineForm;
        switch (lineForm)
        {
            case true:
                coverFormText.text = "라인";
                break;
            case false:
                coverFormText.text = "노드";
                break;
        }
    }

    public void Button_SetArea_SetUnableMove()
    {
        SwitchSetArea(FindNodeType.SetUnableMove);
    }

    public void Button_SetArea_SetHalfCover()
    {
        SwitchSetArea(FindNodeType.SetHalfCover);
    }

    public void Button_SetArea_SetFullCover()
    {
        SwitchSetArea(FindNodeType.SetFullCover);
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

    public void Button_Close()
    {
        gameObject.SetActive(false);
    }

    public void ValueChanged_EnemyMarkerType()
    {
        enemyType = (EnemyMarkerType)markerDropdown.value;
    }
    #endregion

    #region Side
    public void Button_Floor()
    {
        OnInterface(InterfaceType.Side, MapEditorType.Floor, floorUI);
    }

    public void Button_FloorObject()
    {
        OnInterface(InterfaceType.Side, MapEditorType.FloorObject, floorObjectUI);
    }

    public void Button_Hurdle()
    {
        OnInterface(InterfaceType.Side, MapEditorType.Hurdle, hurdleUI);
    }

    public void Button_HalfCover()
    {
        OnInterface(InterfaceType.Side, MapEditorType.HalfCover, halfCoverUI);
    }

    public void Button_FullCover()
    {
        OnInterface(InterfaceType.Side, MapEditorType.FullCover, fullCoverUI);
    }

    public void Button_SideObject()
    {
        OnInterface(InterfaceType.Side, MapEditorType.SideObject, sideObjectUI);
    }
    #endregion

    #region Sub
    public void Button_AllFloor()
    {
        allFloor = !allFloor;
        switch (allFloor)
        {
            case true:
                allFloorText.text = "바닥 일괄적용 ON";
                break;
            case false:
                allFloorText.text = "바닥 일괄적용 OFF";
                break;
        }
    }

    public void Button_FloorRandom()
    {
        floorDirRandom = !floorDirRandom;
        switch (floorDirRandom)
        {
            case true:
                floorRandomText.text = "바닥방향 무작위 ON";
                break;
            case false:
                floorRandomText.text = "바닥방향 무작위 OFF";
                break;
        }
    }

    public void Button_GridSwitch()
    {
        var value = gridSwitchText.text == "그리드 OFF" ? true : false;
        for (int i = 0; i < gameMgr.fieldNodes.Count; i++)
        {
            var node = gameMgr.fieldNodes[i];
            node.SetActiveNodeFrame(value);
        }
        switch (value)
        {
            case true:
                gridSwitchText.text = "그리드 ON";
                break;
            case false:
                gridSwitchText.text = "그리드 OFF";
                break;
        }
    }
    #endregion

    private void OnInterface(InterfaceType _type, MapEditorType _state, GameObject _activeUI)
    {
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
                        switch (editType)
                        {
                            case MapEditorType.Floor:
                                findType = FindNodeType.SetFloor;
                                break;
                            case MapEditorType.FloorObject:
                                findType = FindNodeType.SetObject;
                                break;
                            case MapEditorType.Hurdle:
                                findType = FindNodeType.SetObject;
                                break;
                            case MapEditorType.HalfCover:
                                findType = FindNodeType.SetObject;
                                break;
                            case MapEditorType.FullCover:
                                findType = FindNodeType.SetObject;
                                break;
                            case MapEditorType.SideObject:
                                findType = FindNodeType.SetObject;
                                break;
                            default:
                                break;
                        }
                        break;
                    case false:
                        if (selectItem != null)
                        {
                            selectItem.outline.enabled = false;
                            selectItem = null;
                        }
                        sideButtons.transform.localPosition = sidePos_Off;
                        sideUI.transform.localPosition = sidePos_Off;
                        findType = FindNodeType.None;
                        break;
                }
            }
            else if (!value)
            {
                for (int i = 0; i < setTypeOutlines.Count; i++)
                {
                    var outline = setTypeOutlines[i];
                    outline.enabled = false;
                }
                findType = FindNodeType.None;
            }
            activeUI.SetActive(value);
        }
    }

    private void SwitchSetArea(FindNodeType _findType)
    {
        var index = (int)_findType - 1;
        switch (findType)
        {
            case FindNodeType.None:
                setTypeOutlines[index].enabled = true;
                findType = _findType;
                break;
            case FindNodeType.SetUnableMove:
                SetFindType();
                break;
            case FindNodeType.SetHalfCover:
                SetFindType();
                break;
            case FindNodeType.SetFullCover:
                SetFindType();
                break;
            default:
                break;
        }

        void SetFindType()
        {
            if (findType == _findType)
            {
                setTypeOutlines[index].enabled = false;
                findType = FindNodeType.None;
            }
            else
            {
                for (int i = 0; i < setTypeOutlines.Count; i++)
                {
                    var outline = setTypeOutlines[i];
                    if (i == index)
                    {
                        outline.enabled = true;
                    }
                    else
                    {
                        outline.enabled = false;
                    }
                }
                findType = _findType;
            }
        }
    }
    #endregion
}