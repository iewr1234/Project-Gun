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

    public GameObject components;
    private TextMeshProUGUI nameText;
    private List<Image> tabButtonImages = new List<Image>();

    [Header("--- Assignment Variable---")]
    public Vector2Int size;
    public List<StorageInfo> storageInfos = new List<StorageInfo>();
    public int tabIndex;

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
        ClearStorage();
        var floor = storageInfos.Find(x => x.type == StorageInfo.StorageType.Floor);
        if (floor != null && floor.itemList.Count == 0)
        {
            var itemNode = gameMenuMgr.gameMgr.nodeList.Find(x => x.nodePos == floor.nodePos);
            itemNode.SetItemCase(false);
            gameMenuMgr.dataMgr.gameData.floorStorages.Remove(floor);
        }
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
        ClearStorage();
        ItemSlotsPlacement(storageInfo.slotSize);
        for (int i = 0; i < storageInfos[tabIndex].itemList.Count; i++)
        {
            var storageItem = storageInfos[tabIndex].itemList[i];
            var setSlots = gameMenuMgr.FindAllMultiSizeSlots(itemSlots, storageItem.itemSize, storageItem.slotIndex);
            //if (setSlots[0].item != null && setSlots[0].item.itemData.serialID == storageItem.itemData.serialID) continue;

            if (setSlots.Count == storageItem.itemSize.x * storageItem.itemSize.y)
            {
                gameMenuMgr.SetItemInStorage(storageItem, setSlots);
            }
        }
    }

    public void CheckBaseStorage(ItemHandler item)
    {
        if (item.itemSlots.Count == 0) return;
        if (item.itemSlots[0].otherStorage == null) return;

        var storageInfo = storageInfos[tabIndex];
        var find = storageInfo.itemList.Find(x => x.itemData.ID == item.itemData.ID && x.slotIndex == item.itemSlots[0].slotIndex);
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
        if (gameMenuMgr.gameMgr == null) return;
        if (gameMenuMgr.gameMgr.playerList.Count == 0) return;

        var player = gameMenuMgr.gameMgr.playerList[0];
        SetFloorStorage(player.currentNode);
        var floor = storageInfos[^1];
        InputDropItemInStorage(floor, item);
    }

    public void PutInItemOfStorage(StorageInfo storage, ItemHandler item)
    {
        InputDropItemInStorage(storage, item);
    }

    public void DropItmeOnTheReword(ItemHandler item)
    {
        var reword = storageInfos[^1];
        InputDropItemInStorage(reword, item);
    }

    private void InputDropItemInStorage(StorageInfo storage, ItemHandler item)
    {
        var sameItmes = storage.itemList.FindAll(x => x.itemData.ID == item.itemData.ID && x.itemData.maxNesting > 1
                                                   && x.totalCount < x.itemData.maxNesting);
        for (int i = 0; i < sameItmes.Count; i++)
        {
            var sameItem = sameItmes[i];
            if (sameItem.itemData.maxNesting < sameItem.totalCount + item.TotalCount)
            {
                sameItem.totalCount = sameItem.itemData.maxNesting;
                var result = sameItem.itemData.maxNesting - sameItem.totalCount;
                item.ResultTotalCount(-result);
            }
            else
            {
                sameItem.totalCount += item.TotalCount;
                item.DisableItem();
                return;
            }
        }

        var emptySlots = (from y in Enumerable.Range(0, storage.slotSize.y)
                          from x in Enumerable.Range(0, storage.slotSize.x)
                          select new Vector2Int(x, y)).ToList();
        for (int i = 0; i < storage.itemList.Count; i++)
        {
            var itemInfo = storage.itemList[i];
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
                AddItemInStorageInfo(storage, setSlots[0], item);
                item.DisableItem();
                return;
            }
        }

        Debug.Log("no EmptySlot int the FloorStorage");
    }

    public void AddItemInStorageInfo(StorageInfo storageInfo, Vector2Int slotIndex, ItemHandler item)
    {
        var storageItemInfo = new StorageItemInfo()
        {
            indexName = $"{item.itemData.itemName}_{slotIndex.x}/{slotIndex.y}",
            slotIndex = slotIndex,
            itemSize = item.size,
            totalCount = item.TotalCount,
            rotation = item.rotation,
            itemData = item.itemData,
        };

        switch (item.itemData.type)
        {
            case ItemType.Rig:
                storageItemInfo.rigData = item.rigData;
                break;
            case ItemType.Backpack:
                storageItemInfo.backpackData = item.backpackData;
                break;
            case ItemType.MainWeapon:
                storageItemInfo.weaponData = item.weaponData;
                break;
            case ItemType.SubWeapon:
                storageItemInfo.weaponData = item.weaponData;
                break;
            case ItemType.Bullet:
                storageItemInfo.bulletData = item.bulletData;
                break;
            case ItemType.Magazine:
                storageItemInfo.magData = item.magData;
                break;
            case ItemType.Muzzle:
                storageItemInfo.partsData = item.partsData;
                break;
            case ItemType.Sight:
                storageItemInfo.partsData = item.partsData;
                break;
            case ItemType.Attachment:
                storageItemInfo.partsData = item.partsData;
                break;
            case ItemType.UnderBarrel:
                storageItemInfo.partsData = item.partsData;
                break;
            case ItemType.Grenade:
                storageItemInfo.grenadeData = item.grenadeData;
                break;
            default:
                break;
        }
        storageInfo.itemList.Add(storageItemInfo);
    }

    public void UpdateStorageInfo(ItemHandler item)
    {
        if (item.itemSlots.Count == 0) return;
        if (item.itemSlots[0].otherStorage == null) return;

        var storageInfo = storageInfos[tabIndex];
        var storageItem = storageInfo.itemList.Find(x => x.itemData.ID == item.itemData.ID && x.slotIndex == item.itemSlots[0].slotIndex);
        if (storageItem != null)
        {
            storageItem.totalCount = item.TotalCount;
        }
    }

    public void SetFloorStorage(FieldNode node)
    {
        var gameData = gameMenuMgr.dataMgr.gameData;
        var floor = gameData.floorStorages.Find(x => x.nodePos == node.nodePos);
        if (floor == null)
        {
            var floorStorage = new StorageInfo()
            {
                storageName = $"지면({node.nodePos.x}, {node.nodePos.y})",
                type = StorageInfo.StorageType.Floor,
                nodePos = node.nodePos,
                slotSize = DataUtility.floorSlotSize,
            };
            gameData.floorStorages.Add(floorStorage);
            storageInfos.Add(floorStorage);
            node.SetItemCase(true);
        }
        else if (!storageInfos.Contains(floor))
        {
            storageInfos.Add(floor);
        }
    }

    public void ClearStorage()
    {
        //if (storageInfos.Count == 0) return;

        //for (int i = 0; i < storageInfos[tabIndex].itemList.Count; i++)
        //{
        //    var slotIndex = storageInfos[tabIndex].itemList[i].slotIndex;
        //    var itemSlot = itemSlots.Find(x => x.slotIndex == slotIndex && x.item != null);
        //    if (itemSlot != null)
        //    {
        //        var itemSlots = itemSlot.item.itemSlots;
        //        itemSlot.item.DisableItem();
        //        for (int j = 0; j < itemSlots.Count; j++)
        //        {
        //            var _itemSlot = itemSlot.item.itemSlots[j];
        //            _itemSlot.item = null;
        //        }
        //    }
        //}

        var onItemSlots = itemSlots.FindAll(x => x.item != null);
        for (int i = 0; i < onItemSlots.Count; i++)
        {
            var itemSlot = onItemSlots[i];
            if (itemSlot.item == null) continue;

            if (itemSlot.item.gameObject.activeSelf) itemSlot.item.DisableItem();
            itemSlot.item = null;
        }
    }

    public void Button_Tab(int index)
    {
        if (index == tabIndex) return;

        for (int i = gameMenuMgr.activePopUp.Count - 1; i >= 0; i--)
        {
            PopUp_Inventory popUp = gameMenuMgr.activePopUp[i];
            popUp.ClosePopUp();
        }
        GetStorageInfo(index);
    }
}
