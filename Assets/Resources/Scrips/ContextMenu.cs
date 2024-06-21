using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextMenu : MonoBehaviour
{
    [Header("---Access Script---")]
    private InventoryManager invenMgr;

    [Header("--- Assignment Variable---")]
    [SerializeField] private bool onPointer;

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (gameObject.activeSelf && !onPointer && Input.GetMouseButton(0))
        {
            invenMgr.selectItem = null;
            gameObject.SetActive(false);
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
        if (invenMgr.activePopUp.Find(x => x.item == invenMgr.selectItem) == null)
        {
            var popUp = invenMgr.GetPopUp(PopUpState.ItemInformation);
            popUp.PopUp_ItemInformation();
        }
        gameObject.SetActive(false);
    }
}
