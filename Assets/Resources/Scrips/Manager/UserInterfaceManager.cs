using System.Collections;
using System.Collections.Generic;
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

    [Header("---Access Component---")]
    public AimUI aimUI;

    private readonly string aimUIGaugePath = "Sprites/SightGauge/Gauge_SightAp";
    private readonly int aimUIGaugeMax = 5;

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;

        aimUI = new AimUI()
        {
            uiObject = transform.Find("AimUI").gameObject,
            fireRateGauge = transform.Find("AimUI/FireRateGauge").GetComponent<Image>(),
            sightGauge = transform.Find("AimUI/SightGauge").GetComponent<Image>(),
            armorGauge = transform.Find("AimUI/TargetInfo/ArmorGauge").GetComponent<Slider>(),
            healthGauge = transform.Find("AimUI/TargetInfo/HealthGauge").GetComponent<Slider>(),
            staminaGauge = transform.Find("AimUI/TargetInfo/StaminaGauge").GetComponent<Slider>(),
            fireRateNum = 1,
            sightNum = 1,
        };
        aimUI.uiObject.SetActive(false);
    }

    public void SetActiveAimUI(bool value)
    {
        aimUI.uiObject.SetActive(value);
        if (!value)
        {
            aimUI.fireRateNum = 1;
            aimUI.sightNum = 1;
        }
    }

    public void SetfireRateGauge()
    {
        aimUI.fireRateNum++;
        if (aimUI.fireRateNum > aimUIGaugeMax)
        {
            aimUI.fireRateNum = 1;
        }

        aimUI.fireRateGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{aimUI.fireRateNum}");
    }

    public void SetSightGauge()
    {
        aimUI.sightNum++;
        if (aimUI.sightNum > aimUIGaugeMax)
        {
            aimUI.sightNum = 1;
        }

        aimUI.sightGauge.sprite = Resources.Load<Sprite>(aimUIGaugePath + $"{aimUI.sightNum}");
    }

    public void SetTargetInfo(CharacterController targetCtr)
    {
        if (targetCtr.armor != null)
        {
            aimUI.armorGauge.maxValue = targetCtr.armor.maxDurability;
            aimUI.armorGauge.value = targetCtr.armor.durability;
        }
        aimUI.healthGauge.maxValue = targetCtr.maxHealth;
        aimUI.healthGauge.value = targetCtr.health;
        aimUI.staminaGauge.maxValue = targetCtr.maxStamina;
        aimUI.staminaGauge.value = targetCtr.stamina;
    }
}
