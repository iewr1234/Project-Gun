using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum MapItemType
{
    None,
    Floor,
    Object,
}

public class MapItem : MonoBehaviour
{
    [Header("---Access Script---")]
    private MapEditor mapEdt;

    [Header("---Access Component---")]
    [HideInInspector] public Image outline;
    [HideInInspector] public Image image;

    [Header("--- Assignment Variable---")]
    public MapItemType type;

    [Header("[Object]")]
    public CoverType coverType;

    private void Start()
    {
        mapEdt = FindAnyObjectByType<MapEditor>();

        outline = transform.Find("Outline").GetComponent<Image>();
        outline.enabled = false;
        switch (type)
        {
            case MapItemType.Floor:
                image = transform.Find("Image").GetComponent<Image>();
                break;
            case MapItemType.Object:
                break;
            default:
                break;
        }
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
