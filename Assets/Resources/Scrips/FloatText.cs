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
    private Vector3 moveDir;
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
        transform.position += moveDir * (moveSpeed * Time.deltaTime);
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

        moveDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
        timer = 0f;
    }
}
