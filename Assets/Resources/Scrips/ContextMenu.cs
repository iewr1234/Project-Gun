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
    private InventoryManager invenMgr;

    [Header("---Access Component---")]
    private List<Button> buttons = new List<Button>();

    [Header("--- Assignment Variable---")]
    [SerializeField] private bool onPointer;

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

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

        invenMgr.selectItem = item;
        var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
        var worldPos = invenMgr.invenCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, invenMgr.GetCanvasDistance()));
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
            invenMgr.selectItem = null;
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
        if (invenMgr.activePopUp.Find(x => x.item == invenMgr.selectItem) == null)
        {
            var popUp = invenMgr.GetPopUp(PopUpState.ItemInformation);
            popUp.PopUp_ItemInformation(invenMgr.selectItem);
        }
        CloseTheContextMenu(false);
    }

    public void Button_ContextMenu_UninstallBullets()
    {
        for (int i = 0; i < invenMgr.selectItem.magData.loadedBullets.Count; i++)
        {
            var loadedBullet = invenMgr.selectItem.magData.loadedBullets[i];
            var sameBullet = invenMgr.activeItem.Find(x => x.itemData.type == ItemType.Bullet && x.bulletData.ID == loadedBullet.ID
                                                        && x.TotalCount < x.itemData.maxNesting
                                                        && ((x.itemSlots[0].myStorage != null && invenMgr.selectItem.itemSlots[0].myStorage != null
                                                          && x.itemSlots[0].myStorage.type == invenMgr.selectItem.itemSlots[0].myStorage.type)
                                                          || x.itemSlots[0].otherStorage != null && invenMgr.selectItem.itemSlots[0].otherStorage != null));
            if (sameBullet != null)
            {
                sameBullet.SetTotalCount(sameBullet.TotalCount + 1);
            }
            else
            {
                var inMagStorage = invenMgr.selectItem.itemSlots[0].myStorage;
                if (inMagStorage != null)
                {
                    if (!invenMgr.SetItemInStorage(loadedBullet.ID, 1, inMagStorage.itemSlots, false))
                    {
                        sameBullet = invenMgr.activeItem.Find(x => x.itemData.type == ItemType.Bullet && x.bulletData.ID == loadedBullet.ID
                                                           && x.TotalCount < x.itemData.maxNesting
                                                           && x.itemSlots[0].otherStorage != null);
                        if (sameBullet != null)
                        {
                            sameBullet.SetTotalCount(sameBullet.TotalCount + 1);
                        }
                        else
                        {
                            invenMgr.SetItemInStorage(loadedBullet.ID, 1, invenMgr.otherStorage.itemSlots, false);
                        }
                    }
                }
            }
        }
        invenMgr.selectItem.magData.loadedBullets.Clear();
        if (invenMgr.selectItem.equipSlot != null)
        {
            invenMgr.selectItem.equipSlot.countText.text = "0";
        }
        invenMgr.selectItem.SetTotalCount(0);
        CloseTheContextMenu(true);
    }
}
