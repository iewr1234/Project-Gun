using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatText : MonoBehaviour
{
    [Header("---Access Component---")]
    private Canvas canvas;
    [HideInInspector] public TextMeshProUGUI text;

    [Header("--- Assignment Variable---")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveTime;

    private float timer;

    public void SetComponents(GameManager _gameMgr)
    {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = _gameMgr.camMgr.mainCam;
        text = GetComponentInChildren<TextMeshProUGUI>();

        transform.name = $"FloatText_{_gameMgr.floatTextPool.Count}";
        gameObject.SetActive(false);
    }

    private void Update()
    {
        LookAtTheCamera();
        MoveText();
    }

    private void LookAtTheCamera()
    {
        var cam = canvas.worldCamera;
        var pos = transform.position + cam.transform.rotation * Vector3.forward;
        var rot = cam.transform.rotation * Vector3.up;
        transform.LookAt(pos, rot);
    }

    private void MoveText()
    {
        timer += Time.deltaTime;
        transform.position += new Vector3(0f, moveSpeed * Time.deltaTime, 0f);
        if (timer > moveTime)
        {
            gameObject.SetActive(false);
        }
    }

    public void ShowFloatText(Vector3 pos, string text, Color color)
    {
        gameObject.SetActive(true);
        transform.position = pos;
        this.text.text = text;
        this.text.color = color;

        timer = 0f;
    }
}
