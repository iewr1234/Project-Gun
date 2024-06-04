using System.Collections;
using System.Collections.Generic;
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
    public WeaponPartsDataInfo partsData;

    [Header("---Access Component---")]
    public RectTransform rect;
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

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;
        rect = GetComponent<RectTransform>();
        targetImage = transform.Find("BackGround").GetComponent<Image>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();

        samplesTf = transform.Find("Sample");
        for (int i = 0; i < samplesTf.childCount; i++)
        {
            var sample = samplesTf.GetChild(i).gameObject;
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

    public void SetItemInfo(ItemDataInfo _itemData, int count)
    {
        itemData = new ItemDataInfo(_itemData);

        size = _itemData.size;
        countText.enabled = itemData.maxNesting > 1;
        SetTotalCount(count);

        switch (itemData.type)
        {
            case ItemType.MainWeapon:
                var _weaponData = invenMgr.dataMgr.weaponData.weaponInfos.Find(x => x.ID == itemData.dataID);
                weaponData = _weaponData.Copy();
                break;
            case ItemType.Sight:
                partsData = invenMgr.dataMgr.partsData.partsInfos.Find(x => x.ID == itemData.dataID);
                break;
            default:
                break;
        }
        SetItemRotation(false);

        if (activeSample != null)
        {
            activeSample.SetActive(false);
        }
        activeSample = samples.Find(x => x.name == itemData.dataID);
        if (activeSample == null)
        {
            Debug.LogError("Not found Sample object");
        }
        activeSample.SetActive(true);
        gameObject.SetActive(true);
    }

    public void SetSampleItemInfo(ItemDataInfo _itemData, bool rotation)
    {
        itemData = _itemData;
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
        SetItemRotation(rotation);

        if (activeSample != null)
        {
            activeSample.SetActive(false);
        }
        activeSample = samples.Find(x => x.name == itemData.dataID);
        if (activeSample == null)
        {
            Debug.LogError("Not found Sample object");
        }
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

        for (int i = 0; i < weaponData.equipPartsList.Count; i++)
        {
            var partsData = weaponData.equipPartsList[i];
            var smaple = partsSamples.Find(x => x.name == partsData.ID);
            if (smaple)
            {
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Input.GetMouseButton(1)) return;

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
        if (Input.GetMouseButton(1)) return;

        FollowMouse();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (invenMgr == null) return;
        if (Input.GetMouseButton(1)) return;

        if (invenMgr.onEquip)
        {
            invenMgr.EquipItem(this, invenMgr.onEquip);
        }
        else
        {
            if (itemData.size == new Vector2Int(1, 1))
            {
                Debug.Log("1");
                invenMgr.PutTheItem(this, invenMgr.onSlot);
            }
            else
            {
                Debug.Log("2");
                invenMgr.PutTheItem(this, invenMgr.onSlots);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            invenMgr.OpenContextMenu(this);
        }
    }

    public int TotalCount
    {
        private set { totalCount = value; }
        get { return totalCount; }
    }
}
