using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

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
    public GameObject sampleObject;
    public List<MeshRenderer> meshs;
}

public class ItemHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("---Access Script---")]
    public InventoryManager invenMgr;
    public ItemSlot itemSlot;

    [Header("---Access Component---")]
    public RectTransform rect;
    [HideInInspector] public Image targetImage;
    private List<ItemSample> samples = new List<ItemSample>();

    [Header("--- Assignment Variable---")]
    public ItemType type;
    public Vector2Int size = new Vector2Int(1, 1);
    [HideInInspector] public Vector2 pivot;
    [HideInInspector] public int sampleIndex;

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
                sample.sampleObject = samplesTf.GetChild(i).gameObject;
                sample.meshs = sample.sampleObject.GetComponentsInChildren<MeshRenderer>().ToList();
                samples.Add(sample);
            }
        }
    }

    //private void FixedUpdate()
    //{
    //    if (itemSlot == null) return;

    //    if (invenMgr.holdingItem != this)
    //    {
    //        transform.position = itemSlot.transform.position;
    //    }
    //}

    public void SetItemInfo(ItemType _type, Vector2Int _size, int _sampleIndex)
    {
        type = _type;
        size = _size;
        sampleIndex = _sampleIndex;
        switch (type)
        {
            case ItemType.None:
                break;
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
            pivot = new Vector2(-DataUtility.itemSize / 2, DataUtility.itemSize / 2);
        }
        else
        {

        }

        var activeSample = samples.Find(x => x.sampleObject.activeSelf);
        if (activeSample.sampleObject != null) activeSample.sampleObject.SetActive(false);
        samples[sampleIndex].sampleObject.SetActive(true);
        gameObject.SetActive(true);
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
        SetItem(invenMgr.onSlot);
    }

    public void SetItem(ItemSlot itemSlot)
    {
        invenMgr.PutTheItem(this, itemSlot);
        targetImage.raycastTarget = true;
        targetImage.color = Color.clear;
    }

    private void FollowMouse()
    {
        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.localPosition.z);
        var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x + pivot.x, mousePos.y + pivot.y, invenMgr.GetCanvasDistance()));
        transform.position = worldPos;
    }
}
