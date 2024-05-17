using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AimUI
{
    [Header("---Access Component---")]
    public GameObject uiObject;
    public Image fireRateGauge;
    public Image sightGauge;
    public Slider armorGauge;
    public Slider healthGauge;
    public Slider staminaGauge;

    [Header("--- Assignment Variable---")]
    public int fireRateNum;
    public int sightNum;
}

public class UserInterfaceManager : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---\n[BottomUI]")]
    public GameObject bottomUI;
    public List<ActionBlock> actionBlocks;

    [Header("[AimUI]")]
    public GameObject aimUI;
    [HideInInspector] public Image fireRateGauge;
    [HideInInspector] public Image sightGauge;
    [HideInInspector] public Slider armorGauge;
    [HideInInspector] public Slider healthGauge;
    [HideInInspector] public Slider staminaGauge;

    [Header("--- Assignment Variable---\n[AimUI]")]
    public int fireRateNum;
    public int sightNum;

    private readonly string aimUIGaugePath = "Sprites/SightGauge/Gauge_SightAp";
    private readonly int aimUIGaugeMax = 5;

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;

        bottomUI = transform.Find("BottomUI").gameObject;
        actionBlocks = bottomUI.transform.Find("ActionPoint").GetComponentsInChildren<ActionBlock>().ToList();
        for (int i = 0; i < actionBlocks.Count; i++)
        {
            var actionBlock = actionBlocks[i];
            actionBlock.SetComponents();
        }

        aimUI = transform.Find("AimUI").gameObject;
        fireRateGauge = transform.Find("AimUI/FireRateGauge").GetComponent<Image>();
        sightGauge = transform.Find("AimUI/SightGauge").GetComponent<Image>();
        armorGauge = transform.Find("AimUI/TargetInfo/ArmorGauge").GetComponent<Slider>();
        healthGauge = transform.Find("AimUI/TargetInfo/HealthGauge").GetComponent<Slider>();
        staminaGauge = transform.Find("AimUI/TargetInfo/StaminaGauge").GetComponent<Slider>();
        aimUI.SetActive(false);

        fireRateNum = 1;
        sightNum = 1;
    }

    public void SetActionPoint(CharacterController charCtr)
    {
        if (charCtr.ownerType != CharacterOwner.Player) return;

        Debug.Log("!");
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

    public void SetUsedActionPoint(CharacterController charCtr, int usedAction)
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

    public void SetActiveAimUI(bool value)
    {
        aimUI.SetActive(value);
        if (!value)
        {
            fireRateNum = 1;
            sightNum = 1;
            fireRateGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{fireRateNum}");
            sightGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{sightNum}");
        }
    }

    public void SetfireRateGauge()
    {
        fireRateNum++;
        if (fireRateNum > aimUIGaugeMax)
        {
            fireRateNum = 1;
        }

        fireRateGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{fireRateNum}");
    }

    public void SetSightGauge()
    {
        sightNum++;
        if (sightNum > aimUIGaugeMax)
        {
            sightNum = 1;
        }

        sightGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{sightNum}");
    }

    public void SetTargetInfo(CharacterController targetCtr)
    {
        if (targetCtr.armor != null)
        {
            armorGauge.maxValue = targetCtr.armor.maxDurability;
            armorGauge.value = targetCtr.armor.durability;
        }
        healthGauge.maxValue = targetCtr.maxHealth;
        healthGauge.value = targetCtr.health;
        staminaGauge.maxValue = targetCtr.maxStamina;
        staminaGauge.value = targetCtr.stamina;
    }
}
