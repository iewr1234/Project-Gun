using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserInterfaceManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---")]
    [SerializeField] private Canvas canvas;

    [Header("[BottomUI]")]
    public GameObject bottomUI;
    [HideInInspector] public TextMeshProUGUI magNumText;
    public List<ActionBlock> actionBlocks;

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

    private readonly string aimUIGaugePath = "Sprites/SightGauge/Gauge_SightAp";
    private readonly int aimUIGaugeMax = 4;

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;
        canvas = GetComponent<Canvas>();

        bottomUI = transform.Find("BottomUI").gameObject;
        magNumText = transform.Find("BottomUI/MagNum").GetComponent<TextMeshProUGUI>();
        magNumText.enabled = false;
        actionBlocks = bottomUI.transform.Find("ActionPoint").GetComponentsInChildren<ActionBlock>().ToList();
        for (int i = 0; i < actionBlocks.Count; i++)
        {
            var actionBlock = actionBlocks[i];
            actionBlock.SetComponents();
        }
        bottomUI.gameObject.SetActive(false);

        aimUI = transform.Find("AimUI").gameObject;
        shootNumText = transform.Find("AimUI/ShootNum").GetComponent<TextMeshProUGUI>();
        hitAccuracyText = transform.Find("AimUI/HitAccuracy").GetComponent<TextMeshProUGUI>();
        actionPointText = transform.Find("AimUI/ActionPoint/Text").GetComponent<TextMeshProUGUI>();
        fireRateGauge = transform.Find("AimUI/FireRateGauge").GetComponent<Image>();
        sightGauge = transform.Find("AimUI/SightGauge").GetComponent<Image>();
        armorGauge = transform.Find("AimUI/TargetInfo/ArmorGauge").GetComponent<Slider>();
        healthGauge = transform.Find("AimUI/TargetInfo/HealthGauge").GetComponent<Slider>();
        staminaGauge = transform.Find("AimUI/TargetInfo/StaminaGauge").GetComponent<Slider>();
        aimUI.SetActive(false);
    }

    public void SetMagNum(CharacterController charCtr)
    {
        if (charCtr.ownerType != CharacterOwner.Player || charCtr.weapons.Count == 0) return;

        var weapon = charCtr.currentWeapon;
        var loadedAmmo = weapon.weaponData.equipMag.loadedBullets.Count;
        if (weapon.weaponData.chamberBullet != null && weapon.weaponData.chamberBullet.level > 0) loadedAmmo++;

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
        var shootNum = (int)(((float)weapon.weaponData.RPM / 200) * (charCtr.fireRateNum + 1));

        var loadedAmmo = weapon.weaponData.equipMag.loadedBullets.Count;
        if (weapon.weaponData.isChamber) loadedAmmo++;

        shootNumText.color = shootNum > loadedAmmo ? Color.red : Color.black;
        shootNumText.text = $"{shootNum}";
    }

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
        aimUI.SetActive(value);
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
        var totalCost = charCtr.currentWeapon.weaponData.actionCost + charCtr.fireRateNum + charCtr.sightNum;
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
        charCtr.fireRateNum++;
        if (charCtr.fireRateNum > aimUIGaugeMax)
        {
            charCtr.fireRateNum = 0;
        }

        fireRateGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{charCtr.fireRateNum + 1}");
        var totalCost = charCtr.currentWeapon.weaponData.actionCost + charCtr.fireRateNum + charCtr.sightNum;
        SetUsedActionPoint_Bottom(charCtr, totalCost);
        SetShootNum(charCtr);
        SetActionPoint_Aim(charCtr);
    }

    public void SetSightGauge(CharacterController charCtr)
    {
        charCtr.sightNum++;
        if (charCtr.sightNum > aimUIGaugeMax)
        {
            charCtr.sightNum = 0;
        }

        sightGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{charCtr.sightNum + 1}");
        var totalCost = charCtr.currentWeapon.weaponData.actionCost + charCtr.fireRateNum + charCtr.sightNum;
        SetUsedActionPoint_Bottom(charCtr, totalCost);
        SetActionPoint_Aim(charCtr);
    }

    public void SetTargetInfo(TargetInfo targetInfo)
    {
        hitAccuracyText.text = $"{DataUtility.GetHitAccuracy(targetInfo.target, targetInfo)}%";
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
}
