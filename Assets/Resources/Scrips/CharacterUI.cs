using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterUI : MonoBehaviour
{
    public enum AimState
    {
        None,
        Check,
        Done,
    }

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

    private Slider aimGauge;
    private Image aimGaugeFill;
    private GameObject aimComponents;
    private List<Image> gaugeScales = new List<Image>();
    [HideInInspector] public AimState aimState;

    [Header("--- Assignment Variable---")]
    [SerializeField] private Vector3 uiPos;
    [Space(5f)]

    [Header("[AimGauge]")]
    public int targetValue;
    private Color targetColor;
    private float gaugeScaleLength;
    [SerializeField] private float gaugeSpeed;

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

        aimGauge = transform.Find("AimGauge").GetComponent<Slider>();
        aimGaugeFill = aimGauge.transform.Find("Components/FillArea/Fill").GetComponent<Image>();
        aimComponents = aimGauge.transform.Find("Components").gameObject;
        gaugeScales = aimGauge.transform.Find("Components/FillArea/GaugeScales").GetComponentsInChildren<Image>().ToList();
        for (int i = 0; i < gaugeScales.Count; i++)
        {
            var gaugeScale = gaugeScales[i];
            gaugeScale.enabled = false;
        }

        var gaugeLength = aimComponents.transform.Find("FillArea").GetComponent<RectTransform>().sizeDelta.x;
        gaugeScaleLength = gaugeLength * 0.01f;
        aimComponents.SetActive(false);
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

    private void Update()
    {
        AimGaugeProcess();
    }

    private void AimGaugeProcess()
    {
        if (aimState != AimState.Check) return;

        aimGauge.value += gaugeSpeed * Time.deltaTime;
        if (aimGauge.value > targetValue)
        {
            aimGaugeFill.color = targetColor;
            aimState = AimState.Done;
        }
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

    public void SetAimGauge(bool value)
    {
        aimComponents.SetActive(value);
        switch (value)
        {
            case true:
                var activeScales = gaugeScales.FindAll(x => x.enabled);
                for (int i = 0; i < activeScales.Count; i++)
                {
                    var gaugeScale = activeScales[i];
                    gaugeScale.enabled = false;
                }

                var weapon = charCtr.currentWeapon;
                targetValue = weapon.hitValue;
                aimGauge.value = 0;
                for (int i = 0; i < weapon.hitInfos.Count; i++)
                {
                    var hitInfo = weapon.hitInfos[i];
                    if (hitInfo.hitAccuracys[0] >= 100) break;

                    for (int j = 0; j < hitInfo.hitAccuracys.Count; j++)
                    {
                        var hitAccuracy = hitInfo.hitAccuracys[j];
                        var gaugeScale = gaugeScales.Find(x => !x.enabled);
                        var pos = gaugeScale.transform.localPosition;
                        gaugeScale.transform.localPosition = new Vector3(gaugeScaleLength * hitAccuracy, pos.y, pos.z);
                        gaugeScale.enabled = true;
                    }
                }

                if (weapon.hitInfos.FindAll(x => x.isHit).Count == 0)
                {
                    targetColor = Color.red;
                }
                else
                {
                    targetColor = Color.yellow;
                }
                aimState = AimState.Check;
                break;
            case false:
                aimGaugeFill.color = Color.white;
                aimState = AimState.None;
                break;
        }
    }
}
