using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---")]
    [SerializeField] private Canvas canvas;
    [HideInInspector] public GameObject playUI;
    [HideInInspector] public GameObject stageUI;
    [HideInInspector] public GameObject resultUI;

    [Header("[BottomUI]")]
    public GameObject bottomUI;
    [HideInInspector] public TextMeshProUGUI magNumText;
    public List<ActionBlock> actionBlocks;

    public ActionButton watchButton;
    public ActionButton shootButton;
    public ActionButton reloadButton;
    public ActionButton throwButton;
    public Button turnEndButton;

    //[HideInInspector] public GameObject magIcons;
    //[HideInInspector] public List<MagazineIcon> magIconList;
    //[HideInInspector] public GameObject grdIcons;
    //[HideInInspector] public List<GrenadeIcon> grdIconList;
    [HideInInspector] public GameObject ammoIcons;
    [HideInInspector] public List<AmmoIcon> ammoIconList;

    [Header("[AimUI]")]
    public GameObject aimUI;
    [HideInInspector] public TextMeshProUGUI shootNumText;
    [HideInInspector] public TextMeshProUGUI hitAccuracyText;
    [HideInInspector] public TextMeshProUGUI actionPointText;
    [HideInInspector] public Image fireRateGauge;
    [HideInInspector] public Image sightGauge;
    [HideInInspector] public Slider armorGauge;
    [HideInInspector] public Slider healthGauge;
    [HideInInspector] public Slider staminaGauge;

    [Header("--- Assignment Variable---")]
    public Button onButton;
    [HideInInspector] public int iconIndex;

    private List<StageIcon> stageIcons = new List<StageIcon>();
    private bool selcetStage;

    private readonly string aimUIGaugePath = "Sprites/SightGauge/Gauge_SightAp";

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;
        canvas = GetComponent<Canvas>();
        playUI = transform.Find("Play").gameObject;
        stageUI = transform.Find("Stage").gameObject;
        resultUI = transform.Find("Result").gameObject;

        bottomUI = playUI.transform.Find("BottomUI").gameObject;
        magNumText = bottomUI.transform.Find("MagNum").GetComponent<TextMeshProUGUI>();
        magNumText.enabled = false;
        actionBlocks = bottomUI.transform.Find("ActionPoint").GetComponentsInChildren<ActionBlock>().ToList();
        for (int i = 0; i < actionBlocks.Count; i++)
        {
            var actionBlock = actionBlocks[i];
            actionBlock.SetComponents();
        }

        //magIcons = bottomUI.transform.Find("Magazines").gameObject;
        //magIconList = magIcons.GetComponentsInChildren<MagazineIcon>().ToList();
        //for (int i = 0; i < magIconList.Count; i++)
        //{
        //    var magIcon = magIconList[i];
        //    magIcon.SetComponents();
        //}
        //magIcons.SetActive(false);
        //grdIcons = bottomUI.transform.Find("Grenades").gameObject;
        //grdIconList = grdIcons.GetComponentsInChildren<GrenadeIcon>().ToList();
        //for (int i = 0; i < grdIconList.Count; i++)
        //{
        //    var grdIcon = grdIconList[i];
        //    grdIcon.SetComponents();
        //}
        //grdIcons.SetActive(false);

        ammoIcons = bottomUI.transform.Find("AmmoIcons").gameObject;
        ammoIconList = ammoIcons.GetComponentsInChildren<AmmoIcon>().ToList();
        for (int i = 0; i < ammoIconList.Count; i++)
        {
            var ammoIcon = ammoIconList[i];
            ammoIcon.SetComponents();
        }
        ammoIcons.SetActive(false);


        aimUI = playUI.transform.Find("AimUI").gameObject;
        shootNumText = aimUI.transform.Find("ShootNum").GetComponent<TextMeshProUGUI>();
        hitAccuracyText = aimUI.transform.Find("HitAccuracy").GetComponent<TextMeshProUGUI>();
        actionPointText = aimUI.transform.Find("ActionPoint/Text").GetComponent<TextMeshProUGUI>();
        fireRateGauge = aimUI.transform.Find("FireRateGauge").GetComponent<Image>();
        sightGauge = aimUI.transform.Find("SightGauge").GetComponent<Image>();
        armorGauge = aimUI.transform.Find("TargetInfo/ArmorGauge").GetComponent<Slider>();
        healthGauge = aimUI.transform.Find("TargetInfo/HealthGauge").GetComponent<Slider>();
        staminaGauge = aimUI.transform.Find("TargetInfo/StaminaGauge").GetComponent<Slider>();
        aimUI.SetActive(false);

        var actionButtons = bottomUI.transform.Find("ActionButtons").GetComponentsInChildren<ActionButton>().ToList();
        for (int i = 0; i < actionButtons.Count; i++)
        {
            var actionButton = actionButtons[i];
            actionButton.SetComponents(gameMgr);
            switch (actionButton.type)
            {
                case GameState.Shoot:
                    shootButton = actionButton;
                    break;
                case GameState.Reload:
                    reloadButton = actionButton;
                    break;
                case GameState.Watch:
                    watchButton = actionButton;
                    break;
                case GameState.Throw:
                    throwButton = actionButton;
                    break;
                default:
                    break;
            }
        }
        turnEndButton = playUI.transform.Find("TurnEnd").GetComponent<Button>();

        stageIcons = stageUI.transform.Find("StageIcons").GetComponentsInChildren<StageIcon>().ToList();
        for (int i = 0; i < stageIcons.Count; i++)
        {
            var stageIcon = stageIcons[i];
            var stageData = gameMgr.dataMgr.stageData.stageInfos.Find(x => x.ID == "S0001");
            stageIcon.SetComponents(this, stageData);
        }
        stageUI.SetActive(false);

        resultUI.SetActive(false);
    }

    public void SetMagNum(CharacterController charCtr)
    {
        if (charCtr.ownerType != CharacterOwner.Player || charCtr.weapons.Count == 0) return;

        var weapon = charCtr.currentWeapon;
        var loadedAmmo = weapon.weaponData.equipMag.loadedBullets.Count;
        //if (weapon.weaponData.isChamber) loadedAmmo++;

        magNumText.enabled = true;
        magNumText.text = $"{loadedAmmo}/{weapon.weaponData.equipMag.magSize}";
    }

    public void SetMagNum(CharacterController charCtr, int loadedAmmo)
    {
        if (charCtr.ownerType != CharacterOwner.Player) return;

        var weapon = charCtr.currentWeapon;
        magNumText.enabled = true;
        magNumText.text = $"{loadedAmmo}/{weapon.weaponData.equipMag.magSize}";
    }

    public void SetShootNum(CharacterController charCtr)
    {
        var weapon = charCtr.currentWeapon;
        var shootNum = DataUtility.GetShootNum(charCtr.RPM, charCtr.fiarRate);

        var loadedAmmo = weapon.weaponData.equipMag.loadedBullets.Count;
        //if (weapon.weaponData.isChamber) loadedAmmo++;

        shootNumText.color = shootNum > loadedAmmo ? Color.red : Color.black;
        shootNumText.text = $"{shootNum}";
        SetHitAccuracy(charCtr);
    }

    public void SetHitAccuracy(CharacterController charCtr)
    {
        var targetInfo = charCtr.targetList[charCtr.targetIndex];
        var hitAccuracy = DataUtility.GetHitAccuracy(targetInfo);
        if (hitAccuracy > 100)
        {
            hitAccuracy = 100;
        }
        else if (hitAccuracy < 0)
        {
            hitAccuracy = 0;
        }
        hitAccuracyText.text = $"{hitAccuracy}%";
    }

    public void SetActiveAmmoIcon(bool value)
    {
        switch (value)
        {
            case true:
                ammoIcons.SetActive(true);
                break;
            case false:
                ammoIcons.SetActive(false);
                var activeIcons = ammoIconList.FindAll(x => x.gameObject.activeSelf);
                for (int i = 0; i < activeIcons.Count; i++)
                {
                    var activeIcon = activeIcons[i];
                    activeIcon.Initialize();
                }
                break;
        }
    }

    //public void SetActiveMagazineIcon(bool value)
    //{
    //    switch (value)
    //    {
    //        case true:
    //            magIcons.SetActive(true);
    //            break;
    //        case false:
    //            magIcons.SetActive(false);
    //            var activeIcons = magIconList.FindAll(x => x.gameObject.activeSelf);
    //            if (activeIcons.Count > 0)
    //            {
    //                for (int i = 0; i < activeIcons.Count; i++)
    //                {
    //                    var activeIcon = activeIcons[i];
    //                    activeIcon.gameObject.SetActive(false);
    //                }
    //            }
    //            break;
    //    }
    //}

    //public void SetActiveGrenadeIcon(bool value)
    //{
    //    switch (value)
    //    {
    //        case true:
    //            grdIcons.SetActive(true);
    //            break;
    //        case false:
    //            grdIcons.SetActive(false);
    //            var activeIcons = grdIconList.FindAll(x => x.gameObject.activeSelf);
    //            if (activeIcons.Count > 0)
    //            {
    //                for (int i = 0; i < activeIcons.Count; i++)
    //                {
    //                    var activeIcon = activeIcons[i];
    //                    activeIcon.gameObject.SetActive(false);
    //                }
    //            }
    //            break;
    //    }
    //}

    public void SetActionPoint_Bottom(CharacterController charCtr)
    {
        if (charCtr.ownerType != CharacterOwner.Player) return;

        for (int i = 0; i < actionBlocks.Count; i++)
        {
            var actionBlock = actionBlocks[i];
            if (i < charCtr.action)
            {
                actionBlock.SetActionState(ActionBlockState.Active);
            }
            else
            {
                actionBlock.SetActionState(ActionBlockState.Inactive);
            }
        }
    }

    public void SetUsedActionPoint_Bottom(CharacterController charCtr, int usedAction)
    {
        if (charCtr.ownerType != CharacterOwner.Player) return;

        var activeBlocks = actionBlocks.FindAll(x => x.state == ActionBlockState.Active);
        activeBlocks.Reverse();
        for (int i = 0; i < activeBlocks.Count; i++)
        {
            var actionBlock = activeBlocks[i];
            if (i < usedAction)
            {
                activeBlocks[i].SetActionState(ActionBlockState.Used);
            }
            else
            {
                actionBlock.SetActionState(ActionBlockState.Active);
            }
        }
    }

    public void SetActiveAimUI(CharacterController charCtr, bool value)
    {
        playUI.SetActive(value);
        aimUI.SetActive(value);
        bottomUI.SetActive(!value);
        turnEndButton.gameObject.SetActive(!value);
        if (value)
        {
            SetShootNum(charCtr);
            SetActionPoint_Aim(charCtr);
        }
        else
        {
            fireRateGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + "1");
            sightGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + "1");
        }
    }

    public void SetActionPoint_Aim(CharacterController charCtr)
    {
        var totalCost = charCtr.currentWeapon.weaponData.actionCost + charCtr.fiarRate + charCtr.sightRate;
        if (totalCost > charCtr.action)
        {
            actionPointText.color = Color.red;
        }
        else
        {
            actionPointText.color = Color.white;
        }
        actionPointText.text = $"<size=36>{totalCost}</size><color=#D2D2D2>/{charCtr.maxAction} AP</color>";
    }

    public void SetFireRateGauge(CharacterController charCtr)
    {
        charCtr.fiarRate++;
        if (charCtr.fiarRate > DataUtility.shootRateMax)
        {
            charCtr.fiarRate = 0;
        }

        fireRateGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{charCtr.fiarRate + 1}");
        var totalCost = charCtr.currentWeapon.weaponData.actionCost + charCtr.fiarRate + charCtr.sightRate;
        SetUsedActionPoint_Bottom(charCtr, totalCost);
        SetShootNum(charCtr);
        SetActionPoint_Aim(charCtr);
    }

    public void SetSightGauge(CharacterController charCtr)
    {
        charCtr.sightRate++;
        if (charCtr.sightRate > DataUtility.shootRateMax)
        {
            charCtr.sightRate = 0;
        }

        sightGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{charCtr.sightRate + 1}");
        var totalCost = charCtr.currentWeapon.weaponData.actionCost + charCtr.fiarRate + charCtr.sightRate;
        SetUsedActionPoint_Bottom(charCtr, totalCost);
        SetHitAccuracy(charCtr);
        SetActionPoint_Aim(charCtr);
    }

    public void SetTargetInfo(TargetInfo targetInfo)
    {
        SetHitAccuracy(targetInfo.shooter);
        if (targetInfo.target.armor != null)
        {
            armorGauge.maxValue = targetInfo.target.armor.maxDurability;
            armorGauge.value = targetInfo.target.armor.durability;
        }
        healthGauge.maxValue = targetInfo.target.maxHealth;
        healthGauge.value = targetInfo.target.health;
        staminaGauge.maxValue = targetInfo.target.maxStamina;
        staminaGauge.value = targetInfo.target.stamina;
    }

    public void EnterTheStage(StageDataInfo stageData)
    {
        if (selcetStage) return;

        selcetStage = true;
        gameMgr.dataMgr.gameData.stageData = stageData.CopyData();
        gameMgr.dataMgr.gameData.RandomMapSelection();
        gameMgr.sceneHlr.StartLoadScene("SampleScene");
    }

    public void SetStageUI(bool value)
    {
        //switch (value)
        //{
        //    case true:
        //        break;
        //    case false:
        //        break;
        //}
        //playUI.SetActive(!value);
        stageUI.SetActive(value);
        gameMgr.camMgr.lockCam = value;
    }

    public void SetStorageUI(bool value)
    {
        gameMgr.camMgr.lockCam = value;
    }

    public void Button_TurnEnd()
    {
        if (gameMgr.currentTurn != CharacterOwner.Player) return;

        gameMgr.TurnEnd();
    }

    public void Button_Stage_Return()
    {
        SetStageUI(false);
    }

    public AmmoIcon GetAmmoIcon()
    {
        return ammoIconList[iconIndex];
    }
}
