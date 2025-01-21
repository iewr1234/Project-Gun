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
        RemoveChamber,
        RemoveMagazine,
        RemoveMagazineBullets,
        RemoveWeaponBullets,
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
        var worldPos = gameMenuMgr.gameMenuCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, gameMenuMgr.GetCanvasDistance() - 10));
        transform.position = worldPos;
        switch (item.itemData.type)
        {
            case ItemType.MainWeapon:
                WeaponType();
                break;
            case ItemType.SubWeapon:
                WeaponType();
                break;
            case ItemType.Magazine:
                if (item.equipSlot == null) buttons[(int)ButtonType.RemoveMagazineBullets].gameObject.SetActive(true);
                break;
            default:
                break;
        }
        gameObject.SetActive(true);

        void WeaponType()
        {
            switch (item.weaponData.magType)
            {
                case MagazineType.Magazine:
                    if (item.weaponData.isChamber)
                        buttons[(int)ButtonType.RemoveChamber].gameObject.SetActive(true);
                    if (item.weaponData.isMag)
                        buttons[(int)ButtonType.RemoveMagazine].gameObject.SetActive(true);
                    break;
                case MagazineType.IntMagazine:
                    if (item.weaponData.isChamber)
                        buttons[(int)ButtonType.RemoveChamber].gameObject.SetActive(true);
                    if (item.weaponData.equipMag.loadedBullets.Count > 0)
                        buttons[(int)ButtonType.RemoveWeaponBullets].gameObject.SetActive(true);
                    break;
                case MagazineType.Cylinder:
                    if (item.weaponData.equipMag.loadedBullets.Count > 0)
                        buttons[(int)ButtonType.RemoveWeaponBullets].gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }

    public void CloseTheContextMenu(bool value)
    {
        if (value)
        {
            gameMenuMgr.selectItem = null;
        }
        gameObject.SetActive(false);
    }

    private void RemoveChamber()
    {
        var onItem = gameMenuMgr.selectItem;
        if (!onItem.weaponData.isChamber) return;

        var chamberBullet = onItem.weaponData.chamberBullet;
        MoveBulletInStorage(chamberBullet);
        onItem.weaponData.chamberBullet = null;
        onItem.weaponData.isChamber = false;
    }

    private void RemoveMagazine()
    {
        var onItem = gameMenuMgr.selectItem;
        var magData = onItem.weaponData.equipMag;
        onItem.weaponData.equipMag = null;
        onItem.weaponData.isMag = false;
        onItem.SetPartsSample();
        onItem.FixTextTheItemCount();
        if (onItem.itemSlots.Count > 0)
        {
            if (onItem.itemSlots[0].myStorage != null)
            {
                gameMenuMgr.SetItemInStorage(magData, onItem.itemSlots[0].myStorage.itemSlots);
            }
            else
            {
                gameMenuMgr.SetItemInStorage(magData, onItem.itemSlots[0].otherStorage.itemSlots);
            }
        }
        else
        {
            gameMenuMgr.SetItemInStorage(magData, null);
        }

        var popUp = gameMenuMgr.activePopUp.Find(x => x.state == PopUpState.ItemInformation && x.item == onItem);
        if (popUp != null) popUp.PopUp_ItemInformation(popUp.item);
        if (gameMenuMgr.gameMgr != null && gameMenuMgr.gameMgr.playerList.Count > 0)
        {
            var player = gameMenuMgr.gameMgr.playerList[0];
            player.SetAbility();
            var weapon = player.weapons.Find(x => x.weaponData == onItem.weaponData);
            if (weapon != null) weapon.SetParts();
        }
    }

    private void RemoveLoadedBullets()
    {
        var onItem = gameMenuMgr.selectItem;
        MagazineDataInfo magData = null;
        switch (onItem.itemData.type)
        {
            case ItemType.MainWeapon:
                RemoveChamber();
                magData = onItem.weaponData.equipMag;
                break;
            case ItemType.SubWeapon:
                RemoveChamber();
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
            MoveBulletInStorage(loadedBullet);
        }
        magData.loadedBullets.Clear();
        onItem.FixTextTheItemCount();
        var popUp = gameMenuMgr.activePopUp.Find(x => x.state == PopUpState.ItemInformation && x.item == onItem);
        if (popUp != null) popUp.PopUp_ItemInformation(popUp.item);
    }

    private void MoveBulletInStorage(BulletDataInfo loadedBullet)
    {
        var onItem = gameMenuMgr.selectItem;
        if (onItem.equipSlot != null)
        {
            FindEmptyMyStorage(loadedBullet);
        }
        else
        {
            var sameBullet = gameMenuMgr.activeItem.Find(x => x.itemData.type == ItemType.Bullet && x.bulletData.ID == loadedBullet.ID && x.equipSlot == null
                                                           && x.TotalCount < x.itemData.maxNesting
                                                          && (x.itemSlots[0].myStorage != null && onItem.itemSlots[0].myStorage != null
                                                           && x.itemSlots[0].myStorage.type == onItem.itemSlots[0].myStorage.type));
            if (sameBullet != null)
            {
                sameBullet.ResultTotalCount(1);
            }
            else
            {
                var myStorage = onItem.itemSlots[0].myStorage;
                if (myStorage != null)
                {
                    if (!gameMenuMgr.SetItemInStorage(loadedBullet.ID, 1, myStorage.itemSlots, false))
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
                    findSlot.item.ResultTotalCount(1);
                    find = true;
                    break;
                }
                else if (gameMenuMgr.SetItemInStorage(loadedBullet.ID, 1, storage.itemSlots, false))
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
                findSlot.item.ResultTotalCount(1);
                //gameMenuMgr.otherStorage.UpdateStorageInfo(findSlot.item);
            }
            else
            {
                gameMenuMgr.SetItemInStorage(loadedBullet.ID, 1, gameMenuMgr.otherStorage.itemSlots, false);
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

    public void Button_ContextMenu_RemoveChamber()
    {
        RemoveChamber();
        gameMenuMgr.selectItem.FixTextTheItemCount();
        CloseTheContextMenu(true);
    }

    public void Button_ContextMenu_RemoveMagzine()
    {
        RemoveMagazine();
        CloseTheContextMenu(true);
    }

    public void Button_ContextMenu_RemoveBullets()
    {
        RemoveLoadedBullets();
        CloseTheContextMenu(true);
    }
}
