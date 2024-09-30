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
    public Vector2Int slotIndex;
    public ItemHandler item;

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

    public void SetComponents(MyStorage _myStorage)
    {
        myStorage = _myStorage;

        slotImage = transform.Find("BackGround").GetComponent<Image>();

        //slotIndex = _slotIndex;
        //transform.name = $"Slot_X{slotIndex.x}/Y{slotIndex.y}";
        gameObject.SetActive(false);
    }

    public void SetComponents(OtherStorage _otherStorage, Vector2Int _slotIndex)
    {
        otherStorage = _otherStorage;

        slotImage = transform.Find("BackGround").GetComponent<Image>();

        slotIndex = _slotIndex;
        transform.name = $"Slot_X{slotIndex.x}/Y{slotIndex.y}";
    }

    public void SetSlotIndex(Vector2Int _slotIndex)
    {
        slotIndex = _slotIndex;
        transform.name = $"Slot_X{slotIndex.x}/Y{slotIndex.y}";
    }

    public void SetItemSlot(Color color)
    {
        slotImage.color = color;
    }

    public void PointerEnter_ItemSlot()
    {
        var invenMgr = myStorage != null ? myStorage.gameMenuMgr : otherStorage.gameMenuMgr;
        invenMgr.onEquip = null;
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
            invenMgr.onSlots = invenMgr.FindAllMultiSizeSlots(itemSlots, item.size, startIndex);
            var sizeCount = item.size.x * item.size.y;
            var findSlot = invenMgr.onSlots.Find(x => x.item != null && x.item != item);

            var itemNesting = invenMgr.onSlot.item != null && invenMgr.onSlot.item != item
                           && invenMgr.onSlot.item.itemData.ID == item.itemData.ID
                           && invenMgr.onSlot.item.itemData.maxNesting > 1
                           && invenMgr.onSlot.item.TotalCount < invenMgr.onSlot.item.itemData.maxNesting;
            var canMove = !findSlot && sizeCount == invenMgr.onSlots.Count;
            var storageInTheStorage = myStorage != null
                                   && (myStorage.type == MyStorageType.Rig && item.itemData.type == ItemType.Rig
                                   || myStorage.type == MyStorageType.Backpack && item.itemData.type == ItemType.Backpack);
            for (int i = 0; i < invenMgr.onSlots.Count; i++)
            {
                var onSlot = invenMgr.onSlots[i];
                if (itemNesting || (canMove && !storageInTheStorage))
                {
                    onSlot.SetItemSlot(DataUtility.slot_moveColor);
                }
                else if (findSlot && findSlot.item.itemSlots.Contains(invenMgr.onSlot))
                {
                    findSlot.item.SetItemSlots(invenMgr.CheckEquip(findSlot.item, item) ? DataUtility.slot_moveColor : DataUtility.slot_unMoveColor);
                }
                else
                {
                    onSlot.SetItemSlot(DataUtility.slot_unMoveColor);
                }
            }
        }
    }

    public void PointerExit_ItemSlot()
    {
        var invenMgr = myStorage != null ? myStorage.gameMenuMgr : otherStorage.gameMenuMgr;
        invenMgr.onSlot = null;
        if (invenMgr.holdingItem != null)
        {
            for (int i = 0; i < invenMgr.onSlots.Count; i++)
            {
                var onSlot = invenMgr.onSlots[i];
                if (onSlot.item != null && onSlot.item != invenMgr.holdingItem)
                {
                    onSlot.item.SetItemSlots(DataUtility.slot_onItemColor);
                }
                else if (onSlot.item != null && onSlot.item == invenMgr.holdingItem)
                {
                    onSlot.SetItemSlot(DataUtility.slot_onItemColor);
                }
                else
                {
                    onSlot.SetItemSlot(DataUtility.slot_noItemColor);
                }
            }
            invenMgr.onSlots.Clear();

            if (item == null)
            {
                SetItemSlot(DataUtility.slot_noItemColor);
            }
            else
            {
                item.SetItemSlots(DataUtility.slot_onItemColor);
            }
        }
        else if (item != null)
        {
            item.targetImage.raycastTarget = false;
        }
    }
}