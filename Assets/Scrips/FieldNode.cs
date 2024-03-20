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
    public CharacterController charCtr;
    public Cover cover;

    [HideInInspector] public Vector2 nodePos;
    [HideInInspector] public bool canMove;

    public List<FieldNode> orthogonalNodes;
    public List<FieldNode> diagonalNodes;
    [HideInInspector] public List<FieldNode> adjacentNodes = new List<FieldNode>();

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

    public void ReleaseAdjacentNodes()
    {
        var nodeList = new List<FieldNode>();
        for (int i = 0; i < orthogonalNodes.Count; i++)
        {
            nodeList.Clear();
            var orthogonalNode = orthogonalNodes[i];
            for (int j = 0; j < orthogonalNode.diagonalNodes.Count; j++)
            {
                var diagonalNode = orthogonalNode.diagonalNodes[j];
                var find = orthogonalNodes.Find(x => x == diagonalNode);
                if (find != null)
                {
                    nodeList.Add(diagonalNode);
                }
            }

            for (int j = 0; j < nodeList.Count; j++)
            {
                var node = nodeList[j];
                orthogonalNode.diagonalNodes.Remove(node);
                orthogonalNode.adjacentNodes.Remove(node);
            }
        }
    }

    public Color NodeColor
    {
        set { mesh.material.color = value; }
        get { return mesh.material.color; }
    }
}
