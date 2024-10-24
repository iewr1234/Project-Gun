using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OtherStorage : MonoBehaviour
{
    [Header("---Access Script---")]
    public GameMenuManager gameMenuMgr;

    [Header("---Access Component---")]
    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    private GameObject components;
    private TextMeshProUGUI nameText;
    private List<Image> tabButtonImages = new List<Image>();

    [Header("--- Assignment Variable---")]
    public Vector2Int size;
    public List<StorageInfo> storageInfos = new List<StorageInfo>();
    [HideInInspector] public int tabIndex;

    private readonly Color activeColor_tab = new Color(0.78f, 0.78f, 0.78f);
    private readonly Color noneActiveColor_tab = new Color(0.52f, 0.52f, 0.52f);

    public void SetComponents(GameMenuManager _gameMenuMgr)
    {
        gameMenuMgr = _gameMenuMgr;

        components = transform.Find("Components").gameObject;
        nameText = components.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var tabButtons = components.transform.Find("TapButtons").GetComponentsInChildren<Button>();
        for (int i = 0; i < tabButtons.Length; i++)
        {
            var tabButton = tabButtons[i];
            tabButtonImages.Add(tabButton.GetComponent<Image>());
            tabButton.gameObject.SetActive(false);
        }

        itemSlots = GetComponentsInChildren<ItemSlot>().ToList();
        itemSlots.Reverse();
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            var index = new Vector2Int(i % size.x, i / size.x);
            itemSlot.SetComponents(this, index);
        }
    }

    public void ItemSlotsPlacement(Vector2Int size)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            if (i < size.x * size.y)
            {
                itemSlot.gameObject.SetActive(true);
            }
            else if (itemSlot.gameObject.activeSelf)
            {
                itemSlot.gameObject.SetActive(false);
            }
            else
            {
                break;
            }
        }
    }

    public void SetActive(bool value)
    {
        components.SetActive(value);
    }

    public void ActiveTabButtons(int value)
    {
        for (int i = 0; i < value; i++)
        {
            tabButtonImages[i].gameObject.SetActive(true);
            tabButtonImages[i].color = noneActiveColor_tab;
        }
    }

    public void DeactiveTabButtons()
    {
        Debug.Log("!");
        ClearStorage();
        storageInfos.Clear();
        for (int i = 0; i < tabButtonImages.Count; i++)
        {
            tabButtonImages[i].gameObject.SetActive(false);
        }
        tabIndex = 0;
    }

    public void GetStorageInfo(int index)
    {
        tabButtonImages[tabIndex].color = noneActiveColor_tab;
        tabIndex = index;

        var storageInfo = storageInfos[tabIndex];
        nameText.text = storageInfo.storageName;
        tabButtonImages[tabIndex].color = activeColor_tab;
        ItemSlotsPlacement(storageInfo.slotSize);
        for (int i = 0; i < storageInfos[tabIndex].itemList.Count; i++)
        {
            var storageItem = storageInfos[tabIndex].itemList[i];
            var setSlot = gameMenuMgr.FindAllMultiSizeSlots(itemSlots, storageItem.itemSize, storageItem.slotIndex);
            if (setSlot.Count == storageItem.itemSize.x * storageItem.itemSize.y)
            {
                gameMenuMgr.SetItemInStorage(storageItem.itemData, storageItem.totalCount, storageItem.rotation, setSlot);
            }
        }
    }

    public void CheckBaseStorage(ItemHandler item)
    {
        if (item.itemSlots.Count == 0) return;
        if (item.itemSlots[0].otherStorage == null) return;

        var storageInfo = storageInfos[tabIndex];
        var find = storageInfo.itemList.Find(x => x.itemData == item.itemData && x.slotIndex == item.itemSlots[0].slotIndex);
        if (find != null)
        {
            storageInfo.itemList.Remove(find);
        }
    }

    /// <summary>
    /// 바닥에 아이템을 버림
    /// </summary>
    /// <param name="item"></param>
    public void DropItmeOnTheFloor(ItemHandler item)
    {
        var floor = storageInfos[^1];
        var emptySlots = (from y in Enumerable.Range(0, floor.slotSize.y)
                          from x in Enumerable.Range(0, floor.slotSize.x)
                          select new Vector2Int(x, y)).ToList();

        for (int i = 0; i < floor.itemList.Count; i++)
        {
            var itemInfo = floor.itemList[i];
            var fullSlots = from y in Enumerable.Range(0, itemInfo.itemSize.y)
                            from x in Enumerable.Range(0, itemInfo.itemSize.x)
                            select new Vector2Int(itemInfo.slotIndex.x + x, itemInfo.slotIndex.y + y);
            emptySlots = emptySlots.Except(fullSlots).ToList();
        }

        var slotCount = item.size.x * item.size.y;
        for (int i = 0; i < emptySlots.Count; i++)
        {
            var emptySlot = emptySlots[i];
            var itemSlots = from y in Enumerable.Range(0, item.size.y)
                            from x in Enumerable.Range(0, item.size.x)
                            select new Vector2Int(emptySlot.x + x, emptySlot.y + y);
            var setSlots = emptySlots.Intersect(itemSlots).ToList();
            if (setSlots.Count == slotCount)
            {
                var storageItemInfo = new StorageItemInfo()
                {
                    indexName = $"{item.itemData.itemName}_{setSlots[0].x}/{setSlots[0].y}",
                    slotIndex = setSlots[0],
                    itemSize = item.size,
                    totalCount = item.TotalCount,
                    rotation = item.rotation,
                    itemData = item.itemData,
                };
                floor.itemList.Add(storageItemInfo);
                gameMenuMgr.InActiveItem(item);
                return;
            }
        }

        Debug.Log("no EmptySlot int the FloorStorage");
    }

    public void UpdateStorageInfo(ItemHandler item)
    {
        if (item.itemSlots.Count == 0) return;
        if (item.itemSlots[0].otherStorage == null) return;

        var storageInfo = storageInfos[tabIndex];
        var storageItem = storageInfo.itemList.Find(x => x.itemData == item.itemData && x.slotIndex == item.itemSlots[0].slotIndex);
        if (storageItem != null)
        {
            storageItem.totalCount = item.TotalCount;
        }
    }

    public void ClearStorage()
    {
        if (storageInfos.Count == 0) return;

        for (int i = 0; i < storageInfos[tabIndex].itemList.Count; i++)
        {
            var slotIndex = storageInfos[tabIndex].itemList[i].slotIndex;
            var itemSlot = itemSlots.Find(x => x.slotIndex == slotIndex && x.item != null);
            if (itemSlot != null)
            {
                gameMenuMgr.InActiveItem(itemSlot.item);
            }
        }
        //storageInfos.Clear();
    }

    public void Button_Tab(int index)
    {
        ClearStorage();
        GetStorageInfo(index);
    }
}
