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
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image partsIcon;
    [SerializeField] private Sprite[] partsSprites;
    [SerializeField] private List<TextMeshProUGUI> valueTexts;

    [Header("--- Assignment Variable---")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveTime;
    private Vector3 moveDir;
    private float timer;

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;

        canvas.worldCamera = _gameMgr.camMgr.mainCam;
        partsIcon.gameObject.SetActive(false);
        for (int i = 0; i < valueTexts.Count; i++)
        {
            TextMeshProUGUI valueText = valueTexts[i];
            valueText.gameObject.SetActive(false);
        }

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

    public void ShowFloatText(CharacterUI _charUI, BodyPartsType partsType, string text, Color color)
    {
        gameObject.SetActive(true);
        charUI = _charUI;

        Vector3 pos = charUI.transform.position;
        pos.y = charUI.charCtr.transform.position.y + 1.25f;
        transform.position = pos;
        if (partsType != BodyPartsType.Miss && partsType != BodyPartsType.Block)
        {
            partsIcon.gameObject.SetActive(true);
            partsIcon.sprite = partsSprites[(int)partsType];
            partsIcon.color = color;
        }
        else
        {
            partsIcon.gameObject.SetActive(false);
        }

        bool textEnd = false;
        for (int i = 0; i < valueTexts.Count; i++)
        {
            TextMeshProUGUI valueText = valueTexts[i];
            if (textEnd)
            {
                if (!valueText.gameObject.activeSelf) break;

                valueText.gameObject.SetActive(false);
                continue;
            }

            if (partsType == BodyPartsType.Miss || partsType == BodyPartsType.Block)
            {
                valueText.gameObject.SetActive(true);
                valueText.text = text;
                valueText.color = color;
                textEnd = true;
            }
            else
            {
                valueText.gameObject.SetActive(true);
                valueText.text = text.Substring(i, 1);
                valueText.color = color;
            }

            if (i + 1 == text.Length) textEnd = true;
        }
        moveDir = text == "Miss" || text == "Block" ? Vector3.up : new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);
        timer = 0f;
        LookAtTheCamera();
    }
}
