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
    [Space(5f)]

    public bool canMove;

    [HideInInspector] public Vector2 nodePos;
    [HideInInspector] public List<FieldNode> onAxisNodes;
    [HideInInspector] public List<FieldNode> offAxisNodes;
    [HideInInspector] public List<FieldNode> allAxisNodes = new List<FieldNode>();

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

                var node = gameMgr.fieldNodes.Find(x => x.nodePos == new Vector2(nodePos.x + i, nodePos.y + j));
                if (node != null && i != 0 && j != 0)
                {
                    offAxisNodes.Add(node);
                    allAxisNodes.Add(node);
                }
                else if (node != null)
                {
                    onAxisNodes.Add(node);
                    allAxisNodes.Add(node);
                }
            }
        }
    }

    public void ReleaseAdjacentNodes()
    {
        var nodeList = new List<FieldNode>();
        for (int i = 0; i < onAxisNodes.Count; i++)
        {
            nodeList.Clear();
            var onAxisNode = onAxisNodes[i];
            for (int j = 0; j < onAxisNode.offAxisNodes.Count; j++)
            {
                var offAxisNode = onAxisNode.offAxisNodes[j];
                var find = onAxisNodes.Find(x => x == offAxisNode);
                if (find != null)
                {
                    nodeList.Add(offAxisNode);
                }
            }

            for (int j = 0; j < nodeList.Count; j++)
            {
                var node = nodeList[j];
                onAxisNode.offAxisNodes.Remove(node);
                onAxisNode.allAxisNodes.Remove(node);
            }
        }
    }

    public Color NodeColor
    {
        set { mesh.material.color = value; }
        get { return mesh.material.color; }
    }
}
