using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    BaseCamp,
    Floor,
    FloorObject,
    Hurdle,
    Box,
    HalfCover,
    FullCover,
    LineObject,
    BaseObject,
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
    SetBaseObject,
}

public enum MarkerType
{
    None,
    Player,
    Enemy,
    Base,
}

public enum EnemyMarker
{
    None = -1,
    ShortRange,
    MiddleRange,
    LongRange,
    Elite,
    Boss,
}

public enum BaseCampMarker
{
    None = -1,
    Mission_Node,
    Mission_Enter,
    Storage_Node,
    Storage_Enter,
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
    [SerializeField] private TMP_Dropdown enemyDropdown;
    public EnemyMarker enemyType = EnemyMarker.ShortRange;

    [Header("[BaseCamp]")]
    [SerializeField] private GameObject baseUI;
    [SerializeField] private TMP_Dropdown baseDropdown;
    [SerializeField] private List<FieldNode> eventMarkerNodes = new List<FieldNode>();
    public BaseCampMarker baseType = BaseCampMarker.Mission_Node;
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
    private GameObject lineObjectUI;

    [Header("[BaseObject]")]
    private GameObject baseObjectUI;
    #endregion

    [Header("--- Assignment Variable---")]
    [SerializeField] private MapEditorType editType;
    private Vector2Int mapSize;
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

    private const int NODES_PER_FRAME = 10; // 한 프레임당 처리할 최대 작업량 설정
    private const float UPDATE_INTERVAL = 0.1f; // 진행률 업데이트 간격 (0.1초)

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
        enemyDropdown = enemyUI.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
        enemyUI.SetActive(false);

        baseUI = components.Find("Top/UI/BaseCamp").gameObject;
        baseDropdown = baseUI.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
        baseUI.SetActive(false);

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
        lineObjectUI = components.Find("Side/UI/LineObject").gameObject;
        baseObjectUI = components.Find("Side/UI/BaseObject").gameObject;
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
            lineObjectUI.SetActive(value);
            baseObjectUI.SetActive(value);
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
                case MapEditorType.BaseCamp:
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
                case MapEditorType.LineObject:
                    SetObject(true);
                    break;
                case MapEditorType.BaseObject:
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
                case MapEditorType.LineObject:
                    SetObject(false);
                    break;
                case MapEditorType.BaseObject:
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
            switch (findType)
            {
                case FindNodeType.CreateMarker:
                    switch (editType)
                    {
                        case MapEditorType.Player:
                            selectNode.SetOnMarker(true);
                            pMarkerNodes.Add(selectNode);
                            break;
                        case MapEditorType.Enemy:
                            selectNode.SetOnMarker(true, enemyType);
                            eMarkerNodes.Add(selectNode);
                            break;
                        case MapEditorType.BaseCamp:
                            selectNode.SetOnMarker(true, baseType);
                            eventMarkerNodes.Add(selectNode);
                            break;
                        default:
                            break;
                    }
                    break;
                case FindNodeType.DeleteMarker:
                    selectNode.SetOffMarker();
                    switch (editType)
                    {
                        case MapEditorType.Player:
                            pMarkerNodes.Remove(selectNode);
                            break;
                        case MapEditorType.Enemy:
                            eMarkerNodes.Remove(selectNode);
                            break;
                        case MapEditorType.BaseCamp:
                            eventMarkerNodes.Remove(selectNode);
                            break;
                        default:
                            break;
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
                    if (selectItem != null && selectNodes.Find(x => x == selectNode) == null)
                    {
                        if (allFloor)
                        {
                            for (int i = 0; i < gameMgr.nodeList.Count; i++)
                            {
                                var node = gameMgr.nodeList[i];
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
                case FindNodeType.SetBaseObject:
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
            switch (editType)
            {
                case MapEditorType.Player:
                    node.SetNodeOutline(true);
                    break;
                case MapEditorType.Enemy:
                    node.SetNodeOutline(true);
                    break;
                case MapEditorType.BaseCamp:
                    node.SetNodeOutline(true);
                    break;
                default:
                    break;
            }
            selectNode = node;
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
                case FindNodeType.SetBaseObject:
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
        var setNodeList = gameMgr.nodeList.FindAll(x => x.nodePos.x >= xMin && x.nodePos.x < xMax && x.nodePos.y >= yMin && x.nodePos.y < yMax);
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

    private async Task Async_CreateNodes(int xSize, int ySize)
    {
        Debug.Log("시작");

        await ClearFieldNodes();

        await CreateFieldNodes(xSize, ySize);

        Debug.Log("완료");
    }

    public async Task Async_MapLoad(MapData mapData, bool allLoad, bool isBase)
    {
        Debug.Log("Map load start");

        await ClearFieldNodes();

        await CreateFieldNodes(mapData.mapSize.x, mapData.mapSize.y);

        await ReadMapData(mapData, allLoad, isBase);

        Debug.Log("Map load complete");
        if (gameMgr.playerList.Count > 0) gameMgr.camMgr.pivotPoint.position = gameMgr.playerList[0].currentNode.transform.position;
        gameMgr.camMgr.SetBlockLines(true);
        gameMgr.sceneHlr.EndLoadScene();
    }

    private async Task ClearFieldNodes()
    {
        if (gameMgr.nodeList.Count > 0)
        {
            float lastUpdateTime = Time.time;
            int totalNodes = gameMgr.nodeList.Count;
            int processedNodes = 0;

            while (processedNodes < totalNodes)
            {
                int endIndex = Mathf.Min(processedNodes + NODES_PER_FRAME, totalNodes);

                for (int i = processedNodes; i < endIndex; i++)
                {
                    var node = gameMgr.nodeList[i];
                    if (node != null)
                    {
                        for (int j = 0; j < node.outlines.Count; j++)
                        {
                            var outline = node.outlines[j];
                            if (outline == null) continue;
                            Destroy(outline.gameObject);
                        }
                        Destroy(node.gameObject);
                    }
                }

                processedNodes = endIndex;

                // 진행률 업데이트
                if (Time.time - lastUpdateTime >= UPDATE_INTERVAL)
                {
                    float progress = (float)processedNodes / totalNodes * 100;
                    Debug.Log($"기존 노드 제거 진행률: {progress:F1}%");
                    lastUpdateTime = Time.time;
                }

                await Task.Yield(); // 매 청크 처리 후 다음 프레임까지 대기
            }
        }

        pMarkerNodes.Clear();
        eMarkerNodes.Clear();
        gameMgr.nodeList.Clear();
    }

    private async Task CreateFieldNodes(int xSize, int ySize)
    {
        mapSize = new Vector2Int(xSize, ySize);
        var size = DataUtility.nodeSize;
        var interval = DataUtility.nodeInterval;
        int totalNodes = mapSize.x * mapSize.y;
        int createdNodes = 0;
        float lastUpdateTime = Time.time;

        FieldNode nodePrefab = Resources.Load<FieldNode>("Prefabs/FieldNode");
        List<Vector2Int> positions = new List<Vector2Int>();

        // 모든 위치 미리 계산
        for (int i = 0; i < mapSize.y; i++)
        {
            for (int j = 0; j < mapSize.x; j++)
            {
                positions.Add(new Vector2Int(j, i));
            }
        }

        // 청크 단위로 노드 생성
        for (int index = 0; index < positions.Count; index += NODES_PER_FRAME)
        {
            int endIndex = Mathf.Min(index + NODES_PER_FRAME, positions.Count);

            for (int i = index; i < endIndex; i++)
            {
                var pos2D = positions[i];
                var fieldNode = Instantiate(nodePrefab);
                fieldNode.transform.SetParent(fieldNodeTf, false);
                var pos = new Vector3(
                    (pos2D.x * size) + (pos2D.x * interval),
                    0f,
                    (pos2D.y * size) + (pos2D.y * interval)
                );
                fieldNode.transform.position = pos;
                fieldNode.SetComponents(gameMgr, pos2D);
                gameMgr.nodeList.Add(fieldNode);
                createdNodes++;
            }

            // 진행률 업데이트
            if (Time.time - lastUpdateTime >= UPDATE_INTERVAL)
            {
                float progress = (float)createdNodes / totalNodes * 100;
                Debug.Log($"노드 생성 진행률: {progress:F1}%");
                lastUpdateTime = Time.time;
            }

            await Task.Yield();
        }

        // 인접 노드 및 아웃라인 추가
        lastUpdateTime = Time.time;
        int processedNodes = 0;

        while (processedNodes < gameMgr.nodeList.Count)
        {
            int endIndex = Mathf.Min(processedNodes + NODES_PER_FRAME, gameMgr.nodeList.Count);

            for (int i = processedNodes; i < endIndex; i++)
            {
                var node = gameMgr.nodeList[i];
                node.AddAdjacentNodes();
                node.AddNodeOutline(nodeOutlineTf);
            }

            processedNodes = endIndex;

            if (Time.time - lastUpdateTime >= UPDATE_INTERVAL)
            {
                float progress = (float)processedNodes / gameMgr.nodeList.Count * 100;
                Debug.Log($"인접 노드 및 아웃라인 추가 진행률: {progress:F1}%");
                lastUpdateTime = Time.time;
            }

            await Task.Yield();
        }
    }

    private async Task ReadMapData(MapData mapData, bool allLoad, bool isBase)
    {
        float lastUpdateTime = Time.time;
        int totalNodes = mapData.nodeDatas.Length;
        int processedNodes = 0;

        while (processedNodes < totalNodes)
        {
            int endIndex = Mathf.Min(processedNodes + NODES_PER_FRAME, totalNodes);

            for (int i = processedNodes; i < endIndex; i++)
            {
                var nodeData = mapData.nodeDatas[i];
                var node = gameMgr.nodeList[i];

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
                    await ProcessLineCoverData(nodeData, node, allLoad);
                }

                // ObjectData
                if (nodeData.isObject)
                {
                    await ProcessObjectData(nodeData, node);
                }

                // MarkerData
                if (nodeData.isMarker)
                {
                    await HandleMarkerDataAsync(node, nodeData, allLoad);
                }
            }

            processedNodes = endIndex;

            if (Time.time - lastUpdateTime >= UPDATE_INTERVAL)
            {
                float progress = (float)processedNodes / totalNodes * 100;
                Debug.Log($"맵 데이터 로드 진행률: {progress:F1}%");
                lastUpdateTime = Time.time;
            }

            await Task.Yield();
        }

        if (!allLoad)
        {
            await SpawnCharactersAsync(isBase);
        }
    }

    private async Task ProcessLineCoverData(NodeData nodeData, FieldNode node, bool allLoad)
    {
        for (int j = 0; j < nodeData.lCoverTypes.Length; j++)
        {
            var setDirection = nodeData.lCoverDirs[j];
            var coverType = nodeData.lCoverTypes[j];
            if (setDirection != TargetDirection.None && coverType != FindNodeType.None)
            {
                node.SetOnArea(setDirection, coverType, allLoad);
            }

            if (j % NODES_PER_FRAME == 0)
            {
                await Task.Yield();
            }
        }
    }

    private async Task ProcessObjectData(NodeData nodeData, FieldNode node)
    {
        for (int j = 0; j < nodeData.objectDatas.Length; j++)
        {
            var objectData = nodeData.objectDatas[j];
            var objectUI = GetObjectUI(objectData.objectType);
            var objectItem = mapItems.Find(x => x.name == $"{objectData.itemName}");
            node.SetOnObject(objectItem, objectData.setDir);

            if (j % NODES_PER_FRAME == 0)
            {
                await Task.Yield();
            }
        }
    }

    private async Task HandleMarkerDataAsync(FieldNode node, NodeData nodeData, bool allLoad)
    {
        switch (nodeData.markerType)
        {
            case MarkerType.Player:
                pMarkerNodes.Add(node);
                node.SetOnMarker(allLoad);
                break;
            case MarkerType.Enemy:
                eMarkerNodes.Add(node);
                node.SetOnMarker(allLoad, nodeData.enemyType);
                break;
            case MarkerType.Base:
                eventMarkerNodes.Add(node);
                node.SetOnMarker(allLoad, nodeData.baseType);
                if (node.baseType == BaseCampMarker.Storage_Node)
                {
                    await HandleBaseStorageAsync(node);
                }
                break;
        }
    }

    private async Task HandleBaseStorageAsync(FieldNode node)
    {
        var baseStorages = gameMgr.dataMgr.gameData.baseStorages;
        var find = baseStorages.Find(x => x.nodePos == node.nodePos);
        if (find == null)
        {
            var baseStorage = new StorageInfo()
            {
                storageName = $"{node.baseStorage.storageName}",
                type = StorageInfo.StorageType.Storage,
                nodePos = node.nodePos,
                slotSize = node.baseStorage.slotSize,
            };
            baseStorages.Add(baseStorage);

            List<StartingItemDataInfo> startingStorageItems = gameMgr.dataMgr.startingItemData.startingItemInfos
                                                             .FindAll(x => x.createLocation == baseStorage.storageName);
            if (startingStorageItems.Count > 0)
                gameMgr.gameMenuMgr.CreateStorageItems(baseStorage, startingStorageItems);
        }
        await Task.Yield();
    }

    private async Task SpawnCharactersAsync(bool isBase)
    {
        for (int i = 0; i < pMarkerNodes.Count; i++)
        {
            var playerNode = pMarkerNodes[i];
            gameMgr.CreateCharacter(CharacterOwner.Player, playerNode.nodePos, gameMgr.dataMgr.gameData.playerID);
        }

        if (isBase)
        {
            gameMgr.playerList[0].EnterTheBase();
            await Task.Yield();
        }
        else
        {
            await SpawnEnemiesAsync();
        }
    }

    private async Task SpawnEnemiesAsync()
    {
        var stageData = gameMgr.dataMgr.gameData.stageData;
        for (int i = 0; i < eMarkerNodes.Count; i++)
        {
            var markerNode = eMarkerNodes[i];
            SpawnEnemyInfo enemyInfo = GetEnemyInfo(markerNode.enemyType, stageData);
            if (enemyInfo.ID == null) continue;

            if (enemyInfo.ID.Length > 0)
            {
                gameMgr.CreateCharacter(CharacterOwner.Enemy, markerNode.nodePos, enemyInfo.ID);
                await Task.Yield();
            }
        }
    }

    private SpawnEnemyInfo GetEnemyInfo(EnemyMarker enemyType, StageDataInfo stageData)
    {
        switch (enemyType)
        {
            case EnemyMarker.ShortRange:
                return stageData.shortRangeEnemys.Count > 0
                    ? stageData.shortRangeEnemys[UnityEngine.Random.Range(0, stageData.shortRangeEnemys.Count)]
                    : new SpawnEnemyInfo();
            case EnemyMarker.MiddleRange:
                return stageData.middleRangeEnemys.Count > 0
                    ? stageData.middleRangeEnemys[UnityEngine.Random.Range(0, stageData.middleRangeEnemys.Count)]
                    : new SpawnEnemyInfo();
            case EnemyMarker.LongRange:
                return stageData.longRangeEnemys.Count > 0
                    ? stageData.longRangeEnemys[UnityEngine.Random.Range(0, stageData.longRangeEnemys.Count)]
                    : new SpawnEnemyInfo();
            case EnemyMarker.Elite:
                return stageData.eliteEnemys.Count > 0
                    ? stageData.eliteEnemys[UnityEngine.Random.Range(0, stageData.eliteEnemys.Count)]
                    : new SpawnEnemyInfo();
            case EnemyMarker.Boss:
                return stageData.bossEnemy;
            default:
                return new SpawnEnemyInfo();
        }
    }

    private GameObject GetObjectUI(MapEditorType type)
    {
        switch (type)
        {
            case MapEditorType.FloorObject:
                return floorObjectUI;
            case MapEditorType.HalfCover:
                return halfCoverUI;
            case MapEditorType.FullCover:
                return fullCoverUI;
            case MapEditorType.LineObject:
                return lineObjectUI;
            default:
                return null;
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

        gameMgr.dataMgr.SaveMapData(saveInput.text, mapSize, gameMgr.nodeList);
        gameMgr.dataMgr.ReadMapLoadIndex(loadDropdown);
        saveInput.text = "";
    }

    public async void Button_Data_Load()
    {
        if (loadDropdown.options.Count == 0 || saveInput.text.Length > 0) return;

        var loadName = loadDropdown.options[loadDropdown.value].text;
        var mapData = gameMgr.dataMgr.LoadMapData(loadName);
        if (mapData != null)
        {
            await Async_MapLoad(mapData, true, false);
        }
    }

    public void Button_CreateNode()
    {
        OnInterface(InterfaceType.Top, MapEditorType.CreateNode, createNodeUI);
    }

    public async void Button_CreateNode_Create()
    {
        if (xSizeInput.text.Length == 0 || ySizeInput.text.Length == 0) return;

        await Async_CreateNodes(int.Parse(xSizeInput.text), int.Parse(ySizeInput.text));
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

    public void Button_BaseCamp()
    {
        OnInterface(InterfaceType.Top, MapEditorType.BaseCamp, baseUI);
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
        enemyType = (EnemyMarker)enemyDropdown.value;
    }

    public void ValueChanged_BaseCampMarkerType()
    {
        baseType = (BaseCampMarker)baseDropdown.value;
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
        OnInterface(InterfaceType.Side, MapEditorType.LineObject, lineObjectUI);
    }

    public void Button_BaseObject()
    {
        OnInterface(InterfaceType.Side, MapEditorType.BaseObject, baseObjectUI);
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
        for (int i = 0; i < gameMgr.nodeList.Count; i++)
        {
            var node = gameMgr.nodeList[i];
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
                            case MapEditorType.LineObject:
                                findType = FindNodeType.SetObject;
                                break;
                            case MapEditorType.BaseObject:
                                findType = FindNodeType.SetBaseObject;
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