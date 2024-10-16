using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenu : MonoBehaviour
{
    private enum ButtonType
    {
        ItemInformation,
        UninstallBullets_Magazine,
        UninstallBullets_Weapon,
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
            case ItemType.MainWeapon:
                if (item.weaponData.isMag && item.weaponData.equipMag.intMag && item.weaponData.equipMag.loadedBullets.Count > 0)
                {
                    buttons[(int)ButtonType.UninstallBullets_Weapon].gameObject.SetActive(true);
                }
                break;
            case ItemType.SubWeapon:
                if (item.weaponData.isMag && item.weaponData.equipMag.intMag && item.weaponData.equipMag.loadedBullets.Count > 0)
                {
                    buttons[(int)ButtonType.UninstallBullets_Weapon].gameObject.SetActive(true);
                }
                break;
            case ItemType.Magazine:
                buttons[(int)ButtonType.UninstallBullets_Magazine].gameObject.SetActive(true);
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

    private void RemoveLoadedBullets()
    {
        var onItem = gameMenuMgr.selectItem;
        MagazineDataInfo magData = null;
        switch (onItem.itemData.type)
        {
            case ItemType.MainWeapon:
                magData = onItem.weaponData.equipMag;
                break;
            case ItemType.SubWeapon:
                magData = onItem.weaponData.equipMag;
                break;
            case ItemType.Magazine:
                magData = onItem.magData;
                break;
            default:
                break;
        }
        if (magData == null) return;

        for (int i = 0; i < magData.loadedBullets.Count; i++)
        {
            var loadedBullet = magData.loadedBullets[i];
            if (onItem.equipSlot != null)
            {
                FindEmptyMyStorage(loadedBullet);
            }
            else
            {
                var sameBullet = gameMenuMgr.activeItem.Find(x => (x.itemSlots[0].myStorage != null && onItem.itemSlots[0].myStorage != null
                                                                && x.itemSlots[0].myStorage.type == onItem.itemSlots[0].myStorage.type)
                                                                && x.itemData.type == ItemType.Bullet && x.bulletData.ID == loadedBullet.ID
                                                                && x.TotalCount < x.itemData.maxNesting);
                if (sameBullet != null)
                {
                    sameBullet.SetTotalCount(sameBullet.TotalCount + 1);
                }
                else
                {
                    var myStorage = onItem.itemSlots[0].myStorage;
                    if (myStorage != null)
                    {
                        if (!gameMenuMgr.SetItemInStorage(loadedBullet.ID, 1, myStorage.itemSlots, false, true))
                        {
                            NestingOrMove(loadedBullet);
                        }
                    }
                    else
                    {
                        NestingOrMove(loadedBullet);
                    }
                }
            }
        }
        magData.loadedBullets.Clear();

        if (onItem.equipSlot != null)
        {
            onItem.equipSlot.SetLoadedBulletCount();
        }
        else
        {
            onItem.SetLoadedBulletCount();
        }
        CloseTheContextMenu(true);

        void FindEmptyMyStorage(BulletDataInfo loadedBullet)
        {
            var storages = gameMenuMgr.myStorages.OrderByDescending(x => x.type).ToList();
            var find = false;
            for (int j = 0; j < storages.Count; j++)
            {
                var storage = storages[j];
                var findSlot = storage.itemSlots.Find(x => x.item != null && x.item.itemData.type == ItemType.Bullet
                                                        && x.item.bulletData.ID == loadedBullet.ID
                                                        && x.item.TotalCount < x.item.itemData.maxNesting);
                if (findSlot != null)
                {
                    findSlot.item.SetTotalCount(findSlot.item.TotalCount + 1);
                    find = true;
                    break;
                }
                else if (gameMenuMgr.SetItemInStorage(loadedBullet.ID, 1, storage.itemSlots, false, true))
                {
                    find = true;
                    break;
                }
            }

            if (!find)
            {
                NestingOrMove(loadedBullet);
            }
        }

        void NestingOrMove(BulletDataInfo loadedBullet)
        {
            var findSlot = gameMenuMgr.otherStorage.itemSlots.Find(x => x.item != null && x.item.itemData.type == ItemType.Bullet
                                                                     && x.item.bulletData.ID == loadedBullet.ID
                                                                     && x.item.TotalCount < x.item.itemData.maxNesting);
            if (findSlot != null)
            {
                findSlot.item.SetTotalCount(findSlot.item.TotalCount + 1);
            }
            else
            {
                gameMenuMgr.SetItemInStorage(loadedBullet.ID, 1, gameMenuMgr.otherStorage.itemSlots, false, true);
            }
        }
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
        RemoveLoadedBullets();
    }
}
