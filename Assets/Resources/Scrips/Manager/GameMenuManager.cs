using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameMenuState
{
    None,
    Status,
    Inventory,
}

public class GameMenuManager : MonoBehaviour
{
    [Header("---Access Script---")]
    public DataManager dataMgr;
    public GameManager gameMgr;

    [Header("[InventoryUI]")]
    public List<EquipSlot> allEquips = new List<EquipSlot>();
    public TextMeshProUGUI weightText;
    public List<MyStorage> myStorages = new List<MyStorage>();
    public OtherStorage otherStorage;
    private List<PopUp_Inventory> popUpList;

    [HideInInspector] public PopUp_Warning popUp_warning;
    [HideInInspector] public ContextMenu contextMenu;

    [Header("---Access Component---")]
    public Camera gameMenuCam;
    private Canvas gameMenuUI;

    [Header("[StatusUI]")]
    private GameObject statusUI;
    [SerializeField] private List<StatusValue> statusTexts = new List<StatusValue>();
    [SerializeField] private List<StatusValue> physicalTexts = new List<StatusValue>();
    [SerializeField] private List<StatusValue> abilityTexts = new List<StatusValue>();
    private Button main1Tab;
    private Button main2Tab;
    private Button subTab;

    [Header("[InventoryUI]")]
    private GameObject invenUI;
    private ScrollRect myScrollRect;
    private GameObject myScrollbar;
    private ScrollRect otherScrollRect;
    private GameObject otherScrollbar;

    private ItemHandler sampleItem;
    [HideInInspector] public Transform itemPool;

    [Header("[Right Buttons]")]
    public Button statusButton;
    public Button invenButton;

    [Header("[Right Buttons]")]
    public Button nextButton;
    public Button returnButton;
    public Button closeButton;

    [Header("--- Assignment Variable---")]
    public GameMenuState state;
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
    public List<PopUp_Inventory> activePopUp;

    [HideInInspector] public bool showMenu;
    private bool itemSplit;

    private void Awake()
    {
        var find = FindObjectsOfType<GameMenuManager>();
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
        dataMgr = FindAnyObjectByType<DataManager>();
        popUp_warning = FindAnyObjectByType<PopUp_Warning>();
        popUpList = transform.Find("GameMenuUI/InventoryUI/PopUpList").GetComponentsInChildren<PopUp_Inventory>().ToList();
        for (int i = 0; i < popUpList.Count; i++)
        {
            var popUp = popUpList[i];
            popUp.SetComponents(this);
        }
        weightText = transform.Find("GameMenuUI/InventoryUI/Equip/WeightText").GetComponent<TextMeshProUGUI>();
        contextMenu = transform.Find("GameMenuUI/ContextMenu").GetComponent<ContextMenu>();
        contextMenu.SetComponents(this);

        gameMenuCam = transform.Find("GameMenuCamera").GetComponent<Camera>();
        gameMenuUI = transform.Find("GameMenuUI").GetComponent<Canvas>();
        gameMenuUI.worldCamera = gameMenuCam;
        SetStatusUI();
        SetInventoryUI();

        statusButton = gameMenuUI.transform.Find("TopBar/LeftButtons/Status").GetComponent<Button>();
        invenButton = gameMenuUI.transform.Find("TopBar/LeftButtons/Inventory").GetComponent<Button>();

        nextButton = gameMenuUI.transform.Find("TopBar/RightButtons/Next").GetComponent<Button>();
        nextButton.gameObject.SetActive(false);
        returnButton = gameMenuUI.transform.Find("TopBar/RightButtons/Return").GetComponent<Button>();
        returnButton.gameObject.SetActive(false);
        closeButton = gameMenuUI.transform.Find("TopBar/RightButtons/Close").GetComponent<Button>();
        closeButton.gameObject.SetActive(false);

        void SetStatusUI()
        {
            statusUI = gameMenuUI.transform.Find("StatusUI").gameObject;
            statusTexts = statusUI.transform.Find("ValueGroups/StatusGroup").GetComponentsInChildren<StatusValue>().ToList();
            physicalTexts = statusUI.transform.Find("ValueGroups/PhysicalGroup").GetComponentsInChildren<StatusValue>().ToList();
            abilityTexts = statusUI.transform.Find("ValueGroups/AbilityGroup").GetComponentsInChildren<StatusValue>().ToList();
            main1Tab = statusUI.transform.Find("ValueGroups/AbilityGroup/AbilityTab/Main1").GetComponent<Button>();
            main1Tab.interactable = false;
            main2Tab = statusUI.transform.Find("ValueGroups/AbilityGroup/AbilityTab/Main2").GetComponent<Button>();
            main2Tab.interactable = false;
            subTab = statusUI.transform.Find("ValueGroups/AbilityGroup/AbilityTab/Sub").GetComponent<Button>();
            subTab.interactable = false;

            statusUI.SetActive(false);
        }

        void SetInventoryUI()
        {
            invenUI = gameMenuUI.transform.Find("InventoryUI").gameObject;
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
            otherStorage.components.SetActive(false);

            myScrollRect = invenUI.transform.Find("MyStorage/ScrollView").GetComponent<ScrollRect>();
            myScrollbar = invenUI.transform.Find("MyStorage/ScrollView/Scrollbar Vertical").gameObject;
            otherScrollRect = invenUI.transform.Find("OtherStorage/Components/ScrollView").GetComponent<ScrollRect>();
            otherScrollbar = invenUI.transform.Find("OtherStorage/Components/ScrollView/Scrollbar Vertical").gameObject;

            itemPool = invenUI.transform.Find("ItemPool");
            sampleItem = itemPool.transform.Find("SampleItem").GetComponent<ItemHandler>();
            sampleItem.SetComponents(this);
            InactiveSampleItem();
            CreateItems();

            if (dataMgr != null)
            {
                List<StartingItemDataInfo> pStartingItems = dataMgr.startingItemData.startingItemInfos.FindAll(x => x.createLocation == "Player");
                for (int i = 0; i < pStartingItems.Count; i++)
                {
                    var startingItem = pStartingItems[i];
                    switch (startingItem.createSpace)
                    {
                        case CreateSpace.Equip:
                            SetInitialItemInEquipSlot(startingItem);
                            break;
                        case CreateSpace.Pocket:
                            var pocket = myStorages.Find(x => x.type == MyStorageType.Pocket);
                            SetInitialItemInStorage(startingItem.itemID, startingItem.createNum, pocket.itemSlots, true);
                            break;
                        case CreateSpace.Rig:
                            var rig = myStorages.Find(x => x.type == MyStorageType.Rig);
                            SetInitialItemInStorage(startingItem.itemID, startingItem.createNum, rig.itemSlots, true);
                            break;
                        case CreateSpace.Backpack:
                            var backpack = myStorages.Find(x => x.type == MyStorageType.Backpack);
                            SetInitialItemInStorage(startingItem.itemID, startingItem.createNum, backpack.itemSlots, true);
                            break;
                        default:
                            break;
                    }
                }
            }
            invenUI.SetActive(false);
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
        if (Input.GetKeyUp(KeyCode.O))
        {
            StatusProcess();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            InventoryProcess();
        }

        switch (state)
        {
            case GameMenuState.Status:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (gameMgr.gameState == GameState.Result) return;

                    StatusProcess();
                }
                break;
            case GameMenuState.Inventory:
                if (holdingItem != null && Input.GetKeyDown(KeyCode.R))
                {
                    RotateItem();
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (activePopUp.Count > 0)
                    {
                        activePopUp[^1].ClosePopUp();
                    }
                    else if (gameMgr.gameState != GameState.Result)
                    {
                        InventoryProcess();
                    }
                }

                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    itemSplit = true;
                }
                else if (Input.GetKeyUp(KeyCode.LeftControl))
                {
                    itemSplit = false;
                }
                break;
            default:
                break;
        }
    }

    private void StatusProcess()
    {
        if (gameMgr == null) return;
        if (gameMgr.gameState == GameState.Stage) return;
        if (gameMgr.gameState == GameState.Result) return;

        if (state != GameMenuState.Status)
        {
            TurnOffGameMenuUI();
            ShowStatus(true);
        }
        else
        {
            ShowStatus(false);
        }
    }

    public void ShowStatus(bool showStatus)
    {
        if (gameMgr.gameState == GameState.Shoot) return;
        if (gameMgr.gameState == GameState.Watch) return;
        if (gameMgr.playerList.Count == 0) return;

        var player = gameMgr.playerList[0];
        SetStatusTexts();
        SetPhysicalTexts();
        SetAbilityTexts();
        showMenu = showStatus;
        statusUI.SetActive(showStatus);
        ConvertGameMenu(player, showStatus, GameMenuState.Status);

        void SetStatusTexts()
        {
            if (!showStatus) return;

            statusTexts[0].SetAbilityText("힘", player.strength);
            statusTexts[1].SetAbilityText("활력", player.vitality);
            statusTexts[2].SetAbilityText("지능", player.intellect);
            statusTexts[3].SetAbilityText("지혜", player.wisdom);
            statusTexts[4].SetAbilityText("민첩", player.agility);
            statusTexts[5].SetAbilityText("솜씨", player.dexterity);
            statusTexts[6].SetAbilityText("이동력", player.Mobility);
        }

        void SetPhysicalTexts()
        {
            if (!showStatus) return;

            physicalTexts[0].SetAbilityText("최대행동력", player.maxAction);
            physicalTexts[1].SetAbilityText("최대체력", player.maxHealth);
            physicalTexts[2].SetAbilityText("최대기력", player.maxStamina);
            physicalTexts[3].SetAbilityText("시야", player.sight);
            physicalTexts[4].SetAbilityText("조준", player.aiming);
            physicalTexts[5].SetAbilityText("반응", player.reaction);
        }

        void SetAbilityTexts()
        {
            if (!showStatus) return;

            if (player.weapons.Count == 0)
            {
                abilityTexts[0].SetAbilityText("발사속도", player.RPM);
                abilityTexts[1].SetAbilityText("사거리", player.range);
                abilityTexts[2].SetAbilityText("경계각", player.watchAngle);
                abilityTexts[3].SetAbilityText("정확도", player.MOA);
                abilityTexts[4].SetAbilityText("안정성", player.stability);
                abilityTexts[5].SetAbilityText("반동", player.rebound);
                abilityTexts[6].SetAbilityText("장약", player.propellant);
                abilityTexts[7].SetAbilityText("피해량", player.damage);
                abilityTexts[8].SetAbilityText("관통", player.penetrate);
                abilityTexts[9].SetAbilityText("방어구 손상", player.armorBreak);
                abilityTexts[10].SetAbilityText("파편화", player.critical);
                main1Tab.interactable = false;
                main2Tab.interactable = false;
                subTab.interactable = false;
            }
            else
            {
                main1Tab.interactable = FindWeapon(EquipType.MainWeapon1);
                main2Tab.interactable = FindWeapon(EquipType.MainWeapon2);
                subTab.interactable = FindWeapon(EquipType.SubWeapon);
            }

            bool FindWeapon(EquipType equipType)
            {
                var weapon = player.weapons.Find(x => x.equipSlot.type == equipType);
                if (weapon != null)
                {
                    if (weapon == player.currentWeapon) ChangeAbilityTexts(player.ability, weapon.weaponData);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    private void ChangeAbilityTexts(CharacterController.Ability ability, WeaponDataInfo weaponData)
    {
        if (weaponData == null) return;

        abilityTexts[0].SetAbilityText("발사속도", ability.RPM + weaponData.RPM);
        abilityTexts[1].SetAbilityText("사거리", ability.range + weaponData.range);
        abilityTexts[2].SetAbilityText("경계각", ability.watchAngle + weaponData.watchAngle);
        abilityTexts[3].SetAbilityText("정확도", ability.MOA + weaponData.MOA);
        abilityTexts[4].SetAbilityText("안정성", ability.stability + weaponData.stability);
        abilityTexts[5].SetAbilityText("반동", ability.rebound + weaponData.rebound);

        if (weaponData.isMag && weaponData.equipMag.loadedBullets.Count > 0)
        {
            var bulletData = weaponData.equipMag.loadedBullets[^1];
            abilityTexts[6].SetAbilityText("장약", ability.propellant + bulletData.propellant);
            abilityTexts[7].SetAbilityText("피해량", ability.damage + bulletData.damage);
            abilityTexts[8].SetAbilityText("관통", ability.penetrate + bulletData.penetrate);
            abilityTexts[9].SetAbilityText("방어구 손상", ability.armorBreak + bulletData.armorBreak);
            abilityTexts[10].SetAbilityText("파편화", ability.critical + bulletData.critical);
        }
        else
        {
            abilityTexts[6].SetAbilityText("장약", ability.propellant);
            abilityTexts[7].SetAbilityText("피해량", ability.damage);
            abilityTexts[8].SetAbilityText("관통", ability.penetrate);
            abilityTexts[9].SetAbilityText("방어구 손상", ability.armorBreak);
            abilityTexts[10].SetAbilityText("파편화", ability.critical);
        }
    }

    private void InventoryProcess()
    {
        if (gameMgr == null) return;
        if (gameMgr.gameState == GameState.Stage) return;
        if (gameMgr.gameState == GameState.Result) return;

        if (state != GameMenuState.Inventory)
        {
            TurnOffGameMenuUI();
            ShowInventory(true);
        }
        else
        {
            ShowInventory(false);
        }
    }

    public void ShowInventory(bool showInven)
    {
        SetInventory(showInven, null);
    }

    public void ShowInventory(bool showInven, FieldNode node)
    {
        SetInventory(showInven, node);
    }

    private void SetInventory(bool showInven, FieldNode node)
    {
        if (gameMgr.gameState == GameState.Shoot) return;
        if (gameMgr.gameState == GameState.Watch) return;
        if (gameMgr.playerList.Count == 0) return;

        var player = gameMgr.playerList[0];
        invenUI.SetActive(showInven);
        itemSplit = false;
        if (showInven)
        {
            SetOtherStorage(node);
            SetStorageUI(true);
        }
        else
        {
            ItemMoveCancel(holdingItem, onSlots);
        }
        ConvertGameMenu(player, showInven, GameMenuState.Inventory);

        if (!showInven)
        {
            for (int i = activePopUp.Count - 1; i >= 0; i--)
            {
                var popUp = activePopUp[i];
                popUp.ClosePopUp();
            }
        }
    }

    private void ConvertGameMenu(CharacterController charCtr, bool value, GameMenuState state)
    {
        if (gameMgr.gameState != GameState.Result)
        {
            if (charCtr.state == CharacterState.Base)
            {
                gameMgr.gameState = value ? GameState.GameMenu : GameState.Base;
            }
            else
            {
                gameMgr.gameState = value ? GameState.GameMenu : GameState.None;
            }
        }
        gameMgr.camMgr.lockCam = value;

        gameMenuCam.enabled = value;
        gameMgr.DeselectCharacter();

        if (gameMgr.uiMgr != null)
        {
            gameMgr.uiMgr.playUI.SetActive(!value);
        }
        if (gameMgr.mapEdt != null)
        {
            gameMgr.mapEdt.gameObject.SetActive(!value);
        }

        if (!value && this.state != GameMenuState.None)
        {
            SetStorageUI(false);
        }
        this.state = value ? state : GameMenuState.None;
    }

    private void TurnOffGameMenuUI()
    {
        if (state == GameMenuState.None) return;

        switch (state)
        {
            case GameMenuState.Status:
                statusUI.SetActive(false);
                //StatusProcess();
                break;
            case GameMenuState.Inventory:
                for (int i = activePopUp.Count - 1; i >= 0; i--)
                {
                    var popUp = activePopUp[i];
                    popUp.ClosePopUp();
                }
                invenUI.SetActive(false);
                //InventoryProcess();
                break;
            default:
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
        if (!invenUI.activeSelf) return;

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
        if (!invenUI.activeSelf) return;

        myScrollRect.vertical = myScrollbar.activeSelf;
        otherScrollRect.vertical = otherScrollbar.activeSelf;
    }

    /// <summary>
    /// 아이템을 집음
    /// </summary>
    /// <param name="item"></param>
    public void TakeTheItem(ItemHandler item)
    {
        var splitPopUp = activePopUp.Find(x => x.state == PopUpState.Split);
        if (splitPopUp != null) splitPopUp.ClosePopUp();

        if (item.equipSlot == null)
        {
            ActiveSampleItem(item);
        }
        else
        {
            item.equipSlot.countText.enabled = false;
            item.equipSlot.chamberImage.enabled = false;
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

    /// <summary>
    /// 생성 아이템을 저장고에 넣음
    /// </summary>
    /// <param name="dataID"></param>
    /// <param name="count"></param>
    /// <param name="itemSlots"></param>
    /// <param name="insertOption"></param>
    public bool SetItemInStorage(string dataID, int count, List<ItemSlot> itemSlots, bool insertOption)
    {
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.dataID == dataID);
        if (itemData == null)
        {
            Debug.Log("Not found item");
            return false;
        }

        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count, insertOption);

        var emptySlots = FindEmptySlots(item, itemSlots);
        if (emptySlots == null)
        {
            Debug.Log("not found ItemSlot");
            item.DisableItem();
            return false;
        }
        else
        {
            PutTheItem(item, emptySlots);
            return true;
        }
    }

    /// <summary>
    /// 생성 아이템을 저장고에 넣음
    /// </summary>
    /// <param name="itemData"></param>
    /// <param name="count"></param>
    /// <param name="itemSlots"></param>
    /// <param name="insertOption"></param>
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
            item.DisableItem();
        }
        else
        {
            PutTheItem(item, emptySlots);
        }
    }

    public void SetItemInStorage(ItemDataInfo itemData, int count, bool rotation, List<ItemSlot> itemSlots)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count, false);
        item.SetItemRotation(rotation);
        PutTheItem(item, itemSlots);
    }

    public void SetItemInStorage(StorageItemInfo storageItem, List<ItemSlot> itemSlots)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(storageItem);
        item.SetItemRotation(storageItem.rotation);
        PutTheItem(item, itemSlots);
    }

    public void SetItemInStorage(MagazineDataInfo magData, List<ItemSlot> storageSlots)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(magData);
        if (!CheckSameItmeAndNesting(item, storageSlots)) return;

        List<ItemSlot> emptySlots = null;
        if (storageSlots != null)
        {
            emptySlots = FindEmptySlots(item, storageSlots);
            if (emptySlots != null)
            {
                //onSlots = emptySlots;
                PutTheItem(item, emptySlots);
                return;
            }
        }

        var myStorages = this.myStorages.FindAll(x => (storageSlots == null || (storageSlots != null && x != storageSlots[0].myStorage))
                                                   && (x.type == MyStorageType.Pocket || x.type == MyStorageType.Rig))
                                        .OrderByDescending(x => x.type).ToList();
        for (int i = 0; i < myStorages.Count; i++)
        {
            var myStorage = myStorages[i];
            var itemSlots = myStorage.itemSlots.FindAll(x => x.gameObject.activeSelf);
            if (!CheckSameItmeAndNesting(item, itemSlots)) return;

            emptySlots = FindEmptySlots(item, itemSlots);
            if (emptySlots != null) break;
        }

        if (emptySlots != null)
        {
            //onSlots = emptySlots;
            PutTheItem(item, emptySlots);
        }
        else if (otherStorage.components.activeSelf)
        {
            MoveItmeInOtherStorage(item);
        }
        else
        {
            otherStorage.DropItmeOnTheFloor(item);
        }
    }

    public void SetItemInStorage(BulletDataInfo bulletData, int count, List<ItemSlot> storageSlots)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(bulletData, count);
        if (!CheckSameItmeAndNesting(item, storageSlots)) return;

        List<ItemSlot> emptySlots = null;
        if (storageSlots != null)
        {
            emptySlots = FindEmptySlots(item, storageSlots);
            if (emptySlots != null)
            {
                //onSlots = emptySlots;
                PutTheItem(item, emptySlots);
                return;
            }
        }

        var myStorages = this.myStorages.FindAll(x => (storageSlots == null || (storageSlots != null && x != storageSlots[0].myStorage))
                                                   && (x.type == MyStorageType.Pocket || x.type == MyStorageType.Rig))
                                        .OrderByDescending(x => x.type).ToList();
        for (int i = 0; i < myStorages.Count; i++)
        {
            var myStorage = myStorages[i];
            var itemSlots = myStorage.itemSlots.FindAll(x => x.gameObject.activeSelf);
            if (!CheckSameItmeAndNesting(item, itemSlots)) return;

            emptySlots = FindEmptySlots(item, itemSlots);
            if (emptySlots != null) break;
        }

        if (emptySlots != null)
        {
            //onSlots = emptySlots;
            PutTheItem(item, emptySlots);
        }
        else if (otherStorage.components.activeSelf)
        {
            MoveItmeInOtherStorage(item);
        }
        else
        {
            otherStorage.DropItmeOnTheFloor(item);
        }
    }

    public void MoveItemInMyStorage(ItemHandler item, List<ItemSlot> itemSlots)
    {
        if (!CheckSameItmeAndNesting(item, itemSlots)) return;

        var emptySlots = FindEmptySlots(item, itemSlots);
        if (emptySlots == null)
        {
            MoveItmeInOtherStorage(item);
        }
        else
        {
            //onSlots = emptySlots;
            PutTheItem(item, emptySlots);
        }
    }

    public void DropChamberBullet(BulletDataInfo bulletData)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(bulletData, 1);
        otherStorage.DropItmeOnTheFloor(item);
    }

    private void MoveItmeInOtherStorage(ItemHandler item)
    {
        if (!CheckSameItmeAndNesting(item, otherStorage.itemSlots)) return;

        if (otherStorage.tabIndex != otherStorage.storageInfos.Count - 1)
        {
            var emptySlots = FindEmptySlots(item, otherStorage.itemSlots);
            if (emptySlots != null)
            {
                //onSlots = emptySlots;
                PutTheItem(item, emptySlots);
            }
            else
            {
                otherStorage.DropItmeOnTheFloor(item);
            }
        }
        else
        {
            //onSlots = FindEmptySlots(item, otherStorage.itemSlots);
            PutTheItem(item, FindEmptySlots(item, otherStorage.itemSlots));
        }
    }

    /// <summary>
    /// 빈 슬롯을 검색
    /// </summary>
    /// <param name="item"></param>
    /// <param name="itemSlots"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 같은 아이템 검색 후 중첩
    /// </summary>
    /// <param name="item"></param>
    /// <param name="itemSlots"></param>
    /// <returns></returns>
    private bool CheckSameItmeAndNesting(ItemHandler item, List<ItemSlot> itemSlots)
    {
        bool isItem = true;
        var sameItemSlots = FindSameItemSlots(item, itemSlots);
        if (sameItemSlots == null) return isItem;

        for (int i = 0; i < sameItemSlots.Count; i++)
        {
            var sameItem = sameItemSlots[i].item;
            if (sameItem.itemData.maxNesting < sameItem.TotalCount + item.TotalCount)
            {
                sameItem.SetTotalCount(sameItem.itemData.maxNesting);
                var result = sameItem.itemData.maxNesting - sameItem.TotalCount;
                item.ResultTotalCount(-result);
            }
            else
            {
                sameItem.ResultTotalCount(item.TotalCount);
                item.DisableItem();
                isItem = false;
                break;
            }
        }

        return isItem;

        List<ItemSlot> FindSameItemSlots(ItemHandler item, List<ItemSlot> itemSlots)
        {
            if (item.itemData.maxNesting == 1) return null;

            var sameItemSlots = itemSlots.FindAll(x => x.item != null && x.item.itemData.ID == item.itemData.ID
                                                    && x.item.TotalCount < x.item.itemData.maxNesting);
            return sameItemSlots;
        }
    }

    public void SetInitialItemInEquipSlot(StartingItemDataInfo startingItem)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.ID == startingItem.itemID);
        item.SetItemInfo(itemData, startingItem.createNum, true);

        var equipSlot = allEquips.Find(x => x.CheckEquip(item) == true);
        equipSlot.item = item;
        equipSlot.slotText.enabled = false;
        equipSlot.SetItemCount();

        item.countText.enabled = false;
        item.chamberImage.enabled = false;
        item.equipSlot = equipSlot;
        item.SetItemScale(true);
        item.ChangeRectPivot(true);
        item.transform.SetParent(equipSlot.transform, false);
        item.transform.localPosition = Vector3.zero;
        switch (item.itemData.type)
        {
            case ItemType.Rig:
                if (equipSlot.myStorage != null && item.rigData != null)
                {
                    equipSlot.myStorage.SetStorageSize(item.rigData.storageSize);
                }
                break;
            case ItemType.Backpack:
                if (equipSlot.myStorage != null && item.backpackData != null)
                {
                    equipSlot.myStorage.SetStorageSize(item.backpackData.storageSize);
                }
                break;
            default:
                break;
        }
    }

    public bool SetInitialItemInStorage(string itemDataID, int count, List<ItemSlot> itemSlots, bool insertOption)
    {
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.ID == itemDataID);
        if (itemData == null)
        {
            Debug.Log("Not found item");
            return false;
        }

        var item = items.Find(x => !x.gameObject.activeSelf);
        item.SetItemInfo(itemData, count, insertOption);

        var emptySlots = FindEmptySlots(item, itemSlots);
        if (emptySlots == null)
        {
            Debug.Log("not found ItemSlot");
            item.DisableItem();
            return false;
        }
        else
        {
            PutTheItem(item, emptySlots);
            return true;
        }
    }

    public void SetItemInEquipSlot(BulletDataInfo bulletData, int count, EquipSlot equipSlot)
    {
        var item = items.Find(x => !x.gameObject.activeSelf);
        var itemData = dataMgr.itemData.itemInfos.Find(x => x.dataID == bulletData.ID);
        item.SetItemInfo(itemData, count, false);

        equipSlot.item = item;
        equipSlot.slotText.enabled = false;
        equipSlot.countText.enabled = false;
        equipSlot.chamberImage.enabled = false;

        item.countText.enabled = false;
        item.chamberImage.enabled = false;
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
        equipSlot.chamberImage.enabled = false;

        item.countText.enabled = false;
        item.chamberImage.enabled = false;
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
        equipSlot.chamberImage.enabled = false;

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
                return Check_BulletType();
            case ItemType.Magazine:
                return (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                    && !onItem.weaponData.isMag
                    && putItem.magData.compatModel.Contains(onItem.weaponData.model)
                    && putItem.magData.compatCaliber == onItem.weaponData.caliber;
            case ItemType.Muzzle:
                return Check_PartsType();
            case ItemType.Sight:
                return Check_PartsType();
            case ItemType.Attachment:
                return Check_PartsType();
            case ItemType.UnderBarrel:
                return Check_PartsType();
            default:
                return false;
        }

        bool Check_BulletType()
        {
            switch (onItem.itemData.type)
            {
                case ItemType.MainWeapon:
                    return WeaponType();
                case ItemType.SubWeapon:
                    return WeaponType();
                case ItemType.Magazine:
                    return onItem.magData.compatCaliber == putItem.bulletData.caliber
                        && onItem.magData.loadedBullets.Count < onItem.magData.magSize;
                default:
                    return false;
            }

            bool WeaponType()
            {
                if (!onItem.weaponData.isChamber)
                {
                    return onItem.weaponData.caliber == putItem.bulletData.caliber;
                }
                else
                {
                    if (onItem.weaponData.isMag)
                    {
                        return onItem.weaponData.equipMag.compatCaliber == putItem.bulletData.caliber
                            && onItem.weaponData.equipMag.loadedBullets.Count < onItem.weaponData.equipMag.magSize;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        bool Check_PartsType()
        {
            switch (putItem.partsData.type)
            {
                case WeaponPartsType.Muzzle:
                    return (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                         && onItem.weaponData.equipPartsList.Find(x => x.type == putItem.partsData.type) == null
                         && putItem.partsData.compatModel.Contains(onItem.weaponData.model)
                         && onItem.weaponData.useMuzzle.Contains(putItem.partsData.size);
                case WeaponPartsType.Sight:
                    return (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                         && onItem.weaponData.equipPartsList.Find(x => x.type == putItem.partsData.type) == null
                         && putItem.partsData.compatModel.Contains(onItem.weaponData.model)
                         && onItem.weaponData.useSight.Contains(putItem.partsData.size);
                case WeaponPartsType.Attachment:
                    return (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                         && putItem.partsData.compatModel.Contains(onItem.weaponData.model)
                         && onItem.weaponData.useAttachment.Contains(putItem.partsData.size);
                case WeaponPartsType.UnderBarrel:
                    return (onItem.itemData.type == ItemType.MainWeapon || onItem.itemData.type == ItemType.SubWeapon)
                         && onItem.weaponData.equipPartsList.Find(x => x.type == putItem.partsData.type) == null
                         && putItem.partsData.compatModel.Contains(onItem.weaponData.model)
                         && onItem.weaponData.useUnderBarrel.Contains(putItem.partsData.size);
                default:
                    return false;
            }
        }
    }

    public void PutTheItem(ItemHandler item, List<ItemSlot> itemSlots)
    {
        if (itemSplit && itemSlots.Find(x => x.item != null) == null
         && item.itemData.type != ItemType.Magazine && item.TotalCount > 1
         && onSlots.Count > 0)
        {
            ItemSplit();
        }
        else
        {
            if (onSlot != null && onSlot.item != null && onSlots.Count > 0)
            {
                var itemNesting = onSlot.item != item && onSlot.item.itemData.ID == item.itemData.ID
                               && onSlot.item.itemData.maxNesting > 1 && onSlot.item.TotalCount < onSlot.item.itemData.maxNesting;
                var itemMove = onSlots.Find(x => x.item != null && x.item != item) == null && onSlots.Count == item.size.x * item.size.y;
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
                    otherStorage.CheckBaseStorage(item);
                    ItemMove(item, itemSlots, true);
                }
                else
                {
                    ItemMove(item, itemSlots, false);
                }
            }
            else
            {
                var itemMove = itemSlots.Find(x => x.item != null && x.item != item) == null
                            && itemSlots.Count == item.size.x * item.size.y;
                var storageInTheStorage = itemSlots.Count > 0 && itemSlots[0].myStorage != null
                                      && (itemSlots[0].myStorage.type == MyStorageType.Rig && item.itemData.type == ItemType.Rig
                                       || itemSlots[0].myStorage.type == MyStorageType.Backpack && item.itemData.type == ItemType.Backpack);
                if (itemMove && !storageInTheStorage)
                {
                    otherStorage.CheckBaseStorage(item);
                    ItemMove(item, itemSlots, true);
                }
                else
                {
                    ItemMove(item, itemSlots, false);
                }
            }

            item.targetImage.color = Color.clear;
            holdingItem = null;
            onSlots.Clear();
            InactiveSampleItem();
        }

        void ItemNesting()
        {
            UnequipItem(item);
            onSlot.item.SetItemSlots(DataUtility.slot_onItemColor);
            var newTotal = onSlot.item.TotalCount + item.TotalCount;
            var maxValue = onSlot.item.itemData.maxNesting;
            if (maxValue >= newTotal)
            {
                onSlot.item.SetTotalCount(newTotal);
                item.DisableItem();
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
            otherStorage.UpdateStorageInfo(onSlot.item);
        }

        void ItemSplit()
        {
            for (int i = activePopUp.Count - 1; i >= 0; i--)
            {
                activePopUp[i].ClosePopUp();
            }

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

    public void ItemMove(ItemHandler item, List<ItemSlot> itemSlots, bool value)
    {
        switch (value)
        {
            case true:
                ItemRegistration();
                item.SetItemSlots(null, DataUtility.slot_noItemColor);
                item.itemSlots = new List<ItemSlot>(itemSlots);

                for (int i = 0; i < item.itemSlots.Count; i++)
                {
                    var itemSlot = item.itemSlots[i];
                    itemSlot.item = item;
                    itemSlot.SetItemSlot(DataUtility.slot_onItemColor);
                }
                UnequipItem(item);

                item.SetItemRotation(item.rotation);
                item.SetItemScale(false);
                item.ChangeRectPivot(false);
                item.transform.SetParent(item.itemSlots[0].transform, false);
                item.transform.localPosition = Vector3.zero;
                break;
            case false:
                if (item.equipSlot != null)
                {
                    item.equipSlot.slotText.enabled = false;
                    switch (item.equipSlot.type)
                    {
                        case EquipType.MainWeapon1:
                            item.equipSlot.countText.enabled = true;
                            item.equipSlot.chamberImage.enabled = true;
                            break;
                        case EquipType.MainWeapon2:
                            item.equipSlot.countText.enabled = true;
                            item.equipSlot.chamberImage.enabled = true;
                            break;
                        case EquipType.SubWeapon:
                            item.equipSlot.countText.enabled = true;
                            item.equipSlot.chamberImage.enabled = true;
                            break;
                        case EquipType.Magazine:
                            item.equipSlot.countText.enabled = true;
                            item.equipSlot.chamberImage.enabled = false;
                            break;
                        default:
                            item.equipSlot.countText.enabled = false;
                            item.equipSlot.chamberImage.enabled = false;
                            break;
                    }
                    item.SetItemRotation(false);
                    item.SetItemScale(true);
                    item.ChangeRectPivot(true);
                    item.transform.SetParent(item.equipSlot.transform, false);
                    item.transform.localPosition = Vector3.zero;
                }
                else
                {
                    if (item.itemSlots.Count == 0) Debug.Log($"{item.name}: no itemSlots");

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

        void ItemRegistration()
        {
            if (itemSlots.Count == 0) return;
            if (gameMgr == null) return;
            if (gameMgr.playerList.Count == 0) return;
            //if (otherStorage.storageInfos.Count == 0) return;

            otherStorage.SetFloorStorage(gameMgr.playerList[0].currentNode);
            var storageInfo = otherStorage.storageInfos[otherStorage.tabIndex];
            if (itemSlots[0].otherStorage != null && storageInfo.itemList.Find(x => x.itemData.serialID == item.itemData.serialID) == null)
            {
                otherStorage.AddItemInStorageInfo(storageInfo, itemSlots[0].slotIndex, item);
            }
            else if (itemSlots[0].otherStorage == null && item.itemSlots.Count > 0)
            {
                var find = storageInfo.itemList.Find(x => x.itemData.serialID == item.itemData.serialID);
                if (find != null)
                {
                    storageInfo.itemList.Remove(find);
                }
            }
        }
    }

    private void ItemMoveCancel(ItemHandler item, List<ItemSlot> itemSlots)
    {
        var splitPopUp = activePopUp.Find(x => x.state == PopUpState.Split);
        if (splitPopUp != null) splitPopUp.ClosePopUp();

        onSlot = null;
        if (holdingItem != null)
        {
            for (int i = 0; i < onSlots.Count; i++)
            {
                var onSlot = onSlots[i];
                if (onSlot.item != null)
                {
                    onSlot.SetItemSlot(DataUtility.slot_onItemColor);
                }
                else
                {
                    onSlot.SetItemSlot(DataUtility.slot_noItemColor);
                }
            }

            if (onEquip != null)
            {
                onEquip.PointerExit_EquipSlot();
                onEquip = null;
            }
            ItemMove(item, itemSlots, false);
            item.targetImage.color = Color.clear;
            holdingItem = null;
            InactiveSampleItem();
        }
        onSlots.Clear();
    }

    /// <summary>
    /// 아이템 장착
    /// </summary>
    /// <param name="putItem"></param>
    /// <param name="equipSlot"></param>
    public void EquipItem(ItemHandler putItem, EquipSlot equipSlot)
    {
        if (equipSlot.CheckEquip(putItem) && equipSlot != putItem.equipSlot)
        {
            switch (putItem.itemData.type)
            {
                case ItemType.Head:
                    EquipProcess();
                    equipSlot.SetItemCount();
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        playerCtr.AddArmor(putItem);
                    }
                    break;
                case ItemType.Body:
                    EquipProcess();
                    equipSlot.SetItemCount();
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        playerCtr.AddArmor(putItem);
                    }
                    break;
                case ItemType.Rig:
                    EquipProcess();
                    if (equipSlot.myStorage != null && putItem.rigData != null)
                    {
                        equipSlot.myStorage.SetStorageSize(putItem.rigData.storageSize);
                    }
                    break;
                case ItemType.Backpack:
                    EquipProcess();
                    if (equipSlot.myStorage != null && putItem.backpackData != null)
                    {
                        equipSlot.myStorage.SetStorageSize(putItem.backpackData.storageSize);
                    }
                    break;
                case ItemType.MainWeapon:
                    EquipProcess();
                    equipSlot.SetItemCount();
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        playerCtr.AddWeapon(putItem, equipSlot);
                    }
                    break;
                case ItemType.SubWeapon:
                    EquipProcess();
                    equipSlot.SetItemCount();
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var playerCtr = gameMgr.playerList[0];
                        playerCtr.AddWeapon(putItem, equipSlot);
                    }
                    break;
                case ItemType.Bullet:
                    Equip_BulletType();
                    break;
                case ItemType.Magazine:
                    Equip_MagazineType();
                    break;
                case ItemType.Muzzle:
                    Equip_WeaponPartsType();
                    break;
                case ItemType.Sight:
                    Equip_WeaponPartsType();
                    break;
                case ItemType.Attachment:
                    Equip_WeaponPartsType();
                    break;
                case ItemType.UnderBarrel:
                    Equip_WeaponPartsType();
                    break;
                default:
                    break;
            }

            if (activePopUp.Contains(equipSlot.popUp))
            {
                equipSlot.popUp.item.SetPartsSample();
                equipSlot.popUp.PopUp_ItemInformation(equipSlot.popUp.item);
            }
        }
        else if (putItem.equipSlot != null)
        {
            putItem.equipSlot.slotText.enabled = false;
            switch (putItem.equipSlot.type)
            {
                case EquipType.MainWeapon1:
                    putItem.equipSlot.countText.enabled = true;
                    putItem.equipSlot.chamberImage.enabled = true;
                    break;
                case EquipType.MainWeapon2:
                    putItem.equipSlot.countText.enabled = true;
                    putItem.equipSlot.chamberImage.enabled = true;
                    break;
                case EquipType.SubWeapon:
                    putItem.equipSlot.countText.enabled = true;
                    putItem.equipSlot.chamberImage.enabled = true;
                    break;
                case EquipType.Magazine:
                    putItem.equipSlot.countText.enabled = true;
                    putItem.equipSlot.chamberImage.enabled = false;
                    break;
                default:
                    putItem.equipSlot.countText.enabled = false;
                    putItem.equipSlot.chamberImage.enabled = false;
                    break;
            }
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
        putItem.targetImage.raycastTarget = true;
        putItem.targetImage.color = Color.clear;

        holdingItem = null;
        InactiveSampleItem();

        if (gameMgr != null && gameMgr.playerList.Count > 0)
        {
            var player = gameMgr.playerList[0];
            player.SetTotalWeight();
        }

        void EquipProcess()
        {
            otherStorage.CheckBaseStorage(putItem);
            UnequipItem(putItem);
            equipSlot.item = putItem;
            equipSlot.slotText.enabled = false;

            putItem.countText.enabled = false;
            putItem.chamberImage.enabled = false;
            putItem.equipSlot = equipSlot;
            putItem.SetItemSlots(null, DataUtility.slot_noItemColor);
            putItem.ChangeRectPivot(true);
            putItem.SetItemRotation(false);
            putItem.transform.SetParent(equipSlot.transform, false);
            putItem.transform.localPosition = Vector3.zero;
            putItem.targetImage.raycastTarget = true;
            putItem.SetItemScale(true);
            putItem.itemSlots.Clear();
        }

        void Equip_BulletType()
        {
            List<ItemSlot> itemSlots = null;
            switch (equipSlot.type)
            {
                case EquipType.MainWeapon1:
                    QuickEquip(equipSlot.item, putItem);
                    break;
                case EquipType.MainWeapon2:
                    QuickEquip(equipSlot.item, putItem);
                    break;
                case EquipType.SubWeapon:
                    QuickEquip(equipSlot.item, putItem);
                    break;
                case EquipType.Chamber:
                    itemSlots = new List<ItemSlot>(putItem.itemSlots);
                    EquipProcess();
                    if (putItem.TotalCount > 1)
                    {
                        var count = putItem.TotalCount - 1;
                        SetItemInStorage(putItem.itemData, count, itemSlots, false);
                    }
                    equipSlot.popUp.item.weaponData.chamberBullet = putItem.bulletData;
                    equipSlot.popUp.item.weaponData.isChamber = true;
                    equipSlot.popUp.item.FixTextTheItemCount();
                    equipSlot.SetItemCount();

                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var player = gameMgr.playerList[0];
                        player.SetAbility();
                    }
                    break;
                case EquipType.Magazine:
                    otherStorage.CheckBaseStorage(putItem);
                    UnequipItem(putItem);
                    InsertBullet(equipSlot.popUp.item, putItem);

                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var player = gameMgr.playerList[0];
                        player.SetAbility();
                    }
                    break;
                default:
                    break;
            }

        }

        void Equip_MagazineType()
        {
            switch (equipSlot.type)
            {
                case EquipType.MainWeapon1:
                    QuickEquip(equipSlot.item, putItem);
                    break;
                case EquipType.MainWeapon2:
                    QuickEquip(equipSlot.item, putItem);
                    break;
                case EquipType.SubWeapon:
                    QuickEquip(equipSlot.item, putItem);
                    break;
                case EquipType.Magazine:
                    EquipProcess();
                    equipSlot.popUp.item.weaponData.equipMag = putItem.magData;
                    equipSlot.popUp.item.weaponData.isMag = true;
                    equipSlot.popUp.item.FixTextTheItemCount();
                    equipSlot.SetItemCount();

                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var player = gameMgr.playerList[0];
                        player.SetAbility();
                        var weapon = player.weapons.Find(x => x.weaponData == equipSlot.popUp.item.weaponData);
                        if (weapon != null) weapon.SetParts();
                    }
                    break;
                default:
                    break;
            }

        }

        void Equip_WeaponPartsType()
        {
            switch (equipSlot.type)
            {
                case EquipType.MainWeapon1:
                    QuickEquip(equipSlot.item, putItem);
                    break;
                case EquipType.MainWeapon2:
                    QuickEquip(equipSlot.item, putItem);
                    break;
                case EquipType.SubWeapon:
                    QuickEquip(equipSlot.item, putItem);
                    break;
                case EquipType.Muzzle:
                    WeaponPartsSlot();
                    break;
                case EquipType.Sight:
                    WeaponPartsSlot();
                    break;
                case EquipType.Attachment:
                    WeaponPartsSlot();
                    break;
                case EquipType.UnderBarrel:
                    WeaponPartsSlot();
                    break;
                default:
                    break;
            }

            void WeaponPartsSlot()
            {
                EquipProcess();
                equipSlot.countText.enabled = false;
                equipSlot.chamberImage.enabled = false;
                if (equipSlot.popUp.item.weaponData.equipPartsList.Find(x => x.ID == putItem.partsData.ID) == null)
                {
                    equipSlot.popUp.item.weaponData.equipPartsList.Add(putItem.partsData);
                }

                if (gameMgr != null && gameMgr.playerList.Count > 0)
                {
                    var player = gameMgr.playerList[0];
                    var weapon = player.weapons.Find(x => x.weaponData == equipSlot.popUp.item.weaponData);
                    if (weapon != null) weapon.SetParts();
                }
            }
        }
    }

    /// <summary>
    /// 빠른 장착
    /// </summary>
    /// <param name="onItem"></param>
    /// <param name="putItem"></param>
    public void QuickEquip(ItemHandler onItem, ItemHandler putItem)
    {
        otherStorage.CheckBaseStorage(putItem);
        UnequipItem(putItem);
        onItem.SetItemSlots(DataUtility.slot_onItemColor);
        onItem.targetImage.raycastTarget = true;
        switch (putItem.itemData.type)
        {
            case ItemType.Bullet:
                switch (onItem.itemData.type)
                {
                    case ItemType.MainWeapon:
                        WeaponType();
                        break;
                    case ItemType.SubWeapon:
                        WeaponType();
                        break;
                    case ItemType.Magazine:
                        InsertBullet(onItem, putItem);
                        break;
                    default:
                        break;
                }
                break;
            case ItemType.Magazine:
                onItem.weaponData.equipMag = putItem.magData;
                onItem.weaponData.isMag = true;
                onItem.FixTextTheItemCount();
                onItem.SetPartsSample();
                putItem.DisableItem();
                break;
            case ItemType.Muzzle:
                onItem.weaponData.equipPartsList.Add(putItem.partsData);
                onItem.SetPartsSample();
                putItem.DisableItem();
                break;
            case ItemType.Sight:
                onItem.weaponData.equipPartsList.Add(putItem.partsData);
                onItem.SetPartsSample();
                putItem.DisableItem();
                break;
            case ItemType.Attachment:
                onItem.weaponData.equipPartsList.Add(putItem.partsData);
                onItem.SetPartsSample();
                putItem.DisableItem();
                break;
            case ItemType.UnderBarrel:
                onItem.weaponData.equipPartsList.Add(putItem.partsData);
                onItem.SetPartsSample();
                putItem.DisableItem();
                break;
            default:
                break;
        }

        var popUp = activePopUp.Find(x => x.state == PopUpState.ItemInformation && x.item == onItem);
        if (popUp != null) popUp.PopUp_ItemInformation(popUp.item);

        if (gameMgr != null && gameMgr.playerList.Count > 0)
        {
            var player = gameMgr.playerList[0];
            player.SetAbility();
            var weapon = player.weapons.Find(x => x.weaponData == onItem.weaponData);
            if (weapon != null) weapon.SetParts();
            player.SetTotalWeight();
        }

        void WeaponType()
        {
            if (holdingItem != null && holdingItem == putItem)
            {
                if (!onItem.weaponData.isChamber && onItem.weaponData.magType != MagazineType.Cylinder)
                {
                    onItem.weaponData.chamberBullet = putItem.bulletData;
                    onItem.weaponData.isChamber = true;
                    putItem.ResultTotalCount(-1);
                }

                if (onItem.weaponData.isMag && putItem.TotalCount > 0)
                {
                    InsertBullet(onItem, putItem);
                }
                else
                {
                    if (putItem.TotalCount == 0)
                    {
                        putItem.DisableItem();
                    }
                    else
                    {
                        putItem.transform.SetParent(putItem.itemSlots[0].transform, false);
                        putItem.transform.localPosition = Vector3.zero;
                    }
                    onItem.FixTextTheItemCount();
                }
            }
            else
            {
                var reloadNum = gameMgr.uiMgr.GetAmmoIcon().value;
                for (int i = 0; i < reloadNum; i++)
                {
                    if (!onItem.weaponData.isChamber && onItem.weaponData.magType != MagazineType.Cylinder)
                    {
                        onItem.weaponData.chamberBullet = putItem.bulletData;
                        onItem.weaponData.isChamber = true;
                    }
                    else
                    {
                        onItem.weaponData.equipMag.loadedBullets.Add(putItem.bulletData);
                    }
                }

                if (reloadNum < putItem.TotalCount)
                {
                    putItem.ResultTotalCount(-reloadNum);
                }
                else
                {
                    putItem.DisableItem();
                }
                onItem.FixTextTheItemCount();
            }

            if (gameMgr != null && gameMgr.playerList.Count > 0)
            {
                var player = gameMgr.playerList[0];
                player.SetAbility();
            }
        }
    }

    private void InsertBullet(ItemHandler onItem, ItemHandler putItem)
    {
        if (onItem.itemData == null) return;

        MagazineDataInfo magData = null;
        switch (onItem.itemData.type)
        {
            case ItemType.MainWeapon:
                if (!onItem.weaponData.isMag) return;

                magData = onItem.weaponData.equipMag;
                break;
            case ItemType.SubWeapon:
                if (!onItem.weaponData.isMag) return;

                magData = onItem.weaponData.equipMag;
                break;
            case ItemType.Magazine:
                magData = onItem.magData;
                break;
            default:
                break;
        }
        if (magData == null) return;

        if (magData.magSize >= magData.loadedBullets.Count + putItem.TotalCount)
        {
            for (int i = 0; i < putItem.TotalCount; i++)
            {
                magData.loadedBullets.Add(putItem.bulletData);
            }
            putItem.DisableItem();
        }
        else
        {
            var num = magData.magSize - magData.loadedBullets.Count;
            for (int i = 0; i < num; i++)
            {
                magData.loadedBullets.Add(putItem.bulletData);
            }
            putItem.transform.SetParent(putItem.itemSlots[0].transform, false);
            putItem.transform.localPosition = Vector3.zero;
            putItem.ResultTotalCount(-num);
        }
        onItem.FixTextTheItemCount();
    }

    /// <summary>
    /// 아이템 장착해제
    /// </summary>
    /// <param name="item"></param>
    public void UnequipItem(ItemHandler item)
    {
        if (item.equipSlot != null)
        {
            ItemHandler onItem;
            switch (item.itemData.type)
            {
                case ItemType.Head:
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var player = gameMgr.playerList[0];
                        player.RemoveArmor(item.armorData.ID, item.equipSlot);
                    }
                    break;
                case ItemType.Body:
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var player = gameMgr.playerList[0];
                        player.RemoveArmor(item.armorData.ID, item.equipSlot);
                    }
                    break;
                case ItemType.Rig:
                    var rigStorage = item.equipSlot.myStorage;
                    if (rigStorage != null)
                    {
                        var inItemSlots = rigStorage.itemSlots.FindAll(x => x.item != null);
                        for (int i = 0; i < inItemSlots.Count; i++)
                        {
                            var itemSlot = inItemSlots[i];
                            if (itemSlot.item == null) continue;

                            MoveItemInMyStorage(itemSlot.item, otherStorage.itemSlots);
                        }
                        rigStorage.SetStorageSize(Vector2Int.zero);
                    }
                    break;
                case ItemType.Backpack:
                    var backpackStorage = item.equipSlot.myStorage;
                    if (backpackStorage != null)
                    {
                        var inItemSlots = backpackStorage.itemSlots.FindAll(x => x.item != null);
                        for (int i = 0; i < inItemSlots.Count; i++)
                        {
                            var itemSlot = inItemSlots[i];
                            if (itemSlot.item == null) continue;

                            MoveItemInMyStorage(itemSlot.item, otherStorage.itemSlots);
                        }
                        backpackStorage.SetStorageSize(Vector2Int.zero);
                    }
                    break;
                case ItemType.MainWeapon:
                    //item.SetLoadedBulletCount();
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var player = gameMgr.playerList[0];
                        player.RemoveWeapon(item.weaponData.ID, item.equipSlot);
                    }
                    break;
                case ItemType.SubWeapon:
                    //item.SetLoadedBulletCount();
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var player = gameMgr.playerList[0];
                        player.RemoveWeapon(item.weaponData.ID, item.equipSlot);
                    }
                    break;
                case ItemType.Bullet:
                    onItem = item.equipSlot.popUp.item;
                    if (onItem.weaponData.magType == MagazineType.IntMagazine && onItem.weaponData.equipMag.loadedBullets.Count > 0)
                    {
                        var chamberBullet = onItem.weaponData.equipMag.loadedBullets[^1];
                        onItem.weaponData.chamberBullet = chamberBullet;
                        onItem.weaponData.equipMag.loadedBullets.Remove(chamberBullet);
                    }
                    else
                    {
                        onItem.weaponData.chamberBullet = null;
                        onItem.weaponData.isChamber = false;
                    }
                    item.countText.enabled = true;
                    item.equipSlot.popUp.item.FixTextTheItemCount();
                    break;
                case ItemType.Magazine:
                    onItem = item.equipSlot.popUp.item;
                    onItem.weaponData.equipMag = null;
                    onItem.weaponData.isMag = false;
                    onItem.FixTextTheItemCount();
                    if (gameMgr != null && gameMgr.playerList.Count > 0)
                    {
                        var player = gameMgr.playerList[0];
                        player.SetAbility();
                        var weapon = player.weapons.Find(x => x.weaponData == onItem.weaponData);
                        if (weapon != null) weapon.SetParts();
                    }
                    break;
                default:
                    if (item.partsData != null && item.partsData.type != WeaponPartsType.None)
                    {
                        var find = item.equipSlot.popUp.item.weaponData.equipPartsList.Find(x => x.ID == item.partsData.ID);
                        item.equipSlot.popUp.item.weaponData.equipPartsList.Remove(find);
                        if (gameMgr != null && gameMgr.playerList.Count > 0)
                        {
                            var player = gameMgr.playerList[0];
                            player.SetAbility();
                            var weapon = player.weapons.Find(x => x.weaponData == item.equipSlot.popUp.item.weaponData);
                            if (weapon != null) weapon.SetParts();
                        }
                    }
                    break;
            }

            //if (gameMgr != null && gameMgr.playerList.Count > 0
            //&& (item.itemData.type == ItemType.MainWeapon || item.itemData.type == ItemType.SubWeapon))
            //{
            //    var playerCtr = gameMgr.playerList[0];
            //    //var weaponName = item.weaponData.GetWeaponName(item.equipSlot.type);
            //    playerCtr.RemoveWeapon(item.weaponData.ID, item.equipSlot);
            //}

            item.equipSlot.item = null;
            item.equipSlot.SetItemCount();
            if (activePopUp.Contains(item.equipSlot.popUp))
            {
                item.equipSlot.popUp.item.SetPartsSample();
                item.equipSlot.popUp.PopUp_ItemInformation(item.equipSlot.popUp.item);
            }
            item.equipSlot = null;
            item.FixTextTheItemCount();

        }

        if (gameMgr != null && gameMgr.playerList.Count > 0)
        {
            var player = gameMgr.playerList[0];
            player.SetTotalWeight();
        }
    }

    /// <summary>
    /// 범위 내의 아이템슬롯 검색
    /// </summary>
    /// <param name="itemSlots"></param>
    /// <param name="itemSize"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public List<ItemSlot> FindAllMultiSizeSlots(List<ItemSlot> itemSlots, Vector2Int itemSize, Vector2Int startIndex)
    {
        var setSlots = itemSlots.FindAll(x => x.gameObject.activeSelf
                                           && x.slotIndex.x >= startIndex.x
                                           && x.slotIndex.y >= startIndex.y
                                           && x.slotIndex.x < startIndex.x + itemSize.x
                                           && x.slotIndex.y < startIndex.y + itemSize.y);

        return setSlots;
    }

    /// <summary>
    /// 샘플 아이템 활성화
    /// </summary>
    /// <param name="item"></param>
    public void ActiveSampleItem(ItemHandler item)
    {
        sampleItem.transform.SetParent(item.transform.parent, false);
        sampleItem.transform.localPosition = Vector3.zero;
        sampleItem.SetSampleItemInfo(item);
    }

    /// <summary>
    /// 팝업창을 가져옴
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 활성된 팝업을 제거
    /// </summary>
    /// <param name="removePopUp"></param>
    public void RemoveActivePopUp(PopUp_Inventory removePopUp)
    {
        removePopUp.item = null;
        removePopUp.state = PopUpState.None;
        removePopUp.gameObject.SetActive(false);
        activePopUp.Remove(removePopUp);

        ResetActivePopUp();
    }

    /// <summary>
    /// 활성된 팝업을 초기화
    /// </summary>
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

    /// <summary>
    /// 샘플 아이템 비활성화
    /// </summary>
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

    /// <summary>
    /// 캔버스 간 거리를 가져옴
    /// </summary>
    /// <returns></returns>
    public int GetCanvasDistance()
    {
        if (gameMgr != null)
        {
            return (int)gameMenuUI.planeDistance;
        }
        else
        {
            return 0;
        }
    }

    public void CreateStorageItems(StorageInfo storage, List<StartingItemDataInfo> startingItems)
    {
        for (int i = 0; i < startingItems.Count; i++)
        {
            var itemInfo = startingItems[i];
            var itemData = dataMgr.itemData.itemInfos.Find(x => x.ID == itemInfo.itemID);
            if (itemData == null) continue;

            var item = items.Find(x => !x.gameObject.activeSelf);
            item.SetItemInfo(itemData, itemInfo.createNum, itemData.addOption);
            otherStorage.PutInItemOfStorage(storage, item);
        }
    }

    public void SetLootStorage()
    {
        otherStorage.storageInfos.Clear();
        var lootStorage = new StorageInfo()
        {
            storageName = "전리품",
            type = StorageInfo.StorageType.Reward,
            nodePos = Vector2Int.zero,
            slotSize = DataUtility.floorSlotSize,
        };
        otherStorage.storageInfos.Add(lootStorage);

        for (int i = 0; i < dataMgr.gameData.floorStorages.Count; i++)
        {
            var floorStorage = dataMgr.gameData.floorStorages[i];
            for (int j = 0; j < floorStorage.itemList.Count; j++)
            {
                var itemData = floorStorage.itemList[j];
                var item = items.Find(x => !x.gameObject.activeSelf);
                item.SetItemInfo(itemData);
                item.SetItemRotation(itemData.rotation);
                otherStorage.DropItmeOnTheReword(item);
            }
        }
        dataMgr.gameData.floorStorages.Clear();
    }

    /// <summary>
    /// 결과창 UI 설정
    /// </summary>
    /// <param name="value"></param>
    public void SetResultUI(bool value)
    {
        switch (value)
        {
            case true:
                SetStorageUI(true);
                //for (int i = 0; i < gameMgr.enemyList.Count; i++)
                //{
                //    var dropTableData = gameMgr.enemyList[i].dropTableData;
                //    var itemLevel = Random.Range(dropTableData.minItemLevel, dropTableData.maxItemLevel);

                //    // Equipment
                //    if (dropTableData.dropInfo_equipment.itemMaxNum > 0 && dropTableData.dropInfo_equipment.TotalPercentage > 0)
                //    {
                //        SetLootItem("Equipment", dropTableData.dropInfo_equipment);
                //    }

                //    // Expendable
                //    if (dropTableData.dropInfo_expendable.itemMaxNum > 0 && dropTableData.dropInfo_expendable.TotalPercentage > 0)
                //    {
                //        //SetLootItem("Expendable", dropTableData.dropInfo_expendable);
                //    }

                //    // Ingredient
                //    if (dropTableData.dropInfo_ingredient.itemMaxNum > 0 && dropTableData.dropInfo_ingredient.TotalPercentage > 0)
                //    {
                //        //SetLootItem("Ingredient", dropTableData.dropInfo_ingredient);
                //    }
                //}

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

        //void SetLootItem(string classType, DropTable dropTable)
        //{
        //    var itemNum = Random.Range(dropTable.itemMinNum, dropTable.itemMaxNum);
        //    for (int j = 0; j < itemNum; j++)
        //    {
        //        var rarity = GetRarity(dropTable);
        //        var itemDatas = dataMgr.itemData.itemInfos.FindAll(x => x.setDropTable && x.rarity == rarity && ItemClassification(classType, x.type));
        //        if (itemDatas.Count == 0) break;

        //        var itemData = itemDatas[Random.Range(0, itemDatas.Count)];
        //        SetItemInStorage(itemData, 1, otherStorage.itemSlots, true);
        //    }
        //}

        //ItemRarity GetRarity(DropTable dropTable)
        //{
        //    var percentage = Random.Range(0, dropTable.TotalPercentage);
        //    if (percentage < dropTable.LowGrade)
        //    {
        //        return ItemRarity.LowGrade;
        //    }
        //    else if (percentage < dropTable.Nomal)
        //    {
        //        return ItemRarity.Nomal;
        //    }
        //    else if (percentage < dropTable.MiddleGrade)
        //    {
        //        return ItemRarity.MiddleGrade;
        //    }
        //    else if (percentage < dropTable.HighGrade)
        //    {
        //        return ItemRarity.HighGrade;
        //    }
        //    else if (percentage < dropTable.Advanced)
        //    {
        //        return ItemRarity.Advanced;
        //    }
        //    else
        //    {
        //        return ItemRarity.Set;
        //    }
        //}

        //bool ItemClassification(string classType, ItemType type)
        //{
        //    switch (classType)
        //    {
        //        case "Equipment":
        //            if (type == ItemType.Head) return true;
        //            if (type == ItemType.Body) return true;
        //            if (type == ItemType.Rig) return true;
        //            if (type == ItemType.Backpack) return true;
        //            if (type == ItemType.MainWeapon) return true;
        //            if (type == ItemType.SubWeapon) return true;
        //            if (type == ItemType.Magazine) return true;
        //            if (type == ItemType.Muzzle) return true;
        //            if (type == ItemType.Sight) return true;
        //            if (type == ItemType.FrontHandle) return true;
        //            if (type == ItemType.Attachment) return true;
        //            if (type == ItemType.UnderBarrel) return true;
        //            break;
        //        case "Expendable":
        //            break;
        //        case "Ingredient":
        //            break;
        //        default:
        //            break;
        //    }

        //    return false;
        //}
    }

    public void SetOtherStorage(FieldNode node)
    {
        if (otherStorage.components.activeSelf) return;

        otherStorage.storageInfos.Clear();
        var baseStorages = dataMgr.gameData.baseStorages;
        //var storageInfos = new List<StorageInfo>();
        if (node != null)
        {
            var storageInfo = baseStorages.Find(x => x.nodePos == node.nodePos);
            if (storageInfo != null) otherStorage.storageInfos.Add(storageInfo);
        }
        var currentNode = gameMgr.playerList[0].currentNode;
        //otherStorage.storageInfos = storageInfos.Union(baseStorages.FindAll(x => x.nodePos.x <= currentNode.nodePos.x + 1 && x.nodePos.x >= currentNode.nodePos.x - 1
        //                                                                      && x.nodePos.y <= currentNode.nodePos.y + 1 && x.nodePos.y >= currentNode.nodePos.y - 1)).ToList();

        if (gameMgr.gameState != GameState.Result)
        {
            for (int i = 0; i < baseStorages.Count; i++)
            {
                var baseStorage = baseStorages[i];
                if (baseStorage.nodePos.x <= currentNode.nodePos.x + 1 && baseStorage.nodePos.x >= currentNode.nodePos.x - 1
                 && baseStorage.nodePos.y <= currentNode.nodePos.y + 1 && baseStorage.nodePos.y >= currentNode.nodePos.y - 1
                 && !otherStorage.storageInfos.Contains(baseStorage))
                {
                    otherStorage.storageInfos.Add(baseStorage);
                }
            }
            otherStorage.SetFloorStorage(currentNode);
        }
    }

    public void SetStorageUI(bool value)
    {
        switch (value)
        {
            case true:
                if (otherStorage.components.activeSelf) return;

                otherStorage.components.SetActive(true);
                otherStorage.ActiveTabButtons(otherStorage.storageInfos.Count);
                otherStorage.GetStorageInfo(0);
                showMenu = true;
                break;
            case false:
                otherStorage.components.SetActive(false);
                otherStorage.DeactiveTabButtons();
                showMenu = false;
                break;
        }
    }

    public void Button_Status()
    {
        if (state == GameMenuState.Status) return;

        TurnOffGameMenuUI();
        StatusProcess();
    }

    public void Button_Status_Main1()
    {
        var player = gameMgr.playerList[0];
        var weaponData = player.weapons.Find(x => x.equipSlot.type == EquipType.MainWeapon1).weaponData;
        ChangeAbilityTexts(player.ability, weaponData);
    }

    public void Button_Status_Main2()
    {
        var player = gameMgr.playerList[0];
        var weaponData = player.weapons.Find(x => x.equipSlot.type == EquipType.MainWeapon2).weaponData;
        ChangeAbilityTexts(player.ability, weaponData);
    }

    public void Button_Status_Sub()
    {
        var player = gameMgr.playerList[0];
        var weaponData = player.weapons.Find(x => x.equipSlot.type == EquipType.SubWeapon).weaponData;
        ChangeAbilityTexts(player.ability, weaponData);
    }

    public void Button_Inventory()
    {
        if (state == GameMenuState.Inventory) return;

        TurnOffGameMenuUI();
        InventoryProcess();
    }

    public void Button_Result_Next()
    {
        if (otherStorage.storageInfos[otherStorage.tabIndex].itemList.Count > 0)
        {
            popUp_warning.SetWarning(WarningState.DeleteDropItems);
        }
        else
        {
            gameMgr.NextMap();
        }
    }

    public void Button_Result_Return()
    {
        if (otherStorage.storageInfos[otherStorage.tabIndex].itemList.Count > 0)
        {
            popUp_warning.SetWarning(WarningState.DeleteDropItems);
        }
        else
        {
            gameMgr.ReturnBase();
        }
    }

    public void Button_Storage_Close()
    {
        SetStorageUI(false);
    }
}
