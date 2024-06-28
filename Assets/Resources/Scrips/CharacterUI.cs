using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterUI : MonoBehaviour
{
    [Header("---Access Script---")]
    private CharacterController charCtr;

    [Header("---Access Component---")]
    private Canvas canvas;
    [HideInInspector] public Slider armorGauge;
    [HideInInspector] public Slider healthGauge;
    [HideInInspector] public Slider staminaGauge;
    [HideInInspector] public TextMeshProUGUI armorText;
    [HideInInspector] public TextMeshProUGUI healthText;
    [HideInInspector] public TextMeshProUGUI staminaText;

    [Header("--- Assignment Variable---")]
    [SerializeField] private Vector3 uiPos;

    public void SetComponents(CharacterController _charCtr)
    {
        charCtr = _charCtr;
        charCtr.charUI = this;
        transform.name = $"{charCtr.name}_UI";
        LookAtTheCamera();

        canvas = GetComponent<Canvas>();
        canvas.worldCamera = charCtr.GameMgr.camMgr.mainCam;
        canvas.sortingOrder = -1;
        armorGauge = transform.Find("ArmorGauge").GetComponent<Slider>();
        healthGauge = transform.Find("HealthGauge").GetComponent<Slider>();
        staminaGauge = transform.Find("StaminaGauge").GetComponent<Slider>();
        armorText = armorGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        healthText = healthGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        staminaText = staminaGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();

        //if (charCtr.armor != null)
        //{
        //    armorGauge.maxValue = charCtr.armor.maxDurability;
        //    armorGauge.value = charCtr.armor.durability;
        //}
        //else
        //{
        //    armorGauge.gameObject.SetActive(false);
        //}
        armorGauge.gameObject.SetActive(false);

        healthGauge.maxValue = charCtr.maxHealth;
        healthGauge.value = charCtr.health;
        staminaGauge.maxValue = charCtr.maxStamina;
        staminaGauge.value = charCtr.stamina;
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

    public void SetCharacterValue()
    {
        if (charCtr.armor != null)
        {
            armorGauge.value = charCtr.armor.durability;
            armorText.text = $"{armorGauge.value} / {armorGauge.maxValue}";
        }
        healthGauge.value = charCtr.health;
        healthText.text = $"{healthGauge.value} / {healthGauge.maxValue}";
        staminaGauge.value = charCtr.stamina;
        staminaText.text = $"{staminaGauge.value} / {staminaGauge.maxValue}";
    }
}
