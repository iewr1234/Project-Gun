using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUp_Inventory : MonoBehaviour
{
    private enum State
    {
        None,
        Split,
        ItemInformation,
    }

    private struct Split
    {
        public GameObject uiObject;
        public Slider slider;
        public TextMeshProUGUI countText;
    }

    private struct ItemInformation
    {
        public GameObject uiObject;
        public Transform samplesTf;
        public List<GameObject> samples;

        public GameObject activeSample;
    }

    [Header("---Access Script---")]
    private InventoryManager invenMgr;

    [Header("---Access Component---")]
    public TextMeshProUGUI topText;
    [Space(5f)]

    private Split split;
    private ItemInformation itemInfo;

    [Header("--- Assignment Variable---")]
    [SerializeField] private State state;
    [SerializeField] private ItemHandler item;
    [SerializeField] private ItemSlot itemSlot;

    private readonly Vector3Int defaultPos_split = new Vector3Int(0, 150, 0);
    private readonly Vector3Int defaultPos_itemInfo = new Vector3Int(0, 350, 0);

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

        var _samplesTf = transform.Find("ItemInformation/Sample");
        itemInfo = new ItemInformation()
        {
            uiObject = transform.Find("ItemInformation").gameObject,
            samplesTf = _samplesTf,
            samples = FindAllSamples(),
        };

        gameObject.SetActive(false);

        List<GameObject> FindAllSamples()
        {
            var samples = new List<GameObject>();
            for (int i = 0; i < _samplesTf.childCount; i++)
            {
                var sample = _samplesTf.GetChild(i).gameObject;
                sample.SetActive(false);
                samples.Add(sample);
            }

            return samples;
        }
    }

    #region Split
    public void PopUp_Split(ItemHandler _item, ItemSlot _itemSlot)
    {
        gameObject.SetActive(true);
        transform.localPosition = defaultPos_split;
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
            case State.ItemInformation:
                invenMgr.selectItem = null;
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
    #endregion

    #region
    public void PopUp_ItemInformation()
    {
        gameObject.SetActive(true);
        transform.localPosition = defaultPos_itemInfo;
        topText.text = $"{invenMgr.selectItem.itemData.itemName}";
        state = State.ItemInformation;

        itemInfo.uiObject.SetActive(true);
        if (itemInfo.activeSample != null)
        {
            itemInfo.activeSample.SetActive(false);
        }
        itemInfo.activeSample = itemInfo.samples.Find(x => x.name == invenMgr.selectItem.itemData.dataID);
        itemInfo.activeSample.SetActive(true);
    }
    #endregion
}
