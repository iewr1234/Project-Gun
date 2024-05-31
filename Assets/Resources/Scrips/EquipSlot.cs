using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Progress;

public enum EquipType
{
    None,
    Head,
    Body,
    Rig,
    Backpack,
    MainWeapon,
    SubWeapon,
    Chamber,
    Magazine,
    Muzzle,
    Sight,
    UnderRail,
    Rail,
}

public class EquipSlot : MonoBehaviour
{
    [Header("---Access Script---")]
    private InventoryManager invenMgr;

    [Header("---Access Component---")]
    public Image backImage;
    public TextMeshProUGUI slotText;

    [Header("--- Assignment Variable---")]
    public EquipType type;
    public List<WeaponPartsSize> sizeList;
    public ItemHandler item;

    private readonly Color defaultColor = new Color(50 / 255f, 50 / 255f, 50 / 255f);

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

        backImage = transform.Find("BackGround").GetComponent<Image>();
        slotText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    public void EquipItem(ItemHandler _item)
    {
        item = _item;
        backImage.color = defaultColor;
        slotText.enabled = false;

        item.equipSlot = this;
        for (int i = 0; i < item.itemSlots.Count; i++)
        {
            var itemSlot = item.itemSlots[i];
            itemSlot.SetSlotColor(Color.white);
            itemSlot.item = null;
        }
        item.itemSlots.Clear();

        item.ChangeRectPivot(true);
        item.transform.SetParent(transform, false);
        item.transform.localPosition = Vector3.zero;
        item.targetImage.color = Color.clear;
        item.targetImage.raycastTarget = true;

        invenMgr.holdingItem = null;
        invenMgr.InactiveSampleItem();
    }

    public void PointerEnter_EquipSlot()
    {
        invenMgr.onEquip = this;
        if (invenMgr.holdingItem != null)
        {
            if (invenMgr.holdingItem.CheckEquip(this))
            {
                backImage.color = DataUtility.slot_onItemColor;
            }
            else
            {
                backImage.color = DataUtility.slot_unMoveColor;
            }
        }
        else if (item != null)
        {
            item.targetImage.raycastTarget = true;
        }
    }

    public void PointerExit_EquipSlot()
    {
        invenMgr.onEquip = null;
        backImage.color = defaultColor;
        if (item != null)
        {
            item.targetImage.raycastTarget = false;
        }
    }
}
