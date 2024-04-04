using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Cover : MonoBehaviour
{
    [Header("---Access Script---")]
    [HideInInspector] public FieldNode node;

    [Header("---Access Component---")]
    private Canvas canvas;
    private List<Image> coverImages;

    public void SetComponents(FieldNode _node)
    {
        node = _node;
        canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        coverImages = canvas.transform.GetComponentsInChildren<Image>().ToList();
        ShowCoverImage();

        node.cover = this;
        node.canMove = false;
        node.ReleaseAdjacentNodes();
    }

    public void ShowCoverImage(TargetDirection dir)
    {
        coverImages[(int)dir].enabled = true;
    }

    public void ShowCoverImage()
    {
        for (int i = 0; i < coverImages.Count; i++)
        {
            var coverImage = coverImages[i];
            coverImage.enabled = false;
        }
    }
}
