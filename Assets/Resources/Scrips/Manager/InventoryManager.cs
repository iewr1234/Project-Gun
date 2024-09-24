using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("---Access Script---")]
    public List<EquipSlot> allEquips = new List<EquipSlot>();
    public List<MyStorage> myStorages = new List<MyStorage>();
    public OtherStorage otherStorage;

    [HideInInspector] public DataManager dataMgr;
    [HideInInspector] public GameManager gameMgr;
    [HideInInspector] public BaseManager baseMgr;

    //[HideInInspector] public PopUp_Inventory popUp;
    public List<PopUp_Inventory> activePopUp;
    private List<PopUp_Inventory> popUpList;

    [HideInInspector] public ContextMenu contextMenu;

    [Header("---Access Component---")]
    public ItemHandler sampleItem;

    [HideInInspector] public Camera invenCam;
    [HideInInspector] public Camera subCam;
    private Canvas invenUI;

    private ScrollRect myScrollRect;
    private GameObject myScrollbar;

    private ScrollRect otherScrollRect;
    private GameObject otherScrollbar;

    public Button nextButton;
    public Button returnButton;
    public Button closeButton;

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

    [HideInInspector] public bool showStorage;
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

    public void Start()
    {
        SetComponents();
    }

    public void SetComponents()
    {
        //gameMgr = _gmaeMgr;
        dataMgr = FindAnyObjectByType<DataManager>();

        //popUp = transform.Find("InventoryUI/PopUp").GetComponent<PopUp_Inventory>();
        //popUp.SetComponents(this);
        popUpList = transform.Find("InventoryUI/PopUpList").GetComponentsInChildren<PopUp_Inventory>().ToList();
        for (int i = 0; i < popUpList.Count; i++)
        {
            var popUp = popUpList[i];
            popUp.SetComponents(this);
        }
        contextMenu = transform.Find("InventoryUI/ContextMenu").GetComponent<ContextMenu>();
        contextMenu.SetComponents(this);

        invenCam = transform.Find("InventoryCamera").GetComponent<Camera>();
        subCam = transform.Find("InventoryCamera/SubCamera").GetComponent<Camera>();
        invenUI = transform.Find("InventoryUI").GetComponent<Canvas>();
        invenUI.worldCamera = invenCam;

        myScrollRect = invenUI.transform.Find("MyStorage/ScrollView").GetComponent<ScrollRect>();
        myScrollbar = invenUI.transform.Find("MyStorage/ScrollView/Scrollbar Vertical").gameObject;

        otherScrollRect = invenUI.transform.Find("OtherStorage/Components/ScrollView").GetComponent<ScrollRect>();
        otherScrollbar = invenUI.transform.Find("OtherStorage/Components/ScrollView/Scrollbar Vertical").gameObject;

        nextButton = invenUI.transform.Find("TopBar/Buttons/Next").GetComponent<Button>();
        nextButton.gameObject.SetActive(false);
        returnButton = invenUI.transform.Find("TopBar/Buttons/Return").GetComponent<Button>();
        returnButton.gameObject.SetActive(false);
        closeButton = invenUI.transform.Find("TopBar/Buttons/Close").GetComponent<Button>();
        closeButton.gameObject.SetActive(false);

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
        otherStorage.SetActive(false);

        CreateItems();
        invenUI.gameObject.SetActive(false);

        if (dataMgr == null) return;

        for (int i = 0; i < dataMgr.gameData.initialItemIDList.Count; i++)
        {
            var initialItem = dataMgr.gameData.initialItemIDList[i];
            switch (initialItem.createPos)
            {
                case CreatePos.Equip:
                    SetItemInEquipSlot(initialItem);
                    break;
                case CreatePos.Pocket:
                    var pocket = myStorages.Find(x => x.type == MyStorageType.Pocket);
                    SetItemInStorage(initialItem.ID, initialItem.num, pocket.itemSlots, true);
                    break;
                case CreatePos.Rig:
                    var rig = myStorages.Find(x => x.type == MyStorageType.Rig);
                    SetItemInStorage(initialItem.ID, initialItem.num, rig.itemSlots, true);
                    break;
                case CreatePos.Backpack:
                    var backpack = myStorages.Find(x => x.type == MyStorageType.Backpack);
                    SetItemInStorage(initialItem.ID, initialItem.num, backpack.itemSlots, true);
                    break;
                default:
                    break;
            }
        }
    }

    private void CreateItems()
    {
        for (int i = 0; i < itemPoolMax; i++)
        {
            var item = Instantiate(Resources.Load<ItemHandler>("Prefabs/Inventory/Item"));
            item.transform.SetParent(itemPool, false);
            item.SetComponents(this, i);
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

    /// <summary>
    /// 키보드 입력
    /// </summary>
    private void KeyboardInput()
    {
        if (gameMgr != null && Input.GetKeyDown(KeyCode.I))
        {
            if (gameMgr.gameState == GameState.Result) return;
            if (gameMgr.playerList.Count == 0) return;

            itemSplit = false;
            var player = gameMgr.playerList[0];
            var value = invenUI.gameObject.activeSelf;
            if (player.state == CharacterState.Base)
            {
                gameMgr.gameState = value ? GameState.Base : GameState.Inventory;
            }
            else
            {
                gameMgr.gameState = value ? GameState.None : GameState.Inventory;
            }
            gameMgr.camMgr.lockCam = !value;
            if (!value)
            {
                SetOtherStorage(null);
            }
            SetStorageUI(!value);
            ShowInventory();
        }

        if (!invenUI.gameObject.activeSelf) return;

        if (holdingItem != null && Input.GetKeyDown(KeyCode.R))
        {
            RotateItem();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && activePopUp.Count > 0)
        {
            activePopUp[^1].Button_PopUp_Close();
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

    /// <summary>
    /// 인벤토리 열기
    /// </summary>
    public void ShowInventory()
    {
        if (gameMgr == null) return;
        if (gameMgr.gameState == GameState.Shoot || gameMgr.gameState == GameState.Watch) return;

        var value = !invenCam.enabled;
        invenCam.enabled = value;
        subCam.enabled = value;
        invenUI.gameObject.SetActive(value);

        gameMgr.DeselectCharacter();
        //gameMgr.gameState = invenUI.gameObject.activeSelf ? GameState.Inventory : GameState.None;
        if (!value && activePopUp.Count > 0)
        {
            for (int i = activePopUp.Count - 1; i >= 0; i--)
            {
                var popUp = activePopUp[i];
                popUp.Button_PopUp_Close();
            }
        }

        if (gameMgr.uiMgr != null)
        {
            gameMgr.uiMgr.playUI.SetActive(!value);
        }
        if (gameMgr.mapEdt != null)
        {
            gameMgr.mapEdt.gameObject.SetActive(!value);
        }
    }

    public void SetItemStorage(bool value)
    {
        if (otherStorage.storageInfos.Count == 0) return;

        otherStorage.SetActive(value);
        showStorage = value;
        switch (value)
        {
            case true:
                otherStorage.ActiveTabButtons(otherStorage.storageInfos.Count);
                otherStorage.GetStorageInfo(0);
                break;
            case false:
                otherStorage.DeactiveTabButtons();
                break;
        }
    }

    /// <summary>
    /// 아이템 회전
    /// </summary>
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

    /// <summary>
    /// 마우스 입력
    /// </summary>
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
                //if (activePopUp.Find(x => x.item == onEquip.item) != null) return;

                selectItem = onEquip.item;
                var popUp = GetPopUp(PopUpState.ItemInformation);
                popUp.PopUp_ItemInformation(selectItem);
            }
            else if (onSlot != null && onSlot.item != null)
            {
                //if (activePopUp.Find(x => x.item == onSlot.item) != null) return;

                selectItem = onSlot.item;
                var popUp = GetPopUp(PopUpState.ItemInformation);
                popUp.PopUp_ItemInformation(selectItem);
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

    public void SetItemInStorage(string dataID, int count, List<ItemSlot> itemSlots, bool insertOption)
    {
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.dataID == dataID);
        if (itemData == null)
        {
            Debug.Log("Not found item");
            return;
        }

        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count, insertOption);

        var emptySlots = FindEmptySlots(item, itemSlots);
        if (emptySlots == null)
        {
            Debug.Log("not found ItemSlot");
            InActiveItem(item);
        }
        else
        {
            PutTheItem(item, emptySlots);
        }
    }

    public void SetItemInStorage(ItemDataInfo itemData, int count, List<ItemSlot> itemSlots, bool insertOption)
    {
        if (itemData == null)
        {
            Debug.Log("Not found item");
            return;
        }

        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count, insertOption);

        var emptySlots = FindEmptySlots(item, itemSlots);
        if (emptySlots == null)
        {
            Debug.Log("not found ItemSlot");
            InActiveItem(item);
        }
        else
        {
            PutTheItem(item, emptySlots);
        }
    }

    private List<ItemSlot> FindEmptySlots(ItemHandler item, List<ItemSlot> itemSlots)
    {
        int index = 0;
        List<ItemSlot> emptySlots = null;
        while (index != itemSlots.Count)
        {
            var emptySlot = itemSlots[index];
            if (emptySlot.item != null)
            {
                index++;
                emptySlots = null;
                continue;
            }

            emptySlots = FindAllMultiSizeSlots(itemSlots, item.size, emptySlot.slotIndex);
            if (emptySlots.Find(x => x.item != null) == null && emptySlots.Count == item.size.x * item.size.y)
            {
                break;
            }
            else
            {
                index++;
                emptySlots = null;
                continue;
            }
        }

        return emptySlots;
    }

    public void SetItemInStorage(ItemDataInfo itemData, int count, List<ItemSlot> itemSlots)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count, false);
        PutTheItem(item, itemSlots);
    }

    public void SetItemInStorage(MagazineDataInfo magData)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(magData);

        List<ItemSlot> emptySlots = null;
        var rigStorages = myStorages.FindAll(x => x.type == MyStorageType.Rig);
        for (int i = 0; i < rigStorages.Count; i++)
        {
            var rigStorage = rigStorages[i];
            var itemSlots = rigStorage.itemSlots.FindAll(x => x.gameObject.activeSelf);
            emptySlots = FindEmptySlots(item, itemSlots);
            if (emptySlots == null)
            {
                continue;
            }
            else
            {
                break;
            }
        }

        if (emptySlots.Count < item.size.x * item.size.y)
        {
            var itemSlots = otherStorage.itemSlots.FindAll(x => x.gameObject.activeSelf);
            emptySlots = FindEmptySlots(item, itemSlots);
        }
        PutTheItem(item, emptySlots);
    }

    public void SetItemInEquipSlot(InitialItem initialItem)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.dataID == initialItem.ID);
        item.SetItemInfo(itemData, initialItem.num, false);

        var equipSlot = allEquips.Find(x => x.CheckEquip(item) == true);
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

    public void SetItemInEquipSlot(BulletDataInfo bulletData, int count, EquipSlot equipSlot)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.dataID == bulletData.ID);
        item.SetItemInfo(itemData, count, false);

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
        item.SetItemInfo(itemData, count, false);
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
        item.SetItemInfo(itemData, count, false);

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
        if (onItem == null) return false;
        if (onItem == putItem) return false;

        switch (putItem.itemData.type)
        {
            case ItemType.Bullet:
                //if (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                //{
                //    return !onItem.weaponData.isChamber && onItem.weaponData.caliber == putItem.bulletData.caliber;
                //}
                /*else */
                if (onItem.itemData.type == ItemType.Magazine)
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
                var itemMove = onSlots.Find(x => x.item != item) == null && onSlots.Count == item.size.x * item.size.y;
                if (itemNesting)
                {
                    ItemNesting();
                }
                else if (CheckEquip(onSlot.item, item))
                {
                    QuickEquip(onSlot.item, item);
                }
                else if (itemMove)
                {
                    CheckBaseStorage();
                    ItemMove(true);
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
                    CheckBaseStorage();
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

        void CheckBaseStorage()
        {
            if (item.itemSlots.Count == 0) return;
            if (item.itemSlots[0].otherStorage == null) return;

            var storageInfo = otherStorage.storageInfos[otherStorage.tabIndex];
            var find = storageInfo.itemList.Find(x => x.slotIndex == item.itemSlots[0].slotIndex);
            if (find != null)
            {
                storageInfo.itemList.Remove(find);
            }
        }

        void ItemMove(bool value)
        {
            switch (value)
            {
                case true:
                    ItemRegistration();
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
                //item.SetItemSlots(DataUtility.slot_noItemColor);
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

        void ItemSplit()
        {
            item.SetItemSlots(DataUtility.slot_onItemColor);
            item.transform.SetParent(item.itemSlots[0].transform, false);
            item.transform.localPosition = Vector3.zero;

            var popUp = GetPopUp(PopUpState.Split);
            popUp.PopUp_Split(item, onSlots);

            holdingItem = null;
            sampleItem.transform.position = onSlots[0].transform.position;

            item.targetImage.color = Color.clear;
        }

        void ItemRegistration()
        {
            if (itemSlots.Count == 0) return;
            if (otherStorage.storageInfos.Count == 0) return;

            var storageInfo = otherStorage.storageInfos[otherStorage.tabIndex];
            if (itemSlots[0].otherStorage != null && storageInfo.itemList.Find(x => x.slotIndex == itemSlots[0].slotIndex) == null)
            {
                var storageItemInfo = new StorageItemInfo()
                {
                    indexName = $"{item.itemData.itemName}_{itemSlots[0].slotIndex.x}/{itemSlots[0].slotIndex.y}",
                    slotIndex = itemSlots[0].slotIndex,
                    itemSize = item.size,
                    totalCount = item.TotalCount,
                    itemData = item.itemData,
                };
                storageInfo.itemList.Add(storageItemInfo);
            }
            else if (itemSlots[0].otherStorage == null && item.itemSlots.Count > 0)
            {
                var find = storageInfo.itemList.Find(x => x.slotIndex == item.itemSlots[0].slotIndex);
                if (find != null)
                {
                    storageInfo.itemList.Remove(find);
                }
            }
        }
    }

    public void EquipItem(ItemHandler putItem, EquipSlot equipSlot)
    {
        if (equipSlot.CheckEquip(putItem))
        {
            if (putItem.itemSlots.Count > 0 && putItem.itemSlots[0].otherStorage != null)
            {
                var storageInfo = otherStorage.storageInfos[otherStorage.tabIndex];
                var find = storageInfo.itemList.Find(x => x.slotIndex == putItem.itemSlots[0].slotIndex);
                if (find != null)
                {
                    storageInfo.itemList.Remove(find);
                }
            }
            else if (putItem.equipSlot != null)
            {
                UnequipItem(putItem);
            }

            var itemSlots = new List<ItemSlot>(putItem.itemSlots);
            equipSlot.item = putItem;
            equipSlot.slotText.enabled = false;

            putItem.countText.enabled = false;
            putItem.equipSlot = equipSlot;
            putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
            putItem.ChangeRectPivot(true);
            putItem.SetItemRotation(false);
            putItem.transform.SetParent(equipSlot.transform, false);
            putItem.transform.localPosition = Vector3.zero;
            putItem.targetImage.raycastTarget = true;
            putItem.SetItemScale(true);

            switch (putItem.itemData.type)
            {
                case ItemType.Backpack:
                    if (equipSlot.myStorage != null && putItem.backpackData != null)
                    {
                        equipSlot.myStorage.SetStorageSize(putItem.backpackData.storageSize);
                    }
                    break;
                case ItemType.MainWeapon:
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        playerCtr.AddWeapon(putItem);
                    }
                    break;
                case ItemType.SubWeapon:
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        playerCtr.AddWeapon(putItem);
                    }
                    break;
                //case ItemType.Bullet:
                //    if (item.TotalCount > 1)
                //    {
                //        var count = item.TotalCount - 1;
                //        SetItemInStorage(item.itemData, count, itemSlots);
                //        item.SetTotalCount(1);
                //    }
                //    equipSlot.countText.enabled = false;
                //    popUp.item.weaponData.chamberBullet = item.bulletData;
                //    popUp.item.weaponData.isChamber = true;
                //    break;
                case ItemType.Bullet:
                    if (equipSlot.intMagMax > 0)
                    {
                        var equipMag = equipSlot.popUp.item.weaponData.equipMag;
                        var newTotal = equipMag.loadedBullets.Count + putItem.TotalCount;
                        if (equipMag.magSize >= newTotal)
                        {
                            for (int i = 0; i < newTotal; i++)
                            {
                                equipMag.loadedBullets.Add(putItem.bulletData);
                            }
                        }
                        else
                        {
                            var num = equipMag.magSize - equipMag.loadedBullets.Count;
                            for (int i = 0; i < num; i++)
                            {
                                equipMag.loadedBullets.Add(putItem.bulletData);
                            }
                            SetItemInStorage(putItem.itemData, putItem.TotalCount - num, putItem.itemSlots, false);
                        }
                        equipSlot.popUp.item.SetTotalCount(equipMag.loadedBullets.Count);
                    }
                    break;
                case ItemType.Magazine:
                    equipSlot.countText.enabled = true;
                    equipSlot.countText.text = $"{putItem.TotalCount}";
                    equipSlot.popUp.item.weaponData.equipMag = putItem.magData;
                    equipSlot.popUp.item.weaponData.isMag = true;
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        var weapon = playerCtr.weapons.Find(x => x.weaponData == equipSlot.popUp.item.weaponData);
                        if (weapon != null)
                        {
                            weapon.SetParts(putItem.magData.magName, true);
                            if (putItem.magData.loadedBullets.Count > 0)
                            {
                                var chamberBullet = putItem.magData.loadedBullets[0];
                                playerCtr.SetBulletAbility(true, chamberBullet);
                            }
                        }
                    }
                    break;
                default:
                    equipSlot.countText.enabled = false;
                    if (putItem.partsData != null && putItem.partsData.type != WeaponPartsType.None)
                    {
                        if (equipSlot.popUp.item.weaponData.equipPartsList.Find(x => x.ID == putItem.partsData.ID) == null)
                        {
                            equipSlot.popUp.item.weaponData.equipPartsList.Add(putItem.partsData);
                        }
                        if (gameMgr != null && gameMgr.playerList.Count > 0)
                        {
                            var playerCtr = gameMgr.playerList[0];
                            var weapon = playerCtr.weapons.Find(x => x.weaponData == equipSlot.popUp.item.weaponData);
                            if (weapon != null)
                            {
                                weapon.SetParts(putItem.partsData.partsName, true);
                            }
                        }
                    }
                    break;
            }
            putItem.itemSlots.Clear();

            if (activePopUp.Contains(equipSlot.popUp))
            {
                equipSlot.popUp.item.SetPartsSample();
                equipSlot.popUp.PopUp_ItemInformation(equipSlot.popUp.item);
            }
        }
        else if (putItem.equipSlot != null)
        {
            putItem.equipSlot.slotText.enabled = false;
            putItem.SetItemScale(true);
            putItem.ChangeRectPivot(true);
            putItem.transform.SetParent(putItem.equipSlot.transform, false);
            putItem.transform.localPosition = Vector3.zero;
        }
        else
        {
            putItem.SetItemSlots(DataUtility.slot_onItemColor);
            putItem.transform.SetParent(putItem.itemSlots[0].transform, false);
            putItem.transform.position = putItem.itemSlots[0].transform.position;
        }
        equipSlot.backImage.color = DataUtility.equip_defaultColor;
        putItem.targetImage.color = Color.clear;

        holdingItem = null;
        InactiveSampleItem();
    }

    public void QuickEquip(ItemHandler onItem, ItemHandler putItem)
    {
        if (putItem.equipSlot != null)
        {
            UnequipItem(putItem);
        }

        onItem.SetItemSlots(DataUtility.slot_onItemColor);
        switch (putItem.itemData.type)
        {
            case ItemType.Bullet:
                //if (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                //{
                //    onItem.weaponData.chamberBullet = putItem.bulletData;
                //    onItem.weaponData.isChamber = true;
                //    if (putItem.TotalCount > 1)
                //    {
                //        putItem.transform.SetParent(putItem.itemSlots[0].transform, false);
                //        putItem.transform.localPosition = Vector3.zero;
                //        putItem.SetTotalCount(putItem.TotalCount - 1);
                //    }
                //    else
                //    {
                //        putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
                //        InActiveItem(putItem);
                //    }
                //}

                switch (onItem.itemData.type)
                {
                    case ItemType.MainWeapon:
                        InsertBullet_Weapon();
                        break;
                    case ItemType.SubWeapon:
                        InsertBullet_Weapon();
                        break;
                    case ItemType.Magazine:
                        InsertBullet_Magazine();
                        break;
                    default:
                        break;
                }
                break;
            case ItemType.Magazine:
                onItem.weaponData.equipMag = putItem.magData;
                onItem.weaponData.isMag = true;
                //putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
                InActiveItem(putItem);
                onItem.SetPartsSample();
                break;
            case ItemType.Sight:
                onItem.weaponData.equipPartsList.Add(putItem.partsData);
                //putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
                InActiveItem(putItem);
                onItem.SetPartsSample();
                break;
            default:
                break;
        }

        onItem.targetImage.raycastTarget = true;
        var popUp = activePopUp.Find(x => x.state == PopUpState.ItemInformation && x.item == onItem);
        if (popUp != null)
        {
            popUp.PopUp_ItemInformation(popUp.item);
        }

        void InsertBullet_Weapon()
        {
            var reloadNum = gameMgr.uiMgr.GetAmmoIcon().value;
            if (reloadNum < putItem.TotalCount)
            {
                for (int i = 0; i < reloadNum; i++)
                {
                    onItem.weaponData.equipMag.loadedBullets.Add(putItem.bulletData);
                }
                putItem.SetTotalCount(putItem.TotalCount - reloadNum);
            }
            else
            {
                InActiveItem(putItem);
            }
        }

        void InsertBullet_Magazine()
        {
            var newTotal = onItem.magData.loadedBullets.Count + putItem.TotalCount;
            if (onItem.magData.magSize >= newTotal)
            {
                for (int i = 0; i < newTotal; i++)
                {
                    onItem.magData.loadedBullets.Add(putItem.bulletData);
                }
                //putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
                InActiveItem(putItem);
            }
            else
            {
                var num = onItem.magData.magSize - onItem.magData.loadedBullets.Count;
                for (int i = 0; i < num; i++)
                {
                    onItem.magData.loadedBullets.Add(putItem.bulletData);
                }
                putItem.transform.SetParent(putItem.itemSlots[0].transform, false);
                putItem.transform.localPosition = Vector3.zero;
                putItem.SetTotalCount(putItem.TotalCount - num);
            }
            onItem.SetTotalCount(onItem.magData.loadedBullets.Count);
        }
    }

    public void UnequipItem(ItemHandler item)
    {
        if (item.equipSlot == null) return;

        //var popUp = item.equipSlot.popUp;
        switch (item.itemData.type)
        {
            case ItemType.Backpack:
                if (item.equipSlot.myStorage != null)
                {
                    item.equipSlot.myStorage.SetStorageSize(Vector2Int.zero);
                }
                break;
            case ItemType.Bullet:
                item.equipSlot.popUp.item.weaponData.equipMag.loadedBullets.Clear();
                item.countText.enabled = true;
                break;
            case ItemType.Magazine:
                item.SetTotalCount(item.magData.loadedBullets.Count);
                item.equipSlot.popUp.item.weaponData.equipMag = null;
                item.equipSlot.popUp.item.weaponData.isMag = false;
                if (gameMgr != null && gameMgr.playerList.Count > 0)
                {
                    var playerCtr = gameMgr.playerList[0];
                    var weapon = playerCtr.weapons.Find(x => x.weaponData == item.equipSlot.popUp.item.weaponData);
                    if (weapon != null)
                    {
                        weapon.SetParts(item.magData.magName, false);
                    }
                }
                break;
            default:
                if (item.partsData != null && item.partsData.type != WeaponPartsType.None)
                {
                    var find = item.equipSlot.popUp.item.weaponData.equipPartsList.Find(x => x.ID == item.partsData.ID);
                    item.equipSlot.popUp.item.weaponData.equipPartsList.Remove(find);
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        var weapon = playerCtr.weapons.Find(x => x.weaponData == item.equipSlot.popUp.item.weaponData);
                        if (weapon != null)
                        {
                            weapon.SetParts(item.partsData.partsName, true);
                        }
                    }
                }
                break;
        }

        item.equipSlot.item = null;
        if (activePopUp.Contains(item.equipSlot.popUp))
        {
            item.equipSlot.popUp.item.SetPartsSample();
            item.equipSlot.popUp.PopUp_ItemInformation(item.equipSlot.popUp.item);
        }
        item.equipSlot = null;

        if (gameMgr != null && gameMgr.playerList.Count > 0
         && (item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon))
        {
            var playerCtr = gameMgr.playerList[0];
            playerCtr.RemoveWeapon(item.weaponData.weaponName);
        }
    }

    public List<ItemSlot> FindAllMultiSizeSlots(List<ItemSlot> itemSlots, Vector2Int itemSize, Vector2Int startIndex)
    {
        var setSlots = itemSlots.FindAll(x => x.slotIndex.x >= startIndex.x
                                           && x.slotIndex.y >= startIndex.y
                                           && x.slotIndex.x < startIndex.x + itemSize.x
                                           && x.slotIndex.y < startIndex.y + itemSize.y);

        return setSlots;
    }

    public void ActiveSampleItem(ItemHandler item)
    {
        sampleItem.transform.SetParent(item.transform.parent, false);
        sampleItem.transform.localPosition = Vector3.zero;
        sampleItem.SetSampleItemInfo(item);
    }

    public PopUp_Inventory GetPopUp(PopUpState state)
    {
        PopUp_Inventory popUp = null;
        var activeItemInfo = activePopUp.Find(x => x.item == selectItem && x.state == PopUpState.ItemInformation);
        var activePopUps = activePopUp.FindAll(x => x.state == state);
        if (activeItemInfo)
        {
            popUp = activeItemInfo;
            RemoveActivePopUp(popUp);
            popUp.transform.SetSiblingIndex(activePopUp.Count);
            popUp.index = activePopUp.Count;
            popUp.SetPopUpPosition();
            activePopUp.Add(popUp);
            return popUp;
        }
        else if (activePopUps.Count < 3)
        {
            popUp = popUpList.Find(x => !x.gameObject.activeSelf);
        }
        else
        {
            popUp = activePopUps[0];
            activePopUps.RemoveAt(0);
            RemoveActivePopUp(popUp);
        }

        popUp.transform.SetSiblingIndex(activePopUp.Count);
        popUp.index = activePopUp.Count;
        switch (state)
        {
            case PopUpState.Split:
                popUp.transform.localPosition = DataUtility.popUp_defaultPos_split;
                break;
            default:
                if (activePopUps.Count > 0)
                {
                    var prevPopUp = activePopUps.OrderByDescending(x => x.index).First();
                    var offset = new Vector3(50f, -50f, 0f);
                    popUp.transform.localPosition = prevPopUp.transform.localPosition + offset;
                }
                else
                {
                    popUp.transform.localPosition = DataUtility.popUp_defaultPos;
                }
                break;
        }
        popUp.SetPopUpPosition();
        activePopUp.Add(popUp);

        return popUp;
    }

    public void RemoveActivePopUp(PopUp_Inventory removePopUp)
    {
        removePopUp.item = null;
        removePopUp.state = PopUpState.None;
        removePopUp.gameObject.SetActive(false);
        activePopUp.Remove(removePopUp);

        ResetActivePopUp();
    }

    public void ResetActivePopUp()
    {
        for (int i = 0; i < activePopUp.Count; i++)
        {
            var popUp = activePopUp[i];
            popUp.transform.SetSiblingIndex(i);
            popUp.index = i;
            popUp.SetPopUpPosition();
        }
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
        item.SetItemSlots(null, DataUtility.slot_noItemColor);
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

    public void SetResultUI(bool value)
    {
        switch (value)
        {
            case true:
                SetStorageUI(true);
                for (int i = 0; i < gameMgr.enemyList.Count; i++)
                {
                    var dropTableData = gameMgr.enemyList[i].dropTableData;
                    var itemLevel = Random.Range(dropTableData.minItemLevel, dropTableData.maxItemLevel);

                    // Equipment
                    if (dropTableData.dropInfo_equipment.itemMaxNum > 0 && dropTableData.dropInfo_equipment.TotalPercentage > 0)
                    {
                        SetLootItem("Equipment", dropTableData.dropInfo_equipment);
                    }

                    // Expendable
                    if (dropTableData.dropInfo_expendable.itemMaxNum > 0 && dropTableData.dropInfo_expendable.TotalPercentage > 0)
                    {
                        //SetLootItem("Expendable", dropTableData.dropInfo_expendable);
                    }

                    // Ingredient
                    if (dropTableData.dropInfo_ingredient.itemMaxNum > 0 && dropTableData.dropInfo_ingredient.TotalPercentage > 0)
                    {
                        //SetLootItem("Ingredient", dropTableData.dropInfo_ingredient);
                    }
                }

                if (gameMgr.dataMgr.gameData.stageData.waveNum >= 0)
                {
                    nextButton.gameObject.SetActive(true);
                }
                else
                {
                    returnButton.gameObject.SetActive(true);
                }
                break;
            case false:
                nextButton.gameObject.SetActive(false);
                returnButton.gameObject.SetActive(false);
                break;
        }

        void SetLootItem(string classType, DropTable dropTable)
        {
            var itemNum = Random.Range(dropTable.itemMinNum, dropTable.itemMaxNum);
            for (int j = 0; j < itemNum; j++)
            {
                var rarity = GetRarity(dropTable);
                var itemDatas = dataMgr.itemData.itemInfos.FindAll(x => x.setDropTable && x.rarity == rarity && ItemClassification(classType, x.type));
                if (itemDatas.Count == 0) break;

                var itemData = itemDatas[Random.Range(0, itemDatas.Count)];
                SetItemInStorage(itemData, 1, otherStorage.itemSlots, true);
            }
        }

        ItemRarity GetRarity(DropTable dropTable)
        {
            var percentage = Random.Range(0, dropTable.TotalPercentage);
            if (percentage < dropTable.LowGrade)
            {
                return ItemRarity.LowGrade;
            }
            else if (percentage < dropTable.Nomal)
            {
                return ItemRarity.Nomal;
            }
            else if (percentage < dropTable.MiddleGrade)
            {
                return ItemRarity.MiddleGrade;
            }
            else if (percentage < dropTable.HighGrade)
            {
                return ItemRarity.HighGrade;
            }
            else if (percentage < dropTable.Advanced)
            {
                return ItemRarity.Advanced;
            }
            else
            {
                return ItemRarity.Set;
            }
        }

        bool ItemClassification(string classType, ItemType type)
        {
            switch (classType)
            {
                case "Equipment":
                    if (type == ItemType.Head) return true;
                    if (type == ItemType.Body) return true;
                    if (type == ItemType.Rig) return true;
                    if (type == ItemType.Backpack) return true;
                    if (type == ItemType.MainWeapon) return true;
                    if (type == ItemType.SubWeapon) return true;
                    if (type == ItemType.Magazine) return true;
                    if (type == ItemType.Muzzle) return true;
                    if (type == ItemType.Sight) return true;
                    if (type == ItemType.FrontHandle) return true;
                    if (type == ItemType.Attachment) return true;
                    if (type == ItemType.UnderBarrel) return true;
                    break;
                case "Expendable":
                    break;
                case "Ingredient":
                    break;
                default:
                    break;
            }

            return false;
        }
    }

    public void SetOtherStorage(FieldNode node)
    {
        var baseStorages = dataMgr.gameData.baseStorages;
        var storageInfos = new List<StorageInfo>();
        if (node != null)
        {
            var storageInfo = baseStorages.Find(x => x.nodePos == node.nodePos);
            storageInfos.Add(storageInfo);
        }
        var currentNode = gameMgr.playerList[0].currentNode;
        otherStorage.storageInfos = storageInfos.Union(baseStorages.FindAll(x => x.nodePos.x <= currentNode.nodePos.x + 1 && x.nodePos.x >= currentNode.nodePos.x - 1
                                                                              && x.nodePos.y <= currentNode.nodePos.y + 1 && x.nodePos.y >= currentNode.nodePos.y - 1)).ToList();
        var floorStorage = new StorageInfo()
        {
            storageName = "지면",
            nodePos = currentNode.nodePos,
            slotSize = DataUtility.floorSlotSize,
        };
        otherStorage.storageInfos.Add(floorStorage);
    }

    public void SetLootStorage()
    {
        var lootStorage = new StorageInfo()
        {
            storageName = "전리품",
            nodePos = Vector2Int.zero,
            slotSize = DataUtility.floorSlotSize,
        };
        otherStorage.storageInfos.Add(lootStorage);
    }

    public void SetStorageUI(bool value)
    {
        //closeButton.gameObject.SetActive(value);
        otherStorage.SetActive(value);
        showStorage = value;
        switch (value)
        {
            case true:
                otherStorage.ActiveTabButtons(otherStorage.storageInfos.Count);
                otherStorage.GetStorageInfo(0);
                break;
            case false:
                otherStorage.DeactiveTabButtons();
                break;
        }
    }

    public void Button_Result_Next()
    {
        nextButton.enabled = false;
        SetStorageUI(false);
        gameMgr.NextMap();
    }

    public void Button_Result_Return()
    {
        returnButton.enabled = false;
        SetStorageUI(false);
        gameMgr.ReturnBase();
    }

    public void Button_Storage_Close()
    {
        SetStorageUI(false);
    }
}
