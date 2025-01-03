using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum WarningState
{
    None,
    DeleteDropItems,
}

public class PopUp_Warning : MonoBehaviour
{
    [Header("---Access Script---")]
    [SerializeField] private GameMenuManager gameMenuMgr;

    [Header("---Access Component---")]
    public GameObject components;
    public Image fade;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI explanText;
    private Button[] buttons;

    [Header("--- Assignment Variable---")]
    public WarningState state;

    public void Start()
    {
        DontDestroyOnLoad(gameObject);

        gameMenuMgr = FindAnyObjectByType<GameMenuManager>();

        components = transform.Find("Components").gameObject;
        fade = transform.Find("Fade").GetComponent<Image>();
        fade.enabled = false;
        topText = transform.Find("Components/Top/Text").GetComponent<TextMeshProUGUI>();
        explanText = transform.Find("Components/ExplanText").GetComponent<TextMeshProUGUI>();
        buttons = transform.Find("Components/Buttons").GetComponentsInChildren<Button>();

        components.SetActive(false);
    }

    public void SetWarning(WarningState _state)
    {
        if (state == _state) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            var button = buttons[i];
            button.gameObject.SetActive(false);
        }
        components.SetActive(true);
        fade.enabled = true;

        state = _state;
        switch (state)
        {
            case WarningState.DeleteDropItems:
                topText.text = "아이템 분실 경고";
                explanText.text = "바닥에 놓인 아이템이 분실될 것입니다.\n그래도 진행하시겠습니까?";
                SetButton(buttons[0], "확인", "DeleteDropItems_Check");
                SetButton(buttons[1], "취소", "DeleteDropItems_Cancel");
                break;
            default:
                break;
        }
    }

    private void SetButton(Button button, string buttonText, string functionName)
    {
        button.gameObject.SetActive(true);
        var textMesh = button.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        textMesh.text = buttonText;

        button.onClick.RemoveAllListeners();
        switch (functionName)
        {
            case "DeleteDropItems_Check":
                button.onClick.AddListener(() => Button_DeleteDropItems_Check());
                break;
            case "DeleteDropItems_Cancel":
                button.onClick.AddListener(() => Button_DeleteDropItems_Cancel());
                break;
            default:
                break;
        }
    }

    private void CloseWarning()
    {
        components.SetActive(false);
        fade.enabled = false;
        state = WarningState.None;
    }

    public void Button_PopUp_Close()
    {
        if (gameMenuMgr.gameMgr.uiMgr.selcetStage != null) gameMenuMgr.gameMgr.uiMgr.selcetStage = null;

        CloseWarning();
    }

    public void Button_DeleteDropItems_Check()
    {
        CloseWarning();
        var gameMgr = gameMenuMgr.gameMgr;
        switch (gameMgr.gameState)
        {
            case GameState.Stage:
                gameMgr.EnterTheStage();
                break;
            case GameState.Result:
                if (gameMgr.dataMgr.gameData.stageData.waveNum >= 0)
                {
                    gameMgr.NextMap();
                }
                else
                {
                    gameMgr.ReturnBase();
                }
                break;
            default:
                break;
        }

        //switch (gameMenuMgr.state)
        //{
        //    case GameMenuState.Status:
        //        gameMenuMgr.otherStorage.DeactiveTabButtons();
        //        gameMenuMgr.ShowStatus(false);
        //        break;
        //    case GameMenuState.Inventory:
        //        gameMenuMgr.ShowInventory(false);
        //        break;
        //    default:
        //        break;
        //}
    }

    public void Button_DeleteDropItems_Cancel()
    {
        if (gameMenuMgr.gameMgr.uiMgr.selcetStage != null) gameMenuMgr.gameMgr.uiMgr.selcetStage = null;

        CloseWarning();
    }
}
