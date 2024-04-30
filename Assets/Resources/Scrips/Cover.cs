using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum CoverForm
{
    None,
    Node,
    Line,
}

public enum CoverType
{
    None,
    Half,
    Full,
}

public class Cover : MonoBehaviour
{
    [Header("---Access Component---")]
    private MeshRenderer coverMesh;
    private Canvas canvas;
    private List<Image> coverImages;

    [Header("[NodeCover]")]
    public FieldNode coverNode;

    [Header("[LineCover]")]
    public NodeOutline outline;
    public FieldNode frontNode;
    public FieldNode backNode;

    [Header("--- Assignment Variable---")]
    public CoverForm formType;
    public CoverType coverType;

    private readonly Vector3 nodeHalfCover_Pos = new Vector3(0f, 0.4f, 0f);
    private readonly Vector3 nodeHalfCover_Scale = new Vector3(1.3f, 0.8f, 1.3f);
    private readonly Vector3 nodeFullCover_Pos = new Vector3(0f, 1f, 0f);
    private readonly Vector3 nodeFullCover_Scale = new Vector3(1.3f, 2f, 1.3f);

    private readonly Vector3 lineHalfCover_Pos = new Vector3(0f, 0.4f, 0f);
    private readonly Vector3 lineHalfCover_Scale = new Vector3(0.05f, 0.8f, 1.3f);
    private readonly Vector3 lineFullCover_Pos = new Vector3(0f, 1f, 0f);
    private readonly Vector3 lineFullCover_Scale = new Vector3(0.05f, 2f, 1.3f);

    public void SetComponents(FieldNode _node, CoverType _type)
    {
        coverNode = _node;
        coverNode.cover = this;
        coverNode.canMove = false;
        coverNode.ReleaseAdjacentNodes();

        coverMesh = transform.Find("CoverMesh").GetComponent<MeshRenderer>();
        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        coverImages = canvas.transform.GetComponentsInChildren<Image>().ToList();

        formType = CoverForm.Node;
        coverType = _type;
        coverMesh.material = Resources.Load<Material>("Materials/Cover");
        switch (coverType)
        {
            case CoverType.Half:
                coverMesh.transform.localPosition = nodeHalfCover_Pos;
                coverMesh.transform.localScale = nodeHalfCover_Scale;
                for (int i = 0; i < coverImages.Count; i++)
                {
                    var coverImage = coverImages[i];
                    coverImage.sprite = Resources.Load<Sprite>("Sprites/Icon_HalfCover");
                    coverImage.enabled = false;
                }
                break;
            case CoverType.Full:
                coverMesh.transform.localPosition = nodeFullCover_Pos;
                coverMesh.transform.localScale = nodeFullCover_Scale;
                for (int i = 0; i < coverImages.Count; i++)
                {
                    var coverImage = coverImages[i];
                    coverImage.sprite = Resources.Load<Sprite>("Sprites/Icon_FullCover");
                    coverImage.enabled = false;
                }
                break;
            default:
                break;
        }
    }

    public void SetComponents(NodeOutline _nodeOutline, FieldNode _node, TargetDirection setDirection, CoverType _type)
    {
        outline = _nodeOutline;
        outline.lineCover = this;
        frontNode = _node;
        backNode = _node.onAxisNodes[(int)setDirection];
        frontNode.ReleaseAdjacentNodes(backNode);

        coverMesh = transform.Find("CoverMesh").GetComponent<MeshRenderer>();
        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        coverImages = canvas.transform.GetComponentsInChildren<Image>().ToList();

        formType = CoverForm.Line;
        coverType = _type;
        coverMesh.material = Resources.Load<Material>("Materials/Cover");
        switch (coverType)
        {
            case CoverType.Half:
                coverMesh.transform.localPosition = lineHalfCover_Pos;
                coverMesh.transform.localScale = lineHalfCover_Scale;
                for (int i = 0; i < coverImages.Count; i++)
                {
                    var coverImage = coverImages[i];
                    coverImage.sprite = Resources.Load<Sprite>("Sprites/Icon_HalfCover");
                    coverImage.enabled = false;
                }
                break;
            case CoverType.Full:
                frontNode.allAxisNodes.Remove(backNode);
                backNode.allAxisNodes.Remove(frontNode);
                coverMesh.transform.localPosition = lineFullCover_Pos;
                coverMesh.transform.localScale = lineFullCover_Scale;
                for (int i = 0; i < coverImages.Count; i++)
                {
                    var coverImage = coverImages[i];
                    coverImage.sprite = Resources.Load<Sprite>("Sprites/Icon_FullCover");
                    coverImage.enabled = false;
                }
                break;
            default:
                break;
        }
    }

    public void SetActiveCoverImage(TargetDirection dir)
    {
        switch (dir)
        {
            case TargetDirection.None:
                for (int i = 0; i < coverImages.Count; i++)
                {
                    var coverImage = coverImages[i];
                    coverImage.enabled = false;
                }
                break;
            default:
                coverImages[(int)dir].enabled = true;
                break;
        }
    }

    public void SetActiveCoverImage(FieldNode node)
    {
        if (node == frontNode)
        {
            coverImages[1].enabled = true;
        }
        else
        {
            coverImages[0].enabled = true;
        }
    }
}
