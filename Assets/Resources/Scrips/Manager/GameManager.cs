using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum ActionState
{
    None,
    Move,
    Shot,
    Watch,
}

[System.Serializable]
public struct MovePass
{
    public string indexName;
    public List<FieldNode> passNodes;
    public int moveNum;
}

public class GameManager : MonoBehaviour
{
    [Header("---Access Script---")]
    public DataManager dataMgr;
    public CameraManager camMgr;
    public UserInterfaceManager uiMgr;
    public MapEditor mapEdt;

    [Header("---Access Component---")]
    [SerializeField] private ArrowPointer arrowPointer;
    [SerializeField] private Collider targetCheck;

    private Transform characterTf;
    private Transform linePoolTf;
    private Transform rangePoolTf;
    private Transform bulletsPoolTf;
    private Transform warningPoolTf;
    private Transform passPointPoolTf;

    [Header("--- Assignment Variable---")]
    [SerializeField] private ActionState actionState;
    [HideInInspector] public List<FieldNode> fieldNodes = new List<FieldNode>();

    [Header("[Move]")]
    [SerializeField] private bool addPass;
    private int expectedAction;
    private int expectedStamina;

    [SerializeField] private List<FieldNode> movableNodes = new List<FieldNode>();
    [SerializeField] private List<FieldNode> openNodes = new List<FieldNode>();
    [SerializeField] private List<FieldNode> closeNodes = new List<FieldNode>();
    [SerializeField] private List<MovePass> passList = new List<MovePass>();
    [SerializeField] private int moveNum;
    private LineRenderer moveLine;

    [Header("[Character]")]
    public List<CharacterController> playerList;
    public List<CharacterController> enemyList;
    public CharacterController selectChar;

    [Header("[FieldNode]")]
    [SerializeField] private FieldNode targetNode;

    private DrawRange currentRange;
    [HideInInspector] public List<LineRenderer> linePool = new List<LineRenderer>();
    [HideInInspector] public List<DrawRange> rangePool = new List<DrawRange>();
    [HideInInspector] public List<Bullet> bulletPool = new List<Bullet>();
    [HideInInspector] public List<FireWarning> warningPool = new List<FireWarning>();
    [HideInInspector] public List<MeshRenderer> passPointPool = new List<MeshRenderer>();

    [HideInInspector] public LayerMask nodeLayer;
    [HideInInspector] public LayerMask coverLayer;
    [HideInInspector] public LayerMask watchLayer;

    private readonly int linePoolMax = 15;
    private readonly int bulletPoolMax = 30;
    private readonly int warningPoolMax = 30;
    private readonly int passPointPoolMax = 30;

    public void Start()
    {
        dataMgr = FindAnyObjectByType<DataManager>();
        camMgr = FindAnyObjectByType<CameraManager>();
        camMgr.SetComponents(this);
        uiMgr = FindAnyObjectByType<UserInterfaceManager>();
        uiMgr.SetComponents(this);
        mapEdt = FindAnyObjectByType<MapEditor>();
        mapEdt.SetComponents(this);

        arrowPointer = FindAnyObjectByType<ArrowPointer>();
        arrowPointer.SetComponents();
        arrowPointer.gameObject.SetActive(false);
        targetCheck = GameObject.FindGameObjectWithTag("TargetCheck").GetComponent<Collider>();
        targetCheck.gameObject.SetActive(false);

        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;
        linePoolTf = GameObject.FindGameObjectWithTag("Lines").transform;
        rangePoolTf = GameObject.FindGameObjectWithTag("Ranges").transform;
        bulletsPoolTf = GameObject.FindGameObjectWithTag("Bullets").transform;
        warningPoolTf = GameObject.FindGameObjectWithTag("Warnings").transform;
        passPointPoolTf = GameObject.FindGameObjectWithTag("PassPoints").transform;
        nodeLayer = LayerMask.GetMask("Node");
        coverLayer = LayerMask.GetMask("Cover");
        watchLayer = LayerMask.GetMask("Cover") | LayerMask.GetMask("Character");

        CreateLines();
        CreateBullets();
        CreateWarnings();
        CreatePassPoint();
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

        // Set Weapons
        var weaponIDs = new string[3] { charData.mainWeapon1_ID, charData.mainWeapon2_ID, charData.subWeapon_ID };
        for (int i = 0; i < weaponIDs.Length; i++)
        {
            var weaponID = weaponIDs[i];
            var weaponData = dataMgr.weaponData.weaponInfos.Find(x => x.ID == weaponID);
            if (weaponData != null)
            {
                var weapon = Instantiate(Resources.Load<Weapon>($"Prefabs/Weapon/{weaponData.prefabName}"));
                weapon.SetComponets(charCtr, weaponData);
            }
        }
        for (int i = 0; i < charCtr.weapons.Count; i++)
        {
            var weapon = charCtr.weapons[i];
            if (charCtr.currentWeapon == null)
            {
                weapon.EquipWeapon();
                weapon.WeaponSwitching("Right");
                charCtr.currentWeapon = weapon;
            }
            else
            {
                weapon.WeaponSwitching("Holster");
            }
        }

        // Set Armor
        if (charData.armorID != "None")
        {
            var armorData = dataMgr.armorData.armorInfos.Find(x => x.ID == charData.armorID);
            charCtr.armor = new Armor(armorData);
        }

        // Set CharacterUI
        var charUI = Instantiate(Resources.Load<CharacterUI>("Prefabs/Character/CharacterUI"));
        charUI.transform.SetParent(characterTf, false);
        charUI.SetComponents(charCtr);
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

    private void CreateWarnings()
    {
        for (int i = 0; i < warningPoolMax; i++)
        {
            var fireWarning = Instantiate(Resources.Load<FireWarning>("Prefabs/FireWarning"));
            fireWarning.transform.SetParent(warningPoolTf, false);
            fireWarning.SetComponents();
            fireWarning.gameObject.SetActive(false);
            warningPool.Add(fireWarning);
        }
    }

    private void CreatePassPoint()
    {
        for (int i = 0; i < passPointPoolMax; i++)
        {
            var passPoint = Instantiate(Resources.Load<MeshRenderer>("Prefabs/PassPoint"));
            passPoint.transform.SetParent(passPointPoolTf, false);
            passPoint.gameObject.SetActive(false);
            passPointPool.Add(passPoint);
        }
    }

    public void Update()
    {
        KeyboardInput();
        MouseInput();
        PointerUpEvent();
        CreatePlayer();
        CreateEnemy();
        TurnEnd();

        void TurnEnd()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                for (int i = 0; i < playerList.Count; i++)
                {
                    var player = playerList[i];
                    player.SetAction(player.maxAction);
                    player.SetStamina(player.maxStamina);
                }
                for (int i = 0; i < enemyList.Count; i++)
                {
                    var enemy = enemyList[i];
                    enemy.SetAction(enemy.maxAction);
                    enemy.SetStamina(enemy.maxStamina);
                }
            }
        }
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
                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space))
                {
                    ClearLine();
                    selectChar.FindTargets(selectChar.currentNode);
                    if (selectChar.SetTargetOn())
                    {
                        RemoveTargetNode();
                        SwitchMovableNodes(false);
                        SwitchCharacterUI(false);
                        actionState = ActionState.Shot;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.R) && selectChar.currentWeapon.loadedAmmo < selectChar.currentWeapon.magMax)
                {
                    ClearLine();
                    selectChar.AddCommand(CommandType.Reload);
                    SwitchMovableNodes(false);
                    selectChar = null;
                    actionState = ActionState.None;
                }
                else if (Input.GetKeyDown(KeyCode.X) && selectChar.weapons.Count > 1)
                {
                    selectChar.AddCommand(CommandType.ChangeWeapon);
                }
                else if (Input.GetKeyDown(KeyCode.T))
                {
                    RemoveTargetNode();
                    ClearLine();
                    SwitchMovableNodes(false);
                    currentRange = rangePool.Find(x => !x.gameObject.activeSelf);
                    actionState = ActionState.Watch;
                }
                else if (Input.GetKeyDown(KeyCode.Escape) && passList.Count > 0)
                {
                    ClearPassPoint();
                    ShowMovableNodes(selectChar);
                    ResultNodePass(selectChar, targetNode);
                    DrawMoveLine();
                }

                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    addPass = true;
                }
                else if (Input.GetKeyUp(KeyCode.LeftControl))
                {
                    addPass = false;
                }
                break;
            case ActionState.Shot:
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    selectChar.SetNextTargetOn();
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    uiMgr.SetfireRateGauge();
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    uiMgr.SetSightGauge();
                }
                else if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space))
                {
                    if (selectChar.currentWeapon.chamberBullet)
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
                        SwitchCharacterUI(true);
                        camMgr.SetCameraState(CameraState.None);
                        uiMgr.SetActiveAimUI(false);
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
                    var targetInfo = selectChar.targetList[selectChar.targetIndex];
                    targetInfo.target.AddCommand(CommandType.Targeting, false, transform);
                    camMgr.SetCameraState(CameraState.None);
                    selectChar.SetTargetOff();
                    selectChar = null;
                    SwitchCharacterUI(true);
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

        void SwitchCharacterUI(bool value)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                var player = playerList[i];
                player.charUI.gameObject.SetActive(value);
            }
            for (int i = 0; i < enemyList.Count; i++)
            {
                var enemy = enemyList[i];
                enemy.charUI.gameObject.SetActive(value);
            }
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
                        else if (node == targetNode && node.canMove && selectChar != null)
                        {
                            ClearLine();
                            RemoveTargetNode();
                            CharacterMove(selectChar, node);
                            selectChar = null;
                            actionState = ActionState.None;
                        }
                        break;
                    case ActionState.Watch:
                        selectChar.SetWatch();
                        currentRange = null;
                        selectChar.state = CharacterState.Watch;
                        selectChar = null;
                        actionState = ActionState.None;
                        break;
                    default:
                        break;
                }
            }
        }
        else if (Input.GetMouseButtonDown(1) && selectChar != null)
        {
            switch (actionState)
            {
                case ActionState.Move:
                    if (addPass)
                    {
                        if (passList.Count > 0 && passList[0].passNodes[0] == targetNode) return;

                        var passPoint = passPointPool.Find(x => !x.gameObject.activeSelf);
                        var pos = closeNodes[0].transform.position;
                        pos.y += 0.1f;
                        passPoint.transform.position = pos;
                        passPoint.gameObject.SetActive(true);
                        var movePass = new MovePass()
                        {
                            indexName = $"PassPoint_{closeNodes[0].name}",
                            passNodes = new List<FieldNode>(closeNodes),
                            moveNum = moveNum,
                        };
                        closeNodes.Clear();
                        passList.Insert(0, movePass);
                        ShowMovableNodes(selectChar, passList[0].passNodes[0]);
                    }
                    break;
                case ActionState.Shot:
                    break;
                case ActionState.Watch:
                    break;
                default:
                    break;
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
                    if (targetNode != node && node == selectChar.currentNode && passList.Count == 0)
                    {
                        arrowPointer.gameObject.SetActive(false);
                        moveLine.enabled = false;
                        ClearFireWarning();
                        RemoveTargetNode();
                        selectChar.FindTargets(node);
                        DrawAimLine(node);
                        node.CheckCoverNode(true);
                        targetNode = node;
                        return;
                    }

                    if (targetNode != node && movableNodes.Contains(node))
                    {
                        RemoveTargetNode();
                        ResultNodePass(selectChar, node);
                        DrawMoveLine();

                        selectChar.FindTargets(node);
                        DrawAimLine(node);
                        node.CheckCoverNode(true);
                        targetNode = node;
                    }
                    //else if (!movableNodes.Contains(node))
                    //{
                    //    ClearLine();
                    //    RemoveTargetNode();
                    //}
                    break;
                case ActionState.Watch:
                    if (targetNode != node)
                    {
                        if (node != null && node != selectChar.currentNode)
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
        Queue<FieldNode> queue = new Queue<FieldNode>();
        HashSet<FieldNode> visited = new HashSet<FieldNode>();

        queue.Enqueue(charCtr.currentNode);
        visited.Add(charCtr.currentNode);

        var action = charCtr.action;
        if (charCtr.stamina < action * 5)
        {
            action = (int)(charCtr.stamina * 0.2f);
        }
        charCtr.maxMoveNum = (int)(charCtr.Mobility * action);

        int moveRange = 0;
        while (queue.Count > 0 && moveRange <= charCtr.maxMoveNum)
        {
            int nodesInCurrentRange = queue.Count;
            for (int i = 0; i < nodesInCurrentRange; i++)
            {
                FieldNode node = queue.Dequeue();
                movableNodes.Add(node); // 이동 가능 노드로 추가

                foreach (FieldNode neighbor in node.onAxisNodes)
                {
                    if (neighbor != null && neighbor.canMove && !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            moveRange++;
        }
        SwitchMovableNodes(true);
    }

    private void ShowMovableNodes(CharacterController charCtr, FieldNode currentNode)
    {
        SwitchMovableNodes(false);
        Queue<FieldNode> queue = new Queue<FieldNode>();
        HashSet<FieldNode> visited = new HashSet<FieldNode>();

        queue.Enqueue(currentNode);
        visited.Add(currentNode);

        var moveNum = charCtr.maxMoveNum - passList.Sum(x => x.moveNum);
        int moveRange = 0;
        while (queue.Count > 0 && moveRange <= moveNum)
        {
            int nodesInCurrentRange = queue.Count;
            for (int i = 0; i < nodesInCurrentRange; i++)
            {
                FieldNode node = queue.Dequeue();
                movableNodes.Add(node); // 이동 가능 노드로 추가

                foreach (FieldNode neighbor in node.onAxisNodes)
                {
                    if (neighbor != null && neighbor.canMove && !visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            moveRange++;
        }
        SwitchMovableNodes(true);
    }

    #region ShowMovableNodes(Old)
    ///// <summary>
    ///// 이동가능 노드 표시
    ///// </summary>
    ///// <param name="charCtr"></param>
    //private void ShowMovableNodes(CharacterController charCtr)
    //{
    //    SwitchMovableNodes(false);
    //    openNodes.Add(charCtr.currentNode);
    //    var mobility = (int)DataUtility.GetFloorValue(charCtr.mobility * charCtr.action, 0);
    //    ChainOfMovableNode(charCtr.currentNode, mobility);
    //    SwitchMovableNodes(true);
    //}

    ///// <summary>
    ///// 이동노드 연쇄적용
    ///// </summary>
    ///// <param name="node"></param>
    ///// <param name="mobility"></param>
    //private void ChainOfMovableNode(FieldNode node, int mobility)
    //{
    //    mobility--;
    //    for (int i = 0; i < node.onAxisNodes.Count; i++)
    //    {
    //        var onAxisNode = node.onAxisNodes[i];
    //        if (onAxisNode == null) continue;

    //        if (CheckMoveOfNextNode(node, i))
    //        {
    //            if (!openNodes.Contains(onAxisNode))
    //            {
    //                openNodes.Add(onAxisNode);
    //            }
    //            ChainOfMovableNode(node, onAxisNode, mobility);
    //        }
    //    }
    //}

    ///// <summary>
    ///// 이동노드 연쇄적용
    ///// </summary>
    ///// <param name="prevNode"></param>
    ///// <param name="node"></param>
    ///// <param name="mobility"></param>
    //private void ChainOfMovableNode(FieldNode prevNode, FieldNode node, int mobility)
    //{
    //    var canChain = mobility > 0 && node.canMove;
    //    if (!canChain) return;

    //    mobility--;
    //    for (int i = 0; i < node.onAxisNodes.Count; i++)
    //    {
    //        var onAxisNode = node.onAxisNodes[i];
    //        if (onAxisNode == null || onAxisNode == prevNode) continue;

    //        if (CheckMoveOfNextNode(node, i))
    //        {
    //            if (!openNodes.Contains(onAxisNode))
    //            {
    //                openNodes.Add(onAxisNode);
    //            }
    //            ChainOfMovableNode(node, onAxisNode, mobility);
    //        }
    //    }
    //}

    //private bool CheckMoveOfNextNode(FieldNode node, int index)
    //{
    //    var nextNode = node.onAxisNodes[index];
    //    if (!nextNode.canMove)
    //    {
    //        return false;
    //    }
    //    else
    //    {
    //        var outline = node.outlines[index];
    //        if (outline == null)
    //        {
    //            Debug.LogError("Not Found outline");
    //        }

    //        if (outline.lineCover == null && !outline.unableMove.enabled)
    //        {
    //            return true;
    //        }
    //        else if (outline.lineCover != null && outline.lineCover.coverType == CoverType.Half)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //}
    #endregion

    /// <summary>
    /// 이동가능 노드 표시변경
    /// </summary>
    /// <param name="value"></param>
    private void SwitchMovableNodes(bool value)
    {
        switch (value)
        {
            case true:
                for (int i = 0; i < movableNodes.Count; i++)
                {
                    var movableNode = movableNodes[i];
                    movableNode.SetNodeOutline(movableNodes);
                }
                break;
            case false:
                for (int i = 0; i < movableNodes.Count; i++)
                {
                    var movableNode = movableNodes[i];
                    movableNode.SetNodeOutline(false);
                }
                movableNodes.Clear();
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

        for (int i = 0; i < movableNodes.Count; i++)
        {
            var movableNode = movableNodes[i];
            movableNode.SetNodeOutline(false);
        }

        var movePass = closeNodes;
        if (passList.Count > 0)
        {
            foreach (var pass in passList)
            {
                var newPass = movePass.Concat(pass.passNodes).Distinct().ToList();
                movePass = newPass;
            }
            ClearPassPoint();
        }

        if (charCtr.animator.GetCurrentAnimatorStateInfo(0).IsTag("Cover"))
        {
            charCtr.AddCommand(CommandType.LeaveCover);
        }
        charCtr.AddCommand(CommandType.Move, arrowPointer.GetMoveCost(), movePass);
        charCtr.AddCommand(CommandType.TakeCover);
    }

    /// <summary>
    /// 이동경로 계산
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    private void ResultNodePass(CharacterController charCtr, FieldNode endNode)
    {
        openNodes.Clear();
        closeNodes.Clear();

        //var startNode = charCtr.currentNode;
        var startNode = passList.Count > 0 ? passList[0].passNodes[0] : charCtr.currentNode;
        startNode.G = 0f;
        startNode.H = DataUtility.GetDistance(startNode.transform.position, endNode.transform.position);
        openNodes.Add(startNode);
        while (openNodes.Count > 0)
        {
            var currentNode = GetNodeWithLowestF();
            openNodes.Remove(currentNode);
            closeNodes.Add(currentNode);
            if (currentNode == endNode)
            {
                ConstructNodePath();
                break;
            }

            for (int i = 0; i < currentNode.allAxisNodes.Count; i++)
            {
                var axisNode = currentNode.allAxisNodes[i];
                if (closeNodes.Contains(axisNode)) continue;
                if (!movableNodes.Contains(axisNode)) continue;

                var _G = currentNode.G + DataUtility.GetDistance(currentNode.transform.position, axisNode.transform.position);
                if (!openNodes.Contains(axisNode) || _G < axisNode.G)
                {
                    axisNode.parentNode = currentNode;
                    axisNode.G = _G;
                    axisNode.H = DataUtility.GetDistance(axisNode.transform.position, endNode.transform.position);

                    if (!openNodes.Contains(axisNode)) openNodes.Add(axisNode);
                }
            }
        }

        FieldNode GetNodeWithLowestF()
        {
            var lowestNode = openNodes[0];
            for (int i = 0; i < openNodes.Count; i++)
            {
                var node = openNodes[i];
                if (node.F < lowestNode.F)
                {
                    lowestNode = node;
                }
            }
            return lowestNode;
        }

        void ConstructNodePath()
        {
            var passList = new List<FieldNode>();
            var currentNode = closeNodes[^1];
            while (currentNode != startNode)
            {
                passList.Add(currentNode);
                currentNode = currentNode.parentNode;
            }
            passList.Add(startNode);
            closeNodes = passList;
        }
    }

    /// <summary>
    /// 이동경로 라인 그리기
    /// </summary>
    private void DrawMoveLine()
    {
        ClearFireWarning();
        moveNum = 0;
        moveLine.enabled = true;
        moveLine.positionCount = closeNodes.Count + passList.Sum(x => x.passNodes.Count - 1);
        var height = 0.1f;
        var passNodes = new List<FieldNode>();
        for (int i = 0; i < closeNodes.Count; i++)
        {
            var node = closeNodes[i];
            moveNum += RanderAndCheck(node, i, closeNodes);
            passNodes.Add(node);
        }
        foreach (var movePass in passList)
        {
            for (int i = 0; i < movePass.passNodes.Count; i++)
            {
                var node = movePass.passNodes[i];
                RanderAndCheck(node, i, movePass.passNodes);
                if (i > 0) passNodes.Add(node);
            }
        }
        for (int i = 0; i < passNodes.Count; i++)
        {
            var passNode = passNodes[i];
            moveLine.SetPosition(i, passNode.transform.position + new Vector3(0f, height, 0f));
        }

        arrowPointer.gameObject.SetActive(true);
        arrowPointer.transform.position = closeNodes[0].transform.position + new Vector3(0f, 0.5f, 0f);
        var moveCost = (int)Mathf.Ceil((moveNum + passList.Sum(x => x.moveNum)) / selectChar.Mobility);
        arrowPointer.SetMoveCost(moveCost);

        int RanderAndCheck(FieldNode node, int index, List<FieldNode> nodes)
        {
            var moveNum = 0;
            var pos = node.transform.position;
            if (index + 1 < nodes.Count)
            {
                var nextNode = nodes[index + 1];
                if (node.nodePos.x != nextNode.nodePos.x && node.nodePos.y != nextNode.nodePos.y)
                {
                    moveNum += 2;
                }
                else
                {
                    moveNum++;
                }

                if (selectChar.ownerType == CharacterOwner.Player)
                {
                    selectChar.CheckWatcher(node);
                }
            }
            return moveNum;
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
        arrowPointer.gameObject.SetActive(false);
        moveLine.enabled = false;
        ClearFireWarning();
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

    private void ClearPassPoint()
    {
        passList.Clear();
        for (int i = 0; i < passPointPool.Count; i++)
        {
            var passPoint = passPointPool[i];
            if (passPoint.gameObject.activeSelf)
            {
                passPoint.gameObject.SetActive(false);
            }
            else
            {
                break;
            }
        }
    }

    public void SetFireWarning(FieldNode node)
    {
        var fireWarning = warningPool.Find(x => !x.gameObject.activeSelf);
        fireWarning.gameObject.SetActive(true);
        fireWarning.transform.position = node.transform.position;
    }

    public void ClearFireWarning()
    {
        for (int i = 0; i < warningPool.Count; i++)
        {
            var fireWarning = warningPool[i];
            fireWarning.gameObject.SetActive(false);
        }
    }

    private void CreatePlayer()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                if (node.canMove)
                {
                    CreateCharacter(CharacterOwner.Player, node.nodePos, "C0001");
                }
            }
        }
    }

    /// <summary>
    /// 적 캐릭터 생성
    /// </summary>
    private void CreateEnemy()
    {
        if (Input.GetKeyDown(KeyCode.F2))
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
