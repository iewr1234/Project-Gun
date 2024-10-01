using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusValue : MonoBehaviour
{
    [Header("---Access Component---")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI valueText;

    public void SetAbilityText(string name, int value)
    {
        nameText.text = name;
        valueText.text = $"{value}";
    }

    public void SetAbilityText(string name, float value)
    {
        nameText.text = name;
        valueText.text = $"{value}";
    }
}
