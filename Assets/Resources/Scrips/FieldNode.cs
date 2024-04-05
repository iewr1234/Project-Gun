using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class FieldNode : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;

    [Header("---Access Component---")]
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject fog;
    [SerializeField] private TextMeshProUGUI posText;

    [Header("--- Assignment Variable---")]
    public CharacterController charCtr;
    public Cover cover;
    [Space(5f)]

    public bool canSee;
    public bool canMove;

    [HideInInspector] public Vector2 nodePos;
    [HideInInspector] public List<FieldNode> onAxisNodes;
    [HideInInspector] public List<FieldNode> offAxisNodes;
    [HideInInspector] public List<FieldNode> allAxisNodes = new List<FieldNode>();

    public void SetComponents(GameManager _gameMgr, Vector2 _nodePos)
    {
        gameMgr = _gameMgr;
        nodePos = _nodePos;

        transform.name = $"X{nodePos.x}/Y{nodePos.y}";
        mesh = GetComponentInChildren<MeshRenderer>();
        var material = new Material(Resources.Load<Material>("Materials/Node"));
        mesh.material = material;

        canvas = transform.GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        fog = transform.Find("Fog").gameObject;
        posText = transform.Find("Canvas/PositionText").GetComponent<TextMeshProUGUI>();
        posText.text = $"X{nodePos.x} / Y{nodePos.y}";

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
                if (i != 0 && j != 0)
                {
                    offAxisNodes.Add(node);
                    allAxisNodes.Add(node);
                }
                else
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
            if (onAxisNode == null) continue;

            for (int j = 0; j < onAxisNode.offAxisNodes.Count; j++)
            {
                var offAxisNode = onAxisNode.offAxisNodes[j];
                if (offAxisNode == null) continue;

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

    public void SetVisibleNode(bool value)
    {
        fog.SetActive(!value);
        canSee = value;
    }

    public void CheckCoverNode(bool value)
    {
        if (value)
        {
            for (int i = 0; i < onAxisNodes.Count; i++)
            {
                var onAxisNode = onAxisNodes[i];
                var isCover = onAxisNode != null && onAxisNode.cover != null;
                if (!isCover) continue;

                switch ((TargetDirection)i)
                {
                    case TargetDirection.Left:
                        onAxisNode.cover.ShowCoverImage(TargetDirection.Right);
                        break;
                    case TargetDirection.Front:
                        onAxisNode.cover.ShowCoverImage(TargetDirection.Back);
                        break;
                    case TargetDirection.Back:
                        onAxisNode.cover.ShowCoverImage(TargetDirection.Front);
                        break;
                    case TargetDirection.Right:
                        onAxisNode.cover.ShowCoverImage(TargetDirection.Left);
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            for (int i = 0; i < onAxisNodes.Count; i++)
            {
                var onAxisNode = onAxisNodes[i];
                var isCover = onAxisNode != null && onAxisNode.cover != null;
                if (!isCover) continue;

                onAxisNode.cover.ShowCoverImage();
            }
        }
    }

    public Color NodeColor
    {
        set { mesh.material.color = value; }
        get { return mesh.material.color; }
    }
}
