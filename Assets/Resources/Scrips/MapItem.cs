using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MapItemType
{
    None,
    Floor,
    FloorObject,
    Hurdle,
    Box,
    HalfCover,
    FullCover,
    SideObject,
}

public class MapItem : MonoBehaviour
{
    [Header("---Access Script---")]
    private MapEditor mapEdt;

    [Header("---Access Component---")]
    [HideInInspector] public Image outline;
    [HideInInspector] public Image maskImage;
    private TextMeshProUGUI sizeText;

    [Header("--- Assignment Variable---")]
    public MapEditorType type;

    [Header("[Object]")]
    public Vector2 size = new Vector2(1f, 1f);

    public void SetComponents(MapEditor _mapEdt)
    {
        mapEdt = _mapEdt;

        outline = transform.Find("Outline").GetComponent<Image>();
        outline.enabled = false;
        maskImage = transform.Find("Mask").GetComponent<Image>();
        sizeText = transform.Find("SizeText").GetComponent<TextMeshProUGUI>();

        var typeText = transform.name.Split('_')[0];
        switch (typeText)
        {
            case "Floor":
                type = MapEditorType.Floor;
                break;
            case "FloorObject":
                type = MapEditorType.FloorObject;
                break;
            case "Hurdle":
                type = MapEditorType.Hurdle;
                break;
            case "Box":
                type = MapEditorType.Box;
                break;
            case "Half":
                type = MapEditorType.HalfCover;
                break;
            case "Full":
                type = MapEditorType.FullCover;
                break;
            case "SideObject":
                type = MapEditorType.SideObject;
                break;
            default:
                break;
        }

        if (type == MapEditorType.Floor)
        {
            sizeText.enabled = false;
        }
        else
        {
            sizeText.text = $"{size.x} x {size.y}";
        }
    }

    public void PointerEnter_MapItem()
    {
        mapEdt.onSideButton = true;
        outline.enabled = true;
    }

    public void PointerExit_MapItem()
    {
        mapEdt.onSideButton = false;
        if (mapEdt.selectItem != this)
        {
            outline.enabled = false;
        }
    }

    public void Button_MapItem()
    {
        mapEdt.SelectMapItem(this);
    }
}
