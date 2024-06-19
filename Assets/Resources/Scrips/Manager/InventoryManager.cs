using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    [HideInInspector] public DataManager dataMgr;
    [HideInInspector] public PopUp_Inventory popUp;
    [HideInInspector] public ContextMenu contextMenu;

    [Space(5f)]
    public List<EquipSlot> allEquips = new List<EquipSlot>();
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

    private bool click;
    private float clickTime;

    private List<ItemHandler> items = new List<ItemHandler>();
    private readonly int itemPoolMax = 100;

    [Space(5f)]
    public List<ItemHandler> activeItem = new List<ItemHandler>();

    private bool itemSplit;

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

        var charEquips = invenUI.transform.Find("Equip/Slots").GetComponentsInChildren<EquipSlot>().ToList();
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
        SetItemInStorage("T0001", 1, otherStorage.itemSlots);

        SetItemInStorage("T0002", 1, otherStorage.itemSlots);

        SetItemInStorage("T0003", 1, otherStorage.itemSlots);
        SetItemInStorage("T0003", 1, otherStorage.itemSlots);
        SetItemInStorage("T0003", 1, otherStorage.itemSlots);
        SetItemInStorage("T0003", 1, otherStorage.itemSlots);

        SetItemInStorage("T0004", 100, otherStorage.itemSlots);
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
        MouseInput();
        StorageScrollView();
    }

    private void KeyboardInput()
    {
        if (gameMgr != null && Input.GetKeyDown(KeyCode.I))
        {
            itemSplit = false;
            gameMgr.uiMgr.bottomUI.SetActive(false);
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
        if (gameMgr.gameState == GameState.Shoot || gameMgr.gameState == GameState.Watch) return;

        var value = !invenCam.enabled;
        invenCam.enabled = value;
        subCam.enabled = value;
        invenUI.gameObject.SetActive(value);
        gameMgr.DeselectCharacter();
        gameMgr.gameState = invenUI.gameObject.activeSelf ? GameState.Inventory : GameState.None;
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
                        onSlot.SetItemSlot(DataUtility.slot_onItemColor);
                    }
                    else if (onSlot.item != null && onSlot.item == holdingItem)
                    {
                        onSlot.SetItemSlot(DataUtility.slot_onItemColor);
                    }
                    else
                    {
                        onSlot.SetItemSlot(DataUtility.slot_noItemColor);
                    }
                }
                onSlots.Clear();
            }
            onSlot.PointerEnter_ItemSlot();
        }
    }

    private void MouseInput()
    {
        if (!invenUI.gameObject.activeSelf) return;

        DoubleClick();
        if (Input.GetMouseButtonDown(0))
        {
            if (!click)
            {
                click = true;
                clickTime = 0f;
            }
            else if (onEquip != null && onEquip.item != null)
            {
                selectItem = onEquip.item;
                popUp.PopUp_ItemInformation();
            }
            else if (onSlot != null && onSlot.item != null)
            {
                selectItem = onSlot.item;
                popUp.PopUp_ItemInformation();
            }
        }

        void DoubleClick()
        {
            if (!click) return;

            clickTime += Time.deltaTime;
            if (clickTime > 0.2f)
            {
                click = false;
                clickTime = 0f;
            }
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
        item.SetItemScale(false);
        item.transform.SetParent(itemPool, false);
        holdingItem = item;

        var findEquips = allEquips.FindAll(x => x.CheckEquip(item));
        for (int i = 0; i < findEquips.Count; i++)
        {
            var equipSlot = findEquips[i];
            equipSlot.outline.enabled = true;
        }

        var items = activeItem.FindAll(x => x.equipSlot == null && CheckEquip(x, item));
        for (int i = 0; i < items.Count; i++)
        {
            var _item = items[i];
            _item.frameImage.enabled = true;
        }
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
        var emptySlots = FindAllMultiSizeSlots(itemSlots, item, emptySlot.slotIndex);
        PutTheItem(item, emptySlots);
    }

    public void SetItemInStorage(ItemDataInfo itemData, int count, List<ItemSlot> itemSlots)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count);
        PutTheItem(item, itemSlots);
    }

    public void SetItemInEquipSlot(BulletDataInfo bulletData, int count, EquipSlot equipSlot)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.dataID == bulletData.ID);
        item.SetItemInfo(itemData, count);

        equipSlot.item = item;
        equipSlot.slotText.enabled = false;
        equipSlot.countText.enabled = false;

        item.countText.enabled = false;
        item.equipSlot = equipSlot;
        item.SetItemScale(true);
        item.ChangeRectPivot(true);
        item.transform.SetParent(equipSlot.transform, false);
        item.transform.localPosition = Vector3.zero;
    }

    public void SetItemInEquipSlot(MagazineDataInfo magData, int count, EquipSlot equipSlot)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.dataID == magData.ID);
        item.SetItemInfo(itemData, count);
        item.magData.loadedBullets = magData.loadedBullets;

        equipSlot.item = item;
        equipSlot.slotText.enabled = false;
        equipSlot.countText.enabled = true;

        item.countText.enabled = false;
        item.equipSlot = equipSlot;
        item.SetItemScale(true);
        item.ChangeRectPivot(true);
        item.transform.SetParent(equipSlot.transform, false);
        item.transform.localPosition = Vector3.zero;
    }

    public void SetItemInEquipSlot(WeaponPartsDataInfo partsData, int count, EquipSlot equipSlot)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.dataID == partsData.ID);
        item.SetItemInfo(itemData, count);

        equipSlot.item = item;
        equipSlot.slotText.enabled = false;
        equipSlot.countText.enabled = false;

        item.equipSlot = equipSlot;
        item.SetItemScale(true);
        item.ChangeRectPivot(true);
        item.transform.SetParent(equipSlot.transform, false);
        item.transform.localPosition = Vector3.zero;
    }

    public bool CheckEquip(ItemHandler onItem, ItemHandler putItem)
    {
        if (onItem == putItem) return false;

        switch (putItem.itemData.type)
        {
            case ItemType.Bullet:
                if (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                {
                    return !onItem.weaponData.isChamber && onItem.weaponData.caliber == putItem.bulletData.caliber;
                }
                else if (onItem.itemData.type == ItemType.Magazine)
                {
                    return onItem.magData.loadedBullets.Count < onItem.magData.magSize && onItem.magData.compatCaliber == putItem.bulletData.caliber;
                }
                else
                {
                    return false;
                }
            case ItemType.Magazine:
                return (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                    && !onItem.weaponData.isMag && putItem.magData.compatModel.Contains(onItem.weaponData.model);
            case ItemType.Sight:
                return (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                    && onItem.weaponData.equipPartsList.Find(x => x.type == WeaponPartsType.Sight) == null
                    && putItem.partsData.compatModel.Contains(onItem.weaponData.model);
            default:
                return false;
        }
    }

    public void PutTheItem(ItemHandler item, List<ItemSlot> itemSlots)
    {
        if (itemSplit && itemSlots.Find(x => x.item != null) == null
         && item.itemData.type != ItemType.Magazine && item.TotalCount > 1)
        {
            ItemSplit();
        }
        else
        {
            if (onSlot != null && onSlot.item != null)
            {
                var itemNesting = onSlot.item != item && onSlot.item.itemData.ID == item.itemData.ID
                               && onSlot.item.itemData.maxNesting > 1 && onSlot.item.TotalCount < onSlot.item.itemData.maxNesting;
                if (itemNesting)
                {
                    ItemNesting();
                }
                else if (CheckEquip(onSlot.item, item))
                {
                    QuickEquip();
                }
                else
                {
                    ItemMove(false);
                }
            }
            else
            {
                var itemMove = itemSlots.Find(x => x.item != null && x.item != item) == null && itemSlots.Count == item.size.x * item.size.y;
                if (itemMove)
                {
                    ItemMove(true);
                }
                else
                {
                    ItemMove(false);
                }
            }

            item.targetImage.color = Color.clear;
            holdingItem = null;
            onSlots.Clear();
            InactiveSampleItem();
        }

        void ItemMove(bool value)
        {
            switch (value)
            {
                case true:
                    item.SetItemSlots(null, DataUtility.slot_noItemColor);
                    item.itemSlots = new List<ItemSlot>(itemSlots);
                    UnequipItem(item);

                    for (int i = 0; i < itemSlots.Count; i++)
                    {
                        var itemSlot = itemSlots[i];
                        itemSlot.item = item;
                        itemSlot.SetItemSlot(DataUtility.slot_onItemColor);
                    }

                    item.itemSlots = new List<ItemSlot>(itemSlots);
                    item.SetItemScale(false);
                    item.ChangeRectPivot(false);
                    item.transform.SetParent(itemSlots[0].transform, false);
                    item.transform.localPosition = Vector3.zero;
                    if (item.itemData.type == ItemType.Magazine)
                    {
                        item.countText.enabled = true;
                    }
                    break;
                case false:
                    if (item.equipSlot != null)
                    {
                        item.equipSlot.slotText.enabled = false;
                        item.SetItemScale(true);
                        item.ChangeRectPivot(true);
                        item.transform.SetParent(item.equipSlot.transform, false);
                        item.transform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        item.SetItemRotation(sampleItem.rotation);
                        item.transform.SetParent(item.itemSlots[0].transform, false);
                        item.transform.position = item.itemSlots[0].transform.position;
                    }

                    for (int i = 0; i < itemSlots.Count; i++)
                    {
                        var itemSlot = itemSlots[i];
                        if (itemSlot.item == null)
                        {
                            itemSlot.SetItemSlot(DataUtility.slot_noItemColor);
                        }
                        else
                        {
                            itemSlot.item.SetItemSlots(DataUtility.slot_onItemColor);
                        }
                    }
                    break;
            }

            if (holdingItem != null)
            {
                item.targetImage.raycastTarget = true;
            }
        }

        void ItemNesting()
        {
            onSlot.item.SetItemSlots(DataUtility.slot_onItemColor);
            var newTotal = onSlot.item.TotalCount + item.TotalCount;
            var maxValue = onSlot.item.itemData.maxNesting;
            if (maxValue >= newTotal)
            {
                item.SetItemSlots(DataUtility.slot_noItemColor);
                onSlot.item.SetTotalCount(newTotal);
                InActiveItem(item);
            }
            else
            {
                item.SetItemSlots(DataUtility.slot_onItemColor);
                item.transform.SetParent(item.itemSlots[0].transform, false);
                item.transform.localPosition = Vector3.zero;
                item.SetTotalCount(newTotal - maxValue);
                onSlot.item.SetTotalCount(maxValue);
            }
            onSlot.item.targetImage.raycastTarget = true;
        }

        void QuickEquip()
        {
            onSlot.item.SetItemSlots(DataUtility.slot_onItemColor);
            switch (item.itemData.type)
            {
                case ItemType.Bullet:
                    if (onSlot.item.itemData.type == ItemType.MainWeapon || onSlot.item.itemData.type == ItemType.SubWeapon)
                    {
                        onSlot.item.weaponData.chamberBullet = item.bulletData;
                        onSlot.item.weaponData.isChamber = true;
                        if (item.TotalCount > 1)
                        {
                            item.transform.SetParent(item.itemSlots[0].transform, false);
                            item.transform.localPosition = Vector3.zero;
                            item.SetTotalCount(item.TotalCount - 1);
                        }
                        else
                        {
                            item.SetItemSlots(null, DataUtility.slot_noItemColor);
                            InActiveItem(item);
                        }
                    }
                    else if (onSlot.item.itemData.type == ItemType.Magazine)
                    {
                        var newTotal = onSlot.item.magData.loadedBullets.Count + item.TotalCount;
                        if (onSlot.item.magData.magSize >= newTotal)
                        {
                            for (int i = 0; i < newTotal; i++)
                            {
                                onSlot.item.magData.loadedBullets.Add(item.bulletData);
                            }
                            item.SetItemSlots(null, DataUtility.slot_noItemColor);
                            InActiveItem(item);
                        }
                        else
                        {
                            var num = onSlot.item.magData.magSize - onSlot.item.magData.loadedBullets.Count;
                            for (int i = 0; i < num; i++)
                            {
                                onSlot.item.magData.loadedBullets.Add(item.bulletData);
                            }
                            item.transform.SetParent(item.itemSlots[0].transform, false);
                            item.transform.localPosition = Vector3.zero;
                            item.SetTotalCount(item.TotalCount - num);
                        }
                        onSlot.item.SetTotalCount(onSlot.item.magData.loadedBullets.Count);
                    }
                    break;
                case ItemType.Magazine:
                    onSlot.item.weaponData.equipMag = item.magData;
                    onSlot.item.weaponData.isMag = true;
                    item.SetItemSlots(null, DataUtility.slot_noItemColor);
                    InActiveItem(item);
                    onSlot.item.SetPartsSample();
                    break;
                case ItemType.Sight:
                    onSlot.item.weaponData.equipPartsList.Add(item.partsData);
                    item.SetItemSlots(null, DataUtility.slot_noItemColor);
                    InActiveItem(item);
                    onSlot.item.SetPartsSample();
                    break;
                default:
                    break;
            }
            onSlot.item.targetImage.raycastTarget = true;
            if (popUp.gameObject.activeSelf && selectItem == onSlot.item)
            {
                popUp.PopUp_ItemInformation();
            }
        }

        void ItemSplit()
        {
            item.SetItemSlots(DataUtility.slot_onItemColor);
            item.transform.SetParent(item.itemSlots[0].transform, false);
            item.transform.localPosition = Vector3.zero;

            popUp.PopUp_Split(item, onSlots);

            holdingItem = null;
            sampleItem.transform.position = onSlots[0].transform.position;

            item.targetImage.color = Color.clear;
        }
    }

    public void EquipItem(ItemHandler item, EquipSlot equipSlot)
    {
        if (equipSlot.CheckEquip(item))
        {
            var itemSlots = new List<ItemSlot>(item.itemSlots);
            equipSlot.item = item;
            equipSlot.slotText.enabled = false;

            item.countText.enabled = false;
            item.equipSlot = equipSlot;
            item.SetItemSlots(null, DataUtility.slot_noItemColor);
            item.itemSlots.Clear();

            item.ChangeRectPivot(true);
            item.SetItemRotation(false);
            item.transform.SetParent(equipSlot.transform, false);
            item.transform.localPosition = Vector3.zero;
            item.targetImage.raycastTarget = true;
            item.SetItemScale(true);

            switch (item.itemData.type)
            {
                case ItemType.MainWeapon:
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        playerCtr.AddWeapon(item.weaponData);
                    }
                    break;
                case ItemType.SubWeapon:
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        playerCtr.AddWeapon(item.weaponData);
                    }
                    break;
                case ItemType.Bullet:
                    if (item.TotalCount > 1)
                    {
                        var count = item.TotalCount - 1;
                        SetItemInStorage(item.itemData, count, itemSlots);
                        item.SetTotalCount(1);
                    }
                    equipSlot.countText.enabled = false;
                    popUp.item.weaponData.chamberBullet = item.bulletData;
                    popUp.item.weaponData.isChamber = true;
                    break;
                case ItemType.Magazine:
                    equipSlot.countText.enabled = true;
                    equipSlot.countText.text = $"{item.TotalCount}";
                    popUp.item.weaponData.equipMag = item.magData;
                    popUp.item.weaponData.isMag = true;
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        var weapon = playerCtr.weapons.Find(x => x.weaponData == popUp.item.weaponData);
                        if (weapon != null)
                        {
                            weapon.SetParts(item.magData.ID, true);
                        }
                    }
                    break;
                default:
                    equipSlot.countText.enabled = false;
                    if (item.partsData != null && item.partsData.type != WeaponPartsType.None)
                    {
                        if (popUp.item.weaponData.equipPartsList.Find(x => x.ID == item.partsData.ID) == null)
                        {
                            popUp.item.weaponData.equipPartsList.Add(item.partsData);
                        }
                        if (gameMgr != null && gameMgr.playerList.Count > 0)
                        {
                            var playerCtr = gameMgr.playerList[0];
                            var weapon = playerCtr.weapons.Find(x => x.weaponData == popUp.item.weaponData);
                            if (weapon != null)
                            {
                                weapon.SetParts(item.partsData.ID, true);
                            }
                        }
                    }
                    break;
            }

            if (popUp.item != null)
            {
                popUp.item.SetPartsSample();
                popUp.SetPartsSample();
            }
        }
        else if (item.equipSlot != null)
        {
            item.equipSlot.slotText.enabled = false;
            item.SetItemScale(true);
            item.ChangeRectPivot(true);
            item.transform.SetParent(item.equipSlot.transform, false);
            item.transform.localPosition = Vector3.zero;
        }
        else
        {
            item.SetItemSlots(DataUtility.slot_onItemColor);
            item.transform.SetParent(item.itemSlots[0].transform, false);
            item.transform.position = item.itemSlots[0].transform.position;
        }
        equipSlot.backImage.color = DataUtility.equip_defaultColor;
        item.targetImage.color = Color.clear;

        holdingItem = null;
        InactiveSampleItem();
    }

    public void UnequipItem(ItemHandler item)
    {
        if (item.equipSlot == null) return;

        switch (item.itemData.type)
        {
            case ItemType.Bullet:
                popUp.item.weaponData.chamberBullet = null;
                popUp.item.weaponData.isChamber = false;
                break;
            case ItemType.Magazine:
                item.SetTotalCount(item.magData.loadedBullets.Count);
                popUp.item.weaponData.equipMag = null;
                popUp.item.weaponData.isMag = false;
                if (gameMgr != null && gameMgr.playerList.Count > 0)
                {
                    var playerCtr = gameMgr.playerList[0];
                    var weapon = playerCtr.weapons.Find(x => x.weaponData == popUp.item.weaponData);
                    if (weapon != null)
                    {
                        weapon.SetParts(item.magData.ID, false);
                    }
                }
                break;
            default:
                if (item.partsData != null && item.partsData.type != WeaponPartsType.None)
                {
                    var find = popUp.item.weaponData.equipPartsList.Find(x => x.ID == item.partsData.ID);
                    popUp.item.weaponData.equipPartsList.Remove(find);
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        var weapon = playerCtr.weapons.Find(x => x.weaponData == popUp.item.weaponData);
                        if (weapon != null)
                        {
                            weapon.SetParts(item.partsData.ID, true);
                        }
                    }
                }
                break;
        }

        item.equipSlot.item = null;
        item.equipSlot = null;
        if (popUp.item != null)
        {
            popUp.item.SetPartsSample();
            popUp.SetPartsSample();
        }

        if (gameMgr != null && gameMgr.playerList.Count > 0
         && (item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon))
        {
            var playerCtr = gameMgr.playerList[0];
            playerCtr.RemoveWeapon(item.weaponData.weaponName);
        }
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

    public void InActiveItem(ItemHandler item)
    {
        item.itemData = null;
        item.weaponData = null;
        item.magData = null;
        item.partsData = null;
        if (item.equipSlot)
        {
            item.equipSlot.item = null;
            item.equipSlot = null;
        }
        //item.SetItemSlots(null, DataUtility.slot_noItemColor);
        item.itemSlots.Clear();
        item.transform.SetParent(itemPool, false);

        if (item.activeSample != null)
        {
            item.activeSample.SetActive(false);
            item.activeSample = null;
        }
        item.gameObject.SetActive(false);
        activeItem.Remove(item);
    }

    public void InactiveSampleItem()
    {
        sampleItem.transform.SetParent(itemPool, false);
        sampleItem.transform.SetAsFirstSibling();
        sampleItem.gameObject.SetActive(false);
        for (int i = 0; i < allEquips.Count; i++)
        {
            var equipSlot = allEquips[i];
            equipSlot.outline.enabled = false;
        }
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            item.frameImage.enabled = false;
        }
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
