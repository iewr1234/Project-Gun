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

    public void SetItemSlot(Color color)
    {
        slotImage.color = color;
    }

    public void PointerEnter_ItemSlot()
    {
        var invenMgr = myStorage != null ? myStorage.invenMgr : otherStorage.invenMgr;
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
            invenMgr.onSlots = invenMgr.FindAllMultiSizeSlots(itemSlots, item, startIndex);
            var sizeCount = item.size.x * item.size.y;
            var findSlot = invenMgr.onSlots.Find(x => x.item != null && x.item != item);

            var itemNesting = invenMgr.onSlot.item != null && invenMgr.onSlot.item != item
                           && invenMgr.onSlot.item.itemData.ID == item.itemData.ID
                           && invenMgr.onSlot.item.itemData.maxNesting > 1
                           && invenMgr.onSlot.item.TotalCount < invenMgr.onSlot.item.itemData.maxNesting;
            var insertBullets = findSlot && findSlot.item.itemData.type == ItemType.Magazine && item.itemData.type == ItemType.Bullet;
            var equipMagazine = findSlot && findSlot.item.itemSlots.Contains(invenMgr.onSlot)
                             && item.itemData.type == ItemType.Magazine
                             && (findSlot.item.itemData.type == ItemType.MainWeapon || findSlot.item.itemData.type == ItemType.SubWeapon);
            var canMove = !findSlot && sizeCount == invenMgr.onSlots.Count;
            for (int i = 0; i < invenMgr.onSlots.Count; i++)
            {
                var onSlot = invenMgr.onSlots[i];
                if (itemNesting || canMove)
                {
                    onSlot.SetItemSlot(DataUtility.slot_moveColor);
                }
                else if (insertBullets)
                {
                    findSlot.item.SetItemSlots(findSlot.item.magData.loadedBullets.Count < findSlot.item.magData.magSize ? DataUtility.slot_moveColor : DataUtility.slot_unMoveColor);
                }
                else if (equipMagazine)
                {
                    findSlot.item.SetItemSlots(!findSlot.item.weaponData.isMag
                                            && item.magData.compatModel.Contains(findSlot.item.weaponData.model) ? DataUtility.slot_moveColor : DataUtility.slot_unMoveColor);
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
            }

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