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
    public Image backImage;
    public TextMeshProUGUI slotText;

    [Header("--- Assignment Variable---")]
    public EquipType type;
    public List<WeaponPartsSize> sizeList;
    public ItemHandler item;

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

        backImage = transform.Find("BackGround").GetComponent<Image>();
        slotText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    public void PointerEnter_EquipSlot()
    {

    }

    public void PointerExit_EquipSlot()
    {

    }
}
