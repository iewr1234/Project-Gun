using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum MyStorageType
{
    None,
    Rig,
    Backpack,
}

public class MyStorage : MonoBehaviour
{
    [Header("---Access Script---")]
    public InventoryManager invenMgr;

    [Header("---Access Component---")]
    private RectTransform rect;
    private RectTransform itemsRect;
    private GridLayoutGroup gridLayout;
    [Space(5f)]

    public EquipSlot equipSlot;
    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    [Header("--- Assignment Variable---")]
    public MyStorageType type;
    public Vector2Int size;
    private int reSizeNum;

    private readonly Vector2Int minRectSize_my = new Vector2Int(700, 150);

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

        rect = GetComponent<RectTransform>();
        rect.sizeDelta = minRectSize_my;
        itemsRect = transform.Find("ItemSlots").GetComponent<RectTransform>();
        gridLayout = GetComponentInChildren<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

        equipSlot = GetComponentInChildren<EquipSlot>();
        itemSlots = itemsRect.GetComponentsInChildren<ItemSlot>().ToList();

        SetStorageSize();
        itemSlots.Reverse();
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            var index = new Vector2Int(i % size.x, i / size.x);
            itemSlot.SetComponents(this, index);
            if (size.y == 0 || i >= size.x * size.y)
            {
                itemSlot.gameObject.SetActive(false);
            }
        }
    }

    private void SetStorageSize()
    {
        int xCount;
        int yCount;
        switch (type)
        {
            case MyStorageType.Rig:
                if (equipSlot.item == null)
                {
                    xCount = 3;
                    yCount = 2;
                    gridLayout.constraintCount = xCount;
                    size = new Vector2Int(xCount, yCount);
                    reSizeNum = xCount * yCount;
                }
                else
                {
                    gridLayout.constraintCount = 6;
                }
                break;
            case MyStorageType.Backpack:
                if (equipSlot.item == null)
                {
                    xCount = 6;
                    yCount = 2;
                    gridLayout.constraintCount = xCount;
                    size = new Vector2Int(xCount, yCount);
                    reSizeNum = xCount * yCount;
                }
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        ResizeStorage();
    }


    private void ResizeStorage()
    {
        if (size.x * size.y <= reSizeNum) return;

        var addSize = new Vector2(0, 8);
        if (rect.sizeDelta == itemsRect.sizeDelta + addSize) return;

        rect.sizeDelta = itemsRect.sizeDelta + addSize;
    }
}
