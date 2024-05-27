using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private DataManager dataMgr;
    private GameManager gameMgr;
    private List<MyStorage> myStorages = new List<MyStorage>();
    private OtherStorage otherStorage;

    [Header("---Access Component---")]
    public ItemHandler sampleItem;

    [HideInInspector] public Camera invenCam;
    [HideInInspector] public Camera sampleCam;
    private Canvas invenUI;

    private ScrollRect myScrollRect;
    private GameObject myScrollbar;

    private ScrollRect otherScrollRect;
    private GameObject otherScrollbar;

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
        dataMgr = _gmaeMgr.dataMgr;
        gameMgr = _gmaeMgr;

        invenCam = transform.Find("InventoryCamera").GetComponent<Camera>();
        sampleCam = transform.Find("InventoryCamera/SampleCamera").GetComponent<Camera>();
        invenUI = transform.Find("InventoryUI").GetComponent<Canvas>();
        invenUI.worldCamera = invenCam;

        myScrollRect = invenUI.transform.Find("MyStorage/ScrollView").GetComponent<ScrollRect>();
        myScrollbar = invenUI.transform.Find("MyStorage/ScrollView/Scrollbar Vertical").gameObject;

        otherScrollRect = invenUI.transform.Find("OtherStorage/ScrollView").GetComponent<ScrollRect>();
        otherScrollbar = invenUI.transform.Find("OtherStorage/ScrollView/Scrollbar Vertical").gameObject;

        itemPool = invenUI.transform.Find("ItemPool");
        sampleItem = invenUI.transform.Find("SampleItem").GetComponent<ItemHandler>();
        sampleItem.SetComponents(this);
        sampleItem.gameObject.SetActive(false);

        myStorages = invenUI.transform.Find("MyStorage/ScrollView/Viewport/Content").GetComponentsInChildren<MyStorage>().ToList();
        for (int i = 0; i < myStorages.Count; i++)
        {
            var storage = myStorages[i];
            storage.SetComponents(this);
        }
        otherStorage = invenUI.transform.Find("OtherStorage").GetComponent<OtherStorage>();
        otherStorage.SetComponents(this);

        CreateItems();
        invenUI.gameObject.SetActive(false);

        SetItemInStorage("T0001", 1, otherStorage.itemSlots);
        for (int i = 0; i < 3; i++)
        {
            SetItemInStorage("T0002", 3, otherStorage.itemSlots);
        }
        for (int i = 0; i < 3; i++)
        {
            SetItemInStorage("T0002", 2, otherStorage.itemSlots);
        }
    }

    private void CreateItems()
    {
        for (int i = 0; i < itemPoolMax; i++)
        {
            var item = Instantiate(Resources.Load<ItemHandler>("Prefabs/Inventory/Item"));
            item.transform.name = $"Item_{i}";
            item.transform.SetParent(itemPool, false);
            item.SetComponents(this);
            item.gameObject.SetActive(false);
            items.Add(item);
        }
    }

    private void Update()
    {
        KeyboardInput();
        StorageScrollView();
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

        var value = !invenCam.enabled;
        invenCam.enabled = value;
        sampleCam.enabled = value;
        invenUI.gameObject.SetActive(value);
        gameMgr.DeselectCharacter();
        gameMgr.gameState = invenUI.gameObject.activeSelf ? GameState.Inventory : GameState.None;
        gameMgr.uiMgr.SetActiveGameUI(!value);
        if (gameMgr.mapEdt != null)
        {
            gameMgr.mapEdt.gameObject.SetActive(!value);
        }
    }

    private void RotateItem()
    {
        if (holdingItem.itemData.size.x == 1 && holdingItem.itemData.size.y == 1) return;

        holdingItem.SetItemRotation(!holdingItem.rotation);
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

    private void StorageScrollView()
    {
        if (!invenUI.gameObject.activeSelf) return;

        myScrollRect.vertical = myScrollbar.activeSelf;
        otherScrollRect.vertical = otherScrollbar.activeSelf;
    }

    public void TakeTheItem(ItemHandler item)
    {
        sampleItem.SetSampleItemInfo(item.itemData, item.rotation);
        sampleItem.transform.position = item.transform.position;
        item.transform.SetParent(itemPool, false);
        holdingItem = item;
    }

    public void SetItemInStorage(string itemID, int count, List<ItemSlot> itemSlots)
    {
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.ID == itemID);
        if (itemData == null)
        {
            Debug.Log("Not found item");
            return;
        }

        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count);
        var emptySlot = itemSlots.Find(x => x.item == null);
        if (item.size == new Vector2Int(1, 1))
        {
            PutTheItem(item, emptySlot);
        }
        else
        {
            var emptySlots = FindAllMultiSizeSlots(itemSlots, item, emptySlot.slotIndex);
            PutTheItem(item, emptySlots);
        }
    }

    public void PutTheItem(ItemHandler item, ItemSlot itemSlot)
    {
        if (itemSlot == null)
        {
            ItemMove(false);
        }
        else if (itemSlot.item != null && itemSlot.item != item && itemSlot.item.itemData == item.itemData)
        {
            if (item.itemData.maxNesting == 1)
            {
                ItemMove(false);
            }
            else
            {
                ItemNesting();
            }
        }
        else
        {
            ItemMove(true);
        }
        //holdingItem = null;
        //sampleItem.gameObject.SetActive(false);

        //item.targetImage.raycastTarget = true;
        //item.targetImage.color = Color.clear;

        void ItemMove(bool value)
        {
            switch (value)
            {
                case true:
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
                    break;
                case false:
                    item.transform.SetParent(item.itemSlot.transform, false);
                    item.transform.position = item.itemSlot.transform.position;
                    if (itemSlot != null && itemSlot.item != null)
                    {
                        itemSlot.SetSlotColor(DataUtility.slot_onItemColor);
                    }
                    break;
            }
            holdingItem = null;
            sampleItem.gameObject.SetActive(false);

            item.targetImage.raycastTarget = true;
            item.targetImage.color = Color.clear;
        }

        void ItemNesting()
        {
            if (itemSlot.item.totalCount == itemSlot.item.itemData.maxNesting)
            {
                ItemMove(false);
            }
            else if (itemSlot.item.totalCount < itemSlot.item.itemData.maxNesting)
            {

                itemSlot.SetSlotColor(DataUtility.slot_onItemColor);
                var slotCount = itemSlot.item.totalCount;
                var itemCount = item.totalCount;
                if (slotCount + itemCount > itemSlot.item.itemData.maxNesting)
                {
                    if (item.itemSlot != null)
                    {
                        item.itemSlot.SetSlotColor(DataUtility.slot_onItemColor);
                    }
                    var count = slotCount + itemCount - itemSlot.item.itemData.maxNesting;
                    item.totalCount = count;
                    item.transform.SetParent(item.itemSlot.transform, false);
                    item.transform.position = item.itemSlot.transform.position;
                    item.countText.text = $"{item.totalCount}";
                    itemSlot.item.totalCount = itemSlot.item.itemData.maxNesting;
                }
                else
                {
                    if (item.itemSlot != null)
                    {
                        item.itemSlot.item = null;
                        item.itemSlot.SetSlotColor(Color.white);
                    }
                    item.gameObject.SetActive(false);
                    itemSlot.item.totalCount += itemCount;
                }
                item.targetImage.color = Color.clear;
                holdingItem = null;
                sampleItem.gameObject.SetActive(false);

                itemSlot.item.targetImage.raycastTarget = true;
                itemSlot.item.countText.text = $"{itemSlot.item.totalCount}";
            }
        }
    }

    public void PutTheItem(ItemHandler item, List<ItemSlot> itemSlots)
    {
        var findItem = itemSlots.Find(x => x.item != null && x.item != item);
        if (findItem || itemSlots.Count < item.size.x * item.size.y)
        {
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
            return (int)invenUI.planeDistance;
        }
        else
        {
            return 0;
        }
    }
}
