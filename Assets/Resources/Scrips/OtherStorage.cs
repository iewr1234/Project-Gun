using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OtherStorage : MonoBehaviour
{
    [Header("---Access Script---")]
    public InventoryManager invenMgr;

    [Header("---Access Component---")]
    public List<ItemSlot> itemSlots = new List<ItemSlot>();

    [Header("--- Assignment Variable---")]
    public Vector2Int size;

    public void SetComponents(InventoryManager _invenMgr)
    {
        invenMgr = _invenMgr;

        itemSlots = GetComponentsInChildren<ItemSlot>().ToList();
        for (int i = 0; i < itemSlots.Count; i++)
        {
            var itemSlot = itemSlots[i];
            var index = new Vector2Int(i % size.x, i / size.x);
            itemSlot.SetComponents(this, index);
        }
    }

    public void SaveInStorage(ItemHandler item)
    {
        var emptySlot = itemSlots.Find(x => x.item == null);
        emptySlot.item = item;
        item.transform.SetParent(emptySlot.transform);
        item.transform.localPosition = Vector3.zero;
    }
}
