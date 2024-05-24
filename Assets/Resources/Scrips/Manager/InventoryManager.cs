using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;
    private List<MyStorage> myStorages = new List<MyStorage>();
    private OtherStorage otherStorage;

    [Header("---Access Component---")]
    public ItemHandler sampleItem;

    private Canvas inventoryUI;
    private Transform myItemTf;
    private Transform otherItemTf;
    private Transform itemPool;

    [Header("--- Assignment Variable---")]
    public ItemSlot onSlot;
    public List<ItemSlot> onSlots;
    public ItemHandler holdingItem;

    private List<ItemHandler> items = new List<ItemHandler>();
    private readonly int itemPoolMax = 100;

    private void Awake()
    {
        var find = FindObjectsOfType<InventoryManager>();
        if (find.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void SetComponents(GameManager _gmaeMgr)
    {
        gameMgr = _gmaeMgr;

        inventoryUI = transform.Find("InventoryUI").GetComponent<Canvas>();
        inventoryUI.worldCamera = gameMgr.camMgr.mainCam;
        inventoryUI.planeDistance = 2f;
        myItemTf = inventoryUI.transform.Find("MyStorage/StorageField");
        otherItemTf = inventoryUI.transform.Find("OtherStorage/StorageField");
        itemPool = inventoryUI.transform.Find("ItemPool");
        sampleItem = inventoryUI.transform.Find("SampleItem").GetComponent<ItemHandler>();
        sampleItem.SetComponents(this, true);
        sampleItem.gameObject.SetActive(false);

        myStorages = inventoryUI.transform.Find("MyStorage/StorageField/StorageItems").GetComponentsInChildren<MyStorage>().ToList();
        for (int i = 0; i < myStorages.Count; i++)
        {
            var storage = myStorages[i];
            storage.SetComponents(this);
        }
        otherStorage = inventoryUI.transform.Find("OtherStorage").GetComponent<OtherStorage>();
        otherStorage.SetComponents(this);

        CreateItems();
        inventoryUI.gameObject.SetActive(false);

        var testItem = items.Find(x => !x.gameObject.activeSelf);
        testItem.SetItemInfo(ItemType.None, new Vector2Int(1, 2), 0);
        var emptySlot = otherStorage.itemSlots.Find(x => x.item == null);
        var emptySlots = FindAllMultiSizeSlots(otherStorage.itemSlots, testItem, emptySlot.slotIndex);
        PutTheItem(testItem, emptySlots);

        testItem = items.Find(x => !x.gameObject.activeSelf);
        testItem.SetItemInfo(ItemType.None, new Vector2Int(1, 1), 0);
        emptySlot = otherStorage.itemSlots.Find(x => x.item == null);
        PutTheItem(testItem, emptySlot);
    }

    private void CreateItems()
    {
        for (int i = 0; i < itemPoolMax; i++)
        {
            var item = Instantiate(Resources.Load<ItemHandler>("Prefabs/Inventory/Item"));
            item.transform.name = $"Item_{i}";
            item.transform.SetParent(itemPool, false);
            item.SetComponents(this, false);
            item.gameObject.SetActive(false);
            items.Add(item);
        }
    }

    private void Update()
    {
        KeyboardInput();
    }

    private void KeyboardInput()
    {
        if (gameMgr != null && Input.GetKeyDown(KeyCode.I))
        {
            ShowInventory();
        }
        else if (holdingItem != null && Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
    }

    public void ShowInventory()
    {
        if (gameMgr.gameState == GameState.Shot || gameMgr.gameState == GameState.Watch) return;

        var value = !inventoryUI.gameObject.activeSelf;
        inventoryUI.gameObject.SetActive(value);
        gameMgr.DeselectCharacter();
        gameMgr.gameState = inventoryUI.gameObject.activeSelf ? GameState.Inventory : GameState.None;
        gameMgr.uiMgr.SetActiveGameUI(!value);
        if (gameMgr.mapEdt != null) gameMgr.mapEdt.gameObject.SetActive(!value);
    }

    private void RotateItem()
    {
        if (holdingItem.size.x == 1 && holdingItem.size.y == 1) return;

        var sample = holdingItem.GetSample();
        Vector3 newRot;
        if (sample.sampleObject.transform.localRotation.x == 0f)
        {
            newRot = sample.sampleObject.transform.localRotation.eulerAngles;
            newRot.x = 90f;
        }
        else
        {
            newRot = sample.sampleObject.transform.localRotation.eulerAngles;
            newRot.x = 0f;
        }
        sample.sampleObject.transform.localRotation = Quaternion.Euler(newRot);
        var newSize = new Vector2Int(holdingItem.size.y, holdingItem.size.x);
        holdingItem.SetItemInfo(holdingItem.type, newSize, sample.index);
        holdingItem.FollowMouse();

        if (onSlot != null)
        {
            if (onSlots.Count > 0)
            {
                for (int i = 0; i < onSlots.Count; i++)
                {
                    var onSlot = onSlots[i];
                    if (onSlot.item != null && onSlot.item != holdingItem)
                    {
                        onSlot.SetSlotColor(DataUtility.slot_onItemColor);
                    }
                    else if (onSlot.item != null && onSlot.item == holdingItem)
                    {
                        onSlot.SetSlotColor(DataUtility.slot_onItemColor);
                    }
                    else
                    {
                        onSlot.SetSlotColor(Color.white);
                    }
                }
                onSlots.Clear();
            }
            onSlot.PointerEnter_ItemSlot();
        }
    }

    public void TakeTheItem(ItemHandler item)
    {
        var sample = item.GetSample();
        var index = sample.index;
        var rot = sample.sampleObject.transform.localRotation.eulerAngles;
        sampleItem.SetItemInfo(item.type, item.size, index, rot);
        sampleItem.transform.position = item.transform.position;
        item.transform.SetParent(itemPool, false);
        holdingItem = item;
    }

    public void PutTheItem(ItemHandler item, ItemSlot itemSlot)
    {
        if (itemSlot == null || itemSlot.item != null)
        {
            //var itemTf = item.itemSlot.myStorage != null ? myItemTf : otherItemTf;
            item.transform.SetParent(item.itemSlot.transform, false);
            item.transform.position = item.itemSlot.transform.position;
            if (itemSlot != null && itemSlot.item != null)
            {
                //itemSlot.item.targetImage.color = DataUtility.slot_onItemColor;
                itemSlot.SetSlotColor(DataUtility.slot_onItemColor);
            }
        }
        else
        {
            if (item.itemSlot != null)
            {
                item.itemSlot.item = null;
                item.itemSlot.SetSlotColor(Color.white);
            }
            itemSlot.item = item;
            itemSlot.SetSlotColor(DataUtility.slot_onItemColor);

            item.itemSlot = itemSlot;
            item.transform.SetParent(itemSlot.transform, false);
            item.transform.localPosition = Vector3.zero;
        }
        holdingItem = null;
        sampleItem.gameObject.SetActive(false);

        item.targetImage.raycastTarget = true;
        item.targetImage.color = Color.clear;
    }

    public void PutTheItem(ItemHandler item, List<ItemSlot> itemSlots)
    {
        var findItem = itemSlots.Find(x => x.item != null && x.item != item);
        if (findItem || itemSlots.Count < item.size.x * item.size.y)
        {
            //var itemTf = item.itemSlots[0].myStorage != null ? myItemTf : otherItemTf;
            item.transform.SetParent(item.itemSlots[0].transform, false);
            item.transform.position = item.itemSlots[0].transform.position;
            item.targetImage.color = DataUtility.slot_onItemColor;
            for (int i = 0; i < itemSlots.Count; i++)
            {
                var itemSlot = itemSlots[i];
                if (itemSlot.item != null)
                {
                    itemSlot.item.targetImage.color = DataUtility.slot_onItemColor;
                }
                else
                {
                    itemSlot.SetSlotColor(Color.white);
                }
            }
        }
        else
        {
            if (item.itemSlots.Count > 0)
            {
                for (int i = 0; i < item.itemSlots.Count; i++)
                {
                    var itemSlot = item.itemSlots[i];
                    itemSlot.item = null;
                    itemSlot.SetSlotColor(Color.white);
                }
                item.itemSlots.Clear();
            }

            for (int i = 0; i < itemSlots.Count; i++)
            {
                var itemSlot = itemSlots[i];
                itemSlot.item = item;
                itemSlot.SetSlotColor(DataUtility.slot_onItemColor);
            }
            item.itemSlots = new List<ItemSlot>(itemSlots);
            item.transform.SetParent(itemSlots[0].transform, false);
            item.transform.localPosition = Vector3.zero;
        }
        holdingItem = null;
        onSlots.Clear();
        sampleItem.gameObject.SetActive(false);

        item.targetImage.raycastTarget = true;
        item.targetImage.color = Color.clear;
    }

    public List<ItemSlot> FindAllMultiSizeSlots(List<ItemSlot> itemSlots, ItemHandler item, Vector2Int startIndex)
    {
        var setSlots = itemSlots.FindAll(x => x.slotIndex.x >= startIndex.x
                                           && x.slotIndex.y >= startIndex.y
                                           && x.slotIndex.x < startIndex.x + item.size.x
                                           && x.slotIndex.y < startIndex.y + item.size.y);

        return setSlots;
    }

    public int GetCanvasDistance()
    {
        if (gameMgr != null)
        {
            return (int)inventoryUI.planeDistance;
        }
        else
        {
            return 0;
        }
    }
}
