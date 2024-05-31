using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    [HideInInspector] public DataManager dataMgr;
    [HideInInspector] public PopUp_Inventory popUp;
    [HideInInspector] public ContextMenu contextMenu;

    [Space(5f)]
    [SerializeField] private List<EquipSlot> charEquips = new List<EquipSlot>();
    [SerializeField] private List<MyStorage> myStorages = new List<MyStorage>();
    [SerializeField] private OtherStorage otherStorage;

    [Header("---Access Component---")]
    public ItemHandler sampleItem;

    [HideInInspector] public Camera invenCam;
    [HideInInspector] public Camera subCam;
    private Canvas invenUI;

    private ScrollRect myScrollRect;
    private GameObject myScrollbar;

    private ScrollRect otherScrollRect;
    private GameObject otherScrollbar;

    private Transform itemPool;

    [Header("--- Assignment Variable---")]
    public EquipSlot onEquip;
    public ItemSlot onSlot;
    public List<ItemSlot> onSlots;
    public ItemHandler holdingItem;
    public ItemHandler selectItem;

    private List<ItemHandler> items = new List<ItemHandler>();
    private readonly int itemPoolMax = 100;

    [SerializeField] private bool itemSplit;

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
        dataMgr = _gmaeMgr.dataMgr;
        popUp = transform.Find("InventoryUI/PopUp").GetComponent<PopUp_Inventory>();
        popUp.SetComponents(this);
        contextMenu = transform.Find("InventoryUI/ContextMenu").GetComponent<ContextMenu>();
        contextMenu.SetComponents(this);

        invenCam = transform.Find("InventoryCamera").GetComponent<Camera>();
        subCam = transform.Find("InventoryCamera/SubCamera").GetComponent<Camera>();
        invenUI = transform.Find("InventoryUI").GetComponent<Canvas>();
        invenUI.worldCamera = invenCam;

        myScrollRect = invenUI.transform.Find("MyStorage/ScrollView").GetComponent<ScrollRect>();
        myScrollbar = invenUI.transform.Find("MyStorage/ScrollView/Scrollbar Vertical").gameObject;

        otherScrollRect = invenUI.transform.Find("OtherStorage/ScrollView").GetComponent<ScrollRect>();
        otherScrollbar = invenUI.transform.Find("OtherStorage/ScrollView/Scrollbar Vertical").gameObject;

        itemPool = invenUI.transform.Find("ItemPool");
        sampleItem = itemPool.transform.Find("SampleItem").GetComponent<ItemHandler>();
        sampleItem.SetComponents(this);
        InactiveSampleItem();

        charEquips = invenUI.transform.Find("Equip/Slots").GetComponentsInChildren<EquipSlot>().ToList();
        for (int i = 0; i < charEquips.Count; i++)
        {
            var charEquip = charEquips[i];
            charEquip.SetComponents(this);
        }

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
        SetItemInStorage("T0002", 1, otherStorage.itemSlots);
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
            itemSplit = false;
            ShowInventory();
        }

        if (!invenUI.gameObject.activeSelf) return;

        if (holdingItem != null && Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            itemSplit = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            itemSplit = false;
        }
    }

    public void ShowInventory()
    {
        if (gameMgr.gameState == GameState.Shot || gameMgr.gameState == GameState.Watch) return;

        var value = !invenCam.enabled;
        invenCam.enabled = value;
        subCam.enabled = value;
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
        if (item.equipSlot == null)
        {
            ActiveSampleItem(item);
        }
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

    public void SetItemInStorage(ItemDataInfo itemData, int count, ItemSlot itemSlot)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count);
        PutTheItem(item, itemSlot);
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
        else if (itemSlot != null && itemSlot.item == null)
        {
            ItemMove(true);
        }
        else
        {
            ItemMove(false);
        }

        void ItemMove(bool value)
        {
            switch (value)
            {
                case true:
                    if (itemSlot.item == null && item.TotalCount > 1 && itemSplit)
                    {
                        ItemSplit();
                        itemSplit = false;
                        return;
                    }
                    else
                    {
                        if (item.itemSlots.Count > 0)
                        {
                            item.itemSlots[0].item = null;
                            item.itemSlots[0].SetSlotColor(Color.white);
                            item.itemSlots.Clear();
                        }
                        itemSlot.item = item;
                        itemSlot.SetSlotColor(DataUtility.slot_onItemColor);

                        item.itemSlots.Add(itemSlot);
                        item.transform.SetParent(itemSlot.transform, false);
                        item.transform.localPosition = Vector3.zero;
                    }
                    break;
                case false:
                    item.transform.SetParent(item.itemSlots[0].transform, false);
                    item.transform.position = item.itemSlots[0].transform.position;
                    if (itemSlot != null && itemSlot.item != null)
                    {
                        itemSlot.SetSlotColor(DataUtility.slot_onItemColor);
                    }
                    break;
            }

            if (holdingItem != null)
            {
                item.targetImage.raycastTarget = true;
            }
            item.targetImage.color = Color.clear;
            holdingItem = null;
            InactiveSampleItem();
        }

        void ItemNesting()
        {
            if (itemSlot.item.TotalCount == itemSlot.item.itemData.maxNesting)
            {
                ItemMove(false);
            }
            else if (itemSlot.item.TotalCount < itemSlot.item.itemData.maxNesting)
            {
                itemSlot.SetSlotColor(DataUtility.slot_onItemColor);
                var slotCount = itemSlot.item.TotalCount;
                var itemCount = item.TotalCount;
                if (slotCount + itemCount > itemSlot.item.itemData.maxNesting)
                {
                    if (item.itemSlots.Count > 0)
                    {
                        item.itemSlots[0].SetSlotColor(DataUtility.slot_onItemColor);
                    }
                    var count = slotCount + itemCount - itemSlot.item.itemData.maxNesting;
                    item.transform.SetParent(item.itemSlots[0].transform, false);
                    item.transform.position = item.itemSlots[0].transform.position;
                    item.SetTotalCount(count);
                    itemSlot.item.SetTotalCount(itemSlot.item.itemData.maxNesting);
                }
                else
                {
                    if (item.itemSlots.Count > 0)
                    {
                        item.itemSlots[0].item = null;
                        item.itemSlots[0].SetSlotColor(Color.white);
                        item.itemSlots.Clear();
                    }
                    item.gameObject.SetActive(false);
                    itemSlot.item.ResultTotalCount(itemCount);
                }
                item.targetImage.color = Color.clear;
                holdingItem = null;
                InactiveSampleItem();

                itemSlot.item.targetImage.raycastTarget = true;
            }
        }

        void ItemSplit()
        {
            item.transform.SetParent(item.itemSlots[0].transform, false);
            item.transform.position = item.itemSlots[0].transform.position;
            itemSlot.SetSlotColor(DataUtility.slot_onItemColor);

            popUp.PopUp_Split(item, itemSlot);

            holdingItem = null;
            sampleItem.transform.position = itemSlot.transform.position;

            item.targetImage.color = Color.clear;
        }
    }

    public void PutTheItem(ItemHandler item, List<ItemSlot> itemSlots)
    {
        var findItem = itemSlots.Find(x => x.item != null && x.item != item);
        if (findItem || itemSlots.Count < item.size.x * item.size.y)
        {
            item.SetItemRotation(sampleItem.rotation);
            if (item.equipSlot != null)
            {

            }
            else
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

        item.targetImage.color = Color.clear;
        if (holdingItem != null)
        {
            item.targetImage.raycastTarget = true;
        }
        holdingItem = null;
        onSlots.Clear();
        InactiveSampleItem();
    }

    public List<ItemSlot> FindAllMultiSizeSlots(List<ItemSlot> itemSlots, ItemHandler item, Vector2Int startIndex)
    {
        var setSlots = itemSlots.FindAll(x => x.slotIndex.x >= startIndex.x
                                           && x.slotIndex.y >= startIndex.y
                                           && x.slotIndex.x < startIndex.x + item.size.x
                                           && x.slotIndex.y < startIndex.y + item.size.y);

        return setSlots;
    }

    public void ActiveSampleItem(ItemHandler item)
    {
        sampleItem.transform.SetParent(item.transform.parent, false);
        sampleItem.transform.localPosition = Vector3.zero;
        sampleItem.SetSampleItemInfo(item.itemData, item.rotation);
    }

    public void InactiveSampleItem()
    {
        sampleItem.transform.SetParent(itemPool, false);
        sampleItem.transform.SetAsFirstSibling();
        sampleItem.gameObject.SetActive(false);
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

    public void OpenContextMenu(ItemHandler item)
    {
        selectItem = item;
        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, contextMenu.transform.position.z);
        var worldPos = invenCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, GetCanvasDistance()));
        contextMenu.transform.position = worldPos;
        contextMenu.gameObject.SetActive(true);
    }
}
