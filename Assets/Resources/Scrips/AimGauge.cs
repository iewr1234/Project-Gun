using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AimGauge : MonoBehaviour
{
    public enum State
    {
        None,
        Check,
        Done,
    }

    [Header("---Access Component---")]
    public GameObject components;
    private Slider aimGauge;
    private Image aimGaugeFill;
    private List<GaugeScale> gaugeScales = new List<GaugeScale>();

    [Header("--- Assignment Variable---")]
    public State state;
    public int targetValue;
    private Color targetColor;
    private float gaugeScaleLength;
    private readonly float gaugeSpeed = 200f;

    public void SetComponents()
    {
        components = transform.Find("Components").gameObject;
        aimGauge = GetComponent<Slider>();
        aimGaugeFill = components.transform.Find("FillArea/Fill").GetComponent<Image>();
        gaugeScales = components.transform.Find("FillArea/GaugeScales").GetComponentsInChildren<GaugeScale>().ToList();
        for (int i = 0; i < gaugeScales.Count; i++)
        {
            var gaugeScale = gaugeScales[i];
            gaugeScale.SetComponents();
        }

        var gaugeLength = components.transform.Find("FillArea").GetComponent<RectTransform>().sizeDelta.x;
        gaugeScaleLength = gaugeLength * 0.01f;
        components.SetActive(false);
    }

    private void Update()
    {
        AimGaugeProcess();
    }

    private void AimGaugeProcess()
    {
        if (state != State.Check) return;

        aimGauge.value += gaugeSpeed * Time.deltaTime;
        if (aimGauge.value >= targetValue)
        {
            aimGauge.value = targetValue;
            aimGaugeFill.color = targetColor;
            state = State.Done;
        }
    }

    public void SetAimGauge(CharacterController charCtr)
    {
        var activeScales = gaugeScales.FindAll(x => x.gameObject.activeSelf);
        for (int i = 0; i < activeScales.Count; i++)
        {
            var gaugeScale = activeScales[i];
            gaugeScale.gameObject.SetActive(false);
        }

        var targetInfo = charCtr.targetList[charCtr.targetIndex];
        var shootNum = DataUtility.GetShootNum(charCtr.RPM, charCtr.fiarRate);
        var weapon = charCtr.currentWeapon;
        weapon.CheckHitBullet(targetInfo, shootNum);
        GaugeScalePlacement(weapon);
        aimGauge.value = 0;
    }

    public void SetAimGauge(bool value, Weapon weapon)
    {
        if (!value) return;

        var activeScales = gaugeScales.FindAll(x => x.gameObject.activeSelf);
        for (int i = 0; i < activeScales.Count; i++)
        {
            var gaugeScale = activeScales[i];
            gaugeScale.gameObject.SetActive(false);
        }

        targetValue = weapon.hitValue;
        aimGauge.value = 0;
        GaugeScalePlacement(weapon);

        if (weapon.hitInfos.FindAll(x => x.isHit).Count == 0)
        {
            targetColor = Color.red;
        }
        else
        {
            targetColor = Color.yellow;
        }
        state = State.Check;
        components.SetActive(true);
    }

    private void GaugeScalePlacement(Weapon weapon)
    {
        for (int i = 0; i < weapon.hitInfos.Count; i++)
        {
            var hitInfo = weapon.hitInfos[i];
            if (hitInfo.hitAccuracys[0] >= 100) break;

            for (int j = 0; j < hitInfo.hitAccuracys.Count; j++)
            {
                var hitAccuracy = hitInfo.hitAccuracys[j];
                var gaugeScale = gaugeScales.Find(x => !x.gameObject.activeSelf);
                var pos = gaugeScale.transform.localPosition;
                pos.x = gaugeScaleLength * hitAccuracy;
                gaugeScale.SetGaugeScale(pos, hitAccuracy);
            }
        }
    }

    public void SetAimGauge(bool value)
    {
        if (value) return;

        aimGaugeFill.color = Color.white;
        state = State.None;
        components.SetActive(false);
    }
}
