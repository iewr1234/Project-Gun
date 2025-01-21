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

    public bool CheckEquip(ItemHandler putItem, bool outputError)
    {
        var itemEquip = item != null && item != putItem;
        //if (this.item != null && this.item != item) return false;
        if (putItem == null || putItem.itemData == null) return false;

        ErrorUI errorUI = outputError && gameMenuMgr.gameMgr != null ? gameMenuMgr.gameMgr.errorUI : null;

        if(outputError)
        {

        }
        switch (type)
        {
            case EquipType.Head:
                return CommonCheckProcess(ItemType.Head);
            case EquipType.Body:
                return CommonCheckProcess(ItemType.Body);
            case EquipType.Rig:
                return CommonCheckProcess(ItemType.Rig);
            case EquipType.Backpack:
                return CommonCheckProcess(ItemType.Backpack);
            case EquipType.MainWeapon1:
                return CheckWeaponType();
            case EquipType.MainWeapon2:
                return CheckWeaponType();
            case EquipType.SubWeapon:
                return CheckWeaponType();
            case EquipType.Chamber:
                return !itemEquip && putItem.itemData.type == ItemType.Bullet
                    && popUp != null && popUp.item != null && (popUp.item.itemData.type == ItemType.MainWeapon || popUp.item.itemData.type == ItemType.SubWeapon)
                    && popUp.item.weaponData.caliber == putItem.bulletData.caliber;
            case EquipType.Magazine:
                return CheckMagazineType();
            case EquipType.Muzzle:
                return CheckPartsType();
            case EquipType.Sight:
                return CheckPartsType();
            case EquipType.Attachment:
                return CheckPartsType();
            case EquipType.UnderBarrel:
                return CheckPartsType();
            default:
                return false;
        }

        bool CommonCheckProcess(ItemType itemType)
        {
            if (errorUI != null)
            {
                if (itemEquip && putItem.itemData.type == itemType)
                {
                    // 장착은 가능하지만 이미 아이템이 존재하는 경우
                    errorUI.ShowError("EC00010");
                }
                else if (putItem.itemData.type != itemType)
                {
                    // 장착이 불가능한 경우
                    errorUI.ShowError("EC00006");
                }
            }

            return !itemEquip && putItem.itemData.type == itemType;
        }

        bool CheckWeaponType()
        {
            switch (putItem.itemData.type)
            {
                case ItemType.MainWeapon:
                    if (itemEquip && type != EquipType.SubWeapon)
                    {
                        // 장착은 가능하지만 이미 아이템이 존재하는 경우
                        if (errorUI != null) errorUI.ShowError("EC00010");
                        return false;
                    }
                    else if (type == EquipType.SubWeapon)
                    {
                        // 주무기를 보조무기 슬롯에 장착하려는 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case ItemType.SubWeapon:
                    if (itemEquip && type == EquipType.SubWeapon)
                    {
                        // 장착은 가능하지만 이미 아이템이 존재하는 경우
                        if (errorUI != null) errorUI.ShowError("EC00010");
                        return false;
                    }
                    else if (type != EquipType.SubWeapon)
                    {
                        // 보조무기를 주무기 슬롯에 장착하려는 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case ItemType.Bullet:
                    if (item == null)
                    {
                        // 무기 슬롯에 탄을 삽입할 무기가 존재하지 않는 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else if (!item.weaponData.isMag && item.weaponData.isChamber)
                    {
                        // 무기에 탄창이 없고 약실내 탄이 존재할 경우
                        if (errorUI != null) errorUI.ShowError("EC00010");
                        return false;
                    }
                    else if (item.weaponData.isMag
                          && item.weaponData.equipMag.loadedBullets.Count == item.weaponData.equipMag.magSize
                          && item.weaponData.isChamber)
                    {
                        // 무기에 탄창이 가득 찼으며 약실내 탄이 존재할 경우
                        if (errorUI != null) errorUI.ShowError("EC00010");
                        return false;
                    }
                    else if (item.weaponData.caliber != putItem.bulletData.caliber)
                    {
                        // 무기와 탄의 구경이 같지 않을 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case ItemType.Magazine:
                    if (item == null)
                    {
                        // 무기 슬롯에 탄창을 장착할 무기가 존재하지 않을 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else if (item.weaponData.magType != MagazineType.Magazine)
                    {
                        // 탄창을 장착할 수 없는 무기인 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else if (item.weaponData.isMag)
                    {
                        // 무기에 장착된 탄창이 존재할 경우
                        if (errorUI != null) errorUI.ShowError("EC00010");
                        return false;
                    }
                    else if (!putItem.magData.compatModel.Contains(item.weaponData.model))
                    {
                        // 탄창의 모델이 무기와 호환되지 않는 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else if (putItem.magData.compatCaliber != item.weaponData.caliber)
                    {
                        // 탄창에 사용되는 구경이 무기와 맞지 않는 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case ItemType.Muzzle:
                    return w_CheckParts(WeaponPartsType.Muzzle);
                case ItemType.Sight:
                    return w_CheckParts(WeaponPartsType.Sight);
                case ItemType.Attachment:
                    return w_CheckParts(WeaponPartsType.Attachment);
                case ItemType.UnderBarrel:
                    return w_CheckParts(WeaponPartsType.UnderBarrel);
                default:
                    // 장착할 수 없는 타입의 아이템인 경우
                    if (errorUI != null) errorUI.ShowError("EC00006");
                    return false;
            }

            bool w_CheckParts(WeaponPartsType partsType)
            {
                if (putItem.itemData.type != ItemType.Muzzle
                 && putItem.itemData.type != ItemType.Sight
                 && putItem.itemData.type != ItemType.Attachment
                 && putItem.itemData.type != ItemType.UnderBarrel)
                {
                    // 무기부품이 아닌 경우
                    if (errorUI != null) errorUI.ShowError("EC00006");
                    return false;
                }

                if (item == null || (item.itemData.type != ItemType.MainWeapon && item.itemData.type != ItemType.SubWeapon))
                {
                    // 장착할 무기가 존재하지 않을 경우
                    if (errorUI != null) errorUI.ShowError("EC00006");
                    return false;
                }
                else
                {
                    List<WeaponPartsSize> useParts;
                    switch (partsType)
                    {
                        case WeaponPartsType.Muzzle:
                            useParts = item.weaponData.useMuzzle;
                            break;
                        case WeaponPartsType.Sight:
                            useParts = item.weaponData.useSight;
                            break;
                        case WeaponPartsType.Attachment:
                            useParts = item.weaponData.useAttachment;
                            break;
                        case WeaponPartsType.UnderBarrel:
                            useParts = item.weaponData.useUnderBarrel;
                            break;
                        default:
                            // 체크오류
                            return false;
                    }

                    if (useParts.Count == 0)
                    {
                        // 무기가 부품을 장착할 수 없는 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else if (!putItem.partsData.compatModel.Contains(item.weaponData.model))
                    {
                        // 부품의 모델이 무기와 호환되지 않는 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else if (!useParts.Contains(putItem.partsData.size))
                    {
                        // 부품 사이즈가 무기가 장착할 수 없는 사이즈일 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else if (item.weaponData.equipPartsList.Find(x => x.type == partsType) != null)
                    {
                        // 이미 장착된 부품이 존재하는 경우
                        if (errorUI != null) errorUI.ShowError("EC000010");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        bool CheckMagazineType()
        {
            switch (putItem.itemData.type)
            {
                case ItemType.Bullet:
                    if (popUp == null || popUp.item == null) return false;

                    switch (popUp.item.weaponData.magType)
                    {
                        case MagazineType.Magazine:
                            if (!popUp.item.weaponData.isMag)
                            {
                                // 탄을 삽입할 탄창이 존재하지 않는 경우
                                if (errorUI != null) errorUI.ShowError("EC00002");
                                return false;
                            }
                            else if (popUp.item.weaponData.caliber != putItem.bulletData.caliber)
                            {
                                // 탄의 구경이 무기와 호환되지 않는 경우
                                if (errorUI != null) errorUI.ShowError("EC00006");
                                return false;
                            }
                            else if (popUp.item.weaponData.equipMag.loadedBullets.Count == popUp.item.weaponData.equipMag.magSize)
                            {
                                // 무기의 탄창이 가득 찼을 경우
                                if (errorUI != null) errorUI.ShowError("EC00010");
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        default:
                            if (popUp.item.weaponData.caliber != putItem.bulletData.caliber)
                            {
                                // 탄의 구경이 무기와 호환되지 않는 경우
                                if (errorUI != null) errorUI.ShowError("EC00006");
                                return false;
                            }
                            else if (popUp.item.weaponData.equipMag.loadedBullets.Count == popUp.item.weaponData.equipMag.magSize)
                            {
                                // 무기의 탄창이 가득 찼을 경우
                                if (errorUI != null) errorUI.ShowError("EC00010");
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                    }
                case ItemType.Magazine:
                    if (popUp == null || popUp.item == null) return false;

                    if (item != null)
                    {
                        // 탄창이 이미 존재하는 경우
                        if (errorUI != null) errorUI.ShowError("EC00010");
                        return false;
                    }
                    else if (!putItem.magData.compatModel.Contains(model))
                    {
                        // 탄창의 모델이 무기와 호환되지 않는 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else if (putItem.magData.compatCaliber != caliber)
                    {
                        // 탄창에 사용되는 구경이 무기와 맞지 않는 경우
                        if (errorUI != null) errorUI.ShowError("EC00006");
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                default:
                    return false;
            }
        }

        bool CheckPartsType()
        {
            if (popUp == null || popUp.item == null) return false;

            if (putItem.itemData.type != ItemType.Muzzle
             && putItem.itemData.type != ItemType.Sight
             && putItem.itemData.type != ItemType.Attachment
             && putItem.itemData.type != ItemType.UnderBarrel)
            {
                // 무기부품이 아닌 경우
                if (errorUI != null) errorUI.ShowError("EC00006");
                return false;
            }

            var weapon = popUp.item.itemData.type == ItemType.MainWeapon || popUp.item.itemData.type == ItemType.SubWeapon ? popUp.item : null;
            if (weapon == null)
            {
                // PopUp의 아이템이 무기가 아닌 경우
                return false;
            }

            List<WeaponPartsSize> useParts;
            switch (type)
            {
                case EquipType.Muzzle:
                    useParts = weapon.weaponData.useMuzzle;
                    break;
                case EquipType.Sight:
                    useParts = weapon.weaponData.useSight;
                    break;
                case EquipType.Attachment:
                    useParts = weapon.weaponData.useAttachment;
                    break;
                case EquipType.UnderBarrel:
                    useParts = weapon.weaponData.useUnderBarrel;
                    break;
                default:
                    // 무기부품이 아닌 경우
                    return false;
            }

            if (useParts.Count == 0)
            {
                // 무기가 부품을 장착할 수 없는 경우
                if (errorUI != null) errorUI.ShowError("EC00006");
                return false;
            }
            else if (!putItem.partsData.compatModel.Contains(weapon.weaponData.model))
            {
                // 부품의 모델이 무기와 호환되지 않는 경우
                if (errorUI != null) errorUI.ShowError("EC00006");
                return false;
            }
            else if (!useParts.Contains(putItem.partsData.size))
            {
                // 부품 사이즈가 무기가 장착할 수 없는 사이즈일 경우
                if (errorUI != null) errorUI.ShowError("EC00006");
                return false;
            }
            else if (weapon.weaponData.equipPartsList.Find(x => x.type == putItem.partsData.type) != null)
            {
                // 이미 장착된 부품이 존재하는 경우
                if (errorUI != null) errorUI.ShowError("EC000010");
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public bool CheckEquip(BulletDataInfo bulletData)
    {
        if (popUp == null || popUp.item == null) return false;

        ErrorUI errorUI = gameMenuMgr.gameMgr != null ? gameMenuMgr.gameMgr.errorUI : null;
        if (type != EquipType.Chamber)
        {
            // 약실 슬롯이 아닌 경우
            if (errorUI != null) errorUI.ShowError("EC00006");
            return false;
        }
        else if (item != null)
        {
            // 약실에 탄이 존재하는 경우
            if (errorUI != null) errorUI.ShowError("EC00010");
            return false;
        }
        else if (bulletData.caliber != caliber)
        {
            // 무기와 탄의 구경이 같지 않을 경우
            if (errorUI != null) errorUI.ShowError("EC00006");
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool CheckEquip(MagazineDataInfo magData)
    {
        if (popUp == null || popUp.item == null) return false;

        ErrorUI errorUI = gameMenuMgr.gameMgr != null ? gameMenuMgr.gameMgr.errorUI : null;
        if (type != EquipType.Magazine)
        {
            // 탄창 슬롯이 아닌 경우
            if (errorUI != null) errorUI.ShowError("EC00006");
            return false;
        }
        else if (item != null)
        {
            // 장착된 탄창이 존재하는 경우
            if (errorUI != null) errorUI.ShowError("EC00010");
            return false;
        }
        else if (!magData.compatModel.Contains(model))
        {
            // 탄창의 모델이 무기와 호환되지 않는 경우
            if (errorUI != null) errorUI.ShowError("EC00006");
            return false;
        }
        else if (magData.compatCaliber != caliber)
        {
            // 탄창에 사용되는 구경이 무기와 맞지 않는 경우
            if (errorUI != null) errorUI.ShowError("EC00006");
            return false;
        }
        else
        {
            return true;
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

                string spriteName = (item.weaponData.weaponType != global::WeaponType.Revolver && item.weaponData.isChamber)
                                 || (item.weaponData.weaponType == global::WeaponType.Revolver && item.weaponData.equipMag.loadedBullets.Count > 0)
                                  ? "Icon_Chamber_on" : "Icon_Chamber_off";
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
            if (CheckEquip(gameMenuMgr.holdingItem, false))
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
            item.SetActiveItemTarget(true);
        }
    }

    public void PointerExit_EquipSlot()
    {
        gameMenuMgr.onEquip = null;
        backImage.color = DataUtility.equip_defaultColor;
        if (item != null)
        {
            item.SetActiveItemTarget(false);
        }
    }
}
