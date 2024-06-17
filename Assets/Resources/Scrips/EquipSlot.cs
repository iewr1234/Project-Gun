using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public Image outline;
    public Image backImage;
    public TextMeshProUGUI slotText;
    public TextMeshProUGUI countText;

    [Header("--- Assignment Variable---")]
    public EquipType type;
    public List<WeaponPartsSize> sizeList;
    public int model;
    public float caliber;
    public ItemHandler item;

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

        outline = transform.Find("Outline").GetComponent<Image>();
        outline.enabled = false;
        backImage = transform.Find("BackGround").GetComponent<Image>();
        slotText = transform.Find("SlotName").GetComponent<TextMeshProUGUI>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();
        countText.enabled = false;

        invenMgr.allEquips.Add(this);
    }

    public bool CheckEquip(ItemHandler item)
    {
        if (item == null || item.itemData == null) return false;

        switch (type)
        {
            case EquipType.Head:
                return item.itemData.type == ItemType.Head;
            case EquipType.Body:
                return item.itemData.type == ItemType.Body;
            case EquipType.Rig:
                return item.itemData.type == ItemType.Rig;
            case EquipType.Backpack:
                return item.itemData.type == ItemType.Backpack;
            case EquipType.MainWeapon:
                return item.itemData.type == ItemType.MainWeapon;
            case EquipType.SubWeapon:
                return item.itemData.type == ItemType.SubWeapon;
            case EquipType.Chamber:
                return item.itemData.type == ItemType.Bullet
                    && item.bulletData != null;
            case EquipType.Magazine:
                if (item.itemData.type == ItemType.Magazine)
                {
                    return this.item == null
                        && item.magData != null
                        && item.magData.compatModel.Contains(model);
                }
                return false;
            case EquipType.Sight:
                return item.itemData.type == ItemType.Sight
                    && item.partsData != null
                    && item.partsData.compatModel.Contains(model);
            default:
                return false;
        }
    }

    public bool CheckEquip(BulletDataInfo bulletData)
    {
        return type == EquipType.Chamber
                    && bulletData != null
                    && bulletData.caliber == caliber;
    }

    public bool CheckEquip(MagazineDataInfo magData)
    {
        return type == EquipType.Magazine
                    && magData != null
                    && magData.compatModel.Contains(model);
    }

    public bool CheckEquip(WeaponPartsDataInfo partsData)
    {
        switch (type)
        {
            case EquipType.Sight:
                return partsData != null
                    && partsData.type == WeaponPartsType.Sight
                    && partsData.compatModel.Contains(model);
            default:
                return false;
        }
    }

    public void PointerEnter_EquipSlot()
    {
        invenMgr.onSlot = null;
        invenMgr.onSlots.Clear();
        invenMgr.onEquip = this;
        if (invenMgr.holdingItem != null)
        {
            if (CheckEquip(invenMgr.holdingItem))
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
        backImage.color = DataUtility.equip_defaultColor;
        if (item != null)
        {
            item.targetImage.raycastTarget = false;
        }
    }
}
