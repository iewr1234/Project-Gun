using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArrowPointer : MonoBehaviour
{
    [Header("---Access Component---")]
    private GameObject actionCost;
    private TextMeshProUGUI costText;

    [Header("--- Assignment Variable---")]
    private readonly float speed = 200f;

    public void SetComponents()
    {
        actionCost = transform.Find("ActionCost").gameObject;
        costText = actionCost.transform.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        RotationObject();
        LookAtTheCamera();
    }

    private void RotationObject()
    {
        var rotVal = Time.unscaledDeltaTime * speed;
        var newRot = new Vector3(0f, rotVal, 0f);
        transform.Rotate(newRot);
    }

    private void LookAtTheCamera()
    {
        var cam = Camera.main;
        var pos = actionCost.transform.position + cam.transform.rotation * Vector3.forward;
        var rot = cam.transform.rotation * Vector3.up;
        actionCost.transform.LookAt(pos, rot);
    }

    public void SetMoveCost(int cost)
    {
        costText.text = $"{cost}";
    }

    public int GetMoveCost()
    {
        return int.Parse(costText.text);
    }
}
