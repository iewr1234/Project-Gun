using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("---Access Script---")]
    public GameMenuManager gameMenuMgr;
    [Space(5f)]

    public RigDataInfo rigData;
    public BackpackDataInfo backpackData;
    public WeaponDataInfo weaponData;
    public BulletDataInfo bulletData;
    public MagazineDataInfo magData;
    public WeaponPartsDataInfo partsData;
    public GrenadeDataInfo grenadeData;

    [Header("---Access Component---")]
    public RectTransform rect;
    [HideInInspector] public Image frameImage;
    [HideInInspector] public Image targetImage;
    [HideInInspector] public TextMeshProUGUI countText;

    private Transform samplesTf;
    [SerializeField] private List<GameObject> samples = new List<GameObject>();
    [SerializeField] private List<GameObject> partsSamples = new List<GameObject>();

    [Header("--- Assignment Variable---")]
    public ItemDataInfo itemData;
    [HideInInspector] public Vector2Int size = new Vector2Int(1, 1);
    [HideInInspector] public bool rotation;

    [Space(5f)]
    [SerializeField] private int totalCount;

    [Space(5f)]
    public EquipSlot equipSlot;
    public List<ItemSlot> itemSlots;
    [HideInInspector] public Vector2Int pivotIndex;
    [SerializeField] private Vector2 movePivot;

    public GameObject activeSample;
    private int index;

    private readonly Vector3 defaultScale = new Vector3(400f, 400f, 400f);

    public void SetComponents(GameMenuManager _gameMenuMgr)
    {
        gameMenuMgr = _gameMenuMgr;
        weaponData = null;
        bulletData = null;
        magData = null;
        partsData = null;

        rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2Int(DataUtility.itemSize, DataUtility.itemSize);
        frameImage = transform.Find("Frame").GetComponent<Image>();
        frameImage.enabled = false;
        targetImage = transform.Find("BackGround").GetComponent<Image>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();

        samplesTf = transform.Find("Sample");
        for (int i = 0; i < samplesTf.childCount; i++)
        {
            var sample = samplesTf.GetChild(i).gameObject;
            if (gameObject.name == "SampleItem")
            {
                var meshs = sample.transform.GetComponentsInChildren<MeshRenderer>().ToList();
                for (int j = 0; j < meshs.Count; j++)
                {
                    var mesh = meshs[j];
                    mesh.material = new Material(mesh.material);
                    mesh.material.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                    mesh.material.color = new Color(1f, 1f, 1f, 100 / 255f);
                }
            }

            if (sample.GetComponent<Weapon>() != null)
            {
                var partsSamples = sample.transform.Find("PartsTransform").GetComponentsInChildren<Transform>();
                for (int j = 0; j < partsSamples.Length; j++)
                {
                    var partsSample = partsSamples[j];
                    if (partsSample.CompareTag("WeaponParts"))
                    {
                        partsSample.gameObject.SetActive(false);
                        this.partsSamples.Add(partsSample.gameObject);
                    }
                }
            }
            sample.SetActive(false);
            samples.Add(sample);
        }
    }

    public void SetComponents(GameMenuManager _gameMenuMgr, int _index)
    {
        index = _index;
        transform.name = $"Item_{index}";

        gameMenuMgr = _gameMenuMgr;
        weaponData = null;
        bulletData = null;
        magData = null;
        partsData = null;

        rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2Int(DataUtility.itemSize, DataUtility.itemSize);
        frameImage = transform.Find("Frame").GetComponent<Image>();
        frameImage.enabled = false;
        targetImage = transform.Find("BackGround").GetComponent<Image>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();

        samplesTf = transform.Find("Sample");
        for (int i = 0; i < samplesTf.childCount; i++)
        {
            var sample = samplesTf.GetChild(i).gameObject;
            if (gameObject.name == "SampleItem")
            {
                var meshs = sample.transform.GetComponentsInChildren<MeshRenderer>().ToList();
                for (int j = 0; j < meshs.Count; j++)
                {
                    var mesh = meshs[j];
                    mesh.material = new Material(mesh.material);
                    mesh.material.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                    mesh.material.color = new Color(1f, 1f, 1f, 100 / 255f);
                }
            }

            if (sample.GetComponent<Weapon>() != null)
            {
                var partsSamples = sample.transform.Find("PartsTransform").GetComponentsInChildren<Transform>();
                for (int j = 0; j < partsSamples.Length; j++)
                {
                    var partsSample = partsSamples[j];
                    if (partsSample.CompareTag("WeaponParts"))
                    {
                        partsSample.gameObject.SetActive(false);
                        this.partsSamples.Add(partsSample.gameObject);
                    }
                }
            }
            sample.SetActive(false);
            samples.Add(sample);
        }
    }

    public void SetItemInfo(ItemDataInfo _itemData, int count, bool insertOption)
    {
        itemData = _itemData.CopyData();
        string sampleName = InputItemData();
        InsertItemOption();
        SetItemRotation(false);
        SetItemCount(count);

        activeSample = samples.Find(x => x.name == sampleName);
        if (activeSample == null)
        {
            Debug.LogError($"Not found Sample object: {sampleName}");
        }
        activeSample.SetActive(true);
        gameObject.SetActive(true);

        if (!gameMenuMgr.activeItem.Contains(this))
        {
            gameMenuMgr.activeItem.Add(this);
        }

        string InputItemData()
        {
            transform.name = $"Item_{index}_{itemData.itemName}";
            size = itemData.size;
            switch (itemData.type)
            {
                case ItemType.Rig:
                    var _rigData = gameMenuMgr.dataMgr.rigData.rigInfos.Find(x => x.ID == itemData.dataID);
                    rigData = _rigData.CopyData();
                    return rigData.rigName;
                case ItemType.Backpack:
                    var _backpackData = gameMenuMgr.dataMgr.backpackData.backpackInfos.Find(x => x.ID == itemData.dataID);
                    backpackData = _backpackData.CopyData();
                    return backpackData.backpackName;
                case ItemType.MainWeapon:
                    var _mainWeaponData = gameMenuMgr.dataMgr.weaponData.weaponInfos.Find(x => x.ID == itemData.dataID);
                    weaponData = _mainWeaponData.CopyData(gameMenuMgr.dataMgr);
                    SetPartsSample();
                    return weaponData.prefabName;
                case ItemType.SubWeapon:
                    var _subWeaponData = gameMenuMgr.dataMgr.weaponData.weaponInfos.Find(x => x.ID == itemData.dataID);
                    weaponData = _subWeaponData.CopyData(gameMenuMgr.dataMgr);
                    SetPartsSample();
                    return weaponData.prefabName;
                case ItemType.Bullet:
                    var _bulletData = gameMenuMgr.dataMgr.bulletData.bulletInfos.Find(x => x.ID == itemData.dataID);
                    bulletData = _bulletData.CopyData();
                    return bulletData.prefabName;
                case ItemType.Magazine:
                    var _magData = gameMenuMgr.dataMgr.magData.magInfos.Find(x => x.ID == itemData.dataID);
                    magData = _magData.CopyData();
                    var loadedBullet = gameMenuMgr.dataMgr.bulletData.bulletInfos.Find(x => x.ID == magData.loadedBulletID);
                    if (loadedBullet != null)
                    {
                        for (int i = 0; i < magData.magSize; i++)
                        {
                            magData.loadedBullets.Add(loadedBullet);
                        }
                    }
                    return magData.prefabName;
                case ItemType.Muzzle:
                    var _muzzleData = gameMenuMgr.dataMgr.partsData.partsInfos.Find(x => x.ID == itemData.dataID);
                    partsData = _muzzleData.CopyData();
                    return partsData.prefabName;
                case ItemType.Sight:
                    var _sightData = gameMenuMgr.dataMgr.partsData.partsInfos.Find(x => x.ID == itemData.dataID);
                    partsData = _sightData.CopyData();
                    return partsData.prefabName;
                case ItemType.Attachment:
                    var _attachData = gameMenuMgr.dataMgr.partsData.partsInfos.Find(x => x.ID == itemData.dataID);
                    partsData = _attachData.CopyData();
                    return partsData.prefabName;
                case ItemType.UnderBarrel:
                    var _underBarrelData = gameMenuMgr.dataMgr.partsData.partsInfos.Find(x => x.ID == itemData.dataID);
                    partsData = _underBarrelData.CopyData();
                    return partsData.prefabName;
                case ItemType.Grenade:
                    var _grenadeData = gameMenuMgr.dataMgr.grenadeData.grenadeInfos.Find(x => x.ID == itemData.dataID);
                    grenadeData = _grenadeData.CopyData();
                    return grenadeData.grenadeName;
                default:
                    return null;
            }
        }

        void InsertItemOption()
        {
            if (!insertOption) return;

            var optionSheet = gameMenuMgr.dataMgr.optionSheetData.optionSheetInfos.Find(x => x.levelInfo.minLevel < itemData.level && x.levelInfo.maxLevel >= itemData.level);
            var rankOption = optionSheet.rankOptions[(int)itemData.rarity - 1];
            AddOption(rankOption.option1_rank);
            AddOption(rankOption.option2_rank);
            AddOption(rankOption.option3_rank);
            AddOption(rankOption.option4_rank);

            void AddOption(int optionRank)
            {
                if (optionRank == 0) return;

                var options = gameMenuMgr.dataMgr.itemOptionData.itemOptionInfos.FindAll(x => x.rank == optionRank && (x.mainType == 0 || x.mainType == (int)itemData.type));
                if (options.Count == 0) return;

                var option = options[Random.Range(0, options.Count)];
                var type = option.optionType;
                var value = Random.Range(option.minValue, option.maxValue + 1);
                var itemOption = new ItemOption()
                {
                    indexName = $"{type}: {value}",
                    type = type,
                    value = value,
                    scriptText = DataUtility.GetScriptText(option.scriptText, value),
                };
                itemData.itemOptions.Add(itemOption);
            }
        }
    }

    public void SetItemInfo(StorageItemInfo storageItem)
    {
        itemData = storageItem.itemData;
        transform.name = $"Item_{index}_{itemData.itemName}";
        switch (itemData.type)
        {
            case ItemType.Rig:
                rigData = storageItem.rigData;
                break;
            case ItemType.Backpack:
                backpackData = storageItem.backpackData;
                break;
            case ItemType.MainWeapon:
                weaponData = storageItem.weaponData;
                break;
            case ItemType.SubWeapon:
                weaponData = storageItem.weaponData;
                break;
            case ItemType.Bullet:
                bulletData = storageItem.bulletData;
                break;
            case ItemType.Magazine:
                magData = storageItem.magData;
                break;
            case ItemType.Muzzle:
                partsData = storageItem.partsData;
                break;
            case ItemType.Sight:
                partsData = storageItem.partsData;
                break;
            case ItemType.Attachment:
                partsData = storageItem.partsData;
                break;
            case ItemType.UnderBarrel:
                partsData = storageItem.partsData;
                break;
            case ItemType.Grenade:
                grenadeData = storageItem.grenadeData;
                break;
            default:
                break;
        }
        SetItemRotation(false);
        SetItemCount(storageItem.totalCount);

        string sampleName = GetSampleName();
        activeSample = samples.Find(x => x.name == sampleName);
        if (activeSample == null)
        {
            Debug.LogError("Not found Sample object");
        }
        activeSample.SetActive(true);
        gameObject.SetActive(true);

        if (!gameMenuMgr.activeItem.Contains(this))
        {
            gameMenuMgr.activeItem.Add(this);
        }
    }

    public void SetItemInfo(MagazineDataInfo _magData)
    {
        itemData = gameMenuMgr.dataMgr.itemData.itemInfos.Find(x => x.dataID == _magData.ID).CopyData();
        transform.name = $"Item_{index}_{itemData.itemName}";
        size = itemData.size;
        magData = _magData.CopyData();
        var loadedBullet = gameMenuMgr.dataMgr.bulletData.bulletInfos.Find(x => x.ID == magData.loadedBulletID);
        if (magData.loadedBullets.Count == 0 && loadedBullet != null)
        {
            for (int i = 0; i < magData.magSize; i++)
            {
                magData.loadedBullets.Add(loadedBullet);
            }
        }
        countText.enabled = true;
        SetTotalCount(magData.loadedBullets.Count, magData.magSize);

        string sampleName = GetSampleName();
        activeSample = samples.Find(x => x.name == sampleName);
        if (activeSample == null)
        {
            Debug.LogError("Not found Sample object");
        }
        activeSample.SetActive(true);
        gameObject.SetActive(true);

        if (!gameMenuMgr.activeItem.Contains(this))
        {
            gameMenuMgr.activeItem.Add(this);
        }
    }

    public void SetItemInfo(BulletDataInfo _bulletData, int count)
    {
        itemData = gameMenuMgr.dataMgr.itemData.itemInfos.Find(x => x.dataID == _bulletData.ID).CopyData();
        transform.name = $"Item_{index}_{itemData.itemName}";
        size = itemData.size;
        bulletData = _bulletData.CopyData();
        countText.enabled = true;
        SetTotalCount(count);
        gameObject.SetActive(true);

        if (!gameMenuMgr.activeItem.Contains(this))
        {
            gameMenuMgr.activeItem.Add(this);
        }
    }

    public void SetSampleItemInfo(ItemHandler item)
    {
        itemData = item.itemData;
        size = itemData.size;
        switch (itemData.type)
        {
            case ItemType.MainWeapon:
                break;
            case ItemType.SubWeapon:
                break;
            case ItemType.Magazine:
                break;
            case ItemType.Rig:
                break;
            case ItemType.Backpack:
                break;
            default:
                break;
        }
        SetItemRotation(item.rotation);

        if (activeSample != null)
        {
            activeSample.SetActive(false);
        }

        string sampleName = item.GetSampleName();
        activeSample = samples.Find(x => x.name == sampleName);
        if (activeSample == null)
        {
            Debug.LogError("Not found Sample object");
        }
        SetPartsSample(item.weaponData);
        activeSample.SetActive(true);
        gameObject.SetActive(true);
    }

    public string GetSampleName()
    {
        switch (itemData.type)
        {
            case ItemType.Rig:
                return rigData.rigName;
            case ItemType.Backpack:
                return backpackData.backpackName;
            case ItemType.MainWeapon:
                return weaponData.prefabName;
            case ItemType.SubWeapon:
                return weaponData.prefabName;
            case ItemType.Bullet:
                return bulletData.prefabName;
            case ItemType.Magazine:
                return magData.prefabName;
            case ItemType.Muzzle:
                return partsData.prefabName;
            case ItemType.Sight:
                return partsData.prefabName;
            case ItemType.Attachment:
                return partsData.prefabName;
            case ItemType.UnderBarrel:
                return partsData.prefabName;
            case ItemType.Grenade:
                return grenadeData.grenadeName;
            default:
                return null;
        }
    }

    public void SetPartsSample()
    {
        var activeSamples = partsSamples.FindAll(x => x.activeSelf);
        for (int i = 0; i < activeSamples.Count; i++)
        {
            var activeSample = activeSamples[i];
            activeSample.SetActive(false);
        }

        if (itemData.type != ItemType.MainWeapon && itemData.type != ItemType.SubWeapon) return;

        if (weaponData.isMag)
        {
            var smaple = partsSamples.Find(x => x.name == weaponData.equipMag.prefabName);
            if (smaple != null) smaple.SetActive(true);
        }

        for (int i = 0; i < weaponData.equipPartsList.Count; i++)
        {
            var partsData = weaponData.equipPartsList[i];
            var smaples = partsSamples.FindAll(x => x.name == partsData.prefabName);
            for (int j = 0; j < smaples.Count; j++)
            {
                var smaple = smaples[j];
                smaple.SetActive(true);
            }
        }
    }

    public void SetPartsSample(WeaponDataInfo weaponData)
    {
        var activeSamples = partsSamples.FindAll(x => x.activeSelf);
        for (int i = 0; i < activeSamples.Count; i++)
        {
            var activeSample = activeSamples[i];
            activeSample.SetActive(false);
        }

        if (itemData.type != ItemType.MainWeapon && itemData.type != ItemType.SubWeapon) return;

        if (weaponData.isMag)
        {
            var smaple = partsSamples.Find(x => x.name == weaponData.equipMag.prefabName);
            if (smaple)
            {
                smaple.SetActive(true);
            }
        }

        for (int i = 0; i < weaponData.equipPartsList.Count; i++)
        {
            var partsData = weaponData.equipPartsList[i];
            var smaples = partsSamples.FindAll(x => x.name == partsData.prefabName);
            for (int j = 0; j < smaples.Count; j++)
            {
                var smaple = smaples[j];
                smaple.SetActive(true);
            }
        }
    }

    public void SetTotalCount(int newCount)
    {
        totalCount = newCount;
        countText.text = $"{totalCount}";
    }

    public void SetTotalCount(int newCount, int maxValue)
    {
        totalCount = newCount;
        countText.text = $"{totalCount}/{maxValue}";
    }

    public void ResultTotalCount(int value)
    {
        totalCount += value;
        countText.text = $"{totalCount}";
    }

    private void SetItemCount(int count)
    {
        switch (itemData.type)
        {
            case ItemType.MainWeapon:
                SetLoadedBulletCount();
                break;
            case ItemType.SubWeapon:
                SetLoadedBulletCount();
                break;
            case ItemType.Magazine:
                SetLoadedBulletCount();
                break;
            default:
                countText.enabled = itemData.maxNesting > 1;
                SetTotalCount(count);
                break;
        }
    }

    public void SetLoadedBulletCount()
    {
        if (equipSlot != null)
        {
            equipSlot.SetLoadedBulletCount();
            return;
        }

        switch (itemData.type)
        {
            case ItemType.MainWeapon:
                WeaponType();
                break;
            case ItemType.SubWeapon:
                WeaponType();
                break;
            case ItemType.Magazine:
                MagazineType();
                break;
            default:
                break;
        }

        void WeaponType()
        {
            var bulletNum = 0;
            var magSize = 0;
            if (weaponData.isChamber) bulletNum++;
            if (weaponData.isMag)
            {
                bulletNum += weaponData.equipMag.loadedBullets.Count;
                magSize = weaponData.equipMag.magSize;
            }

            countText.enabled = true;
            SetTotalCount(bulletNum, magSize);
        }

        void MagazineType()
        {
            countText.enabled = true;
            SetTotalCount(magData.loadedBullets.Count, magData.magSize);
        }
    }

    public void SetItemRotation(bool _rotation)
    {
        rotation = _rotation;
        size = rotation ? new Vector2Int(itemData.size.y, itemData.size.x) : itemData.size;
        if (size == new Vector2Int(1, 1))
        {
            rect.sizeDelta = new Vector2Int(DataUtility.itemSize, DataUtility.itemSize);
            pivotIndex = new Vector2Int(0, 0);
            movePivot = new Vector2(-DataUtility.itemSize / 2, DataUtility.itemSize / 2);
        }
        else
        {
            rect.sizeDelta = new Vector2Int(DataUtility.itemSize * size.x, DataUtility.itemSize * size.y);
            pivotIndex = new Vector2Int(size.x / 2, size.y / 2);
            var pivotX = (pivotIndex.x * DataUtility.itemSize) + (DataUtility.itemSize / 2);
            var pivotY = (pivotIndex.y * DataUtility.itemSize) + (DataUtility.itemSize / 2);
            movePivot = new Vector2(-pivotX, pivotY);
        }

        var itemRot = samplesTf.localRotation.eulerAngles;
        if (rotation)
        {
            itemRot.x = -90f;
        }
        else
        {
            itemRot.x = 0f;
        }
        samplesTf.localRotation = Quaternion.Euler(itemRot);
    }

    public void SetItemScale(bool value)
    {
        if (size == new Vector2Int(1, 1)) return;
        if (itemData != null && itemData.type == ItemType.Rig) return;
        if (itemData != null && itemData.type == ItemType.Backpack) return;
        if (itemData != null && itemData.type == ItemType.MainWeapon) return;
        if (itemData != null && itemData.type == ItemType.SubWeapon) return;
        if (itemData != null && itemData.type == ItemType.Bullet) return;

        switch (value)
        {
            case true:
                var maxSize = size.x >= size.y ? size.x : size.y;
                var reducedValue = 1f / maxSize;
                var reducedScale = new Vector3(defaultScale.x * reducedValue, defaultScale.y * reducedValue, defaultScale.z * reducedValue);
                samplesTf.localScale = reducedScale;
                break;
            case false:
                samplesTf.localScale = defaultScale;
                break;
        }
    }

    /// <summary>
    /// 아이템 비활성화
    /// </summary>
    /// <param name="item"></param>
    public void DisableItem()
    {
        transform.name = $"Item_{index}";
        itemData = null;
        rigData = null;
        backpackData = null;
        weaponData = null;
        bulletData = null;
        magData = null;
        partsData = null;
        grenadeData = null;

        if (equipSlot)
        {
            equipSlot.item = null;
            equipSlot = null;
        }
        SetItemScale(false);
        SetItemSlots(null, DataUtility.slot_noItemColor);
        itemSlots.Clear();
        transform.SetParent(gameMenuMgr.itemPool, false);

        if (activeSample != null)
        {
            activeSample.SetActive(false);
            activeSample = null;
        }
        gameObject.SetActive(false);
        gameMenuMgr.activeItem.Remove(this);
    }

    public void FollowMouse()
    {
        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.localPosition.z);
        var worldPos = gameMenuMgr.gameMenuCam.ScreenToWorldPoint(new Vector3(mousePos.x + movePivot.x, mousePos.y + movePivot.y, gameMenuMgr.GetCanvasDistance() - 100));
        transform.position = worldPos;
    }

    public void ChangeRectPivot(bool isEquip)
    {
        switch (isEquip)
        {
            case true:
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                break;
            case false:
                rect.anchorMax = new Vector2(0f, 1f);
                rect.anchorMin = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                break;
        }
    }

    public void SetItemSlots(Color color)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            itemSlot.SetItemSlot(color);
        }
    }

    public void SetItemSlots(ItemHandler item, Color color)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            itemSlot.item = item;
            itemSlot.SetItemSlot(color);
        }
    }

    private bool CheckItemDrag()
    {
        if (gameMenuMgr == null) return false;
        if (gameMenuMgr.popUp_warning.state != WarningState.None) return false;
        if (Input.GetMouseButton(1)) return false;

        return true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CheckItemDrag()) return;

        if (equipSlot != null)
        {
            equipSlot.slotText.enabled = true;
            ChangeRectPivot(false);
        }
        targetImage.raycastTarget = false;
        var color = DataUtility.slot_onItemColor;
        color.a = 100 / 255f;
        targetImage.color = color;

        gameMenuMgr.TakeTheItem(this);
        FollowMouse();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CheckItemDrag()) return;

        FollowMouse();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CheckItemDrag()) return;

        if (gameMenuMgr.onEquip && !gameMenuMgr.onSlot)
        {
            gameMenuMgr.EquipItem(this, gameMenuMgr.onEquip);
        }
        else
        {
            gameMenuMgr.PutTheItem(this, gameMenuMgr.onSlots);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            var splitPopUp = gameMenuMgr.activePopUp.Find(x => x.state == PopUpState.Split);
            if (splitPopUp != null) splitPopUp.ClosePopUp();

            gameMenuMgr.contextMenu.OpenTheContextMenu(this);
        }
    }

    public int TotalCount
    {
        private set { totalCount = value; }
        get { return totalCount; }
    }
}
