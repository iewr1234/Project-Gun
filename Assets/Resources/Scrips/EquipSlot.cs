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
    MainWeapon1,
    MainWeapon2,
    SubWeapon,
    Chamber,
    Magazine,
    Muzzle,
    Sight,
    Attachment,
    UnderBarrel,
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
        var itemEquip = item != null && item != putItem;
        //if (this.item != null && this.item != item) return false;
        if (putItem == null || putItem.itemData == null) return false;

        switch (type)
        {
            case EquipType.Head:
                return !itemEquip && putItem.itemData.type == ItemType.Head;
            case EquipType.Body:
                return !itemEquip && putItem.itemData.type == ItemType.Body;
            case EquipType.Rig:
                return !itemEquip && putItem.itemData.type == ItemType.Rig;
            case EquipType.Backpack:
                return !itemEquip && putItem.itemData.type == ItemType.Backpack;
            case EquipType.MainWeapon1:
                return WeaponType();
            case EquipType.MainWeapon2:
                return WeaponType();
            case EquipType.SubWeapon:
                return WeaponType();
            case EquipType.Chamber:
                return !itemEquip && putItem.itemData.type == ItemType.Bullet
                    && popUp != null && popUp.item != null && (popUp.item.itemData.type == ItemType.MainWeapon || popUp.item.itemData.type == ItemType.SubWeapon)
                    && popUp.item.weaponData.caliber == putItem.bulletData.caliber;
            case EquipType.Magazine:
                return MagazineType();
            case EquipType.Muzzle:
                return PartsType();
            case EquipType.Sight:
                return PartsType();
            case EquipType.Attachment:
                return PartsType();
            case EquipType.UnderBarrel:
                return PartsType();
            default:
                return false;
        }

        bool WeaponType()
        {
            switch (putItem.itemData.type)
            {
                case ItemType.MainWeapon:
                    return !itemEquip && (type == EquipType.MainWeapon1 || type == EquipType.MainWeapon2);
                case ItemType.SubWeapon:
                    return !itemEquip && type == EquipType.SubWeapon;
                case ItemType.Bullet:
                    if (item == null || !(item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon)) return false;

                    if (item.weaponData.isMag)
                    {
                        return item.weaponData.caliber == putItem.bulletData.caliber
                           && (item.weaponData.equipMag.loadedBullets.Count < item.weaponData.equipMag.magSize || !item.weaponData.isChamber);
                    }
                    else
                    {
                        return !item.weaponData.isChamber && item.weaponData.caliber == putItem.bulletData.caliber;
                    }
                case ItemType.Magazine:
                    if (item == null || !(item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon)) return false;

                    return item.weaponData.magType == global::MagazineType.Magazine && !item.weaponData.isMag
                        && putItem.magData.compatModel.Contains(item.weaponData.model);
                default:
                    return false;
            }
        }

        bool MagazineType()
        {
            switch (putItem.itemData.type)
            {
                case ItemType.Bullet:
                    if (popUp.item == null) return false;

                    switch (popUp.item.weaponData.magType)
                    {
                        case global::MagazineType.Magazine:
                            return popUp.item.weaponData.isMag && popUp.item.weaponData.caliber == putItem.bulletData.caliber
                                && popUp.item.weaponData.equipMag.loadedBullets.Count < popUp.item.weaponData.equipMag.magSize;
                        case global::MagazineType.IntMagazine:
                            return popUp.item.weaponData.caliber == putItem.bulletData.caliber
                                && popUp.item.weaponData.equipMag.loadedBullets.Count < popUp.item.weaponData.equipMag.magSize;
                        case global::MagazineType.Cylinder:
                            return popUp.item.weaponData.caliber == putItem.bulletData.caliber
                                && popUp.item.weaponData.equipMag.loadedBullets.Count < popUp.item.weaponData.equipMag.magSize;
                        default:
                            return false;
                    }
                case ItemType.Magazine:
                    return item == null && putItem.magData != null && popUp != null && putItem.magData.compatModel.Contains(model);
                default:
                    return false;
            }
        }

        bool PartsType()
        {
            switch (putItem.partsData.type)
            {
                case WeaponPartsType.Muzzle:
                    return !itemEquip
                         && type == EquipType.Muzzle
                         && putItem.partsData != null
                         && putItem.partsData.compatModel.Contains(model);
                case WeaponPartsType.Sight:
                    return !itemEquip
                         && type == EquipType.Sight
                         && putItem.partsData != null
                         && putItem.partsData.compatModel.Contains(model);
                case WeaponPartsType.Attachment:
                    return !itemEquip
                         && type == EquipType.Attachment
                         && putItem.partsData != null
                         && putItem.partsData.compatModel.Contains(model);
                case WeaponPartsType.UnderBarrel:
                    return !itemEquip
                         && type == EquipType.UnderBarrel
                         && putItem.partsData != null
                         && putItem.partsData.compatModel.Contains(model);
                default:
                    return false;
            }
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
                    && popUp != null && popUp.item != null
                    && magData != null
                    && magData.compatModel.Contains(model);
    }

    public bool CheckEquip(WeaponPartsDataInfo partsData)
    {
        switch (type)
        {
            case EquipType.Muzzle:
                return partsData != null
                    && popUp != null && popUp.item != null
                    && partsData.type == WeaponPartsType.Muzzle
                    && partsData.compatModel.Contains(model);
            case EquipType.Sight:
                return partsData != null
                    && popUp != null && popUp.item != null
                    && partsData.type == WeaponPartsType.Sight
                    && partsData.compatModel.Contains(model);
            case EquipType.Attachment:
                return partsData != null
                    && popUp != null && popUp.item != null
                    && partsData.type == WeaponPartsType.Attachment
                    && partsData.compatModel.Contains(model);
            case EquipType.UnderBarrel:
                return partsData != null
                    && popUp != null && popUp.item != null
                    && partsData.type == WeaponPartsType.UnderBarrel
                    && partsData.compatModel.Contains(model);
            default:
                return false;
        }
    }

    public void SetLoadedBulletCount()
    {
        if (item != null)
        {
            SetCountText(item);
        }
        else
        {
            slotText.enabled = true;
            countText.enabled = false;
        }
    }

    public void SetLoadedBulletCount(ItemHandler item)
    {
        SetCountText(item);
    }

    private void SetCountText(ItemHandler item)
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
            if (type == EquipType.Magazine)
            {
                if (item.weaponData.magType == global::MagazineType.Magazine)
                {
                    if (item.weaponData.isMag)
                    {
                        countText.enabled = true;
                        countText.text = $"{item.weaponData.equipMag.loadedBullets.Count}<size=14>/{item.weaponData.equipMag.magSize}</size>";
                    }
                    else
                    {
                        slotText.enabled = true;
                        countText.enabled = false;
                    }

                }
                else
                {
                    slotText.enabled = true;
                    countText.enabled = true;
                    countText.text = $"{item.weaponData.equipMag.loadedBullets.Count}<size=14>/{item.weaponData.equipMag.magSize}</size>";
                }
            }
            else
            {
                var loadedNum = 0;
                var magMax = 0;
                if (item.weaponData.isChamber) loadedNum++;

                if (item.weaponData.magType == global::MagazineType.Magazine)
                {
                    if (item.weaponData.isMag)
                    {
                        loadedNum += item.weaponData.equipMag.loadedBullets.Count;
                        magMax = item.weaponData.equipMag.magSize;
                    }
                }
                else
                {
                    loadedNum += item.weaponData.equipMag.loadedBullets.Count;
                    magMax = item.weaponData.equipMag.magSize;

                }
                slotText.enabled = false;
                countText.enabled = true;
                countText.text = $"{loadedNum}<size=14>/{magMax}</size>";
            }
        }

        void MagazineType()
        {
            countText.enabled = true;
            countText.text = $"{item.TotalCount}/{item.magData.magSize}";
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
