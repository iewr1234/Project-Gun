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

    public bool CheckEquip(ItemHandler putItem)
    {
        var noEquip = item != null && item != putItem;
        //if (this.item != null && this.item != item) return false;
        if (putItem == null || putItem.itemData == null) return false;

        switch (type)
        {
            case EquipType.Head:
                return !noEquip && putItem.itemData.type == ItemType.Head;
            case EquipType.Body:
                return !noEquip && putItem.itemData.type == ItemType.Body;
            case EquipType.Rig:
                return !noEquip && putItem.itemData.type == ItemType.Rig;
            case EquipType.Backpack:
                return !noEquip && putItem.itemData.type == ItemType.Backpack;
            case EquipType.MainWeapon1:
                return WeaponType();
            case EquipType.MainWeapon2:
                return WeaponType();
            case EquipType.SubWeapon:
                return WeaponType();
            //case EquipType.Chamber:
            //    return item.itemData.type == ItemType.Bullet
            //        && item.bulletData != null;
            case EquipType.Magazine:
                return MagazineType();
            case EquipType.Sight:
                return !noEquip
                    && putItem.itemData.type == ItemType.Sight
                    && putItem.partsData != null
                    && putItem.partsData.compatModel.Contains(model);
            default:
                return false;
        }

        bool WeaponType()
        {
            switch (putItem.itemData.type)
            {
                case ItemType.MainWeapon:
                    return !noEquip && (type == EquipType.MainWeapon1 || type == EquipType.MainWeapon2);
                case ItemType.SubWeapon:
                    return !noEquip && type == EquipType.SubWeapon;
                case ItemType.Bullet:
                    return item != null && (item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon)
                        && item.weaponData.isMag && item.weaponData.equipMag.intMag
                        && item.weaponData.equipMag.compatCaliber == putItem.bulletData.caliber
                        && item.weaponData.equipMag.loadedBullets.Count < item.weaponData.equipMag.magSize;
                case ItemType.Magazine:
                    if (item != null && (item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon))
                    {
                        if (!item.weaponData.isMag)
                        {
                            //무기에 탄창이 없을 경우
                            return putItem.magData.compatModel.Contains(item.weaponData.model);
                        }
                        else
                        {
                            if (!item.weaponData.equipMag.intMag)
                            {
                                //무기에 탄창이 있는 경우


                                return false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        bool MagazineType()
        {
            switch (putItem.itemData.type)
            {
                case ItemType.Bullet:
                    if (intMagMax == 0) return false;
                    if (item != null && item.TotalCount == intMagMax) return false;

                    return putItem.bulletData != null && putItem.bulletData.caliber == caliber;
                case ItemType.Magazine:
                    return item == null && putItem.magData != null && popUp != null && putItem.magData.compatModel.Contains(model);
                default:
                    return false;
            }
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
