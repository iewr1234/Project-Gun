using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FieldNode : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;

    [Header("---Access Component---")]
    [SerializeField] private MeshRenderer mesh;

    [Header("--- Assignment Variable---")]
    public Vector2 nodePos;
    public List<FieldNode> orthogonalNodes;
    public List<FieldNode> diagonalNodes;
    [HideInInspector] public List<FieldNode> adjacentNodes = new List<FieldNode>();

    [Space(5f)]
    public bool canMove;

    public void SetComponents(GameManager _gameMgr, Vector2 _nodePos)
    {
        gameMgr = _gameMgr;
        nodePos = _nodePos;

        transform.name = $"Node_({nodePos.x}, {nodePos.y})";
        mesh = GetComponentInChildren<MeshRenderer>();
        var material = new Material(Resources.Load<Material>("Materials/Node"));
        mesh.material = material;

        canMove = true;
    }

    public void AddAdjacentNodes()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                var adjacentNode = gameMgr.fieldNodes.Find(x => x.nodePos == new Vector2(nodePos.x + i, nodePos.y + j));
                if (adjacentNode != null && i != 0 && j != 0)
                {
                    diagonalNodes.Add(adjacentNode);
                    adjacentNodes.Add(adjacentNode);
                }
                else if (adjacentNode != null)
                {
                    orthogonalNodes.Add(adjacentNode);
                    adjacentNodes.Add(adjacentNode);
                }
            }
        }
    }

    public void GetAdjacentNodes(ref List<FieldNode> openNodes, ref List<FieldNode> closeNodes)
    {
        for (int i = 0; i < adjacentNodes.Count; i++)
        {
            var adjacentNode = adjacentNodes[i];
            var findClose = closeNodes.Find(x => x == adjacentNode);
            if (findClose == null && adjacentNode.canMove)
            {
                var findOpen = openNodes.Find(x => x == adjacentNode);
                if (findOpen == null)
                {
                    openNodes.Add(adjacentNode);
                }
            }
        }
    }

    public Color NodeColor
    {
        set { mesh.material.color = value; }
        get { return mesh.material.color; }
    }
}
