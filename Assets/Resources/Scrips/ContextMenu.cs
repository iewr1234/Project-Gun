using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour
{
    private enum ButtonType
    {
        ItemInformation,
        UninstallBullets,
    }

    [Header("---Access Script---")]
    private GameMenuManager gameMenuMgr;

    [Header("---Access Component---")]
    private List<Button> buttons = new List<Button>();

    [Header("--- Assignment Variable---")]
    [SerializeField] private bool onPointer;

    public void SetComponents(GameMenuManager _gameMenuMgr)
    {
        gameMenuMgr = _gameMenuMgr;

        buttons = transform.Find("Buttons").GetComponentsInChildren<Button>().ToList();

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameObject.activeSelf && !onPointer && Input.GetMouseButton(0))
        {
            CloseTheContextMenu(true);
        }
    }

    public void OpenTheContextMenu(ItemHandler item)
    {
        for (int i = 1; i < buttons.Count; i++)
        {
            var button = buttons[i];
            if (!button.gameObject.activeSelf) continue;

            button.gameObject.SetActive(false);
        }

        gameMenuMgr.selectItem = item;
        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
        var worldPos = gameMenuMgr.gameMenuCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, gameMenuMgr.GetCanvasDistance()));
        transform.position = worldPos;

        //buttons[(int)ButtonType.ItemInformation].gameObject.SetActive(true);
        switch (item.itemData.type)
        {
            case ItemType.Magazine:
                buttons[(int)ButtonType.UninstallBullets].gameObject.SetActive(true);
                break;
            default:
                break;
        }
        gameObject.SetActive(true);
    }

    public void CloseTheContextMenu(bool value)
    {
        if (value)
        {
            gameMenuMgr.selectItem = null;
        }
        gameObject.SetActive(false);
    }

    public void PointerEnter_ContextMenu()
    {
        onPointer = true;
    }

    public void PointerExit_ContextMenu()
    {
        onPointer = false;
    }

    public void Button_ContextMenu_ItemInformation()
    {
        if (gameMenuMgr.activePopUp.Find(x => x.item == gameMenuMgr.selectItem) == null)
        {
            var popUp = gameMenuMgr.GetPopUp(PopUpState.ItemInformation);
            popUp.PopUp_ItemInformation(gameMenuMgr.selectItem);
        }
        CloseTheContextMenu(false);
    }

    public void Button_ContextMenu_UninstallBullets()
    {
        for (int i = 0; i < gameMenuMgr.selectItem.magData.loadedBullets.Count; i++)
        {
            var loadedBullet = gameMenuMgr.selectItem.magData.loadedBullets[i];
            var sameBullet = gameMenuMgr.activeItem.Find(x => x.itemData.type == ItemType.Bullet && x.bulletData.ID == loadedBullet.ID
                                                        && x.TotalCount < x.itemData.maxNesting
                                                        && ((x.itemSlots[0].myStorage != null && gameMenuMgr.selectItem.itemSlots[0].myStorage != null
                                                          && x.itemSlots[0].myStorage.type == gameMenuMgr.selectItem.itemSlots[0].myStorage.type)
                                                          || x.itemSlots[0].otherStorage != null && gameMenuMgr.selectItem.itemSlots[0].otherStorage != null));
            if (sameBullet != null)
            {
                sameBullet.SetTotalCount(sameBullet.TotalCount + 1);
            }
            else
            {
                var inMagStorage = gameMenuMgr.selectItem.itemSlots[0].myStorage;
                if (inMagStorage != null)
                {
                    if (!gameMenuMgr.SetItemInStorage(loadedBullet.ID, 1, inMagStorage.itemSlots, false))
                    {
                        sameBullet = gameMenuMgr.activeItem.Find(x => x.itemData.type == ItemType.Bullet && x.bulletData.ID == loadedBullet.ID
                                                           && x.TotalCount < x.itemData.maxNesting
                                                           && x.itemSlots[0].otherStorage != null);
                        if (sameBullet != null)
                        {
                            sameBullet.SetTotalCount(sameBullet.TotalCount + 1);
                        }
                        else
                        {
                            gameMenuMgr.SetItemInStorage(loadedBullet.ID, 1, gameMenuMgr.otherStorage.itemSlots, false);
                        }
                    }
                }
            }
        }
        gameMenuMgr.selectItem.magData.loadedBullets.Clear();
        if (gameMenuMgr.selectItem.equipSlot != null)
        {
            gameMenuMgr.selectItem.equipSlot.countText.text = "0";
        }
        gameMenuMgr.selectItem.SetTotalCount(0);
        CloseTheContextMenu(true);
    }
}
