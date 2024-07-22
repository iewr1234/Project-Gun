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
    public InventoryManager invenMgr;
    [Space(5f)]

    public WeaponDataInfo weaponData;
    public BulletDataInfo bulletData;
    public MagazineDataInfo magData;
    public WeaponPartsDataInfo partsData;

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

    private readonly Vector3 defaultScale = new Vector3(400f, 400f, 400f);

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;
        weaponData = null;
        bulletData = null;
        magData = null;
        partsData = null;

        rect = GetComponent<RectTransform>();
        frameImage = transform.Find("Frame").GetComponent<Image>();
        targetImage = transform.Find("BackGround").GetComponent<Image>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();

        samplesTf = transform.Find("Sample");
        for (int i = 0; i < samplesTf.childCount; i++)
        {
            var sample = samplesTf.GetChild(i).gameObject;
            if (gameObject.layer == LayerMask.NameToLayer("Sample"))
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

            if (sample.name[0] == 'W')
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
        size = _itemData.size;
        switch (itemData.type)
        {
            case ItemType.MainWeapon:
                var _mainWeaponData = invenMgr.dataMgr.weaponData.weaponInfos.Find(x => x.ID == itemData.dataID);
                weaponData = _mainWeaponData.CopyData(invenMgr.dataMgr);
                SetPartsSample();
                break;
            case ItemType.SubWeapon:
                var _subWeaponData = invenMgr.dataMgr.weaponData.weaponInfos.Find(x => x.ID == itemData.dataID);
                weaponData = _subWeaponData.CopyData(invenMgr.dataMgr);
                SetPartsSample();
                break;
            case ItemType.Bullet:
                var _bulletData = invenMgr.dataMgr.bulletData.bulletInfos.Find(x => x.ID == itemData.dataID);
                bulletData = _bulletData.CopyData();
                break;
            case ItemType.Magazine:
                var _magData = invenMgr.dataMgr.magData.magInfos.Find(x => x.ID == itemData.dataID);
                magData = _magData.CopyData();
                break;
            case ItemType.Sight:
                var _partsData = invenMgr.dataMgr.partsData.partsInfos.Find(x => x.ID == itemData.dataID);
                partsData = _partsData.CopyData();
                break;
            default:
                break;
        }
        InsertItemOption();

        SetItemRotation(false);
        if (itemData.type == ItemType.Magazine)
        {
            countText.enabled = true;
            SetTotalCount(magData.loadedBullets.Count);
        }
        else
        {
            countText.enabled = itemData.maxNesting > 1;
            SetTotalCount(count);
        }

        activeSample = samples.Find(x => x.name == itemData.dataID);
        if (activeSample == null)
        {
            Debug.LogError("Not found Sample object");
        }
        activeSample.SetActive(true);
        gameObject.SetActive(true);

        if (!invenMgr.activeItem.Contains(this))
        {
            invenMgr.activeItem.Add(this);
        }

        void InsertItemOption()
        {
            if (!insertOption) return;

            var optionSheet = invenMgr.dataMgr.optionSheetData.optionSheetInfos.Find(x => x.levelInfo.minLevel < itemData.level && x.levelInfo.maxLevel >= itemData.level);
            var rankOption = optionSheet.rankOptions[(int)itemData.rarity - 1];
            AddOption(rankOption.option1_rank);
            AddOption(rankOption.option2_rank);
            AddOption(rankOption.option3_rank);
            AddOption(rankOption.option4_rank);
        }

        void AddOption(int optionRank)
        {
            if (optionRank == 0) return;

            var options = invenMgr.dataMgr.itemOptionData.itemOptionInfos.FindAll(x => x.rank == optionRank && (x.mainType == 0 || x.mainType == (int)itemData.type));
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

    public void SetItemInfo(MagazineDataInfo magData)
    {
        itemData = invenMgr.dataMgr.itemData.itemInfos.Find(x => x.dataID == magData.ID);
        size = itemData.size;
        this.magData = magData.CopyData();
        countText.enabled = true;
        SetTotalCount(magData.loadedBullets.Count);

        activeSample = samples.Find(x => x.name == itemData.dataID);
        if (activeSample == null)
        {
            Debug.LogError("Not found Sample object");
        }
        activeSample.SetActive(true);
        gameObject.SetActive(true);

        if (!invenMgr.activeItem.Contains(this))
        {
            invenMgr.activeItem.Add(this);
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
        activeSample = samples.Find(x => x.name == itemData.dataID);
        if (activeSample == null)
        {
            Debug.LogError("Not found Sample object");
        }
        SetPartsSample(item.weaponData);
        activeSample.SetActive(true);
        gameObject.SetActive(true);
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

    public void SetTotalCount(int value)
    {
        totalCount = value;
        countText.text = $"{totalCount}";
    }

    public void ResultTotalCount(int value)
    {
        totalCount += value;
        countText.text = $"{totalCount}";
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
        if (itemData.type == ItemType.MainWeapon || itemData.type == ItemType.SubWeapon) return;

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

    public void FollowMouse()
    {
        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.localPosition.z);
        var worldPos = invenMgr.invenCam.ScreenToWorldPoint(new Vector3(mousePos.x + movePivot.x, mousePos.y + movePivot.y, invenMgr.GetCanvasDistance()));
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
        if (invenMgr == null) return false;
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

        invenMgr.TakeTheItem(this);
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

        if (invenMgr.onEquip && !invenMgr.onSlot)
        {
            invenMgr.EquipItem(this, invenMgr.onEquip);
        }
        else
        {
            invenMgr.PutTheItem(this, invenMgr.onSlots);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            invenMgr.contextMenu.OpenTheContextMenu(this);
        }
    }

    public int TotalCount
    {
        private set { totalCount = value; }
        get { return totalCount; }
    }
}
