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
    public Canvas canvas;
    public GameObject components;
    private Slider armorGauge;
    private Slider healthGauge;
    private Slider staminaGauge;
    private TextMeshProUGUI armorText;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI staminaText;
    [HideInInspector] public AimGauge aimGauge;

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
        components = transform.Find("Components").gameObject;
        armorGauge = components.transform.Find("ArmorGauge").GetComponent<Slider>();
        healthGauge = components.transform.Find("HealthGauge").GetComponent<Slider>();
        staminaGauge = components.transform.Find("StaminaGauge").GetComponent<Slider>();
        armorText = armorGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        healthText = healthGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        staminaText = staminaGauge.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        aimGauge = components.transform.Find("AimGauge").GetComponent<AimGauge>();
        aimGauge.SetComponents();

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
        //if (charCtr.equipBody)
        //{
        //    armorGauge.value = charCtr.bodyArmor.durability;
        //    armorText.text = $"{armorGauge.value} / {armorGauge.maxValue}";
        //}
        healthGauge.value = charCtr.health;
        healthText.text = $"{healthGauge.value} / {healthGauge.maxValue}";
        staminaGauge.value = charCtr.stamina;
        staminaText.text = $"{staminaGauge.value} / {staminaGauge.maxValue}";
    }
}
