using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum CoverType
{
    None,
    Half,
    Full,
}

public class Cover : MonoBehaviour
{
    [Header("---Access Script---")]
    [HideInInspector] public FieldNode node;

    [Header("---Access Component---")]
    [HideInInspector] public GameObject coverObject;
    private Canvas canvas;
    private List<Image> coverImages;

    [Header("--- Assignment Variable---")]
    public CoverType type;

    private readonly Vector3 halfCover_Pos = new Vector3(0f, 0.4f, 0f);
    private readonly Vector3 halfCover_Scale = new Vector3(1.30f, 0.8f, 1.30f);
    private readonly Vector3 fullCover_Pos = new Vector3(0f, 1f, 0f);
    private readonly Vector3 fullCover_Scale = new Vector3(1.30f, 2f, 1.30f);

    public void SetComponents(FieldNode _node, CoverType _type)
    {
        node = _node;
        node.cover = this;
        node.canMove = false;
        node.ReleaseAdjacentNodes();

        coverObject = transform.Find("CoverObject").gameObject;
        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        coverImages = canvas.transform.GetComponentsInChildren<Image>().ToList();

        type = _type;
        switch (type)
        {
            case CoverType.Half:
                coverObject.transform.localPosition = halfCover_Pos;
                coverObject.transform.localScale = halfCover_Scale;
                for (int i = 0; i < coverImages.Count; i++)
                {
                    var coverImage = coverImages[i];
                    coverImage.sprite = Resources.Load<Sprite>("Sprites/Icon_HalfCover");
                    coverImage.enabled = false;
                }
                break;
            case CoverType.Full:
                coverObject.transform.localPosition = fullCover_Pos;
                coverObject.transform.localScale = fullCover_Scale;
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
}
