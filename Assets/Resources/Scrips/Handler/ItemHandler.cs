using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public enum ItemType
{
    None,
    MainWeapon,
    SubWeapon,
    Magazine,
    Rig,
    Backpack,
}

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
    private List<ItemSample> samples = new List<ItemSample>();

    [Header("--- Assignment Variable---")]
    public ItemType type;
    public Vector2Int size = new Vector2Int(1, 1);
    [Space(5f)]

    public ItemSlot itemSlot;
    public List<ItemSlot> itemSlots = new List<ItemSlot>();
    [HideInInspector] public Vector2Int pivotIndex;
    [SerializeField] private Vector2 movePivot;
    [SerializeField] private int sampleIndex;

    public void SetComponents(InventoryManager _invenMgr, bool isSample)
    {
        invenMgr = _invenMgr;
        rect = GetComponent<RectTransform>();
        targetImage = transform.Find("BackGround").GetComponent<Image>();
        SetSamples();

        rect.sizeDelta = new Vector2Int(DataUtility.itemSize * size.x, DataUtility.itemSize * size.y);

        void SetSamples()
        {
            var samplesTf = transform.Find("Sample");
            for (int i = 0; i < samplesTf.childCount; i++)
            {
                var sample = new ItemSample();
                sample.index = i;
                sample.sampleObject = samplesTf.GetChild(i).gameObject;
                sample.meshs = sample.sampleObject.GetComponentsInChildren<MeshRenderer>().ToList();
                samples.Add(sample);
            }
        }
    }

    public void SetItemInfo(ItemType _type, Vector2Int _size, int _sampleIndex)
    {
        type = _type;
        size = _size;
        sampleIndex = _sampleIndex;
        switch (type)
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

        var activeSample = samples.Find(x => x.sampleObject.activeSelf);
        if (activeSample.sampleObject != null) activeSample.sampleObject.SetActive(false);
        samples[sampleIndex].sampleObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void SetItemInfo(ItemType _type, Vector2Int _size, int _sampleIndex, Vector3 _rot)
    {
        type = _type;
        size = _size;
        sampleIndex = _sampleIndex;
        switch (type)
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

        var activeSample = samples.Find(x => x.sampleObject.activeSelf);
        if (activeSample.sampleObject != null) activeSample.sampleObject.SetActive(false);
        samples[sampleIndex].sampleObject.SetActive(true);
        samples[sampleIndex].sampleObject.transform.localRotation = Quaternion.Euler(_rot);
        gameObject.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        targetImage.raycastTarget = false;
        var color = DataUtility.slot_onItemColor;
        color.a = 100 / 255f;
        targetImage.color = color;

        //if (size == new Vector2Int(1, 1))
        //{
        //    itemSlot.SetSlotColor(Color.white);
        //}
        //else
        //{
        //    for (int i = 0; i < itemSlots.Count; i++)
        //    {
        //        var itemSlot = itemSlots[i];
        //        itemSlot.SetSlotColor(Color.white);
        //    }
        //}

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

        if (size == new Vector2Int(1, 1))
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
