using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [Header("---Access Script---")]
    public CameraManager camMgr;

    //[Header("---Access Component---")]
    private Transform fieldNodeTf;
    private Transform characterTf;
    //private Transform copyCharacterTf;
    private Transform bulletsPoolTf;

    [Header("--- Assignment Variable---\n[Character]")]
    public List<CharacterController> playerList;
    public List<CharacterController> enemyList;
    public CharacterController selectChar;
    public bool targeting;

    [Header("[FieldNode]")]
    [SerializeField] private Vector2 fieldSize;
    private LayerMask nodeLayer;

    [HideInInspector] public List<FieldNode> fieldNodes = new List<FieldNode>();
    private List<FieldNode> openNodes = new List<FieldNode>();
    private List<FieldNode> closeNodes = new List<FieldNode>();
    private FieldNode targetNode;

    [HideInInspector] public List<Bullet> bulletPool = new List<Bullet>();

    private readonly int bulletPoolMax = 50;

    public void Start()
    {
        camMgr = FindAnyObjectByType<CameraManager>();
        camMgr.SetComponents();

        fieldNodeTf = GameObject.FindGameObjectWithTag("FieldNodes").transform;
        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;
        //copyCharacterTf = GameObject.FindGameObjectWithTag("CopyCharacters").transform;
        bulletsPoolTf = GameObject.FindGameObjectWithTag("Bullets").transform;
        nodeLayer = LayerMask.GetMask("Node");

        CreateField();
        CreateCharacter(CharacterOwner.Player, new Vector2(0f, 0f), "Soldier_A", "Rifle_01");
        CreateBullets();
    }

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

        //var copyChar = Instantiate(charCtr);
        //copyChar.transform.SetParent(copyCharacterTf);
        //copyChar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        //copyChar.SetComponentsOfCopy(charCtr);
    }

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

    private void KeyboardInput()
    {
        if (selectChar == null) return;

        if (camMgr.state == CameraState.None)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                selectChar.FindTargets();
                if (selectChar.SetTarget())
                {
                    for (int i = 0; i < openNodes.Count; i++)
                    {
                        var openNode = openNodes[i];
                        openNode.NodeColor = Color.gray;
                    }
                    openNodes.Clear();
                }
            }
            else if (Input.GetKeyDown(KeyCode.R) && selectChar.weapon.magAmmo < selectChar.weapon.magMax)
            {
                selectChar.AddCommand(CommandType.Reload);
                for (int i = 0; i < openNodes.Count; i++)
                {
                    var openNode = openNodes[i];
                    openNode.NodeColor = Color.gray;
                }
                openNodes.Clear();
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
                if (selectChar.weapon.magAmmo > 0)
                {
                    //InputCommands(selectChar);
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
                    for (int i = 0; i < openNodes.Count; i++)
                    {
                        var openNode = openNodes[i];
                        openNode.NodeColor = Color.gray;
                    }
                    openNodes.Clear();
                    camMgr.SetCameraState(CameraState.None);
                    selectChar = null;
                }
                else if (selectChar.weapon.magAmmo == 0)
                {
                    Debug.Log($"{selectChar.name}: No Ammo");
                }
                else
                {
                    Debug.Log($"{selectChar.name}: No Target");
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
                    for (int i = 0; i < openNodes.Count; i++)
                    {
                        var movableNode = openNodes[i];
                        movableNode.NodeColor = Color.gray;
                    }
                    openNodes.Clear();
                    selectChar = null;
                }
                else if (node.charCtr != null && selectChar == null)
                {
                    selectChar = node.charCtr;
                    ShowMovableNodes(selectChar);
                }
                else if (node.canMove && selectChar != null)
                {
                    CharacterMove(selectChar, node);
                    selectChar = null;
                }
            }
        }
    }

    private void PointerUpEvent()
    {
        if (selectChar == null) return;

        var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
        {
            var node = hit.collider.GetComponentInParent<FieldNode>();
            if (targetNode != node && openNodes.Find(x => x == node) != null)
            {
                ResultNodePass(selectChar.currentNode, node);
                targetNode = node;
            }
        }
        else
        {
            targetNode = null;
        }
    }

    private void ShowMovableNodes(CharacterController charCtr)
    {
        for (int i = 0; i < openNodes.Count; i++)
        {
            var movableNode = openNodes[i];
            movableNode.NodeColor = Color.gray;
        }
        openNodes.Clear();
        ChainOfMovableNode(charCtr.currentNode, charCtr.currentNode, charCtr.mobility, true);
    }

    private void ChainOfMovableNode(FieldNode currentNode, FieldNode node, int mobility, bool isFirst)
    {
        var canChain = isFirst || (mobility > 0 && node.canMove);
        if (!canChain) return;

        mobility--;
        for (int i = 0; i < node.onAxisNodes.Count; i++)
        {
            var onAxisNode = node.onAxisNodes[i];
            if (onAxisNode != currentNode && onAxisNode.canMove)
            {
                var find = openNodes.Find(x => x == onAxisNode);
                if (find == null)
                {
                    openNodes.Add(onAxisNode);
                }
                onAxisNode.NodeColor = Color.white;
                ChainOfMovableNode(currentNode, onAxisNode, mobility, false);
            }
        }
    }

    private void CharacterMove(CharacterController charCtr, FieldNode targetNode)
    {
        if (charCtr.animator.GetBool("isMove")) return;

        for (int i = 0; i < openNodes.Count; i++)
        {
            var openNode = openNodes[i];
            openNode.NodeColor = Color.gray;
        }
        //ResultNodePass(charCtr.currentNode, targetNode);
        if (charCtr.animator.GetCurrentAnimatorStateInfo(0).IsTag("Cover"))
        {
            charCtr.AddCommand(CommandType.LeaveCover);
        }
        charCtr.AddCommand(CommandType.Move, closeNodes);
        charCtr.AddCommand(CommandType.TakeCover);
    }

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
}
