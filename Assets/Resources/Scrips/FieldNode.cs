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
    [HideInInspector] public BaseStorage baseStorage;

    [Header("---Access Component---")]
    private MeshRenderer mesh;
    private Canvas canvas;
    private GameObject frame;
    [HideInInspector] public List<NodeOutline> outlines;
    [HideInInspector] public MeshRenderer unableMove;
    private GameObject itemCase;

    private GameObject marker;
    private Image markerImage;
    private Image markerOutline;

    private TextMeshProUGUI posText;

    [Header("--- Assignment Variable---")]
    public CharacterController charCtr;
    public Cover cover;
    [Space(5f)]

    public bool canMove;
    public bool canShoot;
    public int moveCost;
    public float costMOB;
    [Space(5f)]

    public MarkerType markerType;
    public EnemyMarker enemyType = EnemyMarker.None;
    public BaseCampMarker baseType = BaseCampMarker.None;
    [HideInInspector] public bool hitNode;
    [HideInInspector] public CharacterController watcher;
    [Space(5f)]

    [HideInInspector] public Vector2Int nodePos;
    public List<FieldNode> onAxisNodes;
    public List<FieldNode> offAxisNodes;
    public List<FieldNode> allAxisNodes = new List<FieldNode>();

    [Space(5f)]
    public List<SetObject> setObjects = new List<SetObject>();

    [Header("[A* Algorithm]")]
    [HideInInspector] public FieldNode parentNode;
    [HideInInspector] public float G;
    [HideInInspector] public float H;
    [HideInInspector] public float F => G + H;

    [HideInInspector] public int aiScore;

    public void SetComponents(GameManager _gameMgr, Vector2Int _nodePos)
    {
        gameMgr = _gameMgr;
        nodePos = _nodePos;

        transform.name = $"X{nodePos.x}/Y{nodePos.y}";
        mesh = transform.Find("Mesh").GetComponent<MeshRenderer>();
        mesh.enabled = false;

        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        frame = transform.Find("Frame").gameObject;
        outlines = new List<NodeOutline>(new NodeOutline[4].ToList());
        unableMove = transform.Find("UnableMove").GetComponent<MeshRenderer>();
        itemCase = transform.Find("ItemCase").gameObject;
        //SetItemCase(false);

        marker = transform.Find("Canvas/Marker").gameObject;
        markerImage = marker.transform.Find("Image").GetComponent<Image>();
        markerOutline = marker.transform.Find("Outline").GetComponent<Image>();
        marker.SetActive(false);

        posText = transform.Find("Canvas/PositionText").GetComponent<TextMeshProUGUI>();
        posText.text = $"X{nodePos.x} / Y{nodePos.y}";
    }

    //public void SetComponents(BaseManager _baseMgr, Vector2Int _nodePos)
    //{
    //    baseMgr = _baseMgr;
    //    nodePos = _nodePos;

    //    transform.name = $"X{nodePos.x}/Y{nodePos.y}";
    //    mesh = transform.Find("Mesh").GetComponent<MeshRenderer>();
    //    mesh.enabled = false;

    //    canvas = GetComponentInChildren<Canvas>();
    //    canvas.worldCamera = Camera.main;
    //    frame = transform.Find("Frame").gameObject;
    //    outlines = new List<NodeOutline>(new NodeOutline[4].ToList());
    //    unableMove = transform.Find("UnableMove").GetComponent<MeshRenderer>();

    //    marker = transform.Find("Canvas/Marker").gameObject;
    //    markerOutline = marker.transform.Find("Outline").GetComponent<Image>();
    //    markerImage = marker.transform.Find("Image").GetComponent<Image>();
    //    marker.SetActive(false);

    //    posText = transform.Find("Canvas/PositionText").GetComponent<TextMeshProUGUI>();
    //    posText.text = $"X{nodePos.x} / Y{nodePos.y}";
    //}

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

                var node = gameMgr.nodeList.Find(x => x.nodePos == new Vector2(nodePos.x + i, nodePos.y + j));
                if (i != 0 && j != 0)
                {
                    if (node != null)
                    {
                        offAxisNodes.Add(node);
                        allAxisNodes.Add(node);
                    }
                }
                else
                {
                    onAxisNodes.Add(node);
                    if (node != null)
                    {
                        allAxisNodes.Add(node);
                    }
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

    public void ReleaseAdjacentNodes(FieldNode nextNode)
    {
        CheckAndRemoveNode(this, nextNode);
        CheckAndRemoveNode(nextNode, this);

        void CheckAndRemoveNode(FieldNode node, FieldNode nextNode)
        {
            for (int i = 0; i < node.onAxisNodes.Count; i++)
            {
                var onAxisNode = node.onAxisNodes[i];
                if (onAxisNode == null) continue;

                var find = nextNode.offAxisNodes.Find(x => x == onAxisNode);
                if (find != null)
                {
                    find.offAxisNodes.Remove(nextNode);
                    find.allAxisNodes.Remove(nextNode);
                    nextNode.offAxisNodes.Remove(find);
                    nextNode.allAxisNodes.Remove(find);
                }
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
                    if (onAxisNode == null) continue;

                    if (onAxisNode.cover != null)
                    {
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
                    else if (outlines[i].lineCover != null)
                    {
                        outlines[i].lineCover.SetActiveCoverImage(onAxisNode);
                    }
                }
                break;
            case false:
                for (int i = 0; i < onAxisNodes.Count; i++)
                {
                    var onAxisNode = onAxisNodes[i];
                    if (onAxisNode == null) continue;

                    if (onAxisNode.cover != null)
                    {
                        onAxisNode.cover.SetActiveCoverImage(TargetDirection.None);
                    }
                    else if (outlines[i].lineCover != null)
                    {
                        outlines[i].lineCover.SetActiveCoverImage(TargetDirection.None);
                    }
                }
                break;
        }
    }

    public void AddNodeOutline(Transform nodeOutlineTf)
    {
        NodeOutline outlinePrefab = Resources.Load<NodeOutline>("Prefabs/NodeOutline");
        for (int i = 0; i < onAxisNodes.Count; i++)
        {
            var onAxisNode = onAxisNodes[i];
            if (onAxisNode == null)
            {
                CreateNodeOutline(i);
            }
            else
            {
                var dir = (TargetDirection)i;
                var symmetricDir = TargetDirection.None;
                switch (dir)
                {
                    case TargetDirection.Left:
                        symmetricDir = TargetDirection.Right;
                        break;
                    case TargetDirection.Front:
                        symmetricDir = TargetDirection.Back;
                        break;
                    case TargetDirection.Back:
                        symmetricDir = TargetDirection.Front;
                        break;
                    case TargetDirection.Right:
                        symmetricDir = TargetDirection.Left;
                        break;
                    default:
                        Debug.LogError("SetNodeOutline: inappropriate direction");
                        break;
                }

                if (onAxisNode.outlines[(int)symmetricDir] == null)
                {
                    onAxisNode.outlines[(int)symmetricDir] = CreateNodeOutline(i);
                }
            }
        }

        NodeOutline CreateNodeOutline(int index)
        {
            var outline = Instantiate(outlinePrefab);
            outline.transform.SetParent(nodeOutlineTf, false);
            outline.SetComponents(index, this);
            return outline;
        }
    }

    public void SetNodeOutline(bool value)
    {
        for (int i = 0; i < outlines.Count; i++)
        {
            var outline = outlines[i];
            outline.SetActiveLine(value);
        }
    }

    public void SetNodeOutline(TargetDirection targetDir)
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

    public void SetNodeOutline(List<FieldNode> movableNodes)
    {
        for (int i = 0; i < onAxisNodes.Count; i++)
        {
            var onAxisNode = onAxisNodes[i];
            if (onAxisNode == movableNodes[0]) continue;

            if (onAxisNode == null)
            {
                if (canShoot || charCtr != null)
                {
                    outlines[i].SetActiveLine(true);
                }
                else
                {
                    outlines[i].SetActiveLine(true, DataUtility.maxMoveLineColor);
                }
            }
            else if (movableNodes.Contains(onAxisNode) && canShoot && !onAxisNode.canShoot)
            {
                outlines[i].SetActiveLine(true);
            }
            else if (!movableNodes.Contains(onAxisNode))
            {
                if (canShoot)
                {
                    outlines[i].SetActiveLine(true);
                }
                else
                {
                    outlines[i].SetActiveLine(true, DataUtility.maxMoveLineColor);
                }
            }
        }
    }

    public void SetNodeOutline(List<FieldNode> movableNodes, TargetDirection targetDir)
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
            else if (!movableNodes.Contains(onAxisNode))
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

    public void SetOnArea(FindNodeType findType, bool allLoad)
    {
        if (unableMove.enabled || cover != null) return;

        if (findType == FindNodeType.SetUnableMove)
        {
            unableMove.enabled = allLoad;
            canMove = false;
            ReleaseAdjacentNodes();
        }
        else
        {
            var coverType = findType == FindNodeType.SetFullCover ? CoverType.Full : CoverType.Half;
            var nodeCover = Instantiate(Resources.Load<Cover>($"Prefabs/Cover/NodeCover"));
            nodeCover.transform.SetParent(transform, false);
            nodeCover.SetComponents(this, coverType, allLoad);
        }
    }

    public void SetOnArea(TargetDirection setDirection, FindNodeType findType, bool allLoad)
    {
        var outline = outlines[(int)setDirection];
        if (unableMove.enabled || outline.lineCover != null) return;

        if (findType == FindNodeType.SetUnableMove)
        {
            outline.unableMove.enabled = allLoad;
            var nextNode = onAxisNodes[(int)setDirection];
            ReleaseAdjacentNodes(nextNode);
            allAxisNodes.Remove(nextNode);
            nextNode.allAxisNodes.Remove(this);
        }
        else
        {
            var coverType = findType == FindNodeType.SetFullCover ? CoverType.Full : CoverType.Half;
            if (onAxisNodes[(int)setDirection] != null && outline.lineCover == null)
            {
                var lineCover = Instantiate(Resources.Load<Cover>($"Prefabs/Cover/LineCover"));
                lineCover.transform.SetParent(outline.transform, false);
                lineCover.SetComponents(outline, this, setDirection, coverType, allLoad);
                switch (setDirection)
                {
                    case TargetDirection.Front:
                        lineCover.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
                        break;
                    case TargetDirection.Back:
                        lineCover.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                        break;
                    case TargetDirection.Right:
                        lineCover.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void SetOffArea()
    {
        if (cover == null && !unableMove.enabled) return;

        if (cover != null)
        {
            Destroy(cover.gameObject);
        }
        else
        {
            unableMove.enabled = false;
        }

        for (int i = 0; i < onAxisNodes.Count; i++)
        {
            var onAxisNode = onAxisNodes[i];
            if (onAxisNode == null) continue;

            onAxisNode.AddAdjacentNodes();
        }
        canMove = true;
    }

    public void SetItemCase(bool value)
    {
        itemCase.SetActive(value);
    }

    public void SetOnMarker(bool isActive)
    {
        markerType = MarkerType.Player;
        markerImage.sprite = Resources.Load<Sprite>("Sprites/player_marker");
        markerImage.color = DataUtility.color_PlayerMarker;
        markerOutline.color = DataUtility.color_PlayerMarker;
        marker.SetActive(isActive);
    }

    public void SetOnMarker(bool isActive, EnemyMarker _enemyType)
    {
        markerType = MarkerType.Enemy;
        enemyType = _enemyType;
        switch (enemyType)
        {
            case EnemyMarker.ShortRange:
                markerImage.sprite = Resources.Load<Sprite>("Sprites/enemy_S");
                break;
            case EnemyMarker.MiddleRange:
                markerImage.sprite = Resources.Load<Sprite>("Sprites/enemy_M");
                break;
            case EnemyMarker.LongRange:
                markerImage.sprite = Resources.Load<Sprite>("Sprites/enemy_L");
                break;
            case EnemyMarker.Elite:
                markerImage.sprite = Resources.Load<Sprite>("Sprites/enemy_Elite");
                break;
            case EnemyMarker.Boss:
                markerImage.sprite = Resources.Load<Sprite>("Sprites/enemy_Boss");
                break;
            default:
                break;
        }
        markerImage.color = DataUtility.color_EnemyMarker;
        markerOutline.color = DataUtility.color_EnemyMarker;
        marker.SetActive(isActive);
    }

    public void SetOnMarker(bool isActive, BaseCampMarker _baseType)
    {
        markerType = MarkerType.Base;
        baseType = _baseType;
        switch (baseType)
        {
            case BaseCampMarker.Mission_Node:
                markerImage.sprite = Resources.Load<Sprite>("Sprites/Icon_Event");
                canMove = false;
                ReleaseAdjacentNodes();
                break;
            case BaseCampMarker.Mission_Enter:
                markerImage.sprite = Resources.Load<Sprite>("Sprites/Icon_Cross");
                break;
            case BaseCampMarker.Storage_Node:
                markerImage.sprite = Resources.Load<Sprite>("Sprites/Icon_Event");
                canMove = false;
                ReleaseAdjacentNodes();
                break;
            case BaseCampMarker.Storage_Enter:
                markerImage.sprite = Resources.Load<Sprite>("Sprites/Icon_Cross");
                break;
            default:
                break;
        }
        markerImage.color = DataUtility.color_baseMarker;
        markerOutline.color = DataUtility.color_baseMarker;
        marker.SetActive(isActive);
    }

    public void SetOffMarker()
    {
        enemyType = EnemyMarker.None;
        if (baseType == BaseCampMarker.Mission_Node)
        {
            canMove = true;
            for (int i = 0; i < onAxisNodes.Count; i++)
            {
                var onAxisNode = onAxisNodes[i];
                if (onAxisNode == null) continue;

                onAxisNode.AddAdjacentNodes();
            }
        }
        baseType = BaseCampMarker.None;
        marker.SetActive(false);
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
            subNode.setObjects.Add(setObject);
        }
    }

    private SetObject SwitchOfMapItemType(MapItem item, TargetDirection setDirection)
    {
        switch (item.type)
        {
            case MapEditorType.FloorObject:
                return SetObject();
            case MapEditorType.Hurdle:
                return SetObject();
            case MapEditorType.HalfCover:
                return SetObject();
            case MapEditorType.FullCover:
                return SetObject();
            case MapEditorType.SideObject:
                return SetObject();
            case MapEditorType.BaseObject:
                return SetObject();
            default:
                return null;
        }

        SetObject SetObject()
        {
            var _object = Instantiate(Resources.Load<GameObject>($"Prefabs/Object/{item.type}/{item.name}"));
            _object.name = item.name;
            _object.transform.SetParent(transform, false);
            _object.transform.localRotation = DataUtility.GetSetRotationOfObject(setDirection);
            if (item.type == MapEditorType.BaseObject)
            {
                var _baseStorage = _object.GetComponent<BaseStorage>();
                if (_baseStorage != null)
                {
                    baseStorage = _baseStorage;
                }
            }

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
            if (type == MapEditorType.BaseObject)
            {
                baseStorage = null;
            }

            var setNode = setObject.setNode;
            //setNode.RemoveSetObject(type);
            setNode.setObjects.Remove(setObject);
            for (int i = 0; i < setObject.subNodes.Count; i++)
            {
                var subNode = setObject.subNodes[i];
                var _setObject = subNode.setObjects.Find(x => x.type == type);
                //subNode.RemoveSetObject(type);
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

    public TextMeshProUGUI PosText
    {
        private set { posText = value; }
        get { return posText; }
    }
}
