using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterUI : MonoBehaviour
{
    [Header("---Access Script---")]
    public CharacterController charCtr;

    [Header("---Access Component---")]
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public GameObject components;

    private Slider hArmorGauge;
    private Slider bArmorGauge;
    private Slider healthGauge;
    private Slider staminaGauge;

    private TextMeshProUGUI hArmorText;
    private TextMeshProUGUI bArmorText;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI staminaText;

    [HideInInspector] public AimGauge aimGauge;

    [Header("--- Assignment Variable---")]
    [SerializeField] private Vector3 uiPos = new Vector3(0f, 2f, 0f);

    public void SetComponents(CharacterController _charCtr)
    {
        charCtr = _charCtr;
        charCtr.charUI = this;
        transform.name = $"{charCtr.name}_UI";
        LookAtTheCamera();

        canvas = GetComponent<Canvas>();
        canvas.worldCamera = charCtr.GameMgr.camMgr.mainCam;
        canvas.sortingOrder = -1;
        components = transform.Find("Components").gameObject;

        hArmorGauge = components.transform.Find("HeadArmorGauge").GetComponent<Slider>();
        bArmorGauge = components.transform.Find("BodyArmorGauge").GetComponent<Slider>();
        healthGauge = components.transform.Find("HealthGauge").GetComponent<Slider>();
        staminaGauge = components.transform.Find("StaminaGauge").GetComponent<Slider>();

        hArmorText = hArmorGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        bArmorText = bArmorGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        healthText = healthGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        staminaText = staminaGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        aimGauge = components.transform.Find("AimGauge").GetComponent<AimGauge>();
        aimGauge.SetComponents();

        healthGauge.maxValue = charCtr.maxHealth;
        healthGauge.value = charCtr.health;
        staminaGauge.maxValue = charCtr.maxStamina;
        staminaGauge.value = charCtr.stamina;
        SetActiveArmorGauge();
        SetCharacterValue();
    }

    private void FixedUpdate()
    {
        LookAtTheCamera();
    }

    private void LookAtTheCamera()
    {
        transform.position = charCtr.transform.position + uiPos;

        var cam = Camera.main;
        var pos = transform.position + cam.transform.rotation * Vector3.forward;
        var rot = cam.transform.rotation * Vector3.up;
        transform.LookAt(pos, rot);
    }

    public void SetActiveArmorGauge()
    {
        hArmorGauge.gameObject.SetActive(false);
        bArmorGauge.gameObject.SetActive(false);
        if (charCtr.armors.Count == 0) return;

        for (int i = 0; i < charCtr.armors.Count; i++)
        {
            Armor armor = charCtr.armors[i];
            Slider armorGauge = armor.type == Armor.Type.Head ? hArmorGauge : bArmorGauge;
            armorGauge.maxValue = armor.armorData.maxDurability;
            armorGauge.value = armor.armorData.durability;
            armorGauge.gameObject.SetActive(true);
        }
    }

    public void SetCharacterValue()
    {
        for (int i = 0; i < charCtr.armors.Count; i++)
        {
            Armor armor = charCtr.armors[i];
            Slider armorGauge = armor.type == Armor.Type.Head ? hArmorGauge : bArmorGauge;
            TextMeshProUGUI armorText = armor.type == Armor.Type.Head ? hArmorText : bArmorText;
            armorGauge.value = armor.armorData.durability;
            armorText.text = $"{armorGauge.value} / {armorGauge.maxValue}";
        }
        healthGauge.value = charCtr.health;
        healthText.text = $"{healthGauge.value} / {healthGauge.maxValue}";
        staminaGauge.value = charCtr.stamina;
        staminaText.text = $"{staminaGauge.value} / {staminaGauge.maxValue}";
    }
}
