using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum ItemType
{
    None,
    MainWeapon,
    SubWeapon,
    Magazine,
    Rig,
    Backpack,
}

public class ItemHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("---Access Script---")]
    public InventoryManager invenMgr;
    public ItemSlot itemSlot;

    [Header("---Access Component---")]
    public RectTransform rect;
    private Image targetImage;

    [Header("--- Assignment Variable---")]
    public ItemType type;
    public Vector2Int size = new Vector2Int(1, 1);
    [HideInInspector] public Vector2 pivot;

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;
        rect = GetComponent<RectTransform>();
        targetImage = transform.Find("BackGround").GetComponent<Image>();

        ResizeItemRect();
    }

    private void ResizeItemRect()
    {
        rect.sizeDelta = new Vector2Int(DataUtility.itemSize * size.x, DataUtility.itemSize * size.y);
    }

    public void SetItemInfo(ItemType _type, Vector2Int _size)
    {
        type = _type;
        size = _size;
        switch (type)
        {
            case ItemType.None:
                break;
            case ItemType.MainWeapon:
                break;
            case ItemType.SubWeapon:
                break;
            case ItemType.Magazine:
                break;
            case ItemType.Rig:
                break;
            case ItemType.Backpack:
                break;
            default:
                break;
        }

        if (size == new Vector2Int(1, 1))
        {
            pivot = new Vector2(-DataUtility.itemSize / 2, DataUtility.itemSize / 2);
        }
        else
        {

        }
        gameObject.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        targetImage.raycastTarget = false;
        invenMgr.TakeTheItem(this);
        FollowMouse();
    }

    public void OnDrag(PointerEventData eventData)
    {
        FollowMouse();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        invenMgr.PutTheItem(this, invenMgr.onSlot);
        targetImage.raycastTarget = true;
    }

    private void FollowMouse()
    {
        var pos = Input.mousePosition;
        pos.x += pivot.x;
        pos.y += pivot.y;
        transform.position = pos;
    }
}
