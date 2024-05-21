using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    [Header("---Access Script---")]
    public MyStorage myStorage;
    public OtherStorage otherStorage;

    [Header("--- Assignment Variable---")]
    public ItemHandler item;
    [HideInInspector] public Vector2Int slotIndex;

    public void SetComponents(MyStorage _myStorage, Vector2Int _slotIndex)
    {
        myStorage = _myStorage;

        slotIndex = _slotIndex;
        transform.name = $"Slot_X{slotIndex.x}/Y{slotIndex.y}";
    }

    public void SetComponents(OtherStorage _otherStorage, Vector2Int _slotIndex)
    {
        otherStorage = _otherStorage;

        slotIndex = _slotIndex;
        transform.name = $"Slot_X{slotIndex.x}/Y{slotIndex.y}";
    }

    public void Button_ItemSlot()
    {
        var invenMgr = myStorage != null ? myStorage.invenMgr : otherStorage.invenMgr;
        if (item != null)
        {
            invenMgr.TakeTheItem(item);
            item = null;
        }
        else
        {
            invenMgr.PutTheItem(this);
        }
    }
}