using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [Header("---Access Script---")]
    public MyStorage myStorage;
    public OtherStorage otherStorage;

    [Header("---Access Component---")]
    [SerializeField] private Image slotImage;

    [Header("--- Assignment Variable---")]
    public ItemHandler item;
    [HideInInspector] public Vector2Int slotIndex;

    public void SetComponents(MyStorage _myStorage, Vector2Int _slotIndex)
    {
        myStorage = _myStorage;

        slotImage = GetComponent<Image>();

        slotIndex = _slotIndex;
        transform.name = $"Slot_X{slotIndex.x}/Y{slotIndex.y}";
    }

    public void SetComponents(OtherStorage _otherStorage, Vector2Int _slotIndex)
    {
        otherStorage = _otherStorage;

        slotImage = GetComponent<Image>();

        slotIndex = _slotIndex;
        transform.name = $"Slot_X{slotIndex.x}/Y{slotIndex.y}";
    }

    public void SetSlotColor(Color color)
    {
        slotImage.color = color;
    }

    public void PointerEnter_ItemSlot()
    {
        var invenMgr = myStorage != null ? myStorage.invenMgr : otherStorage.invenMgr;
        invenMgr.onSlot = this;
        if (invenMgr.holdingItem != null && invenMgr.holdingItem.itemSlot != this)
        {
            if (item != null)
            {
                if (invenMgr.holdingItem.size == new Vector2Int(1, 1))
                {
                    item.targetImage.color = DataUtility.slot_unMoveColor;
                }
                else
                {

                }
            }
            else
            {
                if (invenMgr.holdingItem.size == new Vector2Int(1, 1))
                {
                    SetSlotColor(DataUtility.slot_moveColor);
                }
                else
                {

                }
            }
        }
    }

    public void PointerExit_ItemSlot()
    {
        var invenMgr = myStorage != null ? myStorage.invenMgr : otherStorage.invenMgr;
        invenMgr.onSlot = null;
        if (invenMgr.holdingItem != null)
        {
            if (item == null)
            {
                SetSlotColor(Color.white);
            }
            else if (invenMgr.holdingItem != item && item.targetImage.color != DataUtility.slot_onItemColor)
            {
                if (item.size == new Vector2Int(1, 1))
                {
                    item.targetImage.color = DataUtility.slot_onItemColor;
                }
                else
                {

                }
            }
        }
    }

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    if (item == null) return;

    //    var invenMgr = myStorage != null ? myStorage.invenMgr : otherStorage.invenMgr;
    //    if (invenMgr.onSlot != this) return;

    //    var color = DataUtility.slot_onItemColor;
    //    color.a = 100 / 255f;
    //    item.targetImage.color = color;
    //    invenMgr.TakeTheItem(item);
    //    FollowMouse();
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    FollowMouse();
    //}

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    SetItem(invenMgr.onSlot);
    //}
}