using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireWarning : MonoBehaviour
{
    [Header("---Access Component---")]
    private Canvas canvas;
    private Image backImage;
    private Image iconImage;

    public void SetComponents()
    {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        backImage = transform.Find("BackGround").GetComponent<Image>();
        iconImage = transform.Find("IconImage").GetComponent<Image>();
    }

    private void Update()
    {
        LookAtTheCamera();
    }

    private void LookAtTheCamera()
    {
        var cam = Camera.main;
        var pos = backImage.transform.position + cam.transform.rotation * Vector3.forward;
        var rot = cam.transform.rotation * Vector3.up;
        backImage.transform.LookAt(pos, rot);
        iconImage.transform.LookAt(pos, rot);
    }
}
