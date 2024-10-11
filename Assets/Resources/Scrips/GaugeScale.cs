using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GaugeScale : MonoBehaviour
{
    private GameObject valueComponents;
    private TextMeshProUGUI valueText;

    public void SetComponents()
    {
        valueComponents = transform.Find("Value").gameObject;
        valueText = valueComponents.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        valueComponents.SetActive(false);
        gameObject.SetActive(false);
    }

    public void SetGaugeScale(Vector3 pos, int value)
    {
        transform.localPosition = pos;
        valueText.text = $"{value}";
        valueComponents.SetActive(false);
        gameObject.SetActive(true);
    }

    public void PointerEnter_GaugeScale()
    {
        valueComponents.SetActive(true);
    }

    public void PointerExit_GaugeScale()
    {
        valueComponents.SetActive(false);
    }
}
