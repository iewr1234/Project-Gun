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
    private Slider hPartsGauge;
    private Slider hBodyGauge;
    private Slider staminaGauge;

    private TextMeshProUGUI hArmorText;
    private TextMeshProUGUI bArmorText;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI staminaText;

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
        hPartsGauge = components.transform.Find("HealthGauge/PartsHealth").GetComponent<Slider>();
        hBodyGauge = components.transform.Find("HealthGauge/BodyHealth").GetComponent<Slider>();
        staminaGauge = components.transform.Find("StaminaGauge").GetComponent<Slider>();
        staminaGauge.gameObject.SetActive(false);

        hArmorText = hArmorGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        bArmorText = bArmorGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        healthText = hBodyGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        staminaText = staminaGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        int maxHealth = charCtr.healthList.Sum(x => x.maxHealth);
        hPartsGauge.maxValue = maxHealth;
        hBodyGauge.maxValue = maxHealth;
        staminaGauge.maxValue = charCtr.maxStamina;
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
            armorText.text = $"{Mathf.FloorToInt(armorGauge.value * 0.01f)} / {Mathf.FloorToInt(armorGauge.maxValue * 0.01f)}";
        }
        int curHealth = charCtr.healthList.Sum(x => x.health);
        hPartsGauge.value = curHealth;
        hBodyGauge.value = charCtr.healthList.Find(x => x.type == BodyPartsType.Body).health;
        healthText.text = $"{curHealth} / {hBodyGauge.maxValue}";
        staminaGauge.value = charCtr.stamina;
        staminaText.text = $"{staminaGauge.value} / {staminaGauge.maxValue}";
    }
}
