using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum ActionState
{
    None,
    Move,
    Shot,
    Watch,
}

public class GameManager : MonoBehaviour
{
    [Header("---Access Script---")]
    public DataManager dataMgr;
    public CameraManager camMgr;
    public MapEditor mapEdt;

    [Header("---Access Component---")]
    [SerializeField] private GameObject arrowPointer;

    private Transform characterTf;
    private Transform linePoolTf;
    private Transform rangePoolTf;
    private Transform bulletsPoolTf;

    [Header("--- Assignment Variable---")]
    [SerializeField] private ActionState actionState;

    [Header("[Character]")]
    public List<CharacterController> playerList;
    public List<CharacterController> enemyList;
    public CharacterController selectChar;

    [Header("[FieldNode]")]
    [SerializeField] private FieldNode targetNode;

    [HideInInspector] public List<FieldNode> fieldNodes = new List<FieldNode>();
    private List<FieldNode> openNodes = new List<FieldNode>();
    private List<FieldNode> closeNodes = new List<FieldNode>();

    private LineRenderer moveLine;
    private DrawRange currentRange;
    [HideInInspector] public List<LineRenderer> linePool = new List<LineRenderer>();
    [HideInInspector] public List<DrawRange> rangePool = new List<DrawRange>();
    [HideInInspector] public List<Bullet> bulletPool = new List<Bullet>();

    [HideInInspector] public LayerMask nodeLayer;
    [HideInInspector] public LayerMask coverLayer;
    [HideInInspector] public LayerMask watchLayer;

    private readonly int linePoolMax = 15;
    private readonly int bulletPoolMax = 30;

    public void Start()
    {
        dataMgr = FindAnyObjectByType<DataManager>();
        camMgr = FindAnyObjectByType<CameraManager>();
        camMgr.SetComponents(this);
        mapEdt = FindAnyObjectByType<MapEditor>();
        mapEdt.SetComponents(this);

        arrowPointer = GameObject.FindGameObjectWithTag("ArrowPointer");
        arrowPointer.SetActive(false);

        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;
        linePoolTf = GameObject.FindGameObjectWithTag("Lines").transform;
        rangePoolTf = GameObject.FindGameObjectWithTag("Ranges").transform;
        bulletsPoolTf = GameObject.FindGameObjectWithTag("Bullets").transform;
        nodeLayer = LayerMask.GetMask("Node");
        coverLayer = LayerMask.GetMask("Cover");
        watchLayer = LayerMask.GetMask("Cover") | LayerMask.GetMask("Character");

        //CreateCharacter(CharacterOwner.Player, new Vector2(0f, 0f), "C0001");
        CreateLines();
        CreateBullets();
    }

    /// <summary>
    /// 캐릭터 생성
    /// </summary>
    private void CreateCharacter(CharacterOwner ownerType, Vector2 nodePos, string charID)
    {
        var charData = dataMgr.charData.charInfos.Find(x => x.ID == charID);
        var charCtr = Instantiate(Resources.Load<CharacterController>($"Prefabs/Character/{charData.prefabName}"));
        charCtr.transform.SetParent(characterTf, false);
        var node = fieldNodes.Find(x => x.nodePos == nodePos);
        charCtr.transform.position = node.transform.position;
        if (ownerType == CharacterOwner.Enemy)
        {
            charCtr.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        charCtr.SetComponents(this, ownerType, charData, node);

        var weaponData = dataMgr.weaponData.weaponInfos.Find(x => x.ID == charData.mainWeaponID);
        var weapon = Instantiate(Resources.Load<Weapon>($"Prefabs/Weapon/{weaponData.prefabName}"));
        weapon.SetComponets(charCtr, weaponData);

        if (charData.armorID != "None")
        {
            var armorData = dataMgr.armorData.armorInfos.Find(x => x.ID == charData.armorID);
            charCtr.armor = new Armor(armorData);
        }

        CreateRange();
    }

    /// <summary>
    /// 라인 생성
    /// </summary>
    private void CreateLines()
    {
        var width = 0.03f;
        moveLine = Instantiate(Resources.Load<LineRenderer>("Prefabs/DrawLine"));
        moveLine.transform.SetParent(linePoolTf, false);
        moveLine.startWidth = width;
        moveLine.material = Resources.Load<Material>("Materials/Line/DrawLine_Move");
        moveLine.enabled = false;

        for (int i = 0; i < linePoolMax; i++)
        {
            var line = Instantiate(Resources.Load<LineRenderer>("Prefabs/DrawLine"));
            line.transform.SetParent(linePoolTf, false);
            line.startWidth = width;
            line.enabled = false;
            linePool.Add(line);
        }
    }

    /// <summary>
    /// 범위 생성
    /// </summary>
    private void CreateRange()
    {
        var range = Instantiate(Resources.Load<DrawRange>("Prefabs/DrawRange"));
        range.transform.SetParent(rangePoolTf, false);
        range.SetComponents();
        range.gameObject.SetActive(false);
        rangePool.Add(range);
    }

    /// <summary>
    /// 탄환 생성
    /// </summary>
    private void CreateBullets()
    {
        for (int i = 0; i < bulletPoolMax; i++)
        {
            var bullet = Instantiate(Resources.Load<Bullet>("Prefabs/Weapon/Bullet"));
            bullet.transform.SetParent(bulletsPoolTf, false);
            bullet.gameObject.SetActive(false);
            bulletPool.Add(bullet);
        }
    }

    public void Update()
    {
        KeyboardInput();
        MouseInput();
        PointerUpEvent();
        CreateCover();
        CreateEnemy();
    }

    /// <summary>
    /// 키보드 입력
    /// </summary>
    private void KeyboardInput()
    {
        if (selectChar == null) return;

        switch (actionState)
        {
            case ActionState.Move:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ClearLine();
                    selectChar.FindTargets(selectChar.currentNode);
                    if (selectChar.SetTargetOn())
                    {
                        RemoveTargetNode();
                        SwitchMovableNodes(false);
                        actionState = ActionState.Shot;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.R) && selectChar.weapon.loadedAmmo < selectChar.weapon.magMax)
                {
                    ClearLine();
                    selectChar.AddCommand(CommandType.Reload);
                    SwitchMovableNodes(false);
                    selectChar = null;
                    actionState = ActionState.None;
                }
                else if (Input.GetKeyDown(KeyCode.T))
                {
                    RemoveTargetNode();
                    ClearLine();
                    SwitchMovableNodes(false);
                    currentRange = rangePool.Find(x => !x.gameObject.activeSelf);
                    actionState = ActionState.Watch;
                }
                break;
            case ActionState.Shot:
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    selectChar.SetNextTargetOn();
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (selectChar.weapon.chamberBullet)
                    {
                        if (selectChar.animator.GetBool("isCover"))
                        {
                            selectChar.AddCommand(CommandType.Aim);
                            selectChar.AddCommand(CommandType.Shoot);
                            selectChar.AddCommand(CommandType.BackCover);
                        }
                        else
                        {
                            selectChar.AddCommand(CommandType.Aim);
                            selectChar.AddCommand(CommandType.Shoot);
                        }
                        SwitchMovableNodes(false);
                        camMgr.SetCameraState(CameraState.None);
                        selectChar = null;
                        actionState = ActionState.None;
                    }
                    else
                    {
                        Debug.Log($"{selectChar.name}: No Ammo");
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    selectChar.SetTargetOff();
                    var targetInfo = selectChar.targetList[selectChar.targetIndex];
                    targetInfo.target.AddCommand(CommandType.Targeting, false, transform);
                    camMgr.SetCameraState(CameraState.None);
                    selectChar = null;
                    actionState = ActionState.None;
                }
                break;
            case ActionState.Watch:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    selectChar.SetWatch();
                    currentRange = null;
                    selectChar.state = CharacterState.Watch;
                    selectChar = null;
                    actionState = ActionState.None;
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    currentRange.gameObject.SetActive(false);
                    currentRange = null;
                    selectChar = null;
                    actionState = ActionState.None;
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 마우스 입력
    /// </summary>
    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                switch (actionState)
                {
                    case ActionState.None:
                        if (node.charCtr != null && selectChar == null)
                        {
                            selectChar = node.charCtr;
                            if (selectChar.state == CharacterState.Watch)
                            {
                                selectChar.AimOff();
                                if (selectChar.animator.GetBool("isCover"))
                                {
                                    selectChar.AddCommand(CommandType.BackCover);
                                }
                                selectChar.watchInfo.drawRang.gameObject.SetActive(false);
                                selectChar.state = CharacterState.None;
                            }
                            ShowMovableNodes(selectChar);
                            actionState = ActionState.Move;
                        }
                        break;
                    case ActionState.Move:
                        if (node.charCtr != null && selectChar != null && node.charCtr == selectChar)
                        {
                            SwitchMovableNodes(false);
                            ClearLine();
                            RemoveTargetNode();
                            selectChar = null;
                            actionState = ActionState.None;
                        }
                        else if (node.canMove && selectChar != null)
                        {
                            ClearLine();
                            RemoveTargetNode();
                            CharacterMove(selectChar, node);
                            selectChar = null;
                            actionState = ActionState.None;
                        }
                        break;
                    case ActionState.Watch:
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 포인터 업 이벤트
    /// </summary>
    private void PointerUpEvent()
    {
        if (selectChar == null) return;

        var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
        {
            var node = hit.collider.GetComponentInParent<FieldNode>();
            switch (actionState)
            {
                case ActionState.Move:
                    if (targetNode != node && node == selectChar.currentNode)
                    {
                        arrowPointer.SetActive(false);
                        moveLine.enabled = false;
                        RemoveTargetNode();
                        selectChar.FindTargets(node);
                        DrawAimLine(node);
                        node.CheckCoverNode(true);
                        targetNode = node;
                        return;
                    }

                    var find = openNodes.Find(x => x == node);
                    if (targetNode != node && find != null)
                    {
                        RemoveTargetNode();
                        ResultNodePass(selectChar.currentNode, node);
                        DrawMoveLine();

                        selectChar.FindTargets(node);
                        DrawAimLine(node);
                        node.CheckCoverNode(true);
                        targetNode = node;
                    }
                    else if (find == null)
                    {
                        ClearLine();
                        RemoveTargetNode();
                    }
                    break;
                case ActionState.Watch:
                    if (targetNode != node)
                    {
                        if (node != selectChar.currentNode)
                        {
                            currentRange.SetRange(selectChar, node);
                            currentRange.transform.LookAt(node.transform);
                        }
                        targetNode = node;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// TargetNode 제거
    /// </summary>
    private void RemoveTargetNode()
    {
        if (targetNode != null)
        {
            targetNode.CheckCoverNode(false);
        }
        targetNode = null;
    }

    /// <summary>
    /// 이동가능 노드 표시
    /// </summary>
    /// <param name="charCtr"></param>
    private void ShowMovableNodes(CharacterController charCtr)
    {
        SwitchMovableNodes(false);
        openNodes.Add(charCtr.currentNode);
        ChainOfMovableNode(charCtr.currentNode, charCtr.mobility, true);
        SwitchMovableNodes(true);
    }

    /// <summary>
    /// 이동노드 연쇄적용
    /// </summary>
    /// <param name="node"></param>
    /// <param name="mobility"></param>
    /// <param name="isFirst"></param>
    private void ChainOfMovableNode(FieldNode node, int mobility, bool isFirst)
    {
        var canChain = isFirst || (mobility > 0 && node.canMove);
        if (!canChain) return;

        mobility--;
        for (int i = 0; i < node.onAxisNodes.Count; i++)
        {
            var onAxisNode = node.onAxisNodes[i];
            if (onAxisNode == null) continue;

            if (onAxisNode.canMove)
            {
                var find = openNodes.Find(x => x == onAxisNode);
                if (find == null)
                {
                    openNodes.Add(onAxisNode);
                }
                ChainOfMovableNode(onAxisNode, mobility, false);
            }
        }
    }

    /// <summary>
    /// 이동가능 노드 표시변경
    /// </summary>
    /// <param name="value"></param>
    private void SwitchMovableNodes(bool value)
    {
        switch (value)
        {
            case true:
                for (int i = 0; i < openNodes.Count; i++)
                {
                    var movableNode = openNodes[i];
                    movableNode.SetNodeOutLine(openNodes);
                }
                break;
            case false:
                for (int i = 0; i < openNodes.Count; i++)
                {
                    var movableNode = openNodes[i];
                    movableNode.SetNodeOutLine(false);
                }
                openNodes.Clear();
                break;
        }
    }

    /// <summary>
    /// 캐릭터 이동명령
    /// </summary>
    /// <param name="charCtr"></param>
    /// <param name="targetNode"></param>
    private void CharacterMove(CharacterController charCtr, FieldNode targetNode)
    {
        if (charCtr.animator.GetBool("isMove")) return;

        for (int i = 0; i < openNodes.Count; i++)
        {
            var movableNode = openNodes[i];
            movableNode.SetNodeOutLine(false);
        }
        //ResultNodePass(charCtr.currentNode, targetNode);
        if (charCtr.animator.GetCurrentAnimatorStateInfo(0).IsTag("Cover"))
        {
            charCtr.AddCommand(CommandType.LeaveCover);
        }
        charCtr.AddCommand(CommandType.Move, closeNodes);
        charCtr.AddCommand(CommandType.TakeCover);
    }

    /// <summary>
    /// 이동경로 계산
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    private void ResultNodePass(FieldNode startNode, FieldNode endNode)
    {
        closeNodes.Clear();
        var _openNodes = new List<FieldNode>(openNodes);
        FindNodeRoute(startNode, endNode);

        _openNodes = new List<FieldNode>(closeNodes);
        closeNodes.Clear();
        FindNodeRoute(endNode, startNode);

        void FindNodeRoute(FieldNode _startNode, FieldNode _endNode)
        {
            var currentNode = _startNode;
            closeNodes.Add(currentNode);
            FieldNode nextNode = null;
            while (currentNode != _endNode)
            {
                float currentF = 999999f;
                for (int i = 0; i < currentNode.allAxisNodes.Count; i++)
                {
                    var node = currentNode.allAxisNodes[i];
                    var findOpen = _openNodes.Find(x => x == node);
                    var findClose = closeNodes.Find(x => x == node);
                    if (findOpen != null && findClose == null)
                    {
                        var g = DataUtility.GetDistance(currentNode.transform.position, node.transform.position);
                        var h = DataUtility.GetDistance(node.transform.position, _endNode.transform.position);
                        var f = g + h;
                        if (f < currentF)
                        {
                            currentF = f;
                            nextNode = node;
                        }
                    }
                }

                if (nextNode != null)
                {
                    currentNode = nextNode;
                    closeNodes.Add(currentNode);
                    _openNodes.Remove(currentNode);
                    nextNode = null;
                }
                else
                {
                    closeNodes.Remove(currentNode);
                    if (closeNodes.Count == 0)
                    {
                        Debug.Log("not find NodePass");
                        break;
                    }
                    else
                    {
                        currentNode = closeNodes[^1];
                    }
                }
            }
        }
    }

    /// <summary>
    /// 이동경로 라인 그리기
    /// </summary>
    private void DrawMoveLine()
    {
        moveLine.enabled = true;
        moveLine.positionCount = closeNodes.Count;
        var height = 0.1f;
        for (int i = 0; i < closeNodes.Count; i++)
        {
            var pos = closeNodes[i].transform.position;
            pos.y += height;
            moveLine.SetPosition(i, pos);
            if (i == 0)
            {
                arrowPointer.SetActive(true);
                pos = closeNodes[i].transform.position + new Vector3(0f, 0.5f, 0f);
                arrowPointer.transform.position = pos;
            }
        }
    }

    /// <summary>
    /// 조준경로 라인 그리기
    /// </summary>
    /// <param name="node"></param>
    private void DrawAimLine(FieldNode node)
    {
        var height = 0.75f;
        for (int i = 0; i < linePool.Count; i++)
        {
            var line = linePool[i];
            if (i < selectChar.targetList.Count)
            {
                var targetInfo = selectChar.targetList[i];
                var pos = node.transform.position;
                pos.y += height;
                var targetPos = targetInfo.target.currentNode.transform.position;
                targetPos.y += height;
                var dir = Vector3.Normalize(targetPos - pos);
                pos += dir * DataUtility.lineInterval;
                targetPos -= dir * DataUtility.lineInterval;

                line.enabled = true;
                line.positionCount = 2;
                line.SetPosition(0, pos);
                line.SetPosition(1, targetPos);
                if (targetInfo.targetCover == null)
                {
                    line.material = Resources.Load<Material>("Materials/Line/DrawLine_Shoot_NonCover");
                }
                else
                {
                    line.material = Resources.Load<Material>("Materials/Line/DrawLine_Shoot_Cover");
                }
            }
            else
            {
                if (!line.enabled)
                {
                    break;
                }
                else
                {
                    line.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// 라인 제거
    /// </summary>
    private void ClearLine()
    {
        arrowPointer.SetActive(false);
        moveLine.enabled = false;
        for (int i = 0; i < linePool.Count; i++)
        {
            var line = linePool[i];
            if (!line.enabled)
            {
                break;
            }
            else
            {
                line.enabled = false;
            }
        }
    }

    /// <summary>
    /// 엄폐물 생성
    /// </summary>
    private void CreateCover()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var node = GetEmptyNode();
            var cover = Instantiate(Resources.Load<Cover>("Prefabs/Cover"));
            cover.transform.SetParent(node.transform, false);
            //cover.SetComponents(node, CoverType.Half);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            var node = GetEmptyNode();
            var cover = Instantiate(Resources.Load<Cover>("Prefabs/Cover"));
            cover.transform.SetParent(node.transform, false);
            //cover.SetComponents(node, CoverType.Full);
        }

        FieldNode GetEmptyNode()
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                if (node != null && node.canMove)
                {
                    return node;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 적 캐릭터 생성
    /// </summary>
    private void CreateEnemy()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                if (node.canMove)
                {
                    CreateCharacter(CharacterOwner.Enemy, node.nodePos, "C0002");
                }
            }
        }
    }
}
