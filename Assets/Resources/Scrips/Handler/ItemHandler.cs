using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[System.Serializable]
public struct ItemSample
{
    public int index;
    public GameObject sampleObject;
    public List<MeshRenderer> meshs;
}

public class ItemHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("---Access Script---")]
    public InventoryManager invenMgr;

    [Header("---Access Component---")]
    public RectTransform rect;
    [HideInInspector] public Image targetImage;
    [HideInInspector] public TextMeshProUGUI countText;
    [SerializeField] private List<ItemSample> samples = new List<ItemSample>();

    [Header("--- Assignment Variable---")]
    public ItemDataInfo itemData;
    public Vector2Int size = new Vector2Int(1, 1);
    public int totalCount;
    [HideInInspector] public bool rotation;
    [Space(5f)]

    public ItemSlot itemSlot;
    public List<ItemSlot> itemSlots = new List<ItemSlot>();
    [HideInInspector] public Vector2Int pivotIndex;
    [SerializeField] private Vector2 movePivot;
    [SerializeField] private int sampleIndex;

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;
        rect = GetComponent<RectTransform>();
        targetImage = transform.Find("BackGround").GetComponent<Image>();
        countText = transform.Find("Count").GetComponent<TextMeshProUGUI>();
        SetSamples();
        rect.sizeDelta = new Vector2Int(DataUtility.itemSize, DataUtility.itemSize);

        void SetSamples()
        {
            var samplesTf = transform.Find("Sample");
            for (int i = 0; i < samplesTf.childCount; i++)
            {
                var sample = new ItemSample();
                sample.index = i;
                sample.sampleObject = samplesTf.GetChild(i).gameObject;
                sample.sampleObject.SetActive(false);
                sample.meshs = sample.sampleObject.GetComponentsInChildren<MeshRenderer>().ToList();
                samples.Add(sample);
            }
        }
    }

    public void SetItemInfo(ItemDataInfo _itemData, int count)
    {
        itemData = _itemData;
        size = _itemData.size;
        totalCount = count;
        if (itemData.maxNesting == 1)
        {
            countText.enabled = false;
        }
        else
        {
            countText.enabled = true;
            countText.text = $"{totalCount}";
        }

        sampleIndex = itemData.index;
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
        SetItemRotation(false);

        var activeSample = samples.Find(x => x.sampleObject.activeSelf && x.sampleObject != samples[sampleIndex].sampleObject);
        if (activeSample.sampleObject != null)
        {
            activeSample.sampleObject.SetActive(false);
        }
        samples[sampleIndex].sampleObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void SetSampleItemInfo(ItemDataInfo _itemData, bool rotation)
    {
        itemData = _itemData;
        size = itemData.size;
        sampleIndex = _itemData.index;
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

        var activeSample = samples.Find(x => x.sampleObject.activeSelf);
        if (activeSample.sampleObject != null)
        {
            activeSample.sampleObject.SetActive(false);
        }
        samples[sampleIndex].sampleObject.SetActive(true);
        gameObject.SetActive(true);
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

        var sample = GetSample();
        var itemRot = sample.sampleObject.transform.localRotation.eulerAngles;
        if (rotation)
        {
            itemRot.x += 90f;
        }
        sample.sampleObject.transform.localRotation = Quaternion.Euler(itemRot);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        targetImage.raycastTarget = false;
        var color = DataUtility.slot_onItemColor;
        color.a = 100 / 255f;
        targetImage.color = color;

        invenMgr.TakeTheItem(this);
        FollowMouse();
    }

    public void OnDrag(PointerEventData eventData)
    {
        FollowMouse();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (invenMgr == null) return;

        if (itemData.size == new Vector2Int(1, 1))
        {
            invenMgr.PutTheItem(this, invenMgr.onSlot);
        }
        else
        {
            invenMgr.PutTheItem(this, invenMgr.onSlots);
        }
    }

    public void FollowMouse()
    {
        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.localPosition.z);
        var worldPos = invenMgr.invenCam.ScreenToWorldPoint(new Vector3(mousePos.x + movePivot.x, mousePos.y + movePivot.y, invenMgr.GetCanvasDistance()));
        transform.position = worldPos;
    }

    public ItemSample GetSample()
    {
        return samples[sampleIndex];
    }
}
