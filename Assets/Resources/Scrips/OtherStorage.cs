using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OtherStorage : MonoBehaviour
{
    [Header("---Access Script---")]
    public InventoryManager invenMgr;

    [Header("---Access Component---")]
    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    private GameObject components;
    private TextMeshProUGUI nameText;
    private List<Image> tabButtonImages = new List<Image>();

    [Header("--- Assignment Variable---")]
    public Vector2Int size;
    public List<StorageInfo> storageInfos = new List<StorageInfo>();
    [HideInInspector] public int tabIndex;

    private readonly Color activeColor_tab = new Color(0.78f, 0.78f, 0.78f);
    private readonly Color noneActiveColor_tab = new Color(0.52f, 0.52f, 0.52f);

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

        components = transform.Find("Components").gameObject;
        nameText = components.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var tabButtons = components.transform.Find("TapButtons").GetComponentsInChildren<Button>();
        for (int i = 0; i < tabButtons.Length; i++)
        {
            var tabButton = tabButtons[i];
            tabButtonImages.Add(tabButton.GetComponent<Image>());
            tabButton.gameObject.SetActive(false);
        }

        itemSlots = GetComponentsInChildren<ItemSlot>().ToList();
        itemSlots.Reverse();
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            var index = new Vector2Int(i % size.x, i / size.x);
            itemSlot.SetComponents(this, index);
        }
    }

    public void ItemSlotsPlacement(Vector2Int size)
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            if (i < size.x * size.y)
            {
                itemSlot.gameObject.SetActive(true);
            }
            else if (itemSlot.gameObject.activeSelf)
            {
                itemSlot.gameObject.SetActive(false);
            }
            else
            {
                break;
            }
        }
    }

    public void SetActive(bool value)
    {
        components.SetActive(value);
    }

    public void ActiveTabButtons(int value)
    {
        for (int i = 0; i < value; i++)
        {
            tabButtonImages[i].gameObject.SetActive(true);
            tabButtonImages[i].color = noneActiveColor_tab;
        }
    }

    public void DeactiveTabButtons()
    {
        for (int i = 0; i < tabButtonImages.Count; i++)
        {
            tabButtonImages[i].gameObject.SetActive(false);
        }
    }

    public void GetStorageInfo()
    {
        var storageInfo = storageInfos[tabIndex];
        nameText.text = storageInfo.storageName;
        tabButtonImages[tabIndex].color = activeColor_tab;
        ItemSlotsPlacement(storageInfo.slotSize);
    }

    public void Button_Tab(int index)
    {
        tabButtonImages[tabIndex].color = noneActiveColor_tab;
        tabIndex = index;
        GetStorageInfo();
    }
}
