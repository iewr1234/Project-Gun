using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ActionBlockState
{
    Inactive,
    Active,
    Used,
}

public class ActionBlock : MonoBehaviour
{
    [Header("---Access Component---")]
    private Image fillImage;

    [Header("--- Assignment Variable---")]
    public ActionBlockState state;

    private readonly Color inactiveColor = new Color(90 / 255f, 90 / 255f, 90 / 255f);
    private readonly Color activeColor = new Color(92 / 255f, 189 / 255f, 1f);

    public void SetComponents()
    {
        fillImage = transform.Find("Fill").GetComponent<Image>();
        SetActionState(ActionBlockState.Active);
    }

    public void SetActionState(ActionBlockState newState)
    {
        switch (newState)
        {
            case ActionBlockState.Inactive:
                fillImage.color = inactiveColor;
                state = newState;
                break;
            case ActionBlockState.Active:
                fillImage.color = activeColor;
                state = newState;
                break;
            case ActionBlockState.Used:
                fillImage.color = Color.white;
                break;
            default:
                break;
        }
    }
}
