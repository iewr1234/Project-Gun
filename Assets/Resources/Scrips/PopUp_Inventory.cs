using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Progress;

public enum PopUpState
{
    None,
    Split,
    ItemInformation,
}

public class PopUp_Inventory : MonoBehaviour
{
    [System.Serializable]
    private struct Split
    {
        public GameObject uiObject;
        public Slider slider;
        public TextMeshProUGUI countText;
    }

    [System.Serializable]
    private struct ItemInformation
    {
        public GameObject uiObject;
        public Transform samplesTf;
        public List<TextMeshProUGUI> infoTexts;
        public List<GameObject> samples;
        public List<GameObject> partsSamples;
        public List<EquipSlot> equipSlots;

        public GameObject activeSample;
    }

    [Header("---Access Script---")]
    private GameMenuManager gameMenuMgr;

    [Header("---Access Component---")]
    public TextMeshProUGUI topText;
    [Space(5f)]

    [SerializeField] private Split split;
    [SerializeField] private ItemInformation itemInfo;

    [Header("--- Assignment Variable---")]
    public PopUpState state;
    public ItemHandler item;

    [HideInInspector] public List<TextMeshProUGUI> optionTexts = new List<TextMeshProUGUI>();
    [HideInInspector] public List<ItemSlot> itemSlots = new List<ItemSlot>();
    [HideInInspector] public int index;

    public void SetComponents(GameMenuManager _gameMenuMgr)
    {
        gameMenuMgr = _gameMenuMgr;

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
            equipSlots = FindAllEquipSlots(),
        };
        FindAllSamples();

        optionTexts = transform.Find("ItemInformation/Options").GetComponentsInChildren<TextMeshProUGUI>().ToList();

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

        List<EquipSlot> FindAllEquipSlots()
        {
            var equipSlots = transform.Find("ItemInformation/Slots").GetComponentsInChildren<EquipSlot>();
            for (int i = 0; i < equipSlots.Length; i++)
            {
                var equipSlot = equipSlots[i];
                equipSlot.SetComponents(gameMenuMgr, this);
            }

            return equipSlots.ToList();
        }

        void FindAllSamples()
        {
            var samples = new List<GameObject>();
            var partsSamples = new List<GameObject>();
            for (int i = 0; i < itemInfo.samplesTf.childCount; i++)
            {
                var sample = _samplesTf.GetChild(i).gameObject;
                var weapon = sample.GetComponent<Weapon>();
                if (weapon != null)
                {
                    List<MeshFilter> partsObjects = weapon.GetWeaponPartsObjects();
                    for (int j = 0; j < partsObjects.Count; j++)
                    {
                        MeshFilter parts = partsObjects[j];
                        partsSamples.Add(parts.gameObject);
                    }
                }
                sample.SetActive(false);
                samples.Add(sample);
            }
            itemInfo.samples = samples;
            itemInfo.partsSamples = partsSamples;
        }
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        Button_PopUp_Close();
    //    }
    //}

    #region Split
    public void PopUp_Split(ItemHandler _item, List<ItemSlot> _itemSlots)
    {
        gameObject.SetActive(true);
        //transform.localPosition = DataUtility.popUp_defaultPos_split;
        topText.text = "아이템 나누기";
        state = PopUpState.Split;

        split.uiObject.SetActive(true);
        itemInfo.uiObject.SetActive(false);

        item = _item;
        itemSlots = _itemSlots;
        split.slider.value = 1;
        split.slider.minValue = 1;
        split.slider.maxValue = item.TotalCount - 1;
        split.countText.text = "1";
    }

    public void Button_PopUp_Split_Accept()
    {
        gameMenuMgr.InactiveSampleItem();
        if (split.slider.value == item.TotalCount)
        {
            gameMenuMgr.PutTheItem(item, itemSlots);
        }
        else if (split.slider.value > 0)
        {
            item.ResultTotalCount((int)-split.slider.value);
            item.SetColorOfBackImage(new Color(0f, 0f, 0f, 240 / 255f));
            gameMenuMgr.SetItemInStorage(item.itemData, (int)split.slider.value, false, itemSlots);
        }
        else
        {
            item.SetItemSlots(DataUtility.slot_noItemColor);
            item.SetItemCount(item.TotalCount);
            item.SetColorOfBackImage(new Color(0f, 0f, 0f, 240 / 255f));
            for (int i = 0; i < itemSlots.Count; i++)
            {
                var itemSlot = itemSlots[i];
                itemSlot.SetItemSlot(DataUtility.slot_noItemColor);
            }
        }
        split.uiObject.SetActive(false);
        gameObject.SetActive(false);

        item = null;
        state = PopUpState.None;
    }

    public void OnValue_PopUp_Split()
    {
        if (split.countText == null) return;

        split.countText.text = $"{split.slider.value}";
    }
    #endregion

    #region ItemInformation
    public void PopUp_ItemInformation(ItemHandler _item)
    {
        switch (gameObject.activeSelf)
        {
            case true:
                for (int i = 0; i < itemInfo.equipSlots.Count; i++)
                {
                    var equipSlot = itemInfo.equipSlots[i];
                    if (equipSlot.item == null) continue;

                    equipSlot.item.DisableItem();
                }
                break;
            case false:
                gameObject.SetActive(true);
                break;
        }

        item = _item;
        topText.text = $"{item.itemData.itemName}";
        state = PopUpState.ItemInformation;

        itemInfo.uiObject.SetActive(true);
        split.uiObject.SetActive(false);

        if (itemInfo.activeSample != null)
        {
            itemInfo.activeSample.SetActive(false);
        }

        string sampleName = item.GetSampleName();
        itemInfo.activeSample = itemInfo.samples.Find(x => x.name == sampleName);
        itemInfo.activeSample.SetActive(true);
        for (int i = 0; i < itemInfo.infoTexts.Count; i++)
        {
            var infoText = itemInfo.infoTexts[i];
            infoText.gameObject.SetActive(false);
        }
        for (int i = 0; i < itemInfo.equipSlots.Count; i++)
        {
            var equipSlot = itemInfo.equipSlots[i];
            equipSlot.gameObject.SetActive(false);
        }

        switch (item.itemData.type)
        {
            case ItemType.MainWeapon:
                WeaponInfo();
                break;
            case ItemType.SubWeapon:
                WeaponInfo();
                break;
            case ItemType.Sight:
                WeaponPartsInfo();
                break;
            default:
                break;
        }
        OptionInfo();

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
                $"{item.weaponData.GetWeaponWeight()}",
                $"{item.weaponData.RPM}",
                $"{item.weaponData.range}",
                $"{item.weaponData.watchAngle}°",
                $"{item.weaponData.MOA}",
                $"{item.weaponData.stability}",
                $"{item.weaponData.rebound}",
                $"{item.weaponData.actionCost_shot}"
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
                EquipType.Attachment,
                EquipType.UnderBarrel,
            };

            for (int i = 0; i < type.Length; i++)
            {
                var equipSlot = itemInfo.equipSlots[i];
                equipSlot.model = item.weaponData.model;
                equipSlot.caliber = item.weaponData.caliber;
                switch (item.weaponData.magType)
                {
                    case MagazineType.IntMagazine:
                        equipSlot.intMagMax = item.weaponData.equipMag.magSize;
                        break;
                    case MagazineType.Cylinder:
                        equipSlot.intMagMax = item.weaponData.equipMag.magSize;
                        break;
                    default:
                        break;
                }

                equipSlot.type = type[i];
                switch (equipSlot.type)
                {
                    case EquipType.Chamber:
                        SetChamberSlot(equipSlot, item.weaponData);
                        break;
                    case EquipType.Magazine:
                        SetMagazineSlot(equipSlot, item.weaponData);
                        break;
                    case EquipType.Muzzle:
                        SetWeaponPartSlot("총구", equipSlot, type[i], item.weaponData.useMuzzle);
                        break;
                    case EquipType.Sight:
                        SetWeaponPartSlot("조준경", equipSlot, type[i], item.weaponData.useSight);
                        break;
                    case EquipType.Attachment:
                        SetWeaponPartSlot("부착물", equipSlot, type[i], item.weaponData.useAttachment);
                        break;
                    case EquipType.UnderBarrel:
                        SetWeaponPartSlot("하부", equipSlot, type[i], item.weaponData.useUnderBarrel);
                        break;
                    default:
                        equipSlot.gameObject.SetActive(false);
                        break;
                }
            }
            SetPartsSample();

            void SetChamberSlot(EquipSlot equipSlot, WeaponDataInfo weaponData)
            {
                switch (weaponData.magType)
                {
                    case MagazineType.Magazine:
                        equipSlot.slotText.text = "약실";
                        equipSlot.gameObject.SetActive(true);
                        break;
                    case MagazineType.IntMagazine:
                        equipSlot.slotText.text = "약실";
                        equipSlot.gameObject.SetActive(true);
                        break;
                    default:
                        equipSlot.gameObject.SetActive(false);
                        break;
                }
            }

            void SetMagazineSlot(EquipSlot equipSlot, WeaponDataInfo weaponData)
            {
                switch (weaponData.magType)
                {
                    case MagazineType.Magazine:
                        equipSlot.slotText.text = "탄창";
                        equipSlot.gameObject.SetActive(true);
                        break;
                    case MagazineType.IntMagazine:
                        equipSlot.slotText.text = "내부탄창";
                        equipSlot.gameObject.SetActive(true);
                        break;
                    case MagazineType.Cylinder:
                        equipSlot.slotText.text = "실린더";
                        equipSlot.gameObject.SetActive(true);
                        break;
                    default:
                        equipSlot.gameObject.SetActive(false);
                        break;
                }
            }

            void SetWeaponPartSlot(string slotText, EquipSlot equipSlot, EquipType type, List<WeaponPartsSize> sizeList)
            {
                if (sizeList.Count == 0)
                {
                    equipSlot.gameObject.SetActive(false);
                }
                else
                {
                    equipSlot.sizeList = sizeList;
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
                $"{item.partsData.weight}",
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

        void OptionInfo()
        {
            if (!item.itemData.addOption)
            {
                for (int i = 0; i < optionTexts.Count; i++)
                {
                    var optionText = optionTexts[i];
                    optionText.text = $"옵션{i + 1} : ---";
                }
                return;
            }

            var optionMax = 4;
            var index = 0;
            for (int i = 0; i < item.itemData.itemOptions.Count; i++)
            {
                var itemOption = item.itemData.itemOptions[i];
                var optionText = optionTexts[i];
                var sign = itemOption.value < 0 ? "-" : "+";
                //optionText.text = $"옵션{i + 1} : {itemOption.type} {sign}{itemOption.value}";
                optionText.text = $"옵션{i + 1} : {itemOption.scriptText}";
                index++;
            }

            if (index == optionMax) return;

            for (int i = index; i < optionTexts.Count; i++)
            {
                var optionText = optionTexts[i];
                optionText.text = $"옵션{i + 1} : ---";
            }
        }
    }

    public void SetPartsSample()
    {
        var activeSamples = itemInfo.partsSamples.FindAll(x => x.activeSelf);
        for (int i = 0; i < activeSamples.Count; i++)
        {
            var activeSample = activeSamples[i];
            activeSample.SetActive(false);
        }

        var partsList = new List<WeaponPartsDataInfo>();
        for (int i = 0; i < itemInfo.equipSlots.Count; i++)
        {
            var equipSlot = itemInfo.equipSlots[i];
            switch (equipSlot.type)
            {
                case EquipType.Chamber:
                    var bulletData = item.weaponData.chamberBullet;
                    if (item.weaponData.isChamber && equipSlot.CheckEquip(bulletData))
                    {
                        gameMenuMgr.SetItemInEquipSlot(bulletData, 1, equipSlot);
                        var smaples = itemInfo.partsSamples.FindAll(x => x.name == bulletData.ID);
                        for (int j = 0; j < smaples.Count; j++)
                        {
                            var smaple = smaples[j];
                            smaple.SetActive(true);
                        }
                    }
                    else
                    {
                        equipSlot.slotText.enabled = true;
                        equipSlot.countText.enabled = false;
                        equipSlot.chamberImage.enabled = false;
                    }
                    break;
                case EquipType.Magazine:
                    switch (item.weaponData.magType)
                    {
                        case MagazineType.Magazine:
                            if (item.weaponData.isMag && equipSlot.CheckEquip(item.weaponData.equipMag))
                            {
                                if (equipSlot.item == null) gameMenuMgr.SetItemInEquipSlot(item.weaponData.equipMag, 1, equipSlot);

                                var smaples = itemInfo.partsSamples.FindAll(x => x.name == item.weaponData.equipMag.prefabName);
                                for (int j = 0; j < smaples.Count; j++)
                                {
                                    var smaple = smaples[j];
                                    smaple.SetActive(true);
                                }
                            }
                            equipSlot.SetLoadedBulletCount(item);
                            break;
                        case MagazineType.IntMagazine:
                            equipSlot.SetLoadedBulletCount(item);
                            break;
                        case MagazineType.Cylinder:
                            equipSlot.SetLoadedBulletCount(item);
                            break;
                        default:
                            equipSlot.SetItemCount();
                            break;
                    }
                    break;
                case EquipType.Muzzle:
                    Sample_WeaponPartsType(WeaponPartsType.Muzzle);
                    break;
                case EquipType.Sight:
                    Sample_WeaponPartsType(WeaponPartsType.Sight);
                    break;
                case EquipType.Attachment:
                    Sample_WeaponPartsType(WeaponPartsType.Attachment);
                    break;
                case EquipType.UnderBarrel:
                    Sample_WeaponPartsType(WeaponPartsType.UnderBarrel);
                    break;
                default:
                    break;

                    void Sample_WeaponPartsType(WeaponPartsType partsType)
                    {
                        Weapon weapon = itemInfo.activeSample.GetComponent<Weapon>();
                        if (weapon == null) return;

                        var partsData = item.weaponData.equipPartsList.Find(x => x.type == partsType);
                        switch (partsType)
                        {
                            case WeaponPartsType.Muzzle:
                                if (weapon.baseMuzzle != null) weapon.baseMuzzle.SetActive(partsData == null);
                                break;
                            case WeaponPartsType.Sight:
                                if (weapon.baseSight != null) weapon.baseSight.SetActive(partsData == null);
                                break;
                            default:
                                break;
                        }

                        if (partsData != null)
                        {
                            if (equipSlot.item == null) gameMenuMgr.SetItemInEquipSlot(partsData, 1, equipSlot);

                            var smaple = weapon.partsMfs.Find(x => x.name == partsData.prefabName);
                            smaple.gameObject.SetActive(true);

                            //var smaples = itemInfo.partsSamples.FindAll(x => x.name == partsData.prefabName);
                            //for (int j = 0; j < smaples.Count; j++)
                            //{
                            //    var smaple = smaples[j];
                            //    smaple.SetActive(true);
                            //}
                        }
                        else
                        {
                            equipSlot.slotText.enabled = true;
                            equipSlot.item = null;
                        }
                    }
            }
        }
    }
    #endregion

    public void SetPopUpPosition(Vector3 newPos)
    {
        transform.position = newPos;

        SetPopUpPosition();
    }

    private void FollowMouse()
    {
        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.localPosition.z);
        var worldPos = gameMenuMgr.gameMenuCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, gameMenuMgr.GetCanvasDistance()));
        transform.position = worldPos;

        SetPopUpPosition();
    }

    public void SetPopUpPosition()
    {
        var pos = transform.localPosition;
        if (split.uiObject.activeSelf && pos.y < -260f)
        {
            pos.y = -260f;
        }
        else if (itemInfo.uiObject.activeSelf && pos.y < 40f)
        {
            pos.y = 40f;
        }
        else if (pos.y > 440f)
        {
            pos.y = 440f;
        }

        if (pos.x > 635f)
        {
            pos.x = 635f;
        }
        else if (pos.x < -635f)
        {
            pos.x = -635f;
        }

        pos.z = -150f - (index * 150);
        transform.localPosition = pos;
    }

    public void ClosePopUp()
    {
        switch (state)
        {
            case PopUpState.Split:
                gameMenuMgr.InactiveSampleItem();
                item.SetItemSlots(DataUtility.slot_noItemColor);
                item.SetItemCount(item.TotalCount);
                item.SetColorOfBackImage(new Color(0f, 0f, 0f, 240 / 255f));
                for (int i = 0; i < itemSlots.Count; i++)
                {
                    var itemSlot = itemSlots[i];
                    itemSlot.SetItemSlot(DataUtility.slot_noItemColor);
                }
                itemSlots.Clear();
                break;
            case PopUpState.ItemInformation:
                for (int i = 0; i < itemInfo.equipSlots.Count; i++)
                {
                    var equipSlot = itemInfo.equipSlots[i];
                    if (equipSlot.item == null) continue;

                    equipSlot.item.DisableItem();
                }
                gameMenuMgr.selectItem = null;
                break;
            default:
                break;
        }
        gameMenuMgr.RemoveActivePopUp(this);
    }

    public void BeginDrag_PopUp()
    {
        var activePopUps = gameMenuMgr.activePopUp.FindAll(x => x.state == state);
        if (activePopUps.Count > 1 && activePopUps[^1] != this)
        {
            gameMenuMgr.activePopUp.Remove(this);
            gameMenuMgr.activePopUp.Add(this);
            gameMenuMgr.ResetActivePopUp();
        }

        FollowMouse();
    }

    public void OnDrag_PopUp()
    {
        FollowMouse();
    }

    public void Button_PopUp_Close()
    {
        ClosePopUp();
    }
}
