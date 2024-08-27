using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    [Header("---Access Script---")]
    private GameManager gameMgr;

    [Header("---Access Component---")]
    private Button button;
    private Image backImage;

    [Header("--- Assignment Variable---")]
    public GameState type;

    private readonly Color activeColor = new Color(200 / 255f, 0f, 0f);
    private readonly Color inactiveColor = new Color(16 / 255f, 22 / 255f, 26 / 255f);

    public void SetComponents(GameManager _gameMgr)
    {
        gameMgr = _gameMgr;

        button = GetComponent<Button>();
        backImage = transform.Find("BackGround").GetComponent<Image>();
    }

    public void SetActiveButton(bool value)
    {
        switch (value)
        {
            case true:
                backImage.color = activeColor;
                break;
            case false:
                backImage.color = inactiveColor;
                break;
        }
    }

    public void Button_ActionButton()
    {
        switch (type)
        {
            case GameState.Shoot:
                gameMgr.ShootingAction_Move();
                SetActiveButton(false);
                break;
            case GameState.Reload:
                gameMgr.ReloadAction_Move();
                SetActiveButton(true);
                break;
            case GameState.Throw:
                gameMgr.ThrowAction_Move();
                SetActiveButton(true);
                break;
            default:
                break;
        }
        gameMgr.uiMgr.onButton = null;
    }

    public void PointerEnter_ActionButton()
    {
        if (gameMgr.gameState == type) return;

        gameMgr.uiMgr.onButton = button;
        SetActiveButton(true);
    }

    public void PointerExit_ActionButton()
    {
        if (gameMgr.gameState == type) return;

        gameMgr.uiMgr.onButton = null;
        SetActiveButton(false);
    }
}
