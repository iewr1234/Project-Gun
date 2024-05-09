using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayerCheck : MonoBehaviour
{
    [Header("---Access Script---")]
    private MapEditor mapEdt;

    [Header("---Access Component---")]
    private Image checkImage;

    [Header("--- Assignment Variable---")]
    [SerializeField] private LayerMask layer;
    [SerializeField] private bool check;

    private void Start()
    {
        mapEdt = FindObjectOfType<MapEditor>();
        checkImage = transform.Find("CheckBox/CheckImage").GetComponent<Image>();
        check = true;
    }

    public void Button_LayerCheck()
    {
        check = !check;
        checkImage.enabled = check;
        mapEdt.SwitchingLayer(layer, check);
    }
}
