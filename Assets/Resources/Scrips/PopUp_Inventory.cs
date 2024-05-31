using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public List<TextMeshProUGUI> infoTexts;
        public List<GameObject> samples;
        public List<EquipSlot> equipSlots;

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
            infoTexts = FindAllInformationTexts(),
            samples = FindAllSamples(),
            equipSlots = FindAllEquipSlots(),
        };

        gameObject.SetActive(false);

        List<TextMeshProUGUI> FindAllInformationTexts()
        {
            var infoTexts = transform.Find("ItemInformation/Texts").GetComponentsInChildren<TextMeshProUGUI>();
            for (int i = 0; i < infoTexts.Length; i++)
            {
                var infoText = infoTexts[i];
                infoText.gameObject.SetActive(false);
            }

            return infoTexts.ToList();
        }

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

        List<EquipSlot> FindAllEquipSlots()
        {
            var equipSlots = transform.Find("ItemInformation/Slots").GetComponentsInChildren<EquipSlot>();
            for (int i = 0; i < equipSlots.Length; i++)
            {
                var equipSlot = equipSlots[i];
                equipSlot.SetComponents(invenMgr);
            }

            return equipSlots.ToList();
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
        for (int i = 0; i < itemInfo.infoTexts.Count; i++)
        {
            var infoText = itemInfo.infoTexts[i];
            infoText.gameObject.SetActive(false);
        }

        var item = invenMgr.selectItem;
        switch (item.itemData.type)
        {
            case ItemType.MainWeapon:
                WeaponInfo();
                break;
            case ItemType.Sight:
                WeaponPartsInfo();
                break;
            default:
                break;
        }

        void WeaponInfo()
        {
            string[] labels =
            {
                "무게",
                "RPM",
                "사거리",
                "경계각",
                "정확도",
                "안정성",
                "반동",
                "행동소모"
            };
            string[] values =
            {
                $"{item.itemData.weight}",
                $"{item.weaponData.RPM}",
                $"{item.weaponData.range}",
                $"{item.weaponData.watchAngle}°",
                $"{item.weaponData.MOA}",
                $"{item.weaponData.stability}",
                $"{item.weaponData.rebound}",
                $"{item.weaponData.actionCost}"
            };

            for (int i = 0; i < labels.Length; i++)
            {
                var labelName = itemInfo.infoTexts[i * 2];
                labelName.text = labels[i];
                labelName.gameObject.SetActive(true);

                var valueText = itemInfo.infoTexts[i * 2 + 1];
                valueText.text = values[i];
                valueText.gameObject.SetActive(true);
            }

            EquipType[] type =
            {
                EquipType.Chamber,
                EquipType.Magazine,
                EquipType.Muzzle,
                EquipType.Sight,
                EquipType.UnderRail,
                EquipType.Rail,
            };

            for (int i = 0; i < itemInfo.equipSlots.Count; i++)
            {
                var equipSlot = itemInfo.equipSlots[i];
                if (i >= type.Length)
                {
                    equipSlot.gameObject.SetActive(false);
                    continue;
                }

                switch (type[i])
                {
                    case EquipType.Chamber:
                        equipSlot.slotText.text = "약실";
                        equipSlot.gameObject.SetActive(true);
                        break;
                    case EquipType.Muzzle:
                        SetWeaponPartSlot("총구", equipSlot, type[i], item.weaponData.useMuzzle);
                        break;
                    case EquipType.Sight:
                        SetWeaponPartSlot("조준경", equipSlot, type[i], item.weaponData.useSight);
                        break;
                    case EquipType.UnderRail:
                        SetWeaponPartSlot("하부", equipSlot, type[i], item.weaponData.useUnderRail);
                        break;
                    case EquipType.Rail:
                        SetWeaponPartSlot("레일", equipSlot, type[i], item.weaponData.useRail);
                        break;
                    default:
                        equipSlot.gameObject.SetActive(false);
                        break;
                }
            }

            void SetWeaponPartSlot(string slotText, EquipSlot equipSlot, EquipType type, List<WeaponPartsSize> sizeList)
            {
                if (sizeList[0] == WeaponPartsSize.None)
                {
                    equipSlot.gameObject.SetActive(false);
                }
                else
                {
                    slotText += "\n<size=12>";
                    for (int i = 0; i < sizeList.Count; i++)
                    {
                        if (i > 0)
                        {
                            slotText += ", ";
                        }
                        switch (sizeList[i])
                        {
                            case WeaponPartsSize.Small:
                                slotText += "S";
                                break;
                            case WeaponPartsSize.Medium:
                                slotText += "M";
                                break;
                            case WeaponPartsSize.Large:
                                slotText += "L";
                                break;
                            default:
                                break;
                        }
                    }
                    slotText += "</size>";
                    equipSlot.slotText.text = slotText;
                    equipSlot.gameObject.SetActive(true);
                }
            }
        }

        void WeaponPartsInfo()
        {
            string[] labels =
            {
                "무게",
                "RPM",
                "사거리",
                "경계각",
                "정확도",
                "안정성",
                "반동",
                "행동소모"
            };
            string[] values =
            {
                $"{item.itemData.weight}",
                $"{item.partsData.RPM}",
                $"{item.partsData.range}",
                $"{item.partsData.watchAngle}°",
                $"{item.partsData.MOA}",
                $"{item.partsData.stability}",
                $"{item.partsData.rebound}",
                $"{item.partsData.actionCost}"
            };

            for (int i = 0; i < labels.Length; i++)
            {
                var labelName = itemInfo.infoTexts[i * 2];
                labelName.text = labels[i];
                labelName.gameObject.SetActive(true);

                var valueText = itemInfo.infoTexts[i * 2 + 1];
                valueText.text = values[i];
                valueText.gameObject.SetActive(true);
            }
        }
    }
    #endregion
}
