using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    [Header("---Access Script---")]
    public CameraManager camMgr;

    [Header("---Access Component---")]
    [SerializeField] private GameObject arrowPointer;

    private Transform fieldNodeTf;
    private Transform characterTf;
    private Transform linePoolTf;
    private Transform bulletsPoolTf;

    [Header("--- Assignment Variable---\n[Character]")]
    public List<CharacterController> playerList;
    public List<CharacterController> enemyList;
    public CharacterController selectChar;
    public bool targeting;

    [Header("[FieldNode]")]
    [SerializeField] private Vector2 fieldSize;

    [HideInInspector] public List<FieldNode> fieldNodes = new List<FieldNode>();
    private List<FieldNode> visibleNodes = new List<FieldNode>();
    private List<FieldNode> openNodes = new List<FieldNode>();
    private List<FieldNode> closeNodes = new List<FieldNode>();
    private FieldNode targetNode;

    private LineRenderer moveLine;
    [HideInInspector] public List<LineRenderer> linePool = new List<LineRenderer>();
    [HideInInspector] public List<Bullet> bulletPool = new List<Bullet>();

    [HideInInspector] public LayerMask nodeLayer;
    [HideInInspector] public LayerMask coverLayer;

    private readonly int linePoolMax = 15;
    private readonly int bulletPoolMax = 30;

    public void Start()
    {
        camMgr = FindAnyObjectByType<CameraManager>();
        camMgr.SetComponents();

        arrowPointer = GameObject.FindGameObjectWithTag("ArrowPointer");
        arrowPointer.SetActive(false);

        fieldNodeTf = GameObject.FindGameObjectWithTag("FieldNodes").transform;
        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;
        linePoolTf = GameObject.FindGameObjectWithTag("Lines").transform;
        bulletsPoolTf = GameObject.FindGameObjectWithTag("Bullets").transform;
        nodeLayer = LayerMask.GetMask("Node");
        coverLayer = LayerMask.GetMask("Cover");

        CreateField();
        CreateCharacter(CharacterOwner.Player, new Vector2(0f, 0f), "Soldier_A", "Rifle_01");
        CreateLines();
        CreateBullets();
    }

    /// <summary>
    /// 필드 생성
    /// </summary>
    private void CreateField()
    {
        var size_X = (int)fieldSize.x;
        var size_Y = (int)fieldSize.y;
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
                fieldNode.SetComponents(this, new Vector2(j, i));
                fieldNode.NodeColor = Color.gray;
                fieldNodes.Add(fieldNode);
            }
        }

        for (int i = 0; i < fieldNodes.Count; i++)
        {
            var node = fieldNodes[i];
            node.AddAdjacentNodes();
        }
    }

    /// <summary>
    /// 캐릭터 생성
    /// </summary>
    /// <param name="ownerType"></param>
    /// <param name="nodePos"></param>
    /// <param name="charName"></param>
    /// <param name="weaponName"></param>
    private void CreateCharacter(CharacterOwner ownerType, Vector2 nodePos, string charName, string weaponName)
    {
        var charCtr = Instantiate(Resources.Load<CharacterController>($"Prefabs/Character/{charName}"));
        charCtr.transform.SetParent(characterTf, false);
        var node = fieldNodes.Find(x => x.nodePos == nodePos);
        charCtr.transform.position = node.transform.position;
        if (ownerType == CharacterOwner.Enemy)
        {
            charCtr.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        charCtr.SetComponents(this, ownerType, node);

        var weapon = Instantiate(Resources.Load<Weapon>($"Prefabs/Weapon/{weaponName}"));
        weapon.SetComponets(charCtr);
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

        if (camMgr.state == CameraState.None)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ClearLine();
                selectChar.FindTargets(selectChar.currentNode);
                if (selectChar.SetTarget())
                {
                    SwitchMovableNodes(false);
                }
            }
            else if (Input.GetKeyDown(KeyCode.R) && selectChar.weapon.loadedAmmo < selectChar.weapon.magMax)
            {
                ClearLine();
                selectChar.AddCommand(CommandType.Reload);
                SwitchMovableNodes(false);
                selectChar = null;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                selectChar.SetNextTarget();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (selectChar.weapon.chamberBullet)
                {
                    if (selectChar.animator.GetBool("isCover"))
                    {
                        selectChar.AddCommand(CommandType.CoverAim);
                        selectChar.AddCommand(CommandType.Shoot);
                        selectChar.AddCommand(CommandType.BackCover);
                    }
                    else
                    {
                        selectChar.AddCommand(CommandType.Shoot);
                    }
                    SwitchMovableNodes(false);
                    camMgr.SetCameraState(CameraState.None);
                    selectChar = null;
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
                selectChar = null;
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
                if (node.charCtr != null && selectChar != null && node.charCtr == selectChar)
                {
                    SwitchMovableNodes(false);
                    ClearLine();
                    RemoveTargetNode();
                    selectChar = null;
                }
                else if (node.charCtr != null && selectChar == null)
                {
                    selectChar = node.charCtr;
                    ShowMovableNodes(selectChar);
                }
                else if (node.canMove && selectChar != null)
                {
                    ClearLine();
                    RemoveTargetNode();
                    CharacterMove(selectChar, node);
                    selectChar = null;
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
            if (node == selectChar.currentNode)
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
    /// 시야 내의 노드들을 표시
    /// </summary>
    /// <param name="sight"></param>
    /// <param name="node"></param>
    public void ShowVisibleNodes(int sight, FieldNode node)
    {
        SwitchVisibleNode(false);
        visibleNodes.Clear();
        var findNodes = fieldNodes.FindAll(x => DataUtility.GetDistance(x.transform.position, node.transform.position) < sight);
        for (int i = 0; i < findNodes.Count; i++)
        {
            var findNode = findNodes[i];
            var pos = node.transform.position;
            var targetPos = findNode.transform.position;
            if (!CheckSight())
            {
                visibleNodes.Add(findNode);
                continue;
            }

            for (int j = 0; j < node.onAxisNodes.Count; j++)
            {
                var onAxisNode = node.onAxisNodes[j];
                if (onAxisNode != null && onAxisNode.canMove)
                {
                    pos = onAxisNode.transform.position;
                    if (!CheckSight())
                    {
                        visibleNodes.Add(findNode);
                        break;
                    }
                }
            }

            bool CheckSight()
            {
                var dir = Vector3.Normalize(targetPos - pos);
                var dist = DataUtility.GetDistance(pos, targetPos);

                if (Physics.Raycast(pos, dir, out RaycastHit hit, dist, coverLayer))
                {
                    var coverNode = hit.collider.GetComponentInParent<FieldNode>();
                    if (coverNode != null && visibleNodes.Find(x => x == coverNode) == null)
                    {
                        visibleNodes.Add(coverNode);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        SwitchVisibleNode(true);

        void SwitchVisibleNode(bool value)
        {
            for (int i = 0; i < visibleNodes.Count; i++)
            {
                var visibleNode = visibleNodes[i];
                visibleNode.SetVisibleNode(value);
            }
        }
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
    /// <param name="currentNode"></param>
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
                    movableNode.SetMovableNode(openNodes);
                }
                break;
            case false:
                for (int i = 0; i < openNodes.Count; i++)
                {
                    var movableNode = openNodes[i];
                    movableNode.SetMovableNode();
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
            movableNode.SetMovableNode();
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
                var interval = 0.5f;
                var dir = Vector3.Normalize(targetPos - pos);
                pos += dir * interval;
                targetPos -= dir * interval;

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
        if (Input.GetKeyDown(KeyCode.C))
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                if (node.canMove)
                {
                    var cover = Instantiate(Resources.Load<Cover>("Prefabs/Cover"));
                    cover.transform.SetParent(node.transform, false);
                    cover.SetComponents(node);
                }
            }
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
                    CreateCharacter(CharacterOwner.Enemy, node.nodePos, "Insurgent_A", "Rifle_02");
                }
            }
        }
    }
}
