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
    [SerializeField] private ItemHandler sampleItem;

    private Canvas inventoryUI;
    private Transform myItemTf;
    private Transform otherItemTf;
    private Transform itemPool;

    [Header("--- Assignment Variable---")]
    public ItemSlot onSlot;
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
        testItem.SetItemInfo(ItemType.None, new Vector2Int(1, 1), 0);
        var emptySlot = otherStorage.itemSlots.Find(x => x.item == null);
        testItem.SetItem(emptySlot);

        testItem = items.Find(x => !x.gameObject.activeSelf);
        testItem.SetItemInfo(ItemType.None, new Vector2Int(1, 1), 0);
        emptySlot = otherStorage.itemSlots.Find(x => x.item == null);
        testItem.SetItem(emptySlot);
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

    public void TakeTheItem(ItemHandler item)
    {
        sampleItem.SetItemInfo(item.type, item.size, item.sampleIndex);
        sampleItem.transform.position = item.transform.position;
        item.transform.SetParent(itemPool, false);
        holdingItem = item;
    }

    public void PutTheItem(ItemHandler item, ItemSlot itemSlot)
    {
        if (itemSlot == null || itemSlot.item != null)
        {
            var itemTf = item.itemSlot.myStorage != null ? myItemTf : otherItemTf;
            item.transform.SetParent(itemTf, false);
            item.transform.position = item.itemSlot.transform.position;
            if (itemSlot.item != null)
            {
                if (itemSlot.item.size == new Vector2Int(1, 1))
                {
                    itemSlot.item.targetImage.color = DataUtility.slot_onItemColor;
                }
                else
                {

                }
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
