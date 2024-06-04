using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, ICanvasRaycastFilter
{
    [Header("---Access Script---")]
    public MyStorage myStorage;
    public OtherStorage otherStorage;

    [Header("---Access Component---")]
    [SerializeField] private Image slotImage;

    [Header("--- Assignment Variable---")]
    public ItemHandler item;
    [HideInInspector] public Vector2Int slotIndex;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetComponents(MyStorage _myStorage, Vector2Int _slotIndex)
    {
        myStorage = _myStorage;

        slotImage = transform.Find("BackGround").GetComponent<Image>();

        slotIndex = _slotIndex;
        transform.name = $"Slot_X{slotIndex.x}/Y{slotIndex.y}";
    }

    public void SetComponents(OtherStorage _otherStorage, Vector2Int _slotIndex)
    {
        otherStorage = _otherStorage;

        slotImage = transform.Find("BackGround").GetComponent<Image>();

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
        if (invenMgr.holdingItem != null)
        {
            if (myStorage != null)
            {
                ProcessOfSlotSize(myStorage.itemSlots, invenMgr.holdingItem);
            }
            else
            {
                ProcessOfSlotSize(otherStorage.itemSlots, invenMgr.holdingItem);
            }
        }
        else if (item != null)
        {
            item.targetImage.raycastTarget = true;
        }

        void ProcessOfSlotSize(List<ItemSlot> itemSlots, ItemHandler item)
        {
            var startIndex = invenMgr.onSlot.slotIndex - item.pivotIndex;
            invenMgr.onSlots = invenMgr.FindAllMultiSizeSlots(itemSlots, item, startIndex);
            var sizeCount = item.size.x * item.size.y;
            var findSlot = invenMgr.onSlots.Find(x => x.item != null && x.item != item);
            for (int i = 0; i < invenMgr.onSlots.Count; i++)
            {
                var onSlot = invenMgr.onSlots[i];
                if (sizeCount > invenMgr.onSlots.Count)
                {
                    onSlot.SetSlotColor(DataUtility.slot_unMoveColor);
                }
                else if (findSlot && findSlot.item.itemData.ID == item.itemData.ID)
                {
                    if (findSlot.item.TotalCount < findSlot.item.itemData.maxNesting)
                    {
                        onSlot.SetSlotColor(DataUtility.slot_moveColor);
                    }
                    else
                    {
                        onSlot.SetSlotColor(DataUtility.slot_unMoveColor);
                    }
                }
                else if (findSlot)
                {
                    onSlot.SetSlotColor(DataUtility.slot_unMoveColor);
                }
                else
                {
                    onSlot.SetSlotColor(DataUtility.slot_moveColor);
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
            if (invenMgr.onSlots.Count > 0)
            {
                for (int i = 0; i < invenMgr.onSlots.Count; i++)
                {
                    var onSlot = invenMgr.onSlots[i];
                    if (onSlot.item != null && onSlot.item != invenMgr.holdingItem)
                    {
                        //onSlot.item.targetImage.color = DataUtility.slot_onItemColor;
                        onSlot.SetSlotColor(DataUtility.slot_onItemColor);
                    }
                    else if (onSlot.item != null && onSlot.item == invenMgr.holdingItem)
                    {
                        onSlot.SetSlotColor(DataUtility.slot_onItemColor);
                    }
                    else
                    {
                        onSlot.SetSlotColor(Color.white);
                    }
                }
                invenMgr.onSlots.Clear();
            }
            else
            {
                if (item == null)
                {
                    SetSlotColor(Color.white);
                }
                else
                {
                    SetSlotColor(DataUtility.slot_onItemColor);
                }
            }
        }
        else if (item != null)
        {
            item.targetImage.raycastTarget = false;
        }
    }
}