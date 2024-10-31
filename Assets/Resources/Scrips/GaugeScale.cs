using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GaugeScale : MonoBehaviour
{
    private Image image;
    private RectTransform imageRect;
    private GameObject valueComponents;
    private TextMeshProUGUI valueText;

    public void SetComponents()
    {
        image = transform.Find("Image").GetComponent<Image>();
        imageRect = image.GetComponent<RectTransform>();
        valueComponents = transform.Find("Value").gameObject;
        valueText = valueComponents.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        valueComponents.SetActive(false);
        gameObject.SetActive(false);
    }

    public void SetGaugeScale(Vector3 pos, int value, bool isPellet)
    {
        if (!isPellet)
        {
            image.color = Color.white;
            imageRect.offsetMax = new Vector2(0f, 0f);
            imageRect.offsetMin = new Vector2(0f, 0f);
        }
        else
        {
            image.color = Color.yellow;
            imageRect.offsetMax = new Vector2(0f, -3f);
            imageRect.offsetMin = new Vector2(0f, 3f);
        }
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
