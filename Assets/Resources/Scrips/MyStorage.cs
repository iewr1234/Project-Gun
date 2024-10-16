using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum MyStorageType
{
    None,
    Pocket,
    Backpack,
    Rig,
}

public class MyStorage : MonoBehaviour
{
    [Header("---Access Script---")]
    public GameMenuManager gameMenuMgr;

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
    //private int reSizeNum;

    private readonly Vector2Int minRectSize_my = new Vector2Int(700, 170);
    private readonly Vector2Int expandRectSize_my = new Vector2Int(700, 230);
    private readonly int expandY = 70;

    public void SetComponents(GameMenuManager _gameMenuMgr)
    {
        gameMenuMgr = _gameMenuMgr;

        rect = GetComponent<RectTransform>();
        rect.sizeDelta = minRectSize_my;
        itemsRect = transform.Find("ItemSlots").GetComponent<RectTransform>();
        gridLayout = GetComponentInChildren<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

        equipSlot = GetComponentInChildren<EquipSlot>();
        if (equipSlot != null)
        {
            equipSlot.SetComponents(gameMenuMgr, this);
            switch (type)
            {
                case MyStorageType.Backpack:
                    equipSlot.type = EquipType.Backpack;
                    break;
                case MyStorageType.Rig:
                    equipSlot.type = EquipType.Rig;
                    break;
                default:
                    break;
            }
        }
        itemSlots = itemsRect.GetComponentsInChildren<ItemSlot>().ToList();

        itemSlots.Reverse();
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            //var index = size.x == 0 ? Vector2Int.zero : new Vector2Int(i % size.x, i / size.x);
            //var index = new Vector2Int(i % 6, i / 6);
            itemSlot.SetComponents(this);
            //if (size.y == 0 || i >= size.x * size.y)
            //{
            //    itemSlot.gameObject.SetActive(false);
            //}
        }

        switch (type)
        {
            case MyStorageType.Pocket:
                SetStorageSize(new Vector2Int(4, 1));
                break;
            case MyStorageType.Backpack:
                SetStorageSize(Vector2Int.zero);
                break;
            case MyStorageType.Rig:
                SetStorageSize(Vector2Int.zero);
                break;
            default:
                break;
        }
    }

    public void SetStorageSize(Vector2Int newSize)
    {
        var activeSlots = itemSlots.FindAll(x => x.gameObject.activeSelf);
        for (int i = 0; i < activeSlots.Count; i++)
        {
            var itemSlot = activeSlots[i];
            itemSlot.gameObject.SetActive(false);
        }

        size = newSize;
        gridLayout.constraintCount = size.x;
        if (size.y > 2)
        {
            rect.sizeDelta = expandRectSize_my + new Vector2Int(0, expandY * (size.y - 3));
        }
        else
        {
            rect.sizeDelta = minRectSize_my;
        }
        if (size.x == 0 && size.y == 0) return;

        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            if (i < size.x * size.y)
            {
                var index = new Vector2Int(i % size.x, i / size.x);
                itemSlot.SetSlotIndex(index);
                itemSlot.gameObject.SetActive(true);
            }
        }
    }

    //private void Update()
    //{
    //    ResizeStorage();
    //}


    //private void ResizeStorage()
    //{
    //    if (size.x * size.y <= reSizeNum) return;

    //    var addSize = new Vector2(0, 8);
    //    if (rect.sizeDelta == itemsRect.sizeDelta + addSize) return;

    //    rect.sizeDelta = itemsRect.sizeDelta + addSize;
    //}
}
