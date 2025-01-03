using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatText : MonoBehaviour
{
    private GameManager gameMgr;
    private CharacterUI charUI;

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
        gameMgr = _gameMgr;

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
        Vector3 _moveDir = charUI.transform.TransformDirection(moveDir);
        transform.position += _moveDir * (moveSpeed * Time.deltaTime);

        if (timer > moveTime) gameObject.SetActive(false);
    }

    public void ShowFloatText(CharacterUI _charUI, string text, Color color)
    {
        gameObject.SetActive(true);
        charUI = _charUI;

        Vector3 pos = charUI.transform.position;
        pos.y = charUI.charCtr.transform.position.y + 1.25f;
        transform.position = pos;
        this.text.text = text;
        this.text.color = color;

        moveDir = this.text.text == "Miss" ? Vector3.up : new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
        timer = 0f;
    }
}
