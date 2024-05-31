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

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

        backImage = transform.Find("BackGround").GetComponent<Image>();
        slotText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
    }

    public void EquipItem(ItemHandler _item)
    {
        item = _item;
        item.transform.SetParent(transform, false);
        item.rect.pivot = new Vector2(0.5f, 0.5f);
        item.transform.localScale = Vector3.zero;
        slotText.enabled = false;
    }

    public void PointerEnter_EquipSlot()
    {
        invenMgr.onEquip = this;
    }

    public void PointerExit_EquipSlot()
    {
        invenMgr.onEquip = null;
    }
}
