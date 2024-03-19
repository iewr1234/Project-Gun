using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("---Access Script---")]
    public CameraManager camMgr;

    [Header("---Access Component---")]
    [SerializeField] private Transform fieldNodeTrf;
    [SerializeField] private Transform characterTrf;

    [Header("--- Assignment Variable---\n[Character]")]
    public List<CharacterController> playerList;
    public List<CharacterController> enemyList;

    [Header("[FieldNode]")]
    [SerializeField] private Vector2 fieldSize;
    [SerializeField] private LayerMask layerMask;

    [HideInInspector] public List<FieldNode> fieldNodes = new List<FieldNode>();
    private List<FieldNode> openNodes = new List<FieldNode>();
    private List<FieldNode> closeNodes = new List<FieldNode>();

    private readonly float nodeSize = 1.2f;
    private readonly float nodeInterval = 0.1f;

    public void Start()
    {
        camMgr = FindAnyObjectByType<CameraManager>();
        camMgr.SetComponents();

        fieldNodeTrf = GameObject.FindGameObjectWithTag("FieldNodes").transform;
        characterTrf = GameObject.FindGameObjectWithTag("Characters").transform;
        CreateField();
        CreateCharacter(CharacterOwner.Player);
    }

    public void Update()
    {
        MouseClick();
    }

    private void MouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = camMgr.mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                var node = hit.collider.GetComponentInParent<FieldNode>();
                CharacterMove(playerList[0], node);
            }
        }
    }

    private void CreateField()
    {
        var size_X = (int)fieldSize.x;
        var size_Y = (int)fieldSize.y;
        for (int i = 0; i < size_Y; i++)
        {
            for (int j = 0; j < size_X; j++)
            {
                var fieldNode = Instantiate(Resources.Load<FieldNode>("Prefabs/FieldNode"));
                fieldNode.transform.SetParent(fieldNodeTrf, false);
                var pos = new Vector3(j * nodeSize, 0f, i * nodeSize);
                if (j > 0)
                {
                    pos.x += j * nodeInterval;
                }
                if (i > 0)
                {
                    pos.z += i * nodeInterval;
                }
                fieldNode.transform.position = pos;
                fieldNode.SetComponents(this, new Vector2(i, j));
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

    private void CreateCharacter(CharacterOwner ownerType)
    {
        var charCtr = Instantiate(Resources.Load<CharacterController>("Prefabs/Character/Soldier_A"));
        charCtr.transform.SetParent(characterTrf, false);
        charCtr.transform.position = Vector3.zero;
        var node = fieldNodes.Find(x => x.nodePos == new Vector2(0f, 0f));
        charCtr.SetComponents(this, ownerType, node);
    }

    private void CharacterMove(CharacterController charCtr, FieldNode targetNode)
    {
        ResultNodePass(charCtr.currentNode, targetNode);
        charCtr.AddCommand(CommandType.Move, closeNodes);
    }

    private void ResultNodePass(FieldNode startNode, FieldNode endNode)
    {
        openNodes.Clear();
        for (int i = 0; i < closeNodes.Count; i++)
        {
            var closeNode = closeNodes[i];
            closeNode.NodeColor = Color.gray;
        }
        closeNodes.Clear();

        var currentNode = startNode;
        closeNodes.Add(currentNode);
        currentNode.GetAdjacentNodes(ref openNodes, ref closeNodes);
        FieldNode nextNode = null;
        while (currentNode != endNode)
        {
            float curF = 9999f;
            for (int i = 0; i < currentNode.adjacentNodes.Count; i++)
            {
                var adjacentNode = currentNode.adjacentNodes[i];
                var findOpen = openNodes.Find(x => x == adjacentNode);
                var findClose = closeNodes.Find(x => x == adjacentNode);
                if (findOpen != null && findClose == null)
                {
                    var g = DataUtility.GetDistance(currentNode.transform.position, adjacentNode.transform.position);
                    var h = DataUtility.GetDistance(adjacentNode.transform.position, endNode.transform.position);
                    var f = g + h;
                    if (f < curF)
                    {
                        curF = f;
                        nextNode = adjacentNode;
                    }
                }
            }

            if (nextNode != null)
            {
                currentNode = nextNode;
                closeNodes.Add(currentNode);
                openNodes.Remove(currentNode);
                currentNode.GetAdjacentNodes(ref openNodes, ref closeNodes);
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

        for (int i = 0; i < closeNodes.Count; i++)
        {
            var closeNode = closeNodes[i];
            closeNode.NodeColor = Color.yellow;
        }
    }
}
