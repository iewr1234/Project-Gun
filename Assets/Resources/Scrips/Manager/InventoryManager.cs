using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameManager gameMgr;
    [HideInInspector] public DataManager dataMgr;

    [Space(5f)]
    //[HideInInspector] public PopUp_Inventory popUp;
    public List<PopUp_Inventory> activePopUp;
    private List<PopUp_Inventory> popUpList;

    [HideInInspector] public ContextMenu contextMenu;

    [Space(5f)]
    public List<EquipSlot> allEquips = new List<EquipSlot>();
    public List<MyStorage> myStorages = new List<MyStorage>();
    public OtherStorage otherStorage;

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
            charEquip.SetComponents(this, null);
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

        SetItemInStorage("Rifle_1", 1, otherStorage.itemSlots);
        SetItemInStorage("Rifle_2", 1, otherStorage.itemSlots);
        SetItemInStorage("Pistol_1", 1, otherStorage.itemSlots);

        SetItemInStorage("Scope_1", 1, otherStorage.itemSlots);
        SetItemInStorage("Scope_2", 1, otherStorage.itemSlots);

        SetItemInStorage("Magazine_1", 1, otherStorage.itemSlots);
        SetItemInStorage("Magazine_1", 1, otherStorage.itemSlots);
        SetItemInStorage("Magazine_2", 1, otherStorage.itemSlots);
        SetItemInStorage("Magazine_2", 1, otherStorage.itemSlots);
        SetItemInStorage("Magazine_3", 1, otherStorage.itemSlots);

        SetItemInStorage("Bullet_1", 100, otherStorage.itemSlots);
        SetItemInStorage("Bullet_2", 100, otherStorage.itemSlots);
        SetItemInStorage("Bullet_3", 50, otherStorage.itemSlots);
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
        else if (Input.GetKeyDown(KeyCode.Escape) && activePopUp.Count > 0)
        {
            activePopUp[^1].Button_PopUp_Close();
            RemoveActivePopUp(activePopUp[^1]);
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

        gameMgr.uiMgr.bottomUI.SetActive(!value);
        gameMgr.DeselectCharacter();
        gameMgr.gameState = invenUI.gameObject.activeSelf ? GameState.Inventory : GameState.None;
        if (!value && activePopUp.Count > 0)
        {
            for (int i = activePopUp.Count - 1; i >= 0; i--)
            {
                var popUp = activePopUp[i];
                popUp.Button_PopUp_Close();
            }
        }

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
                //if (activePopUp.Find(x => x.item == onEquip.item) != null) return;

                selectItem = onEquip.item;
                var popUp = GetPopUp(PopUpState.ItemInformation);
                popUp.PopUp_ItemInformation();
            }
            else if (onSlot != null && onSlot.item != null)
            {
                //if (activePopUp.Find(x => x.item == onSlot.item) != null) return;

                selectItem = onSlot.item;
                var popUp = GetPopUp(PopUpState.ItemInformation);
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

    public void SetItemInStorage(string itemName, int count, List<ItemSlot> itemSlots)
    {
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.itemName == itemName);
        if (itemData == null)
        {
            Debug.Log("Not found item");
            return;
        }

        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count);

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

            emptySlots = FindAllMultiSizeSlots(itemSlots, item, emptySlot.slotIndex);
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
        item.SetItemInfo(itemData, count);
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

        if (emptySlots == null)
        {
            var itemSlots = otherStorage.itemSlots.FindAll(x => x.gameObject.activeSelf);
            emptySlots = FindEmptySlots(item, itemSlots);
        }
        PutTheItem(item, emptySlots);
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
                if (itemNesting)
                {
                    ItemNesting();
                }
                else if (CheckEquip(onSlot.item, item))
                {
                    QuickEquip(onSlot.item, item);
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
    }

    public void EquipItem(ItemHandler putItem, EquipSlot equipSlot)
    {
        if (equipSlot.CheckEquip(putItem))
        {
            if (putItem.equipSlot != null)
            {
                UnequipItem(putItem);
            }

            var itemSlots = new List<ItemSlot>(putItem.itemSlots);
            equipSlot.item = putItem;
            equipSlot.slotText.enabled = false;

            putItem.countText.enabled = false;
            putItem.equipSlot = equipSlot;
            putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
            putItem.itemSlots.Clear();

            putItem.ChangeRectPivot(true);
            putItem.SetItemRotation(false);
            putItem.transform.SetParent(equipSlot.transform, false);
            putItem.transform.localPosition = Vector3.zero;
            putItem.targetImage.raycastTarget = true;
            putItem.SetItemScale(true);
     
            switch (putItem.itemData.type)
            {
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
                            weapon.SetParts(putItem.magData.ID, true);
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
                                weapon.SetParts(putItem.partsData.ID, true);
                            }
                        }
                    }
                    break;
            }

            if (activePopUp.Contains(equipSlot.popUp))
            {
                equipSlot.popUp.item.SetPartsSample();
                equipSlot.popUp.PopUp_ItemInformation();
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
                /*else*/
                if (onItem.itemData.type == ItemType.Magazine)
                {
                    var newTotal = onItem.magData.loadedBullets.Count + putItem.TotalCount;
                    if (onItem.magData.magSize >= newTotal)
                    {
                        for (int i = 0; i < newTotal; i++)
                        {
                            onItem.magData.loadedBullets.Add(putItem.bulletData);
                        }
                        putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
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
                break;
            case ItemType.Magazine:
                onItem.weaponData.equipMag = putItem.magData;
                onItem.weaponData.isMag = true;
                putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
                InActiveItem(putItem);
                onItem.SetPartsSample();
                break;
            case ItemType.Sight:
                onItem.weaponData.equipPartsList.Add(putItem.partsData);
                putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
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
            popUp.PopUp_ItemInformation();
        }
    }

    public void UnequipItem(ItemHandler item)
    {
        if (item.equipSlot == null) return;

        //var popUp = item.equipSlot.popUp;
        switch (item.itemData.type)
        {
            //case ItemType.Bullet:
            //    popUp.item.weaponData.chamberBullet = null;
            //    popUp.item.weaponData.isChamber = false;
            //    break;
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
                        weapon.SetParts(item.magData.ID, false);
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
                            weapon.SetParts(item.partsData.ID, true);
                        }
                    }
                }
                break;
        }

        if (activePopUp.Contains(item.equipSlot.popUp))
        {
            item.equipSlot.popUp.item.SetPartsSample();
            item.equipSlot.item = null;
            item.equipSlot.popUp.PopUp_ItemInformation();
        }
        item.equipSlot = null;

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
}
