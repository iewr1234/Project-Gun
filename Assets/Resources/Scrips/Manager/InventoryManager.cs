using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class InventoryManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;
    private List<MyStorage> myStorages = new List<MyStorage>();
    private OtherStorage otherStorage;

    [Header("---Access Component---")]
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
        myStorages = transform.Find("InventoryUI/MyStorage/StorageField/StorageItems").GetComponentsInChildren<MyStorage>().ToList();
        for (int i = 0; i < myStorages.Count; i++)
        {
            var storage = myStorages[i];
            storage.SetComponents(this);
        }
        otherStorage = transform.Find("InventoryUI/OtherStorage").GetComponent<OtherStorage>();
        otherStorage.SetComponents(this);

        inventoryUI = transform.Find("InventoryUI").GetComponent<Canvas>();
        myItemTf = inventoryUI.transform.Find("MyStorage/StorageField");
        otherItemTf = inventoryUI.transform.Find("OtherStorage/StorageField");
        itemPool = inventoryUI.transform.Find("ItemPool");

        CreateItems();
        inventoryUI.gameObject.SetActive(false);

        var testItem = items.Find(x => !x.gameObject.activeSelf);
        testItem.SetItemInfo(ItemType.None, new Vector2Int(1, 1));
        var emptySlot = otherStorage.itemSlots.Find(x => x.item == null);
        PutTheItem(testItem, emptySlot);
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
        if (gameMgr.gameState != GameState.None && gameMgr.gameState != GameState.Move) return;

        inventoryUI.gameObject.SetActive(!inventoryUI.gameObject.activeSelf);
        gameMgr.DeselectCharacter();
        gameMgr.gameState = inventoryUI.gameObject.activeSelf ? GameState.Inventory : GameState.None;
    }

    public void TakeTheItem(ItemHandler item)
    {
        item.transform.SetParent(itemPool, false);
    }

    public void PutTheItem(ItemHandler item, ItemSlot itemSlot)
    {
        if (itemSlot == null)
        {
            var itemTf = item.itemSlot.myStorage != null ? myItemTf : otherItemTf;
            item.transform.SetParent(itemTf, false);
            item.transform.position = item.itemSlot.transform.position;
        }
        else
        {
            item.itemSlot = itemSlot;
            itemSlot.item = item;
            var itemTf = itemSlot.myStorage != null ? myItemTf : otherItemTf;
            item.transform.SetParent(itemTf, false);
            item.transform.position = itemSlot.transform.position;
        }
        holdingItem = null;
    }
}
