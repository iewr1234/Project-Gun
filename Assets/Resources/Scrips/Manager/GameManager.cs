using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public enum ActionState
{
    None,
    Move,
    Shot,
    Watch,
}

[System.Serializable]
public struct Test
{
    public float moveDist;
    public List<FieldNode> visitedNodes;
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
    [SerializeField] private List<FieldNode> movableNodes = new List<FieldNode>();
    [SerializeField] private List<FieldNode> openNodes = new List<FieldNode>();
    [SerializeField] private List<FieldNode> closeNodes = new List<FieldNode>();
    public List<Test> tests = new List<Test>();

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

    public void Update()
    {
        KeyboardInput();
        MouseInput();
        PointerUpEvent();
        //CreateCover();
        CreatePlayer();
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
                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space))
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
                break;
            case ActionState.Shot:
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    selectChar.SetNextTargetOn();
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
                        camMgr.SetCameraState(CameraState.None);
                        selectChar.charUI.gameObject.SetActive(true);
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
                    selectChar.charUI.gameObject.SetActive(true);
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
                    else if (!movableNodes.Contains(node))
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
        Queue<FieldNode> queue = new Queue<FieldNode>();
        HashSet<FieldNode> visited = new HashSet<FieldNode>();

        queue.Enqueue(charCtr.currentNode);
        visited.Add(charCtr.currentNode);

        var mobility = (int)DataUtility.GetFloorValue(charCtr.mobility * charCtr.action, 0);

        int moveRange = 0;
        while (queue.Count > 0 && moveRange <= mobility)
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
    private void ResultNodePass(CharacterController charCtr, FieldNode endNode)
    {
        openNodes.Clear();
        closeNodes.Clear();

        var startNode = charCtr.currentNode;
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

        void GetNodeWithNewParent(FieldNode node)
        {
            var axisNodes = node.allAxisNodes.Intersect(openNodes).ToList();
            if (axisNodes.Count == 0) return;

            var parentNode = axisNodes[0];
            for (int i = 0; i < axisNodes.Count; i++)
            {
                var axisNode = axisNodes[i];
                if (axisNode.G < parentNode.G)
                {
                    parentNode = axisNode;
                }
            }

            node.parentNode = parentNode;
            node.G = node.parentNode.G + DataUtility.GetDistance(node.transform.position, node.parentNode.transform.position);
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
