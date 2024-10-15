using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditorInternal.Profiling.Memory.Experimental;

public enum EquipType
{
    None,
    Head,
    Body,
    Rig,
    Backpack,
    MainWeapon1,
    MainWeapon2,
    SubWeapon,
    //Chamber,
    Magazine,
    Muzzle,
    Sight,
    UnderRail,
    Rail,
}

public class EquipSlot : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameMenuManager gameMenuMgr;
    public MyStorage myStorage;
    public PopUp_Inventory popUp;

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
    public int intMagMax;
    public ItemHandler item;

    public void SetComponents(GameMenuManager _gameMenuMgr)
    {
        gameMenuMgr = _gameMenuMgr;

        outline = transform.Find("Outline").GetComponent<Image>();
        outline.enabled = false;
        backImage = transform.Find("BackGround").GetComponent<Image>();
        slotText = transform.Find("SlotName").GetComponent<TextMeshProUGUI>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();
        countText.enabled = false;

        gameMenuMgr.allEquips.Add(this);
    }

    public void SetComponents(GameMenuManager _gameMenuMgr, PopUp_Inventory _popUp)
    {
        gameMenuMgr = _gameMenuMgr;
        popUp = _popUp;

        outline = transform.Find("Outline").GetComponent<Image>();
        outline.enabled = false;
        backImage = transform.Find("BackGround").GetComponent<Image>();
        slotText = transform.Find("SlotName").GetComponent<TextMeshProUGUI>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();
        countText.enabled = false;

        gameMenuMgr.allEquips.Add(this);
    }

    public void SetComponents(GameMenuManager _gameMenuMgr, MyStorage _myStorage)
    {
        gameMenuMgr = _gameMenuMgr;
        myStorage = _myStorage;

        outline = transform.Find("Outline").GetComponent<Image>();
        outline.enabled = false;
        backImage = transform.Find("BackGround").GetComponent<Image>();
        slotText = transform.Find("SlotName").GetComponent<TextMeshProUGUI>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();
        countText.enabled = false;

        gameMenuMgr.allEquips.Add(this);
    }

    public bool CheckEquip(ItemHandler item)
    {
        if (this.item != null && this.item != item) return false;
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
            case EquipType.MainWeapon1:
                return item.itemData.type == ItemType.MainWeapon;
            case EquipType.MainWeapon2:
                return item.itemData.type == ItemType.MainWeapon;
            case EquipType.SubWeapon:
                return item.itemData.type == ItemType.SubWeapon;
            //case EquipType.Chamber:
            //    return item.itemData.type == ItemType.Bullet
            //        && item.bulletData != null;
            case EquipType.Magazine:
                switch (item.itemData.type)
                {
                    case ItemType.Bullet:
                        if (intMagMax == 0) return false;
                        if (this.item != null && this.item.TotalCount == intMagMax) return false;

                        return item.bulletData != null && item.bulletData.caliber == caliber;
                    case ItemType.Magazine:
                        return this.item == null && item.magData != null && popUp != null && item.magData.compatModel.Contains(model);
                    default:
                        return false;
                }
            case EquipType.Sight:
                return item.itemData.type == ItemType.Sight
                    && item.partsData != null
                    && item.partsData.compatModel.Contains(model);
            default:
                return false;
        }
    }

    //public bool CheckEquip(BulletDataInfo bulletData)
    //{
    //    return type == EquipType.Chamber
    //                && bulletData != null
    //                && bulletData.caliber == caliber;
    //}

    public bool CheckEquip(MagazineDataInfo magData)
    {
        return type == EquipType.Magazine
                    && popUp != null && popUp.item != null
                    && magData != null
                    && magData.compatModel.Contains(model);
    }

    public bool CheckEquip(WeaponPartsDataInfo partsData)
    {
        switch (type)
        {
            case EquipType.Sight:
                return partsData != null
                    && popUp != null && popUp.item != null
                    && partsData.type == WeaponPartsType.Sight
                    && partsData.compatModel.Contains(model);
            default:
                return false;
        }
    }

    public void SetLoadedBulletCount()
    {
        if (item == null) return;

        switch (item.itemData.type)
        {
            case ItemType.MainWeapon:
                WeaponType();
                break;
            case ItemType.SubWeapon:
                WeaponType();
                break;
            case ItemType.Magazine:
                MagazineType();
                break;
            default:
                countText.enabled = false;
                break;
        }

        void WeaponType()
        {
            countText.enabled = true;
            if (item.weaponData.isMag)
            {
                countText.text = $"{item.weaponData.equipMag.loadedBullets.Count}";
            }
            else
            {
                countText.text = "0";
            }
        }

        void MagazineType()
        {
            countText.enabled = true;
            countText.text = $"{item.TotalCount}";
        }
    }

    public void PointerEnter_EquipSlot()
    {
        gameMenuMgr.onSlot = null;
        gameMenuMgr.onSlots.Clear();
        gameMenuMgr.onEquip = this;
        if (gameMenuMgr.holdingItem != null)
        {
            if (CheckEquip(gameMenuMgr.holdingItem))
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
        gameMenuMgr.onEquip = null;
        backImage.color = DataUtility.equip_defaultColor;
        if (item != null)
        {
            item.targetImage.raycastTarget = false;
        }
    }
}
