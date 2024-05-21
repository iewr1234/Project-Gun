using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None,
    MainWeapon,
    SubWeapon,
    Magazine,
    Rig,
    Backpack,
}

public class ItemHandler : MonoBehaviour
{
    [Header("---Access Component---")]
    public RectTransform rect;

    [Header("--- Assignment Variable---")]
    public ItemType type;
    public Vector2Int size = new Vector2Int(1, 1);

    [HideInInspector] public int poolIndex;

    public void SetComponents(int index)
    {
        rect = GetComponent<RectTransform>();

        ResizeItemRect();
        poolIndex = index;
    }

    private void ResizeItemRect()
    {
        rect.sizeDelta = new Vector2Int(DataUtility.itemSize * size.x, DataUtility.itemSize * size.y);
    }

    public void SetItemInfo(ItemType _type, Vector2Int _size)
    {
        type = _type;
        size = _size;
        switch (type)
        {
            case ItemType.None:
                break;
            case ItemType.MainWeapon:
                break;
            case ItemType.SubWeapon:
                break;
            case ItemType.Magazine:
                break;
            case ItemType.Rig:
                break;
            case ItemType.Backpack:
                break;
            default:
                break;
        }
        gameObject.SetActive(true);
    }
}
