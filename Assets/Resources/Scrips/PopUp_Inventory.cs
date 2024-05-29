using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Progress;

public class PopUp_Inventory : MonoBehaviour
{
    private enum State
    {
        None,
        Split,
    }

    private struct Split
    {
        public GameObject uiObject;
        public Slider slider;
        public TextMeshProUGUI countText;
    }

    [Header("---Access Script---")]
    private InventoryManager invenMgr;

    [Header("---Access Component---")]
    public TextMeshProUGUI topText;
    [Space(5f)]

    private Split split;

    [Header("--- Assignment Variable---")]
    [SerializeField] private State state;
    [SerializeField] private ItemHandler item;
    [SerializeField] private ItemSlot itemSlot;

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

        topText = transform.Find("Top/Text").GetComponent<TextMeshProUGUI>();

        split = new Split()
        {
            uiObject = transform.Find("Split").gameObject,
            slider = transform.Find("Split/Slider").GetComponent<Slider>(),
            countText = transform.Find("Split/Count/Text").GetComponent<TextMeshProUGUI>(),
        };
        split.uiObject.SetActive(false);

        gameObject.SetActive(false);
    }

    public void PopUp_Split(ItemHandler _item, ItemSlot _itemSlot)
    {
        gameObject.SetActive(true);
        topText.text = "아이템 나누기";
        state = State.Split;

        split.uiObject.SetActive(true);
        item = _item;
        itemSlot = _itemSlot;
        split.slider.maxValue = item.TotalCount;
        split.slider.value = 0;
        split.countText.text = "0";
    }

    public void Button_PopUp_Close()
    {
        switch (state)
        {
            case State.Split:
                invenMgr.InactiveSampleItem();
                if (itemSlot != null)
                {
                    itemSlot.SetSlotColor(Color.white);
                }
                itemSlot = null;
                break;
            default:
                break;
        }
        gameObject.SetActive(false);
        state = State.None;
    }

    public void Button_PopUp_Split_Accept()
    {
        invenMgr.InactiveSampleItem();
        if (split.slider.value == item.TotalCount)
        {
            invenMgr.PutTheItem(item, itemSlot);
        }
        else if (split.slider.value > 0)
        {
            item.ResultTotalCount((int)-split.slider.value);
            invenMgr.SetItemInStorage(item.itemData, (int)split.slider.value, itemSlot);
        }
        else
        {
            itemSlot.SetSlotColor(Color.white);
        }
        split.uiObject.SetActive(false);
        gameObject.SetActive(false);
        state = State.None;
    }

    public void OnValue_PopUp_Split()
    {
        split.countText.text = $"{split.slider.value}";
    }
}
