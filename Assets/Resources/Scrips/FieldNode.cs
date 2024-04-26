using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SetObject
{
    public MapEditorType type;
    public Vector2 size;
    public FieldNode setNode;
    public List<FieldNode> subNodes = new List<FieldNode>();
    public GameObject setObject;
    public TargetDirection setDir;
}

public class FieldNode : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;

    [Header("---Access Component---")]
    private MeshRenderer mesh;
    private Canvas canvas;
    private GameObject frame;
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
    [Space(5f)]

    public List<SetObject> setObjects = new List<SetObject>();

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
        onAxisNodes.Clear();
        offAxisNodes.Clear();
        allAxisNodes.Clear();

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

    public void SetNodeOutLine(TargetDirection targetDir)
    {
        for (int i = 0; i < outlines.Count; i++)
        {
            var outline = outlines[i];
            if (i == (int)targetDir)
            {
                outline.SetActiveLine(true, Color.red);
            }
            else
            {
                outline.SetActiveLine(true);
            }
        }
    }

    public void SetNodeOutLine(List<FieldNode> openNodes)
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

    public void SetNodeOutLine(List<FieldNode> openNodes, TargetDirection targetDir)
    {
        for (int i = 0; i < onAxisNodes.Count; i++)
        {
            var onAxisNode = onAxisNodes[i];
            if (onAxisNode == null)
            {
                if (i == (int)targetDir)
                {
                    outlines[i].SetActiveLine(true, Color.red);
                }
                else
                {
                    outlines[i].SetActiveLine(true);
                }
            }
            else if (openNodes.Find(x => x == onAxisNode) == null)
            {
                if (i == (int)targetDir)
                {
                    outlines[i].SetActiveLine(true, Color.red);
                }
                else
                {
                    outlines[i].SetActiveLine(true);
                }
            }
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

    public void SetOnFloor(MapItem item, bool random)
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
        mesh.material = item.maskImage.material;

        if (cover == null)
        {
            canMove = true;
        }
    }

    public void SetOnFloor(MapItem item, Quaternion rot)
    {
        mesh.enabled = true;
        mesh.transform.localRotation = rot;
        mesh.material = item.maskImage.material;

        if (cover == null)
        {
            canMove = true;
        }
    }

    public void SetOffFloor()
    {
        if (!mesh.enabled) return;

        mesh.enabled = false;
        marker.SetActive(false);
        if (cover != null)
        {
            Destroy(cover.gameObject);
        }
        canMove = false;
    }

    public void SetOnObject(MapItem item, TargetDirection setDirection)
    {
        var find = setObjects.Find(x => x.type == item.type);
        if (find != null) return;

        SwitchOfMapItemType(item, setDirection);
    }

    public void SetOnObject(List<FieldNode> setNodes, MapItem item, TargetDirection setDirection)
    {
        var findAll = setNodes.Find(x => x.setObjects.Find(x => x.type == item.type) != null);
        if (findAll != null) return;

        var subNodes = new List<FieldNode>(setNodes);
        subNodes.Remove(this);
        var setObject = SwitchOfMapItemType(item, setDirection);
        setObject.subNodes = subNodes;
        for (int i = 0; i < subNodes.Count; i++)
        {
            var subNode = subNodes[i];
            switch (item.type)
            {
                case MapEditorType.HalfCover:
                    var subCover_Half = Instantiate(Resources.Load<Cover>($"Prefabs/Cover"));
                    subCover_Half.transform.SetParent(subNode.transform, false);
                    subCover_Half.SetComponents(subNode, CoverType.Half);
                    break;
                case MapEditorType.FullCover:
                    var subCover_Full = Instantiate(Resources.Load<Cover>($"Prefabs/Cover"));
                    subCover_Full.transform.SetParent(subNode.transform, false);
                    subCover_Full.SetComponents(subNode, CoverType.Full);
                    break;
                default:
                    break;
            }
            subNode.setObjects.Add(setObject);
        }
    }

    private SetObject SwitchOfMapItemType(MapItem item, TargetDirection setDirection)
    {
        switch (item.type)
        {
            case MapEditorType.FloorObject:
                return SetObject();
            case MapEditorType.HalfCover:
                return SetCover(CoverType.Half);
            case MapEditorType.FullCover:
                return SetCover(CoverType.Full);
            case MapEditorType.SideObject:
                return SetObject();
            default:
                return null;
        }

        SetObject SetCover(CoverType coverType)
        {
            var _cover = Instantiate(Resources.Load<Cover>($"Prefabs/Cover"));
            var _object = Instantiate(Resources.Load<GameObject>($"Prefabs/Object/{coverType}Cover/{item.name}"));
            _object.name = item.name;
            _cover.transform.SetParent(transform, false);
            _object.transform.SetParent(transform, false);
            _cover.SetComponents(this, coverType, _object, setDirection);

            var setObject = new SetObject()
            {
                type = item.type,
                size = item.size,
                setNode = this,
                setObject = _object,
                setDir = setDirection,
            };
            setObjects.Add(setObject);
            return setObject;
        }

        SetObject SetObject()
        {
            var _object = Instantiate(Resources.Load<GameObject>($"Prefabs/Object/{item.type}/{item.name}"));
            _object.name = item.name;
            _object.transform.SetParent(transform, false);
            _object.transform.localRotation = DataUtility.GetSetRotation(setDirection);

            var setObject = new SetObject()
            {
                type = item.type,
                size = item.size,
                setNode = this,
                setObject = _object,
                setDir = setDirection,
            };
            setObjects.Add(setObject);
            return setObject;
        }
    }

    public void SetOffObject(MapEditorType type)
    {
        if (setObjects.Count == 0) return;

        var setObject = setObjects.Find(x => x.type == type);
        if (setObject != null)
        {
            var setNode = setObject.setNode;
            setNode.RemoveSetObject(type);
            setNode.setObjects.Remove(setObject);
            for (int i = 0; i < setObject.subNodes.Count; i++)
            {
                var subNode = setObject.subNodes[i];
                var _setObject = subNode.setObjects.Find(x => x.type == type);
                subNode.RemoveSetObject(type);
                subNode.setObjects.Remove(_setObject);
            }
            Destroy(setObject.setObject);
        }
    }

    private void RemoveSetObject(MapEditorType type)
    {
        switch (type)
        {
            case MapEditorType.HalfCover:
                canMove = true;
                AddAdjacentNodes();
                Destroy(cover.gameObject);
                break;
            case MapEditorType.FullCover:
                canMove = true;
                AddAdjacentNodes();
                Destroy(cover.gameObject);
                break;
            default:
                break;
        }
    }

    public void SetActiveNodeFrame(bool value)
    {
        frame.SetActive(value);
        posText.enabled = value;
    }

    public MeshRenderer Mesh
    {
        private set { mesh = value; }
        get { return mesh; }
    }

    public GameObject Marker
    {
        private set { marker = value; }
        get { return marker; }
    }

    public Image MarkerOutline
    {
        private set { markerOutline = value; }
        get { return markerOutline; }
    }

    public TextMeshProUGUI MarkerText
    {
        private set { markerText = value; }
        get { return markerText; }
    }
}
