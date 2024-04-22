using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FieldNode : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;

    [Header("---Access Component---")]
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private Canvas canvas;
    [HideInInspector] public GameObject frame;
    private List<NodeOutline> outlines = new List<NodeOutline>();
    //private MeshRenderer fog;

    private GameObject marker;
    private Image markerOutline;
    private TextMeshProUGUI markerText;

    private TextMeshProUGUI posText;

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
        mesh = transform.Find("Mesh").GetComponent<MeshRenderer>();
        mesh.enabled = false;
        //var material = new Material(Resources.Load<Material>("Materials/Node"));
        //mesh.material = material;

        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        frame = transform.Find("Frame").gameObject;
        outlines = GetComponentsInChildren<NodeOutline>().ToList();
        for (int i = 0; i < outlines.Count; i++)
        {
            var outline = outlines[i];
            outline.SetComponents();
        }
        //fog = transform.Find("Fog").GetComponent<MeshRenderer>();

        marker = transform.Find("Canvas/Marker").gameObject;
        markerOutline = marker.transform.Find("Outline").GetComponent<Image>();
        markerText = marker.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        marker.SetActive(false);

        posText = transform.Find("Canvas/PositionText").GetComponent<TextMeshProUGUI>();
        posText.text = $"X{nodePos.x} / Y{nodePos.y}";

        //canMove = true;
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

    public void SetMovableNode(List<FieldNode> openNodes)
    {
        for (int i = 0; i < onAxisNodes.Count; i++)
        {
            var onAxisNode = onAxisNodes[i];
            if (onAxisNode == null)
            {
                outlines[i].SetActiveLine(true);
            }
            else if (openNodes.Find(x => x == onAxisNode) == null)
            {
                outlines[i].SetActiveLine(true);
            }
        }
    }

    public void CheckCoverNode(bool value)
    {
        switch (value)
        {
            case true:
                for (int i = 0; i < onAxisNodes.Count; i++)
                {
                    var onAxisNode = onAxisNodes[i];
                    var isCover = onAxisNode != null && onAxisNode.cover != null;
                    if (!isCover) continue;

                    switch ((TargetDirection)i)
                    {
                        case TargetDirection.Left:
                            onAxisNode.cover.SetActiveCoverImage(TargetDirection.Right);
                            break;
                        case TargetDirection.Front:
                            onAxisNode.cover.SetActiveCoverImage(TargetDirection.Back);
                            break;
                        case TargetDirection.Back:
                            onAxisNode.cover.SetActiveCoverImage(TargetDirection.Front);
                            break;
                        case TargetDirection.Right:
                            onAxisNode.cover.SetActiveCoverImage(TargetDirection.Left);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case false:
                for (int i = 0; i < onAxisNodes.Count; i++)
                {
                    var onAxisNode = onAxisNodes[i];
                    var isCover = onAxisNode != null && onAxisNode.cover != null;
                    if (!isCover) continue;

                    onAxisNode.cover.SetActiveCoverImage(TargetDirection.None);
                }
                break;
        }
    }

    public void SetNodeOutLine(bool value)
    {
        for (int i = 0; i < outlines.Count; i++)
        {
            var outline = outlines[i];
            outline.SetActiveLine(value);
        }
    }

    public void SetMarker(CharacterOwner type, int index)
    {
        marker.SetActive(true);
        markerOutline.color = type == CharacterOwner.Player ? DataUtility.color_Player : DataUtility.color_Enemy;
        markerText.color = type == CharacterOwner.Player ? DataUtility.color_Player : DataUtility.color_Enemy;
        markerText.text = type == CharacterOwner.Player ? $"P{index}" : $"E{index}";
        canMove = false;
    }

    public void SetMarker()
    {
        marker.SetActive(false);
        canMove = true;
    }

    public void SetOnNodeMesh(MapItem item, bool random)
    {
        mesh.enabled = true;
        if (random)
        {
            var dirs = new float[] { 0f, 90f, 180f, 270f };
            var index = Random.Range(0, dirs.Length);
            var dir = dirs[index];
            mesh.transform.localRotation = Quaternion.Euler(0f, dir, 0f);
        }
        else
        {
            mesh.transform.localRotation = Quaternion.identity;
        }
        mesh.material = item.image.material;
        canMove = true;
    }

    public void SetOffNodeMesh()
    {
        if (!mesh.enabled) return;

        mesh.enabled = false;
        canMove = false;
    }

    public void SetOnObject(MapItem item)
    {
        switch (item.coverType)
        {
            case CoverType.None:
                break;
            case CoverType.Half:
                if (cover != null)
                {
                    Destroy(cover.gameObject);
                }
                var halfObject = Instantiate(Resources.Load<Cover>($"Prefabs/Object/{item.name}"));
                halfObject.transform.SetParent(transform, false);
                halfObject.SetComponents(this, CoverType.Half);
                break;
            case CoverType.Full:
                if (cover != null)
                {
                    Destroy(cover.gameObject);
                }
                break;
            default:
                break;
        }
    }

    public void SetOffObject()
    {
        if (cover == null) return;

        switch (cover.type)
        {
            case CoverType.Half:
                canMove = true;
                break;
            case CoverType.Full:
                canMove = true;
                break;
            default:
                break;
        }
        Destroy(cover.gameObject);
    }

    public MeshRenderer Mesh
    {
        private set { mesh = value; }
        get { return mesh; }
    }
}
