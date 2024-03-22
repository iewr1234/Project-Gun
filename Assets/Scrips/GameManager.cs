using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("---Access Script---")]
    public CameraManager camMgr;

    //[Header("---Access Component---")]
    private Transform fieldNodeTf;
    private Transform characterTf;
    private Transform bulletsPoolTf;

    [Header("--- Assignment Variable---\n[Character]")]
    public List<CharacterController> playerList;
    public List<CharacterController> enemyList;
    public CharacterController selectChar;

    [Header("[FieldNode]")]
    [SerializeField] private Vector2 fieldSize;
    private LayerMask nodeLayer;

    [HideInInspector] public List<FieldNode> fieldNodes = new List<FieldNode>();
    private List<FieldNode> openNodes = new List<FieldNode>();
    private List<FieldNode> closeNodes = new List<FieldNode>();

    [HideInInspector] public List<Bullet> bulletPool = new List<Bullet>();

    private readonly int bulletPoolMax = 50;

    public void Start()
    {
        camMgr = FindAnyObjectByType<CameraManager>();
        camMgr.SetComponents();

        fieldNodeTf = GameObject.FindGameObjectWithTag("FieldNodes").transform;
        characterTf = GameObject.FindGameObjectWithTag("Characters").transform;
        bulletsPoolTf = GameObject.FindGameObjectWithTag("Bullets").transform;
        nodeLayer = LayerMask.GetMask("Node");

        CreateField();
        CreateCharacter(CharacterOwner.Player, new Vector2(0f, 0f), "Soldier_A", "Rifle_01");
        CreateCharacter(CharacterOwner.Enemy, new Vector2(fieldSize.x - 1, fieldSize.y - 1), "Insurgent_A", "Rifle_02");
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
        CreateCover();
    }

    private void KeyboardInput()
    {
        if (selectChar == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var target = FindTarget(selectChar);
            if (target != null && selectChar.weapon.magAmmo > 0)
            {
                selectChar.AddCommand(CommandType.Shoot, target);
                for (int i = 0; i < openNodes.Count; i++)
                {
                    var openNode = openNodes[i];
                    openNode.NodeColor = Color.gray;
                }
                openNodes.Clear();
                selectChar = null;
            }
            else if (target != null && selectChar.weapon.magAmmo == 0)
            {
                Debug.Log($"{selectChar.name}: No Ammo");
            }
            else
            {
                Debug.Log($"{selectChar.name}: No Target");
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

    private CharacterController FindTarget(CharacterController shooter)
    {
        CharacterController shootTarget = null;
        var targetList = new List<CharacterController>();
        switch (shooter.ownerType)
        {
            case CharacterOwner.Player:
                targetList = enemyList;
                break;
            case CharacterOwner.Enemy:
                targetList = playerList;
                break;
            default:
                break;
        }

        var targetDist = 999999f;
        for (int i = 0; i < targetList.Count; i++)
        {
            var target = targetList[i];
            var dist = DataUtility.GetDistance(shooter.transform.position, target.transform.position);
            if (dist < targetDist && dist <= shooter.weapon.range)
            {
                targetDist = dist;
                shootTarget = target;
            }
        }

        return shootTarget;
    }

    private void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeLayer))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                if (node.charCtr != null && selectChar == null)
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

        ResultNodePass(charCtr.currentNode, targetNode);
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
                if (node.cover == null)
                {
                    var cover = Instantiate(Resources.Load<Cover>("Prefabs/Cover"));
                    cover.transform.SetParent(node.transform, false);
                    cover.SetComponents(node);
                }
            }
        }
    }

    private void ResultNodePass(FieldNode startNode, FieldNode endNode)
    {
        for (int i = 0; i < openNodes.Count; i++)
        {
            var openNode = openNodes[i];
            openNode.NodeColor = Color.gray;
        }
        closeNodes.Clear();
        FindNodeRoute(startNode, endNode);

        openNodes.Clear();
        for (int i = 0; i < closeNodes.Count; i++)
        {
            var closeNode = closeNodes[i];
            openNodes.Add(closeNode);
        }
        ReverseResultNodePass(endNode, startNode);
    }

    private void ReverseResultNodePass(FieldNode startNode, FieldNode endNode)
    {
        closeNodes.Clear();
        FindNodeRoute(startNode, endNode);
    }

    private void FindNodeRoute(FieldNode startNode, FieldNode endNode)
    {
        var currentNode = startNode;
        closeNodes.Add(currentNode);
        FieldNode nextNode = null;
        while (currentNode != endNode)
        {
            float currentF = 999999f;
            for (int i = 0; i < currentNode.allAxisNodes.Count; i++)
            {
                var node = currentNode.allAxisNodes[i];
                var findOpen = openNodes.Find(x => x == node);
                var findClose = closeNodes.Find(x => x == node);
                if (findOpen != null && findClose == null)
                {
                    var g = DataUtility.GetDistance(currentNode.transform.position, node.transform.position);
                    var h = DataUtility.GetDistance(node.transform.position, endNode.transform.position);
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
                openNodes.Remove(currentNode);
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
