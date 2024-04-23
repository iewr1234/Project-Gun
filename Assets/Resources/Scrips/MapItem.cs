using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum MapItemType
{
    None,
    Floor,
    HalfCover,
    FullCover,
    FloorObject,
    Object,
}

public class MapItem : MonoBehaviour
{
    [Header("---Access Script---")]
    private MapEditor mapEdt;

    [Header("---Access Component---")]
    [HideInInspector] public Image outline;
    [HideInInspector] public Image maskImage;

    [Header("--- Assignment Variable---")]
    public MapEditorType type;

    [Header("[Object]")]
    public Vector2 size = new Vector2(1f, 1f);

    private void Start()
    {
        mapEdt = FindAnyObjectByType<MapEditor>();

        outline = transform.Find("Outline").GetComponent<Image>();
        outline.enabled = false;
        maskImage = transform.Find("Mask").GetComponent<Image>();
    }

    public void PointerEnter_MapItem()
    {
        if (mapEdt.selectItem == this) return;

        outline.enabled = true;
    }

    public void PointerExit_MapItem()
    {
        if (mapEdt.selectItem == this) return;

        outline.enabled = false;
    }

    public void Button_MapItem()
    {
        mapEdt.SelectMapItem(this);
    }
}
