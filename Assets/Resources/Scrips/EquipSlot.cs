using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

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
    public Image chamberImage;

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
        chamberImage = transform.Find("Count/Chamber").GetComponent<Image>();
        chamberImage.enabled = false;

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
        chamberImage = transform.Find("Count/Chamber").GetComponent<Image>();
        chamberImage.enabled = false;

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
        chamberImage = transform.Find("Count/Chamber").GetComponent<Image>();
        chamberImage.enabled = false;

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
                        && putItem.magData.compatModel.Contains(item.weaponData.model)
                        && putItem.magData.compatCaliber == item.weaponData.caliber;
                case ItemType.Muzzle:
                    return item != null && (item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon)
                        && item.weaponData.useMuzzle.Count > 0 && item.weaponData.useMuzzle.Contains(putItem.partsData.size)
                        && item.weaponData.equipPartsList.Find(x => x.type == WeaponPartsType.Muzzle) == null;
                case ItemType.Sight:
                    return item != null && (item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon)
                        && item.weaponData.useSight.Count > 0 && item.weaponData.useSight.Contains(putItem.partsData.size)
                        && item.weaponData.equipPartsList.Find(x => x.type == WeaponPartsType.Sight) == null;
                case ItemType.Attachment:
                    return item != null && (item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon)
                        && item.weaponData.useAttachment.Count > 0 && item.weaponData.useAttachment.Contains(putItem.partsData.size)
                        && item.weaponData.equipPartsList.Find(x => x.type == WeaponPartsType.Attachment) == null;
                case ItemType.UnderBarrel:
                    return item != null && (item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon)
                        && item.weaponData.useUnderBarrel.Count > 0 && item.weaponData.useUnderBarrel.Contains(putItem.partsData.size)
                        && item.weaponData.equipPartsList.Find(x => x.type == WeaponPartsType.UnderBarrel) == null;
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
                    return item == null && putItem.magData != null && popUp != null
                        && putItem.magData.compatModel.Contains(model)
                        && putItem.magData.compatCaliber == caliber;
                default:
                    return false;
            }
        }

        bool PartsType()
        {
            var partsData = putItem.partsData;
            if (partsData == null) return false;

            if (popUp == null) return false;

            var weapon = popUp.item;
            if (weapon == null) return false;

            switch (type)
            {
                case EquipType.Muzzle:
                    if (weapon.weaponData.equipPartsList.Find(x => x.type == partsData.type) != null) return false;

                    return partsData != null
                        && partsData.type == WeaponPartsType.Muzzle
                        && partsData.compatModel.Contains(model)
                        && sizeList.Contains(partsData.size);
                case EquipType.Sight:
                    if (weapon.weaponData.equipPartsList.Find(x => x.type == partsData.type) != null) return false;

                    return partsData != null
                        && partsData.type == WeaponPartsType.Sight
                        && partsData.compatModel.Contains(model)
                        && sizeList.Contains(partsData.size);
                case EquipType.Attachment:
                    return partsData != null
                        && partsData.type == WeaponPartsType.Attachment
                        && partsData.compatModel.Contains(model)
                        && sizeList.Contains(partsData.size);
                case EquipType.UnderBarrel:
                    if (weapon.weaponData.equipPartsList.Find(x => x.type == partsData.type) != null) return false;

                    return partsData != null
                        && partsData.type == WeaponPartsType.UnderBarrel
                        && partsData.compatModel.Contains(model)
                        && sizeList.Contains(partsData.size);
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
        if (popUp == null) return false;

        var weapon = popUp.item;
        if (weapon == null) return false;

        switch (type)
        {
            case EquipType.Muzzle:
                if (weapon.weaponData.equipPartsList.Find(x => x.type == partsData.type) != null) return false;

                return partsData != null
                    && partsData.type == WeaponPartsType.Muzzle
                    && partsData.compatModel.Contains(model)
                    && sizeList.Contains(partsData.size);
            case EquipType.Sight:
                if (weapon.weaponData.equipPartsList.Find(x => x.type == partsData.type) != null) return false;

                return partsData != null
                    && partsData.type == WeaponPartsType.Sight
                    && partsData.compatModel.Contains(model)
                    && sizeList.Contains(partsData.size);
            case EquipType.Attachment:
                return partsData != null
                    && partsData.type == WeaponPartsType.Attachment
                    && partsData.compatModel.Contains(model)
                    && sizeList.Contains(partsData.size);
            case EquipType.UnderBarrel:
                if (weapon.weaponData.equipPartsList.Find(x => x.type == partsData.type) != null) return false;

                return partsData != null
                    && partsData.type == WeaponPartsType.UnderBarrel
                    && partsData.compatModel.Contains(model)
                    && sizeList.Contains(partsData.size);
            default:
                return false;
        }
    }

    public void SetItemCount()
    {
        if (item != null)
        {
            FixTextTheItemCount(item);
        }
        else
        {
            slotText.enabled = true;
            countText.enabled = false;
            chamberImage.enabled = false;
        }
    }

    public void SetLoadedBulletCount(ItemHandler item)
    {
        FixTextTheItemCount(item);
    }

    private void FixTextTheItemCount(ItemHandler item)
    {
        if (item == null) return;

        switch (item.itemData.type)
        {
            case ItemType.Head:
                ArmorType();
                break;
            case ItemType.Body:
                ArmorType();
                break;
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
                chamberImage.enabled = false;
                break;
        }

        void ArmorType()
        {
            countText.enabled = true;
            countText.text = $"{item.armorData.durability}<size=14>/{item.armorData.maxDurability}</size>";
            chamberImage.enabled = false;
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
                chamberImage.enabled = false;
            }
            else
            {
                var loadedNum = 0;
                var magMax = 0;
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

                string spriteName = item.weaponData.isChamber ? "Icon_Chamber_on" : "Icon_Chamber_off";
                chamberImage.sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
                chamberImage.enabled = true;
            }
        }

        void MagazineType()
        {
            countText.enabled = true;
            countText.text = $"{item.TotalCount}/{item.magData.magSize}";
            chamberImage.enabled = false;
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
